using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventorySystem.InventoryPage
{

    public class Item : INotifyPropertyChanged, IEditableObject
    {

        // Private item data. 
        private string _UserName = string.Empty;
        private string _PartNum = string.Empty;
        private string _Supplier = string.Empty;
        private string _BatchID = string.Empty;
        private string _Description = string.Empty;
        private string _Qty = string.Empty;
        private string _Area = string.Empty;
        private string _Section = string.Empty;
        private string _ModelNum = string.Empty;
        private string _SerialNums = string.Empty;
        private string _Remarks = string.Empty;

        // Data for undoing canceled edits.
        private Item temp_Item = null;
        private bool _Editing = false;

        // Public item data.
        public string UserName
        {
            get { return this._UserName; }
            set
            {
                if (value != this._UserName)
                {
                    this._UserName = value;
                    NotifyPropertyChanged("UserName");
                }
            }
        }

        public string PartNum
        {
            get { return this._PartNum; }
            set
            {
                if (value != this._PartNum)
                {
                    this._PartNum = value;
                    NotifyPropertyChanged("PartNum");
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
                    NotifyPropertyChanged("Supplier");
                }
            }
        }

        public string BatchID
        {
            get { return this._BatchID; }
            set
            {
                if (value != this._BatchID)
                {
                    this._BatchID = value;
                    NotifyPropertyChanged("BatchID");
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
                    NotifyPropertyChanged("Description");
                }
            }
        }

        public string Qty
        {
            get { return this._Qty; }
            set
            {
                if (value != this._Qty)
                {
                    this._Qty = value;
                    NotifyPropertyChanged("Qty");
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
                    NotifyPropertyChanged("Area");
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
                    NotifyPropertyChanged("Section");
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
                    NotifyPropertyChanged("ModelNum");
                }
            }
        }

        public string SerialNums
        {
            get { return _SerialNums; }
            set
            {
                if (value != this._SerialNums)
                {
                    this._SerialNums = value;
                    NotifyPropertyChanged("SerialNums");
                }
            }
        }

        public string Remarks
        {
            get { return _Remarks; }
            set
            {
                if (value != this._Remarks)
                {
                    this._Remarks = value;
                    NotifyPropertyChanged("Remarks");
                }
            }
        }

        // Implement INotifyPropertyChanged interface.
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        // Implement IEditableObject interface.
        public void BeginEdit()
        {
            if (_Editing == false)
            {
                temp_Item = this.MemberwiseClone() as Item;
                _Editing = true;
            }
        }

        public void CancelEdit()
        {
            if (_Editing == true)
            {
                this.UserName = temp_Item.UserName;
                this.PartNum = temp_Item.PartNum;
                this.Supplier = temp_Item.Supplier;
                this.BatchID = temp_Item.BatchID;
                this.Description = temp_Item.Description;
                this.Qty = temp_Item.Qty;
                this.Area = temp_Item.Area;
                this.Section = temp_Item.Section;
                this.ModelNum = temp_Item.ModelNum;
                this.SerialNums = temp_Item.SerialNums;
                this.Remarks = temp_Item.Remarks;
                _Editing = false;
            }
        }

        public void EndEdit()
        {
            if (_Editing == true)
            {
                temp_Item = null;
                _Editing = false;
            }
        }

    }


}
