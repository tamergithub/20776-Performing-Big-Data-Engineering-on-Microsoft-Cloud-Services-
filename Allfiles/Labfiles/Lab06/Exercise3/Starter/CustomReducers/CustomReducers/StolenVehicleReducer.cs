using Microsoft.Analytics.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomReducers
{
    // Eliminate records for vehicles that have been recovered
    // Note that the recoverty history could be incomplete: a vehicle could be marked as stolen and not recovered, but later marked as stolen and recovered, in which case the vehicle is no longer considered stolen 
    [SqlUserDefinedReducer(IsRecursive = true)]
    public class ReduceByRecoveredVehicles : IReducer
    {
        public override IEnumerable<IRow> Reduce(IRowset input, IUpdatableRow output)
        {
            // Cache the rows in the input rowset (should be records for a single vehicle registration)
            // Only save rows where the vehicle is not marked as having been recovered
            var stolenVehicleRecords = (from row in input.Rows
                                        select new StolenVehicleRecord(
                                            row.Get<string>("VehicleRegistration"),
                                            row.Get<string>("DateStolen"),
                                            row.Get<string>("DateRecovered")
                                        )).ToList();

            // If there aren't any items in the stolenVehicleRecords list, then this vehicle is not stolen so skip over it
            if (stolenVehicleRecords.Count > 0)
            {
                // Sort the data in the stolenVehicleRecords list by DateStolen in descending order, so that the most recent record occurs first
                stolenVehicleRecords.Sort();

                // Retrieve the first record in the stolenVehicleRecords list - this is the most recent record of the vehicle having been stolen
                var stolenVehicleRecord = stolenVehicleRecords.First();

                // If the record does not have a recovery date, then output it, otherwise the vehicle is considered to have been recovered and is no longer stolen
                if (stolenVehicleRecord.DateRecovered == null)
                {
                    output.Set<string>("VehicleRegistration", stolenVehicleRecord.VehicleRegistration);
                    output.Set<DateTime>("DateStolen", stolenVehicleRecord.DateStolen);
                    yield return output.AsReadOnly();
                }
            }
        }
    }

    struct StolenVehicleRecord : IComparable<StolenVehicleRecord>
    {
        public string VehicleRegistration;
        public DateTime DateStolen;
        public DateTime? DateRecovered;

        public StolenVehicleRecord(string VehicleRegistration, string DateStolen, string DateRecovered)
        {
            this.VehicleRegistration = VehicleRegistration;
            this.DateStolen = Convert.ToDateTime(DateStolen);
            if (!String.IsNullOrEmpty(DateRecovered))
            {
                this.DateRecovered = Convert.ToDateTime(DateRecovered);
            }
            else
            {
                this.DateRecovered = null;
            }
        }

        // Comparer that reverse sorts StolenVehicleRecords (an earlier DateStolen is considered greater than a later DateStolen)
        public int CompareTo(StolenVehicleRecord other)
        {
            return DateTime.Compare(other.DateStolen, this.DateStolen);
        }
    }
}