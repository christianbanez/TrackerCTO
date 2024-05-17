﻿using System.Data;
using System.Data.OleDb;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace CTOTracker.View
{
    /// <summary>
    /// Interaction logic for EmployeeView.xaml
    /// </summary>
    public partial class EmployeeView : UserControl
    {
        private DataConnection dataConnection;

        public EmployeeView()
        {
            InitializeComponent();
            dataConnection = new DataConnection();
            LoadEmployeeView();
            AddPnl.Visibility = Visibility.Collapsed;
            employeeSearch.TextChanged += employeeSearch_TextChanged;
            UpdatePnl.Visibility = Visibility.Collapsed;
            PopulateRoleComboBox();
            btnEdit.IsEnabled = false;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            txtEmpID.Clear();
            txtFname.Clear();
            txtLname.Clear();
            txtEmail.Clear();
            txtContact.Clear();
            txtRole.SelectedIndex = -1;
            txtEmpID.IsEnabled = true;
            txtFname.IsEnabled = true;
            txtLname.IsEnabled = true;
            txtEmail.IsEnabled = true;
            txtContact.IsEnabled = true;
            txtRole.IsEnabled = true;

            AddPnl.Visibility = Visibility.Visible;
            UpdatePnl.Visibility = Visibility.Collapsed;
            AddEdit.Visibility = Visibility.Collapsed;
            DataGridEmployee1.IsEnabled = false;
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            UpdatePnl.Visibility = Visibility.Visible;
            AddPnl.Visibility = Visibility.Collapsed;
            AddEdit.Visibility = Visibility.Collapsed;

            txtEmpID.IsEnabled = false;
            txtFname.IsEnabled = true;
            txtLname.IsEnabled = true;
            txtEmail.IsEnabled = true;
            txtContact.IsEnabled = true;
            txtRole.IsEnabled = true;
            DataGridEmployee1.IsEnabled = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            //MessageBoxResult msgRes = MessageBox.Show("Are you sure?", "Cancel", MessageBoxButton.YesNo);
            //if (msgRes == MessageBoxResult.Yes)
            //{
            btnEdit.IsEnabled = false;
            AddEdit.Visibility = Visibility.Visible;
            AddPnl.Visibility = Visibility.Collapsed;
            UpdatePnl.Visibility = Visibility.Collapsed;
            txtEmpID.IsEnabled = false;
            txtFname.IsEnabled = false;
            txtLname.IsEnabled = false;
            txtEmail.IsEnabled = false;
            txtContact.IsEnabled = false;
            txtRole.IsEnabled = false;
            HideTooltip(txtEmpID);
            HideTooltip(txtFname);
            HideTooltip(txtLname);
            HideTooltip(txtEmail);
            HideTooltip(txtContact);
            HideTooltip(txtRole);
            txtEmpID.Clear();
            txtFname.Clear();
            txtLname.Clear();
            txtEmail.Clear();
            txtContact.Clear();
            txtRole.SelectedIndex = -1;
            DataGridEmployee1.IsEnabled = true;
            DataGridEmployee1.SelectedItem = null;
            //}
        }

        private void btnCancel2_Click(object sender, RoutedEventArgs e)
        {
            //MessageBoxResult msgRes = MessageBox.Show("Are you sure?", "Cancel", MessageBoxButton.YesNo);
            //if (msgRes == MessageBoxResult.Yes)
            //{
            btnEdit.IsEnabled = false;
            AddEdit.Visibility = Visibility.Visible;
            AddPnl.Visibility = Visibility.Collapsed;
            UpdatePnl.Visibility = Visibility.Collapsed;
            txtEmpID.IsEnabled = false;
            txtFname.IsEnabled = false;
            txtLname.IsEnabled = false;
            txtEmail.IsEnabled = false;
            txtContact.IsEnabled = false;
            txtRole.IsEnabled = false;
            HideTooltip(txtEmpID);
            HideTooltip(txtFname);
            HideTooltip(txtLname);
            HideTooltip(txtEmail);
            HideTooltip(txtContact);
            HideTooltip(txtRole);
            txtEmpID.Clear();
            txtFname.Clear();
            txtLname.Clear();
            txtEmail.Clear();
            txtContact.Clear();
            txtRole.SelectedIndex = -1;
            DataGridEmployee1.IsEnabled = true;
            DataGridEmployee1.SelectedItem = null;
            //}
            //dataConnection = new DataConnection();
            LoadEmployeeView();
        }

        private void InsertRoleIntoDatabase(string roleName)
        {
            try
            {
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    string query = "INSERT INTO Role (roleName) VALUES (@roleName)";

                    using (OleDbCommand command = new OleDbCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@roleName", roleName);

                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Role has been added to the database!");
                            
                        }
                        else
                        {
                            MessageBox.Show("Failed to add role to the database.");
                            
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inserting task into database: " + ex.Message);
            }
        }
        private void LoadEmployeeView()
        {
            using (OleDbConnection connection = dataConnection.GetConnection())
            {
                try
                {
                    connection.Open();
                    string query = "SELECT Employee.inforID, fName, lName, email, contact, Role.roleName FROM Employee INNER JOIN Role ON Employee.roleID = Role.roleID";   // Specify the columns you want to retrieve
                    OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
                    DataTable dataTable = new DataTable();          // Retrieve data from the database
                    adapter.Fill(dataTable);

                    if (dataTable != null && dataTable.Rows.Count > 0)  // Check if any data is returned
                    {
                        DataGridEmployee1.ItemsSource = dataTable.DefaultView;     // Bind the DataTable to the DataGridView
                    }
                    
                    // Call the method to open the connection
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

        #region---Input Validation----
        //private bool IsValidEmail(string email)
        //{
        //    try
        //    {
        //        var emailValidator = new System.Net.Mail.MailAddress(email);
        //        return (email.LastIndexOf(".") > email.LastIndexOf("@"));
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        private void ShowTooltip(Control control, string message)
        {
            ToolTip tooltip = control.ToolTip as ToolTip;
            if (tooltip == null)
            {
                tooltip = new ToolTip();
                control.ToolTip = tooltip;
            }

            tooltip.Content = message;
            tooltip.PlacementTarget = control;
            tooltip.Placement = PlacementMode.Bottom;
            tooltip.IsOpen = true;

        }

        private void HideTooltip(Control control)
        {
            if (control.ToolTip is ToolTip tooltip)
            {
                tooltip.IsOpen = false;
                control.ToolTip = null; // Clear the tooltip
            }

        }

        private bool IsNumeric(string input)
        {
            return int.TryParse(input, out _);
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var emailValidator = new System.Net.Mail.MailAddress(email);
                return (email.LastIndexOf(".") > email.LastIndexOf("@"));
            }
            catch
            {
                return false;
            }
        }
        private bool ValidateInput()
        {
            bool isValid = true;
            if (string.IsNullOrEmpty(txtEmpID.Text))
            {

                isValid = false;
            }
            if (string.IsNullOrEmpty(txtFname.Text))
            {

                isValid = false;
            }
            if (string.IsNullOrEmpty(txtLname.Text))
            {

                isValid = false;
            }
            if (!IsValidEmail(txtEmail.Text))
            {
                isValid = false;
            }
            if (!IsValidContact(txtContact.Text))
            {

                isValid = false;
            }
            if (string.IsNullOrEmpty(txtRole.Text))
            {

                isValid = false;
            }
            HideTooltip(txtEmpID);
            HideTooltip(txtFname);
            HideTooltip(txtLname);
            HideTooltip(txtEmail);
            HideTooltip(txtContact);
            HideTooltip(txtRole);
            return isValid;
        }

        private bool IsValidContact(string contactNumber)
        {
            return Regex.IsMatch(contactNumber, @"^09\d{9}$");
        }

        private void txtEmpID_PreviewLostKeyboardFocus(object sender, RoutedEventArgs e)
        {
            TextBox txtEmpID = sender as TextBox;
            if (string.IsNullOrEmpty(txtEmpID.Text))
            {
                ShowTooltip(txtEmpID, "ID cannot be empty.");
            }
            else if (!IsNumeric(txtEmpID.Text))
            {
                ShowTooltip(txtEmpID, "Employee ID must be numeric.");
            }
            else
            {
                HideTooltip(txtEmpID);
            }
        }

        private void txtFname_PreviewLostKeyboardFocus(object sender, RoutedEventArgs e)
        {
            TextBox txtFname = sender as TextBox;
            if (string.IsNullOrEmpty(txtFname.Text))
            {
                ShowTooltip(txtFname, "First Name cannot be empty.");
            }
            else
            {
                HideTooltip(txtFname);
            }
        }

        private void txtLname_PreviewLostKeyboardFocus(object sender, RoutedEventArgs e)
        {
            TextBox txtLname = sender as TextBox;
            if (string.IsNullOrEmpty(txtLname.Text))
            {
                ShowTooltip(txtLname, "Last Name cannot be empty.");
            }
            else
            {
                HideTooltip(txtLname);
            }
        }

        private void txtEmail_PreviewLostKeyboardFocus(object sender, RoutedEventArgs e)
        {
            TextBox txtEmail = sender as TextBox;
            if (string.IsNullOrEmpty(txtEmail.Text))
            {
                ShowTooltip(txtEmail, "Email cannot be empty.");
            }
            else if (!IsValidEmail(txtEmail.Text))
            {
                ShowTooltip(txtEmail, "Please enter a valid email address.");
            }
            else
            {
                HideTooltip(txtEmail);
            }
        }

        private void txtContact_LostKeyboardFocus(object sender, RoutedEventArgs e)
        {
            TextBox txtContact = sender as TextBox;
            if (string.IsNullOrEmpty(txtContact.Text))
            {
                ShowTooltip(txtContact, "Contact cannot be empty.");
            }
            else if (!IsValidContact(txtContact.Text))
            {
                ShowTooltip(txtContact, "Please enter a valid Philippines contact number (09xxxxxxxxx).");
            }
            else
            {
                HideTooltip(txtContact);
            }
        }

        private void txtRole_LostKeyboardFocus(object sender, RoutedEventArgs e)
        {

            if (txtRole.SelectedIndex == -1)
            {
                ShowTooltip(txtRole, "Role cannot be empty.");
            }
            else if (txtRole.Text == "")
            {
                ShowTooltip(txtRole, "Role cannot be empty.");
            }
            else
            {
                HideTooltip(txtRole);
            }
        }

        private void txtEmpID_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char ch in e.Text)
            {
                if (!char.IsDigit(ch))
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void DataGridEmployee1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void PopulateRoleComboBox()
        {
            try
            {
                // Fetch data from the Employee table
                List<string> role = GetDataFromRole();

                // Check if 'role' is null before binding to the ComboBox
                if (role != null)
                {
                    txtRole.ItemsSource = role;
                }
                else
                {
                    // Handle the case when 'role' is null
                    MessageBox.Show("No role found.");
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
            // Create a list to store role names
            List<string> role = new List<string>();

            try
            {
                // Get connection from DataConnection
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    // Define the Access query to select role names from the Role table
                    string query = "SELECT roleName FROM Role";

                    // Create a command object with the query and connection
                    OleDbCommand command = new OleDbCommand(query, connection);

                    // Open the connection to the database
                    connection.Open();

                    // Execute the command and retrieve data using a data reader
                    OleDbDataReader reader = command.ExecuteReader();

                    // Iterate through the data reader to read each row
                    while (reader.Read())
                    {
                        // Check if the roleName column contains non-null values
                        if (!reader.IsDBNull(reader.GetOrdinal("roleName")))
                        {
                            // Get the role name
                            string roleName = reader["roleName"].ToString();

                            // Add the role name to the list
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

            // Return the list of role names retrieved from the database
            return role;
        }

        #endregion

        private void InsertEmployee(string inforID, string firstName, string lastName, string email, string contact, string roleID)
        {
            try
            {
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    connection.Open();

                    using (OleDbCommand cmd = connection.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "INSERT INTO Employee (inforID, fName, lName, email, contact, roleID) " +
                                          "VALUES (@inforID, @firstName, @lastName, @email, @contact, @roleID)";

                        cmd.Parameters.AddWithValue("@inforID", inforID);
                        cmd.Parameters.AddWithValue("@firstName", firstName);
                        cmd.Parameters.AddWithValue("@lastName", lastName);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@contact", contact);
                        cmd.Parameters.AddWithValue("@roleID", roleID);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error");
            }
        }

        private void btnSaveAdd_Click(object sender, RoutedEventArgs e)
        {
            using (OleDbConnection connection = dataConnection.GetConnection())
            {
                try
                {
                    string selectedRole = txtRole.Text.Trim();
                    string roleID = GetRoleID(selectedRole);
                    if (roleID == null)
                    {
                        if (txtRole.Text != "")
                        {
                            // If task ID is null, insert the task into the database
                            InsertRoleIntoDatabase(selectedRole);
                            // Retrieve the task ID again after insertion
                            roleID = GetRoleID(selectedRole);
                            PopulateRoleComboBox();
                        }
                        
                    }

                    string inforID = txtEmpID.Text;
                    connection.Open();
                    // Validate input fields
                    if (!ValidateInput())
                    {
                        MessageBox.Show("Fields cannot be empty.", "Error");
                        return;
                    }
                    // Check for existing inforID
                    using (OleDbCommand cmd = new OleDbCommand("SELECT COUNT(*) FROM Employee WHERE inforID = ?", connection))
                    {
                        cmd.Parameters.AddWithValue("?", inforID);
                        int count = (int)cmd.ExecuteScalar();

                        if (count > 0)
                        {
                            MessageBox.Show("infor ID already exists in the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return; // Exit the method to prevent further execution
                        }
                    }
                    string infor_ID = txtEmpID.Text;
                    string firstName = txtFname.Text;
                    string lastName = txtLname.Text;
                    string email = txtEmail.Text;
                    string contact = txtContact.Text;

                    // Check for existing email or contact
                    using (OleDbCommand command = new OleDbCommand("SELECT COUNT(*) FROM Employee WHERE email = ? OR contact = ?", connection))
                    {
                        command.Parameters.AddWithValue("?", email);
                        command.Parameters.AddWithValue("?", contact);
                        int ct = (int)command.ExecuteScalar();

                        if (ct > 0)
                        {
                            MessageBox.Show("Email or Contact already exists in the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return; // Exit the method to prevent further execution
                        }
                    }
                    
                    InsertEmployee(infor_ID, firstName, lastName, email, contact, roleID);
                    MessageBox.Show("Employee added successfully!");

                    LoadEmployeeView();
                    txtEmpID.Clear();
                    txtFname.Clear();
                    txtLname.Clear();
                    txtEmail.Clear();
                    txtContact.Clear();
                    txtRole.Text = "";
                    txtRole.SelectedIndex = -1;
                    AddEdit.Visibility = Visibility.Visible;
                    AddPnl.Visibility = Visibility.Collapsed;
                    UpdatePnl.Visibility = Visibility.Collapsed;
                    txtEmpID.IsEnabled = false;
                    txtFname.IsEnabled = false;
                    txtLname.IsEnabled = false;
                    txtEmail.IsEnabled = false;
                    txtContact.IsEnabled = false;
                    txtRole.IsEnabled = false;
                    DataGridEmployee1.IsEnabled = true;

                }
                catch (Exception ex) 
                {
                    MessageBox.Show("Error: " + ex);
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        private string GetRoleID(string roleName)
        {
            string? roleID = null; // Initialize taskId to null

            try
            {
                using (OleDbConnection connection = dataConnection.GetConnection()) // Create a connection using DataConnection
                {
                    string query = "SELECT roleID FROM Role WHERE roleName = ?"; // SQL query to retrieve task ID based on task name
                    using (OleDbCommand command = new OleDbCommand(query, connection)) // Create a command with the query and connection
                    {
                        command.Parameters.AddWithValue("@roleName", roleName); // Add parameter for task name
                        connection.Open(); // Open the connection
                        object? result = command.ExecuteScalar(); // Execute the query and get the result

                        if (result != null) // Check if the result is not null
                        {
                            roleID = result.ToString(); // Assign the task ID to taskId
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving role ID: " + ex.Message); // Display error message if an exception occurs
            }

            return roleID; // Return roleID if not null, otherwise throw an exception
        }

        private void txtContact_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char ch in e.Text)
            {
                if (!char.IsDigit(ch))
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void DataGridEmployee1_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            DataGrid gd = (DataGrid)sender;

            if (gd.SelectedItems.Count > 1) // Check if only one row is selected for multiple deletion
            {
                txtEmpID.Clear();
                txtFname.Clear();
                txtLname.Clear();
                txtEmail.Clear();
                txtContact.Clear();
                txtRole.Text = "";
                btnDeleteEmp.Visibility = Visibility.Visible;
                btnDeleteEmp.IsEnabled = true;
            }
            
            if (gd.SelectedItems.Count == 1) // Only one row is selected
            {
                DataRowView row_selected = (DataRowView)gd.SelectedItem;

                // Extract values from the row and populate textboxes
                btnEdit.IsEnabled = true;
                txtEmpID.Text = row_selected["inforID"].ToString();
                txtEmpID.IsEnabled = false;
                txtFname.Text = row_selected["fName"].ToString();
                txtLname.Text = row_selected["lName"].ToString();
                txtEmail.Text = row_selected["email"].ToString();
                txtContact.Text = row_selected["contact"].ToString();
                txtRole.Text = row_selected["roleName"].ToString();
            }
        }

        private void btnSaveUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInput())
                {
                    MessageBox.Show("Fields cannot be empty.", "Error");
                    return;
                }
                string selectedRole = txtRole.SelectedItem?.ToString() ?? txtRole.Text;
                string roleID = GetRoleID(selectedRole);

                if (roleID == null)
                {
                    if (txtRole.Text == "")
                    {
                        return;
                    }
                    else
                    {
                        selectedRole = txtRole.Text.Trim();
                        // If task ID is null, insert the task into the database
                        InsertRoleIntoDatabase(selectedRole);
                        // Retrieve the task ID again after insertion
                        roleID = GetRoleID(selectedRole);
                    }
                    
                }
                PopulateRoleComboBox();
                // Retrieve updated values from input fields
                string inforID = txtEmpID.Text;
                string firstName = txtFname.Text;
                string lastName = txtLname.Text;
                string email = txtEmail.Text;
                string contact = txtContact.Text;

                // Update the employee record in the database
                UpdateEmployee(inforID, firstName, lastName, email, contact, roleID);
                btnEdit.IsEnabled = false;

                // Refresh the DataGridView to reflect the changes
                LoadEmployeeView();
                txtRole.SelectedIndex = -1;
                AddEdit.Visibility = Visibility.Visible;
                AddPnl.Visibility = Visibility.Collapsed;
                UpdatePnl.Visibility = Visibility.Collapsed;
                txtFname.IsEnabled = false;
                txtLname.IsEnabled = false;
                txtEmail.IsEnabled = false;
                txtContact.IsEnabled = false;
                txtRole.IsEnabled = false;
                txtEmpID.Clear();
                txtFname.Clear();
                txtLname.Clear();
                txtEmail.Clear();
                txtContact.Clear();
                txtRole.SelectedIndex = -1;
                txtRole.Text = "";
                DataGridEmployee1.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating employee: " + ex.Message, "Error");
            }
        }
        private void UpdateEmployee(string inforID, string firstName, string lastName, string email, string contact, string roleID)
        {
            using (OleDbConnection connection = dataConnection.GetConnection())
            {
                try
                {
                    connection.Open();
                    string fetchQuery = "SELECT inforID, fName, lName, email, contact, roleID FROM Employee WHERE inforID = @inforID";
                    using (OleDbCommand fetchCommand = new OleDbCommand(fetchQuery, connection))
                    {
                        fetchCommand.Parameters.AddWithValue("@inforID", inforID);
                        using (OleDbDataReader reader = fetchCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Assuming you have getters that parse the reader into appropriate types
                                if (reader["inforID"].ToString() == inforID &&
                                    reader["fName"].ToString() == firstName &&
                                    reader["lName"].ToString() == lastName &&
                                    reader["email"].ToString() == email &&
                                    reader["contact"].ToString() == contact &&
                                    reader["roleID"].ToString() == roleID)
                                {
                                    MessageBox.Show("No changes detected to update.");
                                    return;
                                }
                            }
                        }
                        connection.Close();
                    }
                    if (DataGridEmployee1.SelectedItem != null)
                    {
                        connection.Open();
                        using (OleDbCommand cmd = connection.CreateCommand())
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = "UPDATE Employee SET fName = @firstName, lName = @lastName, email = @email, contact = @contact, roleID = @roleID WHERE inforID = @inforID";

                            cmd.Parameters.AddWithValue("@firstName", firstName);
                            cmd.Parameters.AddWithValue("@lastName", lastName);
                            cmd.Parameters.AddWithValue("@email", email);
                            cmd.Parameters.AddWithValue("@contact", contact);
                            cmd.Parameters.AddWithValue("@roleID", roleID);
                            cmd.Parameters.AddWithValue("@inforID", inforID);

                            int rowsAffected = cmd.ExecuteNonQuery();

                            //cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Employee updated successfully!", "Success");
                                LoadEmployeeView();
                            }
                            else
                            {
                                MessageBox.Show("No records updated. Employee ID not found.", "Information");
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Error updating employee");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error updating employee: " + ex);
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
        }

        private void btnDelete_Click_1(object sender, RoutedEventArgs e)
        {
            using (OleDbConnection connection = dataConnection.GetConnection())
            {
                try
                {
                    if (!ValidateInput())
                    {
                        MessageBox.Show("Fields cannot be empty.", "Error");
                        return;
                    }

                    MessageBoxResult msgRes = MessageBox.Show("Are you sure you want to delete this?", "Cancel", MessageBoxButton.YesNo);
                    if (DataGridEmployee1.SelectedItem != null)
                    {
                        DataRowView row_selected = (DataRowView)DataGridEmployee1.SelectedItem;
                        string inforID = row_selected["inforID"].ToString();
                        if (msgRes == MessageBoxResult.Yes)
                        {
                            OleDbCommand cmd = connection.CreateCommand();
                            connection.Open();
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = "Delete from Employee where inforID = @inforID ";
                            cmd.Parameters.AddWithValue("@inforID", inforID);
                            cmd.ExecuteNonQuery();
                            MessageBox.Show("Record Successfully Deleted");
                            LoadEmployeeView();
                            btnEdit.IsEnabled = false;
                            txtEmpID.Clear();
                            txtFname.Clear();
                            txtLname.Clear();
                            txtEmail.Clear();
                            txtContact.Clear();
                            txtEmpID.IsEnabled = false;
                            txtFname.IsEnabled = false;
                            txtLname.IsEnabled = false;
                            txtEmail.IsEnabled = false;
                            txtContact.IsEnabled = false;
                            txtRole.IsEnabled = false;
                            txtRole.SelectedIndex = -1;
                            AddEdit.Visibility = Visibility.Visible;
                            AddPnl.Visibility = Visibility.Collapsed;
                            UpdatePnl.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            btnEdit.IsEnabled = false;
                            txtEmpID.Clear();
                            txtFname.Clear();
                            txtLname.Clear();
                            txtEmail.Clear();
                            txtContact.Clear();
                            txtRole.SelectedIndex = -1;
                            AddEdit.Visibility = Visibility.Visible;
                            AddPnl.Visibility = Visibility.Collapsed;
                            UpdatePnl.Visibility = Visibility.Collapsed;
                            txtFname.IsEnabled = false;
                            txtLname.IsEnabled = false;
                            txtEmail.IsEnabled = false;
                            txtContact.IsEnabled = false;
                            txtRole.IsEnabled = false;
                            DataGridEmployee1.IsEnabled = true;
                            DataGridEmployee1.SelectedItem = null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting employee: " + ex);
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        private void employeeSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = employeeSearch.Text.Trim();

            // If the search text is empty, load all data
            if (string.IsNullOrEmpty(searchText))
            {
                LoadEmployeeView();
                return;
            }
            else
            {

            }
            //Otherwise, filter the data based on the entered initial
            LoadScheduleDataByInitial(searchText);
        }

        private void LoadScheduleDataByInitial(string initial)
        {
            try
            {
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    string query = "SELECT Employee.inforID, fName, lName, email, contact, Role.roleName FROM Employee " +
                "INNER JOIN Role ON Employee.roleID = Role.roleID " +
                "WHERE (fName + ' ' + lName) LIKE @Initial + '%' OR Role.roleName LIKE @Initial + '%' OR (lName + ' ' + fName) LIKE @Initial + '%'";

                    OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
                    adapter.SelectCommand.Parameters.AddWithValue("@Initial", initial);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                
                    DataGridEmployee1.ItemsSource = dataTable.DefaultView;
                   
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private void DataGridEmployee1_AutoGeneratedColumns(object sender, EventArgs e)
        {
            //DataGridEmployee1.Columns[0].Header = "Employee ID";
            //DataGridEmployee1.Columns[0].Visibility = Visibility.Collapsed;
            DataGridEmployee1.Columns[0].Header = "Infor ID";
            DataGridEmployee1.Columns[0].Width = 75;
            DataGridEmployee1.Columns[1].Header = "First Name";
            DataGridEmployee1.Columns[2].Header = "Last Name";
            DataGridEmployee1.Columns[3].Header = "Email";
            DataGridEmployee1.Columns[4].Header = "Contact Number";
            DataGridEmployee1.Columns[5].Header = "Role";

            foreach (var column in DataGridEmployee1.Columns)
            {
                if (column is DataGridTextColumn textColumn)
                {
                    // Apply the custom cell style to specific columns
                    if (textColumn.Header.ToString() == "Infor ID")
                    {
                        textColumn.CellStyle = FindResource("CenteredDataGridCell") as Style;
                        textColumn.HeaderStyle = FindResource("CenteredDataGridColumnHeader") as Style;
                    }
                }
            }
        }

        private void btnDeleteEmp_Click(object sender, RoutedEventArgs e)
        {
            using (OleDbConnection connection = dataConnection.GetConnection())
            {
                try
                {
                    MessageBoxResult msgRes = MessageBox.Show("Are you sure you want to delete this?", "Cancel", MessageBoxButton.YesNo);
                    if (msgRes == MessageBoxResult.Yes)
                    {
                        connection.Open();
                        OleDbCommand cmd = connection.CreateCommand();
                        cmd.CommandType = CommandType.Text;

                        foreach (DataRowView selectedItem in DataGridEmployee1.SelectedItems)
                        {
                            string inforID = selectedItem["inforID"].ToString();
                            cmd.CommandText = "DELETE FROM Employee WHERE inforID = @inforID";
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@inforID", inforID);
                            cmd.ExecuteNonQuery();
                        }

                        MessageBox.Show("Records Successfully Deleted");
                        

                        btnDeleteEmp.IsEnabled = false;
                        btnDeleteEmp.Visibility = Visibility.Collapsed;
                        btnEdit.IsEnabled = false;
                        txtEmpID.IsEnabled = false;
                        txtFname.IsEnabled = false;
                        txtLname.IsEnabled = false;
                        txtEmail.IsEnabled = false;
                        txtContact.IsEnabled = false;
                        txtRole.IsEnabled = false;
                        txtRole.SelectedIndex = -1;
                        AddEdit.Visibility = Visibility.Visible;
                        AddPnl.Visibility = Visibility.Collapsed;
                        UpdatePnl.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        // Reset UI if user cancels deletion
                        btnDeleteEmp.IsEnabled = false;
                        btnDeleteEmp.Visibility = Visibility.Collapsed;
                        btnEdit.IsEnabled = false;
                        txtRole.SelectedIndex = -1;
                        AddEdit.Visibility = Visibility.Visible;
                        AddPnl.Visibility = Visibility.Collapsed;
                        UpdatePnl.Visibility = Visibility.Collapsed;
                        txtFname.IsEnabled = false;
                        txtLname.IsEnabled = false;
                        txtEmail.IsEnabled = false;
                        txtContact.IsEnabled = false;
                        txtRole.IsEnabled = false;
                        DataGridEmployee1.IsEnabled = true;
                        DataGridEmployee1.SelectedItem = null;
                    }
                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting employee: " + ex);
                }
                finally
                {
                    connection.Close();
                }
                
            }
            LoadEmployeeView();
        }
    }
}