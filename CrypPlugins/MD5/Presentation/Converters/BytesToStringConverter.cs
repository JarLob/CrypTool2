﻿using System;
using System.Windows.Data;

namespace Cryptool.MD5.Presentation.Converters
{
    class BytesToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            byte[] byteArray = (byte[])value;

            if (byteArray == null)
                return string.Empty;

            return BitConverter.ToString(byteArray).Replace('-', ' ');
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
