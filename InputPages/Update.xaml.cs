using InventorySystem.InventoryPage;
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

namespace InventorySystem.InputPages
{
    /// <summary>
    /// Interaction logic for Update.xaml
    /// </summary>
    /// 
    public partial class Update : Page
    {
        public string connectionString = "SERVER=localhost; DATABASE=inventory; UID=semi; PASSWORD=semitech;";
        private List<string> areas = new List<string>();
        private List<TextBox> inputBoxes;

        public Update()
        {
            InitializeComponent();
            SetTextBoxesFromUpdate();
            UpdateLocationComboBox();
            inputBoxes = new List<TextBox> { PartNum, Qty, BatchID, Description, ModelNum, SerialNums };
        }

        private void SetTextBoxesFromUpdate() 
        {
            // Get BatchID
            string batchID = SharedData.BatchID;
            BatchID.Text = batchID;

            List<Item> items = GetFullItem();
            int firstIndex = items.FindIndex(a => a.BatchID == batchID);
            int lastIndex = items.FindLastIndex(a => a.BatchID == batchID);

            PartNum.Text = SharedData.PartNum;
            Description.Text = SharedData.Description;
            Location.Text = SharedData.Location;
            if (!string.IsNullOrEmpty(SharedData.ModelNum))
            {
                ModelNum.Text = SharedData.ModelNum;
                ModelNumCheckbox.IsChecked = true;
            }

            if (firstIndex == lastIndex)
            {
                // get quantity from item directly
                Qty.Text = items[firstIndex].Qty;
            }

            // Serial Numbers entered on separate lines 
            else
            {
                int count = lastIndex - firstIndex + 1;
                Qty.Text = count.ToString();

                SerialNumsCheckbox.IsChecked = true;
                StringBuilder serialNums = new StringBuilder();
                for (int i = firstIndex; i <= lastIndex; i++)
                {
                    string serialNum = items[i].SerialNums;
                    serialNums.Append(serialNum + '\n');
                }
                SerialNums.Text = serialNums.ToString();

                SerialNums.CaretIndex = SerialNums.Text.Length;
            }

        }

        private List<Item> GetFullItem()
        {
            List<Item> data = new List<Item>();
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string commandText = "SELECT PartNum, BatchID, Description, CAST(Qty AS CHAR) AS Qty, Location, ModelNum, SerialNums FROM inputs ORDER BY PartNum, BatchID";
                MySqlCommand loadInventory = new MySqlCommand(commandText, connection);
                connection.Open();
                MySqlDataReader reader = loadInventory.ExecuteReader();
                while (reader.Read())
                {
                    var type = typeof(Item);
                    Item obj = (Item)Activator.CreateInstance(type);
                    foreach (var prop in type.GetProperties())
                    {
                        var propType = prop.PropertyType;
                        prop.SetValue(obj, Convert.ChangeType(reader[prop.Name].ToString(), propType));
                    }
                    data.Add(obj);
                }
                connection.Close();
            }
            return data;
        }

