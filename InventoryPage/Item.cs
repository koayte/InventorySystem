using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventorySystem.InventoryPage
{
    public class Item
    {
        public string PartNum {  get; set; }
        public string BatchID { get; set; }
        public string Description { get; set; } 
        public string Qty { get; set; }
        public string Location { get; set; }
        public string? ModelNum { get; set; }
    }

    public class Location
    {
        public string Area { get; set; }
    }

}
