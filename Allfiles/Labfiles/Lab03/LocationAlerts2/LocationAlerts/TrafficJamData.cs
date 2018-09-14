using System.Runtime.Serialization;

namespace LocationAlerts
{
    [DataContract]
    class TrafficJamData
    {
        [DataMember]
        public string CameraID { get; set; }

        [DataMember]
        public double AvgSpeed { get; set; }

        [DataMember]
        public int HourOfDay { get; set; }

        public override string ToString()
        {
            return $"Camera: {CameraID}, reporting average speed of: {AvgSpeed} mph";
        }
    }
}
