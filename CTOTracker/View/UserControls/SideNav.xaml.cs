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

        private void btnSched_Click(object sender, RoutedEventArgs e)
        {
            // Instantiate an instance of ScheduleView
            ScheduleView scheduleView = new ScheduleView();

            // Create a new window
            Window scheduleWindow = new Window();

            // Set the content of the new window to the instance of ScheduleView
            scheduleWindow.Content = scheduleView;

            // Set window properties 
            scheduleWindow.Title = "Schedule View";
            scheduleWindow.Width = 850;
            scheduleWindow.Height = 425;

            // Show the new window
            scheduleWindow.ShowDialog();
        }
    }
}
