using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateStolenVehicleData
{
    class Program
    {
        private const string chars = "ABCDEFGHJKLMNPRSTVWXYZ";
        private const string digits = "123456789";
        private static Random rnd = new Random();
        private static int startYear = 2010;
        private static int endYear = 2017;

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage GenerateStolenVehicleData <folder>");
                return;
            }


            for (int year = startYear; year <= endYear; year++)
            {
                for (int month = 1; month <= 12; month++)
                {
                    int endDay = 0;
                    if (month == 1 || month == 3 || month == 5 || month == 7 || month == 10 || month == 12)
                    {
                        endDay = 31;
                    }
                    else if (month != 2)
                    {
                        endDay = 30;
                    }
                    else if (year != 2012 && year != 2016)
                    {
                        endDay = 28;
                    }
                    else
                    {
                        endDay = 29;
                    }

                    for (int day = 1; day <= endDay; day++)
                    {
                        string dateStolen = $"{year}/{month}/{day}";
                        DateTime ds = new DateTime(year, month, day);

                        Directory.CreateDirectory($@"{args[0]}\{year}\{month}\{day}");
                        using (var writer = new StreamWriter($@"{args[0]}\{year}\{month}\{day}\VehicleData.csv"))
                        {
                            writer.WriteLine("Vehicle Registration,Date Stolen,Date Recovered");
                            writer.Flush();
                            {
                                int numThefts = 400 + rnd.Next(100);
                                for (int numStolenVehicles = 1; numStolenVehicles <= numThefts; numStolenVehicles++)
                                {
                                    char letter1 = chars[rnd.Next(chars.Length)];
                                    char letter2 = chars[rnd.Next(chars.Length)];
                                    char letter3 = chars[rnd.Next(chars.Length)];
                                    char digit1 = digits[rnd.Next(digits.Length)];
                                    char digit2 = digits[rnd.Next(digits.Length)];
                                    char digit3 = digits[rnd.Next(digits.Length)];
                                    string reg = $"{letter1}{letter2}{letter3} {digit1}{digit2}{digit3}";

                                    DateTime dr = ds.AddDays(rnd.Next(50));
                                    string dateRecovered = $"{dr.Year}/{dr.Month}/{dr.Day}";
                                    if (rnd.Next(10) < 2)
                                    {
                                        dateRecovered = string.Empty;
                                    }

                                    string stolenRecord = $"{reg},{dateStolen},{dateRecovered}";
                                    writer.WriteLine(stolenRecord);
                                    writer.Flush();
                                }
                            }
                        }
                    }
                }
            }

            Console.WriteLine($"Files in folder {args[0]} created");
        }
    }
}
