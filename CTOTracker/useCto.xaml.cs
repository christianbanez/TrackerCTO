using CTOTracker.View;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
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
using System.Windows.Shapes;

namespace CTOTracker
{
    /// <summary>
    /// Interaction logic for useCto.xaml
    /// </summary>
    public partial class useCto : Window
    {
        private DataConnection dataConnection; // Declare a field to hold the DataConnection object
        public useCto()
        {
            InitializeComponent();
            dataConnection = new DataConnection();

        }

        private void SelectedScheduleView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        public void LoadSelectedSchedule(List<DataRowView> selectedRows)
        {
            try
            {
                // Create a new DataTable to hold selected schedule data
                DataTable selectedScheduleDataTable = new DataTable();

                // Add columns to the DataTable (adjust as per your data structure)
                selectedScheduleDataTable.Columns.Add("schedID", typeof(int));
                selectedScheduleDataTable.Columns.Add("inforID", typeof(int));
                selectedScheduleDataTable.Columns.Add("fName", typeof(string));
                selectedScheduleDataTable.Columns.Add("lName", typeof(string));
                selectedScheduleDataTable.Columns.Add("completed", typeof(bool));
                selectedScheduleDataTable.Columns.Add("ctoEarned", typeof(double));
                selectedScheduleDataTable.Columns.Add("ctoUsed", typeof(double)); // Change type to double for decimal values
                selectedScheduleDataTable.Columns.Add("ctoBalance", typeof(double)); // Change type to double for decimal values

                // Add selected rows to the new DataTable
                foreach (DataRowView row in selectedRows)
                {
                    DataRow newRow = selectedScheduleDataTable.NewRow();
                    newRow["schedID"] = row["schedID"];
                    newRow["inforID"] = row["inforID"];
                    newRow["fName"] = row["fName"];
                    newRow["lName"] = row["lName"];
                    newRow["completed"] = false; // Assuming 'completed' is always false for selected rows
                    newRow["ctoEarned"] = row["ctoEarned"];
                    newRow["ctoUsed"] = row["ctoUsed"];
                    newRow["ctoBalance"] = row["ctoBalance"];
                    selectedScheduleDataTable.Rows.Add(newRow);
                }

                // Set default value of "CTO Used" to 0.5 when "CTO Balance" is 0.5
                foreach (DataRow row in selectedScheduleDataTable.Rows)
                {
                    if (row["ctoBalance"].ToString() == "0.5")
                    {
                        row["ctoUsed"] = 0.5;
                    }
                    if (row["ctoBalance"].ToString() == "1")
                    {
                        row["ctoUsed"] = 1;
                    }
                }

                // Bind the new DataTable to the SelectedScheduleView
                SelectedScheduleView.ItemsSource = selectedScheduleDataTable.DefaultView;

                // Create and bind the DataGridComboBoxColumn for ctoUsed column
                DataGridComboBoxColumn ctoUsedColumn = new DataGridComboBoxColumn();
                ctoUsedColumn.Header = "CTO Used";
                ctoUsedColumn.ItemsSource = new List<double> { 1, 0.5 }; // Dropdown options
                ctoUsedColumn.SelectedValueBinding = new Binding("ctoUsed");
                ctoUsedColumn.EditingElementStyle = new Style(typeof(ComboBox));
                ctoUsedColumn.EditingElementStyle.Setters.Add(new EventSetter(ComboBox.SelectionChangedEvent, new SelectionChangedEventHandler(ComboBox_SelectionChanged)));
                SelectedScheduleView.Columns.Add(ctoUsedColumn);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading selected schedule: " + ex.Message);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox != null && comboBox.SelectedItem != null)
            {
                // Update the ctoUsed property of the corresponding row
                DataRowView rowView = SelectedScheduleView.SelectedItem as DataRowView;
                if (rowView != null)
                {
                    // Convert the selected item to a double
                    double ctoUsedValue;
                    if (comboBox.SelectedItem.ToString() == "0.5")
                    {
                        ctoUsedValue = 0.5;
                    }
                    else
                    {
                        ctoUsedValue = Convert.ToDouble(comboBox.SelectedItem);
                    }
                    rowView["ctoUsed"] = ctoUsedValue;
                }
            }
        }

        private void confirmChangesBttn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create a new DataTable to hold the rows for ChangesGridView
                DataTable changesDataTable = new DataTable();

                // Add columns to the DataTable (adjust as per your data structure)
                changesDataTable.Columns.Add("schedID", typeof(int));
                changesDataTable.Columns.Add("inforID", typeof(int));
                changesDataTable.Columns.Add("fName", typeof(string));
                changesDataTable.Columns.Add("lName", typeof(string));
                changesDataTable.Columns.Add("completed", typeof(bool));
                changesDataTable.Columns.Add("ctoEarned", typeof(double));
                changesDataTable.Columns.Add("ctoUsed", typeof(double));
                changesDataTable.Columns.Add("ctoBalance", typeof(double)); // Change type to double

                // Calculate the CTO balance and add rows to the changesDataTable
                foreach (var item in SelectedScheduleView.Items)
                {
                    if (item is DataRowView rowView)
                    {
                        double ctoEarned = Convert.ToDouble(rowView["ctoEarned"]);
                        double ctoUsed = Convert.ToDouble(rowView["ctoUsed"]);
                        double ctoBalance = Convert.ToDouble(rowView["ctoBalance"]);

                        // Check if ctoUsed is greater than ctoEarned
                        if (ctoUsed > ctoBalance)
                        {
                            MessageBox.Show("CTO Used cannot be greater than CTO Earned for schedule ID: " + rowView["schedID"], "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return; // Exit the method if error
                        }

                        ctoBalance = Math.Max(0, ctoBalance - ctoUsed);

                        if (ctoEarned == 1)
                        {
                            if (ctoBalance == 0.5)
                            {
                                ctoUsed = 0.5;
                            }
                            if (ctoBalance == 0)
                            {
                                ctoUsed = 1;
                            }
                        }
                        else
                        {
                            ctoUsed = Math.Max(0, ctoUsed - ctoBalance); // Otherwise, use the regular logic
                        }

                        



                        DataRow newRow = changesDataTable.NewRow();
                        newRow["schedID"] = Convert.ToInt32(rowView["schedID"]);
                        newRow["inforID"] = Convert.ToInt32(rowView["inforID"]);
                        newRow["fName"] = rowView["fName"];
                        newRow["lName"] = rowView["lName"];
                        newRow["completed"] = rowView["completed"];
                        newRow["ctoEarned"] = ctoEarned;
                        newRow["ctoUsed"] = ctoUsed;
                        newRow["ctoBalance"] = ctoBalance;
                        changesDataTable.Rows.Add(newRow);
                    }
                }

                // Bind the changesDataTable to the ChangesGridView
                ChangesGridView.ItemsSource = changesDataTable.DefaultView;

                // If all rows pass the verification, further actions can be performed here
                // For example, save changes to the database, etc.
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void useCtoBttn_Click(object sender, RoutedEventArgs e)
        {
            // Ask for confirmation
            MessageBoxResult result = MessageBox.Show("Are you sure you want to update the database with the changes?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

            // Check if the user confirmed
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Get the changes made in the ChangesGridView
                    DataTable changesDataTable = ((DataView)ChangesGridView.ItemsSource).ToTable();

                    // Update the database with the changes
                    foreach (DataRow row in changesDataTable.Rows)
                    {
                        int schedID = Convert.ToInt32(row["schedID"]);
                        double ctoUsed = Convert.ToDouble(row["ctoUsed"]);
                        double ctoBalance = Convert.ToDouble(row["ctoBalance"]);
                        string useDesc = ""; // Placeholder for useDesc input
                        useDesc = useDescTextBox.Text;
                        UpdateCtoUsedInDatabase(schedID, ctoUsed, ctoBalance, useDesc);
                    }

                    MessageBox.Show("Database updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    ScheduleView scheduleView = new ScheduleView();
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error updating database: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Method to update the ctoUsed, ctoBalance, and useDesc values in the database
        private void UpdateCtoUsedInDatabase(int schedID, double ctoUsed, double ctoBalance, string useDesc)
        {
            try
            {
                // Create a SQL query to update the ctoUsed, ctoBalance, and useDesc values
                string query = "UPDATE Schedule SET ctoUsed = @ctoUsed, ctoBalance = @ctoBalance, useDesc = @useDesc WHERE schedID = @schedID";

                // Execute the query with the provided parameters
                using (OleDbConnection connection = dataConnection.GetConnection())
                using (OleDbCommand command = new OleDbCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ctoUsed", ctoUsed);
                    command.Parameters.AddWithValue("@ctoBalance", ctoBalance);
                    command.Parameters.AddWithValue("@useDesc", useDesc);
                    command.Parameters.AddWithValue("@schedID", schedID);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating database record for schedID " + schedID + ": " + ex.Message);
            }
        }


        private void cancelBttn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();  
        }
    }
}
