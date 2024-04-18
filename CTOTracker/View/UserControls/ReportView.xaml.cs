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

        public ReportView()
        {
            InitializeComponent();
            dataConnection = new DataConnection();
            //txtschFname.TextChanged += txtschFname_TextChanged;
            LoadAllData();
            //LoadData();
            txtschFname.TextChanged += txtschFname_TextChanged;
            //PopulateComboBox();
            //cbxFilterRep.SelectionChanged += CbxFilterRep_SelectionChanged;
        }

/*        private void LoadData(string fName=null)
        {
            try
            {
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    string query = "SELECT Employee.inforID, Employee.fName, Employee.lName, Task.taskName, Role.roleName, plannedEnd, ctoEarned, dateUsed, " +
                                    "ctoUsed, ctoBalance FROM (((Schedule " +
                                    "LEFT JOIN Employee ON Schedule.empID = Employee.empID) " +
                                    "LEFT JOIN Role ON Employee.roleID = Role.roleID) " +
                                    "LEFT JOIN Task ON Schedule.taskID = Task.taskID);";
                    if (!string.IsNullOrEmpty(fName))
                    {
                        query += " WHERE Employee.fName LIKE '%'";
                    }

                    OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
                    if (!string.IsNullOrEmpty(fName))
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("?", "%" + fName + "%");
                    }
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    reportDataGrid.Columns.Clear();
                    reportDataGrid.ItemsSource = dataTable.DefaultView;

                    #region
                    //DataView dataView = new DataView(dataTable);
                    reportDataGrid.ItemsSource = dataTable.DefaultView;

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
                    #endregion
                    // Bind the DataTable to the DataGrid
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }*/
        private void LoadAllData()
        {
            try
            {
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    string query = "SELECT Employee.inforID, Employee.fName, Employee.lName, Task.taskName, Role.roleName, plannedEnd, ctoEarned, dateUsed, " +
                                    "ctoUsed, ctoBalance FROM (((Schedule " +
                                    "LEFT JOIN Employee ON Schedule.empID = Employee.empID) " +
                                    "LEFT JOIN Role ON Employee.roleID = Role.roleID) " +
                                    "LEFT JOIN Task ON Schedule.taskID = Task.taskID);";


                    OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    reportDataGrid.Columns.Clear();
                    reportDataGrid.ItemsSource = dataTable.DefaultView;

                    #region
                    //DataView dataView = new DataView(dataTable);
                    reportDataGrid.ItemsSource = dataTable.DefaultView;

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
                    #endregion
                    // Bind the DataTable to the DataGrid
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private double originalDtPnlHeight; // Store the original height of dtPnl
        private double filterPnlHeight = 110;

        private void tgb_FilterPnl_Checked(object sender, RoutedEventArgs e)
        {
            //filter panel animation
            DoubleAnimation showAnimation = new DoubleAnimation();
            showAnimation.From = 45;
            showAnimation.To = 150;
            showAnimation.Duration = TimeSpan.FromSeconds(0.3);
            FilterPnl.BeginAnimation(HeightProperty, showAnimation);

            //dtpnl animation
            ThicknessAnimation animation = new ThicknessAnimation();
            animation.From = new Thickness(0, 45, 0, 0);
            animation.To = new Thickness(0, 90, 0, 0); // Adjust this value as needed
            animation.Duration = TimeSpan.FromSeconds(0.3); // Adjust the duration as needed
            dtPnl.BeginAnimation(MarginProperty, animation);
            //dtPnl.Height -= filterPnlHeight;
        }

        private void tgb_FilterPnl_Unchecked(object sender, RoutedEventArgs e)
        {
            //filter panel animation
            DoubleAnimation hideAnimation = new DoubleAnimation();
            hideAnimation.From = 150;
            hideAnimation.To = 45;
            hideAnimation.Duration = TimeSpan.FromSeconds(0.2);
            FilterPnl.BeginAnimation(HeightProperty, hideAnimation);

            //dtpnl animation
            ThicknessAnimation animation = new ThicknessAnimation();
            animation.From = new Thickness(0, 90, 0, 0); // Adjust this value as needed
            animation.To = new Thickness(0, 45, 0, 0);
            animation.Duration = TimeSpan.FromSeconds(0.3); // Adjust the duration as needed
            dtPnl.BeginAnimation(MarginProperty, animation);
            //dtPnl.Height = originalDtPnlHeight;
        }

        /*private void txtbx_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text == textBox.Tag?.ToString()) // Check if the current text matches the placeholder
            {
                textBox.Text = ""; // Clear the text
                textBox.Foreground = Brushes.Black; // Change the text color back to black
            }
        }*/

            /*private void txtbx_LostFocus(object sender, RoutedEventArgs e)
            {
                TextBox textBox = (TextBox)sender;
                if (string.IsNullOrWhiteSpace(textBox.Text)) // If the TextBox is empty
                {
                    textBox.Text = textBox.Tag?.ToString(); // Set the placeholder text back
                    textBox.Foreground = Brushes.Gray; // Change the text color to gray to indicate it's a placeholder
                }
            }*/

            /*private void txtbx_Loaded(object sender, RoutedEventArgs e)
            {
                TextBox textBox = (TextBox)sender;
                textBox.Text = textBox.Tag?.ToString(); // Set the placeholder text
                textBox.Foreground = Brushes.Gray;
            }*/
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
                    pdfTable.SetWidths(new float[] { 3f, 2f, 2f, 2f, 2f, 4f, 2f, 4f, 2f, 2f }); // Adjust column widths here
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
                LoadAllData();
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
            txtschFname.Text = "First Name";
            LoadAllData();
        }

        private void txtschFname_LostFocus(object sender, RoutedEventArgs e)
        {
            txtschFname.Text = "First Name";
            LoadAllData();
        }

        private void txtschLname_GotFocus(object sender, RoutedEventArgs e)
        {
            txtschLname.Text = "";
        }
    }
}
