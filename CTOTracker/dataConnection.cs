using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Threading.Tasks;
using System.Data.OleDb;


namespace CTOTracker
{
    public class DataConnection
    {
        public string connectionStrings = ConfigurationManager.ConnectionStrings["connectionName"].ConnectionString;
        public string connection;
        //OleDbConnection connection = new OleDbConnection(connectionString);
        //connection.connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source = Provider = Microsoft.ACE.OLEDB.12.0; Data Source = C:\Users\dkeh\source\repos\EmployeeTracker\dbtk.accdb";
        //public static readonly string ConnectionString = ("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\\Users\\dkeh\\source\\repos\\EmployeeTracker\\dbtk.accdb");
        public string conn
        {
            get { return connectionStrings; }
        }

        public OleDbConnection GetConnection()
        {
            // Create a new OleDbConnection object with the connection string
            return new OleDbConnection(connectionStrings);
        }

        public void OpenConnection(OleDbConnection connection)
        {
            try
            {
                // Open the connection
                connection.Open();
                Console.WriteLine("Connection opened successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error opening connection: " + ex.Message);
            }
        }

        public void CloseConnection(OleDbConnection connection)
        {
            try
            {
                // Close the connection
                connection.Close();
                Console.WriteLine("Connection closed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error closing connection: " + ex.Message);
            }
        }
    }
}
