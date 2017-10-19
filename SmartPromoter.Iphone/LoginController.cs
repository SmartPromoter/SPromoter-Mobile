using System;
using System.Threading;
#if !DEBUG
using HockeyApp.iOS;
#endif
using SPromoterMobile.Controller;
using SPromoterMobile.Controller.RESTful;
using SPromoterMobile.Data;
using SPromoterMobile.Models;
using SPromoterMobile.Models.Exceptions;
using SPromoterMobile.Models.Tables;
using UIKit;

namespace SmartPromoter.Iphone
{
    public partial class LoginController : UIViewController
    {
        LoadingOverlay loadingOverlay;
        LoginModel model = new LoginModel();
        LoginCon controller;

        protected LoginController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            model.db = new LoginDA(Sqlite_IOS.DB.dataBase);
            model.rest = new LoginRestCon();
            controller = new LoginCon(model);

            if (model.db.GetInfoUsuario() != null)
            {
                MoveToFeedPDV();
            }

            txtEmpresaLogin.ShouldReturn = (textField) =>
            {
                txtUsuarioLogin.BecomeFirstResponder();
                return true;
            };
            txtEmpresaLogin.ReturnKeyType = UIReturnKeyType.Next;
            txtUsuarioLogin.ShouldReturn = (textField) =>
            {
                txtSenhaLogin.BecomeFirstResponder();
                return true;
            };
            txtUsuarioLogin.ReturnKeyType = UIReturnKeyType.Next;
            txtSenhaLogin.ShouldReturn = (textField) =>
            {
                txtSenhaLogin.EndEditing(true);
                ExecLogin();
                return true;
            };
            txtSenhaLogin.ReturnKeyType = UIReturnKeyType.Send;

            var g = new UITapGestureRecognizer(() => View.EndEditing(true))
            {
                CancelsTouchesInView = false
            };
            View.AddGestureRecognizer(g);

        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
        }

        void MoveToFeedPDV()
        {
            if (Storyboard.InstantiateViewController("FeedPDV") is PDVBarController feedPDV)
            {
                ShowViewController(feedPDV, null);
            }
        }

        partial void ClickLogin(UIButton sender)
        {
            ExecLogin();
        }

        void ExecLogin()
        {
            model.empresa = txtEmpresaLogin.Text;
            model.login = txtUsuarioLogin.Text;
            model.password = txtSenhaLogin.Text;

            var bounds = UIScreen.MainScreen.Bounds;

            loadingOverlay = new LoadingOverlay(bounds);
            loadingOverlay.ExecOverLay();
            View.Add(loadingOverlay);

            new Thread(new ThreadStart(delegate
            {
                try
                {
                    SyncronizerModel modelSync;
                    SyncronizerCon controllerSync;
                    var infoUser = controller.DoLogin(model.empresa, model.login, model.password);
                    infoUser = GetAvatarDeUsuario(infoUser);
                    GetLogoEmpresa(infoUser.SERVIDOR.Substring(0, infoUser.SERVIDOR.IndexOf(".",
                                StringComparison.CurrentCulture)), infoUser);
                    InvokeOnMainThread(() =>
                    {
                        controller.InsertNewUser(infoUser);
                        modelSync = new SyncronizerModel()
                        {
                            db = new SyncronizerDA(Sqlite_IOS.DB.dataBase),
                            dbCache = new CacheDA(Sqlite_IOS.DB.dataBase)
                        };
                        controllerSync = new SyncronizerCon(modelSync);
                        controllerSync.ExecRestApis();
#if !DEBUG
                        BITHockeyManager.SharedHockeyManager.UserId = infoUser.ID;
                        BITHockeyManager.SharedHockeyManager.UserEmail = infoUser.SERVIDOR;
                        BITHockeyManager.SharedHockeyManager.StartManager();
#endif
                        MoveToFeedPDV();
                    });
                }
                catch (InvalidLoginException loginError)
                {
                    InvokeOnMainThread(() =>
                    {
                        var alert = UIAlertController.Create(loginError.Message, null, UIAlertControllerStyle.Alert);
                        alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
                        alert.View.TintColor = UIColor.FromRGB(10, 88, 90);
                        PresentViewController(alert, true, null);
                    });
                }
                catch (Exception ex)
                {

                    var exi = ex.Message;
                    InvokeOnMainThread(() =>
                    {
                        var alert = UIAlertController.Create(
                            "Ocorreu um erro inesperado, verifique sua conexão e os dados de login",
                                                             null, UIAlertControllerStyle.Alert);
                        alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
                        alert.View.TintColor = UIColor.FromRGB(10, 88, 90);
                        PresentViewController(alert, true, null);
                    });


                }
                finally
                {
                    InvokeOnMainThread(() =>
                    {
                        loadingOverlay.Hide();
                    });
                }
            })).Start();
        }


        public TB_USUARIO GetAvatarDeUsuario(TB_USUARIO tableInfo)
        {
            var containerLocal = new ContainerRestCon();
            var arquivo = containerLocal.GetNomeArquivo(tableInfo.ID + ".JPEG");
            try
            {
                if (arquivo == null)
                {
                    tableInfo.AVATAR = new AzureStorage().DownloadImage(tableInfo.ID + ".JPEG", tableInfo.SERVIDOR);
                    return tableInfo;
                }
                tableInfo.AVATAR = arquivo;
                return tableInfo;

            }
            catch (Container404Exception)
            {
                arquivo = null;
            }
            return tableInfo;
        }

        void GetLogoEmpresa(string empresa, TB_USUARIO tableInfo)
        {
            var containerLocal = new ContainerRestCon();
            var arquivo = containerLocal.GetNomeArquivo(empresa + ".jpg");
            try
            {
                if (arquivo == null)
                {
                    var modelSync = new SyncronizerModel()
                    {
                        dbCache = new CacheDA(Sqlite_IOS.DB.dataBase)
                    };
                    new AzureStorage().DownloadImage(empresa + ".jpg", tableInfo.SERVIDOR);
                }
            }
            catch (Container404Exception)
            {
                arquivo = null;
            }
        }


    }
}

