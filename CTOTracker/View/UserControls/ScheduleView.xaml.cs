using Org.BouncyCastle.Ocsp;
using System.Data;
using System.Data.OleDb;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Windows.Threading;
using System.Windows.Data;
using System.Globalization;
//using iTextSharp;
//using iTextSharp.text;
//using iTextSharp.text.pdf;
//using iTextSharp.text.xml;

namespace CTOTracker.View
{
    

    public partial class ScheduleView : UserControl
    {
        private DispatcherTimer checkCompletionTimer;
        private DataConnection dataConnection;
        List<KeyValuePair<string,string>> allEmployees; // Store all employee names
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
            allEmployees = new List<KeyValuePair<string,string>>();
            filteredEmployees = new List<string>();
            
            cbxEmployee.IsEnabled = false;
            LoadScheduleData();
            LoadCTOuseData();
            PopulateEmployeeComboBox();
            //scheduleDataGrid.AutoGeneratingColumn += scheduleDataGrid_AutoGeneratingColumn;
            cbxEmployee.SelectionChanged += cbxEmployee_SelectionChanged;
            cbxFilterTask.SelectionChanged += cbxFilterTask_SelectionChanged;
            PopulateTaskComboBox();
            SetupTimer();
            scheduleDataGrid.SelectionChanged += ScheduleDataGrid_SelectionChanged;
            btnUseCtoUsed.IsEnabled = false;
            showallChkBox.IsChecked = true;

        }

