using InventorySystem.InventoryPage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace InventorySystem
{

    public class Data
    {
        public List<Version> versions { get; set; }

        public Data()
        {
            versions = new List<Version>();
            AddVersionItem();
        }

        public void AddVersionItem()
        {
            Version version = new Version();
            version.VersionNumber = "1.0";
            version.Date = "10 March 2023";
            version.Remarks = "First draft";
            versions.Add(version);

            version = new Version();
            version.VersionNumber = "1.1";
            version.Date = "17 March 2023";
            version.Remarks = "Shifted to db; UI changes";
            versions.Add(version);

            version = new Version();
            version.VersionNumber = "1.2";
            version.Date = "24 March 2023";
            version.Remarks = "User validation; Update function";
            versions.Add(version);

            version = new Version();
            version.VersionNumber = "1.3";
            version.Date = "27 March 2023";
            version.Remarks = "Inventory grouping; History table; Bug fixes";
            versions.Add(version);

            version = new Version();
            version.VersionNumber = "1.4";
            version.Date = "31 March 2023";
            version.Remarks = "Check-out function";
            versions.Add(version);

            version = new Version();
            version.VersionNumber = "1.5";
            version.Date = "10 April 2023";
            version.Remarks = "Export function; Product table";
            versions.Add(version);
        }
    }
    
    public class Version
    {
        public string VersionNumber { get; set; }
        public string Date { get; set; }
        public string Remarks { get; set; }
        
    }
}
