using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PatrolCar
{
    class PatrolCarsDriver
    {
        public ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        private string connectionString;
        private string iotHubUri;
        private int numCars;
        private double latNorth;
        private double latSouth;
        private double longEast;
        private double longWest;
        private double milesPerDegreeLat;
        private double milesPerDegreeLong;
        private int locationReportingInterval;

        public PatrolCarsDriver(string connectionString, string iotHubUri, int numCars, double latNorth, double latSouth, 
            double longEast, double longWest, double milesPerDegreeLat, double milesPerDegreeLong, int locationReportingInterval)
        {
            this.connectionString = connectionString;
            this.iotHubUri = iotHubUri;
            this.numCars = numCars;
            this.latNorth = latNorth;
            this.latSouth = latSouth;
            this.longEast = longEast;
            this.longWest = longWest;
            this.milesPerDegreeLat = milesPerDegreeLat;
            this.milesPerDegreeLong = milesPerDegreeLong;
            this.locationReportingInterval = locationReportingInterval;
        }

        public void Run(CancellationToken token)
        {
            var rnd = new Random();

            for (int carNum = 0; carNum < this.numCars; carNum++)
            {
                double latRange = this.latNorth - this.latSouth;
                double carLatPosition = rnd.NextDouble() * latRange + this.latSouth;
                double longRange = this.longEast - this.longWest;
                double carLongPosition = rnd.NextDouble() * longRange + this.longWest;
                string carName = $"PatrolCar{carNum}";

                var car = new PatrolCar(this.connectionString, this.iotHubUri,carNum, carName, carLatPosition, carLongPosition, 
                    this.milesPerDegreeLat, this.milesPerDegreeLong, this.locationReportingInterval);
                Task.Factory.StartNew(() => car.SendPatrolCarAlerts());
            }

            while (!token.IsCancellationRequested)
            {
                // Run until the user stops the cameras by pressing Enter
            }

            this.runCompleteEvent.Set();
        }

        public void WaitForEnter(CancellationTokenSource tokenSource)
        {
            Console.WriteLine("Press Enter to stop patrol cars");
            Console.ReadLine();
            tokenSource.Cancel();
        }
    }
}
