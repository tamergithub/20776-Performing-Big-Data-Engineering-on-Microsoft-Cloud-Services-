using Microsoft.Analytics.Interfaces;
using System;
using System.Collections.Generic;

namespace CustomOperators
{
    [SqlUserDefinedApplier]
    public class GetStockAnalytics : IApplier
    {
        // Generate a row that contains the opening price and % change in price for a stock item, identified by the ticker in the input row
        public override IEnumerable<IRow> Apply(IRow input, IUpdatableRow output)
        {
            // Retrieve the ticker, price, and quote time from the input row
            string ticker = input.Get<string>("Ticker");
            int price = input.Get<int>("Price");
            DateTime quoteTime = input.Get<DateTime>("QuoteTime");

            // Find the opening price for this stock item
            int openingPrice = getOpeningPrice(ticker, price, quoteTime);

            // Calculate the % change
            int priceDiff = price - openingPrice;
            double percentChange = ((double)priceDiff / openingPrice) * 100;

            // Generate and return the new row
            output.Set<string>("Ticker", ticker);
            output.Set<int>("OpeningPrice", openingPrice);
            output.Set<double>("PercentChange", percentChange);
            yield return output.AsReadOnly();
        }

        #region Supporting code
        private static List<PreviousStockPriceInfo> stockRecords = new List<PreviousStockPriceInfo>();

        // Find the opening price of this stock for the current period
        private static int getOpeningPrice(string ticker, int price, DateTime quoteTime)
        {
            // Check whether there is an existing record for this ticker
            PreviousStockPriceInfo firstQuote;

            // If there are no prices listed for the current ticker then this must be the first (opening) price for the period, so cache it
            if (!findStock(ticker, out firstQuote))
            {
                stockRecords.Add(new PreviousStockPriceInfo(ticker, price, quoteTime));
                return price;
            }
            else
            {
                return firstQuote.LastPrice;
            }
        }

        // Find the last movment for the specified stock and populate lastQuote with the data.
        // Return true if a previous quote was found, false otherwise
        private static bool findStock(string ticker, out PreviousStockPriceInfo lastQuote)
        {
            // Retrieve the previous quote for this time from the stockRecords list
            lastQuote = stockRecords.Find(item => string.Compare(ticker, item.Ticker) == 0);

            // Return a boolean that indicates whether a record was found:
            // If no such item was found, the fields in the lastQuote structure will have their default values, and the Ticker field will be null
            return (lastQuote.Ticker != null);
        }
        #endregion
    }
}