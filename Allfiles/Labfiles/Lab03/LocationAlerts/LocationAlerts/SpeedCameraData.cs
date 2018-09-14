using System.Runtime.Serialization;

namespace LocationAlerts
{
    [DataContract]
    class SpeedCameraData
    {
        [DataMember]
        public string CameraID { get; set; }

        [DataMember]
        public double LocationLatitude { get; set; }

        [DataMember]
        public double LocationLongitude { get; set; }

        [DataMember]
        public double SpeedLimit { get; set; }

        [DataMember]
        public string VehicleRegistration { get; set; }

        [DataMember]
        public double Speed { get; set; }

        public override string ToString()
        {
            return $"Camera: {CameraID}, Position: {LocationLatitude}, {LocationLongitude}, Speed Limit: {SpeedLimit}, Vehicle: {VehicleRegistration}, Speed: {Speed}";
        }
    }
}
