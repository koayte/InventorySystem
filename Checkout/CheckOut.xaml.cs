using System;
using System.Collections.Generic;
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
using InventorySystem.InventoryPage;

namespace InventorySystem.Checkout
{
    /// <summary>
    /// Interaction logic for CheckOut.xaml
    /// </summary>
    public partial class CheckOut : Page
    {
        private List<TextBox> inputBoxes;

        public CheckOut()
        {
            InitializeComponent();
            inputBoxes = new List<TextBox> { BatchID, ModelNum, Description, Area, Section, Qty };
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
        private void Model_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
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
        
        // ------------------------------------------------- PartNum Combobox

        private void PartNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string partNumSelected = (sender as ComboBox).SelectedItem?.ToString() ?? "";
            List<string> serialNumList = new List<string>();
            ClearAll();
            if (!string.IsNullOrEmpty(partNumSelected))
            {
                DataSource dataSource = new DataSource();
                serialNumList = dataSource.items.Where(x => (x.PartNum == partNumSelected) && (x.SerialNums != "")).Select(x => x.SerialNums).ToList();
                if (serialNumList.Count > 0)
                {
                    SerialNumsCheckbox.IsChecked = true;
                    // Bind SerialNum combobox to serialNumList. 
                    SerialNums.ItemsSource = serialNumList;
                }
                else // If there are no SerialNums for this PartNum
                {
                    SerialNumsCheckbox.IsChecked = false;
                    SerialNums.ItemsSource = null;
                    var selectedItem = dataSource.items.Single(x => x.PartNum == partNumSelected);
                    BatchID.Text = selectedItem.BatchID.ToString();
                    Description.Text = selectedItem.Description.ToString();
                    Area.Text = selectedItem.Area.ToString();
                    Section.Text = selectedItem.Section.ToString();
                    Qty.Text = "1";
                    if (!string.IsNullOrEmpty(selectedItem.ModelNum.ToString()))
                    {
                        ModelNumCheckbox.IsChecked = true;
                        ModelNum.Text = selectedItem.ModelNum.ToString();
                    }
                }
            }
            else 
            {
                SerialNumsCheckbox.IsChecked = false;
                SerialNums.ItemsSource = null;
            }
        }

        private void SerialNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string serialNumSelected = (sender as ComboBox).SelectedItem?.ToString() ?? "";
            if (!string.IsNullOrEmpty(serialNumSelected))
            {
                DataSource dataSource = new DataSource();
                var selectedItem = dataSource.items.Single(x => (x.SerialNums == serialNumSelected) && (x.PartNum == PartNum.Text));
                if (selectedItem != null)
                {
                    BatchID.Text = selectedItem.BatchID.ToString();
                    Description.Text = selectedItem.Description.ToString();
                    Area.Text = selectedItem.Area.ToString();
                    Section.Text = selectedItem.Section.ToString();
                    Qty.Text = "1";
                    if (!string.IsNullOrEmpty(selectedItem.ModelNum.ToString()))
                    {
                        ModelNumCheckbox.IsChecked = true;
                        ModelNum.Text = selectedItem.ModelNum.ToString();
                    }
                }
            }
        }

        private void ClearAll()
        {
            // Clear textboxes, reset checkboxes to default
            for (int i = 0; i < inputBoxes.Count; i++)
            {
                inputBoxes[i].Text = String.Empty;
            }
            SerialNums.Text = String.Empty;
            ModelNumCheckbox.IsChecked = false;
            SerialNumsCheckbox.IsChecked = false;

            // Send caret position back to PartNum textbox.
            PartNum.Focus();
        }
    }
}
