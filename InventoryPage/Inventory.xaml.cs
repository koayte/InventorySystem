using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
                bool SupplierFilter = string.IsNullOrEmpty(SupplierSearch.Text) || item.Supplier.Contains(SupplierSearch.Text);
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
            var supplier = ((Item)group.Items[0]).Supplier;
            var remarks = ((Item)group.Items[0]).Remarks;
            int qtyInt = 0;
            for (int i = 0; i < (int)(group.Items.Count); i++)
            {
                qtyInt += Int32.Parse(((Item)group.Items[i]).Qty);
            }
            var qty = qtyInt.ToString();

            StringBuilder serialNums = new StringBuilder();
            if (!string.IsNullOrEmpty(((Item)group.Items[0]).SerialNums))
            {
                for (int i = 0; i < (int)(group.Items.Count); i++)
                {
                    serialNums.Append(((Item)group.Items[i]).SerialNums + '\n');
                }
            }

            else
            {
                serialNums.Append("");
            }

            SharedData.BatchID = batchID;
            SharedData.PartNum = partNum;
            SharedData.Description = description;
            SharedData.Area = area;
            SharedData.Section = section;
            SharedData.ModelNum = modelNum;
            SharedData.Supplier = supplier;
            SharedData.Remarks = remarks;
            SharedData.Qty = qty;
            SharedData.SerialNums = serialNums.ToString();

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


        // ------------------------------------------------------------ Export to .csv
        private void Export_Click(object sender, RoutedEventArgs e)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
            string path = "C:/ProgramData/MySQL/MySQL Server 8.0/Uploads/rtable_" + timestamp + ".csv";
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            Export();
        }

        private void Export()
        {
            string startDate = GetFormattedDate(StartDate);
            string endDate = GetFormattedDate(EndDate);

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string commandText = "SELECT 'UserName', 'PartNum', 'BatchID', 'Supplier', 'Description', 'Qty', 'Area', 'Section', 'ModelNum', 'SerialNums', 'Remarks', 'Time', 'Date' " +
                    "UNION ALL " +
                    "SELECT * FROM rtable WHERE Date BETWEEN @startDate AND @endDate " +
                    "INTO OUTFILE @path " +
                    "FIELDS TERMINATED BY ',' " +
                    "ENCLOSED BY '\"' " +
                    "LINES TERMINATED BY '\r\n'";
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
                string path = "C:/ProgramData/MySQL/MySQL Server 8.0/Uploads/rtable_" + timestamp + ".csv";
                connection.Open();
                using (MySqlCommand cmd = new MySqlCommand(commandText, connection))
                {
                    cmd.Parameters.AddWithValue("@startDate", startDate);
                    cmd.Parameters.AddWithValue("@endDate", endDate);
                    cmd.Parameters.AddWithValue("@path", path);
                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        MessageBox.Show("Export successful!");
                    }
                }
                connection.Close();
            }
        }

        private string GetFormattedDate(DatePicker datePicker)
        {
            string formattedDate = "";
            if (!string.IsNullOrEmpty(datePicker.Text))
            {
                DateTime dateChosen = DateTime.Parse(datePicker.Text);
                formattedDate = dateChosen.ToString("yyyy-MM-dd");
                return formattedDate;
            }
            return formattedDate;
        }

        // ------------------------------------------------------------ Old code :(

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
