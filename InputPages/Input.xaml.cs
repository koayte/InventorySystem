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

namespace InventorySystem
{
    /// <summary>
    /// Interaction logic for Input.xaml
    /// </summary>
    public partial class Input : Page
    {
        public string connectionString = "SERVER=localhost; DATABASE=inventory; UID=semi; PASSWORD=semitech;";

        public Input()
        {
            InitializeComponent();

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
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    string commandText = "SELECT Description, Location FROM inputs WHERE PartNum = @PartNum";
                    MySqlCommand autoFillDescLoc = new MySqlCommand(commandText, connection);
                    autoFillDescLoc.Parameters.AddWithValue("@PartNum", partNum);

                    connection.Open();
                    MySqlDataReader reader = autoFillDescLoc.ExecuteReader();
                    if (reader.Read())
                    {
                        string desc = reader.GetString(0);
                        string location = reader.GetString(1);
                        Description.Text = desc;
                        Location.Text = location;
                    }
                    connection.Close();
                }
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ModelNum.IsReadOnly = false;
            ModelNumber.Foreground = Brushes.Black;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ModelNum.IsReadOnly = true;
            if (!string.IsNullOrEmpty(ModelNum.Text))
            {
                ModelNum.Text = string.Empty;
            }

            ModelNumber.Foreground = Brushes.Gray;
        }
    }
}
