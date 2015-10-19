﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows.Data;

namespace KeySearcher.Converter
{
    [ValueConversion(typeof(BigInteger), typeof(string))]
    public class BigIntegerToStringConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var bigint = (BigInteger)value;
            return bigint.ToString("N0", new CultureInfo("de-DE"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
