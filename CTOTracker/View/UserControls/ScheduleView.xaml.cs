using System.Data;
using System.Data.OleDb;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CTOTracker.View
{
    /// <summary>
    /// Interaction logic for ScheduleView.xaml
    /// </summary>
    public partial class ScheduleView : UserControl
    {
        private DataConnection dataConnection;

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
            LoadScheduleData();
        }

        private void LoadScheduleData()
        {
            try
            {
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    string query = "SELECT Schedule.schedID, Employee.inforID, Employee.fName, Employee.lName, Task.taskName, plannedStart, plannedEnd, timeIn, timeOut, completed, ctoEarned, ctoUsed, ctoBalance FROM (Schedule LEFT JOIN  Employee ON Schedule.empID = Employee.empID) LEFT JOIN Task ON Schedule.taskID = Task.taskID;";

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
            scheduleDataGrid.Columns[5].Header = "Start Date";
            scheduleDataGrid.Columns[6].Header = "End Date";
            scheduleDataGrid.Columns[7].Header = "Time In";
            scheduleDataGrid.Columns[8].Header = "Time Out";
            scheduleDataGrid.Columns[9].Header = "Completed";
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

                LoadScheduleData();
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
            LoadScheduleData();
        }
    }
}