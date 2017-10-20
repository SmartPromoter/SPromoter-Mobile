using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using spromotermobile.droid.Data;
using SPromoterMobile.Data;

namespace spromotermobile.droid
{
    [Activity(Label = "SmartPromoter", Icon = "@drawable/ic_launcher", MainLauncher = true, NoHistory = true, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme = "@style/AppTheme.Splash", HardwareAccelerated = true, WindowSoftInputMode = SoftInput.AdjustPan)]
    public class Splash : AppCompatActivity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }


        protected override void OnResume()
        {
            base.OnResume();
            var startupWork = new Task(() =>
            {
                new SQLHelper().InitTables(SQLite_Android.DB.dataBase);
                GPS.GetGPSTracker(this.ApplicationContext);
            });

            startupWork.ContinueWith(t =>
            {
                StartActivity(new Intent(Application.Context, typeof(Login)));
                OverridePendingTransition(Android.Resource.Animation.SlideInLeft,
                                                  Android.Resource.Animation.SlideOutRight);
            }, TaskScheduler.FromCurrentSynchronizationContext());
            startupWork.Start();
        }
    }
}

