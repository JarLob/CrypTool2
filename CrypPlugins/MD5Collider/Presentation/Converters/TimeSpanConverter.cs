﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Cryptool.Plugins.MD5Collider.Presentation.Converters
{
    [ValueConversion(typeof(TimeSpan), typeof(string))]
    class TimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
        object parameter, System.Globalization.CultureInfo culture)
        {
            TimeSpan timeSpan = (TimeSpan)value;
            String result = String.Empty;
            if (timeSpan != null)
            {
                result = String.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);           
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType,
        object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

    }
}
