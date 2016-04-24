using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace DroneSimulator
{
    class LocationData
    {
        public string id;
        public double lat;
        public double lng;

        public LocationData()
        {
            id = "null";
            lat = 0;
            lng = 0;
        }
    }

    public class TodoItem
    {
        public string ID { get; set; }

        public string Name { get; set; }

        public string Notes { get; set; }

        public bool Done { get; set; }
    }

    public static class Constants
    {
        // URL of REST service
        public static string RestUrl = "http://developer.xamarin.com:8081/api/todoitems{0}";
        // Credentials that are hard coded into the REST service
        public static string Username = "Xamarin";
        public static string Password = "Pa$$w0rd";
    }
}