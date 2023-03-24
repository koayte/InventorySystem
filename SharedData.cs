using InventorySystem.InventoryPage;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace InventorySystem
{
    public static class SharedData
    {
        // Set when update button in Inventory is clicked, get at the Update page. 
        public static string BatchID { get; set; }
        public static string PartNum { get; set; }
        public static string Description { get; set; }
        public static string Area { get; set; }
        public static string Section { get; set; }
        public static string? ModelNum { get; set; }

    }
}
