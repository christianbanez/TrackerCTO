using System.Data;
using System.Data.OleDb;
using System.Windows;
using System.Windows.Controls;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Drawing;

namespace CTOTracker.View.UserControls
{
    /// <summary>
    /// Interaction logic for ReportView.xaml
    /// </summary>
    public partial class ReportView : UserControl
    {
        private DataConnection dataConnection;
        private List<string> allEmployees; // Store all employee names
        private List<string> filteredEmployees; // Store filtered employee names
        //EmployeeView employeeView=new EmployeeView();

        public ReportView()
        {
            InitializeComponent();
            dataConnection = new DataConnection();
            EmployeeReportView();
            PopulateComboBox();
            PopulateEmployeeComboBox();
            filteredEmployees = new List<string>();
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
                "Employee",
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
                if (cbxFilterRep.SelectedItem.ToString() == "Employees with CTO balance")
                {
                    LoadEmployeeReportWithCTO();
                    EmpFilPnl.Visibility = System.Windows.Visibility.Collapsed;
                }
                else if (cbxFilterRep.SelectedItem.ToString() == "Employee")
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
                string outputPath = @"C:\Users\dkeh\source\repos\TrackerCTO\output.pdf"; // Change this path to your desired directory
                
                // Create a PDF document
                Document doc = new Document();
                PdfWriter.GetInstance(doc, new FileStream(outputPath, FileMode.Create));
                doc.Open();
                // Add Header with Company Information
                PdfPTable headerTable = new PdfPTable(1);
                headerTable.WidthPercentage = 100;
                // Add current date and time
                DateTime currentDate = DateTime.Now;
                doc.Add(new Paragraph("Date generated: " + currentDate.ToString()));
                //doc.Add(new Paragraph.Alignment = Element.ALIGN_RIGHT);
                
                // Add company logo (assuming logoPath is the path to the company logo)
                iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(@"C:\Users\dkeh\source\repos\TrackerCTO\CTOTracker\Images\logo.png");
                logo.ScaleToFit(50f, 50f); // Adjust size as needed
                PdfPCell logoCell = new PdfPCell(logo);
                logoCell.HorizontalAlignment = Element.ALIGN_LEFT;
                logoCell.Border = PdfPCell.NO_BORDER;
                headerTable.AddCell(logoCell);
                doc.Add(new Paragraph(" "));
                // Add company name
                PdfPCell companyNameCell = new PdfPCell(new Phrase("EMPLOYEE CTO TRACKER RECORD"));
                companyNameCell.HorizontalAlignment = Element.ALIGN_CENTER;
                companyNameCell.Border = PdfPCell.NO_BORDER;
                headerTable.AddCell(companyNameCell);
                // Add empty cell to create space between header and table
                PdfPCell emptyCell = new PdfPCell(new Phrase(" "));
                emptyCell.Border = PdfPCell.NO_BORDER;
                headerTable.AddCell(emptyCell);
                doc.Add(headerTable);
                // Define a style for the header column
                Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE);
                Font cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 9); // Adjust font size here
                PdfPCell headerCell = new PdfPCell();
                headerCell.BackgroundColor = new BaseColor(51, 122, 183); // Set background color to a shade of blue
                headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
                headerCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                headerCell.Padding = 3;
                // Add DataGrid content to the PDF document
                PdfPTable pdfTable = new PdfPTable(dataGrid.Columns.Count);
                pdfTable.SetWidths(new float[] { 2f, 2f, 2f, 2f, 2f, 2f, 2f }); // Adjust column widths here
                foreach (DataGridColumn column in dataGrid.Columns)
                {
                    headerCell.Phrase = new Phrase(column.Header.ToString(), headerFont);
                    pdfTable.AddCell(headerCell);
                }

                foreach (var item in dataGrid.Items)
                {
                    if (item is DataRowView)
                    {
                        DataRowView rowView = item as DataRowView;
                        DataRow row = rowView.Row;
                        foreach (var cell in row.ItemArray)
                        {
                            PdfPCell cellToAdd = new PdfPCell(new Phrase(cell.ToString(), cellFont));
                            pdfTable.AddCell(cellToAdd);
                        }
                    }
                }
                // Set position of PDF table
                //pdfTable.SetTotalWidth(doc.PageSize.Width - doc.LeftMargin - doc.RightMargin);
                pdfTable.HorizontalAlignment = Element.ALIGN_CENTER;

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
