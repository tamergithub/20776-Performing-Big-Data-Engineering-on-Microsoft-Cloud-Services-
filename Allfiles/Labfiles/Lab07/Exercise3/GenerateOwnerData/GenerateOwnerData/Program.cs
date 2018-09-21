using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateOwnerData
{
    class Program
    {
        private const string chars = "ABCDEFGHJKLMNPRSTVWXYZ";
        private const string digits = "123456789";
        private static string[] titles = { "Mr", "Mrs", "Miss", "Ms", "Lord", "Lady", "Rt. Hon.", "Major", "Sir", "Dame"};

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage GenerateOwnerData <filename>");
                return;
            }

            Random rnd = new Random();
            using (var writer = new StreamWriter(args[0]))
            {
                writer.WriteLine("Vehicle Registration,Title,Forename,Surname,Address Line 1,Address Line 2,Address Line 3,Address Line 4");
                writer.Flush();
                foreach (char a in chars)
                {
                    foreach (char b in chars)
                    {
                        foreach (char c in chars)
                        {
                            foreach (char d in digits)
                            {
                                foreach (char e in digits)
                                {
                                    foreach (char f in digits)
                                    {
                                        string vehicleReg = $"{a}{b}{c} {d}{e}{f}";
                                        string title = titles[rnd.Next(titles.Length)];
                                        string forename = GetRandomString();
                                        string surname = GetRandomString();
                                        string addressLine1 = GetRandomString();
                                        string addressLine2 = GetRandomString();
                                        string addressLine3 = GetRandomString();
                                        string addressLine4 = GetRandomString();

                                        string ownerRecord = $"{vehicleReg},{title},{forename},{surname},{addressLine1},{addressLine2},{addressLine3},{addressLine4}";
                                        writer.WriteLine(ownerRecord);
                                        writer.Flush();
                                    }
                                }
                            }
                        }
                    }
                }
            }

            Console.WriteLine($"File {args[0]} created");
        }

        private static string GetRandomString()
        {
            string randomString = Path.GetRandomFileName();
            randomString = randomString.Replace(".", "");
            return CultureInfo.CurrentUICulture.TextInfo.ToTitleCase(randomString);            
        }
    }
}
