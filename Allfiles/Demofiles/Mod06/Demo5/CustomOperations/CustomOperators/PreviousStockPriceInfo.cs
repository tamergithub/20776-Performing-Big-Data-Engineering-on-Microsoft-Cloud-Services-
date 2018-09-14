using System;

namespace CustomOperators
{
    struct PreviousStockPriceInfo
    {
        public string Ticker;
        public int LastPrice;
        public DateTime LastQuoteTime;

        public PreviousStockPriceInfo(string ticker, int price, DateTime quoteTime)
        {
            this.Ticker = ticker;
            this.LastPrice = price;
            this.LastQuoteTime = quoteTime;
        }
    }
}
