using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Globalization;
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
using System.Windows.Shapes;

namespace CTOTracker
{
    /// <summary>
    /// Interaction logic for AddTask.xaml
    /// </summary>
    public partial class AddTask : Window
    {
        private DataConnection dataConnection; // Declare a field to hold the DataConnection object

        public AddTask()
        {
            InitializeComponent();
            dataConnection = new DataConnection(); // Instantiate the DataConnection object
            startTimeTextBox.Text = "09:00 AM";
            endTimeTextBox.Text = "05:00 PM";
            PopulateEmployeeComboBox();
            PopulateTaskComboBox();
        }

        private void PopulateEmployeeComboBox()
        {
            try
            {
                // Fetch data from the Employee table
                List<string> employees = GetDataFromEmployeeTable();

                // Check if 'employees' is null before binding to the ComboBox
                if (employees != null)
                {
                    Employee_Cmbox.ItemsSource = employees;
                }
                else
                {
                    // Handle the case when 'employees' is null
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
                // Fetch data from the Task table
                List<string> tasks = GetDataFromTaskTable();

                // Check if 'tasks' is null before binding to the ComboBox
                if (tasks != null)
                {
                    // Bind the data to Task ComboBox
                    Task_Cmbox.ItemsSource = tasks;

                    // Select the first item in the Task ComboBox
                    Task_Cmbox.SelectedItem = tasks.Count > 0 ? tasks[0] : null;
                }
                else
                {
                    // Handle the case when 'tasks' is null
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
                    OleDbCommand command = new OleDbCommand(query, connection);

                    // Open the connection to the database
                    connection.Open();

                    // Execute the command and retrieve data using a data reader
                    OleDbDataReader reader = command.ExecuteReader();

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
            return employees;
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

        //Add Button
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
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

                // Retrieve employee ID and task ID from database based on selected names
                string employeeId = GetEmployeeId(selectedEmployee);
                string taskId = GetTaskId(selectedTask);

                // Get selected dates from date pickers
                DateTime startDate = startDatePicker.SelectedDate ?? DateTime.Now;
                DateTime endDate = endDatePicker.SelectedDate ?? DateTime.Now;

                // Get selected times from time pickers (if checkbox is checked)
                string timeIn = (showTimeCheckBox.IsChecked == true) ? startTimeTextBox.Text : string.Empty;
                string timeOut = (showTimeCheckBox.IsChecked == true) ? endTimeTextBox.Text : string.Empty;

                // Insert data into Schedule table
                InsertIntoSchedule(employeeId, taskId, startDate, endDate, timeIn, timeOut);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
               
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

            return employeeId ?? throw new Exception("Employee ID not found."); // Return employeeId if not null, otherwise throw an exception
        }




        private string GetTaskId(string taskName)
        {
            string taskId = null; // Initialize taskId to null

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
        private void InsertIntoSchedule(string employeeId, string taskId, DateTime startDate, DateTime endDate, string timeIn, string timeOut)
        {
            using (OleDbConnection connection = dataConnection.GetConnection()) // Create a connection using DataConnection
            {
                try
                {

                    string query = "INSERT INTO Schedule (empID, taskID, plannedStart, plannedEnd, timeIn, timeOut, ctoEarned) " +
                                   "VALUES (@empID, @taskID, @plannedStart, @plannedEnd, @timeIn, @timeOut, @ctoEarned)"; // Define the SQL query

                    using (OleDbCommand command = new OleDbCommand(query, connection)) // Create a command with the query and connection
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

                            double ctoEarned = CalculateCtoEarned(dateTimeInWithDate, dateTimeOutWithDate);
                            command.Parameters.AddWithValue("@ctoEarned", ctoEarned);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@timeIn", DBNull.Value);
                            command.Parameters.AddWithValue("@timeOut", DBNull.Value);
                            command.Parameters.AddWithValue("@ctoEarned", DBNull.Value);
                        }

                        connection.Open(); // Open the connection
                        int rowsAffected = command.ExecuteNonQuery(); // Execute the query and get the number of rows affected
                        MessageBox.Show($"{rowsAffected} row(s) inserted into Schedule table."); // Display a message with the number of rows affected
                    }


                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error inserting into Schedule table: " + ex.Message); // Display error message if an exception occurs
                }
                finally 
                { 
                    connection.Close(); 
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
