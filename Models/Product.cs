using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventorySystem.InventoryPage
{
    public class Product
    {
        private string _PartNum = string.Empty;
        private string _Description = string.Empty;
        private string _ModelNum = string.Empty;
        private string _Area = string.Empty;
        private string _Section = string.Empty;
        private string _SerialNumsExist = string.Empty;
        private string _Supplier = string.Empty;

        public string PartNum
        {
            get { return this._PartNum; }
            set
            {
                if (value != this._PartNum)
                {
                    this._PartNum = value;
                }
            }
        }

        public string Description
        {
            get { return this._Description; }
            set
            {
                if (value != this._Description)
                {
                    this._Description = value;
                }
            }
        }

        public string ModelNum
        {
            get { return this._ModelNum; }
            set
            {
                if (value != this._ModelNum)
                {
                    this._ModelNum = value;
                }
            }
        }

        public string Area
        {
            get { return this._Area; }
            set
            {
                if (value != this._Area)
                {
                    this._Area = value;
                }
            }
        }

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

        public string SerialNumsExist
        {
            get { return this._SerialNumsExist; }
            set
            {
                if (value != this._SerialNumsExist)
                {
                    this._SerialNumsExist = value;
                }
            }
        }

        public string Supplier
        {
            get { return this._Supplier; }
            set
            {
                if (value != this._Supplier)
                {
                    this._Supplier = value;
                }
            }
        }
    }
}
