using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventorySystem.InventoryPage
{
    public class DataSource
    {
        private string connectionString = "SERVER=localhost; DATABASE=inventory; UID=semi; PASSWORD=semitech;";
        public ObservableCollection<Item> items { get; set; }
        public ObservableCollection<string> areas { get; set; }
        public ObservableCollection<string> sections { get; set; }
        public ObservableCollection<User> users { get; set; }
        public ObservableCollection<ItemAction> itemActions { get; set; }
        public List<string> partNums { get; set; }
        public List<string> sources { get; set; }
        public ObservableCollection<Product> products { get; set; }

        public DataSource()
        {
            items = new ObservableCollection<Item>();
            partNums = new List<string>(); // unique list of part numbers 
            LoadInventoryData();
            areas = new ObservableCollection<string>();
            sections = new ObservableCollection<string>();
            LoadLocationData();
            users = new ObservableCollection<User>();
            LoadUserData();
            itemActions = new ObservableCollection<ItemAction>();
            LoadItemActionData();
            sources = new List<string>();
            LoadSourceData();
            products = new ObservableCollection<Product>();
            LoadProductData();
        }

        private void LoadInventoryData()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string commandText = "SELECT UserName, PartNum, BatchID, Supplier, Description, Qty, Area, Section, ModelNum, SerialNums, Remarks FROM Rtable ORDER BY PartNum, BatchID";
                MySqlCommand loadInventory = new MySqlCommand(commandText, connection);
                connection.Open();
                MySqlDataReader reader = loadInventory.ExecuteReader();

                while (reader.Read())
                {
                    string userName = reader.GetString("UserName");
                    string partNum = reader.GetString("PartNum");
                    string supplier = reader.GetString("Supplier");
                    string batchID = reader.GetString("BatchID");
                    string description = reader.GetString("Description");
                    string qty = reader.GetString("Qty");
                    string area = reader.GetString("Area");
                    string section = reader.GetString("Section");
                    string modelNum = reader.GetString("ModelNum");
                    string serialNum = reader.GetString("SerialNums");
                    string remarks = reader.GetString("Remarks");
                    items.Add(new Item()
                    {
                        UserName = userName,
                        PartNum = partNum,
                        Supplier = supplier,
                        BatchID = batchID,
                        Description = description,
                        Qty = qty,
                        Area = area,
                        Section = section,
                        ModelNum = modelNum,
                        SerialNums = serialNum,
                        Remarks = remarks
                    });

                    if (!partNums.Contains(partNum))
                    {
                        partNums.Add(partNum);
                    }
                }
            }
        }

        private void LoadLocationData()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand getArea = new MySqlCommand("SELECT Area FROM locations GROUP BY Area", connection);
                connection.Open();
                using (var reader = getArea.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string area = reader.GetString("Area");
                        areas.Add(area);
                    }
                }
                connection.Close();
            }

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand getSection = new MySqlCommand("SELECT Section FROM locations GROUP BY Section", connection);
                connection.Open();
                using (var reader = getSection.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string section = reader.GetString("Section");
                        sections.Add(section);
                    }
                }
                connection.Close();
            }
        }

        private void LoadUserData()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand getUser = new MySqlCommand("SELECT * FROM users", connection);
                connection.Open();
                using (var reader = getUser.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string name = reader.GetString("Name");
                        users.Add(new User()
                        {
                            Name = name
                        });
                    }
                }
                connection.Close();
            }
        }
        private void LoadItemActionData() // basically Htable
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string commandText = "SELECT UserName, Status, PartNum, Supplier, BatchID, Description, Qty, Area, Section, ModelNum, SerialNums, Remarks, Time FROM Htable ORDER BY Time DESC";
                MySqlCommand loadInventory = new MySqlCommand(commandText, connection);
                connection.Open();
                MySqlDataReader reader = loadInventory.ExecuteReader();

                while (reader.Read())
                {
                    string userName = reader.GetString("UserName");
                    string status = reader.GetString("Status");
                    string partNum = reader.GetString("PartNum");
                    string supplier = reader.GetString("Supplier");
                    string batchID = reader.GetString("BatchID");
                    string description = reader.GetString("Description");
                    string qty = reader.GetString("Qty");
                    string area = reader.GetString("Area");
                    string section = reader.GetString("Section");
                    string modelNum = reader.GetString("ModelNum");
                    string serialNum = reader.GetString("SerialNums");
                    string remarks = reader.GetString("Remarks");
                    string time = reader.GetString("Time");
                    itemActions.Add(new ItemAction()
                    {
                        UserName = userName,
                        Status = status,
                        PartNum = partNum,
                        Supplier = supplier,
                        BatchID = batchID,
                        Description = description,
                        Qty = qty,
                        Area = area,
                        Section = section,
                        ModelNum = modelNum,
                        SerialNums = serialNum,
                        Remarks = remarks,
                        Time = time
                    });
                }
            }
        }

        private void LoadSourceData() // Supplier
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string commandText = "SELECT Supplier FROM suppliers";
                MySqlCommand cmd = new MySqlCommand(commandText, connection);
                connection.Open();
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    sources.Add(reader.GetString("0"));
                }
                connection.Close();
            }
        }

        private void LoadProductData() // Unique product 
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string commandText = "SELECT * FROM ptable";
                MySqlCommand cmd = new MySqlCommand(commandText, connection);
                connection.Open();
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string partNum = reader.GetString("PartNum");
                    string modelNum = reader.GetString("ModelNum");
                    string description = reader.GetString("Description");
                    string area = reader.GetString("Area");
                    string section = reader.GetString("Section");
                    string serialNumsExist = reader.GetString("SerialNumsExist");
                    products.Add(new Product()
                    {
                        PartNum = partNum,
                        ModelNum = modelNum,
                        Description = description,
                        Area = area,
                        Section = section,
                        SerialNumsExist = serialNumsExist
                    });
                }
                connection.Close();
            }
        }
    }
}
