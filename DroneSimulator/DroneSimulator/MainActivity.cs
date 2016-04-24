using Android.App;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Locations;
using System.Collections.Generic;
using System.Linq;
using Android.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System;

namespace DroneSimulator
{
    [Activity(Label = "DroneSimulator", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity, ILocationListener
    {
        bool firstTimeThrough = true;
        static readonly string TAG = "X:" + typeof(MainActivity).Name;
        Location _currentLocation;
        LocationManager _locationManager;
        string _locationProvider;
        TextView _locationText;
        string _droneID = "123";
        LocationData _jsonPackage;
        Spinner _testCoordinates;

        HttpClient client = new HttpClient();
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);


            client.MaxResponseContentBufferSize = 256000;

            _jsonPackage = new LocationData();
            _jsonPackage.id = _droneID;
            
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.button1);
            _locationText = FindViewById<TextView>(Resource.Id.coordinates);
            _testCoordinates = FindViewById<Spinner>(Resource.Id.CoordinateDrop);

            _testCoordinates.ItemSelected += _testCoordinates_ItemSelected;

            var adapter = ArrayAdapter.CreateFromResource(
                this, Resource.Array.coodrinate_array, Android.Resource.Layout.SimpleSpinnerItem);

            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            _testCoordinates.Adapter = adapter;

            button.Click += delegate {
                //resume collecting location data
                if (_locationManager != null)
                    _locationManager.RequestLocationUpdates(_locationProvider, 0, 0, this);

                _locationText.Text = "button pressed, waiting for location";
                if (_jsonPackage == null)
                {
                    _jsonPackage = new LocationData();
                    _jsonPackage.id = _droneID;
                }
            };

            InitializeLocationManager();


        }

        private void _testCoordinates_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            //pause test coordinates
            if (_locationManager != null)
                _locationManager.RemoveUpdates(this);

            Spinner spinner = (Spinner)sender;

            string toast = string.Format("the coordinates are {0}", spinner.GetItemAtPosition(e.Position));
            if (e.Position != 0)
            {
                Location dummyLocation = new Location("test");
                string[] split_coords = spinner.GetItemAtPosition(e.Position).ToString().Split(',');
                double lat = 0, lng = 0;
                if (!double.TryParse(split_coords[0], out lat) || !double.TryParse(split_coords[1], out lng))
                {
                    Toast.MakeText(this, "ERROR, Cannot parse lat & long",ToastLength.Short).Show();
                    return;
                }
                else
                {
                    dummyLocation.Latitude = lat;
                    dummyLocation.Longitude = lng;
                    OnLocationChanged(dummyLocation);
                    //force an update from the server
                    firstTimeThrough = true;
                    Toast.MakeText(this, toast, ToastLength.Long).Show();
                }
                Toast.MakeText(this, toast, ToastLength.Long).Show();
            }
        }

        public async void OnLocationChanged(Location location)
        {
            if (location == null)
            {
                _locationText.Text = "Unable to determine your location. Try again in a short while.";
            }
            else
            {
                //first time through, go on ahead and copy it over
                if (_currentLocation == null)
                {
                    _currentLocation = location;
                    
                }
                var roundedLat = roundDouble(location.Latitude);
                var roundedLong = roundDouble(location.Longitude);

                if (_currentLocation.Latitude != roundedLat || _currentLocation.Longitude != roundedLong || firstTimeThrough)
                {
                    firstTimeThrough = false;
                    _currentLocation.Latitude = roundedLat;
                    _currentLocation.Longitude = roundedLong;
                    if (_jsonPackage != null)
                    {
                        _jsonPackage.lat = _currentLocation.Latitude;
                        _jsonPackage.lng = _currentLocation.Longitude;
                    }
                    _locationText.Text = string.Format("{0:f4},{1:f4}", roundedLat,roundedLong);

                    postToServer();
                }
            }
        }

        async void postToServer()
        {
            var uri = new Uri (string.Format("http://10.0.1.6:8080/drone?id={0}&lat={1}&lng={2}",_jsonPackage.id, _jsonPackage.lat, _jsonPackage.lng));
            string sContentType = "application/json"; // or application/xml
            var json = JsonConvert.SerializeObject(_jsonPackage);
            var content = new StringContent(json, Encoding.UTF8, sContentType);
            HttpResponseMessage response = null;
            
            response = await client.PostAsync(uri, content);

            if (response.IsSuccessStatusCode)
            {
                Toast.MakeText(this, "post successful", ToastLength.Short).Show();
            }
            else
            {
                Toast.MakeText(this, string.Format("ERROR, something went wrong {0}",response.StatusCode), ToastLength.Short).Show();
            }
        }

        double roundDouble(double input)
        {
            string temp = string.Format("{0:f4}",input);
            double output = 0;
            double.TryParse(temp, out output);
            return output;
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

