using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace LocationAlerts
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private double LatitudeNorth = 0, LatitudeSouth = 0, LongitudeEast = 0, LongitudeWest = 0;
        private string servicebusEndpoint = string.Empty, queueName = string.Empty, sharedAccessKeyName = string.Empty, sharedAccessKey = string.Empty;
        private QueueClient queueClient = null;
        private RandomAccessStreamReference patrolCarImage = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:////assets/patrolcaricon.png"));

        public MainPage()
        {
            this.InitializeComponent();
        }

        async Task<IList<string>> getConfigSettings(string configFileFileName)
        {
            var packageFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            var configFile = await packageFolder.GetFileAsync(configFileFileName);
            return await Windows.Storage.FileIO.ReadLinesAsync(configFile);
        }

        private async void Page_Loading(FrameworkElement sender, object args)
        {
            var configSettings = await getConfigSettings("ConfigSettings.txt");
            foreach (string configItemLine in configSettings)
            {
                string[] configItem = configItemLine.Split('=');
                string itemName = configItem[0].Trim();
                string value = configItem[1].Trim();
                switch (itemName)
                {
                    case "Token":
                        MapControl.MapServiceToken = value;
                        break;
                    case "LatitudeNorth":
                        Double.TryParse(value, out LatitudeNorth);
                        break;
                    case "LatitudeSouth":
                        Double.TryParse(value, out LatitudeSouth);
                        break;
                    case "LongitudeEast":
                        Double.TryParse(value, out LongitudeEast);
                        break;
                    case "LongitudeWest":
                        Double.TryParse(value, out LongitudeWest);
                        break;
                    case "ServicebusEndpoint":
                        servicebusEndpoint = value;
                        break;
                    case "QueueName":
                        queueName = value;
                        break;
                    case "SASKeyName":
                        sharedAccessKeyName = value;
                        break;
                    case "SASKey":
                        sharedAccessKey = $"{value}="; // SAS tokens have an "=" at the end which will be stripped by the Split function earlier
                        break;
                    default: break;
                }
            }

            BasicGeoposition mapCenter = new BasicGeoposition();
            mapCenter.Latitude = (LatitudeNorth + LatitudeSouth) / 2;
            mapCenter.Longitude = (LongitudeEast + LongitudeWest) / 2;
            Geopoint center = new Geopoint(mapCenter);
            MapControl.Center = center;
            MapControl.ZoomLevel = 11;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            string serviceBusConnectionString = new ServiceBusConnectionStringBuilder(servicebusEndpoint, String.Empty, sharedAccessKeyName, sharedAccessKey).ToString();
            queueClient = new QueueClient(serviceBusConnectionString, queueName, ReceiveMode.PeekLock);

            // Handle messages received from the Service Bus queue
            queueClient.RegisterMessageHandler(
                async (message, token) =>
                {   
                    // Deserialize the data as patrol car data
                    string data = Encoding.UTF8.GetString(message.Body);
                    int pos = data.LastIndexOf('{');
                    data = data.Substring(pos);
                    PatrolCarData patrolCarData = JsonConvert.DeserializeObject<PatrolCarData>(data);

                    // Plot the position of the patrol car on the map
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        () =>
                        {
                            MapIcon patrolCarIcon = new MapIcon();
                            BasicGeoposition patrolCarPosition = new BasicGeoposition()
                            {
                                Latitude = patrolCarData.LocationLatitude,
                                Longitude = patrolCarData.LocationLongitude
                            };

                            // If the patrol car already exists then just update its location
                            MapIcon car = (MapIcon)MapControl.MapElements.FirstOrDefault(m => String.Compare((m as MapIcon).Title, patrolCarData.CarID) == 0);
                            if (car != null)
                            {
                                car.Location = new Geopoint(patrolCarPosition);
                            }
                            // Otherwise create a new icon and add it to the map
                            else
                            {
                                patrolCarIcon.Location = new Geopoint(patrolCarPosition);
                                patrolCarIcon.NormalizedAnchorPoint = new Point(0.5, 1.0);
                                patrolCarIcon.Title = patrolCarData.CarID;
                                patrolCarIcon.ZIndex = 0;
                                patrolCarIcon.Image = patrolCarImage;
                                MapControl.MapElements.Add(patrolCarIcon);
                            }
                        });
                    await queueClient.CompleteAsync(message.SystemProperties.LockToken);
                }, new MessageHandlerOptions(async (args) =>
                {
                    // Error handling - discard the message                    
                })
            );
        }

        private async void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            await queueClient.CloseAsync();
        }
    }
}
