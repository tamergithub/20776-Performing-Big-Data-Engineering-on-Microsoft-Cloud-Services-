using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace SpeedCameraUtils
{
    [DataContract]
    public class SpeedCameraAlert
    {
        [DataMember]
        public int DeviceID { get; set; }

        [DataMember]
        public string CameraID { get; set; }

        [DataMember]
        public string VehicleRegistration { get; set; }

        [DataMember]
        public int Speed { get; set; }

        [DataMember]
        public int SpeedLimit { get; set; }

        [DataMember]
        public DateTime Time { get; set; }

        public override string ToString()
        {
            return $"DeviceID: {DeviceID}, CameraID: {CameraID} Time: {Time} Vehicle: {VehicleRegistration} Speed: {Speed} Speed Limit: {SpeedLimit}";
        }
    }
}