        private void ScheduleDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Check if there's any selected row in the scheduleDataGrid
            if (scheduleDataGrid.SelectedItems.Count > 0)
            {
                // Enable the btnUseCtoUsed button
                btnUseCtoUsed.IsEnabled = true;
            }
            else
            {
                // Disable the btnUseCtoUsed button
                btnUseCtoUsed.IsEnabled = false;
            }
        }
            
        private void LoadScheduleData()
        {
            try
            {
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    string query = "SELECT Schedule.schedID, Employee.inforID, Employee.fName, Employee.lName, Task.taskName, completed, " +
                        "Format(plannedStart, 'MM/dd/yyyy') AS plannedStart, Format(plannedEnd, 'MM/dd/yyyy') AS plannedEnd, " +
                        "timeIn, timeOut, ctoEarned, ctoBalance " +
                        "FROM (Schedule LEFT JOIN  Employee ON Schedule.empID = Employee.empID) " +
                        "LEFT JOIN Task ON Schedule.taskID = Task.taskID";

                    // CTO Balance filter
                    string ctoBalanceFilter = " WHERE (ctoBalance > 0.0 OR ctoBalance IS NULL)";
                    query += ctoBalanceFilter;

                    if (!showallChkBox.IsChecked.HasValue || !showallChkBox.IsChecked.Value)
                    {
                        string employeeId = GetSelectedEmployeeId();
                        if (!string.IsNullOrEmpty(employeeId))
                        {
                            // Append the employee ID filter to the query
                            query += " AND (Employee.empID = ?)";
                        }
                    }

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
                        if (!showallChkBox.IsChecked.HasValue || !showallChkBox.IsChecked.Value)
                        {
                            string employeeId = GetSelectedEmployeeId();
                            if (!string.IsNullOrEmpty(employeeId))
                            {
                                adapter.SelectCommand.Parameters.AddWithValue("@empID", employeeId);
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
        private void LoadCTOuseData()
        {
            try
            {
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    string query = "SELECT Employee.inforID, Employee.fName, Employee.lName, Role.roleName, Task.taskName,  " +
                        "Schedule.ctoEarned, CTOuse.ctoUse, CTOuse.useDesc, Format(CTOuse.dateUsed, 'MM/dd/yyyy') AS dateUsed, Schedule.schedID "+
                        "FROM Task INNER JOIN(Role INNER JOIN (Employee INNER JOIN (Schedule INNER JOIN CTOuse ON Schedule.schedID = CTOuse.schedID)ON "+
                        "Employee.empID = Schedule.empID) ON Role.roleID = Employee.roleID) ON Task.taskID = Schedule.taskID";

                    // CTO Balance filter
                    string ctoBalanceFilter = " WHERE (CTOuse.ctoUse > 0.0)";
                    query += ctoBalanceFilter;

                    if (!showallChkBox.IsChecked.HasValue || !showallChkBox.IsChecked.Value)
                    {
                        string employeeId = GetSelectedEmployeeId();
                        if (!string.IsNullOrEmpty(employeeId))
                        {
                            // Append the employee ID filter to the query
                            query += " AND (Employee.empID = ?)";
                        }
                    }
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
                        if (!showallChkBox.IsChecked.HasValue || !showallChkBox.IsChecked.Value)
                        {
                            string employeeId = GetSelectedEmployeeId();
                            if (!string.IsNullOrEmpty(employeeId))
                            {
                                adapter.SelectCommand.Parameters.AddWithValue("@empID", employeeId);
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
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    string query = "SELECT inforID, fName, lName FROM Employee";
                    using (OleDbCommand command = new OleDbCommand(query, connection))
                    {
                        connection.Open();
                        using (OleDbDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string inforID = reader["inforID"].ToString();
                                string fName = reader.IsDBNull(reader.GetOrdinal("fName")) ? "" : reader["fName"].ToString();
                                string lName = reader.IsDBNull(reader.GetOrdinal("lName")) ? "" : reader["lName"].ToString();
                                string fullName = $"{inforID}: {fName} {lName}"; // Concatenate ID with name
                                ComboBoxItem item = new ComboBoxItem
                                {
                                    Text = fullName,
                                    Value = inforID
                                };
                                cbxEmployee.Items.Add(item);
                            }
                            if (cbxEmployee.Items.Count > 0)
                            {
                                cbxEmployee.SelectedIndex = 0; // This should trigger cbxEmployee_SelectionChanged
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error populating ComboBox: " + ex.Message);
            }
            
        }

        public class ComboBoxItem
        {
            public string Text { get; set; }
            public string Value { get; set; }

            public override string ToString()
            {
                return Text;
            }
        }
        private string GetSelectedEmployeeId()
        {
            try
            {
                if (cbxEmployee.SelectedItem != null)
                {
                    ComboBoxItem selectedItem = cbxEmployee.SelectedItem as ComboBoxItem;
                    if (selectedItem != null)
                    {
                        return selectedItem.Value; // This is the employee ID
                    }
                }
                /*MessageBox.Show("No employee selected or improper ComboBox item.")*/;
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving employee ID: " + ex.Message);
                return null;
            }
        }
        //private string GetEmployeeId(string employeeName)
        //{
        //    string? employeeId = null; // Initialize employeeId to null

        //    try
        //    {
        //        using (OleDbConnection connection = dataConnection.GetConnection()) // Create a connection using DataConnection
        //        {
        //            // Modified query to concatenate fName and lName
        //            string query = "SELECT empID FROM Employee WHERE fName & ' ' & lName = ? AND inforID = ?";

        //            using (OleDbCommand command = new OleDbCommand(query, connection)) // Create a command with the query and connection
        //            {
        //                command.Parameters.AddWithValue("@employeeName", employeeName); // Add parameter for employee name

        //                connection.Open(); // Open the connection
        //                object? result = command.ExecuteScalar(); // Execute the query and get the result

        //                if (result != null) // Check if the result is not null
        //                {
        //                    employeeId = result.ToString(); // Assign the employee ID to employeeId
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Error retrieving employee ID: " + ex.Message); // Display error message if an exception occurs
        //    }

        //    // Return employeeId if not null, otherwise throw an exception

        //    return employeeId ?? throw new Exception("Employee ID not found.");

        //}

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


                    LoadScheduleData();
                    LoadCTOuseData();
                    
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


    
     
            LoadScheduleData();
            LoadCTOuseData();

            PopulateTaskComboBox();

        }

        private void cbxEmployee_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbxEmployee.SelectedItem != null)
            {
                showallChkBox.IsChecked = false;
                LoadScheduleData();
                LoadCTOuseData();
            }
        }

        private void ctoUseDataGrid_AutoGeneratedColumns(object sender, EventArgs e)
        {
            ctoUseDataGrid.Columns[0].Header = "Infor ID";
            ctoUseDataGrid.Columns[1].Header = "First Name";
            ctoUseDataGrid.Columns[2].Header = "Last Name";
            ctoUseDataGrid.Columns[3].Header = "Role Name";
            ctoUseDataGrid.Columns[4].Header = "Task";
            ctoUseDataGrid.Columns[5].Header = "CTO Earned";
            ctoUseDataGrid.Columns[6].Header = "CTO Used";
            ctoUseDataGrid.Columns[7].Header = "Use Description";
            ctoUseDataGrid.Columns[8].Header = "Date Used";
            ctoUseDataGrid.Columns[9].Header = "Schedule ID";
            ctoUseDataGrid.Columns[9].Visibility = Visibility.Collapsed;

        }

        private void showallChecked(object sender, RoutedEventArgs e)
        {
            //LoadEmployeeQuery();
            LoadScheduleData();
            LoadCTOuseData();
            cbxEmployee.Text = "";
            cbxEmployee.IsEnabled = false;  // This should trigger cbxEmployee_SelectionChanged

            //cbxEmployee.IsEnabled = false;
        }

        private void showallUnchecked(object sender, RoutedEventArgs e)
        {
            monthPicker.Text = "";
            cbxEmployee.Text = "Employee";
            cbxEmployee.IsEnabled = true; // Re-enable ComboBox

            cbxEmployee.SelectedIndex = -1; // Reset to the first item or to a default state

            cbxFilterTask.Text = "Filter by Task";
            LoadCTOuseData();
            LoadScheduleData();
        }

        private void monthPicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {

            LoadScheduleData();
            LoadCTOuseData();
           
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

            LoadScheduleData();
            LoadCTOuseData();
            
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
        //private void scheduleDataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        //{
        //    if (e.PropertyName == "Completed")
        //    {
        //        DataGridTextColumn column = e.Column as DataGridTextColumn;
        //        if (column != null)
        //        {
        //            column.Binding = new Binding(e.PropertyName)
        //            {
        //                Converter = (BooleanToSymbolConverter)this.Resources["BooleanToSymbolConverter"]
        //            };
        //        }
        //    }
        //}
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
            scheduleDataGrid.Columns[11].Header = "CTO Balance";

            DataGridTextColumn timeInColumn = scheduleDataGrid.Columns[8] as DataGridTextColumn;
            DataGridTextColumn timeOutColumn = scheduleDataGrid.Columns[9] as DataGridTextColumn;

            if (timeInColumn != null && timeOutColumn != null)
            {
                timeInColumn.Binding.StringFormat = "h:mm tt"; // Format time in 12-hour format with AM/PM
                timeOutColumn.Binding.StringFormat = "h:mm tt"; // Format time in 12-hour format with AM/PM
            }
        }

        private void cbxFilterTask_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            taskFilter = cbxFilterTask.SelectedItem?.ToString() ?? "";
     
            LoadScheduleData();
            LoadCTOuseData();
            
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            showallChkBox.IsChecked = true;

            cbxEmployee.SelectedIndex = -1; // This should trigger cbxEmployee_SelectionChange
            //cbxEmployee.IsEnabled = false;
            cbxEmployee.Text = "Employee";
            monthPicker.Text = "";
            cbxFilterTask.Text = "Filter by Task";
        }

        ///- Check Completed if past timeout date
        private void SetupTimer()
        {
            checkCompletionTimer = new DispatcherTimer();
            checkCompletionTimer.Interval = TimeSpan.FromSeconds(10);
            checkCompletionTimer.Tick += CheckCompletionStatus;
            checkCompletionTimer.Start();
        }
        private void CheckCompletionStatus(object sender, EventArgs e)
        {
            LoadScheduleData();  // Re-load and check data
            UpdateCompletedStatuses();  // Method to check and update the completion status
        }
        private void UpdateCompletedStatuses()
        {
            bool updatesMade = false;
            foreach (DataRow row in ((DataView)scheduleDataGrid.ItemsSource).Table.Rows)
            {
                if (!Convert.ToBoolean(row["completed"]) && IsPastTimeout(row["timeOut"].ToString()))
                {
                    row["completed"] = true;  // Update the row's completed status
                    UpdateDatabase(row["schedID"].ToString(), true);  // Assume a method to update the database
                    updatesMade = true;
                }
            }

            if (updatesMade)
            {
                LoadScheduleData(); // Reload data to reflect changes
            }
        }

        private bool IsPastTimeout(string timeoutValue)
        {
            // Check if the input string is null or empty.
            if (string.IsNullOrEmpty(timeoutValue))
            {
                return false; // Optionally, consider a default behavior if timeout is not set.
            }

            // Attempt to parse the timeoutValue to a DateTime object.
            if (DateTime.TryParse(timeoutValue, out DateTime timeoutDate))
            {
                // Return true if the current date/time is greater than the parsed timeout date/time.
                // This indicates that the timeout date/time has already passed.
                return DateTime.Now > timeoutDate;
            }
            else
            {
                // Return false if the date/time could not be parsed.
                return false;
            }
        }

        private void UpdateDatabase(string scheduleId, bool completed)
        {
            using (OleDbConnection connection = dataConnection.GetConnection())
            {
                connection.Open();
                string updateQuery = "UPDATE Schedule SET completed = ? WHERE schedID = ?";
                using (OleDbCommand command = new OleDbCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("?", completed);
                    command.Parameters.AddWithValue("?", scheduleId);
                    command.ExecuteNonQuery();
                }
            }
        }

        
    }
}