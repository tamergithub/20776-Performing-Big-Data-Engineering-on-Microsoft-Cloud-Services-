using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Types.Sql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PriceMovementAnalysis
{
    public class PriceAnalyzer
    {
        private static List<PreviousStockPriceInfo> stockRecords = new List<PreviousStockPriceInfo>();

        // A stock price movement is regarded as suspicious if:
        //    The price moves by more than 10% compared to its previous price and
        //    The previous price and the new price is also greater than 30 (stocks with low prices might easily move by more than 10%) and
        //    The time since the last movement is less than 5 minutes

        private const decimal MOVEMENT_PERCENT = 0.1M;
        private const int LOW_PRICE = 30;
        private const int INTERVAL_MINS = 5;

        public static bool SuspiciousMovement(string ticker, int newPrice, DateTime quoteTime)
        {
            PreviousStockPriceInfo lastQuote;

            // Find the details of the previous quote for this stock (if any)
            if (!findStock(ticker, out lastQuote))
            {
                // If the stock has not already been "seen" before then we cannot compare the new quote against the last quote
                // so just add the stock and return false to indicate that this activity is not suspicious
                stockRecords.Add(new PreviousStockPriceInfo(ticker, newPrice, quoteTime));
                return false;
            }
            else
            {
                bool isSuspicious = false;
                
                // Scrutinize the data for the current quote and compare it against the values for the previous quote
                if (lastQuote.LastPrice > LOW_PRICE && newPrice > LOW_PRICE)
                {
                    if (Math.Abs(lastQuote.LastPrice - newPrice) / (decimal)lastQuote.LastPrice > MOVEMENT_PERCENT)
                    {
                        if (quoteTime.Minute - lastQuote.LastQuoteTime.Minute < INTERVAL_MINS)
                        {
                            isSuspicious = true;
                        }
                    }
                }

                // Store the details for the new quote and overwrite the details of the previous quote
                int location = stockRecords.IndexOf(lastQuote);
                stockRecords[location] = new PreviousStockPriceInfo(ticker, newPrice, quoteTime);

                return isSuspicious;
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
    }

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
