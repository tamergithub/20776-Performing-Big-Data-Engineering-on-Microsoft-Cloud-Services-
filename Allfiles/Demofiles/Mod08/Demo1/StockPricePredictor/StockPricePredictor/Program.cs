using Microsoft.Azure;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace CallRequestResponseService
{
    class Program
    {
        static void Main(string[] args)
        {
            string apiKey = CloudConfigurationManager.GetSetting("APIKey");
            string serviceUrl = CloudConfigurationManager.GetSetting("URL");
            InvokeRequestResponseService(apiKey, serviceUrl).Wait();
        }

        static async Task InvokeRequestResponseService(string apiKey, string serviceUrl)
        {
            using (var client = new HttpClient())
            {
                Console.Write("Enter a ticker: ");
                string ticker = Console.ReadLine();

                Console.Write("Enter a date/time (yyyy-mm-ddThh24:mi:ss): ");
                string quoteTime = Console.ReadLine();

                var scoreRequest = new
                {
                    Inputs = new Dictionary<string, List<Dictionary<string, string>>>() {
                        {
                            "input1",
                            new List<Dictionary<string, string>>(){new Dictionary<string, string>(){
                                            {
                                                "Ticker", ticker
                                            },
                                            {
                                                "Price", "1"
                                            },
                                            {
                                                "QuoteTime", quoteTime
                                            },
                                }
                            }
                        },
                    },
                    GlobalParameters = new Dictionary<string, string>()
                    {
                    }
                };

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                client.BaseAddress = new Uri(serviceUrl);

                HttpResponseMessage response = await client.PostAsJsonAsync("", scoreRequest);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    JToken token = JObject.Parse(result);
                    var predictedPrice = token.SelectToken("Results.output1[0]")["Scored Labels"];
                    Console.WriteLine("Predicted price: {0}", predictedPrice);
                }
                else
                {
                    Console.WriteLine(string.Format("The request failed with status code: {0}", response.StatusCode));

                    // Print the headers - they include the request ID and the timestamp,
                    // which are useful for debugging the failure
                    Console.WriteLine(response.Headers.ToString());

                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseContent);
                }
            }
        }
    }
}
