using Microsoft.Analytics.Interfaces;

namespace CustomAggregations
{
    // Calculate the average of values in a column, but discard negative and zero values from the calculations
    // PositiveAVG expects data values to be doubles, and returns a double
    // Note: This aggregation is NOT recursive as averages are not associative
    [SqlUserDefinedReducer(IsRecursive = false)]
    public class PositiveAVG : IAggregate<double, double>
    {
        private double totalOfPositiveValues;
        private int numOfPositiveValues;

        public override void Init()
        {
            this.totalOfPositiveValues = 0;
            this.numOfPositiveValues = 0;
        }

        public override void Accumulate(double dataValue)
        {
            // Only include positive values in the calculation
            if (dataValue > 0)
            {
                this.totalOfPositiveValues += dataValue;
                this.numOfPositiveValues++;
            }
        }

        public override double Terminate()
        {
            if (numOfPositiveValues > 0)
            {
                return this.totalOfPositiveValues / this.numOfPositiveValues;
            }
            else
            {
                return 0;
            }
        }
    }
}