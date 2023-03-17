using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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
            LoadIntoDataGrid();
        }


        private void LoadIntoDataGrid()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string commandText = "SELECT PartNum, CAST(BatchID AS CHAR) AS BatchID, Description, CAST(Qty AS CHAR) AS Qty, Location, ModelNum, SerialNums, DATE_FORMAT(Time, '%e/%c/%Y %H:%i:%s') AS Time FROM inputs ORDER BY PartNum, BatchID";
                MySqlCommand loadInventory = new MySqlCommand(commandText, connection);
                connection.Open();
                DataTable dt = new DataTable();
                dt.Load(loadInventory.ExecuteReader());
                connection.Close();
                inventoryGrid.DataContext = dt;
            }
        }

        private List<string> GetColumnNames()
        {
            List<string> columnNames = new List<string>();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string commandText = "DESCRIBE inputs";
                MySqlCommand getColumnNames = new MySqlCommand(commandText, connection);
                connection.Open();
                MySqlDataReader reader = getColumnNames.ExecuteReader();
                while (reader.Read())
                {
                    var columnName = reader.GetString(0);
                    columnNames.Add(columnName.ToString());
                }
                connection.Close();
            }
            return columnNames;
        }


        private void ApplyFilter()
        {
            List<string> columnNames = GetColumnNames();

            DataView? dataView = inventoryGrid.ItemsSource as DataView; 

            List<string> searchTexts = new List<string> { PartNumSearch.Text, BatchSearch.Text, DescSearch.Text, QtySearch.Text, LocSearch.Text, ModelNumSearch.Text, SerialNumSearch.Text, DateSearch.Text };
            StringBuilder filter = new StringBuilder();
            for (int i = 0; i < searchTexts.Count; i++)
            {
                string searchText = searchTexts[i];
                string columnName = columnNames[i];

                if (!string.IsNullOrEmpty(searchText))
                {
                    filter.AppendFormat("[{0}] LIKE '%{1}%' AND ", columnName, searchText);
                }
            }

            // Remove last 5 characters from filter string, which is ' AND '.
            if (filter.Length > 0)
            {
                filter.Length -= 5;
            }

            dataView.RowFilter = filter.ToString();
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

    }




}
