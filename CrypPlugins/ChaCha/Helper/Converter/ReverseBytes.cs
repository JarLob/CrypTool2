using System;
using System.Globalization;
using System.Windows.Data;

namespace Cryptool.Plugins.ChaCha.Helper.Converter
{
    internal class ReverseBytes : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            return Formatter.ReverseBytes((byte[])value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}