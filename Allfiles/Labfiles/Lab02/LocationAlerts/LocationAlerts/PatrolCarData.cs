using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
