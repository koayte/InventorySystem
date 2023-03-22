using MySql.Data.MySqlClient;
using Org.BouncyCastle.Crypto.Tls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace InventorySystem
{
    /// <summary>
    /// Interaction logic for Input.xaml
    /// </summary>
    public partial class Input : Page
    {
        public string connectionString = "SERVER=localhost; DATABASE=inventory; UID=semi; PASSWORD=semitech;";
        private DispatcherTimer timer;
        private List<TextBox> inputBoxes;
        private List<string> areas = new List<string>();

        public Input()
        {
            InitializeComponent();
            inputBoxes = new List<TextBox> { PartNum, Qty, BatchID, Description, ModelNum, SerialNums };
            UpdateLocationComboBox();
        }

        // Checking PartNum against database
        private void PartNum_TextChanged(object sender, TextChangedEventArgs e)
        {
            DateTime dateTime = DateTime.Today;
            string todayDate = dateTime.ToString("yyyy-MM-dd");
            string partNum = PartNum.Text;
            if (!string.IsNullOrEmpty(partNum))
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    // Autofill Description and Location based on PartNum 
                    string commandText1 = "SELECT Description, Location FROM inputs WHERE PartNum = @PartNum";
                    MySqlCommand autoFillDescLoc = new MySqlCommand(commandText1, connection);
                    autoFillDescLoc.Parameters.AddWithValue("@PartNum", partNum);

                    // Autocheck ModelNum checkboxes based on PartNum
                    string commandText2 = "SELECT COUNT(1) FROM inputs WHERE PartNum = @PartNum AND ModelNum IS NOT NULL";
                    MySqlCommand autoCheckModelNum = new MySqlCommand(commandText2, connection);
                    autoCheckModelNum.Parameters.AddWithValue("@PartNum", partNum);

                    // Autocheck SerialNums checkboxes based on PartNum
                    string commandText3 = "SELECT COUNT(1) FROM inputs WHERE PartNum = @PartNum AND SerialNums IS NOT NULL";
                    MySqlCommand autoCheckSerialNums = new MySqlCommand(commandText3, connection);
                    autoCheckSerialNums.Parameters.AddWithValue("@PartNum", partNum);

                    // Get BatchByDay regardless of PartNum for that date
                    string commandText4 = "SELECT row_number() OVER (PARTITION BY Date ORDER BY Time) BatchByDay FROM inputs " +
                        "WHERE Date = @Date " +
                        "ORDER BY BatchByDay DESC";
                    MySqlCommand getBatchByDay = new MySqlCommand(commandText4, connection);
                    getBatchByDay.Parameters.AddWithValue("@Date", todayDate);

                    connection.Open();

                    using (var reader = autoFillDescLoc.ExecuteReader())
                    {
                        // PartNum exists in current inventory
                        if (reader.HasRows)
                        {
                            PartNumWarning.Text = "Part Number exists in inventory. Please register for the missing fields.";

                            while (reader.Read())
                            {
                                Description.Text = reader.GetString(0);
                                Location.Text = reader.GetString(1);
                            }
                        }

                        // PartNum does not exist in current inventory
                        else
                        {
                            PartNumWarning.Text = "Part Number does not exist in inventory. Please register for all fields.";
                            // BatchID.Text = "1";
                        }
                    }

                    using (var reader = autoCheckModelNum.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int ModelNumExists = int.Parse(reader.GetString(0));
                            if (ModelNumExists > 0)
                            {
                                ModelNumCheckbox.IsChecked = true;
                            }
                        }
                    }
      
                    using (var reader = autoCheckSerialNums.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int SerialNumExists = int.Parse(reader.GetString(0));
                            if (SerialNumExists > 0)
                            {
                                SerialNumsCheckbox.IsChecked = true;
                            }
                        }
                    }

                    // Set BatchID as YYYY-mm-dd_BatchByDay. E.g. 2023-03-20_3
                    using (var reader = getBatchByDay.ExecuteReader())
                    {
                        // If the first record of the day has already been input into db.
                        if (reader.Read())
                        {
                            int BatchByDay = reader.GetInt32(0) + 1;
                            BatchID.Text = todayDate + "_" + BatchByDay.ToString("00");
                        }

                        // If this is the first record of the day (hence query gives null results)
                        else
                        {
                            BatchID.Text = todayDate + "_" + "01";
                        }
                    }

                    connection.Close();
                }
            }
            else
            {
                PartNumWarning.Text = "";
                ClearAll();
            }
        }
        
        // Making ModelNum and SerialNums non-editable if checkboxes are unchecked.
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
                        SerialNums.Text += '\n';
                        SerialNums.CaretIndex = SerialNums.Text.Length;
                        
                        // Count number of serial numbers and compare against Qty textbox.
                        if (Qty.Text.Length > 0 && Qty.Text.Any(x => char.IsDigit(x)))
                        {
                            int quantity = Convert.ToInt32(Qty.Text);
                            string serialNums = SerialNums.Text;
                            int serialNumsCount = serialNums.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length;
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


        // Navigation around UI
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


        // Locations
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


        // Buttons
        private void addItem_Click(object sender, RoutedEventArgs e)
        {
            List<string> placeholders = new List<string> { "@partNum", "@qty", "@description", "@location", "@batchID", "@modelNum", "@serialNums" };
            List<string> inputs = new List<string> { PartNum.Text, Qty.Text, Description.Text, Location.Text, BatchID.Text, ModelNum.Text, SerialNums.Text };
            var serialNumberList = SerialNums.Text.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string commandText = "INSERT INTO inputs (PartNum, Qty, Description, Location, BatchID, ModelNum, SerialNums) VALUE (@partNum, @qty, @description, @location, @batchId, @modelNum, @serialNums)";
                MySqlCommand addRow = new MySqlCommand(commandText, connection);


                // If serial numbers are not entered, enter NULL value. Else, split into multiple db entries.
                if (string.IsNullOrEmpty(SerialNums.Text))
                {
                    connection.Open();
                    //addRow.Parameters.AddWithValue("@serialNums", DBNull.Value);
                    addRow.Parameters.AddWithValue("@serialNums", "");
                    for (int j = 0; j < placeholders.Count - 1; j++)
                    {
                        switch (j)
                        {
                            //case 5: // Set NULL value if model number is empty.
                            //    if (string.IsNullOrEmpty(inputs[j]))
                            //    {
                            //        addRow.Parameters.AddWithValue(placeholders[j], DBNull.Value);
                            //    }
                            //    else
                            //    {
                            //        addRow.Parameters.AddWithValue(placeholders[j], inputs[j]);
                            //    }
                            //    break;

                            default:
                                addRow.Parameters.AddWithValue(placeholders[j], inputs[j]);
                                break;
                        }
                    }
                    addRow.ExecuteNonQuery();
                    connection.Close();
                }
                else
                {
                    connection.Open();
                    // Create separate rows for each serial number.
                    foreach (var num in serialNumberList)
                    {
                        for (int j = 0; j < placeholders.Count; j++) 
                        {
                            switch (j)
                            {
                                case 1: // Set quantity = 1 for each row.
                                    addRow.Parameters.AddWithValue(placeholders[j], "1");
                                    break;

                                //case 5: // Set NULL value if model number is empty.
                                //    if (string.IsNullOrEmpty(inputs[j]))
                                //    {
                                //        addRow.Parameters.AddWithValue(placeholders[j], DBNull.Value);
                                //    }
                                //    else
                                //    {
                                //        addRow.Parameters.AddWithValue(placeholders[j], inputs[j]);
                                //    }
                                //    break;

                                case 6: // Set each serial number for each row.
                                    addRow.Parameters.AddWithValue(placeholders[j], num);
                                    break;

                                default:
                                    addRow.Parameters.AddWithValue(placeholders[j], inputs[j]);
                                    break;
                            }
                        }
                        addRow.ExecuteNonQuery();
                        addRow.Parameters.Clear();

                    }
                    connection.Close();
                }
            }

            AddNewArea();
            ClearAll();

        }
        private void clearAll_Click(object sender, RoutedEventArgs e)
        {
            ClearAll();
        }
        private void updateItem_Click(object sender, RoutedEventArgs e)
        {

        }



        // User input validation
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
