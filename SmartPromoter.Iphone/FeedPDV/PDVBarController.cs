using System;
using UIKit;
using System.Collections.Generic;
using SPromoterMobile.Data;
using SPromoterMobile;
using UserNotifications;
using System.Diagnostics;
using static SmartPromoter.Iphone.Camera;
using Foundation;
using SPromoterMobile.Controller.RESTful;
using CoreGraphics;
using System.Net;
using CoreLocation;
using HockeyApp;

namespace SmartPromoter.Iphone
{
    public partial class PDVBarController : UIViewController
    {
        MenuPdvsCon controllerPCL;
        GenericCon genController;
        FeedPdvCollections feedCollection;
        MenuPdvsModel modelPCL;
        string requestID = "notificationTimerAlmoco";
        string requestIDInicio = "notificationInicioAlmoco";

        public PDVBarController(IntPtr handle) : base(handle) { }


        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            var modelGeneric = new GenericModel
            {
                dbGenericActivity = new GenericActDA(Sqlite_IOS.DB.dataBase)
            };
            genController = new GenericCon(modelGeneric);
            modelPCL = new MenuPdvsModel
            {
                dbGenericActivity = new GenericActDA(Sqlite_IOS.DB.dataBase),
                dbPdvs = new MenuPdvsDA(Sqlite_IOS.DB.dataBase)
            };
            modelPCL.infoUsuario = modelPCL.dbPdvs.GetUserInfoLogged();
            controllerPCL = new MenuPdvsCon(modelPCL);

            var userInfo = modelPCL.dbPdvs.GetUserInfoLogged();
            nomeUsuario.Text = userInfo[0].NOME;
            cargoPromotor.Text = userInfo[0].CARGO;
            if (userInfo[0].AVATAR != null)
            {
                profileAvatar.SetImage(GetBitMap(userInfo[0].AVATAR), UIControlState.Normal);
            }
            profileAvatar.Layer.CornerRadius = profileAvatar.Frame.Size.Width / 2;
            profileAvatar.Layer.BorderWidth = 1.0f;
            profileAvatar.Layer.BorderColor = UIColor.White.CGColor;
            profileAvatar.ClipsToBounds = true;
            profileAvatar.ImageView.ContentMode = UIViewContentMode.ScaleAspectFill;

            profileAvatar.TouchDown -= ClickCamera();
            profileAvatar.TouchDown += ClickCamera();

            tabBarMenuPdvs.ItemSelected -= ClickTabBar();
            tabBarMenuPdvs.ItemSelected += ClickTabBar();

            headerView.Layer.ShadowColor = UIColor.Black.CGColor;
            headerView.Layer.ShadowOffset = new CGSize(0.0f, 2.6f);
            headerView.Layer.ShadowOpacity = 0.6f;
            PopulateProgressBar(false);
            UpdateData();
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            if (genController.IsHrDeAlmoco())
            {
                PopUpAlmoco("Gostaria de finalizar o horario de almoco em seu ponto eletronico ?");
            }
            UIApplication.Notifications.ObserveDidBecomeActive((sender, e) =>
            {
                if (genController.IsHrDeAlmoco())
                {
                    PopUpAlmoco("Gostaria de finalizar o horario de almoco em seu ponto eletronico ?");
                }
                LocationHelper.UpdateLocation();
            });
        }

        void UpdateData()
        {
            InitCollectionView();
            PopulatePdvs();
            FnInitializeView();
            PopulateProgressBar(false);
            UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;
        }

