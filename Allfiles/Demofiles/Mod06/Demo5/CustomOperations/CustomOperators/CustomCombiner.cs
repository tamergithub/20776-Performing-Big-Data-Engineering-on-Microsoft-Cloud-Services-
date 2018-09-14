using Microsoft.Analytics.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomOperators
{
    [SqlUserDefinedCombiner(Mode = CombinerMode.Inner)]
    public class FindDepartmentRoles : ICombiner
    {        
        // Combine the data in both rowsets to produce a list of employees and the names of the departments in which they have worked
        // The left input data contains the employee ID, employee name, department ID, and comma separated list of roles the employee has performed in that department
        // The right input data contains the department name and department ID
        // The output data should contains employee ID, employee name, department name, and role
        // A single employee will likely generate multiple rows of output
        public override IEnumerable<IRow> Combine(IRowset left, IRowset right, IUpdatableRow output)
        {
            // Read the right rowset containing department details into a local List collection 
            // (you can only enumerate an IRowset collection once, and we need to perform multiple iterations, so the data must be cached locally)
            var rightRowset = (from row in right.Rows                               
                               select new
                               {
                                   deptID = row.Get<int>("DepartmentID"),
                                   deptName = row.Get<string>("DepartmentName")                                   
                               }).ToList();

            // Join the rows in each collection across the Department ID column
            foreach (var leftRow in left.Rows)
            {
                // Find the name for the department
                var department = (from deptInfo in rightRowset
                                  where deptInfo.deptID == leftRow.Get<int>("DepartmentID")
                                  select new { id = deptInfo.deptID, name = deptInfo.deptName }).FirstOrDefault();

                // Output the employee and department role details
                if (department != null)
                {
                    // Split the comma separated list of roles in the employee data into individual values
                    string[] rolesForEmployee = leftRow.Get<string>("Roles").Split(',');

                    // Iterate through this list of roles for the employee and output each one in turn
                    foreach (string role in rolesForEmployee)
                    {
                        output.Set<int>("EmpID", leftRow.Get<int>("EmployeeID"));
                        output.Set<string>("EmpName", leftRow.Get<string>("EmployeeName"));
                        output.Set<string>("DeptName", department.name);
                        output.Set<string>("Role", role);
                        yield return output.AsReadOnly();
                    }
                }
            }
        }
    }
}