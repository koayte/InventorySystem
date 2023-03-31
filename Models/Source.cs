using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventorySystem.InventoryPage
{
    public class Source
    {
		private string _Supplier;

		public string Supplier 
		{
			get { return this._Supplier; }
			set
			{
				if (this._Supplier != value)
				{
					this._Supplier = value;
				}
			}
		}

	}
}
