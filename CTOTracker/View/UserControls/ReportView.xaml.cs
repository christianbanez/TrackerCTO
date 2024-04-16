using System;
using System.Collections.Generic;
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
    /// Interaction logic for ReportView.xaml
    /// </summary>
    public partial class ReportView : UserControl
    {
        public ReportView()
        {
            InitializeComponent();
            PopulateComboBox();
            cbxFilterRep.SelectionChanged += CbxFilterRep_SelectionChanged;
        }

        private void PopulateComboBox()
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
    }
}
