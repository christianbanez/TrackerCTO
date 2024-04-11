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
    /// Interaction logic for SideNav.xaml
    /// </summary>
    public partial class SideNav : UserControl
    {
        public SideNav()
        {
            InitializeComponent();
        }

        private void btnEmpNav(object sender, RoutedEventArgs e)
        {
            // Navigate the "Main" frame to the Employee page
            if (Application.Current.MainWindow != null)
            {
                MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.Main.Navigate(new EmployeeView());
                }
            }
        }

        private void btnSched_Click(object sender, RoutedEventArgs e)
        {
            // Navigate the "Main" frame to the Employee page
            if (Application.Current.MainWindow != null)
            {
                MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.Main.Navigate(new ScheduleView());
                }
            }
        }

        private void btnReport_Click(object sender, RoutedEventArgs e)
        {
            // Navigate the "Main" frame to the Employee page
            if (Application.Current.MainWindow != null)
            {
                MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.Main.Navigate(new ScheduleView());
                }
            }
        }
    }
}
