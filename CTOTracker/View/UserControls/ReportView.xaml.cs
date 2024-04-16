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
            EmployeeReportView();
            PopulateComboBox();
            cbxFilterRep.SelectionChanged += CbxFilterRep_SelectionChanged;
        }
        private void EmployeeReportView()
        {
            string query = "SELECT Employee.inforID, Employee.fName, Employee.lName, Employee.email, Role.roleName, Schedule.ctoBalance\r\nFROM (Role INNER JOIN Employee ON Role.roleID = Employee.roleID) INNER JOIN Schedule ON Employee.empID = Schedule.empID;\r\n";
            LoadEmployeeReport(query);

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
                        reportDataGrid.ItemsSource = dataTable.DefaultView;
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
        private void PopulateComboBox()
        {
            // Create a list of strings to populate the ComboBox
            List<string> filterOptions = new List<string>
            {
                "Employee with CTO balance",
                "All Employee",
                "All Task Schedule"
            };

            // Assign the list as the ItemsSource for the ComboBox
            cbxFilterRep.ItemsSource = filterOptions;
        }

        private void CbxFilterRep_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbxFilterRep.SelectedItem != null)
            {
                // Get the selected item

                // Check if the selected item matches the specific item
                if (cbxFilterRep.SelectedItem.ToString() == "Employee with CTO balance")
                {
                    LoadEmployeeReportWithCTO();
                }
                else if (cbxFilterRep.SelectedItem.ToString() == "All Employee")
                {
                    // Show the Employee Filtered Panel
                    EmpFilPnl.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    // Hide the Employee Filtered Panel
                    EmpFilPnl.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }
        private void LoadEmployeeReportWithCTO()
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
    }
}
