using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Security.Policy;
using Xceed.Wpf.AvalonDock.Themes;

namespace CTOTracker.View.UserControls
{
    /// <summary>
    /// Interaction logic for ReportView.xaml
    /// </summary>
    public partial class ReportView : UserControl
    {
        private DataConnection dataConnection;
        private List<string> allEmployees; // Store all employee names
        private List<string> filteredEmployees; // Store filtered employee names
        //EmployeeView employeeView=new EmployeeView();

        public ReportView()
        {
            InitializeComponent();
            dataConnection = new DataConnection();
            EmployeeReportView();
            PopulateComboBox();
            PopulateEmployeeComboBox();
            filteredEmployees = new List<string>();
            cbxFilterRep.SelectionChanged += CbxFilterRep_SelectionChanged;
        }
        private void EmployeeReportView()
        {
            string query = "SELECT Employee.inforID, Employee.fName, Employee.lName, Employee.email, Role.roleName, Schedule.ctoBalance\r\nFROM (Role INNER JOIN Employee ON Role.roleID = Employee.roleID) INNER JOIN Schedule ON Employee.empID = Schedule.empID;\r\n";
            LoadEmployeeReport(query);

        }
        private void LoadEmployeeReport(string query) //loads the employee report to report data grid
        {
            using (OleDbConnection connection = dataConnection.GetConnection())
            {
                try
                {
                    connection.Open();
                    OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    if (dataTable != null && dataTable.Rows.Count > 0)
                    {
                        reportDataGrid.ItemsSource = dataTable.DefaultView;
                    }
                    else
                    {
                        MessageBox.Show("No data found.", "Information");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error");
                }
                finally
                {
                    connection.Close();
                }
            }
        }
        private void PopulateComboBox()
        {
            // Create a list of strings to populate the ComboBox
            List<string> filterOptions = new List<string>
            {
                "Employee with CTO balance",
                "Employee",
                "All Task Schedule"
            };

            // Assign the list as the ItemsSource for the ComboBox
            cbxFilterRep.ItemsSource = filterOptions;
        }
        private void CbxFilterRep_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbxFilterRep.SelectedItem != null)
            {
                // Get the selected item

                // Check if the selected item matches the specific item
                if (cbxFilterRep.SelectedItem.ToString() == "Employees with CTO balance")
                {
                    LoadEmployeeReportWithCTO();
                    EmpFilPnl.Visibility = System.Windows.Visibility.Collapsed;
                }
                else if (cbxFilterRep.SelectedItem.ToString() == "Employee")
                {
                    // Show the Employee Filtered Panel
                    EmpFilPnl.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    // Hide the Employee Filtered Panel
                    EmpFilPnl.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }
        private void LoadEmployeeReportWithCTO()
        {
            // Your code to load the report for employees with remaining CTO balance
            // Modify your query to retrieve employees with remaining CTO balance
            string query = @"SELECT Employee.inforID, Employee.fName, Employee.lName, Employee.email, Employee.contact, Role.roleName, Schedule.ctoBalance
                            FROM (Employee
                            INNER JOIN Role ON Employee.roleID = Role.roleID)
                            INNER JOIN Schedule ON Employee.empID = Schedule.empID
                            WHERE Schedule.ctoBalance > 0;";

            LoadEmployeeReport(query);
        } //filter combo box (prev version)
        private List<string> GetDataFromEmployeeTable()
        {
            // Create a list to store employee names
            List<string> employees = new List<string>();

            try
            {
                // Get connection from DataConnection
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    // Define the SQL query to select first names (fName) and last names (lName) from the Employee table
                    string query = "SELECT fName, lName FROM Employee";

                    // Create a command object with the query and connection
                    using (OleDbCommand command = new OleDbCommand(query, connection))
                    {
                        // Open the connection to the database
                        connection.Open();

                        // Execute the command and retrieve data using a data reader
                        using (OleDbDataReader reader = command.ExecuteReader())
                        {
                            // Iterate through the data reader to read each row
                            while (reader.Read())
                            {
                                // Check if the fName and lName columns contain non-null values
                                if (!reader.IsDBNull(reader.GetOrdinal("fName")) && !reader.IsDBNull(reader.GetOrdinal("lName")))
                                {
                                    // Concatenate the first name and last name to form the full name
                                    string fullName = $"{reader["fName"]} {reader["lName"]}";
                                    // Add the full name to the list of employees
                                    employees.Add(fullName);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Display an error message if an exception occurs
                MessageBox.Show("Error: " + ex.Message);
            }

            // Return the list of employee names retrieved from the database
            return employees;
        }
        private void PopulateEmployeeComboBox()
        {
            try
            {
                // Fetch data from the Employee table
                allEmployees = GetDataFromEmployeeTable();

                // Check if 'allEmployees' is null before binding to the ComboBox
                if (allEmployees != null)
                {
                    cmbxEmpName.ItemsSource = allEmployees;
                }
                else
                {
                    // Handle the case when 'allEmployees' is null
                    MessageBox.Show("No employees found.");
                }
            }
            catch (Exception ex)
            {
                // Display an error message if an exception occurs
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        private void cmbxEmpName_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Clear the filtered list
            filteredEmployees.Clear();

            string searchText = cmbxEmpName.Text.ToLower();

            // Filter the items in the ComboBox based on the entered text
            foreach (string employee in allEmployees)
            {
                if (employee.ToLower().Contains(searchText))
                {
                    filteredEmployees.Add(employee);
                }
            }

            // Update the ComboBox items source with the filtered list
            cmbxEmpName.ItemsSource = filteredEmployees;

            // Open the dropdown
            cmbxEmpName.IsDropDownOpen = true;
        }

        private void reportDG_DoubleMouseClick(object sender, MouseButtonEventArgs e)
        {
            // Retrieve the selected row (data item)
            DataGrid gd = (DataGrid)sender;
            DataRowView row_selected = (DataRowView)gd.SelectedItem;
           
            try
            {
                if (row_selected != null)
                {
                    // Extract values from the row and populate labels
                    lblID.Content = row_selected["inforID"].ToString();
                    string fullName = row_selected["fName"].ToString() + " " + row_selected["lName"].ToString(); //get fullname of the selected employee
                    lblEmpName.Content = fullName.ToString();
                    lblRole.Content = row_selected["roleName"].ToString();
                    /*kulang pa ng contact number & email*/
                    LoadEmployeeReportHistory(fullName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
        private string GetEmployeeId(string employeeName)
        {
            string? employeeId = null; // Initialize employeeId to null
            try
            {
                using (OleDbConnection connection = dataConnection.GetConnection()) // Create a connection using DataConnection
                {
                    // Modified query to concatenate fName and lName
                    string query = "SELECT empID FROM Employee WHERE fName & ' ' & lName = ?";

                    using (OleDbCommand command = new OleDbCommand(query, connection)) // Create a command with the query and connection
                    {
                        command.Parameters.AddWithValue("@employeeName", employeeName); // Add parameter for employee name
                        connection.Open(); // Open the connection
                        object? result = command.ExecuteScalar(); // Execute the query and get the result

                        if (result != null) // Check if the result is not null
                        {
                            employeeId = result.ToString(); // Assign the employee ID to employeeId
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving employee ID: " + ex.Message); // Display error message if an exception occurs
            }
            // Return employeeId if not null, otherwise throw an exception
            return employeeId ?? throw new Exception("Employee ID not found.");
        }
        private void LoadEmployeeReportHistory(string fullName)
        {
            string employeeId = GetEmployeeId(fullName);
            
            try
            {
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    // Your code to load the report for employees' history
                    // Modify your query to retrieve employees' history
                    string query = @"SELECT Task.taskName, timeIn, timeOut, ctoEarned, ctoUsed, dateUsed, ctoBalance FROM (Schedule INNER JOIN Employee ON Schedule.empID = Employee.empID)" +
                                   "INNER JOIN Task ON Schedule.taskID = Task.taskID WHERE Employee.empID = ?;";

                    using (OleDbCommand command = new OleDbCommand(query, connection)) // Create a command with the query and connection
                    {
                        command.Parameters.AddWithValue("@employeeId", employeeId);
                        OleDbDataAdapter adapter = new OleDbDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        bool allTasksComplete = true;

                        // Modify the data table for display
                        foreach (DataRow row in dataTable.Rows)
                        {
                            // Check for null values in timeIn and timeOut
                            if (row["timeIn"] == DBNull.Value || row["timeOut"] == DBNull.Value)
                            {
                                allTasksComplete = false; 
                            }
                        }
                        if (!allTasksComplete)
                        {
                            // Display a message indicating the task is not yet completed
                            MessageBox.Show("This task not yet completed.", "Information");
                        }
                        else
                        {
                            // Bind the DataTable to the DataGrid
                            scheduleDataGrid1.ItemsSource = dataTable.DefaultView;
                            EmpFilPnl.Visibility = System.Windows.Visibility.Visible;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
}
