using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using HockeyApp;
using spromotermobile.droid.Data;
using SPromoterMobile;
using SPromoterMobile.Data;
using SPromoterMobile.Models.Enums;


namespace spromotermobile.droid
{
    [Activity(Label = "FormDinamico", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/AppTheme", WindowSoftInputMode = SoftInput.StateAlwaysHidden)]
    public class FormDinamico : GenericActivity
    {
        FormDinamicoCon controller;
        UIFormDinamico uiform;
        Camera camera;
        GenericActivityModel model;
        static bool isDoneForm;
        static string lastTagFotoSelected;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.formdinamico);
            var modelForm = new FormDinamicoModel()
            {
                Db = new FormDinamicoDA(SQLite_Android.DB.dataBase),
                IdVisita = Intent.GetStringExtra("idVisita"),
                IdProduto = Intent.GetStringExtra("idProduto")
            };
            controller = new FormDinamicoCon(Intent.GetStringExtra("idVisita"),
                                             Intent.GetStringExtra("idProduto"), false, modelForm);

            uiform = new UIFormDinamico(this, FindViewById<LinearLayout>(Resource.Id.formulario), controller);
            model = new GenericActivityModel
            {
                dbGenericActivity = new GenericActDA(SQLite_Android.DB.dataBase)
            };
            camera = new Camera(BaseContext);
            lastTagFotoSelected = null;
            isDoneForm = false;
            model.myToolbar = modelActivity.myToolbar;
            model.gps = modelActivity.gps;
            model.dialog = modelActivity.dialog;
            model.dbGenericActivity = modelActivity.dbGenericActivity;
            isToRunning = false;
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            OverridePendingTransition(Android.Resource.Animation.SlideInLeft,
                                      Android.Resource.Animation.SlideOutRight);
        }

        protected override void OnActivityResult(int requestCode, Android.App.Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            try
            {
                if (resultCode == Android.App.Result.Ok && requestCode == (int)Camera.CameraCode.OnActivityResultCode)
                {
                    var newName_Foto = "VISITAS_" + Intent.GetStringExtra("idVisita");
                    if (!string.IsNullOrEmpty(Intent.GetStringExtra("idProduto")))
                    {
                        newName_Foto += "_PRODUTO_" + Intent.GetStringExtra("idProduto");
                    }
                    newName_Foto += "_TIPO_" + lastTagFotoSelected;
                    newName_Foto = newName_Foto.Replace(" ", "-");
                    camera.PerformOnActivity(newName_Foto, DateTime.Now);
                    MetricsManager.TrackEvent("FotoSucesso");
                }
                if (resultCode != Android.App.Result.Canceled && resultCode != Android.App.Result.Ok &&
                    requestCode == (int)Camera.CameraCode.OnActivityResultCode)
                {
                    throw new Exception();
                }
            }
            catch (Exception ex)
            {
                MetricsManager.TrackEvent("FotoFalha");
                MetricsManager.TrackEvent(ex.Message);
                RunOnUiThread(() => Toast.MakeText(this, Resources.GetString(Resource.String.erro_msg_ao_tirar_foto), ToastLength.Long).Show());
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            model.myToolbar = modelActivity.myToolbar;
            model.gps = modelActivity.gps;
            model.dialog = modelActivity.dialog;
            model.dbGenericActivity = modelActivity.dbGenericActivity;
        }

        protected override void OnPause()
        {
            if (uiform != null || controller != null)
            {
                uiform.UpdateValues();
                var location = GPS.lastLocation;
                var batery = GetBatteryLevel();
                try
                {
                    if (isDoneForm)
                    {
                        if (location == null)
                        {
                            controller.SetFormToTable(0, 0, StatusAPI.CONCLUIDO, batery);
                        }
                        else
                        {
                            controller.SetFormToTable(location.Latitude, location.Longitude, StatusAPI.CONCLUIDO, batery);
                        }
                        MetricsManager.TrackEvent("FormConcluido");
                    }
                    else
                    {
                        if (location == null)
                        {
                            controller.SetFormToTable(0, 0, StatusAPI.INICIADO, batery);
                        }
                        else
                        {
                            controller.SetFormToTable(location.Latitude, location.Longitude, StatusAPI.INICIADO, batery);
                        }
                        MetricsManager.TrackEvent("FormAtualizado");
                    }
                }
                catch (NullReferenceException ex)
                {
                    MetricsManager.TrackEvent("FormError - " + ex.Message);
                    RunOnUiThread(() => Toast.MakeText(this, Resources.GetString(Resource.String.erro_formulario), ToastLength.Long).Show());
                }
            }
            base.OnPause();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_forms, menu);
            model.myToolbar.Title = GetString(Resource.String.formulario);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.action_foto:
                    DialogFoto(FindViewById(Resource.Id.action_foto));
                    break;
                case Resource.Id.action_concluir:
                    DialogConcluir();
                    break;
                case Resource.Id.action_almoco:
                    OnClickAlmoco();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        #region dialogs
        void DialogFoto(View view)
        {
            var menu = new PopupMenu(this, view);
            var tags = controller.GetTagsFoto();
            foreach (var item in tags)
            {
                menu.Menu.Add(item);
            }
            menu.MenuItemClick += (object sender, PopupMenu.MenuItemClickEventArgs e) =>
            {
                lastTagFotoSelected = e.Item.ToString();
                menu.Dismiss();
                StartActivityForResult(camera.PerfomCamera(), (int)Camera.CameraCode.OnActivityResultCode);
                OverridePendingTransition(Resource.Animation.abc_slide_in_top,
                                  Resource.Animation.abc_slide_out_top);
            };
            menu.Inflate(Resource.Menu.menu_popup);
            RunOnUiThread(() => menu.Show());
        }

        void DialogConcluir()
        {
            AlertDialog.Builder dialogBuilder;
            dialogBuilder = new AlertDialog.Builder(this, Resource.Style.DialogTheme);
            dialogBuilder.SetTitle(Resources.GetString(Resource.String.send_form));
            dialogBuilder.SetMessage(Resources.GetString(Resource.String.send_form_desc));
            dialogBuilder.SetNegativeButton(Resources.GetString(Resource.String.nao), delegate
            {
                MetricsManager.TrackEvent("CancelConcluirForm");
            });
            dialogBuilder.SetPositiveButton(Resources.GetString(Resource.String.sim),
                delegate
                {
                    if (uiform.HasInvalidField())
                    {
                        Toast.MakeText(this, Resources.GetString(Resource.String.form_error_validation),
                                   ToastLength.Long).Show();
                    }
                    else
                    {
                        isDoneForm = true;
                        Toast.MakeText(this, Resources.GetString(Resource.String.form_concluido),
                                       ToastLength.Long).Show();
                        OnBackPressed();
                    }
                });
            model.dialog = dialogBuilder.Create();
            model.dialog.Show();
        }
        #endregion dialogs
    }
}