        void PopulatePdvs()
        {
            feedCollection.Pdvs = new List<Pdv>();

            var onde = controllerPCL.VisitasPendentes();
            foreach (var item in controllerPCL.VisitasPendentes())
            {
                var fullGeoPT = item.ENDERECO.Split('\n');
                string id = item.ID;
                double lat = item.LAT;
                double longi = item.LONG;
                var cell = new Pdv
                {
                    listTypePdv = new List<ListTypePDV>(),
                    NomePDV = fullGeoPT[0].Split('-')[1].Trim(),
                    Endereco = (fullGeoPT[1] + " " + fullGeoPT[2].Replace(" - CEP:", ", CEP:"))
                };
                var index = feedCollection.Pdvs.FindIndex(Pdv => (Pdv.NomePDV.ToUpper().Equals(cell.NomePDV.ToUpper()) &&
                                              Pdv.Endereco.ToUpper().Equals(cell.Endereco.ToUpper())));

                var infoPdv = new ListTypePDV(id);
                if (index >= 0 && feedCollection.Pdvs.Count > 0)
                {
                    if (feedCollection.Pdvs[index].listTypePdv.FindIndex(ListTypePDV => (ListTypePDV.IdVisita.Equals(infoPdv.IdVisita))) < 0)
                    {
                        feedCollection.Pdvs[index].listTypePdv.Add(infoPdv);
                    }
                }
                else
                {
                    cell.listTypePdv.Add(infoPdv);
                    cell.Lat = lat;
                    cell.Longi = longi;
                    cell.IdVisita = id;
                    cell.Justificativa += Justificativa();
                    cell.CheckIn += CheckIn();
                    cell.MapsExec += ExecMaps();
                    feedCollection.Pdvs.Add(cell);
                }
            }
            InvokeOnMainThread(() => collecFeedPdv.ReloadData());
        }

        void InitCollectionView()
        {
            feedCollection = new FeedPdvCollections();
            collecFeedPdv.Source = feedCollection;
        }

        void FnInitializeView()
        {
            InvokeOnMainThread(delegate
            {
                var visita = modelPCL.dbPdvs.GetVisitaAtual();
                if (visita != null)
                {
                    if (visita.Count <= 0)
                    {
                        if (controllerPCL.CheckOutVisita(feedCollection.Pdvs.Count))
                        {
                            CheckOutMessage();
                            var app = new AppDelegate();
                            app.ExecAPIs();
                        }
                    }
                }
            });
        }

        void PopUpAlmoco(string msg)
        {
            var hrsDeAlmoco = new List<DateTime>();
            InvokeOnMainThread(() => hrsDeAlmoco = genController.GetAlmoco());
            if (hrsDeAlmoco.Count == 1)
            {
                var timeAlmoco = hrsDeAlmoco[0].ToString("HH:mm");
                msg = "Iniciado as: " + timeAlmoco + "\n\n" + msg;

            }
            var alert = UIAlertController.Create("Horario de Almoco", msg, UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create("Nao", UIAlertActionStyle.Cancel, (actionCancel) =>
            {
                MetricsManager.TrackEvent("CancelAlmoco");
                if (msg.Contains("Gostaria de finalizar o horario de almoco em seu ponto eletronico ?"))
                {
                    var app = new AppDelegate();
                    app.ExecAPIs();
                    Process.GetCurrentProcess().CloseMainWindow();
                }
            }));

            alert.AddAction(UIAlertAction.Create("Sim", UIAlertActionStyle.Default, (actionOK) =>
            {
                if (SetAlmoco())
                {
                    var app = new AppDelegate();
                    app.ExecAPIs();
                    var contentAlmoco = new UNMutableNotificationContent
                    {
                        Title = "Aviso",
                        Subtitle = "Termino do horario de almoco",
                        Body = "10 Minutos restantes para o fim do horario de almoco"
                    };
                    var notificationTimerAlmoco = UNNotificationRequest.FromIdentifier(requestID, contentAlmoco,
                                                                                  UNTimeIntervalNotificationTrigger.CreateTrigger(60 * 50, false));
                    var contentAlmocoInicio = new UNMutableNotificationContent
                    {
                        Title = "Aviso",
                        Subtitle = "Inicio do horario de almoco",
                        Body = "Iniciado as: " + DateTime.Now.ToString("HH:mm")
                    };
                    var notificationInicioAlmoco = UNNotificationRequest.FromIdentifier(requestIDInicio, contentAlmocoInicio,
                                                                                   UNTimeIntervalNotificationTrigger.CreateTrigger(60 * 50, false));
                    UNUserNotificationCenter.Current.AddNotificationRequest(notificationTimerAlmoco, (err) => { });
                    UNUserNotificationCenter.Current.AddNotificationRequest(notificationInicioAlmoco, (err) => { });
                    Process.GetCurrentProcess().CloseMainWindow();
                }
            }));
            alert.View.TintColor = UIColor.FromRGB(10, 88, 90);
            PresentViewController(alert, true, null);
        }

