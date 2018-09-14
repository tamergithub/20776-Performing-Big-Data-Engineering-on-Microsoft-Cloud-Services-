using System;
using System.Text;
using System.Diagnostics;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System.Threading.Tasks;
using SpeedCameraUtils;

namespace SpeedCameraDevice
{
    class SpeedCamera
    {
        private string eventHubName;
        private int deviceID;
        private string cameraName;
        private double positionLong;
        private double positionLat;
        private int trafficHotspotFactor;
        private int speedLimitForLocation;

        private const string chars = "ABCDEFGHJKLMNPRSTVWXYZ";
        private const string digits = "123456789";
        private int [] speedLimits = { 20, 30, 40, 50, 60, 70 };
        private static Random rnd = new Random();

        public SpeedCamera(string eventHubName, int deviceID, string cameraName, double positionLat, double positionLong)
        {
            this.eventHubName = eventHubName;
            this.deviceID = deviceID;
            this.cameraName = cameraName;
            this.positionLat = positionLat;
            this.positionLong = positionLong;
            this.trafficHotspotFactor = rnd.Next(4) + 1;
            this.speedLimitForLocation = speedLimits[deviceID % speedLimits.Length];
        }

        public async void SendVehicleAlerts()
        {
            var client = EventHubClient.Create(this.eventHubName);

            try
            {
                while (true)
                {
                    var alert = new SpeedCameraAlert()
                    {
                        DeviceID = this.deviceID,
                        CameraID = this.cameraName,
                        Time = DateTime.UtcNow,
                        LocationLatitude = this.positionLat,
                        LocationLongitude = this.positionLong,
                        SpeedLimit = this.speedLimitForLocation,
                        VehicleRegistration = getRegistration(),
                        Speed = getSpeed()                        
                    };

                    var serializedAlert = JsonConvert.SerializeObject(alert);
                    var data = new EventData(Encoding.UTF8.GetBytes(serializedAlert));
                    data.Properties.Add("Type", $"Telemetry_{DateTime.Now.ToLongTimeString()}");
                    Trace.TraceInformation($"Sending: {alert.ToString()}");
                    await client.SendAsync(data);

                    await Task.Delay(rnd.Next(10) * 500);
                }
            }
            catch(Exception e)
            {
                Trace.TraceError($"Error sending message: {e.Message}");
            }
            client.CloseAsync().Wait();
        }

        private int getSpeed()
        {
            int rndSpeed = rnd.Next(120) / trafficHotspotFactor;
            return rndSpeed;
        }

        private string getRegistration()
        {
            char letter = chars[rnd.Next(chars.Length)];
            char digit = digits[rnd.Next(digits.Length)];
            return $"{letter}{digit}";
        }
    }
}
