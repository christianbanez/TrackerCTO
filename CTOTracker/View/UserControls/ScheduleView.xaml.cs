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

        public ScheduleView()
        {
            InitializeComponent();
            dataConnection = new DataConnection();
            employeeNameTextBox.TextChanged += EmployeeNameTextBox_TextChanged;
            LoadScheduleData();
        }


        private void LoadScheduleData(string employeeName = null)
        {
            try
            {
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    string query = "SELECT Schedule.schedID, Employee.inforID, Employee.fName, Employee.lName, Task.taskName, plannedStart, plannedEnd, timeIn, timeOut, ctoEarned, ctoUsed, ctoBalance FROM (Schedule LEFT JOIN  Employee ON Schedule.empID = Employee.empID) LEFT JOIN Task ON Schedule.taskID = Task.taskID";

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



        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void DataGrid_AutoGenerateColumns(object sender, EventArgs e)
        {
            scheduleDataGrid.Columns[0].Header = "Infor ID";
            scheduleDataGrid.Columns[1].Header = "First Name";
            scheduleDataGrid.Columns[2].Header = "Last Name";
            scheduleDataGrid.Columns[3].Header = "Task Name";
            scheduleDataGrid.Columns[4].Header = "Planned Start Date";
            scheduleDataGrid.Columns[5].Header = "Planned End Date";
            scheduleDataGrid.Columns[6].Header = "Time In";
            scheduleDataGrid.Columns[7].Header = "Time Out";
            scheduleDataGrid.Columns[8].Header = "CTO Earned";
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
 
                LoadScheduleData();
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