        bool SetAlmoco()
        {
            bool result = false;
            UIAlertController alert;
            InvokeOnMainThread(delegate
            {
                switch (genController.SetAlmoco())
                {
                    case StatusPontoEletronico.NAO_INICIADO:
                        alert = UIAlertController.Create("E necessario um ponto aberto\npara ativar o horario de almoco", null, UIAlertControllerStyle.Alert);
                        alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
                        alert.View.TintColor = UIColor.FromRGB(10, 88, 90);
                        PresentViewController(alert, true, null);
                        break;
                    case StatusPontoEletronico.ALMOCO_INICIADO:
                        result = true;
                        break;
                    case StatusPontoEletronico.ALMOCO_FINALIZADO:
                        alert = UIAlertController.Create("Horario de Almoco finalizado", null, UIAlertControllerStyle.Alert);
                        alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
                        alert.View.TintColor = UIColor.FromRGB(10, 88, 90);
                        PresentViewController(alert, true, null);
                        if (requestID != null)
                        {
                            string[] ids = { requestID };
                            UNUserNotificationCenter.Current.RemovePendingNotificationRequests(ids);
                        }
                        break;
                    case StatusPontoEletronico.CHECKOUT:
                        alert = UIAlertController.Create("Horario de almoco ja realizado", null, UIAlertControllerStyle.Alert);
                        alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
                        alert.View.TintColor = UIColor.FromRGB(10, 88, 90);
                        PresentViewController(alert, true, null);
                        break;
                }
            });
            return result;
        }

        #region EventHandles
        EventHandler<UITabBarItemEventArgs> ClickTabBar()
        {
            return (sender, e) =>
            {
                if (tabBarMenuPdvs.SelectedItem.Title == tabAlmoco.Title)
                {
                    PopUpAlmoco("Gostaria de iniciar o horario de almoco em seu ponto eletronico ?");
                }
                else if (tabBarMenuPdvs.SelectedItem.Title == tabContas.Title)
                {
                    if (Storyboard.InstantiateViewController("Login") is LoginController login)
                    {
                        ShowViewController(login, null);
                    }
                }
            };
        }

        EventHandler ClickCamera()
        {
            return (sender, e) =>
            {
                TakePicture(this, (obj) =>
                    {
                        var photo = obj.ValueForKey(new NSString("UIImagePickerControllerOriginalImage")) as UIImage;
                        var ids = new List<string>();
                        var photosUrl = new List<string>();
                        photo = photo.Scale(new CGSize(photo.Size.Width * 0.2, photo.Size.Height * 0.2));
                        using (var imgData = photo.AsJPEG(0.7f))
                        {
                            var container = new ContainerRestCon();
                            foreach (var id in modelPCL.infoUsuario)
                            {
                                ids.Add(id.ID);
                                photosUrl.Add(container.GravarArquivo(imgData.AsStream(), id.ID + ".JPEG"));
                            }
                        }
                        InvokeOnMainThread(delegate
                        {
                            modelPCL.dbPdvs.InsertFotoProfile(photosUrl, ids);
                            modelPCL.infoUsuario = modelPCL.dbPdvs.GetUserInfoLogged();
                            profileAvatar.SetImage(GetBitMap(modelPCL.infoUsuario[0].AVATAR), UIControlState.Normal);
                        });
                    });
            };
        }

        EventHandler ExecMaps()
        {
            return (sender, e) =>
            {
                var contaCell = ((UIButton)sender).Superview.Superview.Superview as FeedPDVCell;
                var path = collecFeedPdv.IndexPathForItemAtPoint(new CGPoint(contaCell.Frame.X, contaCell.Frame.Y));
                using (var cell = collecFeedPdv.CellForItem(path) as FeedPDVCell)
                {
                    var info = cell.GetPdvCardUiInfo();
                    //var index = feedCollection.pdvs.FindIndex((obj) => obj.nomePDV.Equals(info.nomePDV) &&
                    //										  obj.endereco.Equals(info.endereco));
                    /*var mark = new MKPlacemark(new CoreLocation.CLLocationCoordinate2D(feedCollection.pdvs[index].lat,
					                                                                  feedCollection.pdvs[index].longi));
					var item = new MKMapItem(mark);*/
                    //item.OpenInMaps();

#pragma warning disable iOSAndMacApiUsageIssue // Find issues with Mac and iOS API usage
                    UIApplication.SharedApplication.OpenUrl(
#pragma warning restore iOSAndMacApiUsageIssue // Find issues with Mac and iOS API usage
                        new NSUrl(string.Format("http://maps.apple.com/?q={0}", WebUtility.UrlEncode(info.Endereco))));
                }
            };
        }

