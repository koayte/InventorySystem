using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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
using InventorySystem.InventoryPage;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Tls;

namespace InventorySystem.Checkout
{
    /// <summary>
    /// Interaction logic for CheckOut.xaml
    /// </summary>
    public partial class CheckOut : Page
    {
        private List<TextBox> inputBoxes;
        private string connectionString = "SERVER=localhost; DATABASE=inventory; UID=semi; PASSWORD=semitech;";
        private ObservableCollection<Product> products;
        private ObservableCollection<Item> items;
        public List<SerialNumber> serialNumObjList;
        public List<string> serialNumsSelected;

        public CheckOut()
        {
            InitializeComponent();

            inputBoxes = new List<TextBox> { Remarks, ModelNum, Area, Section, Supplier, Qty };
            User.Focus();

            DataSource dataSource = new DataSource();
            products = dataSource.products;
            items = dataSource.items;

        }

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
        // ------------------------------------------------- CheckBoxes
        private void Model_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
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

        // ------------------------------------------------- Description Combobox
        //private void DescriptionBox_Bind()
        //{
        //    Description.ItemsSource = products.Select(x => x.Description).ToList();
        //}
        //private void Description_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    ClearAll();
        //    Description.ItemsSource = products.Where(x => x.Description.ToLower().Contains(Description.Text.ToLower())).Select(x => x.Description).ToList();
        //}


        private void Description_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (DescCheckbox.IsChecked == true && PartNumCheckbox.IsChecked == false)
            //{

            //}

            List<string> partNumList = new List<string>();
            var descSelected = (sender as ComboBox).SelectedValue?.ToString() ?? "";
            ClearAll();
            if (!string.IsNullOrEmpty(descSelected)) 
            {
                if (DescCheckbox.IsChecked == true) // Autofill PartNum if user searches by Description.
                {
                    partNumList = products.Where(x => x.Description == descSelected).Select(x => x.PartNum).ToList();
                    if (partNumList.Count == 1)
                    {
                        PartNum.Text = partNumList[0];
                        choosePart.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        MessageBox.Show("More than 1 PartNum found. Please select 1.");
                        choosePart.Visibility = Visibility.Visible;
                        choosePartBox.ItemsSource = partNumList;
                    }
                }
            }
        }

        private void confirmPart_Click(object sender, RoutedEventArgs e)
        {
            PartNum.Text = choosePartBox.SelectedItem as string;
        }

        private void PartNum_TextChanged(object sender, TextChangedEventArgs e)
        {
            BatchAndQty.Text = "BatchID, Remaining Qty \n";
            string partNumSelected = PartNum.Text;
            var prodSelected = products.SingleOrDefault(x => x.PartNum == partNumSelected);
            List<string> batchIDList = new List<string>();
            Dictionary<string, string> batchQtyDict = new Dictionary<string, string>();
            ClearAll();
            if (!string.IsNullOrEmpty(partNumSelected))
            {
                if (prodSelected != null) // Product exists in ptable.
                {
                    if (PartNumCheckbox.IsChecked == true) // Autoselect Description if user searches by PartNum.
                    {
                        Description.SelectedValue = prodSelected.Description.ToString();
                    }

                    batchIDList = items.Where(x => x.PartNum == partNumSelected).Select(x => x.BatchID).Distinct().ToList();
                    if (batchIDList.Count == 0) // Product has run out of stock in rtable.
                    {
                        PartNumWarning.Text = "WARNING: Product is out of stock.";
                        BatchID.ItemsSource = null;
                    }
                    else // Product still has stock in rtable.
                    {
                        PartNumWarning.Text = "";
                        // Autofill Area, Section, ModelNum, Supplier based on PartNum from Product Table
                        Area.Text = prodSelected.Area.ToString();
                        Section.Text = prodSelected.Section.ToString();
                        Supplier.Text = prodSelected.Supplier.ToString();
                        if (!string.IsNullOrEmpty(prodSelected.ModelNum.ToString()))
                        {
                            ModelNumCheckbox.IsChecked = true;
                            ModelNum.Text = prodSelected.ModelNum.ToString();
                        }

                        // Bind relevant BatchID with remaining Qty to BatchID ComboBox.
                        for (int i = 0; i < batchIDList.Count; i++)
                        {
                            string batchID = batchIDList[i];
                            List<int> currentQtyList = items.Where(x => x.BatchID == batchID).Select(x => Int32.Parse(x.Qty)).ToList();
                            string currentQty = currentQtyList.Sum().ToString();
                            batchQtyDict.Add(batchID, currentQty);
                        }
                        BatchAndQty.Text = BatchAndQty.Text + String.Join('\n', batchQtyDict);
                        BatchID.ItemsSource = batchIDList;
                        if (batchIDList.Count == 1)
                        {
                            BatchID.SelectedValue = batchIDList[0];
                        }
                    }
                }

                else // Product does not exist in ptable.
                {
                    PartNumWarning.Text = "WARNING: Product does not exist in product table.";
                }
            }
        }