        private void UpdateItem_Click(object sender, RoutedEventArgs e)
        {
            List<string> placeholders = new List<string> { "@partNum", "@qty", "@description", "@location", "@modelNum", "@serialNums" };
            List<string> inputs = new List<string> { PartNum.Text, Qty.Text, Description.Text, Location.Text, ModelNum.Text, SerialNums.Text };
            var serialNumberList = SerialNums.Text.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string commandText = "DELETE FROM inputs WHERE BatchID = @batchId; " +
                    "INSERT INTO inputs (PartNum, Qty, Description, Location, ModelNum, SerialNums) VALUE (@partNum, @qty, @description, @location, @modelNum, @serialNums)";
                MySqlCommand updateRow = new MySqlCommand(commandText, connection);

                if (string.IsNullOrEmpty(SerialNums.Text))
                {
                    connection.Open();
                    updateRow.Parameters.AddWithValue("@batchId", BatchID.Text);
                    updateRow.Parameters.AddWithValue("@serialNums", "");
                    for (int j = 0; j < placeholders.Count - 1; j++)
                    {
                        updateRow.Parameters.AddWithValue(placeholders[j], inputs[j]);
                    }
                    updateRow.ExecuteNonQuery();
                    connection.Close();
                }

                // Create separate rows for each serial number.
                else
                {
                    connection.Open();
                    foreach (var num in serialNumberList)
                    {
                        updateRow.Parameters.AddWithValue("@batchId", BatchID.Text);
                        for (int j = 0; j < placeholders.Count; j++)
                        {
                            switch (j)
                            {
                                case 1:
                                    updateRow.Parameters.AddWithValue(placeholders[j], "1");
                                    break;

                                case 5:
                                    updateRow.Parameters.AddWithValue(placeholders[j], num);
                                    break;

                                default:
                                    updateRow.Parameters.AddWithValue(placeholders[j], inputs[j]);
                                    break;
                            }
                        }
                        updateRow.ExecuteNonQuery();
                        updateRow.Parameters.Clear();
                    }
                    connection.Close();
                }

            }
            AddNewArea();
            ClearAll();
            updateFrame.Navigate(new Uri("/InventoryPage/Inventory.xaml", UriKind.Relative));
        }

        private void PartNum_TextChanged(object sender, TextChangedEventArgs e)
        {

        }


        // ------------------------------------------------------ Areas and locations 
        private List<string> AddLocationsIntoString()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand getArea = new MySqlCommand("SELECT Area FROM locations", connection);
                connection.Open();
                using (var reader = getArea.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var area = reader.GetString(0);
                        areas.Add(area);
                    }
                }
                connection.Close();
            }
            return (areas);
        }

        private void AddNewArea()
        {
            areas = AddLocationsIntoString();

            // If user inputs a new area that is not in the current database, add to db.
            if (!areas.Contains(Location.Text))
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    string commandText = "INSERT INTO locations (Area) VALUE (@Area)";
                    MySqlCommand addNewArea = new MySqlCommand(commandText, connection);
                    addNewArea.Parameters.AddWithValue("@Area", Location.Text);

                    connection.Open();
                    addNewArea.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }

        private void UpdateLocationComboBox()
        {
            DataTable dt = new DataTable();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand getArea = new MySqlCommand("SELECT Area FROM locations", connection);
                connection.Open();

                // Load into dt, bind dt to Location ComboBox.
                using (var reader = getArea.ExecuteReader())
                {
                    dt.Load(reader);
                    Location.ItemsSource = dt.DefaultView;
                }

                connection.Close();
            }
        }


        // ------------------------------------------------------ Making ModelNum and SerialNums non-editable if checkboxes are unchecked.
        private void Model_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ModelNum.IsReadOnly = false;
            ModelNumber.Foreground = Brushes.Black;
            if (!string.IsNullOrEmpty(Qty.Text) && !string.IsNullOrEmpty(BatchID.Text) && !string.IsNullOrEmpty(Description.Text))
            {
                ModelNum.Focus();
            }
        }

        private void Model_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ModelNum.IsReadOnly = true;
            if (!string.IsNullOrEmpty(ModelNum.Text))
            {
                ModelNum.Text = string.Empty;
            }

            ModelNumber.Foreground = Brushes.Gray;
        }

        private void Serial_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            SerialNums.IsReadOnly = false;
            SerialNumbers.Foreground = Brushes.Black;
            if (!string.IsNullOrEmpty(Qty.Text) && !string.IsNullOrEmpty(BatchID.Text) && !string.IsNullOrEmpty(Description.Text))
            {
                SerialNums.Focus();
            }
        }

        private void Serial_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            SerialNums.IsReadOnly = true;
            if (!string.IsNullOrEmpty(SerialNums.Text))
            {
                SerialNums.Text = string.Empty;
            }

            SerialNumbers.Foreground = Brushes.Gray;
        }

        // ------------------------------------------------------ Navigation around UI
        private void Control_Enter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ((UIElement)sender).MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                textBox.SelectAll();
            }
        }
        private void ClearAll()
        {
            // Clear textboxes, reset checkboxes to default
            for (int i = 0; i < inputBoxes.Count; i++)
            {
                inputBoxes[i].Text = String.Empty;
            }
            Location.Text = String.Empty; // Location is a ComboBox and cannot be part of the inputBoxes TextBox list.
            ModelNumCheckbox.IsChecked = false;
            SerialNumsCheckbox.IsChecked = false;

            // Send caret position back to PartNum textbox.
            PartNum.Focus();
        }

        // ------------------------------------------------------ User input validation
        private void Qty_CheckIfInt(object sender, TextChangedEventArgs e)
        {
            TextBox Qty = sender as TextBox;
            if (Qty.Text.Length > 0)
            {
                if (Qty.Text.Any(x => !char.IsDigit(x)))
                {
                    QtyWarning.Text = "Please enter a number.";
                }
                else
                {
                    QtyWarning.Text = "";
                }
            }
        }

        private void SerialNum_Entered(object sender, EventArgs e)
        {
            
        }
    }
}
