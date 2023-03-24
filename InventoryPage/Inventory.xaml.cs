using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using static Google.Protobuf.Reflection.SourceCodeInfo.Types;

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
        }


        


        // ------------------------------------------------------------ Filter functions
        private void cvs_Filter(object sender, FilterEventArgs e)
        {
            Item item = e.Item as Item;
            if (item != null)
            {
                bool PartNumFilter = string.IsNullOrEmpty(PartNumSearch.Text) || item.PartNum.Contains(PartNumSearch.Text);
                bool BatchIDFilter = string.IsNullOrEmpty(BatchSearch.Text) || item.BatchID.Contains(BatchSearch.Text);
                bool DescFilter = string.IsNullOrEmpty(DescSearch.Text) || item.Description.ToLower().Contains(DescSearch.Text.ToLower());
                bool QtyFilter = string.IsNullOrEmpty(QtySearch.Text) || item.Qty.Contains(QtySearch.Text);
                bool AreaFilter = string.IsNullOrEmpty(AreaSearch.Text) || item.Area.Contains(AreaSearch.Text);
                bool SecFilter = string.IsNullOrEmpty(SecSearch.Text) || item.Section.Contains(SecSearch.Text);
                bool ModelNumFilter = string.IsNullOrEmpty(ModelNumSearch.Text) || item.ModelNum.Contains(ModelNumSearch.Text);
                bool SerialNumFilter = string.IsNullOrEmpty(SerialNumSearch.Text) || item.SerialNums.Contains(SerialNumSearch.Text);
                e.Accepted = PartNumFilter && BatchIDFilter && DescFilter && QtyFilter && AreaFilter && SecFilter && ModelNumFilter && SerialNumFilter;
            }
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(inventoryGrid.ItemsSource).Refresh();
        }

        // ------------------------------------------------------------ Update
        private void Update_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;

            var groupItem = FindVisualParent<GroupItem>(button);

            var group = groupItem.Content as CollectionViewGroup;

            var batchID = ((Item)group.Items[0]).BatchID;
            var partNum = ((Item)group.Items[0]).PartNum;
            var description = ((Item)group.Items[0]).Description;
            var area = ((Item)group.Items[0]).Area;
            var section = ((Item)group.Items[0]).Section;
            var modelNum = ((Item)group.Items[0]).ModelNum;
            SharedData.BatchID = batchID;
            SharedData.PartNum = partNum;
            SharedData.Description = description;
            SharedData.Area = area;
            SharedData.Section = section;
            SharedData.ModelNum = modelNum;

            // MessageBox.Show(batchID);
            inventoryFrame.Navigate(new Uri("/InputPages/Update.xaml", UriKind.Relative));

        }

        private static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null)
                return null;

            var parent = parentObject as T;
            return parent ?? FindVisualParent<T>(parentObject);
        }





        // ------------------------------------------------------------ Old code :(
        private void LoadIntoDataGrid2()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string commandText = "SELECT PartNum, CAST(BatchID AS CHAR) AS BatchID, Description, CAST(Qty AS CHAR) AS Qty, Location, ModelNum, SerialNums, DATE_FORMAT(Time, '%e/%c/%Y %H:%i:%s') AS Time FROM inputs ORDER BY BatchID";
                MySqlCommand loadInventory = new MySqlCommand(commandText, connection);
                connection.Open();
                DataTable dt = new DataTable();
                dt.Load(loadInventory.ExecuteReader());
                connection.Close();
                inventoryGrid.DataContext = dt;
            }
        }

        private void ApplyFilter2()
        {
            List<string> columnNames = GetColumnNames();

            DataView? dataView = inventoryGrid.ItemsSource as DataView;

            List<string> searchTexts = new List<string> { PartNumSearch.Text, BatchSearch.Text, DescSearch.Text, QtySearch.Text, AreaSearch.Text, ModelNumSearch.Text };
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


    }




}
