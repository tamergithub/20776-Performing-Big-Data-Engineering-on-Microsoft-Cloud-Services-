using Microsoft.Analytics.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace CustomOperators
{
    [SqlUserDefinedReducer(IsRecursive = true)]
    public class ReduceByRole : IReducer
    {
        // Reduce the input rowset to summarize the number of employees in each role in each department
        // The data in the input rowset contains the employee ID, employee name, department ID, and comma separated list of roles the employee has performed in that department

        // For each department, return:
        // DepartmentID int,
        // NumberOfAssociates int,
        // NumberOfEmployees int,
        // NumberOfTeamLeaders int,
        // NumberOfManagers int,
        // NumberOfVicePresidents int,
        // NumberOfPresidents int

        public override IEnumerable<IRow> Reduce(IRowset input, IUpdatableRow output)
        {
            Dictionary<string, int> roles = new Dictionary<string, int>();
            roles["associate"] = 0;
            roles["employee"] = 0;
            roles["teamleader"] = 0;
            roles["manager"] = 0;
            roles["vicepresident"] = 0;
            roles["president"] = 0;
            
            var groupedRows = (from row in input.Rows                               
                               select new
                               {
                                   dept = row.Get<int>("DepartmentID"),
                                   roles = row.Get<string>("Roles")
                               }).ToList();

            foreach (var row in groupedRows)
            {
                // Parse the comma separated list of roles into individual values
                string[] rolesForEmployee = row.roles.Split(',');

                // Iterate through this list of roles for the employee aggregate the role counts
                foreach (string role in rolesForEmployee)
                {
                    roles[role]++;
                }
            }

            // Output the aggregate results
            output.Set<int>("DepartmentID", groupedRows.First().dept);
            output.Set<int>("NumberOfAssociates", roles["associate"]);
            output.Set<int>("NumberOfEmployees", roles["employee"]);
            output.Set<int>("NumberOfTeamLeaders", roles["teamleader"]);
            output.Set<int>("NumberOfManagers", roles["manager"]);
            output.Set<int>("NumberOfVicePresidents", roles["vicepresident"]);
            output.Set<int>("NumberOfPresidents", roles["president"]);
            yield return output.AsReadOnly();
        }
    }
}