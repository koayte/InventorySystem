using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Quic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace InventorySystem.InventoryPage
{
    public class groupSumConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var items = value as IEnumerable<Item>;
            if (items == null)
            {
                return "0";
            }
            int sum = 0;
            foreach (var item in items)
            {
                int qty = Int32.Parse(((Item)item).Qty);
                sum += qty;
            }
            return sum.ToString();


            //var groups = CollectionViewSource.GetDefaultView(collection).Groups;
            //Debug.WriteLine($"Value is an ObservableCollection. Value type: {value?.GetType().Name}");
            //MessageBox.Show("help");
            //return "1";

            //var items = (ReadOnlyObservableCollection<Item>)value;
            //int sum = 0;
            //foreach (GroupItem gi in items)
            //{
            //    int qty = Int32.Parse(gi.Qty);
            //    sum += qty;
            //}
  

            



            //if (value is ObservableCollection<Item>)
            //{
            //    var groupItems = (ObservableCollection<Item>)value;
            //    int qtySum = 0;
            //    foreach (var gi in groupItems)
            //    {
            //        int qty = int.Parse(gi.Qty);

            //        qtySum += qty;
            //    }
            //    return qtySum.ToString();
            //}
            //return "AAA";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
