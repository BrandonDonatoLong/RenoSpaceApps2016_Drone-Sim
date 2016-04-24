using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Locations;
using System.Collections.Generic;
using System.Linq;
using Android.Util;

namespace DroneSimulator
{
    [Activity(Label = "DroneSimulator", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity, ILocationListener
    {
        static readonly string TAG = "X:" + typeof(MainActivity).Name;
        Location _currentLocation;
        LocationManager _locationManager;
        string _locationProvider;
        TextView _locationText;
        string _droneID = "Drone123";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.button1);
            _locationText = FindViewById<TextView>(Resource.Id.coordinates);
            button.Click += delegate {
                _locationText.Text = "button pressed, waiting for location";
                InitializeLocationManager();
            };

            InitializeLocationManager();

        }

        public async void OnLocationChanged(Location location)
        {
            //if (location.Latitude - _currentLocation.Latitude > .001 || location.Longitude - _currentLocation.Longitude > .001)
            //{
            //    Log.Debug(TAG, "sensativity hit");
            //}
            _currentLocation = location;
            if (_currentLocation == null)
            {
                _locationText.Text = "Unable to determine your location. Try again in a short while.";
            }
            else
            {
                _locationText.Text = string.Format("{0:f6},{1:f6}", _currentLocation.Latitude, _currentLocation.Longitude);

                //var CurrentLocationJSON = string.Format("{\"id:\"{0}\"location\": {{ \"lat\": {1}  \"lng\": {2} }}", _droneID, _currentLocation.Latitude, _currentLocation.Longitude);
                //_locationText.Text = CurrentLocationJSON;
                
            }
        }

        public void OnProviderDisabled(string provider) { }

        public void OnProviderEnabled(string provider) { }

        public void OnStatusChanged(string provider, Availability status, Bundle extras) { }

        protected override void OnResume()
        {
            base.OnResume();
            if (_locationManager != null)
                _locationManager.RequestLocationUpdates(_locationProvider, 0, 0, this);
        }
        
        protected override void OnPause()
        {
            base.OnPause();
            if (_locationManager != null)
                _locationManager.RemoveUpdates(this);
        }

        protected void InitializeLocationManager()
        {
            _locationManager = (LocationManager)GetSystemService(LocationService);
            Criteria criteriaForLocationService = new Criteria
            {
                Accuracy = Accuracy.Fine
            };
            IList<string> acceptableLocationProviders = _locationManager.GetProviders(criteriaForLocationService, true);

            if (acceptableLocationProviders.Any())
            {
                _locationProvider = acceptableLocationProviders.First();
            }
            else
            {
                _locationProvider = string.Empty;
            }
            Log.Debug(TAG, "Using " + _locationProvider + ".");
        }
    }
}

