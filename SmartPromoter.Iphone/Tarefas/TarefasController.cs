using System;
using UIKit;
using System.Collections.Generic;
using SPromoterMobile;
using SPromoterMobile.Data;
using System.Diagnostics;
using UserNotifications;
using Foundation;
using HockeyApp;

namespace SmartPromoter.Iphone
{
    public partial class TarefasController : UIViewController
    {
        List<Tarefa> tarefas;
        List<ListTypePDV> listIdsPDVs;
        List<string> listIdsUsers;
        TarefasTable tarefasTable;
        MenuTarefasCon controller;
        GenericCon genericController;
        string requestID = "notificationTimerAlmoco";
        string requestIDInicio = "notificationInicioAlmoco";

        public TarefasController(IntPtr handle) : base(handle) { }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            InitPresentation();
            UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;
            if (genericController.IsHrDeAlmoco())
            {
                PopUpAlmoco("Gostaria de finalizar o horario de almoco em seu ponto eletronico ?");
            }
            UIApplication.Notifications.ObserveDidBecomeActive((sender, e) =>
            {
                if (genericController.IsHrDeAlmoco())
                {
                    PopUpAlmoco("Gostaria de finalizar o horario de almoco em seu ponto eletronico ?");
                }
                else if (tarefas != null && tarefas.Count < 1)
                {
                    PopUpCheckOutAutomatico();
                }
                LocationHelper.UpdateLocation();
            });
        }

        void InitPresentation()
        {
            tarefas = new List<Tarefa>();
            var modelPDV = new MenuPdvsModel
            {
                dbGenericActivity = new GenericActDA(Sqlite_IOS.DB.dataBase),
                dbPdvs = new MenuPdvsDA(Sqlite_IOS.DB.dataBase)
            };
            modelPDV.infoUsuario = modelPDV.dbGenericActivity.GetUsersIDsLogged();

            var modelGeneric = new GenericModel
            {
                dbGenericActivity = new GenericActDA(Sqlite_IOS.DB.dataBase)
            };
            genericController = new GenericCon(modelGeneric);

            var visita = modelPDV.dbPdvs.GetVisitaAtual();
            if (visita != null)
            {
                if (visita.Count > 0)
                {
                    listIdsPDVs = new List<ListTypePDV>();
                    foreach (var item in visita)
                    {
                        listIdsPDVs.Add(new ListTypePDV(item.ID));
                    }
                    listIdsUsers = new List<string>();
                    foreach (var itemUser in modelPDV.infoUsuario)
                    {
                        listIdsUsers.Add(itemUser.ID);
                    }
                }
            }

            PopulateTarefas();
            ReloadList();

            tabBarTarefas.ItemSelected += delegate
            {
                if (tabBarTarefas.SelectedItem.Title == tabItemAlmoco.Title)
                {
                    PopUpAlmoco("Gostaria de iniciar o horario de almoco em seu ponto eletronico ?");
                }
                else if (tabBarTarefas.SelectedItem.Title == tabItemConcluir.Title)
                {
                    PopUpForceCheckOut();
                }
            };

            searchBarTarefas.TextChanged += delegate
            {
                if (string.IsNullOrEmpty(searchBarTarefas.Text))
                {
                    PopulateTarefas();
                    ReloadList();
                }
            };

            searchBarTarefas.SearchButtonClicked += delegate
            {
                PopulateTarefas();
                tarefas.RemoveAll(tarefa => (!tarefa.Categoria.ToUpper().Contains(searchBarTarefas.Text.ToUpper()) &&
                                             !tarefa.DescricaoDaTarefa.ToUpper().Contains(searchBarTarefas.Text.ToUpper())));
                ReloadList();
            };

            if (tarefas != null && tarefas.Count < 1)
            {
                PopUpCheckOutAutomatico();
            }
        }

