using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateEmployeeData
{
    enum roles { associate, employee, teamleader, manager, vicepresident, president }

    class Program
    {
        private const string DATAFILE = "EmployeeWorkHistory.tsv";
        private static int[] depts = { 100, 101, 102, 103, 104, 105, 106, 107, 108, 109 };
        
        static void Main(string[] args)
        {
            Random rnd = new Random(); 
            string folderName = $@"D:\DemoFiles\Mod06\Demo 5";

            string fileName = $"{folderName}/{DATAFILE}";
            using (StreamWriter writer = File.AppendText(fileName))
            {
                writer.WriteLine("EmployeeID\tName\tDepartment\rRole History");
                for (int empid = 100000; empid < 400000; empid++)
                {
                    int deptidx = rnd.Next(10);
                    int dept = depts[deptidx];
                    int seniorityFlag = rnd.Next(20000);
                    int seniority = 2;
                    if (seniorityFlag > 18000) seniority = 3;
                    if (seniorityFlag > 19500) seniority = 4;
                    if (seniorityFlag > 19900) seniority = 5;
                    if (seniorityFlag > 19950) seniority = 6;

                    int roleHistory = rnd.Next(seniority);
                    string roleList = string.Empty;
                    for (roles numRoles = 0; (int)numRoles <= roleHistory; numRoles++)
                    {
                        
                        roleList += $"{numRoles.ToString()},";
                    }
                    roleList = roleList.Remove(roleList.Length - 1);
                    string name = GetRandomString();

                    writer.WriteLine($"{empid}\t{name}\t{dept}\t{roleList}");
                }
            }
        }
        private static string GetRandomString()
        {
            string randomString = Path.GetRandomFileName();
            randomString = randomString.Replace(".", "");
            randomString = randomString.Replace("0", "");
            randomString = randomString.Replace("1", "");
            randomString = randomString.Replace("2", "");
            randomString = randomString.Replace("3", "");
            randomString = randomString.Replace("4", "");
            randomString = randomString.Replace("5", "");
            randomString = randomString.Replace("6", "");
            randomString = randomString.Replace("7", "");
            randomString = randomString.Replace("8", "");
            randomString = randomString.Replace("9", "");
            return CultureInfo.CurrentUICulture.TextInfo.ToTitleCase(randomString);
        }
    }
}
