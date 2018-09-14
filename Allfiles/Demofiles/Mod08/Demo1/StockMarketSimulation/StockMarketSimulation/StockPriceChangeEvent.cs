using System;
using System.Runtime.Serialization;

namespace StockMarketSimulation
{
    [DataContract]
    class StockPriceChangeEvent
    {
        [DataMember]
        public string Ticker { get; set; }

        [DataMember]
        public int Price { get; set; }

        [DataMember]
        public DateTime QuoteTime { get; set; }

        public override string ToString()
        {
            return $"Ticker: {Ticker}, Price: {Price} Time: {QuoteTime}";
        }
    }
}
