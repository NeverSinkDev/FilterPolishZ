using FilterPolishZ.ModuleWindows.BaseTypeTiering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace FilterPolishZ.ModuleWindows.Converters
{
    public class BaseTypeToMatrixTierColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is TextBlock a)
            {
                var cell = (a.Parent as DataGridCell);
                var index = cell.Column.DisplayIndex;

                KeyBaseTypeRow data = cell.DataContext as KeyBaseTypeRow;

                if (data == null)
                {
                    return DependencyProperty.UnsetValue;
                }

                var item = GetItemName(index, data);

                if (item == null || item == string.Empty)
                {
                    return DependencyProperty.UnsetValue;
                }

                var result = BaseTypeTieringView.LookUpItem(item);

                if (result.Contains("t1"))
                {
                    return Brushes.DeepSkyBlue;
                }

                if (result.Contains("t2"))
                {
                    return Brushes.YellowGreen;
                }

                if (result.Contains("t3"))
                {
                    return Brushes.Orange;
                }

                return Brushes.Crimson;
            }


            return DependencyProperty.UnsetValue;

        }

        public static string GetItemName(int index, KeyBaseTypeRow data)
        {
            string item = "";
            switch (index)
            {
                case 0:
                    item = data.All?.Name;
                    break;
                case 1:
                    item = data.Boots?.Name;
                    break;
                case 2:
                    item = data.Gloves?.Name;
                    break;
                case 3:
                    item = data.Body?.Name;
                    break;
                case 4:
                    item = data.Shields?.Name;
                    break;
                case 5:
                    item = data.Helmets?.Name;
                    break;

                default:
                    break;
            }

            return item;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