        EventHandler CheckIn()
        {
            return (sender, e) =>
            {
                var contaCell = ((UIButton)sender).Superview.Superview as FeedPDVCell;
                var path = collecFeedPdv.IndexPathForItemAtPoint(new CGPoint(contaCell.Frame.X, contaCell.Frame.Y));
                using (var cell = collecFeedPdv.CellForItem(path) as FeedPDVCell)
                {
                    var info = cell.GetPdvCardUiInfo();
                    var alert = UIAlertController.Create("CheckIn", "Gostaria de efetuar o Check-In em " + info.NomePDV +
                                                         "\n" + info.Endereco + " ? ", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("Nao", UIAlertActionStyle.Cancel, (actionCancel) =>
                    {
                        MetricsManager.TrackEvent("CancelCheckIn");
                    }));

                    alert.AddAction(UIAlertAction.Create("Sim", UIAlertActionStyle.Default, (actionOK) =>
                    {
                        var isToOpenTarefas = false;
                        var index = feedCollection.Pdvs.FindIndex((obj) => obj.NomePDV.Equals(info.NomePDV) &&
                                                                  obj.Endereco.Equals(info.Endereco));

                        var gps = LocationHelper.UpdateLocation();
                        if (gps == null)
                        {
                            var alertGps = UIAlertController.Create("GPS Desativado",
                                "Ligue o GPS ou tire do modo aviao para continuar utilizando o sistema", UIAlertControllerStyle.Alert);
                            alertGps.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, (defaults) => { }));
                            alertGps.View.TintColor = UIColor.FromRGB(10, 88, 90);
                            PresentViewController(alertGps, true, null);
                        }
                        else
                        {
                            var cordenadaEsperada = controllerPCL.GetCoordinates(feedCollection.Pdvs[index].listTypePdv);
                            if (gps.Location == null)
                            {
                                var batery = ((int)(UIDevice.CurrentDevice.BatteryLevel * 100F));
                                controllerPCL.CheckIn(feedCollection.Pdvs[index].listTypePdv,
                                                      LocationHelper.LastLocation.Coordinate.Latitude,
                                                      LocationHelper.LastLocation.Coordinate.Longitude, batery);
                                isToOpenTarefas = true;
                            }
                            else
                            {
                                var gpsEsperado = new CLLocation(cordenadaEsperada[0], cordenadaEsperada[1]);
                                var distance = gpsEsperado.DistanceFrom(gps.Location);
                                if (distance > 500 &&
                                    (int)cordenadaEsperada[0] > 0 && (int)cordenadaEsperada[1] > 0)
                                {
                                    isToOpenTarefas = false;
                                    var alertRangePDV = UIAlertController.Create("PDV longe do esperado",
                                         "Sua localizacao atual está diferente da esperada, tem certeza que gostaria de continuar ?"
                                         , UIAlertControllerStyle.Alert);
                                    alertRangePDV.AddAction(UIAlertAction.Create("Nao", UIAlertActionStyle.Cancel, (actionCancelRangePDV) =>
                                    {
                                        MetricsManager.TrackEvent("CancelCheckIn");
                                    }));

                                    alertRangePDV.AddAction(UIAlertAction.Create("Sim", UIAlertActionStyle.Default, (actionOKRangePDV) =>
                                    {
                                        var batery = ((int)(UIDevice.CurrentDevice.BatteryLevel * 100F));
                                        var gpsLocation = gps.Location.Coordinate;
                                        controllerPCL.RegistroDePontoEletronico();
                                        controllerPCL.CheckIn(feedCollection.Pdvs[index].listTypePdv,
                                                              gpsLocation.Latitude, gpsLocation.Longitude, batery);

                                        if (Storyboard.InstantiateViewController("TarefasController") is TarefasController tarefas)
                                        {
#if !DEBUG
                                            MetricsManager.TrackEvent("CheckInLoja");
#endif
                                            ShowViewController(tarefas, null);
                                        }
                                    }));
                                    alertRangePDV.View.TintColor = UIColor.FromRGB(10, 88, 90);
                                    PresentViewController(alertRangePDV, true, null);
                                }
                                else
                                {
                                    var batery = ((int)(UIDevice.CurrentDevice.BatteryLevel * 100F));
                                    isToOpenTarefas = true;
                                    var gpsLocation = gps.Location.Coordinate;
                                    controllerPCL.CheckIn(feedCollection.Pdvs[index].listTypePdv,
                                                          gpsLocation.Latitude, gpsLocation.Longitude, batery);
                                }
                            }
                            if (isToOpenTarefas)
                            {
                                controllerPCL.RegistroDePontoEletronico();
                                if (Storyboard.InstantiateViewController("TarefasController") is TarefasController tarefas)
                                {
#if !DEBUG
                                    MetricsManager.TrackEvent("CheckInLoja");
#endif
                                    ShowViewController(tarefas, null);
                                }
                            }
                        }
                    }));
                    alert.View.TintColor = UIColor.FromRGB(10, 88, 90);
                    PresentViewController(alert, true, null);
                }
            };
        }

        EventHandler Justificativa()
        {
            return (sender, e) =>
            {
                var contaCell = ((UIButton)sender).Superview.Superview as FeedPDVCell;
                var path = collecFeedPdv.IndexPathForItemAtPoint(new CGPoint(contaCell.Frame.X, contaCell.Frame.Y));
                using (var cell = collecFeedPdv.CellForItem(path) as FeedPDVCell)
                {
                    var info = cell.GetPdvCardUiInfo();
                    var actionSheetAlert = UIAlertController.Create("Justificativa", "Motivo da justificativa", UIAlertControllerStyle.ActionSheet);
                    //foreach (string itemType in controllerPCL.model.camposForm.descricoesDasFotos)
                    //{
                    //	actionSheetAlert.AddAction(UIAlertAction.Create(itemType, UIAlertActionStyle.Default, (action) => { TakePhoto(itemType); }));
                    //}
                    #region lista_de_motivos
                    actionSheetAlert.AddAction(UIAlertAction.Create("Ação em outro pdv por tempo integral", UIAlertActionStyle.Default, (action) =>
                    {
                        JustificarAction("Ação em outro pdv por tempo integral", info.NomePDV, info.Endereco, path);
                    }));
                    actionSheetAlert.AddAction(UIAlertAction.Create("Atestado Medico", UIAlertActionStyle.Default, (action) =>
                    {
                        JustificarAction("Atestado Medico", info.NomePDV, info.Endereco, path);
                    }));
                    actionSheetAlert.AddAction(UIAlertAction.Create("Bateria insuficiente", UIAlertActionStyle.Default, (action) =>
                    {
                        JustificarAction("Bateria insuficiente", info.NomePDV, info.Endereco, path);
                    }));
                    actionSheetAlert.AddAction(UIAlertAction.Create("Cumpri meu horário de trabalho", UIAlertActionStyle.Default, (action) =>
                    {
                        JustificarAction("Cumpri meu horário de trabalho", info.NomePDV, info.Endereco, path);
                    }));
                    actionSheetAlert.AddAction(UIAlertAction.Create("Em treinamento", UIAlertActionStyle.Default, (action) =>
                    {
                        JustificarAction("Em treinamento", info.NomePDV, info.Endereco, path);
                    }));
                    actionSheetAlert.AddAction(UIAlertAction.Create("Estava em reunião", UIAlertActionStyle.Default, (action) =>
                    {
                        JustificarAction("Estava em reunião", info.NomePDV, info.Endereco, path);
                    }));
                    actionSheetAlert.AddAction(UIAlertAction.Create("Instabilidade no sistema", UIAlertActionStyle.Default, (action) =>
                    {
                        JustificarAction("Instabilidade no sistema", info.NomePDV, info.Endereco, path);
                    }));
                    actionSheetAlert.AddAction(UIAlertAction.Create("Loja fora do roteiro planejado", UIAlertActionStyle.Default, (action) =>
                    {
                        JustificarAction("Loja fora do roteiro planejado", info.NomePDV, info.Endereco, path);
                    }));
                    actionSheetAlert.AddAction(UIAlertAction.Create("PDV fechado", UIAlertActionStyle.Default, (action) =>
                    {
                        JustificarAction("PDV fechado", info.NomePDV, info.Endereco, path);
                    }));
                    actionSheetAlert.AddAction(UIAlertAction.Create("PDV não existe", UIAlertActionStyle.Default, (action) =>
                    {
                        JustificarAction("PDV não existe", info.NomePDV, info.Endereco, path);
                    }));
                    actionSheetAlert.AddAction(UIAlertAction.Create("Transito durante o transporte", UIAlertActionStyle.Default, (action) =>
                    {
                        JustificarAction("Transito durante o transporte.", info.NomePDV, info.Endereco, path);
                    }));
                    actionSheetAlert.AddAction(UIAlertAction.Create("Outros", UIAlertActionStyle.Default, (action) =>
                    {
                        JustificarAction("Outros", info.NomePDV, info.Endereco, path);
                    }));
                    actionSheetAlert.AddAction(UIAlertAction.Create("Cancelar", UIAlertActionStyle.Cancel, (action) => { }));
                    #endregion lista_de_motivos
                    var presentationPopover = actionSheetAlert.PopoverPresentationController;
                    if (presentationPopover != null)
                    {
                        presentationPopover.SourceView = View;
                        presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
                    }
                    //actionSheetAlert.View.TintColor = UIColor.Black;
                    actionSheetAlert.View.TintColor = UIColor.FromRGB(10, 88, 90);
                    PresentViewController(actionSheetAlert, true, null);
                }
            };
        }
        #endregion

        void JustificarAction(string motivo, string nomePdv, string enderecoPdv, NSIndexPath path)
        {
            var gps = LocationHelper.UpdateLocation();
            if (gps == null)
            {
                var alertGps = UIAlertController.Create("GPS Desativado",
                "Ligue o GPS ou tire do modo aviao para continuar utilizando o sistema", UIAlertControllerStyle.Alert);
                alertGps.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, (defaults) => { }));
                alertGps.View.TintColor = UIColor.FromRGB(10, 88, 90);
                PresentViewController(alertGps, true, null);
            }
            else
            {
                var index = feedCollection.Pdvs.FindIndex((obj) => obj.NomePDV.Equals(nomePdv) &&
                                                          obj.Endereco.Equals(enderecoPdv));
                var listTypePdv = feedCollection.Pdvs[index].listTypePdv;

                feedCollection.Pdvs.RemoveAt(index);
                collecFeedPdv.PerformBatchUpdates(delegate
                {
                    collecFeedPdv.DeleteItems(new NSIndexPath[] { path });
                }, null);
                feedCollection.Pdvs.RemoveAll((obj) => obj.NomePDV.Equals(nomePdv) && obj.Endereco.Equals(enderecoPdv));

                if (gps.Location == null)
                {
                    var batery = ((int)(UIDevice.CurrentDevice.BatteryLevel * 100F));
                    controllerPCL.Justificativa(listTypePdv, motivo,
                                                LocationHelper.LastLocation.Coordinate.Latitude,
                                                LocationHelper.LastLocation.Coordinate.Longitude, batery);
                }
                else
                {
                    var batery = ((int)(UIDevice.CurrentDevice.BatteryLevel * 100F));
                    var gpsLocation = gps.Location.Coordinate;
                    controllerPCL.Justificativa(listTypePdv, motivo,
                                                gpsLocation.Latitude, gpsLocation.Longitude, batery);
                }
                controllerPCL.RegistroDePontoEletronico();
                PopulateProgressBar(true);
#if !DEBUG
                HockeyApp.MetricsManager.TrackEvent("JustificativaLoja");
#endif
                if (controllerPCL.CheckOutVisita(feedCollection.Pdvs.Count))
                {
                    CheckOutMessage();
                    var app = new AppDelegate();
                    app.ExecAPIs();
                }
            }
        }

        void PopulateProgressBar(bool animated)
        {
            var percentualDeVisitas = controllerPCL.PercentualVisitas();
            txtPercentMeta.Text = percentualDeVisitas + "%";
            if (percentualDeVisitas > 0)
            {
                progressMeta.SetProgress(((float)percentualDeVisitas) / 100, animated);
            }
            else
            {
                progressMeta.SetProgress(0, animated);
            }
        }

        void CheckOutMessage()
        {
            var alert = UIAlertController.Create("Nao Existem mais Pdvs no roteiro programado.", null, UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
            alert.View.TintColor = UIColor.FromRGB(10, 88, 90);
            PresentViewController(alert, true, null);
        }
    }
}