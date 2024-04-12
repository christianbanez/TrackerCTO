using CTOTracker.View;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
        }

        private void frmMain_Loaded(object sender, RoutedEventArgs e)
        {
            frmMain.Navigate(new EmployeeView());
        }

        private void ListViewItem_Selected(object sender, RoutedEventArgs e)
        {
            frmMain.Navigate(new EmployeeView());
        }

        private void ListViewItem_Selected_1(object sender, RoutedEventArgs e)
        {
            frmMain.Navigate(new ScheduleView());
        }
    }
}