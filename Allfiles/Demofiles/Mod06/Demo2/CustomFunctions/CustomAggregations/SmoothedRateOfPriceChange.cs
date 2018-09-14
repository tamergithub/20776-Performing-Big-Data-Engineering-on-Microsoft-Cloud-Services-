using Microsoft.Analytics.Interfaces;
using System;

namespace CustomAggregations
{
    // Custom aggregation that works out the rate of change in price of a specified stock using agiven unit of time
    [SqlUserDefinedReducer(IsRecursive = false)]
    public class SmoothedRateOfPriceChange : IAggregate<int, DateTime, char, double>
    {
        private DateTime? startTime;
        private int initialPrice;
        private int priceChangeSinceStart;
        private TimeSpan? timeSinceStart;
        private char timeUnit; // 'D' for Day, 'H' for Hour, or 'M' for Minute

        public override void Init()
        {
            this.startTime = null;
            this.timeSinceStart = null;
            this.initialPrice = 0;
            this.priceChangeSinceStart = 0;
        }

        // Aggregate the price, based on the currently accumulated price and the change noted in the current row
        public override void Accumulate(int price, DateTime quoteTime, char timeUnit)
        {
            if (this.startTime == null)
            {
                this.startTime = quoteTime;
                this.initialPrice = price;
                this.timeUnit = timeUnit;
            }
            else
            {
                this.priceChangeSinceStart = price - this.initialPrice;
                this.timeSinceStart = quoteTime - this.startTime.Value;
            }
        }

        // Calculate and return the rate of change in price according to the specified time unit
        public override double Terminate()
        {
            double rate = 0;
            switch (this.timeUnit)
            {
                case 'D':
                    rate = rate = (double)this.priceChangeSinceStart / this.timeSinceStart.Value.TotalDays;
                    break;
                case 'H':
                    rate = rate = (double)this.priceChangeSinceStart / this.timeSinceStart.Value.TotalHours;
                    break;
                case 'M':
                default:
                    rate = rate = (double)this.priceChangeSinceStart / this.timeSinceStart.Value.TotalMinutes;
                    break;
            }
            return rate;
        }
    }
}
