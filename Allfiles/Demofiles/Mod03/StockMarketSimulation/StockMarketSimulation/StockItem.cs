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
        private bool trainingRun;
        private static Random rnd = new Random();

        public StockItem(string eventHubName, int stockID, string ticker, int initialPrice, bool trainingRun)
        {
            this.eventHubName = eventHubName;
            this.stockID = stockID;
            this.ticker = ticker;
            this.price = initialPrice;
            this.trainingRun = trainingRun;
            this.previousPrice = initialPrice;
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
                        QuoteTime = DateTime.UtcNow
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
            if (!this.trainingRun && rnd.Next(25) == 1)
            {
                // Throw in the occassional "anomaly" 
                this.price = this.price + 1000;
            }
            else
            {
                this.price = this.previousPrice + rnd.Next(10) - 5;
                if (this.price <= 0)
                {
                    this.price = 0;
                }
                this.previousPrice = this.price;
            }

            return this.price;
        }
    }
}