        void PopulateTarefas()
        {
            var tarefasNewList = new List<Tarefa>();
            var model = new MenuTarefasModel
            {
                idVisitas = listIdsPDVs,
                idsUsuariosLogados = listIdsUsers,
                db = new MenuTarefasDA(Sqlite_IOS.DB.dataBase)
            };
            controller = new MenuTarefasCon(model);

            foreach (var pdvs in listIdsPDVs)
            {

                if (model.db.HasTarefaLoja(pdvs.IdVisita))
                {
                    var logged = model.db.GetUsersIDsLogged();
                    var currentUser = logged.Find((obj) => obj.ID.Equals(
                        model.db.GetIDByVisita(pdvs.IdVisita)));

                    var pdvInfo = model.db.GetLojaInfo(pdvs.IdVisita);
                    var tarefa = new Tarefa
                    {
                        Categoria = currentUser.SERVIDOR.Substring(0, currentUser.SERVIDOR.IndexOf(".", StringComparison.CurrentCulture)),
                        DescricaoDaTarefa = pdvInfo.ENDERECO,
                        Ruptura = null,
                        IdPdv = pdvs.IdVisita,
                        IdProduto = "00000000-0000-0000-0000-000000000000"
                    };
                    tarefasNewList.Add(tarefa);
                }

                var listProdutos = controller.ListProdutos(pdvs.IdVisita);
                if (listProdutos != null)
                {
                    foreach (var rowProduto in listProdutos)
                    {
                        if (rowProduto.ID != "00000000-0000-0000-0000-000000000000")
                        {
                            var tarefa = new Tarefa
                            {
                                Categoria = rowProduto.CATEGORIA,
                                DescricaoDaTarefa = rowProduto.NOME,
                                Ruptura = Ruptura(),
                                IdPdv = pdvs.IdVisita,
                                IdProduto = rowProduto.ID
                            };
                            tarefasNewList.Add(tarefa);
                        }
                    }
                }
            }
            tarefas = tarefasNewList;
        }

        void ReloadList()
        {
            tarefasTable = new TarefasTable(this)
            {
                Tarefas = tarefas
            };
            TarefasTable.Source = tarefasTable;
            TarefasTable.ReloadData();
        }

        void PopUpForceCheckOut()
        {
            var alert = UIAlertController.Create("Forcar CheckOut",
                                                               "Existem tarefas ainda nao concluidas na lista, gostaria de realizar o checkout ? ",
                                                               UIAlertControllerStyle.Alert);

            alert.AddAction(UIAlertAction.Create("Nao", UIAlertActionStyle.Cancel, (actionCancel) =>
            {
                MetricsManager.TrackEvent("CancelForceCheckOut");
            }));

            alert.AddAction(UIAlertAction.Create("Sim", UIAlertActionStyle.Default, (actionOK) =>
            {
                controller.CheckOutTarefa();
#if !DEBUG
                MetricsManager.TrackEvent("ForceCheckOut");
#endif
                if (Storyboard.InstantiateViewController("FeedPDV") is PDVBarController listPdvs)
                {
                    ShowViewController(listPdvs, null);
                }
            }));
            alert.View.TintColor = UIColor.FromRGB(10, 88, 90);
            PresentViewController(alert, true, null);

        }

