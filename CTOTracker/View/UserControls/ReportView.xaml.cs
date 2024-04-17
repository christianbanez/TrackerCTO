using System.Data;
using System.Data.OleDb;
using System.Windows;
using System.Windows.Controls;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;    

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
            string query = "SELECT Employee.inforID, Employee.fName, Employee.lName, Role.roleName, Task.taskName, Schedule.completed, Schedule.ctoBalance\r\nFROM Task INNER JOIN ((Role INNER JOIN Employee ON Role.roleID = Employee.roleID) INNER JOIN Schedule ON Employee.empID = Schedule.empID) ON Task.taskID = Schedule.taskID;\r\n";
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
                    EmpFilPnl.Visibility = System.Windows.Visibility.Collapsed;
                }
                else if (cbxFilterRep.SelectedItem.ToString() == "All Employee")
                {
                    // Show the Employee Filtered Panel
                    EmpFilPnl.Visibility = System.Windows.Visibility.Visible;
                    PopulateEmployeeListComboBox();
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
        private void PopulateEmployeeListComboBox()
        {
            string query = "SELECT fName + ' ' + lName AS FullName FROM Employee";

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
                        // Clear previous items
                        cbxEmpList.Items.Clear();

                        // Populate ComboBox with employee names
                        foreach (DataRow row in dataTable.Rows)
                        {
                            cbxEmpList.Items.Add(row["FullName"]);
                        }
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
        private void ExportToPdf(DataGrid dataGrid)
        {
            try
            {
                // Specify the output directory and file name
                string outputPath = @"Output\output.pdf"; // Change this path to your desired directory

                // Create a PDF document
                Document doc = new Document();
                PdfWriter.GetInstance(doc, new FileStream(outputPath, FileMode.Create));
                doc.Open();

                // Add DataGrid content to the PDF document
                PdfPTable pdfTable = new PdfPTable(dataGrid.Columns.Count);
                foreach (DataGridColumn column in dataGrid.Columns)
                {
                    pdfTable.AddCell(new Phrase(column.Header.ToString()));
                }

                foreach (var item in dataGrid.Items)
                {
                    if (item is DataRowView)
                    {
                        DataRowView rowView = item as DataRowView;
                        DataRow row = rowView.Row;
                        foreach (var cell in row.ItemArray)
                        {
                            pdfTable.AddCell(new Phrase(cell.ToString()));
                        }
                    }
                }

                doc.Add(pdfTable);
                doc.Close();

                MessageBox.Show("PDF exported successfully! Output path: " + outputPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting PDF: " + ex.Message);
            }
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            ExportToPdf(reportDataGrid);
        }
    }
}
