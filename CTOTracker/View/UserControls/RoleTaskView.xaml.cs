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
    /// Interaction logic for RoleTaskView.xaml
    /// </summary>
    public partial class RoleTaskView : UserControl
    {

        private DataConnection dataConnection;
        private DataTable roleDataTable; // Declare roleDataTable at class level
        private DataTable taskDataTable; // Declare taskDataTable at class level

        public RoleTaskView()
        {
            InitializeComponent();
            dataConnection = new DataConnection();
            InitializeRoleGridView();
            InitializeTaskGridView();
            LoadRoleView();
            LoadTaskView();
            roleGridView.IsEnabled = false;
            roleNameInput.IsEnabled = false;
            taskGridView.IsEnabled = false;

        }
        private void InitializeRoleGridView()
        {
            // Create columns for roleName
            DataGridTextColumn roleNameColumn = new DataGridTextColumn();
            roleNameColumn.Header = "Role Name";
            roleNameColumn.Binding = new System.Windows.Data.Binding("roleName");

            // Add column to the roleGridView
            roleGridView.Columns.Add(roleNameColumn);

            // Handle selection changed event
            roleGridView.SelectionChanged += RoleGridView_SelectionChanged;
        }

        private void RoleGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (roleGridView.SelectedItem != null)
            {
                // Get selected row's data
                DataRowView selectedRow = (DataRowView)roleGridView.SelectedItem;
                string roleName = selectedRow["roleName"].ToString();

                // Display data in input field
                roleNameInput.Text = roleName;
            }
        }

        private void LoadRoleView()
        {
            using (OleDbConnection connection = dataConnection.GetConnection())
            {
                try
                {
                    connection.Open();
                    string query = "SELECT roleID, roleName FROM Role";
                    OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);

                    roleDataTable = new DataTable(); // Initialize roleDataTable
                    adapter.Fill(roleDataTable);

                    roleGridView.ItemsSource = roleDataTable.DefaultView; // Bind roleDataTable to roleGridView
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
        private void InsertRoleIntoDatabase(string roleName)
        {
            using (OleDbConnection connection = dataConnection.GetConnection())
            {
                try
                {
                    connection.Open();

                    // Check if the role already exists
                    string checkQuery = "SELECT COUNT(*) FROM Role WHERE roleName = @RoleName";
                    OleDbCommand checkCommand = new OleDbCommand(checkQuery, connection);
                    checkCommand.Parameters.AddWithValue("@RoleName", roleName);
                    int existingRolesCount = (int)checkCommand.ExecuteScalar();

                    if (existingRolesCount > 0)
                    {
                        MessageBox.Show("Role already exists in the database.", "Warning");
                        return; // Exit the method
                    }

                    // Role doesn't exist, proceed with insertion
                    string insertQuery = "INSERT INTO Role (roleName) VALUES (@RoleName)";
                    OleDbCommand insertCommand = new OleDbCommand(insertQuery, connection);
                    insertCommand.Parameters.AddWithValue("@RoleName", roleName);
                    int rowsAffected = insertCommand.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        MessageBox.Show("No rows were affected. Role insertion might have failed.", "Warning");
                    }
                    else
                    {
                        MessageBox.Show("Role inserted successfully.", "Success");
                    }
                }
                catch (OleDbException ex)
                {
                    MessageBox.Show("Database error: " + ex.Message, "Error");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error");
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                    }
                }
            }
        }
        private bool isAdding = false;

        private void roleSave_Click(object sender, RoutedEventArgs e)
        {
            if (!isAdding)
            {
                // Enable the input field when adding is not active
                roleNameInput.IsEnabled = true;
                roleNameInput.Focus(); // Set focus to the input field
                isAdding = true;

                // Enable the editBtn
                roleEditBtn.Visibility = Visibility.Collapsed;

                // Show the appropriate panel
                roleAddEditPnl.Visibility = Visibility.Collapsed;
                roleAddPnl.Visibility = Visibility.Visible;
            }
            else
            {
                // Save functionality goes here
                string roleName = roleNameInput.Text;

                // Check if roleName is empty
                if (string.IsNullOrWhiteSpace(roleName))
                {
                    MessageBox.Show("Role name cannot be empty.", "Warning");
                    return; // Exit the method
                }

                MessageBoxResult result = MessageBox.Show("Are you sure you want to save this role?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    InsertRoleIntoDatabase(roleName);
                    LoadRoleView();
                    roleNameInput.Text = "";

                    // Reset UI
                    roleNameInput.IsEnabled = false;
                    isAdding = false;

                    // Gray out the editBtn
                    roleEditBtn.Visibility = Visibility.Visible;

                    // Show the appropriate panel
                    roleAddEditPnl.Visibility = Visibility.Visible;
                    roleAddPnl.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void roleDeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (roleGridView.SelectedItem != null)
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this role?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    DataRowView selectedRow = (DataRowView)roleGridView.SelectedItem;

                    if (selectedRow.Row.Table.Columns.Contains("roleID"))
                    {
                        string roleId = selectedRow["roleID"].ToString();
                        DeleteRoleFromDatabase(roleId);
                        LoadRoleView();
                    }
                    else
                    {
                        MessageBox.Show("roleID column not found in the DataTable.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a role to delete.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            roleNameInput.Text = "";

        }

        private void DeleteRoleFromDatabase(string roleId)
        {
            using (OleDbConnection connection = dataConnection.GetConnection())
            {
                try
                {
                    connection.Open();
                    string query = "DELETE FROM Role WHERE roleID = @RoleID";
                    OleDbCommand command = new OleDbCommand(query, connection);
                    command.Parameters.AddWithValue("@RoleID", roleId);
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting role from database: " + ex.Message, "Error");
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        private void roleUpdateBtn_Click(object sender, RoutedEventArgs e)
        {
            if (roleGridView.SelectedItem != null)
            {
                DataRowView selectedRow = (DataRowView)roleGridView.SelectedItem;

                if (selectedRow.Row.Table.Columns.Contains("roleID"))
                {
                    string roleId = selectedRow["roleID"].ToString();
                    string roleName = roleNameInput.Text;

                    // Check if roleName is empty
                    if (string.IsNullOrWhiteSpace(roleName))
                    {
                        MessageBox.Show("Role name cannot be empty.", "Warning");
                        return; // Exit the method
                    }

                    // Check if the role already exists
                    if (RoleExists(roleName))
                    {
                        MessageBox.Show("Role already exists in the database.", "Warning");
                        return; // Exit the method
                    }

                    UpdateRoleInDatabase(roleId, roleName);
                    LoadRoleView();

                    roleNameInput.Text = "";

                    // Show the appropriate panel
                    roleAddEditPnl.Visibility = Visibility.Visible;
                    roleEditPnl.Visibility = Visibility.Collapsed;

                    roleGridView.IsEnabled = false;
                }
                else
                {
                    MessageBox.Show("roleID column not found in the DataTable.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a role to update.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private bool RoleExists(string roleName)
        {
            using (OleDbConnection connection = dataConnection.GetConnection())
            {
                try
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM Role WHERE roleName = @RoleName";
                    OleDbCommand command = new OleDbCommand(query, connection);
                    command.Parameters.AddWithValue("@RoleName", roleName);
                    int existingRolesCount = (int)command.ExecuteScalar();
                    return existingRolesCount > 0;
                }
                catch (OleDbException ex)
                {
                    MessageBox.Show("Database error: " + ex.Message, "Error");
                    return true; // Assume role exists to prevent update
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error");
                    return true; // Assume role exists to prevent update
                }
            }
        }

        private void UpdateRoleInDatabase(string roleId, string roleName)
        {
            using (OleDbConnection connection = dataConnection.GetConnection())
            {
                try
                {
                    connection.Open();
                    string query = "UPDATE Role SET roleName = @RoleName WHERE roleID = @RoleID";
                    OleDbCommand command = new OleDbCommand(query, connection);
                    command.Parameters.AddWithValue("@RoleName", roleName);
                    command.Parameters.AddWithValue("@RoleID", roleId);
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        MessageBox.Show("No rows were affected. Role update might have failed.", "Warning");
                    }
                    else
                    {
                        MessageBox.Show("Role updated successfully.", "Success");
                    }
                }
                catch (OleDbException ex)
                {
                    MessageBox.Show("Database error: " + ex.Message, "Error");
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

        private void roleSave_Click_1(object sender, RoutedEventArgs e)
        {
            // Show the roleDeleteBtn and roleUpdateBtn
            roleAddPnl.Visibility = Visibility.Visible;
            // Hide the roleEditBtn
            roleAddEditPnl.Visibility = Visibility.Collapsed;

            // Disable roleGridView (make it unselectable)
            roleGridView.IsEnabled = true;

            // Clear selection in roleGridView
            roleGridView.SelectedItem = null;

            // Clear input fields (assuming roleNameInput is the only input field)
            roleNameInput.Text = "";
        }


        private void roleEditBtn_Click(object sender, RoutedEventArgs e)
        {
            roleNameInput.IsEnabled = true;

            // Show the roleDeleteBtn and roleUpdateBtn
            roleEditPnl.Visibility = Visibility.Visible;
            // Hide the roleEditBtn
            roleAddEditPnl.Visibility = Visibility.Collapsed;

            // Disable roleGridView (make it unselectable)
            roleGridView.IsEnabled = true;

            // Clear selection in roleGridView
            roleGridView.SelectedItem = null;

            // Clear input fields (assuming roleNameInput is the only input field)
            roleNameInput.Text = "";
        }

        private void roleCancelBtn_Click(object sender, RoutedEventArgs e)
        {
            // Reset UI
            roleNameInput.Text = ""; // Clear input field
            roleNameInput.IsEnabled = false; // Disable input field
            isAdding = false; // Reset flag

            // Show the "EDIT" button
            roleEditBtn.Visibility = Visibility.Visible;

            // Show the appropriate panel
            roleAddEditPnl.Visibility = Visibility.Visible;
            roleAddPnl.Visibility = Visibility.Collapsed;
        }

        private void roleCancelBtn_Click_1(object sender, RoutedEventArgs e)
        {
            // Reset UI
            roleNameInput.Text = ""; // Clear input field
            roleNameInput.IsEnabled = false; // Disable input field
            isAdding = false; // Reset flag

            // Show the "EDIT" button
            roleEditBtn.Visibility = Visibility.Visible;

            // Show the appropriate panel
            roleEditPnl.Visibility = Visibility.Collapsed;
            roleAddEditPnl.Visibility = Visibility.Visible;
        }

        private void InitializeTaskGridView()
        {
            // Create columns for taskName and taskDesc
            DataGridTextColumn taskNameColumn = new DataGridTextColumn();
            taskNameColumn.Header = "Task Name";
            taskNameColumn.Binding = new System.Windows.Data.Binding("taskName");

            DataGridTextColumn taskDescColumn = new DataGridTextColumn();
            taskDescColumn.Header = "Task Description";
            taskDescColumn.Binding = new System.Windows.Data.Binding("taskDesc");

            // Add columns to the taskGridView
            taskGridView.Columns.Add(taskNameColumn);
            taskGridView.Columns.Add(taskDescColumn);

            // Handle selection changed event
            taskGridView.SelectionChanged += TaskGridView_SelectionChanged;
        }

        private void TaskGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (taskGridView.SelectedItem != null)
            {
                // Get selected row's data
                DataRowView selectedRow = (DataRowView)taskGridView.SelectedItem;
                string taskName = selectedRow["taskName"].ToString();
                string taskDesc = selectedRow["taskDesc"].ToString();

                // Display data in input fields
                taskNameInput.Text = taskName;
                taskDescInput.Text = taskDesc;
            }
        }
        private void LoadTaskView()
        {
            using (OleDbConnection connection = dataConnection.GetConnection())
            {
                try
                {
                    connection.Open();
                    string query = "SELECT taskID, taskName, taskDesc FROM Task";
                    OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);

                    taskDataTable = new DataTable(); // Initialize taskDataTable
                    adapter.Fill(taskDataTable);

                    taskGridView.ItemsSource = taskDataTable.DefaultView; // Bind taskDataTable to taskGridView
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



        private bool TaskExists(string taskName)
        {
            using (OleDbConnection connection = dataConnection.GetConnection())
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM Task WHERE taskName = @TaskName";
                OleDbCommand command = new OleDbCommand(query, connection);
                command.Parameters.AddWithValue("@TaskName", taskName);
                int count = (int)command.ExecuteScalar();
                return count > 0;
            }
        }

        private void InsertTaskIntoDatabase(string taskName, string taskDesc)
        {
            using (OleDbConnection connection = dataConnection.GetConnection())
            {
                try
                {
                    connection.Open();

                    // Check if the task already exists
                    if (TaskExists(taskName))
                    {
                        MessageBox.Show("Task with the same name already exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    string query = "INSERT INTO Task (taskName, taskDesc) VALUES (@TaskName, @TaskDesc)";
                    OleDbCommand command = new OleDbCommand(query, connection);
                    command.Parameters.AddWithValue("@TaskName", taskName);
                    command.Parameters.AddWithValue("@TaskDesc", taskDesc);
                    command.ExecuteNonQuery();
                }
                catch (OleDbException ex)
                {
                    MessageBox.Show("Database error occurred: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    // Log the exception for debugging or auditing purposes
                    // Logger.Log(ex);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    // Log the exception for debugging or auditing purposes
                    // Logger.Log(ex);
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        private bool IsInputValid(string taskName, string taskDesc)
        {
            if (string.IsNullOrWhiteSpace(taskName) || string.IsNullOrWhiteSpace(taskDesc))
            {
                MessageBox.Show("Task name and description cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private bool isFirstClick = true;

        private void taskSaveBtn_Click_1(object sender, RoutedEventArgs e)
        {
            string taskName = taskNameInput.Text;
            string taskDesc = taskDescInput.Text;

            if (!IsInputValid(taskName, taskDesc))
            {
                return;
            }

            MessageBoxResult result = MessageBox.Show("Are you sure you want to save this task?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    InsertTaskIntoDatabase(taskName, taskDesc);
                    LoadTaskView();
                    taskNameInput.Text = "";
                    taskDescInput.Text = "";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while saving the task: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    // Logger.Log(ex);
                }
            }
        }

        private void addBtnClick_Click(object sender, RoutedEventArgs e)
        {
            if (isFirstClick)
            {
                taskNameInput.IsEnabled = true;
                taskDescInput.IsEnabled = true;
                isFirstClick = false;
            }

            taskAddPnl.Visibility = Visibility.Visible;
            taskAddEditPnl.Visibility = Visibility.Collapsed;
            taskGridView.IsEnabled = true;
            taskGridView.SelectedItem = null;
            taskNameInput.Text = "";
            taskDescInput.Text = "";
        }

        private void editBtn_Click(object sender, RoutedEventArgs e)
        {

            taskNameInput.IsEnabled = true;
            taskDescInput.IsEnabled = true;
            // Show the roleDeleteBtn and roleUpdateBtn
            taskUpdatePnl.Visibility = Visibility.Visible;
            // Hide the roleEditBtn
            taskAddEditPnl.Visibility = Visibility.Collapsed;

            // Disable roleGridView (make it unselectable)
            taskGridView.IsEnabled = true;

            // Clear selection in roleGridView
            taskGridView.SelectedItem = null;

            // Clear input fields (assuming roleNameInput is the only input field)
            taskNameInput.Text = "";
            taskDescInput.Text = "";
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            // Show the roleDeleteBtn and roleUpdateBtn
            taskUpdatePnl.Visibility = Visibility.Collapsed;
            taskAddPnl.Visibility = Visibility.Collapsed;
            // Hide the roleEditBtn
            taskAddEditPnl.Visibility = Visibility.Visible;

            // Enable roleGridView (make it selectable)
            taskGridView.IsEnabled = false;

            // Clear selection in roleGridView
            taskGridView.SelectedItem = null;

            // Clear input fields (assuming roleNameInput is the only input field)
            taskNameInput.Text = "";
            taskDescInput.Text = "";

            taskNameInput.IsEnabled = false;
            taskDescInput.IsEnabled = false;
        }

        private void deleteBtn_Click(object sender, RoutedEventArgs e)
        {
            // Check if a row is selected in the taskGridView
            if (taskGridView.SelectedItem != null)
            {
                // Display confirmation dialog
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this task?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                // Check if the user confirmed the action
                if (result == MessageBoxResult.Yes)
                {
                    // Get the selected row's data
                    DataRowView selectedRow = (DataRowView)taskGridView.SelectedItem;

                    // Check if the taskId column exists in the DataTable
                    if (selectedRow.Row.Table.Columns.Contains("taskID"))
                    {
                        string taskId = selectedRow["taskID"].ToString();

                        // Delete the task from the database
                        DeleteTaskFromDatabase(taskId);

                        // Refresh the taskGridView
                        LoadTaskView();
                    }
                    else
                    {
                        MessageBox.Show("taskId column not found in the DataTable.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a task to delete.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            taskNameInput.Text = "";
            taskDescInput.Text = "";
        }

        private void DeleteTaskFromDatabase(string taskId)
        {
            using (OleDbConnection connection = dataConnection.GetConnection())
            {
                try
                {
                    connection.Open();
                    string query = "DELETE FROM Task WHERE taskId = @TaskID";
                    OleDbCommand command = new OleDbCommand(query, connection);
                    command.Parameters.AddWithValue("@TaskID", taskId);
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting task from database: " + ex.Message, "Error");
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        private void updateBtn_Click(object sender, RoutedEventArgs e)
        {
            // Check if a row is selected in the taskGridView
            if (taskGridView.SelectedItem != null)
            {
                // Get the selected row's data
                DataRowView selectedRow = (DataRowView)taskGridView.SelectedItem;

                // Check if the taskId column exists in the DataTable
                if (selectedRow.Row.Table.Columns.Contains("taskID"))
                {
                    string taskId = selectedRow["taskID"].ToString();
                    string taskName = taskNameInput.Text;
                    string taskDesc = taskDescInput.Text;

                    // Check if input is valid
                    if (!IsInputValid(taskName, taskDesc))
                    {
                        return;
                    }

                    // Ask for confirmation before updating
                    MessageBoxResult result = MessageBox.Show("Are you sure you want to update this task?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.No)
                    {
                        return;
                    }

                    try
                    {
                        // Update data in the database
                        UpdateTaskInDatabase(taskId, taskName, taskDesc);

                        // Refresh the taskGridView
                        LoadTaskView();

                        // Optionally, clear the input fields after updating
                        taskNameInput.Text = "";
                        taskDescInput.Text = "";

                        // Show the appropriate panel
                        taskUpdatePnl.Visibility = Visibility.Collapsed;
                        taskAddEditPnl.Visibility = Visibility.Visible;

                        // Disable taskGridView (make it unselectable)
                        taskGridView.IsEnabled = false;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occurred while updating the task: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        // Log the exception for debugging or auditing purposes
                        // Logger.Log(ex);
                    }
                }
                else
                {
                    MessageBox.Show("taskId column not found in the DataTable.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a task to update.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        private bool TaskExists(string taskName, string taskDesc, string currentTaskId)
        {
            using (OleDbConnection connection = dataConnection.GetConnection())
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM Task WHERE taskName = @TaskName AND taskDesc = @TaskDesc AND taskId <> @TaskID";
                OleDbCommand command = new OleDbCommand(query, connection);
                command.Parameters.AddWithValue("@TaskName", taskName);
                command.Parameters.AddWithValue("@TaskDesc", taskDesc);
                command.Parameters.AddWithValue("@TaskID", currentTaskId);
                int count = (int)command.ExecuteScalar();
                return count > 0;
            }
        }

        private void UpdateTaskInDatabase(string taskId, string taskName, string taskDesc)
        {
            using (OleDbConnection connection = dataConnection.GetConnection())
            {
                try
                {
                    connection.Open();

                    // Check if a task with the same name and description already exists, excluding the current task
                    if (TaskExists(taskName, taskDesc, taskId))
                    {
                        MessageBox.Show("A task with the same name and description already exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    string query = "UPDATE Task SET taskName = @TaskName, taskDesc = @TaskDesc WHERE taskId = @TaskID";
                    OleDbCommand command = new OleDbCommand(query, connection);
                    command.Parameters.AddWithValue("@TaskName", taskName);
                    command.Parameters.AddWithValue("@TaskDesc", taskDesc);
                    command.Parameters.AddWithValue("@TaskID", taskId);
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error updating task in database: " + ex.Message, "Error");
                    // Log the exception for debugging or auditing purposes
                    // Logger.Log(ex);
                }
                finally
                {
                    connection.Close();
                }

            }
        }

        private void taskNameInput_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
