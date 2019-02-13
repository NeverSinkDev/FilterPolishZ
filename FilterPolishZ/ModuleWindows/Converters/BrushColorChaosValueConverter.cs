using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace FilterPolishZ.ModuleWindows.Converters
{
    public class BrushColorChaosValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (float.Parse(value.ToString()) > 100)
            {
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF0000"));
                }
            }
            else if (float.Parse(value.ToString()) > 25)
            {
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF4500"));
                }
            }
            else if (float.Parse(value.ToString()) > 10)
            {
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFD700"));
                }
            }
            else if (float.Parse(value.ToString()) > 5)
            {
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7CFC00"));
                }
            }
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
