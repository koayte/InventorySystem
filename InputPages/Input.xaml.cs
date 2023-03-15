using MySql.Data.MySqlClient;
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
            inputBoxes = new List<TextBox> { PartNum, Qty, Description, ModelNum, SerialNums, BatchID };

            //MySqlCommand cmd = new MySqlCommand("select * from inputs", connection);

            //connection.Open();
            //DataTable dt = new DataTable();
            //dt.Load(cmd.ExecuteReader());
            //connection.Close();

            //dataGrid.DataContext = dt;
        }

        private void PartNum_TextChanged(object sender, TextChangedEventArgs e)
        {
            string partNum = PartNum.Text;
            if (!string.IsNullOrEmpty(partNum))
            {
                // Autofill Description, Location and BatchID based on PartNum 
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    string commandText = "SELECT Description, Location, BatchID FROM inputs WHERE PartNum = @PartNum";
                    MySqlCommand autoFillDescLoc = new MySqlCommand(commandText, connection);
                    autoFillDescLoc.Parameters.AddWithValue("@PartNum", partNum);

                    connection.Open();
                    MySqlDataReader reader = autoFillDescLoc.ExecuteReader();
                    if (reader.Read())
                    {
                        string desc = reader.GetString(0);
                        string location = reader.GetString(1);
                        int batchId = reader.GetInt32(2) + 1;
                        Description.Text = desc;
                        Location.Text = location;
                        BatchID.Text = batchId.ToString();
                    }
                    connection.Close();
                }

                // Autocheck ModelNum checkboxes based on PartNum
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    string commandText = "SELECT COUNT(1) FROM inputs WHERE PartNum = @PartNum AND ModelNum IS NOT NULL";
                    MySqlCommand autoCheckModelNum = new MySqlCommand(commandText, connection);
                    autoCheckModelNum.Parameters.AddWithValue("@PartNum", partNum);

                    connection.Open();
                    MySqlDataReader reader = autoCheckModelNum.ExecuteReader();
                    if (reader.Read())
                    {
                        int ModelNumExists = int.Parse(reader.GetString(0));
                        if (ModelNumExists > 0)
                        {
                            ModelNumCheckbox.IsChecked = true;
                        }
                    }
                    connection.Close();
                }

                // Autocheck SerialNums checkboxes based on PartNum
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    string commandText = "SELECT COUNT(1) FROM inputs WHERE PartNum = @PartNum AND SerialNums IS NOT NULL";
                    MySqlCommand autoCheckSerialNums = new MySqlCommand(commandText, connection);
                    autoCheckSerialNums.Parameters.AddWithValue("@PartNum", partNum);

                    connection.Open();
                    MySqlDataReader reader = autoCheckSerialNums.ExecuteReader();
                    if (reader.Read())
                    {
                        int SerialNumExists = int.Parse(reader.GetString(0));
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


        // Add comma after every serial number being entered by scanning
        private void SerialNum_Entered(object sender, TextChangedEventArgs e)
        {
            TextBox SerialNums = sender as TextBox;

            if (SerialNums.Text.Length > 0)
            {
                char lastChar = SerialNums.Text[SerialNums.Text.Length - 1];

                if (char.IsDigit(lastChar))
                {
                    if (timer != null)
                    {
                        timer.Stop();
                    }

                    timer = new DispatcherTimer();
                    timer.Interval = TimeSpan.FromSeconds(0.5);
                    timer.Tick += (s, args) =>
                    {
                        SerialNums.Text += ",";
                        SerialNums.CaretIndex = SerialNums.Text.Length;
                        timer.Stop();
                    };
                    timer.Start();

                }
            }
        }

        private void addItem_Click(object sender, RoutedEventArgs e)
        {
            //string partNum = PartNum.Text;
            //string qty = Qty.Text;
            //string description = Description.Text;
            //string location = Location.Text;
            //string modelNum = ModelNum.Text;
            //string serialNums = SerialNums.Text;
            //string batchId = BatchID.Text;

            List<string> placeholders = new List<string> { "@partNum", "@qty", "@description", "@location", "@modelNum", "@serialNums", "@batchID" };
            List<string> inputs = new List<string> { PartNum.Text, Qty.Text, Description.Text, Location.Text, ModelNum.Text, SerialNums.Text, BatchID.Text };

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string commandText1 = "INSERT INTO inputs (PartNum, Qty, Description, Location, ModelNum, SerialNums, BatchID) VALUE (@partNum, @qty, @description, @location, @modelNum, @serialNums, @batchId)";
                MySqlCommand addRow = new MySqlCommand(commandText1, connection);

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
                                addRow.Parameters.AddWithValue(placeholders[i], inputs[i]);
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

        }
    }
}
