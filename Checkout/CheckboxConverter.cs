using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace InventorySystem.Checkout
{
    public class CheckboxConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool checkedOpp = true;
            if (value != null)
            {
                bool checkedOtherSearch = (bool)value;
                if (checkedOtherSearch == true)
                {
                    checkedOpp = false;
                }
            }
            return checkedOpp;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
