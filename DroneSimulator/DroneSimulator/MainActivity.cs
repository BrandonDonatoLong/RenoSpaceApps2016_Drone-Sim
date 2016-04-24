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
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            
            _jsonPackage = new LocationData();
            _jsonPackage.id = _droneID;
            
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.button1);
            _locationText = FindViewById<TextView>(Resource.Id.coordinates);
            button.Click += delegate {
                _locationText.Text = "button pressed, waiting for location";
                if (_jsonPackage == null)
                {
                    _jsonPackage = new LocationData();
                    _jsonPackage.id = _droneID;
                }
                InitializeLocationManager();
            };

            InitializeLocationManager();

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
                    _locationText.Text = JsonConvert.SerializeObject(_jsonPackage);
                    string sContentType = "application/json";
                    var temp = new StringContent(_jsonPackage.ToString(), Encoding.UTF8, sContentType);
                    _locationText.Text = temp.ToString();

                    postToServer();
                }
            }
        }

        void postToServer()
        {
            string sUrl = string.Format("http://localhost:8080/drone?id={0}&lat={1}&lng={2}",_jsonPackage.id, _jsonPackage.lat, _jsonPackage.lng);
            string sContentType = "application/json"; // or application/xml
            

            HttpClient client = new HttpClient();
            client.MaxResponseContentBufferSize = 256000;
            var oTaskPostAsync = oHttpClient.PostAsync(sUrl, new StringContent(_jsonPackage.ToString(), Encoding.UTF8, sContentType));
            oTaskPostAsync.ContinueWith((oHttpResponseMessage) =>
            {
                _locationText.Text = "location sent";
            });
        }

        public async Task SaveTodoItemAsync(TodoItem item, bool isNewItem = false)
        {
            // RestUrl = http://developer.xamarin.com:8081/api/todoitems{0}
            var uri = new Uri(string.Format(Constants.RestUrl, item.ID));

  ...
  var json = JsonConvert.SerializeObject(item);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = null;
            if (isNewItem)
            {
                response = await client.PostAsync(uri, content);
            }
  ...

  if (response.IsSuccessStatusCode)
            {
                Debug.WriteLine(@"             TodoItem successfully saved.");

            }
  ...
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

