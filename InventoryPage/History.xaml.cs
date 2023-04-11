using System;
using System.IO;
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
using MySql.Data.MySqlClient;

namespace InventorySystem.InventoryPage
{
    /// <summary>
    /// Interaction logic for History.xaml
    /// </summary>
    public partial class History : Page
    {
        public string connectionString = "SERVER=localhost; DATABASE=inventory; UID=semi; PASSWORD=semitech;";
        public History()
        {
            InitializeComponent();
        }

        // ------------------------------------------------------------ Export to .txt
        private void Export_Click(object sender, RoutedEventArgs e)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
            string path = "C:/ProgramData/MySQL/MySQL Server 8.0/Uploads/htable_" + timestamp + ".csv";
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            Export();
        }

        private void Export()
        {
            string startDate = GetFormattedDate(StartDate);
            string endDate = GetFormattedDate(EndDate);

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string commandText = "SELECT 'UserName', 'Status', 'PartNum', 'BatchID', 'Supplier', 'Description', 'Qty', 'Area', 'Section', 'ModelNum', 'SerialNums', 'Remarks', 'Time', 'Date' " +
                    "UNION ALL " +
                    "SELECT UserName, Status, PartNum, BatchID, Supplier, Description, Qty, Area, Section, ModelNum, SerialNums, Remarks, DATE_FORMAT(Time, \"%Y-%m-%d %H:%i:%s\") AS Time, DATE_FORMAT(Date, \"%Y-%m-%d\") AS Date " +
                    "FROM htable WHERE Date BETWEEN @startDate AND @endDate " +
                    "INTO OUTFILE @path " +
                    "FIELDS TERMINATED BY ',' " +
                    "ENCLOSED BY '\"' " +
                    "LINES TERMINATED BY '\r\n'";
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
                string path = "C:/ProgramData/MySQL/MySQL Server 8.0/Uploads/htable_" + timestamp + ".csv";
                connection.Open();
                using (MySqlCommand cmd = new MySqlCommand(commandText, connection))
                {
                    cmd.Parameters.AddWithValue("@startDate", startDate);
                    cmd.Parameters.AddWithValue("@endDate", endDate);
                    cmd.Parameters.AddWithValue("@path", path);
                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        MessageBox.Show("Export successful!");
                    }
                }
                connection.Close();
            }
        }

        private string GetFormattedDate(DatePicker datePicker)
        {
            string formattedDate = "";
            if (!string.IsNullOrEmpty(datePicker.Text))
            {
                DateTime dateChosen = DateTime.Parse(datePicker.Text);
                formattedDate = dateChosen.ToString("yyyy-MM-dd");
                return formattedDate;
            }
            return formattedDate;
        }
    }

    
}
