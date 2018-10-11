using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.Azure;
using SpeedCameraUtils;
using SpeedCameraDevice;
using System.Threading.Tasks;

namespace SpeedCameraDriver
{
    class Program
    {
        private static CancellationTokenSource tokenSource = new CancellationTokenSource();

        static void Main(string[] args)
        {
            string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            string eventHubName = CloudConfigurationManager.GetSetting("EventHubName");
            string numCamerasString = CloudConfigurationManager.GetSetting("NumCameras");
            string numPartitionsString = CloudConfigurationManager.GetSetting("NumPartitions");
            string latNorthString = CloudConfigurationManager.GetSetting("LatitudeNorth");
            string latSouthString = CloudConfigurationManager.GetSetting("LatitudeSouth");
            string longEastString = CloudConfigurationManager.GetSetting("LongitudeEast");
            string longWestString = CloudConfigurationManager.GetSetting("LongitudeWest");
            if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(eventHubName) ||
                string.IsNullOrEmpty(numCamerasString) || string.IsNullOrEmpty(numPartitionsString) ||
                string.IsNullOrEmpty(latNorthString) || string.IsNullOrEmpty(latSouthString) ||
                string.IsNullOrEmpty(longEastString) || string.IsNullOrEmpty(longWestString))
            {
                Trace.WriteLine("One or more parameters missing in appsettings (app.config)");
                return;
            }

            // Change Culture settings to en-US to ensure formatting of CSV data is correct
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = new System.Globalization.CultureInfo("en-US");

            ServiceBusConnectionStringBuilder builder = new ServiceBusConnectionStringBuilder(connectionString);
            builder.TransportType = TransportType.Amqp;
            var namespaceManager = NamespaceManager.CreateFromConnectionString(builder.ToString());
            EventHubManager.CreateEventHubIfNotExists(eventHubName, int.Parse(numPartitionsString), namespaceManager);

            var driver = new CamerasDriver(eventHubName, int.Parse(numCamerasString), int.Parse(numPartitionsString),
                double.Parse(latNorthString), double.Parse(latSouthString), double.Parse(longEastString), double.Parse(longWestString));
            var token = tokenSource.Token;
            Task.Factory.StartNew(() => driver.Run(token));
            Task.Factory.StartNew(() => driver.WaitForEnter(tokenSource));
            driver.runCompleteEvent.WaitOne();
        }
    }
}
