using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace InventorySystem.InputPages
{
    public class ValidationConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {

            bool filled = true;
            for (int i = 0; i < values.Length - 2; i++)
            {
                object val = values[i];
                if (string.IsNullOrEmpty(val as string))
                {
                    filled = false;
                }
            }

            for (int i = values.Length - 2; i < values.Length; i++)
            {
                object val = values[i];
                if (!string.IsNullOrEmpty(val as string))
                {
                    filled = false;
                }
            }
            return filled;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ExportValidationConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {

            bool filled = true;
            for (int i = 0; i < values.Length; i++)
            {
                object val = values[i];
                if (string.IsNullOrEmpty(val as string))
                {
                    filled = false;
                }
            }
            return filled;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
