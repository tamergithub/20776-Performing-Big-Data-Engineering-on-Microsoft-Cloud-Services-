using Microsoft.Azure;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Threaten
{
    class Program
    {
        static void Main(string[] args)
        {
            string server = $"{CloudConfigurationManager.GetSetting("Server")}.database.windows.net";
            string database = CloudConfigurationManager.GetSetting("Database");
            string username = CloudConfigurationManager.GetSetting("Username");
            string password = CloudConfigurationManager.GetSetting("Password");
            string connectionString = $"Server=tcp:{server},1433;Database={database};User ID={username};Password={password};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30";

            RunQuery(connectionString);
        }

        private static void RunQuery(string connectionString)
        {
            try
            {
                Console.WriteLine("Enter a condition for the WHERE clause: ");
                string condition = Console.ReadLine();
                string query = "SELECT COUNT(*) FROM dbo.StolenVehicle WHERE VehicleRegistration LIKE '" + condition + "'";

                SqlConnection cn = new SqlConnection(connectionString);
                cn.Open();
                SqlCommand cmd = new SqlCommand(query, cn);
                int result = (int)cmd.ExecuteScalar();
                Console.WriteLine($"Number of matching rows: {result}");
                cn.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e.Message}");
            }
        }
    }
}
