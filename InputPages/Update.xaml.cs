using Google.Protobuf.WellKnownTypes;
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
using System.Windows.Threading;

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
        private DispatcherTimer timer;

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

                if (!string.IsNullOrEmpty(items[firstIndex].SerialNums))
                {
                    SerialNumsCheckbox.IsChecked = true;
                    SerialNums.Text = items[firstIndex].SerialNums;
                }

            }

            // Serial Numbers entered on separate lines 5
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
                string commandText = "SELECT PartNum, BatchID, Description, CAST(Qty AS CHAR) AS Qty, Location, ModelNum, SerialNums FROM Rtable ORDER BY PartNum, BatchID";
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

        private void SerialNum_Entered(object sender, TextChangedEventArgs e)
        {
            TextBox SerialNums = sender as TextBox;
            // Add new line after every serial number if there is no automatic new line while scanning.
            if (SerialNums.Text.Length > 0)
            {
                char lastChar = SerialNums.Text[SerialNums.Text.Length - 1];

                if (lastChar != '\n')
                {
                    if (timer != null)
                    {
                        timer.Stop();
                    }

                    timer = new DispatcherTimer();
                    timer.Interval = TimeSpan.FromSeconds(0.5);
                    timer.Tick += (s, args) =>
                    {
                        SerialNums.Text += Environment.NewLine;
                        SerialNums.CaretIndex = SerialNums.Text.Length;

                        // Count number of serial numbers and compare against Qty textbox.
                        if (Qty.Text.Length > 0 && Qty.Text.Any(x => char.IsDigit(x)))
                        {
                            int quantity = Convert.ToInt32(Qty.Text);
                            string serialNums = SerialNums.Text;
                            int serialNumsCount = serialNums.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Length;
                            if (serialNumsCount != quantity)
                            {
                                SerialNumsWarning.Text = "Number of Serial Numbers entered does not match Quantity.";
                            }
                            else
                            {
                                SerialNumsWarning.Text = "";
                            }

                        }
                        timer.Stop();
                    };
                    timer.Start();

                }
            }

        }

        // ------------------------------------------------------ Sending updates to db

        private void UpdateItem_Click(object sender, RoutedEventArgs e)
        {
            List<string> placeholders = new List<string> { "@partNum", "@qty", "@description", "@location", "@batchId", "@modelNum", "@serialNums" };
            List<string> inputs = new List<string> { PartNum.Text, Qty.Text, Description.Text, Location.Text, BatchID.Text, ModelNum.Text, SerialNums.Text };
            var serialNumberList = SerialNums.Text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).ToList();

            // Get indexes for batchID to be updated, so that oldSerialNum(s) can be accessed.
            List<Item> items = GetFullItem();
            string batchID = SharedData.BatchID;
            List<string> oldSerialNumList = new List<string>();
            int firstIndex = items.FindIndex(a => a.BatchID == batchID);
            int lastIndex = items.FindLastIndex(a => a.BatchID == batchID);

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string commandText = "UPDATE Rtable SET PartNum = @partNum, Qty = @qty, Description = @description, Location = @location, ModelNum = @modelNum, SerialNums = @serialNums " +
                    "WHERE BatchID = @batchId && SerialNums = @oldSerialNum";
                MySqlCommand updateRow = new MySqlCommand(commandText, connection);

                if (serialNumberList.Count == 0)
                {
                    connection.Open();
                    updateRow.Parameters.AddWithValue("@oldSerialNum", "");
                    for (int j = 0; j < placeholders.Count; j++)
                    {
                        updateRow.Parameters.AddWithValue(placeholders[j], inputs[j]);
                    }
                    updateRow.ExecuteNonQuery();
                    connection.Close();
                }

                else if (serialNumberList.Count == 1)
                {
                    connection.Open();
                    updateRow.Parameters.AddWithValue("oldSerialNum", items[firstIndex].SerialNums.ToString());
                    for (int j = 0; j < placeholders.Count; j++)
                    {
                        updateRow.Parameters.AddWithValue(placeholders[j], inputs[j]);
                    }
                    updateRow.ExecuteNonQuery();
                    connection.Close();
                }

                // Create separate rows for each serial number.
                else
                {
                    for (int i = firstIndex; i <= lastIndex; i++)
                    {
                        oldSerialNumList.Add(items[i].SerialNums);
                    }

                    if (serialNumberList.Count == oldSerialNumList.Count)
                    {
                        connection.Open();
                        for (int i = 0; i < serialNumberList.Count; i++)
                        {
                            updateRow.Parameters.AddWithValue("@oldSerialNum", oldSerialNumList[i]);
                            for (int j = 0; j < placeholders.Count; j++)
                            {
                                switch (j)
                                {
                                    case 1:
                                        updateRow.Parameters.AddWithValue(placeholders[j], "1");
                                        break;

                                    case 6:
                                        updateRow.Parameters.AddWithValue(placeholders[j], serialNumberList[i]);
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

            }
            AddNewArea();
            ClearAll();
            updateFrame.Navigate(new Uri("/InventoryPage/Inventory.xaml", UriKind.Relative));
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


    }
}
