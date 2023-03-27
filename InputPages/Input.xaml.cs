using InventorySystem.InventoryPage;
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
                    string commandText1 = "SELECT Description, Area, Section FROM Rtable WHERE PartNum = @PartNum";
                    MySqlCommand autoFillDescLoc = new MySqlCommand(commandText1, connection);
                    autoFillDescLoc.Parameters.AddWithValue("@PartNum", partNum);

                    // Autocheck ModelNum checkboxes based on PartNum
                    string commandText2 = "SELECT COUNT(1) FROM Rtable WHERE PartNum = @PartNum AND (ModelNum = '' IS FALSE)";
                    MySqlCommand autoCheckModelNum = new MySqlCommand(commandText2, connection);
                    autoCheckModelNum.Parameters.AddWithValue("@PartNum", partNum);

                    // Autocheck SerialNums checkboxes based on PartNum
                    string commandText3 = "SELECT COUNT(1) FROM Rtable WHERE PartNum = @PartNum AND (SerialNums = '' IS FALSE)";
                    MySqlCommand autoCheckSerialNums = new MySqlCommand(commandText3, connection);
                    autoCheckSerialNums.Parameters.AddWithValue("@PartNum", partNum);

                    // Get Batch for that date
                    string commandText4 = "SELECT Batch FROM batchNumbers WHERE Date = @Date";
                    MySqlCommand getBatch = new MySqlCommand(commandText4, connection);
                    getBatch.Parameters.AddWithValue("@Date", todayDate);

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
                                Area.Text = reader.GetString(1);
                                Section.Text = reader.GetString(2);
                            }
                        }

                        // PartNum does not exist in current inventory
                        else
                        {
                            PartNumWarning.Text = "Part Number does not exist in inventory. Please register for ALL fields.";
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

                    // Set BatchID as YYYY-mm-dd_BatchByDay. E.g. 2023-03-20_003
                    using (var reader = getBatch.ExecuteReader())
                    {
                        // If the first record of the day has already been input into db.
                        if (reader.Read())
                        {
                            int Batch = reader.GetInt32(0) + 1;
                            BatchID.Text = todayDate + "_" + Batch.ToString("000");
                        }

                        // If this is the first record of the day (hence query gives null results)
                        else
                        {
                            BatchID.Text = todayDate + "_" + "001";
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
                            string serialNumsReplaced = SerialNums.Text.Replace("\r\n", "\n");
                            int serialNumsCount = serialNumsReplaced.Split("\n", StringSplitOptions.RemoveEmptyEntries).Length;
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
            Area.Text = String.Empty; // Location is a ComboBox and cannot be part of the inputBoxes TextBox list.
            Section.Text = String.Empty;
            ModelNumCheckbox.IsChecked = false;
            SerialNumsCheckbox.IsChecked = false;

            // Send caret position back to PartNum textbox.
            PartNum.Focus();
        }


        // Locations
        //private List<string> AddLocationsIntoString()
        //{
        //    using (MySqlConnection connection = new MySqlConnection(connectionString))
        //    {
        //        MySqlCommand getArea = new MySqlCommand("SELECT Area FROM locations", connection);
        //        connection.Open();
        //        using (var reader = getArea.ExecuteReader())
        //        {
        //            while (reader.Read())
        //            {
        //                var area = reader.GetString(0);
        //                areas.Add(area);
        //            }
        //        }
        //        connection.Close();
        //    }
        //    return (areas);
        //}

        //private void AddNewArea()
        //{
        //    DataSource dataSource = new DataSource();
        //    List<string> areas = dataSource.locations.Select(x => x.Area).ToList();

        //    // If user inputs a new area that is not in the current database, add to db.
        //    if (!areas.Contains(Location.Text))
        //    {
        //        using (MySqlConnection connection = new MySqlConnection(connectionString))
        //        {
        //            string commandText = "INSERT INTO locations (Area) VALUE (@Area)";
        //            MySqlCommand addNewArea = new MySqlCommand(commandText, connection);
        //            addNewArea.Parameters.AddWithValue("@Area", Location.Text);

        //            connection.Open();
        //            addNewArea.ExecuteNonQuery();
        //            connection.Close();
        //        }
        //    }
        //}


        // Buttons
        private void addItem_Click(object sender, RoutedEventArgs e)
        {
            List<string> placeholders = new List<string> { "@userName", "@partNum", "@qty", "@description", "@area", "@section", "@batchID", "@modelNum", "@serialNums" };
            List<string> inputs = new List<string> { User.Text, PartNum.Text, Qty.Text, Description.Text, Area.Text, Section.Text, BatchID.Text, ModelNum.Text, SerialNums.Text };
            string serialNumsReplaced = SerialNums.Text.Replace("\r\n", "\n");
            var serialNumberList = serialNumsReplaced.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                // ADD INTO REAL-TIME INVENTORY DATABASE TABLE.
                string commandText1 = "INSERT INTO Rtable (UserName, PartNum, Qty, Description, Area, Section, BatchID, ModelNum, SerialNums) VALUE (@userName, @partNum, @qty, @description, @area, @section, @batchId, @modelNum, @serialNums)";
                MySqlCommand addRow = new MySqlCommand(commandText1, connection);

                // If serial numbers are not entered, enter "" value. Else, split into multiple db entries.
                if (string.IsNullOrEmpty(SerialNums.Text))
                {
                    connection.Open();
                    addRow.Parameters.AddWithValue("@serialNums", "");
                    for (int j = 0; j < placeholders.Count - 1; j++)
                    {
                        addRow.Parameters.AddWithValue(placeholders[j], inputs[j]);
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
                                case 2: // Set quantity = 1 for each row.
                                    addRow.Parameters.AddWithValue(placeholders[j], "1");
                                    break;

                                case 8: // Set each serial number for each row.
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

                // ADD INTO BATCHNUMBERS DATABASE TABLE.
                string commandText2 = "INSERT INTO batchNumbers (Batch) VALUES (@Batch) " +
                    "ON DUPLICATE KEY UPDATE Batch = @Batch";
                MySqlCommand addDateBatch = new MySqlCommand(commandText2, connection);
                List<string> DateBatch = BatchID.Text.Split('_').ToList(); // E.g. split 2023-03-24_001 into ("2023-03-24", "001")
                addDateBatch.Parameters.AddWithValue("@Batch", DateBatch[1]);
                connection.Open();
                addDateBatch.ExecuteNonQuery();
                connection.Close();

                // ADD INTO HISTORY DATABASE TABLE.
                string commandText3 = "INSERT INTO Htable (UserName, Status, PartNum, Qty, Description, Area, Section, BatchID, ModelNum, SerialNums) " +
                    "VALUE (@userName, @status, @partNum, @qty, @description, @area, @section, @batchId, @modelNum, @serialNums)";
                MySqlCommand addRecord = new MySqlCommand(commandText3, connection);
                if (serialNumberList.Count <= 1)
                {
                    connection.Open();
                    addRecord.Parameters.AddWithValue("@status", "Check in");
                    for (int j = 0; j < placeholders.Count; j++)
                    {
                        addRecord.Parameters.AddWithValue(placeholders[j], inputs[j]);
                    }
                    addRecord.ExecuteNonQuery();
                    connection.Close();
                }
                else
                {
                    connection.Open();
                    // Create separate rows for each serial number.
                    foreach (var num in serialNumberList)
                    {
                        addRecord.Parameters.AddWithValue("@status", "Check in");
                        for (int j = 0; j < placeholders.Count; j++)
                        {
                            switch (j)
                            {
                                case 2: // Set quantity = 1 for each row.
                                    addRecord.Parameters.AddWithValue(placeholders[j], "1");
                                    break;

                                case 8: // Set each serial number for each row.
                                    addRecord.Parameters.AddWithValue(placeholders[j], num);
                                    break;

                                default:
                                    addRecord.Parameters.AddWithValue(placeholders[j], inputs[j]);
                                    break;
                            }
                        }
                        addRecord.ExecuteNonQuery();
                        addRecord.Parameters.Clear();
                    }
                    connection.Close();
                }
            }

            // AddNewArea();
            ClearAll();

        }
        private void clearAll_Click(object sender, RoutedEventArgs e)
        {
            ClearAll();
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
