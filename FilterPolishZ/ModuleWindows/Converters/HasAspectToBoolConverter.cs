using FilterPolishZ.ModuleWindows.ItemVariationList;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FilterPolishZ.ModuleWindows.Converters
{
    public class HasAspectToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            
            if (ItemVariationListView.ItemVariationInformationStatic != null && ItemVariationListView.ItemVariationInformationStatic.Any(x => x.Name == value.ToString()))
            {
                return true;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
