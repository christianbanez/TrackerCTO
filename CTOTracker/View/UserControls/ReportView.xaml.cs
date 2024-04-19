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
//using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
//using iTextParagraph = iTextSharp.text.Paragraph;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Diagnostics;
using Microsoft.Win32;
using System.IO;

namespace CTOTracker.View.UserControls
{
    /// <summary>
    /// Interaction logic for ReportView.xaml
    /// </summary>
    public partial class ReportView : UserControl
    {
        private DataConnection dataConnection;
        private List<string> allEmployees;
        public ReportView()
        {
            InitializeComponent();
            dataConnection = new DataConnection();
            DataReportView();
            txtschFname.TextChanged += txtschFname_TextChanged;
            chkbxBalance.Checked += (sender, e) => FilterAndLoadData();
            chkbxBalance.Unchecked += (sender, e) => FilterAndLoadData();
            
        }
        private void DataReportView()
        {
            string query = "SELECT Employee.inforID, Employee.fName, Employee.lName, Role.roleName, Task.taskName, Schedule.plannedEnd, Schedule.ctoEarned, Schedule.dateUsed, Schedule.ctoUsed, Schedule.ctoBalance " +
                "FROM (Role INNER JOIN Employee ON Role.roleID = Employee.roleID) " +
                "INNER JOIN (Task INNER JOIN Schedule ON Task.taskID = Schedule.taskID) " +
                "ON Employee.empID = Schedule.empID;";
            LoadAllData(query);

        }
        private bool columnsAdded = false;
        private void LoadAllData(string query)
        {
            using (OleDbConnection connection = dataConnection.GetConnection())
            {
                try
                {
                    connection.Open();
                    OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    reportDataGrid.ItemsSource = null;
                    reportDataGrid.Items.Clear();
                    dataTable.Clear();

                    adapter.Fill(dataTable);
                    reportDataGrid.ItemsSource = dataTable.DefaultView;

                    if (dataTable != null && dataTable.Rows.Count > 0)
                    {
                        reportDataGrid.ItemsSource = dataTable.DefaultView;
                    }
                    else
                    {
                        MessageBox.Show("No data found.", "Information");
                    }
                    if (!columnsAdded)
                    {
                        AddDataGridColumns();
                        columnsAdded = true; // Set the flag to true after adding columns
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
        private void AddDataGridColumns()
        {
            // Create DataGrid columns
            reportDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Infor ID",
                Binding = new Binding("inforID"),
                Width = 75
            });
            reportDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "First Name",
                Binding = new Binding("fName"),
                Width = 185
            });
            reportDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Last Name",
                Binding = new Binding("lName"),
                Width = 185
            });
            reportDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Role",
                Binding = new Binding("roleName"),
                Width = 125
            });
            reportDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Task",
                Binding = new Binding("taskName"),
                Width = 125
            });
            reportDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Date Earned",
                Binding = new Binding("plannedEnd")
            });
            reportDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "CTO Earned",
                Binding = new Binding("ctoEarned")
            });
            reportDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Date Used",
                Binding = new Binding("dateUsed")
            });
            reportDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "CTO Used",
                Binding = new Binding("ctoUsed")
            });
            reportDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "CTO Balance",
                Binding = new Binding("ctoBalance")
            });
        }

        private double originalDtPnlHeight; // Store the original height of dtPnl
        private double filterPnlHeight = 110;
        private void ExportToPdf(DataGrid dataGrid, string outputPath)
        {
            try
            {
                // Create a PDF document
                Document doc = new Document();

                // Show SaveFileDialog to get the output path
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*";
                saveFileDialog.FileName = "output.pdf"; // Default file name
                if (saveFileDialog.ShowDialog() == true)
                {
                    outputPath = saveFileDialog.FileName;

                    // Proceed with PDF creation
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
                    Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.WHITE);
                    Font cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 7); // Adjust font size here
                    PdfPCell headerCell = new PdfPCell();
                    headerCell.BackgroundColor = new BaseColor(51, 122, 183); // Set background color to a shade of blue
                    headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    headerCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    headerCell.Padding = 3;
                    // Add DataGrid content to the PDF document
                    PdfPTable pdfTable = new PdfPTable(dataGrid.Columns.Count);
                    pdfTable.SetWidths(new float[] { 3f, 3f, 3f, 2f, 2f, 4f, 2f, 4f, 3f, 4f }); // Adjust column widths here
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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting PDF: " + ex.Message);
            }
        }

        private void btnExport_Click_1(object sender, RoutedEventArgs e)
        {
            ExportToPdf(reportDataGrid, null);
        }
        private void LoadScheduleDataByInitial(string initial)
        {
            try
            {
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    string query = "SELECT Employee.inforID, Employee.fName, Employee.lName, Task.taskName, Role.roleName, plannedEnd, ctoEarned, dateUsed, " +
                                    "ctoUsed, ctoBalance FROM (((Schedule " +
                                    "LEFT JOIN Employee ON Schedule.empID = Employee.empID) " +
                                    "LEFT JOIN Role ON Employee.roleID = Role.roleID) " +
                                    "LEFT JOIN Task ON Schedule.taskID = Task.taskID) WHERE Employee.fName LIKE @Initial + '%' OR Employee.lName LIKE @Initial + '%'";

                    OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
                    adapter.SelectCommand.Parameters.AddWithValue("@Initial", initial);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    // Bind the DataTable to the DataGrid
                    reportDataGrid.ItemsSource = dataTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
        private void txtschFname_TextChanged(object sender, TextChangedEventArgs e)
        {
            string name = txtschFname.Text.ToString();
            //LoadAllData();
            if (string.IsNullOrEmpty(name))
            {
                DataReportView();
                return;
            }
            else
            {

            }
            //Otherwise, filter the data based on the entered initial
            LoadScheduleDataByInitial(txtschFname.Text);
        }

        private void txtschFname_GotFocus(object sender, RoutedEventArgs e)
        {
            txtschFname.Text = "";
        }

        private void txtschLname_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtschLname.Text))
            {
                txtschLname.Text = "Last Name";
            }
            DataReportView();
        }

        private void txtschFname_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtschFname.Text))
            {
                txtschFname.Text = "First Name";
            }
            DataReportView();
        }

        private void txtschLname_GotFocus(object sender, RoutedEventArgs e)
        {
            txtschLname.Text = "";
        }
        private void LoadEmployeeReportWithCTO()
        {
            // Modify your query to retrieve employees with remaining CTO balance
            string query = @"SELECT Employee.inforID, Employee.fName, Employee.lName, Role.roleName,Task.taskName, Schedule.plannedEnd, Schedule.ctoEarned, Schedule.dateUsed, " +
                "Schedule.ctoUsed, Schedule.ctoBalance FROM (Role INNER JOIN Employee ON Role.roleID = Employee.roleID) " +
                "INNER JOIN(Task INNER JOIN Schedule ON Task.taskID = Schedule.taskID) " +
                "ON Employee.empID = Schedule.empID\r\nWHERE (((Schedule.ctoBalance)>0));";

            LoadAllData(query);
        }
        private void FilterAndLoadData()
        {
            // Check the state of the CheckBox
            if (chkbxBalance.IsChecked == true)
            {
                // Load data with remaining CTO balance
                LoadEmployeeReportWithCTO();
            }
            else 
            {
                // Load all data
                DataReportView();
            }
        }
        private void chkbxBalance_Checked(object sender, RoutedEventArgs e)
        {
            FilterAndLoadData();         }

        /*private void PopulateEmployeeListComboBox(string selectedRole)
        {
            string query = "SELECT * FROM Employees WHERE Role = @Role";

            //string query = "SELECT roleName FROM Role";

            using (OleDbConnection connection = dataConnection.GetConnection())
            {
                try
                {
                    connection.Open();
                    OleDbCommand cmd = new OleDbCommand(query, connection);
                    cmd.Parameters.AddWithValue("@Role", selectedRole);

                    OleDbDataAdapter adapter = new OleDbDataAdapter(cmd);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    if (dataTable != null && dataTable.Rows.Count > 0)
                    {
                        // Clear previous items
                        cmbxRole.Items.Clear();

                        // Populate ComboBox with employee names
                        foreach (DataRow row in dataTable.Rows)
                        {
                            cmbxRole.Items.Add(row["roleName"]);
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
        }*/


    }
}
