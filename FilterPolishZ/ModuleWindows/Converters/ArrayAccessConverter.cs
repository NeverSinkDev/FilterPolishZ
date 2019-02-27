using FilterPolishUtil.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FilterPolishZ.ModuleWindows.Converters
{
    public class ArrayAccessConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values == null || values.Length != 2)
                return null;

            Capsule obj = values[0] as Capsule;
            string idx = values[1] as string;

            if (idx == null || obj == null)
                return null;

            return obj[idx];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
