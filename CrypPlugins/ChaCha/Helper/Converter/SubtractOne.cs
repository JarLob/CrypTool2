using System;
using System.Globalization;
using System.Windows.Data;

namespace Cryptool.Plugins.ChaCha.Helper.Converter
{
    internal class SubtractOne : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value - 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}