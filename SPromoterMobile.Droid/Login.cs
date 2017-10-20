using System;
using Android.App;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Java.Interop;
using SPromoterMobile.Data;
using SPromoterMobile.Controller.RESTful;
using SPromoterMobile.Models;
using SPromoterMobile.Controller;
using System.Threading;
using SPromoterMobile.Models.Exceptions;
using Android.Support.V4.Content;
using Android;
using Android.Support.V4.App;
using Android.Content.PM;
using SPromoterMobile.Models.Tables;
using spromotermobile.droid.Data;

namespace spromotermobile.droid
{

    [Activity(Label = "Login", Icon = "@drawable/ic_launcher", NoHistory = true, ScreenOrientation = ScreenOrientation.Portrait, Theme = "@style/AppTheme", WindowSoftInputMode = SoftInput.AdjustPan)]
    public class Login : Activity
    {
        LoginModel model = new LoginModel();
        LoginCon controller;
        ProgressDialog progressDialog;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.login);
            model.db = new LoginDA(SQLite_Android.DB.dataBase);
            model.rest = new LoginRestCon();
            controller = new LoginCon(model);
            ((NotificationManager)GetSystemService(NotificationService)).CancelAll();
            Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.Camera) != (int)Permission.Granted)
                {
                    ActivityCompat.RequestPermissions(this, new string[]{Manifest.Permission.ReadPhoneState, Manifest.Permission.Camera,
                                        Manifest.Permission.AccessFineLocation,
                                        Manifest.Permission.AccessCoarseLocation,
                                        Manifest.Permission.ReadExternalStorage, Manifest.Permission.WriteExternalStorage},
                                1);
                }
            }

        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
#pragma warning disable XA0001
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
#pragma warning restore XA0001
            switch (requestCode)
            {
                case 1:
                    {
                        foreach (Permission i in grantResults)
                        {
                            if (i != Permission.Granted)
                            {
                                throw new Exception("Authorization has been denied for this permission");
                            }
                        }
                        break;
                    }
            }

        }
        protected override void OnResume()
        {
            base.OnResume();
            model.isToAddNewUser = Intent.GetBooleanExtra("isToAddNewUser", false);
            if (model.db.GetInfoUsuario() != null && !model.isToAddNewUser)
            {
                StartActivity(typeof(MenuPdvs));
            }

            if (model.isToAddNewUser)
            {
                var btn = FindViewById<Button>(Resource.Id.login_btn);
                btn.Text = GetString(Resource.String.adicionar);
            }

        }

        [Export("OnLoginClicked")]
        public void OnButtonLoginClicked(View v)
        {
            var empresa = FindViewById<EditText>(Resource.Id.empresa_editTxtUser);
            var senha = FindViewById<EditText>(Resource.Id.login_editTxtPass);
            var usuario = FindViewById<EditText>(Resource.Id.login_editTxtUser);

            progressDialog = ProgressDialog.Show(this, Resources.GetString(Resource.String.carregando), Resources.GetString(Resource.String.msg_popup_login), true);
            new Thread(new ThreadStart(delegate
            {
                try
                {
                    var infoUser = controller.DoLogin(empresa.Text, usuario.Text, senha.Text);
                    infoUser = GetAvatarDeUsuario(infoUser);
                    RunOnUiThread(delegate
                    {
                        controller.InsertNewUser(infoUser);
                        Sincronizador.context = ApplicationContext;
                        Sincronizador.TryExecSync();
                    });
                    if (Intent.GetBooleanExtra("isToAddNewUser", false))
                    {
                        RunOnUiThread(() => Toast.MakeText(this, Resources.GetString(Resource.String.msg_new_user_ok), ToastLength.Long).Show());
                    }
                    progressDialog.Dismiss();
                    StartActivity(typeof(MenuPdvs));
                    OverridePendingTransition(Resource.Animation.abc_slide_in_bottom,
                                                  Resource.Animation.abc_slide_out_bottom);
                }
                catch (InvalidLoginException loginError)
                {
                    RunOnUiThread(() => Toast.MakeText(this, loginError.Message, ToastLength.Long).Show());
                    if ((progressDialog != null) && progressDialog.IsShowing)
                    {
                        progressDialog.Dismiss();
                    }
                }
                catch (Exception)
                {
                    RunOnUiThread(() => Toast.MakeText(this, Resources.GetString(Resource.String.msg_erro_inesperado_login), ToastLength.Long).Show());
                    if ((progressDialog != null) && progressDialog.IsShowing)
                    {
                        progressDialog.Dismiss();
                    }
                }
            })).Start();
        }


        protected override void OnPause()
        {
            base.OnPause();
            if ((progressDialog != null) && progressDialog.IsShowing)
            {
                progressDialog.Dismiss();
            }
            progressDialog = null;
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
            }
            catch (Container404Exception)
            {
                arquivo = null;
            }
            return tableInfo;
        }
    }
}


