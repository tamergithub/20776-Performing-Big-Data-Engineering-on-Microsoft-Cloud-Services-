using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedCameraUtils
{
    public static class CameraStateManager
    {
        private const string DATAFILE = @"E:\Labfiles\Lab03\CameraData.csv";
        private static SortedList<int, GPSCoords> cameraData = null;

        public static void SaveCameraData(int cameraNum, double cameraLatPosition, double cameraLongPosition)
        {
            using (StreamWriter writer = File.AppendText(DATAFILE))
            {
                // Data is CSV with a ;' character at the end of each line (makes parsing easier)
                writer.WriteLine($"{cameraNum},{cameraLatPosition},{cameraLongPosition};");
            }
        }

        public static void GetCameraData(int cameraNum, out double cameraLatPosition, out double cameraLongPosition)
        {
            if (cameraData == null)
            {
                string allData = string.Empty;

                // Read and cache the data from the file
                using (StreamReader reader = new StreamReader(DATAFILE))
                {
                    allData = reader.ReadToEnd();
                }

                // Break the data down into its individual lines (lines are terminated with a ';' character)
                var lines = allData.Split(';');

                // Process each line and add it to the cameraData list
                cameraData = new SortedList<int, GPSCoords>();
                foreach (string line in lines)
                {
                    var fields = line.Split(',');
                    if (fields.Length == 3)
                    {
                        cameraData.Add(Convert.ToInt16(fields[0]), new GPSCoords(Convert.ToDouble(fields[1]), Convert.ToDouble(fields[2])));
                    }
                }
            }

            // Look up the coords of the specified speed camera and return them
            GPSCoords coords = cameraData[cameraNum];
            cameraLatPosition = coords.Latitude;
            cameraLongPosition = coords.Longitude;
        }
    }

    public struct GPSCoords
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public GPSCoords(double lat, double lng)
        {
            this.Latitude = lat;
            this.Longitude = lng;
        }
    }
}
