using System;
using System.Runtime.Serialization;

namespace SpeedCameraUtils
{
    [DataContract]
    public class VehicleAlert
    {
        [DataMember]
        public int DeviceID { get; set; }

        [DataMember]
        public string CameraID { get; set; }

        [DataMember]
        public string VehicleType { get; set; }

        [DataMember]
        public DateTime Time { get; set; }

        public override string ToString()
        {
            return $"DeviceID: {DeviceID}, CameraID: {CameraID} Time: {Time} Vehicle Type: {VehicleType}";
        }
    }
}
