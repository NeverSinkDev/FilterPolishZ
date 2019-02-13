using System;
using System.Globalization;
using System.Windows.Data;

namespace FilterPolishZ.ModuleWindows.Converters
{
    public class IsGreaterThanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(bool))
            {
                throw new ArgumentException("Target must be a boolean");
            }

            if ((value == null) || (parameter == null))
            {
                return false;
            }

            double convertedValue;
            if (!double.TryParse(value.ToString(), out convertedValue))
            {
                throw new InvalidOperationException("Unconvertable");
            }

            double convertedParameter;
            if (!double.TryParse(parameter.ToString(), out convertedParameter))
            {
                throw new InvalidOperationException("Unconvertable");
            }

            return convertedValue > convertedParameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new NotSupportedException();
        }
    }
}