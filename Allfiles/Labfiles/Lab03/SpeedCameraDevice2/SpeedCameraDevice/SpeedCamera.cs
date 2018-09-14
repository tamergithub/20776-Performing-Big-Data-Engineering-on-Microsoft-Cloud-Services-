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
        private DateTime trafficJamStart;
        private int trafficJamDuration;
        private bool causeTrafficJams;
        private bool jammed;

        private const string chars = "ABCDEFGHJKLMNPRSTVWXYZ";
        private const string digits = "123456789";
        private int [] speedLimits = { 20, 30, 40, 50, 60, 70 };
        private static Random rnd = new Random();

        public SpeedCamera(string eventHubName, int deviceID, string cameraName, double positionLat, double positionLong, bool isTrainingRun)
        {
            this.eventHubName = eventHubName;
            this.deviceID = deviceID;
            this.cameraName = cameraName;
            this.positionLat = positionLat;
            this.positionLong = positionLong;
            this.trafficHotspotFactor = rnd.Next(4) + 1;
            this.speedLimitForLocation = speedLimits[deviceID % speedLimits.Length];
            this.causeTrafficJams = !isTrainingRun;
            this.trafficJamDuration = 0;
            this.jammed = false;
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

                    await Task.Delay(rnd.Next(100) * 500);
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
            // If causeTrafficJams is true, throw in the occasional traffic jam (1 time in 50), for good measure
            int rndSpeed = 0;
            if (!this.jammed && this.causeTrafficJams && (rnd.Next(50) == 1))
            {
                this.jammed = true;
                this.trafficJamStart = DateTime.UtcNow;
                this.trafficJamDuration = rnd.Next(5) + 2; // duration will be between 2 and 7 mins
            }
            
            if (this.jammed)
            {
                Trace.TraceInformation($"Traffic jammed at camera: {this.cameraName}");
                rndSpeed = 0;
                if (DateTime.UtcNow >= this.trafficJamStart.AddMinutes(this.trafficJamDuration))
                {
                    this.jammed = false; // The traffic jam has now passed
                    Trace.TraceInformation($"Traffic jam lifted at camera: {this.cameraName}");
                }
            }
            else
            {
                rndSpeed = (rnd.Next(120) + 10) / trafficHotspotFactor;
            }
            return rndSpeed;
        }

        private string getRegistration()
        {
            char letter1 = chars[rnd.Next(chars.Length)];
            //char letter2 = chars[rnd.Next(chars.Length)];
            char digit1 = digits[rnd.Next(digits.Length)];
            char digit2 = digits[rnd.Next(digits.Length)];
            return $"{letter1}{digit1}{digit2}";
        }
    }
}
