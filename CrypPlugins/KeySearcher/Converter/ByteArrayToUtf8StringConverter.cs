using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace KeySearcher.Converter
{
    [ValueConversion(typeof(byte[]), typeof(string))]
    public class ByteArrayToUtf8StringConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var bytes = (byte[])value;
            return Regex.Replace(Encoding.UTF8.GetString(bytes), @"\r\n?|\n", ""); 
        }
         
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
