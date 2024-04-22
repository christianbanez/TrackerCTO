using System.Data.OleDb;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using Microsoft.Win32;
using System.Xml.Linq;


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
            string query = "SELECT Employee.inforID, Employee.fName, Employee.lName, Employee.email, Employee.contact, Role.roleName, Schedule.ctoBalance\r\nFROM (Role INNER JOIN Employee ON Role.roleID = Employee.roleID) INNER JOIN Schedule ON Employee.empID = Schedule.empID;\r\n";
            LoadEmployeeReport(query);

        }
        private void LoadEmployeeReport(string query) //loads the employee report to report data grid
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
        } //filter combo box (prev version)
        private List<string> GetDataFromEmployeeTable()
        {
            // Create a list to store employee names
            List<string> employees = new List<string>();

            try
            {
                // Get connection from DataConnection
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    // Define the SQL query to select first names (fName) and last names (lName) from the Employee table
                    string query = "SELECT fName, lName FROM Employee";

                    // Create a command object with the query and connection
                    using (OleDbCommand command = new OleDbCommand(query, connection))
                    {
                        // Open the connection to the database
                        connection.Open();

                        // Execute the command and retrieve data using a data reader
                        using (OleDbDataReader reader = command.ExecuteReader())
                        {
                            // Iterate through the data reader to read each row
                            while (reader.Read())
                            {
                                // Check if the fName and lName columns contain non-null values
                                if (!reader.IsDBNull(reader.GetOrdinal("fName")) && !reader.IsDBNull(reader.GetOrdinal("lName")))
                                {
                                    // Concatenate the first name and last name to form the full name
                                    string fullName = $"{reader["fName"]} {reader["lName"]}";
                                    // Add the full name to the list of employees
                                    employees.Add(fullName);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Display an error message if an exception occurs
                MessageBox.Show("Error: " + ex.Message);
            }

            // Return the list of employee names retrieved from the database
            return employees;
        }
        private void PopulateEmployeeComboBox()
        {
            try
            {
                // Fetch data from the Employee table
                allEmployees = GetDataFromEmployeeTable();

                // Check if 'allEmployees' is null before binding to the ComboBox
                if (allEmployees != null)
                {
                    cmbxEmpName.ItemsSource = allEmployees;
                }
                else
                {
                    // Handle the case when 'allEmployees' is null
                    MessageBox.Show("No employees found.");
                }
            }
            catch (Exception ex)
            {
                // Display an error message if an exception occurs
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        private void cmbxEmpName_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Clear the filtered list
            filteredEmployees.Clear();

            string searchText = cmbxEmpName.Text.ToLower();

            // Filter the items in the ComboBox based on the entered text
            foreach (string employee in allEmployees)
            {
                if (employee.ToLower().Contains(searchText))
                {
                    filteredEmployees.Add(employee);
                }
            }

            // Update the ComboBox items source with the filtered list
            cmbxEmpName.ItemsSource = filteredEmployees;

            // Open the dropdown
            cmbxEmpName.IsDropDownOpen = true;
        }

        private void reportDG_DoubleMouseClick(object sender, MouseButtonEventArgs e)
        { 
            try
            {
                // Retrieve the selected row (data item)
                //DataGrid gd = (DataGrid)sender;
                
                if (reportDataGrid.SelectedItem != null)
                {
                    DataRowView row_selected = (DataRowView)reportDataGrid.SelectedItem;

                    // Extract relevant data from the selected row
                    string fullName = row_selected["fName"].ToString() + " " + row_selected["lName"].ToString();
                    string role = row_selected["roleName"].ToString();
                    string contactNum = row_selected["contact"].ToString();
                    string email = row_selected["email"].ToString();
                    string empID = row_selected["inforID"].ToString();

                    LoadEmployeeReportHistory(fullName, role, contactNum, email, empID);
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
        private void LoadEmployeeReportHistory(string fullName, string role, string contactNum, string email, string empID)
        {
            string employeeId = GetEmployeeId(fullName);

            lblEmpName.Content = fullName;
            lblID.Content = empID;
            lblRole.Content = role;
            lblContactNum.Content = contactNum;
            lblEmail.Content = email;

            try
            {
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    // Your code to load the report for employees' history
                    // Modify your query to retrieve employees' history
                    string query = @"SELECT Task.taskName, timeIn, timeOut, ctoEarned, ctoUsed, dateUsed, ctoBalance FROM (Schedule INNER JOIN Employee ON Schedule.empID = Employee.empID)" +
                                   "INNER JOIN Task ON Schedule.taskID = Task.taskID WHERE completed = -1 AND Employee.empID = ?;";

                    using (OleDbCommand command = new OleDbCommand(query, connection)) // Create a command with the query and connection
                    {
                        command.Parameters.AddWithValue("@employeeId", employeeId);
                        OleDbDataAdapter adapter = new OleDbDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        // Check if any rows were returned with completed = -1
                        if (dataTable.Rows.Count > 0)
                        {
                            // Bind the DataTable to the DataGrid
                            scheduleDataGrid1.ItemsSource = dataTable.DefaultView;
                            // Set visibility of EmpFilPnl to visible
                            EmpFilPnl.Visibility = System.Windows.Visibility.Visible;
                        }
                        else
                        {
                            // Display a message indicating the task is not yet completed
                            MessageBox.Show("This task is not yet completed.", "Information");
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
            EmpFilPnl.Visibility = System.Windows.Visibility.Collapsed;
        }
        /*private void ExportToPdfButton_Click(object sender, RoutedEventArgs e)
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

                    // Add labels to the document
                    doc.Add(new iTextSharp.text.Paragraph($"Name: {lblEmpName.Content}"));
                    doc.Add(new iTextSharp.text.Paragraph($"Email: {lblEmail.Content}"));
                    doc.Add(new iTextSharp.text.Paragraph($"Role: {lblRole.Content}"));
                    doc.Add(new iTextSharp.text.Paragraph($"ID: {lblID.Content}"));

                    // Add a table to the document
                    PdfPTable table = new PdfPTable(reportDataGrid.Columns.Count);
                    table.WidthPercentage = 100;

                    // Add headers
                    for (int i = 0; i < reportDataGrid.Columns.Count; i++)
                    {
                        table.AddCell(new PdfPCell(new Phrase(reportDataGrid.Columns[i].Header.ToString())));
                    }

                    // Add data rows
                    for (int i = 0; i < reportDataGrid.Items.Count; i++)
                    {
                        DataRowView rowView = (DataRowView)reportDataGrid.Items[i];
                        DataRow row = rowView.Row;

                        for (int j = 0; j < reportDataGrid.Columns.Count; j++)
                        {
                            table.AddCell(new PdfPCell(new Phrase(row[j].ToString())));
                        }
                    }

                    // Add table to document
                    doc.Add(table);

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
        }*/

        private void btnExport_Click(object sender, RoutedEventArgs e)
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

                    // Add labels to the document
                    doc.Add(new iTextSharp.text.Paragraph($"Name: {lblEmpName.Content}"));
                    doc.Add(new iTextSharp.text.Paragraph($"Email: {lblEmail.Content}"));
                    doc.Add(new iTextSharp.text.Paragraph($"Role: {lblRole.Content}"));
                    doc.Add(new iTextSharp.text.Paragraph($"ID: {lblID.Content}"));

                    


                    // Add DataGrid content to the PDF document
                    PdfPTable pdfTable = new PdfPTable(reportDataGrid.Columns.Count);
                    pdfTable.WidthPercentage = 100;

                    // Define a style for the header column
                    Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.WHITE);
                    Font cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 7); // Adjust font size here

                    // Add headers
                    foreach (DataGridColumn column in reportDataGrid.Columns)
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
                    foreach (var item in reportDataGrid.Items)
                    {
                        var row = reportDataGrid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                        if (row != null)
                        {
                            foreach (DataGridColumn column in reportDataGrid.Columns)
                            {
                                object cellData = column.GetCellContent(item);
                                PdfPCell cellToAdd = new PdfPCell(new Phrase(cellData.ToString(), cellFont));
                                cellToAdd.HorizontalAlignment = Element.ALIGN_CENTER;
                                cellToAdd.VerticalAlignment = Element.ALIGN_MIDDLE;
                                pdfTable.AddCell(cellToAdd);
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

    }//main load
} //namespace
