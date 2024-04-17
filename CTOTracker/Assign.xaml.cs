using System.Data.OleDb;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CTOTracker
{
    /// <summary>
    /// Interaction logic for AddTask.xaml
    /// </summary>
    public partial class AddTask : Window
    {
        private List<string> allEmployees; // Store all employee names
        private List<string> filteredEmployees; // Store filtered employee names
        private List<string> allTask;
        private List<string> filteredTask;
        private DataConnection dataConnection; // Declare a field to hold the DataConnection object
        public AddTask()
        {
            InitializeComponent();
            dataConnection = new DataConnection(); // Instantiate the DataConnection object
            allEmployees = new List<string>();
            filteredEmployees = new List<string>();
            allTask = new List<string>();
            filteredTask = new List<string>();
            //startTimeTextBox.Text = "09:00 AM";
            //endTimeTextBox.Text = "05:00 PM";
            Employee_Cmbox.IsEditable = true; // Allow editing of ComboBox text
            PopulateEmployeeComboBox();
            PopulateTaskComboBox();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            AttachEventHandlers();
        }

        private void AttachEventHandlers()
        {
            // Find controls in the template
            var closeButton = (Button)this.Template.FindName("CloseButton", this);
            var titleBar = (Border)this.Template.FindName("TitleBar", this);

            // Attach event handlers
            if (closeButton != null)
            {
                closeButton.Click += CloseButton_Click;
            }

            if (titleBar != null)
            {
                titleBar.MouseLeftButtonDown += TitleBar_MouseLeftButtonDown;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        // Method to populate AddTask form with selected data
        public void PopulateWithData(string fullName, string taskName, DateTime startDate, DateTime endDate, string timeIn, string timeOut, int schedID)
        {
            // Populate UI controls with selected data
            Employee_Cmbox.Text = fullName;
            Task_Cmbox.Text = taskName;
            startDatePicker.SelectedDate = startDate;
            endDatePicker.SelectedDate = endDate;
            schedIDTextBox.Text = schedID.ToString(); // Set the schedID in the schedIDTextBox

            showTimeCheckBox.IsChecked = !string.IsNullOrEmpty(timeIn) && !string.IsNullOrEmpty(timeOut);

            // Extract only time component from the selected time strings
            if (!string.IsNullOrEmpty(timeIn))
            {
                startTimeTextBox.Text = DateTime.Parse(timeIn).ToString("hh:mm tt");
            }

            if (!string.IsNullOrEmpty(timeOut))
            {
                endTimeTextBox.Text = DateTime.Parse(timeOut).ToString("hh:mm tt");
            }
        }

        public double CalculateCtoEarned(DateTime timeIn, DateTime timeOut)
        {
            // Calculate the duration worked
            TimeSpan duration = timeOut - timeIn;

            // Define thresholds for ctoEarned
            TimeSpan eightHours = TimeSpan.FromHours(8);
            TimeSpan fourHours = TimeSpan.FromHours(4);

            // Compare the duration with thresholds
            if (duration >= eightHours)
            {
                return 1.0; // Full day (8+ hours)
            }
            else if (duration >= fourHours)
            {
                return 0.5; // Half day (4+ hours)
            }
            else
            {
                return 0.0; // Less than 4 hours
            }
        }

        //DatePicker Handler
        private void DatePicker_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Suppress key presses that would result in text input
            e.Handled = true;
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
                    Employee_Cmbox.ItemsSource = allEmployees;
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

        private void PopulateTaskComboBox()
        {
            try
            {
                // Clear existing items from the combo box
                Task_Cmbox.Items.Clear();

                // Fetch data from the Task table
                allTask = GetDataFromTaskTable();

                // Check if 'allTask' is null before binding to the ComboBox
                if (allTask != null)
                {
                    // Bind the data to Task ComboBox
                    Task_Cmbox.ItemsSource = allTask;
                }
                else
                {
                    // Handle the case when 'allTask' is null
                    MessageBox.Show("No tasks found.");
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

        private void Employee_Cmbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Clear the filtered list
            filteredEmployees.Clear();

            string searchText = Employee_Cmbox.Text.ToLower();

            // Filter the items in the ComboBox based on the entered text
            foreach (string employee in allEmployees)
            {
                if (employee.ToLower().Contains(searchText))
                {
                    filteredEmployees.Add(employee);
                }
            }

            // Update the ComboBox items source with the filtered list
            Employee_Cmbox.ItemsSource = filteredEmployees;

            // Open the dropdown
            Employee_Cmbox.IsDropDownOpen = true;
        }

        private List<string> GetDataFromTaskTable()
        {
            List<string> tasks = new List<string>();

            try
            {
                // Get connection from DataConnection
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    // Define the SQL query to select task names from the Task table
                    string query = "SELECT TaskName FROM Task";

                    // Create a command object with the query and connection
                    OleDbCommand command = new OleDbCommand(query, connection);

                    // Open the connection to the database
                    connection.Open();

                    // Execute the command and retrieve data using a data reader
                    OleDbDataReader reader = command.ExecuteReader();

                    // Iterate through the data reader to read each row
                    while (reader.Read())
                    {
                        // Check if the TaskName column value is not null
                        if (!reader.IsDBNull(reader.GetOrdinal("TaskName")))
                        {
                            // Retrieve the TaskName value from the current row
                            string? taskName = reader["TaskName"]?.ToString();

                            // Check if taskName is not null before adding it to the list
                            if (taskName != null)
                            {
                                tasks.Add(taskName);
                            }
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

            // Return the list of task names retrieved from the database
            return tasks;
        }

        //Add Schedule button

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

            return employeeId ?? throw new Exception("Employee ID not found."); // Return employeeId if not null, otherwise throw an exception
        }

        private string GetTaskId(string taskName)
        {
            string? taskId = null; // Initialize taskId to null

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
                            taskId = result.ToString(); // Assign the task ID to taskId
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving task ID: " + ex.Message); // Display error message if an exception occurs
            }

            return taskId ?? throw new Exception("Task ID not found."); // Return taskId if not null, otherwise throw an exception
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //declare month before to blackout in datetimepicker
            //DateTime oneMonthBefore = DateTime.Today.AddMonths(-1);

            try
            {
                // Display a confirmation dialog
                MessageBoxResult result = MessageBox.Show("Are you sure you want to add this task?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                // Check the user's response
                if (result == MessageBoxResult.Yes)
                {
                    // Get selected employee name from ComboBox
                    string selectedEmployee = Employee_Cmbox.SelectedItem?.ToString() ?? string.Empty;

                    // Get selected task name from ComboBox
                    string selectedTask = Task_Cmbox.SelectedItem?.ToString() ?? string.Empty;

                    if (string.IsNullOrEmpty(selectedEmployee) || string.IsNullOrEmpty(selectedTask))
                    {
                        MessageBox.Show("Please select an employee and a task.");
                        return;
                    }
                    string employeeId = GetEmployeeId(selectedEmployee);
                    string taskId = GetTaskId(selectedTask);

                    // Add the blackout date
                    //startDatePicker.BlackoutDates.Add(new CalendarDateRange(DateTime.MinValue, oneMonthBefore));
                    //endDatePicker.BlackoutDates.Add(new CalendarDateRange(DateTime.MinValue, oneMonthBefore));

                    // Get selected dates from date pickers
                    DateTime startDate = startDatePicker.SelectedDate ?? DateTime.Now;
                    DateTime endDate = endDatePicker.SelectedDate ?? DateTime.Now;

                    // Get selected times from time pickers (if checkbox is checked)
                    string timeIn = (showTimeCheckBox.IsChecked == true) ? startTimeTextBox.Text : string.Empty;
                    string timeOut = (showTimeCheckBox.IsChecked == true) ? endTimeTextBox.Text : string.Empty;

                    // Insert data into Schedule table
                    InsertIntoSchedule(employeeId, taskId, startDate, endDate, timeIn, timeOut);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void InsertIntoSchedule(string employeeId, string taskId, DateTime startDate, DateTime endDate, string timeIn, string timeOut)
        {
            using (OleDbConnection connection = dataConnection.GetConnection())
            {
                try
                {
                    // Validate that planned start date is not greater than planned end date
                    if (startDate > endDate)
                    {
                        MessageBox.Show("Planned start date cannot be greater than planned end date.");
                        return;
                    }

                    string query = "INSERT INTO Schedule (empID, taskID, plannedStart, plannedEnd, timeIn, timeOut, completed, ctoEarned, ctoBalance) " +
                                   "VALUES (@empID, @taskID, @plannedStart, @plannedEnd, @timeIn, @timeOut, @completed, @ctoEarned, @ctoBalance)";

                    using (OleDbCommand command = new OleDbCommand(query, connection))
                    {
                        // Add parameters to the command
                        command.Parameters.AddWithValue("@empID", employeeId);
                        command.Parameters.AddWithValue("@taskID", taskId);
                        command.Parameters.AddWithValue("@plannedStart", startDate);
                        command.Parameters.AddWithValue("@plannedEnd", endDate);

                        // Concatenate the date portion of the start date with the timeIn value
                        if (!string.IsNullOrEmpty(timeIn) && !string.IsNullOrEmpty(timeOut))
                        {
                            DateTime timeInDateTime = DateTime.ParseExact(timeIn, "hh:mm tt", CultureInfo.InvariantCulture);
                            DateTime dateTimeInWithDate = startDate.Date + timeInDateTime.TimeOfDay;
                            command.Parameters.AddWithValue("@timeIn", dateTimeInWithDate);

                            DateTime timeOutDateTime = DateTime.ParseExact(timeOut, "hh:mm tt", CultureInfo.InvariantCulture);
                            DateTime dateTimeOutWithDate = endDate.Date + timeOutDateTime.TimeOfDay;
                            command.Parameters.AddWithValue("@timeOut", dateTimeOutWithDate);
                            command.Parameters.AddWithValue("@completed", -1);
                            double ctoEarned = CalculateCtoEarned(dateTimeInWithDate, dateTimeOutWithDate);
                            command.Parameters.AddWithValue("@ctoEarned", ctoEarned);
                            command.Parameters.AddWithValue("@ctoBalance", ctoEarned);
                            
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@timeIn", DBNull.Value);
                            command.Parameters.AddWithValue("@timeOut", DBNull.Value);
                            command.Parameters.AddWithValue("@completed", DBNull.Value);
                            command.Parameters.AddWithValue("@ctoEarned", DBNull.Value);
                            command.Parameters.AddWithValue("@ctoBalance", DBNull.Value);
                        }

                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();
                        MessageBox.Show("Schedule has been added!");
                        connection.Close();
                        this.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error inserting into Schedule table: " + ex.Message);
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SAVE_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string selectedEmployee = Employee_Cmbox.SelectedItem?.ToString() ?? string.Empty;
                string selectedTask = Task_Cmbox.SelectedItem?.ToString() ?? string.Empty;

                if (string.IsNullOrEmpty(selectedEmployee) || string.IsNullOrEmpty(selectedTask))
                {
                    MessageBox.Show("Please select an employee and a task.");
                    return;
                }

                string employeeId = GetEmployeeId(selectedEmployee);
                string taskId = GetTaskId(selectedTask);

                DateTime startDate = startDatePicker.SelectedDate ?? DateTime.Now;
                DateTime endDate = endDatePicker.SelectedDate ?? DateTime.Now;

                string timeIn = (showTimeCheckBox.IsChecked == true) ? startTimeTextBox.Text : string.Empty;
                string timeOut = (showTimeCheckBox.IsChecked == true) ? endTimeTextBox.Text : string.Empty;

                // Assuming schedID is available from the UI (e.g., schedIDTextBox)
                int schedID = Convert.ToInt32(schedIDTextBox.Text); // Adjust conversion based on data type

                // Check if schedID is valid (non-zero) to determine if it's an update operation
                if (schedID != 0)
                {
                    UpdateSchedule(employeeId, taskId, startDate, endDate, timeIn, timeOut, schedID); // Pass schedID to UpdateSchedule
                }
                else
                {
                    // If schedID is zero, it means it's a new schedule entry
                    InsertIntoSchedule(employeeId, taskId, startDate, endDate, timeIn, timeOut);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void UpdateSchedule(string employeeId, string taskId, DateTime startDate, DateTime endDate, string timeIn, string timeOut, int schedID)
        {
            using (OleDbConnection connection = dataConnection.GetConnection())
            {
                try
                {
                    if (startDate > endDate)
                    {
                        MessageBox.Show("Planned start date cannot be greater than planned end date.");
                        return;
                    }

                    // Ask for confirmation before updating
                    MessageBoxResult result = MessageBox.Show("Are you sure you want to update this schedule?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        string query = "UPDATE Schedule SET empID = @empID, taskID = @taskID, plannedStart = @plannedStart, plannedEnd = @plannedEnd, timeIn = @timeIn, timeOut = @timeOut, completed = @completed, ctoEarned = @ctoEarned, ctoBalance = @ctoBalance WHERE schedID = @schedID";

                        using (OleDbCommand command = new OleDbCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@empID", employeeId);
                            command.Parameters.AddWithValue("@taskID", taskId);
                            command.Parameters.AddWithValue("@plannedStart", startDate);
                            command.Parameters.AddWithValue("@plannedEnd", endDate);

                            if (!string.IsNullOrEmpty(timeIn) && !string.IsNullOrEmpty(timeOut))
                            {
                                DateTime timeInDateTime = DateTime.ParseExact(timeIn, "hh:mm tt", CultureInfo.InvariantCulture);
                                DateTime dateTimeInWithDate = startDate.Date + timeInDateTime.TimeOfDay;
                                command.Parameters.AddWithValue("@timeIn", dateTimeInWithDate);

                                DateTime timeOutDateTime = DateTime.ParseExact(timeOut, "hh:mm tt", CultureInfo.InvariantCulture);
                                DateTime dateTimeOutWithDate = endDate.Date + timeOutDateTime.TimeOfDay;
                                command.Parameters.AddWithValue("@timeOut", dateTimeOutWithDate);
                                command.Parameters.AddWithValue("@completed", -1);
                                double ctoEarned = CalculateCtoEarned(dateTimeInWithDate, dateTimeOutWithDate);
                                command.Parameters.AddWithValue("@ctoEarned", ctoEarned);
                                command.Parameters.AddWithValue("@ctoBalance", ctoEarned);
                            }
                            else
                            {
                                command.Parameters.AddWithValue("@timeIn", DBNull.Value);
                                command.Parameters.AddWithValue("@timeOut", DBNull.Value);
                                command.Parameters.AddWithValue("@completed", DBNull.Value);
                                command.Parameters.AddWithValue("@ctoEarned", DBNull.Value);
                                command.Parameters.AddWithValue("@ctoBalance", DBNull.Value);
                            }

                            command.Parameters.AddWithValue("@schedID", schedID);

                            connection.Open();
                            int rowsAffected = command.ExecuteNonQuery();
                            MessageBox.Show("Schedule has been updated!");
                            connection.Close();
                            this.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error updating Schedule table: " + ex.Message);
                }
            }
        }

        private void showTimeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Show the time pickers panel
            addTime.Visibility = Visibility.Visible;
        }

        private void showTimeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // Hide the time pickers panel
            addTime.Visibility = Visibility.Collapsed;
        }
    }
}