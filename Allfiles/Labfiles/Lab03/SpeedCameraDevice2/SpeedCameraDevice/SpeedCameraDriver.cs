using System;
using System.Threading;
using System.Threading.Tasks;
using SpeedCameraUtils;

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
        private bool isTrainingRun;

        public CamerasDriver(string eventHubName, int numCameras, int numPartitions, double latNorth, double latSouth, double longEast, double longWest, bool isTrainingRun)
        {
            this.eventHubName = eventHubName;
            this.numCameras = numCameras;
            this.numPartitions = numPartitions;
            this.latNorth = latNorth;
            this.latSouth = latSouth;
            this.longEast = longEast;
            this.longWest = longWest;
            this.isTrainingRun = isTrainingRun;
        }

        public void Run(CancellationToken token)
        {
            var rnd = new Random();

            for (int cameraNum = 0; cameraNum < this.numCameras; cameraNum++)
            {
                double cameraLatPosition = 0;
                double cameraLongPosition = 0;

                if (isTrainingRun)
                {
                    // Generate random positions for cameras, and then save them
                    double latRange = this.latNorth - this.latSouth;
                    cameraLatPosition = rnd.NextDouble() * latRange + this.latSouth;
                    double longRange = this.longEast - this.longWest;
                    cameraLongPosition = rnd.NextDouble() * longRange + this.longWest;
                    
                    CameraStateManager.SaveCameraData(cameraNum, cameraLatPosition, cameraLongPosition);
                }
                else
                {
                    // Use the existing camera positions (must reuse the same locations and names for the training data to be valid)
                    CameraStateManager.GetCameraData(cameraNum, out cameraLatPosition, out cameraLongPosition);
                }

                string cameraName = $"Camera {cameraNum}";
                var camera = new SpeedCamera(this.eventHubName, cameraNum, cameraName, cameraLatPosition, cameraLongPosition, isTrainingRun);
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
