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
using InventorySystem.InventoryPage;

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

        public Input()
        {
            InitializeComponent();
            inputBoxes = new List<TextBox> { PartNum, Qty, BatchID, Description, ModelNum, SerialNums, Remarks };
        }

        // Checking PartNum against database
        private void PartNum_TextChanged(object sender, TextChangedEventArgs e)
        {
            DateTime dateTime = DateTime.Today;
            string todayDate = dateTime.ToString("yyyy-MM-dd");
            string partNum = PartNum.Text;

            if (!string.IsNullOrEmpty(partNum))
            {
                DataSource dataSource = new DataSource();
                Product selectedProduct = dataSource.products.SingleOrDefault(x => x.PartNum == partNum);
                if (selectedProduct != null) 
                {
                    // Show alert that PartNum exists in product database.
                    PartNumWarning.Text = "Part Number exists in product database.";

                    // Autofill ModelNum, Description, Area, Section, Supplier based on PartNum, from Ptable. 
                    ModelNum.Text = selectedProduct.ModelNum.ToString();
                    Description.Text = selectedProduct.Description.ToString();
                    Area.Text = selectedProduct.Area.ToString();
                    Section.Text = selectedProduct.Section.ToString();
                    Supplier.Text = selectedProduct.Supplier.ToString();

                    // Autocheck ModelNum and SerialNums Checkboxes based on PartNum, from Ptable.
                    if (!string.IsNullOrEmpty(selectedProduct.ModelNum.ToString()))
                    {
                        ModelNumCheckbox.IsChecked = true;
                    }

                    bool serialNumsExist = bool.Parse(selectedProduct.SerialNumsExist.ToString());
                    if (serialNumsExist == true)
                    {
                        SerialNumsCheckbox.IsChecked = true;
                    }
                }

                else
                {
                    PartNumWarning.Text = "Part Number does not exist in product database.";
                    // Clear other textboxes.
                    Description.Text = String.Empty;
                    Qty.Text = String.Empty;
                    Area.Text = String.Empty;
                    Section.Text = String.Empty;
                    Remarks.Text = String.Empty;
                    Supplier.Text = String.Empty;
                    ModelNum.Text = String.Empty;
                    SerialNums.Text = String.Empty;
                    ModelNumCheckbox.IsChecked = false;
                    SerialNumsCheckbox.IsChecked = false;
                }

                // Set BatchID as YYYY-mm-dd_BatchByDay. E.g. 2023-03-20_003
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    string commandText4 = "SELECT Batch FROM batchNumbers WHERE Date = @Date";
                    MySqlCommand getBatch = new MySqlCommand(commandText4, connection);
                    getBatch.Parameters.AddWithValue("@Date", todayDate);

                    connection.Open();
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
                        if (Qty.Text.Length > 0 && Qty.Text.All(Char.IsDigit))
                        {
                            int quantity = Convert.ToInt32(Qty.Text);
                            string serialNumsReplaced = SerialNums.Text.Replace("\r\n", "\n");
                            int serialNumsCount = serialNumsReplaced.Split("\n", StringSplitOptions.RemoveEmptyEntries).Length;
                            if (serialNumsCount != quantity)
                            {
                                SerialNumsWarning.Text = "WARNING: Number of Serial Numbers entered does not match Quantity.";
                                SerialNums.BorderBrush = Brushes.Red;
                            }
                            else
                            {
                                SerialNumsWarning.Text = "";
                                SerialNums.ClearValue(TextBox.BorderBrushProperty);
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
            Supplier.Text = String.Empty;
            ModelNumCheckbox.IsChecked = false;
            SerialNumsCheckbox.IsChecked = false;

            // Send caret position back to PartNum textbox.
            PartNum.Focus();
        }

        // Add user input in Supplier ComboBox to Suppliers table in db.
        private void AddSupplierDB()
        {
            string supplier = Supplier.Text;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string commandText = "INSERT INTO suppliers (Supplier) VALUE (@supplier)" +
                    " ON DUPLICATE KEY UPDATE Supplier = @supplier";
                MySqlCommand addSupplier = new MySqlCommand(commandText, connection);
                addSupplier.Parameters.AddWithValue("@supplier", supplier);
                connection.Open();
                addSupplier.ExecuteNonQuery();
                connection.Close();
            }
        }

        // Buttons
        private void addItem_Click(object sender, RoutedEventArgs e)
        {
            List<string> placeholders = new List<string> { "@userName", "@partNum", "@supplier", "@qty", "@description", "@area", "@section", "@batchID", "@modelNum", "@serialNums", "@remarks"};
            List<string> inputs = new List<string> { User.Text, PartNum.Text, Supplier.Text, Qty.Text, Description.Text, Area.Text, Section.Text, BatchID.Text, ModelNum.Text, SerialNums.Text, Remarks.Text };
            string serialNumsReplaced = SerialNums.Text.Replace("\r\n", "\n");
            var serialNumberList = serialNumsReplaced.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                // ADD INTO REAL-TIME INVENTORY DATABASE TABLE.
                string commandText1 = "INSERT INTO Rtable (UserName, PartNum, Supplier, Qty, Description, Area, Section, BatchID, ModelNum, SerialNums, Remarks) " +
                    "VALUE (@userName, @partNum, @supplier, @qty, @description, @area, @section, @batchId, @modelNum, @serialNums, @remarks)";
                MySqlCommand addRow = new MySqlCommand(commandText1, connection);

                // If serial numbers are not entered, enter "" value. Else, split into multiple db entries.
                if (string.IsNullOrEmpty(SerialNums.Text))
                {
                    connection.Open();
                    for (int j = 0; j < placeholders.Count; j++)
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
                                case 3: // Set quantity = 1 for each row.
                                    addRow.Parameters.AddWithValue(placeholders[j], "1");
                                    break;

                                case 9: // Set each serial number for each row.
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
                string commandText3 = "INSERT INTO Htable (UserName, Status, PartNum, Supplier, Qty, Description, Area, Section, BatchID, ModelNum, SerialNums, Remarks) " +
                    "VALUE (@userName, @status, @partNum, @supplier, @qty, @description, @area, @section, @batchId, @modelNum, @serialNums, @remarks)";
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
                                case 3: // Set quantity = 1 for each row.
                                    addRecord.Parameters.AddWithValue(placeholders[j], "1");
                                    break;

                                case 9: // Set each serial number for each row.
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

                // ADD INTO UNIQUE PRODUCT TABLE.
                string commandText4 = "INSERT INTO ptable (PartNum, ModelNum, Description, Area, Section, SerialNumsExist, Supplier) " +
                    "VALUES (@partNum, @modelNum, @description, @area, @section, @serialNumsExist, @supplier) " +
                    "ON DUPLICATE KEY UPDATE ModelNum = @modelNum, Description = @description, Area = @area, Section = @section, SerialNumsExist = @serialNumsExist, Supplier = @supplier";
                MySqlCommand addProduct = new MySqlCommand(commandText4, connection);
                addProduct.Parameters.AddWithValue("@partNum", PartNum.Text);
                addProduct.Parameters.AddWithValue("@modelNum", ModelNum.Text);
                addProduct.Parameters.AddWithValue("@description", Description.Text);
                addProduct.Parameters.AddWithValue("@area", Area.Text);
                addProduct.Parameters.AddWithValue("@section", Section.Text);
                addProduct.Parameters.AddWithValue("@supplier", Supplier.Text);
                if (string.IsNullOrEmpty(SerialNums.Text))
                {
                    addProduct.Parameters.AddWithValue("@serialNumsExist", "False");
                }
                else
                {
                    addProduct.Parameters.AddWithValue("@serialNumsExist", "True");
                }
                connection.Open();
                addProduct.ExecuteNonQuery();
                connection.Close();
            }

            AddSupplierDB();
            ClearAll();
            Success.Text = "Item added successfully!";

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
                    QtyWarning.Text = "WARNING: Please enter a number for quantity.";
                    Qty.BorderBrush = Brushes.Red;
                }
                else
                {
                    QtyWarning.Text = "";
                    Qty.ClearValue(TextBox.BorderBrushProperty);
                }
            }
        }


    }
}
