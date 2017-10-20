using Android.Content;
using Android.Locations;
using Android.App;
using Android.OS;
using Android.Runtime;

namespace spromotermobile.droid
{
    public class GPS : Service, ILocationListener
    {
        static object lockerObj = new object();
        public static Location lastLocation;
        static LocationManager locMgr;
        static bool alreadyRequested;
        static GPS instance;

        GPS(Context con)
        {
            if (locMgr == null)
                locMgr = con.GetSystemService(LocationService) as LocationManager;
        }

        public static GPS GetGPSTracker(Context conn)
        {
            lock (lockerObj)
            {
                if (instance == null)
                    instance = new GPS(conn);
                return instance;
            }
        }


        public Location UpdateLocation()
        {
            var Provider = locMgr.GetProvider(LocationManager.NetworkProvider);
            if (!alreadyRequested)
            {
                alreadyRequested = true;
                locMgr.RequestLocationUpdates(Provider.Name, 1, 1, this);
            }
            var templastLocation = locMgr.GetLastKnownLocation(Provider.Name);
            if (templastLocation == null)
            {
                var innerProvider = locMgr.GetProvider(LocationManager.PassiveProvider);
                templastLocation = locMgr.GetLastKnownLocation(innerProvider.Name);
                if (templastLocation == null)
                {
                    innerProvider = locMgr.GetProvider(LocationManager.GpsProvider);
                    templastLocation = locMgr.GetLastKnownLocation(innerProvider.Name);
                }
            }
            if (templastLocation != null && !templastLocation.IsFromMockProvider)
            {
                lastLocation = new Location(templastLocation);
            }
            return lastLocation;
        }

        public void Remove()
        {
            alreadyRequested = false;
            locMgr.RemoveUpdates(this);
        }


        public void OnLocationChanged(Location location)
        {
            if (location != null)
            {
                if (!location.IsFromMockProvider && IsBetterLocation(location))
                {
                    lastLocation = new Location(location);
                }
                Remove();
            }
        }

        #region overrides
        public override IBinder OnBind(Intent intent) { return null; }
        public void OnProviderDisabled(string provider) { }
        public void OnProviderEnabled(string provider) { }
        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras) { }
        #endregion overrides

        static int MINUTES = 1000 * 60 * 10;
        protected bool IsBetterLocation(Location location)
        {
            var currentBestLocation = lastLocation;
            if (currentBestLocation == null && location != null)
            {
                return true;
            }
            if (location == null)
            {
                return false;
            }
            // Check whether the new location fix is newer or older
            long timeDelta = location.Time - currentBestLocation.Time;
            var isSignificantlyNewer = timeDelta > MINUTES;
            // If it's been more than two minutes since the current location, use the new location
            // because the user has likely moved
            if (isSignificantlyNewer)
            {
                return true;
                // If the new location is more than two minutes older, it must be worse
            }
            // Check whether the new location fix is more or less accurate
            int accuracyDelta = (int)(location.Accuracy - currentBestLocation.Accuracy);
            var isMoreAccurate = accuracyDelta < 0;
            // Determine location quality using a combination of timeliness and accuracy
            if (isMoreAccurate)
            {
                return true;
            }
            return false;
        }
    }
}




