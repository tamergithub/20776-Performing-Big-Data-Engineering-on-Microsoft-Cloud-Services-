using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SpeedCameraDevice
{
    public class CamerasDriver
    {
        public ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        private string eventHubName;
        private int numCameras;
        private int numPartitions;
        private double latNorth;
        private double latSouth;
        private double longEast;
        private double longWest;

        public CamerasDriver(string eventHubName, int numCameras, int numPartitions, double latNorth, double latSouth, double longEast, double longWest)
        {
            this.eventHubName = eventHubName;
            this.numCameras = numCameras;
            this.numPartitions = numPartitions;
            this.latNorth = latNorth;
            this.latSouth = latSouth;
            this.longEast = longEast;
            this.longWest = longWest;
        }

        public void Run(CancellationToken token)
        {
            var rnd = new Random();

            for (int cameraNum = 0; cameraNum < this.numCameras; cameraNum++)
            {
                double latRange = this.latNorth - this.latSouth;
                double cameraLatPosition = rnd.NextDouble() * latRange + this.latSouth;
                double longRange = this.longEast - this.longWest;
                double cameraLongPosition = rnd.NextDouble() * longRange + this.longWest;
                string cameraName = $"Camera {cameraNum}";

                var camera = new SpeedCamera(this.eventHubName, cameraNum, cameraName, cameraLatPosition, cameraLongPosition);
                Task.Factory.StartNew(() => camera.SendVehicleAlerts());
            }

            while (!token.IsCancellationRequested)
            {
                // Run until the user stops the cameras by pressing Enter
            }

            this.runCompleteEvent.Set();
        }

        public void WaitForEnter(CancellationTokenSource tokenSource)
        {
            Console.WriteLine("Press Enter to stop speed cameras");
            Console.ReadLine();
            tokenSource.Cancel();
        }
    }
}
