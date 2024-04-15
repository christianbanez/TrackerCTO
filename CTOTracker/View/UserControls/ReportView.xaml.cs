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

namespace CTOTracker.View.UserControls
{
    /// <summary>
    /// Interaction logic for ReportView.xaml
    /// </summary>
    public partial class ReportView : UserControl
    {
        private DataConnection dataConnection;
        //EmployeeView employeeView=new EmployeeView();
        
        public ReportView()
        {
            InitializeComponent();
            dataConnection = new DataConnection();
            EmployeeReportView(); // This loads the initial report
            TaskScheduleReportView();
        }

        private void EmployeeReportView()
        {
            string query = "SELECT Employee.inforID, fName, lName, email, contact, Role.roleName FROM Employee INNER JOIN Role ON Employee.roleID = Role.roleID";
            LoadEmployeeReport(query);

        }
        private void TaskScheduleReportView()
        {
            string query = "SELECT * From Schedule";
            LoadTaskScheduleReport(query);

        }
        private void LoadEmployeeReport(string query)
        {
            using (OleDbConnection connection = dataConnection.GetConnection())
            {
                try
                {
                    connection.Open();
                    OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    if (dataTable != null && dataTable.Rows.Count > 0)
                    {
                        employee_ReportDataGrid.ItemsSource = dataTable.DefaultView;
                    }
                    else
                    {
                        MessageBox.Show("No data found.", "Information");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error");
                }
                finally
                {
                    connection.Close();
                }
            }
        }
        private void LoadTaskScheduleReport(string query)
        {
            using (OleDbConnection connection = dataConnection.GetConnection())
            {
                try
                {
                    connection.Open();
                    OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    if (dataTable != null && dataTable.Rows.Count > 0)
                    {
                        taskSchedule_ReportDataGrid.ItemsSource = dataTable.DefaultView;
                    }
                    else
                    {
                        MessageBox.Show("No data found.", "Information");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error");
                }
                finally
                {
                    connection.Close();
                }
            }
        }
        private void employee_ReportDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void cbxFilterBy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string filterBy = ((ComboBoxItem)cbxFilterBy.SelectedItem).Content.ToString();

            if (filterBy == "Remaining CTO Balance")
            {
                LoadEmployeeReportWithRemainingCTO();
            }
            else if (filterBy == "Ongoing Tasks")
            {
                LoadEmployeeReportWithOngoingTasks();
            }
        }
        private void LoadEmployeeReportWithRemainingCTO()
        {
            // Your code to load the report for employees with remaining CTO balance
            // Modify your query to retrieve employees with remaining CTO balance
            string query = @"SELECT Employee.inforID, Employee.fName, Employee.lName, Employee.email, Employee.contact, Role.roleName, Schedule.ctoBalance
FROM (Employee
INNER JOIN Role ON Employee.roleID = Role.roleID)
INNER JOIN Schedule ON Employee.empID = Schedule.empID
WHERE Schedule.ctoBalance > 0;";

            LoadEmployeeReport(query);
        }

        /* private void EmployeeReportView(string query)
         {
             throw new NotImplementedException();
         }*/

        private void LoadEmployeeReportWithOngoingTasks()
        {
            string query = @"
                SELECT inforID, fName, lName, email, contact, Role.roleName
                FROM Employee
                INNER JOIN Role ON Employee.roleID = Role.roleID
                WHERE Employee.inforID IN (
                    SELECT EmployeeID
                    FROM Task
                    WHERE TaskStatus = 'Ongoing'
                )";

            LoadEmployeeReport(query);
        }
    }
}
