using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventorySystem.InventoryPage
{
    public class Location
    {
		private string _Area = string.Empty;
		public string Area
		{
			get { return this._Area ; }
            set
            {
                if (value != this._Area)
                {
                    this._Area = value;
                }
            }
        }

        private string _Section = string.Empty;
        public string Section
        {
            get { return this._Section; }
            set
            {
                if (value != this._Section)
                {
                    this._Section = value;
                }
            }
        }

    }
}
