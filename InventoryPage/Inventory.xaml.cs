using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
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

namespace InventorySystem.InventoryPage
{
    /// <summary>
    /// Interaction logic for Inventory.xaml
    /// </summary>
    
    
    
    public partial class Inventory : Page
    {

        public string connectionString = "SERVER=localhost; DATABASE=inventory; UID=semi; PASSWORD=semitech;";

        public Inventory()
        {
            InitializeComponent();
            loadIntoDataGrid();
        }

        private void loadIntoDataGrid()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string commandText = "SELECT * FROM inputs ORDER BY PartNum, BatchID";
                MySqlCommand loadInventory = new MySqlCommand(commandText, connection);
                connection.Open();
                DataTable dt = new DataTable();
                dt.Load(loadInventory.ExecuteReader());
                connection.Close();
                inventoryGrid.DataContext = dt;
              }
        }
    }

    
}
