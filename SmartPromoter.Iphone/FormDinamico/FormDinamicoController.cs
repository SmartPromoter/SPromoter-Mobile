using Foundation;
using System;
using UIKit;
using SPromoterMobile;
using SPromoterMobile.Controller.RESTful;
using SPromoterMobile.Models.Enums;
using HockeyApp;

namespace SmartPromoter.Iphone
{
    public partial class FormDinamicoController : UIViewController
    {

        UIFormDinamico formDinamico;
        FormDinamicoCon controllerPCL;
        public FormDinamicoController() { }

        public FormDinamicoController(IntPtr handle) : base(handle) { }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            var modelForm = new FormDinamicoModel()
            {
                Db = new FormDinamicoDA(Sqlite_IOS.DB.dataBase),
                IdVisita = IdPdv,
                IdProduto = IdProduto
            };
            controllerPCL = new FormDinamicoCon(IdPdv, IdProduto, false, modelForm);
            formDinamico = new UIFormDinamico(controllerPCL, this, scrollViewFormDinamico);
            formDinamico.IniForm();

            backButton.TouchDown += (sender, e) =>
            {
                SaveForm();
            };

            gestureSaveForm.AddTarget((obj) => SaveForm());

            tabBarFormDinamico.ItemSelected += delegate
            {
                if (tabBarFormDinamico.SelectedItem.Title == tabFoto.Title)
                {
                    var actionSheetAlert = UIAlertController.Create("Tipo de foto", "Selecione a categoria da foto desejada", UIAlertControllerStyle.ActionSheet);
                    var fotos = controllerPCL.GetTagsFoto();

                    foreach (string itemType in fotos)
                    {
                        actionSheetAlert.AddAction(UIAlertAction.Create(itemType, UIAlertActionStyle.Default, (action) => { TakePhoto(itemType); }));
                    }
                    actionSheetAlert.AddAction(UIAlertAction.Create("Outros", UIAlertActionStyle.Default, (action) => { TakePhoto("Outros"); }));
                    actionSheetAlert.AddAction(UIAlertAction.Create("Cancelar", UIAlertActionStyle.Cancel, (action) => { }));
                    var presentationPopover = actionSheetAlert.PopoverPresentationController;
                    if (presentationPopover != null)
                    {
                        presentationPopover.SourceView = View;
                        presentationPopover.PermittedArrowDirections = UIPopoverArrowDirection.Up;
                    }
                    actionSheetAlert.View.TintColor = UIColor.Black;
                    PresentViewController(actionSheetAlert, true, null);
                }
                else if (tabBarFormDinamico.SelectedItem.Title == tabConcluir.Title)
                {
                    var alert = UIAlertController.Create("Concluir", "Gostaria de concluir a tarefa ?", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("Nao", UIAlertActionStyle.Cancel, (actionCancel) =>
                    {
                        MetricsManager.TrackEvent("CancelConcluirTarefa");
                    }));
                    alert.AddAction(UIAlertAction.Create("Sim", UIAlertActionStyle.Default, (actionOK) =>
                    {
                        if (formDinamico.HasInvalidateFields())
                        {
                            var alertError = UIAlertController.Create("Existem campos obrigatorios ainda nao informados.", null, UIAlertControllerStyle.Alert);
                            alertError.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Cancel, (actionCancel) => { }));
                            alertError.View.TintColor = UIColor.FromRGB(10, 88, 90);
                            PresentViewController(alertError, true, null);
                        }
                        else
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
                                var batery = ((int)(UIDevice.CurrentDevice.BatteryLevel * 100F));
                                if (gps.Location != null)
                                {
                                    controllerPCL.SetFormToTable(gps.Location.Coordinate.Latitude,
                                                                 gps.Location.Coordinate.Longitude,
                                                                 StatusAPI.CONCLUIDO, batery);
                                }
                                else
                                {
                                    controllerPCL.SetFormToTable(LocationHelper.LastLocation.Coordinate.Latitude,
                                                                 LocationHelper.LastLocation.Coordinate.Longitude,
                                                                 StatusAPI.CONCLUIDO, batery);
                                }
#if !DEBUG
                                HockeyApp.MetricsManager.TrackEvent("FormConcluido");
#endif
                                DismissViewController(true, null);
                            }
                        }
                    }));
                    alert.View.TintColor = UIColor.FromRGB(10, 88, 90);
                    PresentViewController(alert, true, null);
                }
            };


            var g = new UITapGestureRecognizer(() => View.EndEditing(true))
            {
                CancelsTouchesInView = false
            };
            scrollViewFormDinamico.AddGestureRecognizer(g);

        }

        void SaveForm()
        {
            formDinamico.UpdateValues();
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
                var batery = ((int)(UIDevice.CurrentDevice.BatteryLevel * 100F));
                if (gps.Location != null)
                {
                    controllerPCL.SetFormToTable(gps.Location.Coordinate.Latitude,
                                                 gps.Location.Coordinate.Longitude, StatusAPI.INICIADO, batery);
                }
                else
                {
                    controllerPCL.SetFormToTable(LocationHelper.LastLocation.Coordinate.Latitude,
                                                 LocationHelper.LastLocation.Coordinate.Longitude,
                                                 StatusAPI.INICIADO, batery);
                }
#if !DEBUG
                HockeyApp.MetricsManager.TrackEvent("FormAtualizado");
#endif
                DismissViewController(true, null);
            }
        }

        void TakePhoto(string typePhoto)
        {
            Camera.TakePicture(this, (obj) =>
            {
                var photo = obj.ValueForKey(new NSString("UIImagePickerControllerOriginalImage")) as UIImage;
                photo = photo.Scale(new CoreGraphics.CGSize(photo.Size.Width * 0.2, photo.Size.Height * 0.2));
                using (var imgData = photo.AsJPEG(0.7f))
                {
                    var container = new ContainerRestCon();
                    var newName_Foto = "VISITAS_" + IdPdv;
                    if (!string.IsNullOrEmpty(IdProduto))
                    {
                        newName_Foto += "_PRODUTO_" + IdProduto;
                    }
                    newName_Foto += "_TIPO_" + typePhoto + "_TIMESTAMP_" + DateTime.Now.Ticks + ".JPEG";
                    newName_Foto = newName_Foto.Replace(" ", "-");
                    container.GravarArquivo(imgData.AsStream(), newName_Foto);
#if !DEBUG
                    HockeyApp.MetricsManager.TrackEvent("FotoSucesso");
#endif
                }
            });
        }
    }
}