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
using System.Diagnostics;

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

        // ------------------------------------------------------------ Filter / Search 
        private bool GetDateFilter(DateTime itemActionDate)
        {
            if (StartDate.SelectedDate.HasValue)
            {
                DateTime startDate = StartDate.SelectedDate.Value;
                int resultStart = DateTime.Compare(startDate, itemActionDate);
                if (EndDate.SelectedDate.HasValue)
                {
                    DateTime endDate = EndDate.SelectedDate.Value;
                    int resultEnd = DateTime.Compare(itemActionDate, endDate);
                    if (resultStart <= 0 && resultEnd <= 0)
                    {
                        return true;
                    }
                    return false;
                }
                
                if (resultStart <= 0)
                {
                    return true;
                }
                return false;
            }
            return false;
        }
        
        private void cvs_Filter(object sender, FilterEventArgs e)
        {
            ItemAction itemAction = e.Item as ItemAction;
            if (itemAction != null)
            {
                
                bool UserFilter = string.IsNullOrEmpty(UserSearch.Text) || itemAction.UserName.Contains(UserSearch.Text);
                bool PartNumFilter = string.IsNullOrEmpty(PartNumSearch.Text) || itemAction.PartNum.Contains(PartNumSearch.Text);
                bool ActionFilter = string.IsNullOrEmpty(StatusSearch.Text) || itemAction.Status.Contains(StatusSearch.Text);
                bool BatchIDFilter = string.IsNullOrEmpty(BatchSearch.Text) || itemAction.BatchID.Contains(BatchSearch.Text);
                bool DescFilter = string.IsNullOrEmpty(DescSearch.Text) || itemAction.Description.ToLower().Contains(DescSearch.Text.ToLower());
                bool QtyFilter = string.IsNullOrEmpty(QtySearch.Text) || itemAction.Qty.Contains(QtySearch.Text);
                bool AreaFilter = string.IsNullOrEmpty(AreaSearch.Text) || itemAction.Area.Contains(AreaSearch.Text);
                bool SecFilter = string.IsNullOrEmpty(SecSearch.Text) || itemAction.Section.Contains(SecSearch.Text);
                bool ModelNumFilter = string.IsNullOrEmpty(ModelNumSearch.Text) || itemAction.ModelNum.Contains(ModelNumSearch.Text);
                bool SerialNumFilter = string.IsNullOrEmpty(SerialNumSearch.Text) || itemAction.SerialNums.Contains(SerialNumSearch.Text);
                bool DateFilter = !StartDate.SelectedDate.HasValue || !EndDate.SelectedDate.HasValue || GetDateFilter(DateTime.Parse(itemAction.Time)) == true;
                
                e.Accepted = UserFilter && PartNumFilter && ActionFilter && BatchIDFilter && DescFilter && QtyFilter && AreaFilter && SecFilter && ModelNumFilter && SerialNumFilter && DateFilter;
            }
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(historyGrid.ItemsSource).Refresh();
        }
        

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            StartDate.SelectedDate = null;
            EndDate.SelectedDate = null;
            UserSearch.Text = "";
            PartNumSearch.Text = "";
            StatusSearch.Text = "";
            BatchSearch.Text = "";
            DescSearch.Text = "";
            QtySearch.Text = "";
            AreaSearch.Text = "";
            SecSearch.Text = "";
            ModelNumSearch.Text = "";
            SerialNumSearch.Text = "";
        }

        // ------------------------------------------------------------ Export to .csv
        private void ExportAll_Click(object sender, RoutedEventArgs e)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string path = "C:/ProgramData/MySQL/MySQL Server 8.0/Uploads/history_" + timestamp + ".csv";
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            string commandText = "SELECT 'UserName', 'Action', 'PartNum', 'BatchID', 'Supplier', 'Description', 'Qty', 'Area', 'Section', 'ModelNum', 'SerialNums', 'Remarks', 'DateTime' " +
                    "UNION ALL " +
                    "SELECT UserName, Status, PartNum, BatchID, Supplier, Description, Qty, Area, Section, ModelNum, SerialNums, Remarks, DATE_FORMAT(Time, \"%Y-%m-%d %H:%i:%s\") AS Time " +
                    "FROM htable " +
                    "INTO OUTFILE @path " +
                    "FIELDS TERMINATED BY ',' " +
                    "ENCLOSED BY '\"' " +
                    "LINES TERMINATED BY '\r\n'";

            // Export 
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (MySqlCommand cmd = new MySqlCommand(commandText, connection))
                {
                    cmd.Parameters.AddWithValue("@path", path);
                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        MessageBox.Show("Export successful!");
                        try
                        {
                            Process.Start("explorer.exe", @"C:\ProgramData\MySQL\MySQL Server 8.0\Uploads");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                    }
                }
                connection.Close();
            }
        }
        
        
        private void ExportView_Click(object sender, RoutedEventArgs e)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string path = "C:/ProgramData/MySQL/MySQL Server 8.0/Uploads/history_" + timestamp + ".csv";
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            
            string commandText = "";
            if (StartDate.SelectedDate.HasValue || EndDate.SelectedDate.HasValue)
            {
                commandText = "SELECT 'UserName', 'Action', 'PartNum', 'BatchID', 'Supplier', 'Description', 'Qty', 'Area', 'Section', 'ModelNum', 'SerialNums', 'Remarks', 'DateTime' " +
                    "UNION ALL " +
                    "SELECT UserName, Status, PartNum, BatchID, Supplier, Description, Qty, Area, Section, ModelNum, SerialNums, Remarks, DATE_FORMAT(Time, \"%Y-%m-%d %H:%i:%s\") AS Time " +
                    "FROM htable WHERE (UserName = @userName OR @userName = \"\") " +
                    "AND (((Date BETWEEN @startDate AND @endDate) OR (@startDate = \"\")) " +
                    "OR ((Date BETWEEN @startDate AND @endDate) OR (@endDate = \"\"))) " +
                    "AND (PartNum = @partNum OR @partNum = \"\") " +
                    "AND (Status = @action OR @action = \"\") " +
                    "AND (BatchID = @batchID OR @batchID = \"\") " +
                    "AND (Description = @description OR @description = \"\") " +
                    "AND (Qty = @qty OR @qty = \"\") " +
                    "AND (Area = @area OR @area = \"\") " +
                    "AND (Section = @section OR @section = \"\") " +
                    "AND (ModelNum = @modelNum OR @modelNum = \"\") " +
                    "AND (SerialNums = @serialNums OR @serialNums = \"\") " +
                    "INTO OUTFILE @path " +
                    "FIELDS TERMINATED BY ',' " +
                    "ENCLOSED BY '\"' " +
                    "LINES TERMINATED BY '\r\n'";
            }

            else
            {
                commandText = "SELECT 'UserName', 'Action', 'PartNum', 'BatchID', 'Supplier', 'Description', 'Qty', 'Area', 'Section', 'ModelNum', 'SerialNums', 'Remarks', 'DateTime' " +
                    "UNION ALL " +
                    "SELECT UserName, Status, PartNum, BatchID, Supplier, Description, Qty, Area, Section, ModelNum, SerialNums, Remarks, DATE_FORMAT(Time, \"%Y-%m-%d %H:%i:%s\") AS Time " +
                    "FROM htable WHERE (UserName = @userName OR @userName = \"\") " +
                    "AND (PartNum = @partNum OR @partNum = \"\") " +
                    "AND (Status = @action OR @action = \"\") " +
                    "AND (BatchID = @batchID OR @batchID = \"\") " +
                    "AND (Description = @description OR @description = \"\") " +
                    "AND (Qty = @qty OR @qty = \"\") " +
                    "AND (Area = @area OR @area = \"\") " +
                    "AND (Section = @section OR @section = \"\") " +
                    "AND (ModelNum = @modelNum OR @modelNum = \"\") " +
                    "AND (SerialNums = @serialNums OR @serialNums = \"\") " +
                    "INTO OUTFILE @path " +
                    "FIELDS TERMINATED BY ',' " +
                    "ENCLOSED BY '\"' " +
                    "LINES TERMINATED BY '\r\n'";
            }
            
            
            string startDate = GetFormattedDate(StartDate);
            string endDate = GetFormattedDate(EndDate);
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (MySqlCommand cmd = new MySqlCommand(commandText, connection))
                {
                    cmd.Parameters.AddWithValue("@userName", UserSearch.Text);
                    cmd.Parameters.AddWithValue("@partNum", PartNumSearch.Text);
                    cmd.Parameters.AddWithValue("@action", StatusSearch.Text);
                    cmd.Parameters.AddWithValue("@batchID", BatchSearch.Text);
                    cmd.Parameters.AddWithValue("@description", DescSearch.Text);
                    cmd.Parameters.AddWithValue("@qty", QtySearch.Text);
                    cmd.Parameters.AddWithValue("@area", AreaSearch.Text);
                    cmd.Parameters.AddWithValue("@section", SecSearch.Text);
                    cmd.Parameters.AddWithValue("@modelNum", ModelNumSearch.Text);
                    cmd.Parameters.AddWithValue("@serialNums", SerialNumSearch.Text);
                    if (StartDate.SelectedDate.HasValue || EndDate.SelectedDate.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@startDate", startDate);
                        cmd.Parameters.AddWithValue("@endDate", endDate);
                    }
                    cmd.Parameters.AddWithValue("@path", path);
                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        MessageBox.Show("Export successful!");
                        try
                        {
                            Process.Start("explorer.exe", @"C:\ProgramData\MySQL\MySQL Server 8.0\Uploads");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
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
