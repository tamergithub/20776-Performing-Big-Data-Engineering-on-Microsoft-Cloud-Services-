using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace VehicleTypeCapture
{
    public class CamerasDriver
    {
        public ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        private string eventHubName;
        private int numCameras;
        private int numPartitions;

        public CamerasDriver(string eventHubName, int numCameras, int numPartitions)
        {
            this.eventHubName = eventHubName;
            this.numCameras = numCameras;
            this.numPartitions = numPartitions;
        }

        public void Run(CancellationToken token)
        {
            var rnd = new Random();

            for (int cameraNum = 0; cameraNum < this.numCameras; cameraNum++)
            {
                string cameraName = $"Camera {cameraNum}";

                var camera = new Camera(this.eventHubName, cameraNum, cameraName);
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
