using System.Windows;
using System.Windows.Controls;

namespace CTOTracker.View
{
    /// <summary>
    /// Interaction logic for EmployeeView.xaml
    /// </summary>
    public partial class EmployeeView : UserControl
    {
        public EmployeeView()
        {
            InitializeComponent();
            AddPnl.Visibility = Visibility.Collapsed;
            UpdatePnl.Visibility = Visibility.Collapsed;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddPnl.Visibility = Visibility.Visible;
            UpdatePnl.Visibility = Visibility.Collapsed;
            AddEdit.Visibility = Visibility.Collapsed;
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            UpdatePnl.Visibility = Visibility.Visible;
            AddPnl.Visibility = Visibility.Collapsed;
            AddEdit.Visibility = Visibility.Collapsed;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult msgRes = MessageBox.Show("Are you sure?", "Cancel", MessageBoxButton.YesNo);
            if (msgRes == MessageBoxResult.Yes)
            {
                AddEdit.Visibility = Visibility.Visible;
                AddPnl.Visibility = Visibility.Collapsed;
                UpdatePnl.Visibility = Visibility.Collapsed;
            }
        }

        private void btnCancel2_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult msgRes = MessageBox.Show("Are you sure?", "Cancel", MessageBoxButton.YesNo);
            if (msgRes == MessageBoxResult.Yes)
            {
                AddEdit.Visibility = Visibility.Visible;
                AddPnl.Visibility = Visibility.Collapsed;
                UpdatePnl.Visibility = Visibility.Collapsed;
            }
        }
    }
}