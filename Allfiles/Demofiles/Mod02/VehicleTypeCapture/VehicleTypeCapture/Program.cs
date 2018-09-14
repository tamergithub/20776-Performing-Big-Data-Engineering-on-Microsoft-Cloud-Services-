using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.Azure;
using SpeedCameraUtils;
using System.Threading.Tasks;

namespace VehicleTypeCapture
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
            if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(eventHubName) ||
                string.IsNullOrEmpty(numCamerasString) || string.IsNullOrEmpty(numPartitionsString))
            {
                Trace.WriteLine("One or more parameters missing in appsettings (app.config)");
                return;
            }

            ServiceBusConnectionStringBuilder builder = new ServiceBusConnectionStringBuilder(connectionString);
            builder.TransportType = TransportType.Amqp;
            var namespaceManager = NamespaceManager.CreateFromConnectionString(builder.ToString());
            EventHubManager.CreateEventHubIfNotExists(eventHubName, int.Parse(numPartitionsString), namespaceManager);

            var driver = new CamerasDriver(eventHubName, int.Parse(numCamerasString), int.Parse(numPartitionsString));
            var token = tokenSource.Token;
            Task.Factory.StartNew(() => driver.Run(token));
            Task.Factory.StartNew(() => driver.WaitForEnter(tokenSource));
            driver.runCompleteEvent.WaitOne();
        }
    }
}
