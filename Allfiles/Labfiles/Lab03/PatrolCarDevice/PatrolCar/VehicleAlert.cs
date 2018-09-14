using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PatrolCar
{
    [DataContract]
    class VehicleAlert
    {
        [DataMember]
        public string CarID { get; set; }

        [DataMember]
        public string Vehicle { get; set; }

        [DataMember]
        public double LocationLatitude { get; set; }

        [DataMember]
        public double LocationLongitude { get; set; }

        public override string ToString()
        {
            return $"Alert for patrol car: {CarID}. Dispatched to intercept vehicle {Vehicle} at position {LocationLatitude}, {LocationLongitude}";
        }
    }
}
