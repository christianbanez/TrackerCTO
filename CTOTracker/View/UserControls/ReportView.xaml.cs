using System.Data.OleDb;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Win32;
using System.IO;
using iTextSharp.text.pdf.draw;
using Org.BouncyCastle.Ocsp;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.VisualBasic;

namespace CTOTracker.View.UserControls
{
    /// <summary>
    /// Interaction logic for ReportView.xaml
    /// </summary>
    /// 


    public partial class ReportView : UserControl
    {
        string imagePath = @"C:\Users\dkeh\Source\Repos\TrackerCTO\CTOTracker\Images\VeCTOr Main Icon.png";
        private DataConnection dataConnection;
        private DataView dataView;
        private List<string> allEmployees;

        private string nameFilter = "";
        private string taskFilter = "";
        private string roleFilter = "";
        public ReportView()
        {
            InitializeComponent();
            dataConnection = new DataConnection();
            DataReportView();
            PopulateEmployeeComboBox();
            //txtschFname.TextChanged += txtschFname_TextChanged;
            rbBalance.Checked += (sender, e) => ApplyFiltersAndUpdateDataGrid();
            rbBalance.Unchecked += (sender, e) => ApplyFiltersAndUpdateDataGrid();
            rbUsed.Checked += (sender, e) => ApplyFiltersAndUpdateDataGrid();
            rbUsed.Unchecked += (sender, e) => ApplyFiltersAndUpdateDataGrid();
            rbEmpEarned.Checked += (sender, e) => ApplyFilters2();
            rbEmpEarned.Unchecked += (sender, e) => ApplyFilters2();
            rbEmpBalance.Checked += (sender, e) => ApplyFilters2();
            rbEmpBalance.Unchecked += (sender, e) => ApplyFilters2();
            rbEmpUsed.Checked += (sender, e) => ApplyFilters2();
            rbEmpUsed.Unchecked += (sender, e) => ApplyFilters2();
            cmbxMoY.SelectionChanged += cmbxMoY_SelectionChanged;
            cmbxEmpMoY.SelectionChanged += cmbxEmpMoY_SelectionChanged;
            cmbxTask.SelectionChanged += cmbxTask_SelectionChanged;
            cmbxRole.SelectionChanged += cmbxRole_SelectionChanged;
            txtschFname.SelectionChanged += txtschFname_SelectionChanged;
            //cmbxEoU.SelectionChanged += cmbxEoU_SelectionChanged;
            EmpFilPnl.Visibility = Visibility.Collapsed;
            PopulateRoleComboBox();
            PopulateTaskComboBox();
            PopulateMoYComboBox();
        }
        private void DataReportView()
        {
            string query = "SELECT Employee.inforID AS inforID, Employee.fName AS fName, Employee.lName AS lName, Role.roleName AS roleName, Task.taskName, Format(Schedule.plannedEnd, 'MM/dd/yyyy') AS plannedEnd," +
                " Schedule.ctoEarned, Format(CTOuse.dateUsed, 'MM/dd/yyyy') AS dateUsed, CTOuse.ctoUse, Schedule.ctoBalance FROM (((Schedule LEFT JOIN Employee ON Schedule.empID = Employee.empID)" +
                " LEFT JOIN Role ON Employee.roleID = Role.roleID) LEFT JOIN Task ON Schedule.taskID = Task.taskID) LEFT JOIN CTOuse ON Schedule.schedID = CTOuse.schedID" +
                " WHERE Schedule.completed = -1";
            LoadAllData(query);
        }
        private bool columnsAdded = false;
        private bool columnsAddedemp = false;
        private void LoadAllData(string query)
        {
            using (OleDbConnection connection = dataConnection.GetConnection())
            {
                connection.Open();
                
                using (OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection))
                {
                    if (txtschFname.SelectedItem != null)
                    {
                        string employeeId = GetSelectedEmployeeId();
                        if (!string.IsNullOrEmpty(employeeId))
                        {
                            adapter.SelectCommand.Parameters.AddWithValue("@empID", employeeId);
                        }
                    }
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    if (dataTable != null && dataTable.Rows.Count > 0)
                    {
                        reportDataGrid.ItemsSource = dataTable.DefaultView;
                        if (!columnsAdded)
                        {
                            AddDataGridColumns();
                            columnsAdded = true; // Set the flag to true after adding columns
                        }
                        // Enable MouseDoubleClick event
                        reportDataGrid.MouseDoubleClick += reportDataGrid_MouseDoubleClick;
                    }
                    else
                    {
                        MessageBox.Show("No data found in the Records.", "Information");
                        txtschFname.Text = "";
                        cmbxTask.SelectedIndex = -1;
                        cmbxTask.Tag = "Task";
                        cmbxRole.SelectedIndex = -1;
                        cmbxRole.Tag = "Role";
                        dtEDate.SelectedDate = null;
                        //dtUDate.SelectedDate = null;
                        DataReportView();

                        return;
                    }
                    if (!columnsAdded)
                    {
                        AddDataGridColumns();
                        columnsAdded = true; // Set the flag to true after adding columns
                    }
                }
                

               
            }
        }
        private void ApplyFiltersAndUpdateDataGrid()
        {
            // Construct the query based on the selected filters
            string query = "SELECT Employee.inforID AS inforID, Employee.fName AS fName, Employee.lName AS lName, Role.roleName AS roleName, Task.taskName, Format(Schedule.plannedEnd, 'MM/dd/yyyy') AS plannedEnd," +
                " Schedule.ctoEarned, Format(CTOuse.dateUsed, 'MM/dd/yyyy') AS dateUsed, CTOuse.ctoUse, Schedule.ctoBalance FROM (((Schedule LEFT JOIN Employee ON Schedule.empID = Employee.empID)" +
                " LEFT JOIN Role ON Employee.roleID = Role.roleID) LEFT JOIN Task ON Schedule.taskID = Task.taskID) LEFT JOIN CTOuse ON Schedule.schedID = CTOuse.schedID" +
                " WHERE Schedule.completed = -1";

            try
            {
                if (txtschFname.SelectedItem != null)
                {
                    string employeeId = GetSelectedEmployeeId();
                    if (!string.IsNullOrEmpty(employeeId))
                    {
                        // Append the employee ID filter to the query
                        query += " AND (Employee.empID = ?)";
                    }
                }
                if (!string.IsNullOrEmpty(taskFilter))
                {
                    query += $" AND (Task.taskName = '{taskFilter}')";
                }
                if (!string.IsNullOrEmpty(roleFilter))
                {
                    query += $" AND (Role.roleName = '{roleFilter}')";
                }
                if (rbBalance.IsChecked == true)
                {
                    query += " AND (Schedule.ctoBalance > 0)";
                }
                if (rbUsed.IsChecked == true)
                {
                    query += " AND (CTOuse.ctoUse > 0)";
                }
                if (cmbxMoY.SelectedItem != null && cmbxMoY.SelectedItem.ToString() == "Month/Year")
                {
                    // Check if the date picker has a selected date
                    if (dtEDate.SelectedDate.HasValue)
                    {
                        DateTime selectedDate = dtEDate.SelectedDate.Value;
                        query += $" AND (MONTH(Schedule.plannedEnd) = {selectedDate.Month} AND YEAR(Schedule.plannedEnd) = {selectedDate.Year})";
                    }
                }
                else if (cmbxMoY.SelectedItem != null && cmbxMoY.SelectedItem.ToString() == "Year")
                {
                    // Get the selected year from the date picker
                    int selectedYear = dtEDate.SelectedDate.HasValue ? dtEDate.SelectedDate.Value.Year : DateTime.Now.Year;

                    // Apply the "Year" filter to the query
                    query += $" AND YEAR(Schedule.plannedEnd) = {selectedYear}";
                }
                // Execute the query and update the DataGrid
                LoadAllData(query);
               /* if (cmbxTask.SelectedItem != null && reportDataGrid.Items.Count == 0 && !string.IsNullOrEmpty(taskFilter))
                {
                    MessageBox.Show($"No data found for the selected task: {taskFilter}.", "Information", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    cmbxTask.SelectedIndex = -1;
                }*/
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

        }
        private void AddDataGridColumns() //Columns for reportDataGrid
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
                Binding = new Binding("ctoUse")
            });
            reportDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "CTO Balance",
                Binding = new Binding("ctoBalance")
            });

        }

        private double originalDtPnlHeight; // Store the original height of dtPnl
        private double filterPnlHeight = 110;
        private void ExportToPdf(DataTable dataTable, string outputPath)
        {
            // Check if there is data to export
            if (dataTable == null || dataTable.Rows.Count == 0)
            {
                MessageBox.Show("No data available for export.", "Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Create a PDF document
                Document doc = new Document();

                // Show SaveFileDialog to get the output path
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*";
                saveFileDialog.FileName = $"output_{DateTime.Now:yyyyMMdd}.pdf";
                if (saveFileDialog.ShowDialog() == true)
                {
                    outputPath = saveFileDialog.FileName;

                    // Proceed with PDF creation
                    PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(outputPath, FileMode.Create));
                    doc.Open();

                    // Add Header with Company Information
                    PdfPTable headerTable = new PdfPTable(1);
                    headerTable.WidthPercentage = 100;

                    // Add company logo (assuming logoPath is the path to the company logo)
                    iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(imagePath);
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
                    Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.BLACK);
                    Font cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 7); // Adjust font size here

                    // Add DataGrid content to the PDF document
                    PdfPTable pdfTable = new PdfPTable(dataTable.Columns.Count);
                    pdfTable.WidthPercentage = 100;

                    // Add headers
                    foreach (DataGridColumn column in reportDataGrid.Columns)
                    {
                        // Get the header text of the column
                        string columnHeader = column.Header.ToString();

                        // Add the column header to the PDF table
                        PdfPCell headerCell = new PdfPCell(new Phrase(columnHeader, headerFont));
                        headerCell.BackgroundColor = new BaseColor(230, 230, 250);  // Set background color to a shade of blue
                        headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
                        headerCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        headerCell.Padding = 3;
                        pdfTable.AddCell(headerCell);
                    }
                    // Iterate through the rows and add the corresponding cell data
                    foreach (var item in reportDataGrid.Items)
                    {
                        var row = reportDataGrid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                        if (row != null)
                        {
                            foreach (DataGridColumn column in reportDataGrid.Columns)
                            {
                                var cellContent = column.GetCellContent(item) as TextBlock;
                                if (cellContent != null)
                                {
                                    // Get the text content of the TextBlock
                                    string cellText = cellContent.Text;

                                    // Add the text content to the PDF table
                                    PdfPCell cellToAdd = new PdfPCell(new Phrase(cellText, cellFont));
                                    cellToAdd.HorizontalAlignment = Element.ALIGN_CENTER;
                                    cellToAdd.VerticalAlignment = Element.ALIGN_MIDDLE;
                                    pdfTable.AddCell(cellToAdd);
                                }
                            }
                        }
                    }

                    // Add table to document
                    doc.Add(pdfTable);

                    PdfPTable footerTable = new PdfPTable(1);
                    footerTable.TotalWidth = doc.PageSize.Width - doc.LeftMargin - doc.RightMargin;
                    footerTable.DefaultCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    PdfPCell footerCell = new PdfPCell(new Phrase("Date generated: " + DateTime.Now.ToString("MM/dd/yyyy")));
                    footerCell.Border = 0;
                    footerTable.AddCell(footerCell);
                    footerTable.WriteSelectedRows(0, -1, doc.LeftMargin, doc.BottomMargin + 10, writer.DirectContent);
                    doc.Close();

                    MessageBox.Show("PDF exported successfully! Output path: " + outputPath, "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting PDF: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void btnExport_Click_1(object sender, RoutedEventArgs e)
        {
            // Convert DataView to DataTable
            DataView dataView = reportDataGrid.ItemsSource as DataView;

            // Apply the same filters as applied in the UI
            DataTable filteredDataTable = dataView.ToTable();
            // You may need to apply additional filters here based on UI inputs, such as nameFilter, roleFilter, etc.

            // Call the ExportToPdf method with the filtered data
            ExportToPdf(filteredDataTable, null);
        }

        private void txtschFname_TextChanged(object sender, TextChangedEventArgs e)
        {
            nameFilter = txtschFname.Text.Trim();
            ApplyFiltersAndUpdateDataGrid();
        }

        //------------------------------Task------------------------------------
        private void PopulateTaskComboBox()
        {
            try
            {
                // Fetch data from the Employee table
                List<string> task = GetDataFromTask();

                // Check if 'allEmployees' is null before binding to the ComboBox
                if (task != null)
                {
                    cmbxTask.ItemsSource = task;
                }
                //else
                //{
                //    // Handle the case when 'allEmployees' is null
                //    MessageBox.Show("No employees found.");
                //}
            }
            catch (Exception ex)
            {
                // Display an error message if an exception occurs
                MessageBox.Show("Error: " + ex.Message);
            }

        }
        private void PopulateTaskEmpComboBox()
        {
            try
            {
                // Fetch data from the Employee table
                List<string> task = GetDataFromTaskEmp();

                // Check if 'allEmployees' is null before binding to the ComboBox
                if (task != null)
                {
                    cmbxTask.ItemsSource = task;
                }
                //else
                //{
                //    // Handle the case when 'allEmployees' is null
                //    MessageBox.Show("No employees found.");
                //}
            }
            catch (Exception ex)
            {
                // Display an error message if an exception occurs
                MessageBox.Show("Error: " + ex.Message);
            }

        }

        private void PopulateEmployeeComboBox()
        {

            try
            {
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    string query = "SELECT inforID, fName, lName FROM Employee";
                    using (OleDbCommand command = new OleDbCommand(query, connection))
                    {
                        connection.Open();
                        using (OleDbDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string inforID = reader["inforID"].ToString();
                                string fName = reader.IsDBNull(reader.GetOrdinal("fName")) ? "" : reader["fName"].ToString();
                                string lName = reader.IsDBNull(reader.GetOrdinal("lName")) ? "" : reader["lName"].ToString();
                                string fullName = $"{inforID}: {fName} {lName}"; // Concatenate ID with name
                                ComboBoxItem item = new ComboBoxItem
                                {
                                    Text = fullName,
                                    Value = inforID
                                };
                                txtschFname.Items.Add(item);
                            }
                            //if (cbxEmployee.Items.Count > 0)
                            //{
                            //    cbxEmployee.SelectedIndex = 0; // This should trigger cbxEmployee_SelectionChanged
                            //}
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error populating ComboBox: " + ex.Message);
            }

        }

        public class ComboBoxItem
        {
            public string Text { get; set; }
            public string Value { get; set; }

            public override string ToString()
            {
                return Text;
            }
        }
        private string GetSelectedEmployeeId()
        {
            try
            {
                if (txtschFname.SelectedItem != null)
                {
                    ComboBoxItem selectedItem = txtschFname.SelectedItem as ComboBoxItem;
                    if (selectedItem != null)
                    {
                        return selectedItem.Value; // This is the Infor ID
                    }
                }
                /*MessageBox.Show("No employee selected or improper ComboBox item.")*/;
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving employee ID: " + ex.Message);
                return null;
            }
        }
        private List<string> GetDataFromTaskEmp()
        {
            List<string> tasks = new List<string>();

            try
            {
                // Get connection from DataConnection
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    // Define the Access query to select task ID (taskID) and task name (taskName) from the Task table
                    string query = "SELECT DISTINCT Task.taskName FROM Task INNER JOIN Schedule ON Task.taskID = Schedule.taskID WHERE Schedule.empID = ?";

                    // Create a command object with the query and connection
                    OleDbCommand command = new OleDbCommand(query, connection);

                    // Check if the selected item is not null and employee ID is not empty
                    if (txtschFname.SelectedItem != null)
                    {
                        string employeeId = GetSelectedEmployeeId();
                        if (!string.IsNullOrEmpty(employeeId))
                        {
                            command.Parameters.AddWithValue("?", employeeId); // Add parameter using AddWithValue
                        }
                    }

                    // Open the connection to the database
                    connection.Open();

                    // Execute the command and retrieve data using a data reader
                    OleDbDataReader reader = command.ExecuteReader();

                    // Iterate through the data reader to read each row
                    while (reader.Read())
                    {
                        // Check if the taskName column contains non-null values
                        if (!reader.IsDBNull(reader.GetOrdinal("taskName")))
                        {
                            // Get the task name
                            string taskName = reader["taskName"].ToString();

                            // Add the task name to the list
                            tasks.Add(taskName);
                        }
                    }

                    // Close the data reader
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                // Display an error message if an exception occurs
                MessageBox.Show("Error: " + ex.Message);
            }

            // Return the list of employee names retrieved from the database
            return tasks;
            
        }
        private List<string> GetDataFromTask()
        {
            // Create a list to store employee names
            List<string> task = new List<string>();

            try
            {
                // Get connection from DataConnection
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    // Define the Access query to select task ID (taskID) and task name (taskName) from the Task table
                    string query = "SELECT taskID, taskName FROM Task";

                    // Create a command object with the query and connection
                    OleDbCommand command = new OleDbCommand(query, connection);

                    // Open the connection to the database
                    connection.Open();

                    // Execute the command and retrieve data using a data reader
                    OleDbDataReader reader = command.ExecuteReader();

                    // Iterate through the data reader to read each row
                    while (reader.Read())
                    {
                        // Check if the fName and lName columns contain non-null values
                        if (!reader.IsDBNull(reader.GetOrdinal("taskName")))
                        {
                            // Concatenate the first name and last name to form the full name
                            string taskName = $"{reader["taskName"]}";

                            // Add the full name to the list of employees
                            task.Add(taskName);
                        }
                    }

                    // Close the data reader
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                // Display an error message if an exception occurs
                MessageBox.Show("Error: " + ex.Message);
            }

            // Return the list of employee names retrieved from the database
            return task;
        }

        private void PopulateMoYComboBox()
        {
            cmbxMoY.Items.Add("Month/Year");
            cmbxMoY.Items.Add("Year");

            cmbxEmpMoY.Items.Add("Month/Year");
            cmbxEmpMoY.Items.Add("Year");
        }

        //------------------------------Role------------------------------------
        private void PopulateRoleComboBox()
        {
            try
            {
                // Fetch data from the Employee table
                List<string> role = GetDataFromRole();

                // Check if 'allEmployees' is null before binding to the ComboBox
                if (role != null)
                {
                    cmbxRole.ItemsSource = role;
                }
            }
            catch (Exception ex)
            {
                // Display an error message if an exception occurs
                MessageBox.Show("Error: " + ex.Message);
            }

        }
        private List<string> GetDataFromRole()
        {
            // Create a list to store employee names
            List<string> role = new List<string>();

            try
            {
                // Get connection from DataConnection
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    // Define the Access query to select first names (fName) and last names (lName) from the Employee table
                    string query = "SELECT roleID, roleName FROM Role";

                    // Create a command object with the query and connection
                    OleDbCommand command = new OleDbCommand(query, connection);

                    // Open the connection to the database
                    connection.Open();

                    // Execute the command and retrieve data using a data reader
                    OleDbDataReader reader = command.ExecuteReader();

                    // Iterate through the data reader to read each row
                    while (reader.Read())
                    {
                        // Check if the fName and lName columns contain non-null values
                        if (!reader.IsDBNull(reader.GetOrdinal("roleName")))
                        {
                            // Concatenate the first name and last name to form the full name
                            string roleName = $"{reader["roleName"]}";

                            // Add the full name to the list of employees
                            role.Add(roleName);
                        }
                    }

                    // Close the data reader
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                // Display an error message if an exception occurs
                MessageBox.Show("Error: " + ex.Message);
            }

            // Return the list of employee names retrieved from the database
            return role;
        }

        private void rbBalance_Checked(object sender, RoutedEventArgs e)
        {
            rbUsed.IsChecked = false;
            ApplyFiltersAndUpdateDataGrid();
        }
        private void rbUsed_Checked(object sender, RoutedEventArgs e)
        {
            rbBalance.IsChecked = false;
            ApplyFiltersAndUpdateDataGrid();
        }

        private void tgb_FilterPnl_Checked(object sender, RoutedEventArgs e)
        {
            //filter panel animation
            DoubleAnimation showAnimation = new DoubleAnimation();
            showAnimation.From = 42;
            showAnimation.To = 150;
            showAnimation.Duration = TimeSpan.FromSeconds(0.3);
            FilterPnl.BeginAnimation(HeightProperty, showAnimation);

            //dtpnl animation
            ThicknessAnimation animation = new ThicknessAnimation();
            animation.From = new Thickness(0, 42, 0, 0);
            animation.To = new Thickness(0, 90, 0, 0); // Adjust this value as needed
            animation.Duration = TimeSpan.FromSeconds(0.3); // Adjust the duration as needed
            dtPnl.BeginAnimation(MarginProperty, animation);

            DoubleAnimation gridHeightAnimation = new DoubleAnimation();
            gridHeightAnimation.From = reportDataGrid.ActualHeight;
            gridHeightAnimation.To = 602; // Adjust this value as needed
            gridHeightAnimation.Duration = TimeSpan.FromSeconds(0.3);
            reportDataGrid.BeginAnimation(HeightProperty, gridHeightAnimation);
            //dtPnl.Height -= filterPnlHeight;
        }

        private void tgb_FilterPnl_Unchecked(object sender, RoutedEventArgs e)
        {
            //filter panel animation
            DoubleAnimation hideAnimation = new DoubleAnimation();
            hideAnimation.From = 150;
            hideAnimation.To = 42;
            hideAnimation.Duration = TimeSpan.FromSeconds(0.2);
            FilterPnl.BeginAnimation(HeightProperty, hideAnimation);

            //dtpnl animation
            ThicknessAnimation animation = new ThicknessAnimation();
            animation.From = new Thickness(0, 90, 0, 0); // Adjust this value as needed
            animation.To = new Thickness(0, 42, 0, 0);
            animation.Duration = TimeSpan.FromSeconds(0.3); // Adjust the duration as needed
            dtPnl.BeginAnimation(MarginProperty, animation);

            DoubleAnimation gridHeightAnimation = new DoubleAnimation();
            gridHeightAnimation.From = reportDataGrid.ActualHeight;
            gridHeightAnimation.To = 650; // Adjust this value as needed
            gridHeightAnimation.Duration = TimeSpan.FromSeconds(0.3);
            reportDataGrid.BeginAnimation(HeightProperty, gridHeightAnimation);
            //dtPnl.Height = originalDtPnlHeight;
        }

        private void cmbxRole_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            roleFilter = cmbxRole.SelectedItem?.ToString() ?? "";
            ApplyFiltersAndUpdateDataGrid();
            if (cmbxRole.SelectedItem != null)
            {
                cmbxRole.Tag = "";
                return;
            }
        }

        private void cmbxTask_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            taskFilter = cmbxTask.SelectedItem?.ToString() ?? "";
            if (cmbxTask.SelectedItem != null)
            {
                ApplyFiltersAndUpdateDataGrid();
                cmbxTask.Tag = "";

            }
        }

        private void reportDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (reportDataGrid.SelectedItem != null)
                {
                    DataRowView row_selected = (DataRowView)reportDataGrid.SelectedItem;
                    // Extract relevant data from the selected row
                    string fullName = row_selected["fName"].ToString() + " " + row_selected["lName"].ToString();
                    string role = row_selected["roleName"].ToString();
                    string inforID = row_selected["inforID"].ToString();

                    LoadEmployeeReportHistory(fullName, role, inforID);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private string GetEmployeeId(string employeeName)
        {
            string? employeeId = null; // Initialize employeeId to null
            try
            {
                using (OleDbConnection connection = dataConnection.GetConnection()) // Create a connection using DataConnection
                {
                    // Modified query to concatenate fName and lName
                    string query = "SELECT empID FROM Employee WHERE fName & ' ' & lName = ? AND email = ?";

                    using (OleDbCommand command = new OleDbCommand(query, connection)) // Create a command with the query and connection
                    {
                        command.Parameters.AddWithValue("@employeeName", employeeName); // Add parameter for employee name
                        connection.Open(); // Open the connection
                        object? result = command.ExecuteScalar(); // Execute the query and get the result

                        if (result != null) // Check if the result is not null
                        {
                            employeeId = result.ToString(); // Assign the employee ID to employeeId
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving employee ID: " + ex.Message); // Display error message if an exception occurs
            }
            // Return employeeId if not null, otherwise throw an exception
            return employeeId ?? throw new Exception("Employee ID not found.");
        }
        private void AddDataGridColumnsEmpReport() //Columns for specific employee details
        {
            // Create DataGrid columns
            scheduleDataGrid1.Columns.Add(new DataGridTextColumn
            {
                Header = "Task",
                Binding = new Binding("taskName"),
                Width = 125
            });
            scheduleDataGrid1.Columns.Add(new DataGridTextColumn
            {
                Header = "Time In",
                Binding = new Binding("timeIn")
            });
            scheduleDataGrid1.Columns.Add(new DataGridTextColumn
            {
                Header = "Time Out",
                Binding = new Binding("timeOut")
            });
            scheduleDataGrid1.Columns.Add(new DataGridTextColumn
            {
                Header = "Date Earned",
                Binding = new Binding("plannedEnd"),
                Width = 100
            });
            scheduleDataGrid1.Columns.Add(new DataGridTextColumn
            {
                Header = "CTO Earned",
                Binding = new Binding("ctoEarned"),
                Width = 100
            });
            scheduleDataGrid1.Columns.Add(new DataGridTextColumn
            {
                Header = "CTO Used",
                Binding = new Binding("ctoUse"),
                Width = 100
            });
            scheduleDataGrid1.Columns.Add(new DataGridTextColumn
            {
                Header = "CTO Used Description",
                Binding = new Binding("useDesc"),
                Width = 350
            });
            scheduleDataGrid1.Columns.Add(new DataGridTextColumn
            {
                Header = "CTO Balance",
                Binding = new Binding("ctoBalance")
            });
        }
        private void LoadEmployeeReportHistory(string fullName, string role, string inforID)
        {
            //string employeeId = GetEmployeeId(fullName);

            lblEmpName.Content = fullName;
            lblID.Content = inforID;
            lblRole.Content = role;
            //lblContactNum.Content = contactNum;
            //lblEmail.Content = email;

            try
            {
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    // Your code to load the report for employees' history
                    // Modify your query to retrieve employees' history
                    string query = @"SELECT Task.taskName, FORMAT(Schedule.timeIn, 'h:mm AM/PM') AS timeIn," +
                        " FORMAT(Schedule.timeOut, 'h:mm AM/PM') AS timeOut, FORMAT(Schedule.plannedEnd, 'MM/DD/YY') AS plannedEnd," +
                        " ctoEarned, CTOuse.ctoUse, CTOuse.useDesc, ctoBalance FROM ((Schedule INNER JOIN Employee ON Schedule.empID = Employee.empID )" +
                        "INNER JOIN Task ON Schedule.taskID = Task.taskID)"+ 
                        " LEFT JOIN CTOuse ON Schedule.schedID = CTOuse.schedID WHERE Employee.inforID = ?;";

                    
                    using (OleDbCommand command = new OleDbCommand(query, connection)) // Create a command with the query and connection
                    {
                        command.Parameters.AddWithValue("@inforID", inforID);
                        OleDbDataAdapter adapter = new OleDbDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        // Set visibility of EmpFilPnl to visible
                        //EmpFilPnl.Visibility = System.Windows.Visibility.Visible;
                        //Check if any rows were returned with completed = -1
                        /*if (cmbxMoY.SelectedItem != null && cmbxMoY.SelectedItem.ToString() == "Month/Year")
                        {
                            // Check if the date picker has a selected date
                            if (dtEmpDate.SelectedDate.HasValue)
                            {
                                DateTime selectedDate = dtEmpDate.SelectedDate.Value;
                                query += $" AND (MONTH(Schedule.plannedEnd) = {selectedDate.Month} AND YEAR(Schedule.plannedEnd) = {selectedDate.Year})";
                            }
                        }
                        else if (cmbxEmpMoY.SelectedItem != null && cmbxEmpMoY.SelectedItem.ToString() == "Year")
                        {
                            // Get the selected year from the date picker
                            int selectedYear = dtEmpDate.SelectedDate.HasValue ? dtEmpDate.SelectedDate.Value.Year : DateTime.Now.Year;

                            // Apply the "Year" filter to the query
                            query += $" AND YEAR(Schedule.plannedEnd) = {selectedYear}";
                        }
                        // Execute the query and update the DataGrid
                        LoadEmployeeReportHistory(fullName, role, empID) ;*/
                        if (dataTable.Rows.Count > 0)
                        {
                            // Bind the DataTable to the DataGrid
                            scheduleDataGrid1.ItemsSource = dataTable.DefaultView;

                            AllViewPnl.Visibility = Visibility.Collapsed;
                            EmpFilPnl.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            // Display a message indicating the task is not yet completed
                            MessageBox.Show("This task is not yet completed.", "Information");
                        }
                        if (!columnsAddedemp)
                        {
                            AddDataGridColumnsEmpReport();
                            columnsAddedemp = true; // Set the flag to true after adding columns
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            AllViewPnl.Visibility = Visibility.Visible;
            EmpFilPnl.Visibility = Visibility.Collapsed;
        }

        private void btnExportEmp_Click(object sender, RoutedEventArgs e)
        {
            // Create a SaveFileDialog to choose the output path
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*";
            saveFileDialog.FileName = $"{lblEmpName.Content}_{DateTime.Now:yyyyMMdd}.pdf";

            if (saveFileDialog.ShowDialog() == true)
            {
                string outputPath = saveFileDialog.FileName;

                // Create a Document
                Document doc = new Document();

                try
                {
                    // Initialize the PdfWriter with the document and a file stream
                    PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(outputPath, FileMode.Create));

                    // Open the document
                    doc.Open();

                    // Add Header with Company Information
                    PdfPTable headerTable = new PdfPTable(1);
                    headerTable.WidthPercentage = 100;


                    // Add company logo (assuming logoPath is the path to the company logo)
                    iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(imagePath);
                    logo.ScaleToFit(50f, 50f); // Adjust size as needed
                    PdfPCell logoCell = new PdfPCell(logo);
                    logoCell.HorizontalAlignment = Element.ALIGN_LEFT;
                    logoCell.Border = PdfPCell.NO_BORDER;
                    headerTable.AddCell(logoCell);
                    doc.Add(new iTextSharp.text.Paragraph(" "));

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
                    doc.Add(new Chunk(new LineSeparator(0.5f, 100f, BaseColor.BLACK, Element.ALIGN_CENTER, -1)));


                    // Add labels to the document
                    doc.Add(new iTextSharp.text.Paragraph($"Name: {lblEmpName.Content}"));
                    doc.Add(new iTextSharp.text.Paragraph($"Role: {lblRole.Content}"));
                    doc.Add(new iTextSharp.text.Paragraph($"ID: {lblID.Content}"));
                    doc.Add(new iTextSharp.text.Paragraph(" ")); // Add an empty paragraph
                    doc.Add(new iTextSharp.text.Paragraph("History: ")); // Add an empty paragraph
                    doc.Add(new iTextSharp.text.Paragraph(" ")); // Add an empty paragraph

                    // Add DataGrid content to the PDF document
                    PdfPTable pdfTable = new PdfPTable(scheduleDataGrid1.Columns.Count);
                    pdfTable.WidthPercentage = 100;

                    // Define a style for the header column
                    Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.BLACK);
                    Font cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 7); // Adjust font size here

                    // Add headers
                    foreach (DataGridColumn column in scheduleDataGrid1.Columns)
                    {
                        // Get the header text of the column
                        string columnHeader = column.Header.ToString();

                        // Add the column header to the PDF table
                        PdfPCell headerCell = new PdfPCell(new Phrase(columnHeader, headerFont));
                        headerCell.BackgroundColor = new BaseColor(230, 230, 250); // Set background color to a shade of blue
                        headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
                        headerCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        headerCell.Padding = 3;
                        pdfTable.AddCell(headerCell);
                    }
                    // Iterate through the rows and add the corresponding cell data
                    foreach (var item in scheduleDataGrid1.Items)
                    {
                        var row = scheduleDataGrid1.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                        if (row != null)
                        {
                            foreach (DataGridColumn column in scheduleDataGrid1.Columns)
                            {
                                var cellContent = column.GetCellContent(item) as TextBlock;
                                if (cellContent != null)
                                {
                                    // Get the text content of the TextBlock
                                    string cellText = cellContent.Text;

                                    // Add the text content to the PDF table
                                    PdfPCell cellToAdd = new PdfPCell(new Phrase(cellText, cellFont));
                                    cellToAdd.HorizontalAlignment = Element.ALIGN_CENTER;
                                    cellToAdd.VerticalAlignment = Element.ALIGN_MIDDLE;
                                    pdfTable.AddCell(cellToAdd);
                                }
                            }
                        }
                    }

                    // Add table to document
                    doc.Add(pdfTable);

                    PdfPTable footerTable = new PdfPTable(1);
                    footerTable.TotalWidth = doc.PageSize.Width - doc.LeftMargin - doc.RightMargin;
                    footerTable.DefaultCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    PdfPCell footerCell = new PdfPCell(new Phrase("Date generated: " + DateTime.Now.ToString("MM/dd/yyyy")));
                    footerCell.Border = 0;
                    footerTable.AddCell(footerCell);
                    footerTable.WriteSelectedRows(0, -1, doc.LeftMargin, doc.BottomMargin + 10, writer.DirectContent);

                    MessageBox.Show("PDF exported successfully! Output path: " + outputPath, "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error exporting PDF: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                finally
                {

                    // Close the document
                    doc.Close();
                }

            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtschFname.Text = "";
            cmbxRole.SelectedIndex = -1;
            cmbxRole.Tag = "Role";
            cmbxTask.SelectedIndex = -1;
            cmbxTask.Tag = "Task";
            rbBalance.IsChecked = false;
            rbUsed.IsChecked = false;
            dtEDate.SelectedDate = null;
            cmbxEmpMoY.SelectedIndex = 0;
            //dtUDate.SelectedDate = null;
            PopulateTaskComboBox();
            DataReportView();
        }
        private void ApplyFilters2()
        {
            string inforID = Convert.ToString(lblID.Content);
            try
            {
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    // Your code to load the report for employees' history
                    // Modify your query to retrieve employees' history
                    string query = @"SELECT Task.taskName, FORMAT(Schedule.timeIn, 'h:mm AM/PM') AS timeIn," +
                        " FORMAT(Schedule.timeOut, 'h:mm AM/PM') AS timeOut, FORMAT(Schedule.plannedEnd, 'MM/DD/YY') AS plannedEnd," +
                        " ctoEarned, CTOuse.ctoUse, CTOuse.useDesc, ctoBalance FROM ((Schedule INNER JOIN Employee ON Schedule.empID = Employee.empID )" +
                        "INNER JOIN Task ON Schedule.taskID = Task.taskID)" +
                        " LEFT JOIN CTOuse ON Schedule.schedID = CTOuse.schedID WHERE Employee.inforID = ?";

                    if (rbEmpBalance.IsChecked == true)
                    {
                        query += " AND (Schedule.ctoBalance > 0)";
                    }
                    if (rbEmpUsed.IsChecked == true)
                    {
                        query += " AND (CTOuse.ctoUse > 0)";
                    }
                    if (cmbxEmpMoY.SelectedItem != null && cmbxEmpMoY.SelectedItem.ToString() == "Month/Year")
                    {
                        // Check if the date picker has a selected date
                        if (dtEmpDate.SelectedDate.HasValue)
                        {
                            DateTime selectedDate = dtEmpDate.SelectedDate.Value;
                            query += $" AND (MONTH(Schedule.plannedEnd) = {selectedDate.Month} AND YEAR(Schedule.plannedEnd) = {selectedDate.Year})";
                        }
                    }
                    else if (cmbxEmpMoY.SelectedItem != null && cmbxEmpMoY.SelectedItem.ToString() == "Year")
                    {
                        // Get the selected year from the date picker
                        int selectedYear = dtEmpDate.SelectedDate.HasValue ? dtEmpDate.SelectedDate.Value.Year : DateTime.Now.Year;

                        // Apply the "Year" filter to the query
                        query += $" AND YEAR(Schedule.plannedEnd) = {selectedYear}";
                    }

                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection)) // Create a command with the query and connection
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@inforID", inforID);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        

                        // Bind the DataTable to the DataGrid
                        scheduleDataGrid1.ItemsSource = dataTable.DefaultView;


                        if (!columnsAddedemp)
                        {
                            AddDataGridColumnsEmpReport();
                            columnsAddedemp = true; // Set the flag to true after adding columns
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

        }

        private void dtEDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFiltersAndUpdateDataGrid();
        }
        private string EoUFilter;

        private void cmbxEoU_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //roleFilter = cmbxRole.SelectedItem?.ToString() ?? "";
            //ApplyFiltersAndUpdateDataGrid();
            //if (cmbxEoU.SelectedItem != null)
            //{
            //    cmbxEoU.Tag = "";
            //    return;
            //}
        }

        private void EmpFilPnl_Loaded(object sender, RoutedEventArgs e)
        {
            //PopulateMoYComboBox();
        }

        private void cmbxMoY_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFiltersAndUpdateDataGrid();
        }

        private void cmbxEmpMoY_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters2();
        }
        


        private void rbEmpUsed_Checked(object sender, RoutedEventArgs e)
        {
            rbEmpUsed.IsChecked = true;
            ApplyFilters2();
        }

        private void rbEmpBalance_Checked(object sender, RoutedEventArgs e)
        {
            rbEmpBalance.IsChecked = true;
            ApplyFilters2();
        }

        private void rbEmpEarned_Checked(object sender, RoutedEventArgs e)
        {
            rbEmpEarned.IsChecked = true;
            ApplyFilters2();
        }

        private void btnEmpClear_Click(object sender, RoutedEventArgs e)
        {
            string fullname = Convert.ToString(lblEmpName.Content);
            string role = Convert.ToString(lblRole.Content);
            string inforID = Convert.ToString(lblID.Content);
            rbEmpBalance.IsChecked = false;
            rbEmpUsed.IsChecked = false;
            dtEmpDate.SelectedDate = null;
            cmbxEmpMoY.SelectedIndex = 0;
            //dtUDate.SelectedDate = null;
            LoadEmployeeReportHistory(fullname, role, inforID);
        }

        private void dtEmpDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters2();
        }

        private void txtschFname_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (txtschFname.SelectedItem != null)
            {
                ApplyFiltersAndUpdateDataGrid();
                PopulateTaskEmpComboBox();
            }
            
        }
    }
}
