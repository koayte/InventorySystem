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
            inputBoxes = new List<TextBox> { PartNum, Qty, BatchID, Description, ModelNum, SerialNums };
        }

        private void SetTextBoxesFromUpdate() 
        {
            BatchID.Text = SharedData.BatchID;
            PartNum.Text = SharedData.PartNum;
            Description.Text = SharedData.Description;
            Area.Text = SharedData.Area;
            Section.Text = SharedData.Section;
            Qty.Text = SharedData.Qty;
            Remarks.Text = SharedData.Remarks;
            Supplier.Text = SharedData.Supplier;

            if (!string.IsNullOrEmpty(SharedData.ModelNum))
            {
                ModelNum.Text = SharedData.ModelNum;
                ModelNumCheckbox.IsChecked = true;
            }

            if (!string.IsNullOrEmpty(SharedData.SerialNums))
            {
                SerialNumsCheckbox.IsChecked = true;
                SerialNums.Text = SharedData.SerialNums;
                SerialNums.CaretIndex = SerialNums.Text.Length;
            }
            else
            {
                SerialNumsCheckbox.IsChecked = false;
            }
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

        // ------------------------------------------------------ Sending updates to db

        private void UpdateItem_Click(object sender, RoutedEventArgs e)
        {
            List<string> placeholders = new List<string> { "@userName", "@partNum", "@qty", "@supplier", "@description", "@area", "@section", "@batchId", "@modelNum", "@serialNums", "@remarks" };
            List<string> inputs = new List<string> { User.Text, PartNum.Text, Qty.Text, Supplier.Text, Description.Text, Area.Text, Section.Text, BatchID.Text, ModelNum.Text, SerialNums.Text, Remarks.Text };
            string serialNumsReplaced = SerialNums.Text.Replace("\r\n", "\n");
            var serialNumberList = serialNumsReplaced.Split("\n", StringSplitOptions.RemoveEmptyEntries).ToList();

            // Get indexes for batchID to be updated, so that oldSerialNum(s) can be accessed.
            string batchID = SharedData.BatchID;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                // DELETE ROWS WITH THIS BATCHID
                string commandText = "DELETE FROM Rtable WHERE BatchID = @batchId;";
                MySqlCommand cmd = new MySqlCommand(commandText, connection);
                cmd.Parameters.AddWithValue("@batchId", BatchID.Text);
                connection.Open();
                cmd.ExecuteNonQuery();

                // INSERT UPDATED ROWS WITH THIS BATCHID
                commandText = "INSERT INTO Rtable (UserName, PartNum, BatchID, Supplier, Description, Qty, Area, Section, ModelNum, SerialNums, Remarks) VALUE " +
                    "(@userName, @partNum, @batchId, @supplier, @description, @qty, @area, @section, @modelNum, @serialNums, @remarks)";
                cmd.CommandText = commandText;
                cmd.Parameters.Clear();
                if (serialNumberList.Count <= 1)
                {
                    for (int j = 0; j < placeholders.Count; j++)
                    {
                        cmd.Parameters.AddWithValue(placeholders[j], inputs[j]);
                    }
                    cmd.ExecuteNonQuery();
                }

                // CREATE SEPARATE ROWS FOR EACH SERIAL NUMBER
                else
                {
                    foreach (var num in serialNumberList) 
                    {
                        for (int j = 0; j < placeholders.Count; j++)
                        {
                            switch (j)
                            {
                                case 2:
                                    cmd.Parameters.AddWithValue(placeholders[j], "1");
                                    break;

                                case 9:
                                    cmd.Parameters.AddWithValue(placeholders[j], num);
                                    break;

                                default:
                                    cmd.Parameters.AddWithValue(placeholders[j], inputs[j]);
                                    break;
                            }
                        }
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                    }
                }

                // ADD INTO HISTORY DATABASE TABLE
                cmd.Parameters.Clear();
                commandText = "INSERT INTO Htable (UserName, Status, PartNum, Supplier, Qty, Description, Area, Section, BatchID, ModelNum, SerialNums, Remarks) " +
                    "VALUE (@userName, @status, @partNum, @supplier, @qty, @description, @area, @section, @batchId, @modelNum, @serialNums, @remarks)";
                cmd.CommandText = commandText;

                if (serialNumberList.Count <= 1)
                {
                    cmd.Parameters.AddWithValue("@status", "Update");
                    for (int j = 0; j < placeholders.Count; j++)
                    {
                        cmd.Parameters.AddWithValue(placeholders[j], inputs[j]);
                    }
                    cmd.ExecuteNonQuery();
                }

                // CREATE SEPARATE ROWS FOR EACH SERIAL NUMBER
                else
                {
                    foreach (var num in serialNumberList)
                    {
                        cmd.Parameters.AddWithValue("@status", "Update");
                        for (int j = 0; j < placeholders.Count; j++)
                        {
                            switch (j)
                            {
                                case 2:
                                    cmd.Parameters.AddWithValue(placeholders[j], "1");
                                    break;

                                case 9:
                                    cmd.Parameters.AddWithValue(placeholders[j], num);
                                    break;

                                default:
                                    cmd.Parameters.AddWithValue(placeholders[j], inputs[j]);
                                    break;
                            }
                        }
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                    }

                }
            }
            // AddNewArea();
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

        //private void AddNewArea()
        //{
        //    areas = AddLocationsIntoString();

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
            Area.Text = String.Empty; 
            Section.Text = String.Empty;
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
                    QtyWarning.Text = "Please enter a number for quantity.";
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
