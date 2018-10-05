using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Azure;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Azure.Devices.Client;

namespace PatrolCar
{
    class PatrolCar
    {
        private DeviceClient deviceClient;
        private int carNum;
        private string carName;
        private double carLatPosition;
        private double carLongPosition;
        private int previousDirectionFactorLat;
        private int previousDirectionFactorLong;
        private double milesPerDegreeLat;
        private double milesPerDegreeLong;
        private int locationReportingInterval;
        private double speed;
        private static Random rnd = new Random();
        
        public PatrolCar(string connectionString, string iotHubUri, int carNum, string carName, double carLatPosition, 
            double carLongPosition, double milesPerDegreeLat, double milesPerDegreeLong, int locationReportingInterval)
        {
            RegistryManager registryManager = RegistryManager.CreateFromConnectionString(connectionString);
            this.deviceClient = this.GetOrCreateDevice(registryManager, iotHubUri, carName).Result;
            this.carNum = carNum;
            this.carName = carName;
            this.carLatPosition = carLatPosition;
            this.carLongPosition = carLongPosition;
            this.milesPerDegreeLat = milesPerDegreeLat;
            this.milesPerDegreeLong = milesPerDegreeLong;
            this.locationReportingInterval = locationReportingInterval;
            this.speed = 0;
            this.previousDirectionFactorLat = 0;
            this.previousDirectionFactorLong = 0;
        }

        private async Task<DeviceClient> GetOrCreateDevice(RegistryManager registryManager, string iotHubUri, string carName)
        {
            Device device;
            try
            {
                device = await registryManager.AddDeviceAsync(new Device(carName));
            }
            catch (DeviceAlreadyExistsException)
            {
                device = await registryManager.GetDeviceAsync(carName);
            }
            Trace.TraceInformation($"Generated device key: {device.Authentication.SymmetricKey.PrimaryKey} for {carName}"); 

            DeviceClient deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(carName, device.Authentication.SymmetricKey.PrimaryKey), Microsoft.Azure.Devices.Client.TransportType.Http1);
            Trace.TraceInformation($"Device client for {carName} created");

            return deviceClient;
        }
        public async void SendPatrolCarAlerts()
        {
            try
            {
                while (true)
                {
                    double distance = generateDistanceTravelled(this.locationReportingInterval);
                    GPSCoords newLocation = calculateCoords(this.carLatPosition, this.carLongPosition, distance, this.milesPerDegreeLat, this.milesPerDegreeLong);
                    this.carLatPosition = newLocation.Latitude;
                    this.carLongPosition = newLocation.Longitude;
                    var alert = new PatrolCarLocationAlert()
                    {
                        CarID = this.carName,
                        CarNum = this.carNum,
                        LocationLatitude = this.carLatPosition,
                        LocationLongitude = this.carLongPosition,
                        Speed = this.speed,
                        Time = DateTime.UtcNow
                    };
                    
                    var serializedAlert = JsonConvert.SerializeObject(alert);
                    var data = new Microsoft.Azure.Devices.Client.Message(Encoding.UTF8.GetBytes(serializedAlert));
                    data.Properties.Add("Type", $"Telemetry_{DateTime.Now.ToLongTimeString()}");
                    await this.deviceClient.SendEventAsync(data);
                    Trace.TraceInformation($"Sending: {alert.ToString()}");
                    await Task.Delay(this.locationReportingInterval * 1000);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError($"Error sending message: {e.Message}");
            }           
        }

        private GPSCoords calculateCoords(double carLatPosition, double carLongPosition, double distance, double milesPerDegreeLat, double milesPerDegreeLong)
        {
            int directionFactorLat = 0;
            int directionFactorLong = 0;

            if (this.speed > 60)
            {
                directionFactorLat = this.previousDirectionFactorLat;
                directionFactorLong = this.previousDirectionFactorLong;
            }
            else if (this.speed > 30)
            {
                directionFactorLat = this.previousDirectionFactorLat;
                directionFactorLong = rnd.Next(3) - 1;
                this.previousDirectionFactorLong = directionFactorLong;

            }
            else if (this.speed > 10)
            {
                directionFactorLong = this.previousDirectionFactorLong;
                directionFactorLat = rnd.Next(3) - 1;
                this.previousDirectionFactorLat = directionFactorLat;
            }
            else
            {
                directionFactorLat = rnd.Next(3) - 1;
                directionFactorLong = rnd.Next(3) - 1;
                this.previousDirectionFactorLat = directionFactorLat;
                this.previousDirectionFactorLong = directionFactorLong;
            }

            double latDistance = rnd.NextDouble() * distance * directionFactorLat;
            double longDistance = Math.Sqrt((distance * distance) - (latDistance * latDistance)) * directionFactorLong;
            GPSCoords newCoords = new GPSCoords
            {
                Latitude = carLatPosition + latDistance / milesPerDegreeLat,
                Longitude = carLongPosition + longDistance / milesPerDegreeLong
            };
            return newCoords;
        }

        private double generateDistanceTravelled(int intervalInSecs)
        {
            this.speed += rnd.Next(40) - 20;

            if (this.speed < 0)
                this.speed = 0;

            if (this.speed > 120)
                this.speed = 120;

            double milesPerSecond = this.speed / 3600;
            double distance = milesPerSecond * intervalInSecs;
            Trace.TraceInformation($"Distance travelled: {distance} miles");
            return distance;
        }
    }
}
