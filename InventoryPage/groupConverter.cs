using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace InventorySystem.InventoryPage
{
    public class groupConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var group = value as CollectionViewGroup;
            if (group == null)
            {
                return "0";
            }

            int sum = 0;
            var items = group.Items;

            foreach (var item in items)
            {
                int qty = int.Parse(((Item)item).Qty);
                sum += qty;
            }

            return sum.ToString();

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
            return value;
        }
    }
}
