using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
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

namespace CTOTracker.View
{
    /// <summary>
    /// Interaction logic for EmployeeView.xaml
    /// </summary>
    public partial class EmployeeView : UserControl
    {
        private DataConnection dataConnection;
        public EmployeeView()
        {
            InitializeComponent();
            dataConnection = new DataConnection();
            LoadEmployeeView();

        }
        private void LoadEmployeeView()
        {
            using (OleDbConnection connection = dataConnection.GetConnection())
            {

                try
                {


                    string query = "SELECT inforID, fName, lName, email, contact, roleID FROM Employee";   // Specify the columns you want to retrieve
                    OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection);
                    DataTable dataTable = new DataTable();          // Retrieve data from the database
                    adapter.Fill(dataTable);
                    connection.Open();
                    if (dataTable != null && dataTable.Rows.Count > 0)  // Check if any data is returned
                    {
                        DataGridEmployee1.ItemsSource = dataTable.DefaultView;     // Bind the DataTable to the DataGridView
                    }
                    else
                    {
                        MessageBox.Show("No data found.", "Information");
                    }
                    // Call the method to open the connection

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

        #region---Input Validation----
        private bool IsValidEmail(string email)
        {
            try
            {
                var emailValidator = new System.Net.Mail.MailAddress(email);
                return (email.LastIndexOf(".") > email.LastIndexOf("@"));
            }
            catch
            {
                return false;
            }
        }
        private bool ValidateInput()
        {
            bool isValid = true;
            if (string.IsNullOrEmpty(txtEmpID.Text))
            {
                MessageBox.Show("First Name cannot be empty.", "Error");
                isValid = false;
            }
            if (string.IsNullOrEmpty(txtFname.Text))
            {
                MessageBox.Show("First Name cannot be empty.", "Error");
                isValid = false;
            }
            if (string.IsNullOrEmpty(txtLname.Text))
            {
                MessageBox.Show("Last Name cannot be empty.", "Error");
                isValid = false;
            }
            if (!IsValidEmail(txtEmail.Text))
            {
                MessageBox.Show("Please enter a valid email address.", "Error");
                isValid = false;
            }
            if (!IsValidContact(txtContact.Text))
            {
                MessageBox.Show("Please enter a valid Philippines contact number (09xxxxxxxxx).", "Error");
                isValid = false;
            }

            return isValid;
        }
        private bool IsValidContact(string contactNumber)
        {
            return Regex.IsMatch(contactNumber, @"^09\d{9}$");
        }
        private void txtEmpID_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char ch in e.Text)
            {
                if (!char.IsDigit(ch))
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        #endregion





        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
