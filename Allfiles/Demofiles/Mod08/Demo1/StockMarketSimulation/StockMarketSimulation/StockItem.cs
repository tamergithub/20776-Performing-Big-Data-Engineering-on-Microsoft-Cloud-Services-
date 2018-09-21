using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace StockMarketSimulation
{
    public class StockItem
    {
        private string eventHubName;
        private int stockID;
        private string ticker;
        private int price;
        private int previousPrice;
        private DateTime previousQuoteTime;
        private bool trainingRun;
        private static Random rnd = new Random();

        public StockItem(string eventHubName, int stockID, string ticker, int initialPrice)
        {
            this.eventHubName = eventHubName;
            this.stockID = stockID;
            this.ticker = ticker;
            this.price = initialPrice;
            this.previousPrice = initialPrice;
            this.previousQuoteTime = DateTime.UtcNow.AddHours(16);
        }

        public async void SendStockAlerts()
        {
            var client = EventHubClient.Create(this.eventHubName);

            try
            {
                while (true)
                {
                    var alert = new StockPriceChangeEvent()
                    {
                        Ticker = this.ticker,
                        Price = getPrice(),
                        QuoteTime = getQuoteTime()
                    };

                    var serializedAlert = JsonConvert.SerializeObject(alert);
                    var data = new EventData(Encoding.UTF8.GetBytes(serializedAlert));
                    data.Properties.Add("Type", $"Telemetry_{DateTime.Now.ToLongTimeString()}");
                    Trace.TraceInformation($"Sending: {alert.ToString()}");
                    await client.SendAsync(data);

                    await Task.Delay(rnd.Next(10) * 500);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError($"Error sending message: {e.Message}");
            }
            client.CloseAsync().Wait();
        }

        private int getPrice()
        {
            this.price = this.previousPrice + rnd.Next(10) - 4;
            if (this.price <= 0)
            {
                this.price = this.price + 5;
            }
            this.previousPrice = this.price;

            return this.price;
        }

        private DateTime getQuoteTime()
        {
            int minutes = rnd.Next(5) + 1;
            DateTime newQuoteTime = this.previousQuoteTime.AddMinutes(minutes);
            if (newQuoteTime.Hour > 16)
            {
                newQuoteTime = newQuoteTime.AddHours(16);
                //throw new Exception("Done");
            }
            this.previousQuoteTime = newQuoteTime;
            return newQuoteTime;
        }
    }
}