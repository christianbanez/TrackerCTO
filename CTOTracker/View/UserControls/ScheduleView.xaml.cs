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
            LoadScheduleData();
            PopulateEmployeeComboBox();
            cbxEmployee.SelectionChanged += cbxEmployee_SelectionChanged;
        }

        private void LoadScheduleData()
        {
            try
            {
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    string query = "SELECT Schedule.schedID, Employee.inforID, Employee.fName, Employee.lName, Task.taskName, plannedStart, plannedEnd, timeIn, timeOut, ctoEarned, ctoUsed, ctoBalance, completed FROM (Schedule LEFT JOIN  Employee ON Schedule.empID = Employee.empID) LEFT JOIN Task ON Schedule.taskID = Task.taskID;";

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

        private void LoadEmployeeQuery(string employeeName)
        { 
            try
            {
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    string query = "SELECT Schedule.schedID, Employee.inforID, Employee.fName, Employee.lName, Task.taskName, plannedStart, plannedEnd, timeIn, timeOut, ctoEarned, ctoUsed, ctoBalance, completed FROM (Schedule LEFT JOIN  Employee ON Schedule.empID = Employee.empID) LEFT JOIN Task ON Schedule.taskID = Task.taskID WHERE Employee.fName  & ' ' & Employee.lName = ?;";
                    using (OleDbCommand command = new OleDbCommand(query, connection)) // Create a command with the query and connection
                    {
                        command.Parameters.AddWithValue("@employeeName", employeeName);
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
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Instantiate an instance of the AddTask window
            AddTask addTaskWindow = new AddTask();

            addTaskWindow.SAVE.Visibility = Visibility.Collapsed;
            addTaskWindow.schedIDTextBox.Visibility = Visibility.Collapsed;
            // Show the AddTask window
            addTaskWindow.ShowDialog();
            LoadScheduleData();
        }

        private void DataGrid_AutoGenerateColumns(object sender, EventArgs e)
        {
            scheduleDataGrid.Columns[0].Visibility = Visibility.Collapsed;
            scheduleDataGrid.Columns[0].Header = "Schedule ID";
            scheduleDataGrid.Columns[1].Header = "Infor ID";
            scheduleDataGrid.Columns[2].Header = "First Name";
            scheduleDataGrid.Columns[3].Header = "Last Name";
            scheduleDataGrid.Columns[4].Header = "Task Name";
            scheduleDataGrid.Columns[5].Header = "Planned Start Date";
            scheduleDataGrid.Columns[6].Header = "Planned End Date";
            scheduleDataGrid.Columns[7].Header = "Time In";
            scheduleDataGrid.Columns[8].Header = "Time Out";
            scheduleDataGrid.Columns[9].Header = "CTO Earned";
            scheduleDataGrid.Columns[10].Header = "CTO Used";
            scheduleDataGrid.Columns[11].Header = "CTO Balance";
            scheduleDataGrid.Columns[12].Header = "Completed";
            
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
                // Show the AddTask form
                addTaskWindow.ShowDialog();
                LoadScheduleData();
            }
        }

        private void btnAssignTask_Click(object sender, RoutedEventArgs e)
        {
            // Instantiate an instance of the AddTask window
            AddTask addTaskWindow = new AddTask();

            addTaskWindow.SAVE.Visibility = Visibility.Collapsed;
            addTaskWindow.schedIDTextBox.Visibility = Visibility.Collapsed;
            // Show the AddTask window
            addTaskWindow.ShowDialog();
            LoadScheduleData();
        }

        private void cbxEmployee_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedEmployee = cbxEmployee.SelectedItem?.ToString() ?? string.Empty;

            //string employeeId = GetEmployeeId(selectedEmployee);
            if (cbxEmployee.SelectedItem != null)
            {
                if (cbxEmployee.SelectedItem.ToString() == selectedEmployee)
                {
                    LoadEmployeeQuery(selectedEmployee);
                }
                
            }
            else
            {
                MessageBox.Show("No employees found.");
            }
        }
    }
}