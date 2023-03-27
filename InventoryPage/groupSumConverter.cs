using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
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
    public class GroupSumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is ReadOnlyObservableCollection<Object>))
            {
                Debug.WriteLine("{0}", value.GetType().ToString());
                return "0";
            }
            ReadOnlyObservableCollection<Object> items = (ReadOnlyObservableCollection<Object>)value;
            int sum = 0;
            foreach (Object o in items)
            {
                sum += System.Convert.ToInt32(o.GetType().GetProperty("Qty").GetValue(o));
            }
            return sum.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
