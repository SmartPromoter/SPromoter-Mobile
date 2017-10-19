//
//  LocationHelper.cs
//
//  Author:
//       leonardcolusso <leonardcolusso@gmail.com>
//
//  Copyright (c) 2016 SmartPromoter
//
using CoreLocation;

namespace SmartPromoter.Iphone
{
    public static class LocationHelper
    {
        static readonly CLLocationManager locationManager = new CLLocationManager();

        public static CLLocation LastLocation { get; set; }

        public static CLLocationManager UpdateLocation()
        {
            if (CLLocationManager.LocationServicesEnabled)
            {
                locationManager.RequestWhenInUseAuthorization();

                locationManager.StartUpdatingLocation();

                SetLastLocationOnUpdated();

                return locationManager;
            }
            return null;
        }

        static void SetLastLocationOnUpdated()
        {
            locationManager.LocationsUpdated += (sender, e) =>
            {
                foreach (CLLocation location in e.Locations)
                    LastLocation = location;
            };
        }
    }
}
