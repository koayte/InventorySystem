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
        public static string BatchID { get; set; }
        public static string PartNum { get; set; }
        public static string Description { get; set; }
        public static string Location { get; set; }
        public static string? ModelNum { get; set; }

    }
}
