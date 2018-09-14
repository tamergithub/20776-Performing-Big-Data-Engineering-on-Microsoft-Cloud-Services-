using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace PatrolCar
{
    [DataContract]
    class PatrolCarLocationAlert
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

        [DataMember]
        public DateTime Time { get; set; }

        public override string ToString()
        {
            return $"Time: {Time} Car: {CarID}, CarNum: {CarNum} Position: {LocationLatitude}, {LocationLongitude} Speed: {Speed}";
        }
    }
}
