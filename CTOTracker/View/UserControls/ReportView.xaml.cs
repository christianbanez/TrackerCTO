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

namespace CTOTracker.View.UserControls
{
    /// <summary>
    /// Interaction logic for ReportView.xaml
    /// </summary>
    public partial class ReportView : UserControl
    {
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
            txtschFname.TextChanged += txtschFname_TextChanged;
            chkbxBalance.Checked += (sender, e) => ApplyFiltersAndUpdateDataGrid();
            chkbxBalance.Unchecked += (sender, e) => ApplyFiltersAndUpdateDataGrid();
            chkbxUsed.Checked += (sender, e) => ApplyFiltersAndUpdateDataGrid();
            chkbxUsed.Unchecked += (sender, e) => ApplyFiltersAndUpdateDataGrid();
            cmbxTask.SelectionChanged += cmbxTask_SelectionChanged;
            cmbxRole.SelectionChanged += cmbxRole_SelectionChanged;
            EmpFilPnl.Visibility = Visibility.Collapsed;
            PopulateRoleComboBox();
            PopulateTaskComboBox();
        }
        private void DataReportView()
        {
            string query = "SELECT Employee.inforID, Employee.fName, Employee.lName, Role.roleName, Task.taskName, Format(plannedEnd, 'MM/dd/yyyy') AS plannedEnd, Schedule.ctoEarned, Format(dateUsed, 'MM/dd/yyyy') AS dateUsed, Schedule.ctoUsed, Schedule.ctoBalance " +
                "FROM (Role INNER JOIN Employee ON Role.roleID = Employee.roleID) " +
                "INNER JOIN (Task INNER JOIN Schedule ON Task.taskID = Schedule.taskID) " +
                "ON Employee.empID = Schedule.empID WHERE completed = -1;";
            LoadAllData(query);

        }

