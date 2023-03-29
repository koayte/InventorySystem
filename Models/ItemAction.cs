using Org.BouncyCastle.Asn1.IsisMtt.X509;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventorySystem.InventoryPage
{
    public class ItemAction : INotifyPropertyChanged, IEditableObject
    {
        // Private item data. 
        private string _UserName = string.Empty;
        private string _Status = string.Empty;
        private string _Purpose = string.Empty;
        private string _PartNum = string.Empty;
        private string _BatchID = string.Empty;
        private string _Description = string.Empty;
        private string _Qty = string.Empty;
        private string _Area = string.Empty;
        private string _Section = string.Empty;
        private string _ModelNum = string.Empty;
        private string _SerialNums = string.Empty;
        private string _Time = string.Empty;


        // Data for undoing canceled edits.
        private ItemAction temp_ItemAction = null;
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


        public string Status
        {
            get { return this._Status; }
            set
            {
                if (value != this._Status)
                {
                    this._Status = value;
                    NotifyPropertyChanged("Status");
                }
            }
        }

        public string Purpose
        {
            get { return this._Purpose; }
            set
            {
                if (value != this._Purpose)
                {
                    this._Purpose = value;
                    NotifyPropertyChanged("Purpose");
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
            get { return this._SerialNums; }
            set
            {
                if (value != this._SerialNums)
                {
                    this._SerialNums = value;
                    NotifyPropertyChanged("SerialNums");
                }
            }
        }

        public string Time
        {
            get { return this._Time; }
            set
            {
                if (value != this._Time)
                {
                    this._Time = value;
                    NotifyPropertyChanged("Time");
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
                temp_ItemAction = this.MemberwiseClone() as ItemAction;
                _Editing = true;
            }
        }

        public void CancelEdit()
        {
            if (_Editing == true)
            {
                this.UserName = temp_ItemAction.UserName;
                this.Status = temp_ItemAction.Status;
                this.Purpose = temp_ItemAction.Purpose;
                this.PartNum = temp_ItemAction.PartNum;
                this.BatchID = temp_ItemAction.BatchID;
                this.Description = temp_ItemAction.Description;
                this.Qty = temp_ItemAction.Qty;
                this.Area = temp_ItemAction.Area;
                this.Section = temp_ItemAction.Section;
                this.ModelNum = temp_ItemAction.ModelNum;
                this.SerialNums = temp_ItemAction.SerialNums;
                this.Time = temp_ItemAction.Time;
                _Editing = false;
            }
        }

        public void EndEdit()
        {
            if (_Editing == true)
            {
                temp_ItemAction = null;
                _Editing = false;
            }
        }
    }
}
