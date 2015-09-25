using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using HereFriendsSenderMap.Resources;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using System.IO.IsolatedStorage;
using System.Windows.Shapes;
using System.Windows.Media;
using Microsoft.Phone.Maps.Controls;
using System.Device.Location;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Maps.Services;
using System.ComponentModel;
using Telerik.Windows.Controls;
using Telerik.Windows.Data;
using Telerik.Windows.Controls.Primitives;

namespace HereFriendsSenderMap
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            showRate();
            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
            ShellTile tile = CheckIfTileExist("Share Location");
            if (tile != null)
            {
                pinTileButton.Content = "UnPin Quick Share Tile";
            }
            textBlock1.Text = "";
            stackpanel.Visibility = System.Windows.Visibility.Visible;
            
        }

        GeoCoordinate MyCoordinate = new GeoCoordinate();
        private ReverseGeocodeQuery MyReverseGeocodeQuery = null;
        private bool fromTile = false;

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {

            //if (this.NavigationContext.QueryString.ContainsKey("Comp"))
            //{
            //    stackpanel.Visibility = System.Windows.Visibility.Collapsed;
            //    pineedStackPanel.Visibility = System.Windows.Visibility.Visible;
            //    string param = this.NavigationContext.QueryString["Comp"];//Get "Param" this QueryString. 
            //    //textBlock1.Text = "Welcome back from " + param;
            //    bool result = (param.Equals("Completed"));
            //    if (result)
            //    {
            //        loadingText.Text = "Process Completed, Press back or home to go to the main menu";
            //        return;
            //    }

            //} 
            
            if (this.NavigationContext.QueryString.ContainsKey("Param"))
            {
                stackpanel.Visibility = System.Windows.Visibility.Collapsed;
                pineedStackPanel.Visibility = System.Windows.Visibility.Visible;
                if (!fromTile)
                {
                    string param = this.NavigationContext.QueryString["Param"];//Get "Param" this QueryString. 
                    //textBlock1.Text = "Welcome back from " + param;
                    bool result = (param.Equals("Share Location"));
                    if (result)
                    {
                        getLocation();
                    }
                }
                else
                {
                    loadingText.Text = "Process Completed, Press back or home to return to the phones main menu";
                }
            } 

            if (IsolatedStorageSettings.ApplicationSettings.Contains("LocationConsent"))
            {
                //User already gave us his agreement for using his position
                if ((bool)IsolatedStorageSettings.ApplicationSettings["LocationConsent"] == true)

                    return;
                //If he didn't we ask for it
                else
                {
                    MessageBoxResult result =
                                MessageBox.Show("Can I use your position?",
                                "Location",
                                MessageBoxButton.OKCancel);

                    if (result == MessageBoxResult.OK)
                    {
                        IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = true;
                    }
                    else
                    {
                        IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = false;
                    }

                    IsolatedStorageSettings.ApplicationSettings.Save();
                }
            }

                //Ask for user agreement in using his position
            else
            {
                MessageBoxResult result =
                            MessageBox.Show("Can I use your position?",
                            "Location",
                            MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.OK)
                {
                    IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = true;
                }
                else
                {
                    IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = false;
                }

                IsolatedStorageSettings.ApplicationSettings.Save();
            }



        }

        private void showRate()
        {
            RadRateApplicationReminder ratemyapp = new RadRateApplicationReminder();
            ratemyapp.MessageBoxInfo.Content = "Thank you for using my application! Please rate this app so that I can make more applications and improve this one, Thank you!";
            ratemyapp.MessageBoxInfo.Title = "Give us a 5 star rating!";
            ratemyapp.AllowUsersToSkipFurtherReminders = true ;
            ratemyapp.RecurrencePerUsageCount = 4;
            ratemyapp.Notify();
        }

        //protected override void OnBackKeyPress(CancelEventArgs e)
        //{
        //    base.OnBackKeyPress(e);

        //    //if (!fromTile)
        //    //{
        //    //    // No history, allow the back button
        //    //    // Or do whatever you need to do, like navigate the application page
        //    //    return;
        //    //}

        //    // Cancel the back button press
        //    e.Cancel = true;

        //    NavigationService.Navigate(new Uri("/MainPage.xaml?Comp=Completed", UriKind.Relative));
        //}

        private async void getLocation()
    {
            //Check for the user agreement in use his position. If not, method returns.
            if ((bool)IsolatedStorageSettings.ApplicationSettings["LocationConsent"] != true)
            {
                // The user has opted out of Location.
                return;
            }

            //Declare and Inizialize a Geolocator object
            Geolocator geolocator = new Geolocator();
            //Set his accuracy in Meters 
            geolocator.DesiredAccuracyInMeters = 0;
            geolocator.DesiredAccuracy = PositionAccuracy.High;
            //Let's get the position of the user. Since there is the possibility of getting an Exception, this method is called inside a try block
            try
            {
               
                //The await guarantee the calls  to be returned on the thread from which they were called
                //Since it is call directly from the UI thread, the code is able to access and modify the UI directly when the call returns.
                Geoposition geoposition = await geolocator.GetGeopositionAsync(
                    maximumAge: TimeSpan.FromMinutes(5),
                    timeout: TimeSpan.FromSeconds(10)
                    );

                
                //With this 2 lines of code, the app is able to write on a Text Label the Latitude and the Longitude, given by Geoposition

                MyCoordinate.Latitude = geoposition.Coordinate.Latitude;
                MyCoordinate.Longitude = geoposition.Coordinate.Longitude;

                x = MyCoordinate.Longitude.ToString() + MyCoordinate.Latitude.ToString();
                
                if (MyReverseGeocodeQuery == null || !MyReverseGeocodeQuery.IsBusy)
                {
                    MyReverseGeocodeQuery = new ReverseGeocodeQuery();
                    MyReverseGeocodeQuery.GeoCoordinate = new GeoCoordinate(MyCoordinate.Latitude, MyCoordinate.Longitude);
                    MyReverseGeocodeQuery.QueryCompleted += ReverseGeocodeQuery_QueryCompleted;
                    MyReverseGeocodeQuery.QueryAsync();
                }
                
                //StatusTextBlock.Text = "Status = This is your position :)";
            }
            //If an error is catch 2 are the main causes: the first is that you forgot to includ ID_CAP_LOCATION in your app manifest. 
            //The second is that the user doesn't turned on the Location Services
            catch (Exception ex)
            {
                if ((uint)ex.HResult == 0x80004004)
                {
                    //StatusTextBlock.Text = "Status= Location  is disabled in phone settings.";
                }
                else
                {
                    // something else happened during the acquisition of the location

                }
            }

    }

        private void ReverseGeocodeQuery_QueryCompleted(object sender, QueryCompletedEventArgs<IList<MapLocation>> e)
        {
            if (e.Error == null)
            {
                if (e.Result.Count > 0)
                {
                    MapAddress address = e.Result[0].Information.Address;
                    x = "My Current Location: " + address.HouseNumber + " " + address.Street + ", " + address.City + ", " + address.State + " " + address.PostalCode;
                    fromTile = true;
                    textBlock1.Text = "";
                    ShareSMS();
                    
                }
            }
        }

        string x;


        private void ShareSMS()
        {
            SmsComposeTask smsComposeTask = new SmsComposeTask()
            {
                Body = x 
            };
            
            smsComposeTask.Show();
        }

        //Geolocator geolocator = null;
        private void ShowMyLocationOnTheMap()
        {
            if (MyCoordinate == null)
            {
                return;
            }
            myMap.Layers.Clear();

            // Create a small circle to mark the current location.
            Ellipse myCircle = new Ellipse();
            myCircle.Fill = new SolidColorBrush(Colors.Blue);
            myCircle.Height = 20;
            myCircle.Width = 20;
            myCircle.Opacity = 50;

            // Create a MapOverlay to contain the circle.
            MapOverlay myLocationOverlay = new MapOverlay();
            myLocationOverlay.Content = myCircle;
            myLocationOverlay.PositionOrigin = new Point(0.5, 0.5);
            myLocationOverlay.GeoCoordinate = MyCoordinate;

            // Create a MapLayer to contain the MapOverlay.
            MapLayer locationLayer = new MapLayer();
            locationLayer.Add(myLocationOverlay);


            // Add the MapLayer to the Map.
            myMap.Layers.Add(locationLayer);
            myMap.Center = MyCoordinate;
            myMap.ZoomLevel = 16;
        }

        private async void FSLocationButton_Click(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //Check for the user agreement in use his position. If not, method returns.
            if ((bool)IsolatedStorageSettings.ApplicationSettings["LocationConsent"] != true)
            {
                // The user has opted out of Location.
                return;
            }

            //Declare and Inizialize a Geolocator object
            Geolocator geolocator = new Geolocator();
            //Set his accuracy in Meters 
            geolocator.DesiredAccuracyInMeters = 50;
            
            //Let's get the position of the user. Since there is the possibility of getting an Exception, this method is called inside a try block
            try
            {
                textBlock1.Text = "searching...";
                //The await guarantee the calls  to be returned on the thread from which they were called
                //Since it is call directly from the UI thread, the code is able to access and modify the UI directly when the call returns.
                Geoposition geoposition = await geolocator.GetGeopositionAsync(
                    maximumAge: TimeSpan.FromMinutes(5),
                    timeout: TimeSpan.FromSeconds(10)
                    );

                
                //With this 2 lines of code, the app is able to write on a Text Label the Latitude and the Longitude, given by Geoposition

                MyCoordinate.Latitude = geoposition.Coordinate.Latitude;
                MyCoordinate.Longitude = geoposition.Coordinate.Longitude;
                


                ShowMyLocationOnTheMap();

               

                if (MyReverseGeocodeQuery == null || !MyReverseGeocodeQuery.IsBusy)
                {
                    MyReverseGeocodeQuery = new ReverseGeocodeQuery();
                    MyReverseGeocodeQuery.GeoCoordinate = new GeoCoordinate(MyCoordinate.Latitude, MyCoordinate.Longitude);
                    MyReverseGeocodeQuery.QueryCompleted += ReverseGeocodeQuery_QueryCompleted;
                    MyReverseGeocodeQuery.QueryAsync();
                }
                
                //StatusTextBlock.Text = "Status = This is your position :)";
            }
            //If an error is catch 2 are the main causes: the first is that you forgot to includ ID_CAP_LOCATION in your app manifest. 
            //The second is that the user doesn't turned on the Location Services
            catch (Exception ex)
            {
                if ((uint)ex.HResult == 0x80004004)
                {
                    //StatusTextBlock.Text = "Status= Location  is disabled in phone settings.";
                }
                //else
                {
                    // something else happened during the acquisition of the location
                }
            }
        }

        private void pinTileButton_Tapped(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (pinTileButton.Content.ToString() == "Pin Quick Share to Live Tile")
            {
                string tileParameter = "Share Location";
                ShellTile tile = CheckIfTileExist(tileParameter);
                if (tile == null)
                {
                    
                    StandardTileData secondaryTile = new StandardTileData { Title = tileParameter, BackgroundImage = new Uri("/Assets/Tiles/SMshareIcon.png", UriKind.RelativeOrAbsolute) };
                    ShellTile.Create(new Uri("/MainPage.xaml?" + "Param=" + tileParameter, UriKind.Relative), secondaryTile);
                }
                else
                {
                    //ShareSMS();
                }
            }
            else
            {
                MessageBox.Show("Unpined Quick Share Tile");
                ShellTile x = CheckIfTileExist("Share Location");
                x.Delete();
                pinTileButton.Content = "Pin Quick Share to Live Tile";
            }
        }

        private ShellTile CheckIfTileExist(string tileUri)
        {
            ShellTile shellTile = ShellTile.ActiveTiles.FirstOrDefault(
                    tile => tile.NavigationUri.ToString().Contains(tileUri));
            return shellTile;
        }

        private void myMapControl_Loaded(object sender, RoutedEventArgs e)
        {
            Microsoft.Phone.Maps.MapsSettings.ApplicationContext.ApplicationId = PrivateKeys.mapApplicationID;
            Microsoft.Phone.Maps.MapsSettings.ApplicationContext.AuthenticationToken = PrivateKeys.mapAuthToken;
        }
    }
}