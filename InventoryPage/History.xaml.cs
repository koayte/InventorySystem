using System;
using System.IO;
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
using MySql.Data.MySqlClient;

namespace InventorySystem.InventoryPage
{
    /// <summary>
    /// Interaction logic for History.xaml
    /// </summary>
    public partial class History : Page
    {
        public string connectionString = "SERVER=localhost; DATABASE=inventory; UID=semi; PASSWORD=semitech;";
        public History()
        {
            InitializeComponent();
        }

        // ------------------------------------------------------------ Export to .txt
        private void Export_Click(object sender, RoutedEventArgs e)
        {
            string path = @"C:\ProgramData\MySQL\MySQL Server 8.0\Uploads\htable.csv";
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            Export();
        }

        private void Export()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string commandText = "SELECT * FROM htable INTO OUTFILE 'C:/ProgramData/MySQL/MySQL Server 8.0/Uploads/htable.csv' " +
                    "FIELDS TERMINATED BY ',' " +
                    "ENCLOSED BY '\"' " +
                    "LINES TERMINATED BY '\r\n'";
                connection.Open();
                using (MySqlCommand cmd = new MySqlCommand(commandText, connection))
                {
                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        MessageBox.Show("Export successful!");
                    }
                }
                connection.Close();
            }
        }
    }

    
}
