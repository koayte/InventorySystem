using MySql.Data.MySqlClient;
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

namespace InventorySystem
{
    /// <summary>
    /// Interaction logic for Input.xaml
    /// </summary>
    public partial class Input : Page
    {
        public Input()
        {
            InitializeComponent();

            string connectionString = "SERVER=localhost; DATABASE=inventory_db; UID=semi; PASSWORD=semitech;";

            MySqlConnection connection = new MySqlConnection(connectionString);

            MySqlCommand cmd = new MySqlCommand("select ", connection);
        }

        //private void PartNum_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key== Key.Enter)
        //    {
        //        string PartNum = PartNumInputOG.Text;
                
        //        // Submit request to SQL database
        //    }
        //}

        private void PartNumInputOG_TextChanged(object sender, TextChangedEventArgs e)
        {
            string PartNum = PartNumInputOG.Text;
            if (!string.IsNullOrEmpty(PartNum) )
            {
                // query database, check if exists 
                // if exists, bring to ItemExists page 
                // if not, bring to DoesNotExist page 
            }
        }
    }
}
