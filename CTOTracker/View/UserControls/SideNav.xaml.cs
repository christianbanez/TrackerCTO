using System.Windows;
using System.Windows.Controls;

namespace CTOTracker.View.UserControls
{
    /// <summary>
    /// Interaction logic for SideNav.xaml
    /// </summary>
    public partial class SideNav : UserControl
    {
        public SideNav()
        {
            InitializeComponent();
        }
      /*  private void SideNav_Loaded(object sender, RoutedEventArgs e)
        {
            // Your event handler implementation here
        }*/
        private void btnEmpNav(object sender, RoutedEventArgs e)
        {
            // Navigate the "Main" frame to the Employee page
            if (Application.Current.MainWindow != null)
            {
                MainWindow? mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    //mainWindow.Main.Navigate(new EmployeeView());
                }
            }
        }

        private void btnSched_Click(object sender, RoutedEventArgs e)
        {
            // Navigate the "Main" frame to the Employee page
            if (Application.Current.MainWindow != null)
            {
                MainWindow? mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    //mainWindow.Main.Navigate(new ScheduleView());
                }
            }
        }

        private void btnReport_Click(object sender, RoutedEventArgs e)
        {
            // Navigate the "Main" frame to the Employee page
            if (Application.Current.MainWindow != null)
            {
                MainWindow? mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    // mainWindow.Main.Navigate(new ScheduleView());
                }
            }
        }
    }
}