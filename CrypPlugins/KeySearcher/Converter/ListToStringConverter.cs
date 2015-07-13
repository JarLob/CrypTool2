using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows.Data;

namespace KeySearcher.Converter
{
    [ValueConversion(typeof(ObservableCollection<BigInteger>), typeof(string))]
    public class ListToStringConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var list = (ObservableCollection<BigInteger>)value;

            if (list.Count == 0)
            {
                return "-";
            }

            var convert = String.Join(", ", list.ToArray()); 
            return convert;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