        private bool columnsAdded = false;
        private bool columnsAddedemp = false;
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
                        return;
                    }
                    /*else if (dataTable.Rows.Count == 0)
                    {
                        if (!string.IsNullOrEmpty(nameFilter))
                        {
                            MessageBox.Show("No data found for the specified name.", "Information");
                            txtschFname.Text = "";
                            
                        }
                        else if (!string.IsNullOrEmpty(taskFilter))
                        {
                            MessageBox.Show("No data found for the specified task.", "Information");
                            cmbxTask.SelectedIndex = -1;
                            cmbxTask.Tag = "Task";
                           
                        }
                        else if (!string.IsNullOrEmpty(roleFilter))
                        {
                            MessageBox.Show("No data found for the specified role.", "Information");
                            cmbxRole.SelectedIndex = -1;
                            cmbxRole.Tag = "Role";
                            
                        }
                        else if (!(dtEDate.SelectedDate.HasValue))
                        {
                            MessageBox.Show("No date selected for the date used filter.", "Information");
                            dtEDate.SelectedDate = null;
                            return;
                        }
                        else if (!(dtUDate.SelectedDate.HasValue))
                        {
                            MessageBox.Show("No date selected for the date used filter.", "Information");
                            dtUDate.SelectedDate = null; 
                        }
                        else
                        {
                            MessageBox.Show("No data found for the specified filters.", "Information");// Clear filter fields
                            chkbxBalance.IsChecked = false;
                            chkbxUsed.IsChecked = false;
                        }
                        DataReportView();
                    }*/
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
        private void ApplyFiltersAndUpdateDataGrid()
        {
            // Construct the query based on the selected filters
            string query = "SELECT Employee.inforID, Employee.fName, Employee.lName, Role.roleName, Task.taskName, FORMAT(Schedule.plannedEnd, 'MM/DD/YY') AS plannedEnd, Schedule.ctoEarned, FORMAT(Schedule.dateUsed, 'MM/DD/YY') AS dateUsed, Schedule.ctoUsed, Schedule.ctoBalance " +
                           "FROM (Role INNER JOIN Employee ON Role.roleID = Employee.roleID) " +
                           "INNER JOIN (Task INNER JOIN Schedule ON Task.taskID = Schedule.taskID) " +
                           "ON Employee.empID = Schedule.empID WHERE 1=1 AND completed = -1"; // Start with a dummy condition

            try
            {
                if (!string.IsNullOrEmpty(nameFilter))
                {
                    query += $" AND (Employee.fName LIKE '{nameFilter}%' OR Employee.lName LIKE '{nameFilter}%')";
                }
                if (!string.IsNullOrEmpty(taskFilter))
                {
                    query += $" AND Task.taskName = '{taskFilter}'";
                }
                if (!string.IsNullOrEmpty(roleFilter))
                {
                    query += $" AND Role.roleName = '{roleFilter}'";
                }
                if (chkbxBalance.IsChecked == true)
                {
                    query += " AND Schedule.ctoBalance > 0";
                }
                if (chkbxUsed.IsChecked == true)
                {
                    query += " AND Schedule.ctoUsed > 0";
                }
                if (dtEDate.SelectedDate.HasValue)
                {
                    DateTime selectedDate = dtEDate.SelectedDate.Value;
                    query += $" AND (MONTH(Schedule.plannedEnd) = {selectedDate.Month} AND YEAR(Schedule.plannedEnd) = {selectedDate.Year})";
                }
                if (dtUDate.SelectedDate.HasValue)
                {
                    DateTime selectedDate = dtUDate.SelectedDate.Value;
                    query += $" AND (MONTH(Schedule.dateUsed) = {selectedDate.Month} AND YEAR(Schedule.dateUsed) = {selectedDate.Year})";
                }
                
                // Execute the query and update the DataGrid
                LoadAllData(query);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error");
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
        private void ExportToPdf(DataTable dataTable, string outputPath)
        {
            // Check if there is data to export
            if (dataTable == null || dataTable.Rows.Count == 0)
            {
                MessageBox.Show("No data available for export.", "Information");
                return;
            }

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

                    // Add DataGrid content to the PDF document
                    PdfPTable pdfTable = new PdfPTable(dataTable.Columns.Count);
                    pdfTable.WidthPercentage = 100;

                    // Add headers
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        PdfPCell headerCell = new PdfPCell(new Phrase(column.ColumnName.ToString(), headerFont));
                        headerCell.BackgroundColor = new BaseColor(51, 122, 183); // Set background color to a shade of blue
                        headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
                        headerCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        headerCell.Padding = 3;
                        pdfTable.AddCell(headerCell);
                    }

                    // Add data rows
                    foreach (DataRow row in dataTable.Rows)
                    {
                        foreach (var cell in row.ItemArray)
                        {
                            PdfPCell cellToAdd = new PdfPCell(new Phrase(cell.ToString(), cellFont));
                            cellToAdd.HorizontalAlignment = Element.ALIGN_CENTER;
                            cellToAdd.VerticalAlignment = Element.ALIGN_MIDDLE;
                            pdfTable.AddCell(cellToAdd);
                        }
                    }

                    // Add table to document
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

        private void chkbxBalance_Checked(object sender, RoutedEventArgs e)
        {
            chkbxUsed.IsChecked = false;
            ApplyFiltersAndUpdateDataGrid();
        }
        private void chkbxUsed_Checked(object sender, RoutedEventArgs e)
        {
            chkbxBalance.IsChecked = false;
            ApplyFiltersAndUpdateDataGrid();
        }

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
                    string empID = row_selected["inforID"].ToString();

                    LoadEmployeeReportHistory(fullName, role, empID);
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
                    string query = "SELECT empID FROM Employee WHERE fName & ' ' & lName = ?";

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
                Binding = new Binding("ctoUsed"),
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
        private void LoadEmployeeReportHistory(string fullName, string role, string empID)
        {
            string employeeId = GetEmployeeId(fullName);

            lblEmpName.Content = fullName;
            lblID.Content = empID;
            lblRole.Content = role;
            //lblContactNum.Content = contactNum;
            //lblEmail.Content = email;

            try
            {
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    // Your code to load the report for employees' history
                    // Modify your query to retrieve employees' history
                    string query = @"SELECT Task.taskName, Format(timeIn, 'h:mm AM/PM') AS timeIn,  Format(timeout, 'h:mm AM/PM') AS timeOut, FORMAT(Schedule.plannedEnd, 'MM/DD/YY') AS plannedEnd, ctoEarned, ctoUsed, useDesc, ctoBalance FROM (Schedule INNER JOIN Employee ON Schedule.empID = Employee.empID)" +
                                   "INNER JOIN Task ON Schedule.taskID = Task.taskID WHERE completed = -1 AND Employee.empID = ?;";

                    using (OleDbCommand command = new OleDbCommand(query, connection)) // Create a command with the query and connection
                    {
                        command.Parameters.AddWithValue("@employeeId", employeeId);
                        OleDbDataAdapter adapter = new OleDbDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        // Set visibility of EmpFilPnl to visible
                        //EmpFilPnl.Visibility = System.Windows.Visibility.Visible;
                        //Check if any rows were returned with completed = -1
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
            AllViewPnl.Visibility=Visibility.Visible;
            EmpFilPnl.Visibility = Visibility.Collapsed;
        }

        private void btnExportEmp_Click(object sender, RoutedEventArgs e)
        {
            // Create a SaveFileDialog to choose the output path
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*";
            saveFileDialog.FileName = "exported_data.pdf"; // Default file name

            if (saveFileDialog.ShowDialog() == true)
            {
                string outputPath = saveFileDialog.FileName;

                // Create a Document
                Document doc = new Document();

                try
                {
                    // Initialize the PdfWriter with the document and a file stream
                    PdfWriter.GetInstance(doc, new FileStream(outputPath, FileMode.Create));

                    // Open the document
                    doc.Open();

                    // Add Header with Company Information
                    PdfPTable headerTable = new PdfPTable(1);
                    headerTable.WidthPercentage = 100;
                    // Add current date and time
                    DateTime currentDate = DateTime.Now;
                    doc.Add(new iTextSharp.text.Paragraph("Date generated: " + currentDate.ToString()));

                    // Add company logo (assuming logoPath is the path to the company logo)
                    iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(@"C:\Users\dkeh\source\repos\TrackerCTO\CTOTracker\Images\logo.png");
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
                    Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.WHITE);
                    Font cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 7); // Adjust font size here

                    // Add headers
                    foreach (DataGridColumn column in scheduleDataGrid1.Columns)
                    {
                        // Get the header text of the column
                        string columnHeader = column.Header.ToString();

                        // Add the column header to the PDF table
                        PdfPCell headerCell = new PdfPCell(new Phrase(columnHeader, headerFont));
                        headerCell.BackgroundColor = new BaseColor(51, 122, 183); // Set background color to a shade of blue
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

                    MessageBox.Show("PDF exported successfully! Output path: " + outputPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error exporting PDF: " + ex.Message);
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
            chkbxBalance.IsChecked = false;
            chkbxUsed.IsChecked = false;
            dtEDate.SelectedDate = null;
            dtUDate.SelectedDate = null;
            DataReportView();
        }

        private void dtEDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFiltersAndUpdateDataGrid();
        }

        private void dtUDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFiltersAndUpdateDataGrid();
        }

        
    }
}
