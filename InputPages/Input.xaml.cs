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

        public Input()
        {
            InitializeComponent();
            inputBoxes = new List<TextBox> { PartNum, Qty, BatchID, Description, ModelNum, SerialNums };
        }

        // Checking PartNum against database
        private void PartNum_TextChanged(object sender, TextChangedEventArgs e)
        {
            string partNum = PartNum.Text;
            if (!string.IsNullOrEmpty(partNum))
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    // Autofill Description, Location and BatchID based on PartNum 
                    string commandText1 = "SELECT Description, Location, BatchID FROM inputs WHERE PartNum = @PartNum ORDER BY BatchID";
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

                    connection.Open();
                    MySqlDataReader reader1 = autoFillDescLoc.ExecuteReader();
                    if (reader1.HasRows)
                    {
                        while (reader1.Read())
                        {
                            Description.Text = reader1.GetString(0);
                            Location.Text = reader1.GetString(1);
                            int batchId = reader1.GetInt32(2) + 1;
                            BatchID.Text = batchId.ToString();
                        }
                        // PartNum does not exist in current inventory

                    }
                    else
                    {
                        BatchID.Text = "1";
                    }
                    connection.Close();

                    connection.Open();
                    MySqlDataReader reader2 = autoCheckModelNum.ExecuteReader();
                    if (reader2.Read())
                    {
                        int ModelNumExists = int.Parse(reader2.GetString(0));
                        if (ModelNumExists > 0)
                        {
                            ModelNumCheckbox.IsChecked = true;
                        }
                    }
                    connection.Close();

                    connection.Open();
                    MySqlDataReader reader3 = autoCheckSerialNums.ExecuteReader();
                    if (reader3.Read())
                    {
                        int SerialNumExists = int.Parse(reader3.GetString(0));
                        if (SerialNumExists > 0)
                        {
                            SerialNumsCheckbox.IsChecked = true;
                        }
                    }
                    connection.Close();
                }
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


        // Add new line after every serial number if there is no automatic new line while scanning.
        private void SerialNum_Entered(object sender, TextChangedEventArgs e)
        {
            TextBox SerialNums = sender as TextBox;

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


        // Buttons
        private void addItem_Click(object sender, RoutedEventArgs e)
        {
            List<string> placeholders = new List<string> { "@partNum", "@qty", "@description", "@location", "@modelNum", "@serialNums", "@batchID" };
            List<string> inputs = new List<string> { PartNum.Text, Qty.Text, Description.Text, Location.Text, ModelNum.Text, SerialNums.Text, BatchID.Text };

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string commandText = "INSERT INTO inputs (PartNum, Qty, Description, Location, ModelNum, SerialNums, BatchID) VALUE (@partNum, @qty, @description, @location, @modelNum, @serialNums, @batchId)";
                MySqlCommand addRow = new MySqlCommand(commandText, connection);

                for (int i = 0; i < placeholders.Count; i++)
                {
                    switch (i)
                    {
                        // For parameter @modelNum, if input text is empty, submit a null value.
                        case 4:
                            if (string.IsNullOrEmpty(inputs[i]))
                            {
                                addRow.Parameters.AddWithValue(placeholders[i], DBNull.Value);
                            }
                            else
                            {
                                addRow.Parameters.AddWithValue(placeholders[i], inputs[i]);
                            }
                            break;

                        // For parameter @serialNums, if input text is empty, submit a null value.
                        case 5:
                            if (string.IsNullOrEmpty(inputs[i]))
                            {
                                addRow.Parameters.AddWithValue(placeholders[i], DBNull.Value);
                            }
                            else
                            {
                                // Remove last character if it is a comma.
                                string serialNums = SerialNums.Text;
                                char lastChar = serialNums[serialNums.Length - 1];
                                if (lastChar == ',')
                                {
                                    serialNums = serialNums.Remove(serialNums.Length - 1, 1);
                                    addRow.Parameters.AddWithValue(placeholders[i], serialNums);
                                }
                            }
                            break;

                        default:
                            addRow.Parameters.AddWithValue(placeholders[i], inputs[i]);
                            break;
                    }
                }

                connection.Open();
                addRow.ExecuteNonQuery();
                connection.Close();
            }

            // Clear textboxes so they can be reused, reset checkboxes to default
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
        private void clearAll_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < inputBoxes.Count; i++)
            {
                inputBoxes[i].Text = String.Empty;
            }
            Location.Text = String.Empty; // Location is a ComboBox and cannot be part of the inputBoxes TextBox list.
            ModelNumCheckbox.IsChecked = false;
            SerialNumsCheckbox.IsChecked = false;
            PartNum.Focus();
        }
    }
}
