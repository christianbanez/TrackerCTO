using System.Data;
using System.Data.OleDb;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
//using iTextSharp;
//using iTextSharp.text;
//using iTextSharp.text.pdf;
//using iTextSharp.text.xml;

namespace CTOTracker.View
{
    /// <summary>
    /// Interaction logic for ScheduleView.xaml
    /// </summary>
    public partial class ScheduleView : UserControl
    {
        private DataConnection dataConnection;
        private List<string> allEmployees; // Store all employee names
        private List<string> filteredEmployees; //store filtered employee

        public class TaskModel
        {
            public string EmployeeName { get; set; }
            public string TaskName { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }

        }

        public ScheduleView()
        {
            InitializeComponent();
            dataConnection = new DataConnection();
            allEmployees = new List<string>();
            filteredEmployees = new List<string>();
            showallChkBox.IsChecked = true;
            //LoadScheduleData();
            //LoadCTOuseData();
            PopulateEmployeeComboBox();
            cbxEmployee.SelectionChanged += cbxEmployee_SelectionChanged;
        }

        private void LoadScheduleData()
        {
            try
            {
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    string query = "SELECT Schedule.schedID, Employee.inforID, Employee.fName, Employee.lName, Task.taskName, completed, plannedStart, plannedEnd, timeIn, " +
                        "timeOut, ctoEarned, ctoUsed, ctoBalance FROM (Schedule LEFT JOIN  Employee ON Schedule.empID = Employee.empID) " +
                        "LEFT JOIN Task ON Schedule.taskID = Task.taskID WHERE ctoBalance > 0.0 OR ctoBalance IS Null;";




                    OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    // Bind the DataTable to the DataGrid
                    scheduleDataGrid.ItemsSource = dataTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
        private void LoadCTOuseData()
        {
            try
            {
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    string query = "SELECT Schedule.schedID, Employee.inforID, Employee.fName, Employee.lName, Task.taskName, completed, plannedStart, plannedEnd, timeIn, " +
                        "timeOut, ctoEarned, ctoUsed, ctoBalance FROM (Schedule LEFT JOIN  Employee ON Schedule.empID = Employee.empID) " +
                        "LEFT JOIN Task ON Schedule.taskID = Task.taskID WHERE ctoUsed > 0.0;";

                    OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    // Bind the DataTable to the DataGrid
                    ctoUseDataGrid.ItemsSource = dataTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private void LoadEmployeeQuery(string empID)
        { 
            try
            {
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    string query = "SELECT Schedule.schedID, Employee.inforID, Employee.fName, Employee.lName, Task.taskName, completed, plannedStart, plannedEnd, timeIn, " +
                        "timeOut, ctoEarned, ctoUsed, ctoBalance FROM (Schedule LEFT JOIN  Employee ON Schedule.empID = Employee.empID) " +
                        "LEFT JOIN Task ON Schedule.taskID = Task.taskID WHERE (Employee.empID = ?) AND (ctoBalance > 0.0 OR ctoBalance IS Null);";

                    using (OleDbCommand command = new OleDbCommand(query, connection)) // Create a command with the query and connection
                    {
                        command.Parameters.AddWithValue("@empID", empID);
                        OleDbDataAdapter adapter = new OleDbDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        // Bind the DataTable to the DataGrid
                        scheduleDataGrid.ItemsSource = dataTable.DefaultView;
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
        private void LoadCtoEmployeeQuery(string empID)
        {
            try
            {
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    string query = "SELECT Schedule.schedID, Employee.inforID, Employee.fName, Employee.lName, Task.taskName, completed, plannedStart, plannedEnd, timeIn, " +
                        "timeOut, ctoEarned, ctoUsed, ctoBalance FROM (Schedule LEFT JOIN  Employee ON Schedule.empID = Employee.empID) " +
                        "LEFT JOIN Task ON Schedule.taskID = Task.taskID WHERE ctoUsed > 0.0 AND Employee.empID = ?;";
                    using (OleDbCommand command = new OleDbCommand(query, connection)) // Create a command with the query and connection
                    {
                        command.Parameters.AddWithValue("@empID", empID);
                        OleDbDataAdapter adapter = new OleDbDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        // Bind the DataTable to the DataGrid
                        ctoUseDataGrid.ItemsSource = dataTable.DefaultView;
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
        private void FilterDataByEmployee()
        {
            string selectedEmployee = cbxEmployee.SelectedItem?.ToString() ?? string.Empty;
            string employeeId = GetEmployeeId(selectedEmployee);
            if (!string.IsNullOrEmpty(employeeId))
            {
               
                LoadCtoEmployeeQuery(employeeId);
                LoadEmployeeQuery(employeeId);  // Load data for the selected employee

            }
            else
            {
                MessageBox.Show("Employee not found.");
            }
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
                    cbxEmployee.ItemsSource = allEmployees;
                }
                //else
                //{
                //    // Handle the case when 'allEmployees' is null
                //    MessageBox.Show("No employees found.");
                //}
            }
            catch (Exception ex)
            {
                // Display an error message if an exception occurs
                MessageBox.Show("Error: " + ex.Message);
            }
        }
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

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void DataGrid_AutoGenerateColumns(object sender, EventArgs e)
        {
            scheduleDataGrid.Columns[0].Visibility = Visibility.Collapsed;
            scheduleDataGrid.Columns[0].Header = "Schedule ID";
            scheduleDataGrid.Columns[1].Header = "Infor ID";
            scheduleDataGrid.Columns[1].Width = 75;
            scheduleDataGrid.Columns[2].Header = "First Name";
            scheduleDataGrid.Columns[2].Width = 185;
            scheduleDataGrid.Columns[3].Header = "Last Name";
            scheduleDataGrid.Columns[3].Width = 185;
            scheduleDataGrid.Columns[4].Header = "Task Name";
            scheduleDataGrid.Columns[4].Width = 125;
            scheduleDataGrid.Columns[5].Header = "Completed";
            scheduleDataGrid.Columns[6].Header = "Start Date";
            scheduleDataGrid.Columns[7].Header = "End Date";
            scheduleDataGrid.Columns[8].Header = "Time In";
            scheduleDataGrid.Columns[9].Header = "Time Out";
            scheduleDataGrid.Columns[10].Header = "CTO Earned";
            scheduleDataGrid.Columns[11].Header = "CTO Used";
            scheduleDataGrid.Columns[12].Header = "CTO Balance";
            
        }

        // Event handler for double-clicking on a row in the DataGrid
        private void DataGridRow_MouseDoubleClick_1(object sender, MouseButtonEventArgs e)
        {
            // Check if a row is selected
            if (scheduleDataGrid.SelectedItem != null)
            {
                // Retrieve the selected row (data item)
                DataRowView selectedRow = (DataRowView)scheduleDataGrid.SelectedItem;

                // Extract relevant data from the selected row
                string fullName = selectedRow["fName"].ToString() + " " + selectedRow["lName"].ToString();
                string taskName = selectedRow["taskName"].ToString();
                DateTime startDate = (DateTime)selectedRow["plannedStart"];
                DateTime endDate = (DateTime)selectedRow["plannedEnd"];
                string timeIn = selectedRow["timeIn"].ToString();
                string timeOut = selectedRow["timeOut"].ToString();
                int schedID = Convert.ToInt32(selectedRow["schedID"]); // Assuming schedID is an integer

                // Create an instance of AddTask form
                AddTask addTaskWindow = new AddTask();

                // Pass selected data to AddTask form, including schedID
                addTaskWindow.PopulateWithData(fullName, taskName, startDate, endDate, timeIn, timeOut, schedID);

                addTaskWindow.AddButton.Visibility = Visibility.Collapsed;
                addTaskWindow.SaveButton.Visibility = Visibility.Visible;
                // Show the AddTask form
                addTaskWindow.ShowDialog();


                if (cbxEmployee.IsEnabled && cbxEmployee.SelectedItem != null)
                {
                    FilterDataByEmployee();  // Filter data when a new employee is selected
                }
                else
                {
                    LoadScheduleData();
                    LoadCTOuseData();
                }
            }
        }

        private void btnAssignTask_Click(object sender, RoutedEventArgs e)
        {
            // Instantiate an instance of the AddTask window
            AddTask addTaskWindow = new AddTask();

            addTaskWindow.Visibility = Visibility.Collapsed;
            addTaskWindow.schedIDTextBox.Visibility = Visibility.Collapsed;
            // Show the AddTask window
            addTaskWindow.ShowDialog();

            if (cbxEmployee.IsEnabled && cbxEmployee.SelectedItem != null)
            {
                FilterDataByEmployee();  // Filter data when a new employee is selected
            }
            else
            {
                LoadScheduleData();
                LoadCTOuseData();
            }
        }

        private void cbxEmployee_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            monthPicker.SelectedDate = null;
            if (cbxEmployee.IsEnabled && cbxEmployee.SelectedItem != null)
            {
                FilterDataByEmployee();  // Filter data when a new employee is selected
            }
        }

        private void ctoUseDataGrid_AutoGeneratedColumns(object sender, EventArgs e)
        {
            ctoUseDataGrid.Columns[0].Visibility = Visibility.Collapsed;
            ctoUseDataGrid.Columns[0].Header = "Schedule ID";
            ctoUseDataGrid.Columns[1].Header = "Infor ID";
            ctoUseDataGrid.Columns[1].Width = 75;
            ctoUseDataGrid.Columns[2].Header = "First Name";
            ctoUseDataGrid.Columns[2].Width = 185;
            ctoUseDataGrid.Columns[3].Header = "Last Name";
            ctoUseDataGrid.Columns[3].Width = 185;
            ctoUseDataGrid.Columns[4].Header = "Task Name";
            ctoUseDataGrid.Columns[4].Width = 125;
            ctoUseDataGrid.Columns[5].Header = "Completed";
            ctoUseDataGrid.Columns[6].Header = "Start Date";
            ctoUseDataGrid.Columns[7].Header = "End Date";
            ctoUseDataGrid.Columns[8].Header = "Time In";
            ctoUseDataGrid.Columns[9].Header = "Time Out";
            ctoUseDataGrid.Columns[10].Header = "CTO Earned";
            ctoUseDataGrid.Columns[11].Header = "CTO Used";
            ctoUseDataGrid.Columns[12].Header = "CTO Balance";
        }

        private void LoadDataForMonthEmployee(DateTime date, string empID)
        {
            try
            {
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    string query = @"SELECT Schedule.schedID, Employee.inforID, Employee.fName, Employee.lName, Task.taskName, completed, plannedStart, plannedEnd, timeIn, " +
                        "timeOut, ctoEarned, ctoUsed, ctoBalance FROM (Schedule LEFT JOIN  Employee ON Schedule.empID = Employee.empID) " +
                        "LEFT JOIN Task ON Schedule.taskID = Task.taskID WHERE (Employee.empID = ?) AND (ctoBalance > 0.0 OR ctoBalance IS Null) AND (MONTH(plannedStart) = ? AND YEAR(plannedStart) = ?);";
                    using (OleDbCommand command = new OleDbCommand(query, connection))
                    {
                        // Add month and year as parameters
                        command.Parameters.AddWithValue("@empID", empID);
                        command.Parameters.AddWithValue("@Month", date.Month);
                        command.Parameters.AddWithValue("@Year", date.Year);

                        OleDbDataAdapter adapter = new OleDbDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        // Bind the DataTable to the DataGrid
                        scheduleDataGrid.ItemsSource = dataTable.DefaultView;
                    }


                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private void LoadDataForMonthCtoEmployee(DateTime date, string empID)
        {
            try
            {
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    string query = @"SELECT Schedule.schedID, Employee.inforID, Employee.fName, Employee.lName, Task.taskName, completed, plannedStart, plannedEnd, timeIn, " +
                        "timeOut, ctoEarned, ctoUsed, ctoBalance FROM (Schedule LEFT JOIN  Employee ON Schedule.empID = Employee.empID) " +
                        "LEFT JOIN Task ON Schedule.taskID = Task.taskID WHERE (Employee.empID = ?) AND (ctoUsed > 0.0) AND (MONTH(plannedStart) = ? AND YEAR(plannedStart) = ?);";
                    using (OleDbCommand command = new OleDbCommand(query, connection))
                    {
                        // Add month and year as parameters
                        command.Parameters.AddWithValue("@empID", empID);
                        command.Parameters.AddWithValue("@Month", date.Month);
                        command.Parameters.AddWithValue("@Year", date.Year);

                        OleDbDataAdapter adapter = new OleDbDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        // Bind the DataTable to the DataGrid
                        ctoUseDataGrid.ItemsSource = dataTable.DefaultView;
                    }


                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private void LoadDataForMonthAll(DateTime date)
        {
            try
            {
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    string query = @"SELECT Schedule.schedID, Employee.inforID, Employee.fName, Employee.lName, Task.taskName, completed, plannedStart, plannedEnd, timeIn, " +
                        "timeOut, ctoEarned, ctoUsed, ctoBalance FROM (Schedule LEFT JOIN  Employee ON Schedule.empID = Employee.empID) " +
                        "LEFT JOIN Task ON Schedule.taskID = Task.taskID WHERE (ctoBalance > 0.0 OR ctoBalance IS Null) AND (MONTH(plannedStart) = ? AND YEAR(plannedStart) = ?);";
                    using (OleDbCommand command = new OleDbCommand(query, connection))
                    {
                        // Add month and year as parameters
                        command.Parameters.AddWithValue("@Month", date.Month);
                        command.Parameters.AddWithValue("@Year", date.Year);

                        OleDbDataAdapter adapter = new OleDbDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        // Bind the DataTable to the DataGrid
                        scheduleDataGrid.ItemsSource = dataTable.DefaultView;
                    }


                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private void LoadDataForMonthCtoAll(DateTime date)
        {
            try
            {
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    string query = @"SELECT Schedule.schedID, Employee.inforID, Employee.fName, Employee.lName, Task.taskName, completed, plannedStart, plannedEnd, timeIn, " +
                        "timeOut, ctoEarned, ctoUsed, ctoBalance FROM (Schedule LEFT JOIN  Employee ON Schedule.empID = Employee.empID) " +
                        "LEFT JOIN Task ON Schedule.taskID = Task.taskID WHERE (ctoUsed > 0.0) AND (MONTH(plannedStart) = ? AND YEAR(plannedStart) = ?);";
                    using (OleDbCommand command = new OleDbCommand(query, connection))
                    {
                        // Add month and year as parameters
                        command.Parameters.AddWithValue("@Month", date.Month);
                        command.Parameters.AddWithValue("@Year", date.Year);

                        OleDbDataAdapter adapter = new OleDbDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        // Bind the DataTable to the DataGrid
                        ctoUseDataGrid.ItemsSource = dataTable.DefaultView;
                    }


                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
        private void showallChecked(object sender, RoutedEventArgs e)
        {
            LoadScheduleData();
            LoadCTOuseData();
            cbxEmployee.SelectedIndex = -1;
            cbxEmployee.IsEnabled = false;
            cbxEmployee.Text = "";
            monthPicker.SelectedDate = null;
        }

    

        private void showallUnchecked(object sender, RoutedEventArgs e)
        {
            monthPicker.SelectedDate = null;
            cbxEmployee.IsEnabled = true;  // Enable employee combo box
            if (cbxEmployee.SelectedItem != null)
            {
                FilterDataByEmployee();  // Call a function to filter data based on selected employee
            }
            else
            {
                scheduleDataGrid.ItemsSource = null;  // Clear the DataGrid if no employee is selected
                ctoUseDataGrid.ItemsSource = null;
            }
        }

        private void monthPicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (monthPicker.SelectedDate.HasValue)
            {
                DateTime selectedDate = monthPicker.SelectedDate.Value;

                if (cbxEmployee.IsEnabled && cbxEmployee.SelectedItem != null)
                {
                    // Filter data for the selected employee and selected month/year
                    string selectedEmployee = cbxEmployee.SelectedItem?.ToString() ?? string.Empty;
                    string employeeId = GetEmployeeId(selectedEmployee);
                    if (!string.IsNullOrEmpty(employeeId))
                    {
                        LoadDataForMonthEmployee(selectedDate, employeeId);
                        LoadDataForMonthCtoEmployee(selectedDate, employeeId);
                    }
                }
                else
                {
                    // Filter all data for selected month/year
                    LoadDataForMonthAll(selectedDate);
                    LoadDataForMonthCtoAll(selectedDate);

                }
            }
        }
    }
}