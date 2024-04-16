using CTOTracker.View;
using CTOTracker.View.UserControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CTOTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded; // Attach Loaded event handler
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            AttachEventHandlers();
        }

        private void AttachEventHandlers()
        {
            // Find controls in the template
            var closeButton = (Button)this.Template.FindName("CloseButton", this);
            var titleBar = (Border)this.Template.FindName("TitleBar", this);

            // Attach event handlers
            if (closeButton != null)
            {
                closeButton.Click += CloseButton_Click;
            }

            if (titleBar != null)
            {
                titleBar.MouseLeftButtonDown += TitleBar_MouseLeftButtonDown;
            }
        }

        private void SideNav_Loaded(object sender, RoutedEventArgs e)
        {
            // Your event handler implementation here
        }

        private void frmMain_Loaded(object sender, RoutedEventArgs e)
        {
            frmMain.Navigate(new EmployeeView());
            // Uncheck the ToggleButton when a list view item is selected
            if (listSideNav.SelectedItem != null)
            {
                tgbMenu.IsChecked = false;
            }
        }

        private void ListViewItem_Selected(object sender, RoutedEventArgs e)
        {
            frmMain.Navigate(new EmployeeView());
            // Uncheck the ToggleButton when a list view item is selected
            if (listSideNav.SelectedItem != null)
            {
                tgbMenu.IsChecked = false;
            }
        }

        private void ListViewItem_Selected_1(object sender, RoutedEventArgs e)
        {
            frmMain.Navigate(new ScheduleView());
            // Uncheck the ToggleButton when a list view item is selected
            if (listSideNav.SelectedItem != null)
            {
                tgbMenu.IsChecked = false;
            }
        }

        private void ListViewItem_Selected_2(object sender, RoutedEventArgs e)
        {
            frmMain.Navigate(new ReportView());
            // Uncheck the ToggleButton when a list view item is selected
            if (listSideNav.SelectedItem != null)
            {
                tgbMenu.IsChecked = false;
            }
        }

        private void tgbMenu_MouseEnter(object sender, MouseEventArgs e)
        {
            // Set tooltip visibility

            if (tgbMenu.IsChecked != true)
            {
                ttEmp.Visibility = Visibility.Collapsed;
                ttSched.Visibility = Visibility.Collapsed;
                ttRep.Visibility = Visibility.Collapsed;
            }
            else
            {
                ttEmp.Visibility = Visibility.Visible;
                ttSched.Visibility = Visibility.Visible;
                ttRep.Visibility = Visibility.Visible;
            }
        }

        private void tgbMenu_Checked(object sender, RoutedEventArgs e)
        {
            OverlayGrid.Visibility = Visibility.Visible;
            OverlayGrid.Opacity = 0.5;
        }

        private void tgbMenu_Unchecked(object sender, RoutedEventArgs e)
        {
            OverlayGrid.Visibility = Visibility.Collapsed;
        }

        private void listSideNav_Loaded(object sender, RoutedEventArgs e)
        {
            if (listSideNav.Items.Count > 0)
            {
                ListViewItem firstItem = (ListViewItem)listSideNav.Items[0];
                firstItem.IsSelected = true;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}