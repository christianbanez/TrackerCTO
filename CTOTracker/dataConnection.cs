using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.Windows;


namespace CTOTracker
{
    public class DataConnection
    {
        public string GetConnectionString()
        {
            // Retrieve the connection string from app.config
            string connectionString = ConfigurationManager.ConnectionStrings["connectionName"].ConnectionString;

            // Get the database file name
            string databaseFileName = "dbCto.accdb"; // Replace with your actual database file name

            // Construct the full database path
            string databasePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, databaseFileName);

            // Replace the placeholder with the actual database path
            connectionString = connectionString.Replace("[DATABASE_PATH]", databasePath);
            
            return connectionString;
        }
        //public string connectionStrings = ConfigurationManager.ConnectionStrings["connectionName"].ConnectionString;
        //public string connection;
        //OleDbConnection connection = new OleDbConnection(connectionString);
        //connection.connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source = Provider = Microsoft.ACE.OLEDB.12.0; Data Source = C:\Users\dkeh\source\repos\EmployeeTracker\dbtk.accdb";
        //public static readonly string ConnectionString = ("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\\Users\\dkeh\\source\\repos\\EmployeeTracker\\dbtk.accdb");
        //public string conn
        //{
        //    get { return connectionString; }
        //}

        public OleDbConnection GetConnection()
        {
            // Create a new OleDbConnection object with the connection string
            return new OleDbConnection(GetConnectionString());

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
