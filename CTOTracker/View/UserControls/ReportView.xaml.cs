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
using System.Windows.Media.Animation;
using System.Diagnostics;

namespace CTOTracker.View.UserControls
{
    /// <summary>
    /// Interaction logic for ReportView.xaml
    /// </summary>
    public partial class ReportView : UserControl
    {
        private DataConnection dataConnection;
        private DataView dataView;

        public ReportView()
        {
            InitializeComponent();
            dataConnection = new DataConnection();
            LoadScheduleData();
            originalDtPnlHeight = dtPnl.Height;
            //PopulateComboBox();
            //cbxFilterRep.SelectionChanged += CbxFilterRep_SelectionChanged;
        }

        private void LoadScheduleData()
        {
            try
            {
                using (OleDbConnection connection = dataConnection.GetConnection())
                {
                    string query = "SELECT Schedule.schedID, Employee.inforID, Employee.fName, Employee.lName, Task.taskName, Role.roleName, plannedEnd, ctoEarned, dateUsed, " +
                                    "ctoUsed, ctoBalance FROM (((Schedule " +
                                    "LEFT JOIN Employee ON Schedule.empID = Employee.empID) " +
                                    "LEFT JOIN Role ON Employee.roleID = Role.roleID) " +
                                    "LEFT JOIN Task ON Schedule.taskID = Task.taskID);";

                    OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    //DataView dataView = new DataView(dataTable);
                    //reportDataGrid.ItemsSource = dataView;
                    reportDataGrid.Columns.Clear();

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

                    // Bind the DataTable to the DataGrid
                    dataView = new DataView(dataTable);
                    reportDataGrid.ItemsSource = dataView;
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

        private void txtbx_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text == textBox.Tag?.ToString()) // Check if the current text matches the placeholder
            {
                textBox.Text = ""; // Clear the text
                textBox.Foreground = Brushes.Black; // Change the text color back to black
            }
        }

        private void txtbx_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (string.IsNullOrWhiteSpace(textBox.Text)) // If the TextBox is empty
            {
                textBox.Text = textBox.Tag?.ToString(); // Set the placeholder text back
                textBox.Foreground = Brushes.Gray; // Change the text color to gray to indicate it's a placeholder
            }
        }

        private void txtbx_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            textBox.Text = textBox.Tag?.ToString(); // Set the placeholder text
            textBox.Foreground = Brushes.Gray;
        }

        private void txtschFname_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchFname = txtschFname.Text.Trim();
            if (string.IsNullOrEmpty(searchFname))
            {
                dataView.RowFilter = " ";
            }
            else
            {
                // Filter the data based on the entered first name
                DataView dataView = (DataView)reportDataGrid.ItemsSource;
                if (dataView != null)
                {
                    dataView.RowFilter = $"fName LIKE '%{searchFname}%'";
                }
            }
        }

        private void txtschLname_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchLname = txtschLname.Text.Trim();
            if (string.IsNullOrEmpty(searchLname))
            {
                dataView.RowFilter = " ";
            }
            else
            {
                // Filter the data based on the entered first name
                DataView dataView = (DataView)reportDataGrid.ItemsSource;
                if (dataView != null)
                {
                    dataView.RowFilter = $"fName LIKE '%{searchLname}%'";
                }
            }
        }


        /*        private void ToggleFilterPanelButton_Checked(object sender, RoutedEventArgs e)
                {
                    filterPanel.Visibility = Visibility.Visible;
                }

                private void ToggleFilterPanelButton_Unchecked(object sender, RoutedEventArgs e)
                {
                    filterPanel.Visibility = Visibility.Collapsed;
                }*/

        /*        private void DataGrid_AutoGenerateColumns(object sender, EventArgs e)
                {
                    scheduleDataGrid.Columns[0].Visibility = Visibility.Collapsed;
                    scheduleDataGrid.Columns[0].Header = "Schedule ID";
                    scheduleDataGrid.Columns[1].Header = "Infor ID";
                    scheduleDataGrid.Columns[1].Width = 75;
                    scheduleDataGrid.Columns[2].Header = "First Name";
                    scheduleDataGrid.Columns[2].Width = 190;
                    scheduleDataGrid.Columns[3].Header = "Last Name";
                    scheduleDataGrid.Columns[3].Width = 190;
                    scheduleDataGrid.Columns[4].Header = "Task Name";
                    scheduleDataGrid.Columns[4].Width = 125;
                    scheduleDataGrid.Columns[5].Header = "Role";
                    scheduleDataGrid.Columns[5].Width = 125;
                    scheduleDataGrid.Columns[6].Header = "End Date";
                    scheduleDataGrid.Columns[7].Header = "CTO Earned";
                    scheduleDataGrid.Columns[8].Header = "Use Date";
                    scheduleDataGrid.Columns[9].Header = "CTO Used";
                    scheduleDataGrid.Columns[10].Header = "CTO Balance";
                }
        */

        /*        private void PopulateComboBox()
                {
                    // Create a list of strings to populate the ComboBox
                    List<string> filterOptions = new List<string>
                    {
                        "Option 1",
                        "Option 2",
                        "Option 3"
                    };

                    // Assign the list as the ItemsSource for the ComboBox
                    cbxFilterRep.ItemsSource = filterOptions;
                }

                private void CbxFilterRep_SelectionChanged(object sender, SelectionChangedEventArgs e)
                {
                    // Check if a specific item is selected in the ComboBox
                    if (cbxFilterRep.SelectedItem != null)
                    {
                        // Get the selected item
                        string selectedItem = cbxFilterRep.SelectedItem.ToString();

                        // Check if the selected item matches the specific item
                        if (selectedItem == "Option 2")
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
        */
    }
}
