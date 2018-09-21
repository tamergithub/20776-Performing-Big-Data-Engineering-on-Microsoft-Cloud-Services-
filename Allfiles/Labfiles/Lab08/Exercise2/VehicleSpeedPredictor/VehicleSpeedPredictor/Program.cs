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
                Console.Write("Enter a camera ID (between 0 and 499): ");
                string cameraID = Console.ReadLine();

                Console.Write("Enter an hour (between 0 and 23): ");
                string hour = Console.ReadLine();

                Console.Write("Enter an day (between 1 and 7): ");
                string day = Console.ReadLine();

                var scoreRequest = new
                {
                    Inputs = new Dictionary<string, List<Dictionary<string, string>>>() {
                        {
                            "input1",
                            new List<Dictionary<string, string>>(){new Dictionary<string, string>(){
                                            {
                                                "CameraID", $"Camera {cameraID}"
                                            },
                                            {
                                                "Speed", "1"
                                            },
                                            {
                                                "Hour", hour
                                            },
                                            {
                                                "Day", day
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
                    var predictedSpeed = token.SelectToken("Results.output1[0]")["Scored Label Mean"];
                    Console.WriteLine("Predicted speed: {0}", predictedSpeed);
                }
                else
                {
                    Console.WriteLine(string.Format("The request failed with status code: {0}", response.StatusCode));

                    // Print the headers - they include the requert ID and the timestamp,
                    // which are useful for debugging the failure
                    Console.WriteLine(response.Headers.ToString());

                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseContent);
                }
            }
        }
    }
}