        //private void PartNum_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    BatchAndQty.Text = "BatchID, Remaining Qty \n";
        //    string partNumSelected = PartNum.Text;
        //    var prodSelected = products.SingleOrDefault(x => x.PartNum == partNumSelected);
        //    List<string> batchIDList = new List<string>();
        //    Dictionary<string, string> batchQtyDict = new Dictionary<string, string>();
        //    ClearAll();
        //    Description.Text = "";
        //    if (!string.IsNullOrEmpty(partNumSelected))
        //    {
        //        if (prodSelected != null) // Product exists in ptable.
        //        {
        //            if (string.IsNullOrEmpty(Description.Text)) // Autoselect Description if user searches by PartNum.
        //            {
        //                Description.SelectedValue = prodSelected.Description.ToString();
        //            }

        //            batchIDList = items.Where(x => x.PartNum == partNumSelected).Select(x => x.BatchID).Distinct().ToList();
        //            if (batchIDList.Count == 0) // Product has run out of stock in rtable.
        //            {
        //                PartNumWarning.Text = "WARNING: Product is out of stock.";
        //                BatchID.ItemsSource = null;
        //            }
        //            else // Product still has stock in rtable.
        //            {
        //                PartNumWarning.Text = "";
        //                // Autofill Area, Section, ModelNum, Supplier based on PartNum from Product Table
        //                Area.Text = prodSelected.Area.ToString();
        //                Section.Text = prodSelected.Section.ToString();
        //                Supplier.Text = prodSelected.Supplier.ToString();
        //                if (!string.IsNullOrEmpty(prodSelected.ModelNum.ToString()))
        //                {
        //                    ModelNumCheckbox.IsChecked = true;
        //                    ModelNum.Text = prodSelected.ModelNum.ToString();
        //                }

        //                // Bind relevant BatchID with remaining Qty to BatchID ComboBox.
        //                for (int i = 0; i < batchIDList.Count; i++)
        //                {
        //                    string batchID = batchIDList[i];
        //                    List<int> currentQtyList = items.Where(x => x.BatchID == batchID).Select(x => Int32.Parse(x.Qty)).ToList();
        //                    string currentQty = currentQtyList.Sum().ToString();
        //                    batchQtyDict.Add(batchID, currentQty);
        //                }
        //                BatchAndQty.Text = BatchAndQty.Text + String.Join('\n', batchQtyDict);
        //                BatchID.ItemsSource = batchIDList;
        //                if (batchIDList.Count == 1)
        //                {
        //                    BatchID.SelectedValue = batchIDList[0];
        //                }
        //            }
        //        }

        //        else // Product does not exist in ptable.
        //        {
        //            PartNumWarning.Text = "WARNING: Product does not exist in product table.";
        //        }
        //    }
        //}

