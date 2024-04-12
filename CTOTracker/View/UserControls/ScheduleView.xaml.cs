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
using System.Diagnostics;

namespace CTOTracker.View
{
    /// <summary>
    /// Interaction logic for ScheduleView.xaml
    /// </summary>
    public partial class ScheduleView : UserControl
    {
        private DataConnection dataConnection;
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
                    string query = "SELECT Employee.inforID, Employee.fName, Employee.lName, Task.taskName, plannedStart, plannedEnd, timeIn, timeOut, ctoEarned, ctoUsed, ctoBalance FROM (Schedule LEFT JOIN  Employee ON Schedule.empID = Employee.empID) LEFT JOIN Task ON Schedule.taskID = Task.taskID;";

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
            finally
            {
                dataConnection.GetConnection().Close();
            }
        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Instantiate an instance of the AddTask window
            AddTask addTaskWindow = new AddTask();

            // Show the AddTask window
            addTaskWindow.ShowDialog();
            LoadScheduleData();
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
            scheduleDataGrid.Columns[9].Header = "CTO Used";
            scheduleDataGrid.Columns[10].Header = "CTO Balance";
        }

        private void searchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
               
                ////LoadScheduleData();
                //if (scheduleDataGrid != null)
                //{

                //    string searchText = searchTextBox.Text;
                //    DataView dataView = scheduleDataGrid.ItemsSource as DataView;

                //    if (dataView != null)
                //    {
                //        StringBuilder filter = new StringBuilder();

                //        // Build the filter string using column names from the dataView
                //        for (int i = 0; i < dataView.Table.Columns.Count; i++)
                //        {
                //            string columnName = dataView.Table.Columns[i].ColumnName;
                //            if (i > 0)
                //            {
                //                filter.Append(" OR ");
                //            }
                //            filter.AppendFormat("{0} LIKE '%{1}%'", columnName, searchText);
                //        }

                //        dataView.RowFilter = filter.ToString();
                //        //dataView.RowFilter = $"Infor ID LIKE '%{searchText}%' OR First Name LIKE '%{searchText}%' OR Last Name LIKE '%{searchText}%' OR CTO Earned LIKE '%{searchText}%' OR CTO Used LIKE '%{searchText}%'";
                //    }
                //}
              
                    

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

        }
    }

}
