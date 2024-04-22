using System.Data;
using System.Data.OleDb;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CTOTracker.View
{

    public partial class ScheduleView : UserControl
    {
        private DataConnection dataConnection;
        private List<string> allEmployees; // Store all employee names
        private List<string> filteredEmployees; //store filtered employee

        public ScheduleView()
        {
            InitializeComponent();
            dataConnection = new DataConnection();
            employeeNameTextBox.TextChanged += EmployeeNameTextBox_TextChanged;
            LoadScheduleData();
            //LoadCTOuseData();
            //PopulateEmployeeComboBox();
            //cbxEmployee.SelectionChanged += cbxEmployee_SelectionChanged;
        }


        private void LoadScheduleData(string employeeName = null)
        {
            try
            {
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    string query = "SELECT Schedule.schedID, Employee.inforID, Employee.fName, Employee.lName, Task.taskName, plannedStart, plannedEnd, timeIn, timeOut, completed, ctoEarned, ctoUsed, ctoBalance FROM (Schedule LEFT JOIN  Employee ON Schedule.empID = Employee.empID) LEFT JOIN Task ON Schedule.taskID = Task.taskID WHERE ctoBalance > 0.0 OR ctoBalance Is Null;";

                    if (!string.IsNullOrEmpty(employeeName))
                    {
                        query += " WHERE Employee.fName LIKE '%' OR Employee.lName LIKE '%' OR Task.taskName LIKE + '%'";
                    }

                    OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
                    if (!string.IsNullOrEmpty(employeeName))
                    {
                        adapter.SelectCommand.Parameters.Add(employeeName);
                    }

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
/*        private void LoadCTOuseData()
        {
            try
            {
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    string query = "SELECT Schedule.schedID, Employee.inforID, Employee.fName, Employee.lName, Task.taskName, plannedStart, plannedEnd, timeIn, timeOut, ctoEarned, ctoUsed, ctoBalance, completed FROM (Schedule LEFT JOIN  Employee ON Schedule.empID = Employee.empID) LEFT JOIN Task ON Schedule.taskID = Task.taskID WHERE ctoUsed > 0.0;";



        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void LoadEmployeeQuery(string empID)
        { 
            try
            {
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    string query = "SELECT Schedule.schedID, Employee.inforID, Employee.fName, Employee.lName, Task.taskName, plannedStart, plannedEnd, timeIn, timeOut, ctoEarned, ctoUsed, ctoBalance, completed FROM (Schedule LEFT JOIN  Employee ON Schedule.empID = Employee.empID) LEFT JOIN Task ON Schedule.taskID = Task.taskID WHERE Employee.empID = ?;";
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
        }*/

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

                
                LoadScheduleData();
                //LoadCTOuseData();
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
        }

        private void EmployeeNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = employeeNameTextBox.Text.Trim();

            // If the search text is empty, load all data
            if (string.IsNullOrEmpty(searchText))
            {
                LoadScheduleData();
                return;
            }
            else
            {

            }
            //Otherwise, filter the data based on the entered initial
            //string initial = searchText.Substring(0, 1); // Assuming you're filtering by the first character
            LoadScheduleDataByInitial(searchText);
        }

        private void LoadScheduleDataByInitial(string initial)
        {
            try
            {
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    string query = "SELECT Schedule.schedID, Employee.inforID, Employee.fName, Employee.lName, Task.taskName, plannedStart, plannedEnd, timeIn, timeOut, ctoEarned, ctoUsed, ctoBalance FROM (Schedule LEFT JOIN  Employee ON Schedule.empID = Employee.empID) LEFT JOIN Task ON Schedule.taskID = Task.taskID WHERE Employee.fName LIKE @Initial + '%' OR Employee.lName LIKE @Initial + '%' OR Task.taskName LIKE @Initial + '%'";

                    OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
                    adapter.SelectCommand.Parameters.AddWithValue("@Initial", initial);
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

        private void btnUseCtoUsed_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check if any rows are selected in the scheduleDataGrid
                if (scheduleDataGrid.SelectedItems.Count > 0)
                {
                    // Create a list to hold selected schedule data
                    List<DataRowView> selectedRows = new List<DataRowView>();

                    // Iterate through each selected row in the scheduleDataGrid
                    foreach (DataRowView selectedRow in scheduleDataGrid.SelectedItems)
                    {
                        selectedRows.Add(selectedRow);
                    }

                    // Pass the selected rows to the useCto window
                    useCto useCtoWindow = new useCto();
                    useCtoWindow.LoadSelectedSchedule(selectedRows);
                    useCtoWindow.Show();
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




    }
}