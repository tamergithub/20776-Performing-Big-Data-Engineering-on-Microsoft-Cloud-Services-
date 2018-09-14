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
        public ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        public StockMarketDriver(string eventHubName, int numPartitions)
        {
            this.eventHubName = eventHubName;
            this.numPartitions = numPartitions;
        }

        public void Run(CancellationToken token)
        {
            var rnd = new Random();

            for (int stockID = 0; stockID < 100; stockID++)
            {
                string ticker = string.Empty;
                int initialPrice = 0;

                ticker = Path.GetRandomFileName().Substring(1, 4).ToUpper();
                initialPrice = rnd.Next(100);
 
                var stockItem = new StockItem(this.eventHubName, stockID, ticker, initialPrice);
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