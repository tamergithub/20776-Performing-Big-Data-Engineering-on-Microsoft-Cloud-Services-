using System.Runtime.Serialization;

namespace LocationAlerts
{
    [DataContract]
    class PatrolCarData
    {
        [DataMember]
        public string CarID { get; set; }

        [DataMember]
        public int CarNum { get; set; }

        [DataMember]
        public double LocationLatitude { get; set; }

        [DataMember]
        public double LocationLongitude { get; set; }

        [DataMember]
        public double Speed { get; set; }

        public override string ToString()
        {
            return $"Car: {CarID}, CarNum: {CarNum} Position: {LocationLatitude}, {LocationLongitude} Speed: {Speed}";
        }
    }
}
