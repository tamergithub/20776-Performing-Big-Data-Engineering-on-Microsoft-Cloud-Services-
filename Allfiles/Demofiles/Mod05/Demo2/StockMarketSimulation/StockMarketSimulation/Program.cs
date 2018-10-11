using System;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System.Diagnostics;
using Microsoft.Azure;
using System.Threading;
using System.Threading.Tasks;

namespace StockMarketSimulation
{
   
    class Program
    {
        private static CancellationTokenSource tokenSource = new CancellationTokenSource();

        static void Main(string[] args)
        {
            string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            string eventHubName = CloudConfigurationManager.GetSetting("EventHubName");
            if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(eventHubName))
            {
                Trace.WriteLine("One or more parameters missing in appsettings (app.config)");
                return;
            }

            // Change Culture settings to en-US to ensure formmatting of CSV data is correct
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = new System.Globalization.CultureInfo("en-US");

            ServiceBusConnectionStringBuilder builder = new ServiceBusConnectionStringBuilder(connectionString);
            builder.TransportType = TransportType.Amqp;
            var namespaceManager = NamespaceManager.CreateFromConnectionString(builder.ToString());
            int numPartitions = 16;
            EventHubManager.CreateEventHubIfNotExists(eventHubName, numPartitions, namespaceManager);

            var driver = new StockMarketDriver(eventHubName, numPartitions);
            var token = tokenSource.Token;
            Task.Factory.StartNew(() => driver.Run(token));
            Task.Factory.StartNew(() => driver.WaitForEnter(tokenSource));
            driver.runCompleteEvent.WaitOne();
        }
    }
}
