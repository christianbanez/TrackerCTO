using System.Data;
using System.Data.OleDb;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static System.Runtime.InteropServices.JavaScript.JSType;
//using iTextSharp;
//using iTextSharp.text;
//using iTextSharp.text.pdf;
//using iTextSharp.text.xml;

namespace CTOTracker.View
{

    public partial class ScheduleView : UserControl
    {
        private DataConnection dataConnection;
        private List<string> allEmployees; // Store all employee names
        private List<string> filteredEmployees; //store filtered employee
        private string taskFilter = "";

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
            LoadScheduleData();
            LoadCTOuseData();
            PopulateEmployeeComboBox();
            cbxEmployee.SelectionChanged += cbxEmployee_SelectionChanged;
            cbxFilterTask.SelectionChanged += cbxFilterTask_SelectionChanged;
            PopulateTaskComboBox();
            PopulateMoYComboBox();
        }
        

        private void LoadScheduleData()
        {
            try
            {
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    string query = "SELECT Schedule.schedID, Employee.inforID, Employee.fName, Employee.lName, Task.taskName, completed, " +
                        "Format(plannedStart, 'MM/dd/yyyy') AS plannedStart, Format(plannedEnd, 'MM/dd/yyyy') AS plannedEnd, " +
                        "Format(timeIn, 'h:mm AM/PM') AS timeIn, Format(timeout, 'h:mm AM/PM') AS timeOut, ctoEarned, ctoUsed, ctoBalance " +
                        "FROM (Schedule LEFT JOIN  Employee ON Schedule.empID = Employee.empID) " +
                        "LEFT JOIN Task ON Schedule.taskID = Task.taskID";

                    // CTO Balance filter
                    string ctoBalanceFilter = " WHERE (ctoBalance > 0.0 OR ctoBalance IS NULL)";
                    query += ctoBalanceFilter;

                    // Date filter based on the selected month and year
                    if (monthPicker.SelectedDate.HasValue)
                    {
                        DateTime selectedDate = monthPicker.SelectedDate.Value;
                        query += " AND (MONTH(plannedStart) = ? AND YEAR(plannedStart) = ?)";
                    }
                    if (!string.IsNullOrEmpty(taskFilter))
                    {
                        query += $" AND Task.taskName = '{taskFilter}'";
                    }
                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection))
                    {
                        // Add month and year parameters if applicable
                        if (monthPicker.SelectedDate.HasValue)
                        {
                            DateTime selectedDate = monthPicker.SelectedDate.Value;
                            adapter.SelectCommand.Parameters.AddWithValue("@Month", selectedDate.Month);
                            adapter.SelectCommand.Parameters.AddWithValue("@Year", selectedDate.Year);
                        }

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
        private void LoadCTOuseData()
        {
            try
            {
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    string query = "SELECT Schedule.schedID, Employee.inforID, Employee.fName, Employee.lName, Task.taskName, " +
                        "Format(plannedStart, 'MM/dd/yyyy') AS plannedStart, Format(plannedEnd, 'MM/dd/yyyy') AS plannedEnd, " +
                        "Format(timeIn, 'h:mm AM/PM') AS timeIn, Format(timeout, 'h:mm AM/PM') AS timeOut, ctoEarned, ctoUsed, useDesc " +
                        "FROM (Schedule LEFT JOIN  Employee ON Schedule.empID = Employee.empID) " +
                        "LEFT JOIN Task ON Schedule.taskID = Task.taskID";

                    // CTO Balance filter
                    string ctoBalanceFilter = " WHERE (ctoUsed > 0.0)";
                    query += ctoBalanceFilter;

                    // Date filter based on the selected month and year
                    if (monthPicker.SelectedDate.HasValue)
                    {
                        DateTime selectedDate = monthPicker.SelectedDate.Value;
                        query += " AND (MONTH(plannedStart) = ? AND YEAR(plannedStart) = ?)";
                    }
                    if (!string.IsNullOrEmpty(taskFilter))
                    {
                        query += $" AND Task.taskName = '{taskFilter}'";
                    }

                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection))
                    {
                        // Add month and year parameters if applicable
                        if (monthPicker.SelectedDate.HasValue)
                        {
                            DateTime selectedDate = monthPicker.SelectedDate.Value;
                            adapter.SelectCommand.Parameters.AddWithValue("@Month", selectedDate.Month);
                            adapter.SelectCommand.Parameters.AddWithValue("@Year", selectedDate.Year);
                        }

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

        private void LoadEmployeeQuery()
        {
            try
            {
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    // Base query
                    string baseQuery = "SELECT Schedule.schedID, Employee.inforID, Employee.fName, Employee.lName, Task.taskName, completed, " +
                        "Format(plannedStart, 'MM/dd/yyyy') AS plannedStart, Format(plannedEnd, 'MM/dd/yyyy') AS plannedEnd, " +
                        "Format(timeIn, 'h:mm AM/PM') AS timeIn, Format(timeout, 'h:mm AM/PM') AS timeOut, ctoEarned, ctoUsed, ctoBalance " +
                        "FROM (Schedule LEFT JOIN  Employee ON Schedule.empID = Employee.empID) " +
                        "LEFT JOIN Task ON Schedule.taskID = Task.taskID";

                    // Initialize the complete query with base query only
                    string query = baseQuery;

                    // Apply Employee ID filter only if the checkbox for showing all is unchecked
                    if (!showallChkBox.IsChecked.HasValue || !showallChkBox.IsChecked.Value)
                    {
                        string selectedEmployee = cbxEmployee.SelectedItem?.ToString() ?? string.Empty;
                        string employeeId = GetEmployeeId(selectedEmployee);
                        string empIdFilter = cbxEmployee.SelectedValue != null ? " WHERE Employee.empID = ?" : "";
                        query += empIdFilter;
                    }
                    if (!string.IsNullOrEmpty(taskFilter))
                    {
                        query += $" AND Task.taskName = '{taskFilter}'";
                    }

                    // CTO Balance filter
                    string ctoBalanceFilter = " AND (ctoBalance > 0.0 OR ctoBalance IS NULL)";
                    query += ctoBalanceFilter;

                    // Date filter based on the selected month and year
                    if (monthPicker.SelectedDate.HasValue)
                    {
                        DateTime selectedDate = monthPicker.SelectedDate.Value;
                        query += " AND (MONTH(plannedStart) = ? AND YEAR(plannedStart) = ?)";
                    }

                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection))
                    {
                        // Add employee ID parameter if applicable and checkbox for showing all is unchecked
                        if (!showallChkBox.IsChecked.HasValue || !showallChkBox.IsChecked.Value)
                        {
                            if (cbxEmployee.SelectedValue != null)
                            {
                                adapter.SelectCommand.Parameters.AddWithValue("@empID", GetEmployeeId(cbxEmployee.SelectedItem?.ToString() ?? string.Empty));
                            }
                        }

                        // Add month and year parameters if applicable
                        if (monthPicker.SelectedDate.HasValue)
                        {
                            DateTime selectedDate = monthPicker.SelectedDate.Value;
                            adapter.SelectCommand.Parameters.AddWithValue("@Month", selectedDate.Month);
                            adapter.SelectCommand.Parameters.AddWithValue("@Year", selectedDate.Year);
                        }

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

        private void LoadCtoEmployeeQuery()
        {
            try
            {
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    // Base query
                    string baseQuery = "SELECT Schedule.schedID, Employee.inforID, Employee.fName, Employee.lName, Task.taskName, " +
                        "Format(plannedStart, 'MM/dd/yyyy') AS plannedStart, Format(plannedEnd, 'MM/dd/yyyy') AS plannedEnd, " +
                        "Format(timeIn, 'h:mm AM/PM') AS timeIn, Format(timeout, 'h:mm AM/PM') AS timeOut, ctoEarned, ctoUsed, useDesc " +
                        "FROM (Schedule LEFT JOIN  Employee ON Schedule.empID = Employee.empID) " +
                        "LEFT JOIN Task ON Schedule.taskID = Task.taskID";

                    // Initialize the complete query with base query only
                    string query = baseQuery;

                    // Apply Employee ID filter only if the checkbox for showing all is unchecked
                    if (!showallChkBox.IsChecked.HasValue || !showallChkBox.IsChecked.Value)
                    {
                        string selectedEmployee = cbxEmployee.SelectedItem?.ToString() ?? string.Empty;
                        string employeeId = GetEmployeeId(selectedEmployee);
                        string empIdFilter = cbxEmployee.SelectedValue != null ? " WHERE Employee.empID = ?" : "";
                        query += empIdFilter;
                    }
                    if (!string.IsNullOrEmpty(taskFilter))
                    {
                        query += $" AND Task.taskName = '{taskFilter}'";
                    }

                    // CTO Balance filter
                    string ctoUseFilter = " AND (ctoUsed > 0.0)";
                    query += ctoUseFilter;

                    // Date filter based on the selected month and year
                    if (monthPicker.SelectedDate.HasValue)
                    {
                        DateTime selectedDate = monthPicker.SelectedDate.Value;
                        query += " AND (MONTH(plannedStart) = ? AND YEAR(plannedStart) = ?)";
                    }

                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection))
                    {
                        // Add employee ID parameter if applicable and checkbox for showing all is unchecked
                        if (!showallChkBox.IsChecked.HasValue || !showallChkBox.IsChecked.Value)
                        {
                            if (cbxEmployee.SelectedValue != null)
                            {
                                adapter.SelectCommand.Parameters.AddWithValue("@empID", GetEmployeeId(cbxEmployee.SelectedItem?.ToString() ?? string.Empty));
                            }
                        }

                        // Add month and year parameters if applicable
                        if (monthPicker.SelectedDate.HasValue)
                        {
                            DateTime selectedDate = monthPicker.SelectedDate.Value;
                            adapter.SelectCommand.Parameters.AddWithValue("@Month", selectedDate.Month);
                            adapter.SelectCommand.Parameters.AddWithValue("@Year", selectedDate.Year);
                        }

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

        private void PopulateMoYComboBox()
        {
            cmbxMoY.Items.Add("Month/Year");
            cmbxMoY.Items.Add("Year");
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

        // Event handler for double-clicking on a row in the DataGrid
        private void DataGridRow_MouseDoubleClick_1(object sender, MouseButtonEventArgs e)
        {
            // Check if a row is selected
            if (scheduleDataGrid.SelectedItem != null)
            {
                // Retrieve the selected row (data item)
                DataRowView selectedRow = (DataRowView)scheduleDataGrid.SelectedItem;

                bool completed = Convert.ToBoolean(selectedRow["completed"]); // Check if the task is completed
                // If the task is completed, do not open the Add Task window
                if (completed)
                {
                    MessageBox.Show("This task is already completed. You cannot update it.");
                    return;
                }

                // Extract relevant data from the selected row
                string fullName = selectedRow["fName"].ToString() + " " + selectedRow["lName"].ToString();
                string taskName = selectedRow["taskName"].ToString();
                string startDateString = selectedRow["plannedStart"].ToString();
                string endDateString = selectedRow["plannedEnd"].ToString();
                string timeIn = selectedRow["timeIn"].ToString();
                string timeOut = selectedRow["timeOut"].ToString();
                int schedID = Convert.ToInt32(selectedRow["schedID"]); // Assuming schedID is an integer

                DateTime startDate, endDate;

                // Parse the strings to DateTime
                if (DateTime.TryParse(startDateString, out startDate) && DateTime.TryParse(endDateString, out endDate))
                {
                    // Create an instance of AddTask form
                    AddTask addTaskWindow = new AddTask();

                    // Pass selected data to AddTask form, including schedID
                    addTaskWindow.PopulateWithData(fullName, taskName, startDate, endDate, timeIn, timeOut, schedID);

                    // Ensure the buttons are configured properly
                    addTaskWindow.AddButton.Visibility = Visibility.Collapsed;
                    addTaskWindow.SaveButton.Visibility = Visibility.Visible;

                    // Show the AddTask form
                    addTaskWindow.ShowDialog();


                    if (cbxEmployee.SelectedItem != null)
                    {
                        LoadEmployeeQuery();  // Filter data when a new employee is selected
                        LoadCtoEmployeeQuery();
                    }
                    else
                    {
                        LoadScheduleData();
                        LoadCTOuseData();
                    }
                    PopulateTaskComboBox();
                }
            }
        }

        private void btnAssignTask_Click(object sender, RoutedEventArgs e)
        {
            // Instantiate an instance of the AddTask window
            AddTask addTaskWindow = new AddTask();

            addTaskWindow.SaveButton.Visibility = Visibility.Collapsed;
            addTaskWindow.schedIDTextBox.Visibility = Visibility.Collapsed;
            // Show the AddTask window
            addTaskWindow.ShowDialog();

            if (cbxEmployee.SelectedItem != null)
            {
                LoadEmployeeQuery();  // Filter data when a new employee is selected
                LoadCtoEmployeeQuery();
            }
            else
            {
                LoadScheduleData();
                LoadCTOuseData();
            }
            PopulateTaskComboBox();

        }

        private void cbxEmployee_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbxEmployee.SelectedItem != null)
            {
                showallChkBox.IsChecked = false;
                LoadEmployeeQuery();
                LoadCtoEmployeeQuery();
                /*FilterDataByEmployee(); */ // Filter data when a new employee is selected
            }
        }

        private void ctoUseDataGrid_AutoGeneratedColumns(object sender, EventArgs e)
        {
 
            ctoUseDataGrid.Columns[0].Visibility = Visibility.Collapsed;
            ctoUseDataGrid.Columns[0].Header = "Schedule ID";
            ctoUseDataGrid.Columns[1].Header = "Infor ID";
            ctoUseDataGrid.Columns[1].Width = 75;
            ctoUseDataGrid.Columns[2].Header = "First Name";
            ctoUseDataGrid.Columns[2].Width = 100;
            ctoUseDataGrid.Columns[3].Header = "Last Name";
            ctoUseDataGrid.Columns[3].Width = 100;
            ctoUseDataGrid.Columns[4].Header = "Task";
            ctoUseDataGrid.Columns[4].Width = 125;
            ctoUseDataGrid.Columns[5].Header = "Start Date";
            ctoUseDataGrid.Columns[6].Header = "End Date";
            ctoUseDataGrid.Columns[7].Header = "Time In";
            ctoUseDataGrid.Columns[8].Header = "Time Out";
            ctoUseDataGrid.Columns[9].Header = "CTO Earned";
            ctoUseDataGrid.Columns[10].Header = "CTO Used";
            ctoUseDataGrid.Columns[11].Header = "Use Description";
            ctoUseDataGrid.Columns[11].Width = 200;
        }

        private void showallChecked(object sender, RoutedEventArgs e)
        {
            //LoadEmployeeQuery();
            LoadScheduleData();
            LoadCTOuseData();
            cbxEmployee.SelectedIndex = -1;
            //cbxEmployee.IsEnabled = false;
        }

        private void showallUnchecked(object sender, RoutedEventArgs e)
        {
            monthPicker.Text = "";
            cbxEmployee.Text = "Employee";
            cbxFilterTask.Text = "Filter by Task";
            LoadEmployeeQuery();
            LoadCtoEmployeeQuery();
        }

        private void monthPicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbxEmployee.SelectedItem != null)
            {
                LoadEmployeeQuery();
                LoadCtoEmployeeQuery();
            }
            else
            {
                LoadScheduleData();
                LoadCTOuseData();
            }
        }

        private void btnUseCtoUsed_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                // Check if any rows are selected in the scheduleDataGrid
                if (scheduleDataGrid.SelectedItems.Count > 0)
                {
                    // Create a list to hold selected schedule data
                    List<DataRowView> selectedRows = new List<DataRowView>();
                    string? firstId = null;
                    bool allRowsValid = true;
                    // Iterate through each selected row in the scheduleDataGrid
                    foreach (DataRowView selectedRow in scheduleDataGrid.SelectedItems)
                    {
                        string rowId = Convert.ToString(selectedRow["inforID"]); // Assuming 'ID' is the column name for IDs
                        object balance = selectedRow["ctoBalance"]; // Assuming 'Balance' is the column name for balance                     

                        // Initialize the firstId or compare rowId with firstId
                        if (firstId == null)
                        {
                            firstId = rowId; // Set the first ID for future comparisons
                        }
                        else if (rowId != firstId) // Check if the current row's ID matches the first ID
                        {
                            allRowsValid = false;
                            break;
                        }
                        //if (!ids.Add(rowId) || balance == null || balance == DBNull.Value)
                        //{
                        //    allRowsValid = false;

                        //    break;
                        //}
                        if (balance == null || balance == DBNull.Value)
                        {
                            allRowsValid = false;
                            break;
                        }
                        selectedRows.Add(selectedRow);


                    }
                    if (allRowsValid)
                    {
                        // Pass the selected rows to the useCto window
                        useCto useCtoWindow = new useCto();
                        useCtoWindow.LoadSelectedSchedule(selectedRows);
                        useCtoWindow.Closed += UseCtoWindow_Closed;
                        useCtoWindow.ShowDialog();
                        

                    }
                    else
                    {
                        MessageBox.Show("All selected rows must have the same ID and must have CTO balances.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    //if (allRowsValid)
                    //{
                    //    // Pass the selected rows to the useCto window
                    //    useCto useCtoWindow = new useCto();
                    //    useCtoWindow.LoadSelectedSchedule(selectedRows);
                    //    useCtoWindow.Show();
                    //}
                    //else
                    //{
                    //    MessageBox.Show("Please select rows with unique IDs and non-null balances.");
                    //}
                   
                    
                }
                        
                else
                {
                    MessageBox.Show("No rows selected.");
                }
            
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }

            
        }
        private void UseCtoWindow_Closed(object sender, EventArgs e)
        {
            // This method gets called when the useCto window is closed
            if (cbxEmployee.SelectedItem != null)
            {
                LoadEmployeeQuery();  // Filter data when a new employee is selected
                LoadCtoEmployeeQuery();
            }
            else
            {
                LoadScheduleData();
                LoadCTOuseData();
            }
        }
        private void PopulateTaskComboBox()
        {
            try
            {
                // Fetch data from the Employee table
                List<string> task = GetDataFromTask();

                // Check if 'allEmployees' is null before binding to the ComboBox
                if (task != null)
                {
                    cbxFilterTask.ItemsSource = task;
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
        private List<string> GetDataFromTask()
        {
            // Create a list to store employee names
            List<string> task = new List<string>();

            try
            {
                // Get connection from DataConnection
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    // Define the Access query to select first names (fName) and last names (lName) from the Employee table
                    string query = "SELECT taskID, taskName FROM Task";

                    // Create a command object with the query and connection
                    OleDbCommand command = new OleDbCommand(query, connection);

                    // Open the connection to the database
                    connection.Open();

                    // Execute the command and retrieve data using a data reader
                    OleDbDataReader reader = command.ExecuteReader();

                    // Iterate through the data reader to read each row
                    while (reader.Read())
                    {
                        // Check if the fName and lName columns contain non-null values
                        if (!reader.IsDBNull(reader.GetOrdinal("taskName")))
                        {
                            // Concatenate the first name and last name to form the full name
                            string taskName = $"{reader["taskName"]}";

                            // Add the full name to the list of employees
                            task.Add(taskName);
                        }
                    }

                    // Close the data reader
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                // Display an error message if an exception occurs
                MessageBox.Show("Error: " + ex.Message);
            }

            // Return the list of employee names retrieved from the database
            return task;
        }
        private string GetTaskID(string taskName)
        {
            string? taskID = null; // Initialize taskId to null

            try
            {
                using (OleDbConnection connection = dataConnection.GetConnection()) // Create a connection using DataConnection
                {
                    string query = "SELECT taskID FROM Task WHERE taskName = ?"; // SQL query to retrieve task ID based on task name
                    using (OleDbCommand command = new OleDbCommand(query, connection)) // Create a command with the query and connection
                    {
                        command.Parameters.AddWithValue("@taskName", taskName); // Add parameter for task name

                        connection.Open(); // Open the connection
                        object? result = command.ExecuteScalar(); // Execute the query and get the result

                        if (result != null) // Check if the result is not null
                        {
                            taskID = result.ToString(); // Assign the task ID to taskId
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving role ID: " + ex.Message); // Display error message if an exception occurs
            }

            return taskID ?? throw new Exception("Task ID not found."); // Return taskId if not null, otherwise throw an exception
        }

        private void scheduleDataGrid_AutoGeneratedColumns(object sender, EventArgs e)
        {
            scheduleDataGrid.Columns[0].Visibility = Visibility.Collapsed;
            scheduleDataGrid.Columns[0].Header = "Schedule ID";
            scheduleDataGrid.Columns[1].Header = "Infor ID";
            scheduleDataGrid.Columns[1].Width = 75;
            scheduleDataGrid.Columns[2].Header = "First Name";
            scheduleDataGrid.Columns[2].Width = 165;
            scheduleDataGrid.Columns[3].Header = "Last Name";
            scheduleDataGrid.Columns[3].Width = 165;
            scheduleDataGrid.Columns[4].Header = "Task";
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

        private void cbxFilterTask_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            taskFilter = cbxFilterTask.SelectedItem?.ToString() ?? "";
            if (cbxEmployee.SelectedItem != null)
            {
                LoadEmployeeQuery();
                LoadCtoEmployeeQuery();
            }
            else
            {
                LoadScheduleData();
                LoadCTOuseData();
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            showallChkBox.IsChecked = true;
            cbxEmployee.SelectedIndex = -1;
            //cbxEmployee.IsEnabled = false;
            cbxEmployee.Text = "Employee";
            monthPicker.Text = "";
            cbxFilterTask.Text = "Filter by Task";
        }
    }
}