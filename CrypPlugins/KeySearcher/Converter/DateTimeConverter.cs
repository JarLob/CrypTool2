using System;
using System.Globalization;
using System.Threading;
using System.Windows.Data;

namespace KeySearcherConverter
{
    public class DateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return "-";
            }
            //Hacked: we had an error here. Please fix me :-)
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
