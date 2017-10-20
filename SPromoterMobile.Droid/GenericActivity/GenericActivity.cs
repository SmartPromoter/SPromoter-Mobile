using Android.Content;
using Android.OS;
using Android.Views;
using Android.Support.V7.App;
using SPromoterMobile.Data;
using Android.Widget;
using Android.App;
using Android.Runtime;
using Android.Provider;
using Android.Locations;
using SPromoterMobile;
using HockeyApp.Android;
using spromotermobile.droid.Data;
using spromotermobile.droid.sync;
using Java.Lang;
using HockeyApp;

namespace spromotermobile.droid
{
    public abstract class GenericActivity : AppCompatActivity
    {
        internal GenericActivityModel modelActivity;
        GenericCon controller;
        internal bool isToRunning = true;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
            modelActivity = new GenericActivityModel()
            {
                dbGenericActivity = new GenericActDA(SQLite_Android.DB.dataBase),
                gps = GPS.GetGPSTracker(ApplicationContext),
                myToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar)
            };
            controller = new GenericCon(modelActivity);
            HockeyApp.Android.Metrics.MetricsManager.Register(Application);
        }

        protected override void OnResume()
        {
            base.OnResume();
            Tracking.StartUsage(this);
            CheckApp();
            CustomContentResolver.GetCustomContentResolver(this).Sync(this);
            var result = modelActivity.dbGenericActivity.GetUsersIDsLogged();
            if (result == null || result.Count <= 0)
            {
                StartActivity(typeof(Login));
                OverridePendingTransition(Resource.Animation.abc_slide_in_top,
                                          Resource.Animation.abc_slide_out_top);
                Finish();
            }
            modelActivity.myToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(modelActivity.myToolbar);
            if (controller.IsHrDeAlmoco())
            {
                IniPopUpAlmoco(Resources.GetString(Resource.String.dialog_confirm_end_almoco));
            }
            if (isToRunning || (modelActivity.dialog == null || !modelActivity.dialog.IsShowing))
            {
                if (Sincronizador.context == null || Sincronizador.context != ApplicationContext)
                {
                    Sincronizador.context = ApplicationContext;
                }
                Sincronizador.TryExecSync();
            }
        }

        protected override void OnPause()
        {
            Tracking.StopUsage(this);
            if (isToRunning && (modelActivity.dialog == null || !modelActivity.dialog.IsShowing))
            {
                RunOnUiThread(delegate
                {
                    if (Sincronizador.context == null || Sincronizador.context != ApplicationContext)
                    {
                        Sincronizador.context = ApplicationContext;
                    }
                    Sincronizador.TryExecSync();
                });
            }

            if ((modelActivity.dialog != null) && modelActivity.dialog.IsShowing)
            {
                modelActivity.dialog.Dismiss();
            }
            modelActivity.dialog = null;
            base.OnPause();
        }

        public override void OnBackPressed()
        {
            Finish();
        }

        public void FecharActivity()
        {
            if (Sincronizador.context == null || Sincronizador.context != ApplicationContext)
            {
                Sincronizador.context = ApplicationContext;
            }
            Sincronizador.TryExecSync();
            MoveTaskToBack(true);
            OverridePendingTransition(Resource.Animation.abc_slide_in_bottom,
                              Resource.Animation.abc_slide_out_bottom);
        }

        bool SetAlmoco()
        {
            bool result = false;
            switch (controller.SetAlmoco())
            {
                case StatusPontoEletronico.NAO_INICIADO:
                    Toast.MakeText(this, Resources.GetString(Resource.String.almoco_error_ponto), ToastLength.Long).Show();
                    break;
                case StatusPontoEletronico.ALMOCO_INICIADO:
                    result = true;
                    break;
                case StatusPontoEletronico.ALMOCO_FINALIZADO:
                    Toast.MakeText(this, Resources.GetString(Resource.String.almoco_finalizado), ToastLength.Long).Show();
                    var resultintent = new Intent(this, typeof(AlarmReceiverAlmoco));
                    var pending = PendingIntent.GetBroadcast(this, (int)TipoNotificacao.AlarmeAlmoco, resultintent, PendingIntentFlags.UpdateCurrent);
                    if (pending != null)
                    {
                        var alarmManager = GetSystemService(AlarmService).JavaCast<AlarmManager>();
                        alarmManager.Cancel(pending);
                        pending.Cancel();
                    }
                    break;
                case StatusPontoEletronico.CHECKOUT:
                    Toast.MakeText(this, Resources.GetString(Resource.String.almoco_error), ToastLength.Long).Show();
                    break;
            }
            return result;
        }

        #region OnClicksToolBars
        protected void OnClickAlmoco()
        {
            IniPopUpAlmoco(Resources.GetString(Resource.String.dialog_confirm_almoco));
        }

        protected void IniPopUpAlmoco(string msg)
        {
            var hrsDeAlmoco = controller.GetAlmoco();
            if (hrsDeAlmoco.Count == 1)
            {
                var timeAlmoco = hrsDeAlmoco[0].ToString("HH:mm");
                msg = "Iniciado as: " + timeAlmoco + "\n\n" + msg;
            }

            Android.App.AlertDialog.Builder dialogBuilder;
            dialogBuilder = new Android.App.AlertDialog.Builder(this, Resource.Style.DialogTheme);
            dialogBuilder.SetTitle(Resources.GetString(Resource.String.hr_almoco));
            dialogBuilder.SetMessage(msg);
            dialogBuilder.SetNegativeButton(Resources.GetString(Resource.String.nao), delegate
            {
                if (controller.IsHrDeAlmoco())
                {
                    Toast.MakeText(this, Resources.GetString(Resource.String.dialog_ainda_no_almoco), ToastLength.Long).Show();
                    var intent = new Intent(Intent.ActionMain);
                    intent.AddCategory(Intent.CategoryHome);
                    intent.SetFlags(ActivityFlags.NewTask);
                    StartActivity(intent);
                    OverridePendingTransition(Resource.Animation.abc_slide_in_bottom,
                                                  Resource.Animation.abc_slide_out_bottom);
                }
            });
            dialogBuilder.SetPositiveButton(Resources.GetString(Resource.String.sim),
                delegate
                {
                    MetricsManager.TrackEvent("CancelAlmoco");
                    if (SetAlmoco())
                    {
                        var alarmIntent = new Intent(this, typeof(AlarmReceiverAlmoco));
                        alarmIntent.PutExtra("title", Resources.GetString(Resource.String.hr_almoco));
                        alarmIntent.PutExtra("message", Resources.GetString(Resource.String.min_horario_de_almoco));

                        var pending = PendingIntent.GetBroadcast(this, (int)TipoNotificacao.AlarmeAlmoco, alarmIntent, PendingIntentFlags.UpdateCurrent);
                        var alarmManager = GetSystemService(AlarmService).JavaCast<AlarmManager>();
                        //50 minutos = 3000000
                        alarmManager.Set(AlarmType.ElapsedRealtime, SystemClock.ElapsedRealtime() + 3000000, pending);
                        Toast.MakeText(this, Resources.GetString(Resource.String.almoco_ini), ToastLength.Long).Show();
                        FecharActivity();
                    }
                });
            modelActivity.dialog = dialogBuilder.Create();
            modelActivity.dialog.Show();
        }
        #endregion OnClicksToolbars

        #region Validations
        internal void CheckApp()
        {
            if (IsAirplaneModeOn())
            {
                Toast.MakeText(this, GetString(Resource.String.modo_aviao), ToastLength.Long).Show();
                FecharActivity();
            }
            if (CheckGpsOn())
            {
                Toast.MakeText(this, GetString(Resource.String.gps_off), ToastLength.Long).Show();
                FecharActivity();
            }
            if (IsTimeAutomatic())
            {
                Toast.MakeText(this, GetString(Resource.String.error_time_automatic), ToastLength.Long).Show();
                FecharActivity();
            }
#if !DEBUG
            if (IsDevModeOn())
            {
                Toast.MakeText(this, GetString(Resource.String.error_dev_mode_on), ToastLength.Long).Show();
                FecharActivity();
            }
#endif
        }

        bool IsDevModeOn()
        {
            return Settings.Global.GetInt(ContentResolver,
                                          Settings.Global.DevelopmentSettingsEnabled, 0) == 1;
        }

        bool IsTimeAutomatic()
        {
            return !(Settings.Global.GetInt(ContentResolver, Settings.Global.AutoTime, 0) == 1 &&
                    Settings.Global.GetInt(ContentResolver, Settings.Global.AutoTimeZone, 0) == 1);
        }

        bool IsAirplaneModeOn()
        {
            return Settings.Global.GetInt(ContentResolver,
                                          Settings.Global.AirplaneModeOn, 0) == 1;
        }

        bool CheckGpsOn()
        {
            var mlocManager = (LocationManager)GetSystemService(LocationService);
            var gps = mlocManager.IsProviderEnabled(LocationManager.GpsProvider);
            return !gps;
        }
        #endregion Validations

        protected void OnClickBtnNewUser()
        {
            var i = new Intent(this, typeof(Login));
            i.PutExtra("isToAddNewUser", true);
            StartActivity(i);
            OverridePendingTransition(Resource.Animation.abc_slide_in_top,
                                      Resource.Animation.abc_slide_out_top);
        }

        public int GetBatteryLevel()
        {
            try
            {
                var batteryIntent = RegisterReceiver(null, new IntentFilter(Intent.ActionBatteryChanged));
                float level = batteryIntent != null ? batteryIntent.GetIntExtra(BatteryManager.ExtraLevel, -1) : 0;
                float scale = batteryIntent != null ? batteryIntent.GetIntExtra(BatteryManager.ExtraScale, -1) : 0;
                if ((int)level == -1 || (int)scale == -1)
                {
                    return 50;
                }
                return (int)((level / scale) * 100.0f);
            }
            catch (NullPointerException)
            {
                return 0;
            }
        }
    }
}