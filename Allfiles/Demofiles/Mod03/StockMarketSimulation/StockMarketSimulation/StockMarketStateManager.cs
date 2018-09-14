using System;
using System.Collections.Generic;
using System.IO;

namespace StockMarketSimulation
{
    public class StockMarketStateManager
    {
        private const string DATAFILE = @"E:\Demofiles\Mod03\StockData.txt";
        private static SortedList<int, StockPrice> stockData = null;

        public static void SaveStockMarketData(int stockID, string ticker, int initialPrice)
        {
            using (StreamWriter writer = File.AppendText(DATAFILE))
            {
                // Data is CSV with a ;' character at the end of each line (makes parsing easier)
                writer.WriteLine($"{stockID},{ticker},{initialPrice};");
            }
        }

        public static void GetStockMarketData(int stockID, out string ticker, out int initialPrice)
        {
            if (stockData == null)
            {
                string allData = string.Empty;

                // Read and cache the data from the file
                using (StreamReader reader = new StreamReader(DATAFILE))
                {
                    allData = reader.ReadToEnd();
                }

                // Break the data down into its individual lines (lines are terminated with a ';' character)
                var lines = allData.Split(';');

                // Process each line and add it to the stockData list
                stockData = new SortedList<int, StockPrice>();
                foreach (string line in lines)
                {
                    var fields = line.Split(',');
                    if (fields.Length == 3)
                    {
                        stockData.Add(Convert.ToInt16(fields[0]), new StockPrice(fields[1], Convert.ToInt32(fields[2])));
                    }
                }
            }

            // Look up the ticker and intial price of the specified stock item and return them
            StockPrice stockPrice = stockData[stockID];
            ticker = stockPrice.Ticker;
            initialPrice = stockPrice.Price;
        }
    }
    public struct StockPrice
    {
        public string Ticker { get; set; }
        public int Price { get; set; }

        public StockPrice(string ticker, int price)
        {
            this.Ticker = ticker;
            this.Price = price;
        }
    }
}