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
        public ObservableCollection<Item> items { get; set; }
        public DataSource()
        {
            items = new ObservableCollection<Item>();
            LoadData();
        }

        private void LoadData()
        {
            using (MySqlConnection connection = new MySqlConnection("SERVER=localhost; DATABASE=inventory; UID=semi; PASSWORD=semitech;"))
            {
                string commandText = "SELECT PartNum, BatchID, Description, CAST(Qty AS CHAR) AS Qty, Location, ModelNum, SerialNums FROM inputs ORDER BY PartNum, BatchID";
                MySqlCommand loadInventory = new MySqlCommand(commandText, connection);
                connection.Open();
                MySqlDataReader reader = loadInventory.ExecuteReader();

                while (reader.Read())
                {
                    string partNum = reader.GetString("PartNum");
                    string batchID = reader.GetString("BatchID");
                    string description = reader.GetString("Description");
                    string qty = reader.GetString("Qty");
                    string location = reader.GetString("Location");
                    string modelNum = reader.GetString("ModelNum");
                    string serialNum = reader.GetString("SerialNums");
                    items.Add(new Item()
                    {
                        PartNum = partNum,
                        BatchID = batchID,
                        Description = description,
                        Qty = qty,
                        Location = location,
                        ModelNum = modelNum,
                        SerialNums = serialNum
                    });

                }
            }
        }
    }
}
