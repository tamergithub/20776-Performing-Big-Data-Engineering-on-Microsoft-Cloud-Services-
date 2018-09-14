using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.Azure;
using System;
using System.Diagnostics;

namespace SpeedCameraUtils
{
    public class EventHubManager
    {
        
        public static void CreateEventHubIfNotExists(string eventHubName, int numberOfPartitions, NamespaceManager manager)
        {
            try
            {
                // Create the Event Hub
                Trace.WriteLine("Creating Event Hub");
                EventHubDescription ehd = new EventHubDescription(eventHubName);
                ehd.PartitionCount = numberOfPartitions;
                manager.CreateEventHubIfNotExistsAsync(ehd).Wait();
            }
            catch (AggregateException agexp)
            {
                Trace.WriteLine(agexp.Flatten());
            }
        }
    }
}