        void PopUpCheckOutAutomatico()
        {
            controller.CheckOutTarefa();
            var alert = UIAlertController.Create("CheckOut Automatico",
                                                               "Todos as tarefas foram concluidas, checkout automatico realizado.",
                                                               UIAlertControllerStyle.Alert);

            alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, (actionOK) =>
            {

                if (Storyboard.InstantiateViewController("FeedPDV") is PDVBarController listPdvs)
                {
                    ShowViewController(listPdvs, null);
                }
            }));
            alert.View.TintColor = UIColor.FromRGB(10, 88, 90);
            PresentViewController(alert, true, null);

        }

        void PopUpAlmoco(string msg)
        {
            var hrsDeAlmoco = genericController.GetAlmoco();
            if (hrsDeAlmoco.Count == 1)
            {
                var timeAlmoco = hrsDeAlmoco[0].ToString("HH:mm");
                msg = "Iniciado as: " + timeAlmoco + "\n\n" + msg;

            }
            var alert = UIAlertController.Create("Horario de Almoco", msg, UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create("Nao", UIAlertActionStyle.Cancel, (actionCancel) =>
            {

                if (msg.Contains("Gostaria de finalizar o horario de almoco em seu ponto eletronico ?"))
                {
                    Process.GetCurrentProcess().CloseMainWindow();
                }
            }));

            alert.AddAction(UIAlertAction.Create("Sim", UIAlertActionStyle.Default, (actionOK) =>
            {
                if (SetAlmoco())
                {
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

        internal bool SetAlmoco()
        {
            bool result = false;
            UIAlertController alertError;
            switch (genericController.SetAlmoco())
            {
                case StatusPontoEletronico.NAO_INICIADO:
                    alertError = UIAlertController.Create("E necessario um ponto aberto\npara ativar o horario de almoco", null, UIAlertControllerStyle.Alert);
                    alertError.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Cancel, (actionCancel) => { }));
                    alertError.View.TintColor = UIColor.FromRGB(10, 88, 90);
                    PresentViewController(alertError, true, null);
                    break;
                case StatusPontoEletronico.ALMOCO_INICIADO:
                    result = true;
                    break;
                case StatusPontoEletronico.ALMOCO_FINALIZADO:
                    alertError = UIAlertController.Create("Horario de Almoco finalizado", null, UIAlertControllerStyle.Alert);
                    alertError.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Cancel, (actionCancel) => { }));
                    alertError.View.TintColor = UIColor.FromRGB(10, 88, 90);
                    PresentViewController(alertError, true, null);
                    if (requestID != null)
                    {
                        string[] ids = { requestID };
                        UNUserNotificationCenter.Current.RemovePendingNotificationRequests(ids);
                    }
                    break;
                case StatusPontoEletronico.CHECKOUT:
                    alertError = UIAlertController.Create("Horario de almoco ja realizado", null, UIAlertControllerStyle.Alert);
                    alertError.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Cancel, (actionCancel) => { }));
                    alertError.View.TintColor = UIColor.FromRGB(10, 88, 90);
                    PresentViewController(alertError, true, null);
                    break;
            }

            return result;
        }

        EventHandler Ruptura()
        {
            return (sender, e) =>
            {
                var alert = UIAlertController.Create("Ruptura", "Tem certeza que o item esta em ruptura ?", UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("Nao", UIAlertActionStyle.Cancel, (actionCancel) =>
                {
                    MetricsManager.TrackEvent("CancelRuptura");
                }));

                alert.AddAction(UIAlertAction.Create("Sim", UIAlertActionStyle.Default, (actionOK) =>
                {
                    InvokeOnMainThread(delegate
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
                            var tarefasCell = ((UIButton)sender).Superview.Superview as TarefasCell;
                            var path = TarefasTable.IndexPathForRowAtPoint(new CoreGraphics.CGPoint(tarefasCell.Frame.X, tarefasCell.Frame.Y));
                            using (var cell = TarefasTable.CellAt(path) as TarefasCell)
                            {
                                var task = cell.GetTarefaInfo();
                                foreach (var itemIDVisita in listIdsPDVs)
                                {
                                    var tbProduto = controller.GetProdutosList(task.DescricaoDaTarefa);
                                    foreach (var produto in tbProduto)
                                    {
                                        var idProduto = produto.ID;
                                        if (controller.IsCorrectIDTarefaLoja(itemIDVisita.IdVisita, produto.ID))
                                        {
                                            var batery = ((int)(UIDevice.CurrentDevice.BatteryLevel * 100F));
                                            if (gps.Location != null)
                                            {
                                                controller.SetRuptura(itemIDVisita.IdVisita,
                                                                      controller.GetProdutos(task.DescricaoDaTarefa).ID, gps.Location.Coordinate.Latitude,
                                                                      gps.Location.Coordinate.Longitude, batery);
                                            }
                                            else
                                            {
                                                controller.SetRuptura(itemIDVisita.IdVisita,
                                                                      controller.GetProdutos(task.DescricaoDaTarefa).ID,
                                                                      LocationHelper.LastLocation.Coordinate.Latitude,
                                                                      LocationHelper.LastLocation.Coordinate.Longitude, batery);

                                            }
                                        }
                                    }
                                }
                                var index = tarefasTable.Tarefas.FindIndex((obj) => obj.DescricaoDaTarefa.Equals(task.DescricaoDaTarefa) &&
                                               obj.Categoria.Equals(task.Categoria));
                                TarefasTable.BeginUpdates();
                                tarefasTable.Tarefas.RemoveAt(index);
                                TarefasTable.DeleteRows(new NSIndexPath[] { path }, UITableViewRowAnimation.Left);
                                TarefasTable.EndUpdates();
                                if (tarefas.Count < 1)
                                {
                                    PopUpCheckOutAutomatico();
                                }
                            }
                        }
                    });
#if !DEBUG
                    HockeyApp.MetricsManager.TrackEvent("Ruptura");
#endif
                }));
                alert.View.TintColor = UIColor.FromRGB(10, 88, 90);
                PresentViewController(alert, true, null);
            };
        }
    }
}