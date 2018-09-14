using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace StockMarketSimulation
{
    public class StockMarketDriver
    {
        private string eventHubName;
        private int numPartitions;
        private bool trainingRun;
        public ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        public StockMarketDriver(string eventHubName, int numPartitions, bool trainingRun)
        {
            this.eventHubName = eventHubName;
            this.numPartitions = numPartitions;
            this.trainingRun = trainingRun;
        }

        public void Run(CancellationToken token)
        {
            var rnd = new Random();

            for (int stockID = 0; stockID < 100; stockID++)
            {
                string ticker = string.Empty;
                int initialPrice = 0;

                if (trainingRun)
                {
                    // Generate random tickers and intial prices for the stocks, and then save them
                    ticker = Path.GetRandomFileName().Substring(1, 4).ToUpper();
                    initialPrice = rnd.Next(100);
                    StockMarketStateManager.SaveStockMarketData(stockID, ticker, initialPrice);
                }
                else
                {
                    // Use the existing stocks (must reuse the same tickers and initial prices for the training data to be valid)
                    StockMarketStateManager.GetStockMarketData(stockID, out ticker, out initialPrice);
                }

                var stockItem = new StockItem(this.eventHubName, stockID, ticker, initialPrice, trainingRun);
                Task.Factory.StartNew(() => stockItem.SendStockAlerts());
            }

            while (!token.IsCancellationRequested)
            {
                // Run until the user stops the simulation by pressing Enter
            }

            this.runCompleteEvent.Set();
        }

        public void WaitForEnter(CancellationTokenSource tokenSource)
        {
            Console.WriteLine("Press Enter to stop the simulation");
            Console.ReadLine();
            tokenSource.Cancel();
        }
    }
}