        private void BatchID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string batchID = (sender as ComboBox).SelectedValue?.ToString() ?? "";
            if (!string.IsNullOrEmpty(batchID))
            {
                string partNumSelected = PartNum.Text;
                List<string> serialNumStrList = new List<string>();
                serialNumStrList = items.Where(x => (x.BatchID == batchID) && (x.SerialNums != "")).Select(x => x.SerialNums).ToList();
                serialNumObjList = new List<SerialNumber>();
                foreach (var serialNumString in serialNumStrList)
                {
                    serialNumObjList.Add(new SerialNumber()
                    {
                        SerialNum = serialNumString
                    });
                }
                // Bind SerialNum combobox to serialNumList. 
                if (serialNumObjList.Count > 0)
                {
                    SerialNumsCheckbox.IsChecked = true;
                    SerialNums.ItemsSource = serialNumObjList;
                }
                else // If there are no SerialNums for this PartNum
                {
                    SerialNumsCheckbox.IsChecked = false;
                    SerialNums.ItemsSource = null;
                }
            }
        }


        // ------------------------------------------------- Serial Number multi-select 
        private void checkSerialNum_CheckedandUnchecked(object sender, RoutedEventArgs e)
        {
            serialNumsSelected = new List<string>();
            foreach (var serialNumObj in serialNumObjList)
            {
                if (serialNumObj.CheckStatus == true)
                {
                    serialNumsSelected.Add(serialNumObj.SerialNum);
                }
            }
            SerialNums.Text = string.Join(",", serialNumsSelected.ToArray());

            // Set Qty Checked Out for number of serialNums being selected
            if (serialNumsSelected.Count > 0)
            {
                Qty.Text = serialNumsSelected.Count.ToString();
            }
            else
            {
                Qty.Text = "";
            }
        }

        // ------------------------------------------------- Qty Checked Out

        private void Qty_TextChanged(object sender, TextChangedEventArgs e)
        {
            string batchID = BatchID.SelectedValue?.ToString() ?? "";
            if (Qty.Text.Length > 0)
            {
                if (Qty.Text.Any(x => !char.IsDigit(x)))
                {
                    QtyWarning.Text = "WARNING: Please enter a number for quantity.";
                }

                else
                {
                    QtyWarning.Text = "";
                    if (!string.IsNullOrEmpty(batchID))
                    {
                        List<int> currentQtyList = items.Where(x => x.BatchID == batchID).Select(x => Int32.Parse(x.Qty)).ToList();
                        int currentQtyInt = currentQtyList.Sum();
                        int qtyCheckedOutInt = Int32.Parse(Qty.Text);
                        if (qtyCheckedOutInt > currentQtyInt)
                        {
                            QtyWarning.Text = "WARNING: Please enter a number smaller than the current inventory quantity.";
                        }
                    }
                }
            }
            else
            {
                QtyWarning.Text = "";
            }
        }

        // ------------------------------------------------- Buttons
        private void ClearAll()
        {
            // Clear textboxes, reset checkboxes to default
            for (int i = 0; i < inputBoxes.Count; i++)
            {
                inputBoxes[i].Text = String.Empty;
            }
            SerialNums.Text = String.Empty;
            BatchID.SelectedValue = null;
            ModelNumCheckbox.IsChecked = false;
            SerialNumsCheckbox.IsChecked = false;

            // Send caret position back to PartNum textbox.
            PartNum.Focus();

            choosePart.Visibility = Visibility.Hidden;
        }

        private void checkOut_Click(object sender, RoutedEventArgs e)
        {
            // Disable button after one click 
            checkOut.IsEnabled = false;

            // Get BatchID 
            string batchID = BatchID.SelectedValue?.ToString() ?? "";
            List<int> currentQtyList = items.Where(x => x.BatchID == batchID).Select(x => Int32.Parse(x.Qty)).ToList();
            string currentQty = currentQtyList.Sum().ToString();
            string qtyCheckedOut = Qty.Text;

            List<string> placeholders = new List<string> { "@userName", "@partNum", "@batchID", "@supplier", "@description", "@qty", "@area", "@section",  "@modelNum", "@remarks" };
            List<string> inputs = new List<string> { User.Text, PartNum.Text, batchID, Supplier.Text, Description.Text, Qty.Text, Area.Text, Section.Text, ModelNum.Text, Remarks.Text };

            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    string commandText = "";
                    MySqlCommand cmd = new MySqlCommand(commandText, connection);
                    connection.Open();

                    // If SerialNum is selected
                    if (!string.IsNullOrEmpty(SerialNums.Text))
                    {
                        cmd.CommandText = "DELETE FROM Rtable WHERE PartNum = @partNum && SerialNums = @serialNum";
                        // Delete from Rtable
                        foreach (var serialNumSelected in serialNumsSelected)
                        {
                            cmd.Parameters.AddWithValue("@partNum", PartNum.Text);
                            cmd.Parameters.AddWithValue("@serialNum", serialNumSelected);
                            cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();
                        }
                    }

                    else
                    {
                        if (currentQty == "1") // Delete entry
                        {
                            cmd.CommandText = "DELETE FROM Rtable WHERE PartNum = @partNum && BatchID = @batchID";
                        }

                        else if (currentQty == qtyCheckedOut) // Delete entry
                        {
                            cmd.CommandText = "DELETE FROM Rtable WHERE PartNum = @partNum && BatchID = @batchID";
                        }

                        else // Minus QtyCheckedOut from quantity in Rtable
                        {
                            cmd.CommandText = "UPDATE Rtable SET Qty = @newQty WHERE PartNum = @partNum && BatchID = @batchID";
                            int newQty = Int32.Parse(currentQty) - Int32.Parse(qtyCheckedOut);
                            cmd.Parameters.AddWithValue("@newQty", newQty.ToString());

                        }
                        cmd.Parameters.AddWithValue("@partNum", PartNum.Text);
                        cmd.Parameters.AddWithValue("batchID", batchID);
                        cmd.ExecuteNonQuery();
                    }

                    // Insert into Htable
                    // System.Threading.Thread.Sleep(5000);
                    cmd.Parameters.Clear();
                    cmd.CommandText = "INSERT INTO Htable (UserName, Status, PartNum, BatchID, Supplier, Description, Qty, Area, Section, ModelNum, SerialNums, Remarks) VALUE " +
                        "(@userName, @status, @partNum, @batchID, @supplier, @description, @qty, @area, @section, @modelNum, @serialNums, @remarks)";
                    for (int i = 0; i < placeholders.Count; i++)
                    {
                        cmd.Parameters.AddWithValue(placeholders[i], inputs[i]);
                    }
                    cmd.Parameters.AddWithValue("@status", "Check out");
                    cmd.Parameters.AddWithValue("@serialNums", SerialNums.Text);
                    cmd.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
            
            ClearAll();
            PartNum.Text = String.Empty;
            Description.Text = String.Empty;
            Success.Text = "Item checked out successfully!";

            // Get new inventory 
            DataSource dataSource = new DataSource();
            products = dataSource.products;
            items = dataSource.items;
        }

        private void clearAll_Click(object sender, RoutedEventArgs e)
        {
            ClearAll();
            PartNum.Text = String.Empty;
            Description.Text = String.Empty;
            PartNumCheckbox.IsChecked = false;
            DescCheckbox.IsChecked = false;
        }


    }

    public class SerialNumber
    {
        public string SerialNum { get; set; }
        public Boolean CheckStatus { get; set; }
    }
}
