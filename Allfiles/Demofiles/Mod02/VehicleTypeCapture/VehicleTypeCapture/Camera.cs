using System;
using System.Text;
using System.Diagnostics;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System.Threading.Tasks;
using SpeedCameraUtils;

namespace VehicleTypeCapture
{
    class Camera
    {
        private string eventHubName;
        private int deviceID;
        private string cameraName;
        private static Random rnd = new Random();

        public Camera(string eventHubName, int deviceID, string cameraName)
        {
            this.eventHubName = eventHubName;
            this.deviceID = deviceID;
            this.cameraName = cameraName;
        }
        public async void SendVehicleAlerts()
        {
            var client = EventHubClient.Create(this.eventHubName);

            try
            {
                while (true)
                {
                    var alert = new VehicleAlert()
                    {
                        DeviceID = this.deviceID,
                        CameraID = this.cameraName,
                        Time = DateTime.UtcNow,
                        VehicleType = ((VehicleType)rnd.Next(4)).ToString()
                    };

                    var serializedAlert = JsonConvert.SerializeObject(alert);
                    var data = new EventData(Encoding.UTF8.GetBytes(serializedAlert));
                    data.Properties.Add("Type", $"Telemetry_{DateTime.Now.ToLongTimeString()}");
                    Trace.TraceInformation($"Sending: {alert.ToString()}");
                    await client.SendAsync(data);

                    await Task.Delay(rnd.Next(10) * 500);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError($"Error sending message: {e.Message}");
            }
            client.CloseAsync().Wait();
        }
    }
}
