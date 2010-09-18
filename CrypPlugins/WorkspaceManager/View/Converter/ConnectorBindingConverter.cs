﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Globalization;
using WorkspaceManager.View.Container;

namespace WorkspaceManager.View.Converter
{
    public class ConnectorBindingConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            ConnectorView connector = (ConnectorView)parameter;
            double X = System.Convert.ToDouble(values[0]);
            double Y = System.Convert.ToDouble(values[1]);
            double Height = System.Convert.ToDouble(values[2]);
            double Width = System.Convert.ToDouble(values[3]);

            switch (connector.Orientation)
            {
                case ConnectorOrientation.West:
                    return new Point(X-3 , Y + Height / 2);
                case ConnectorOrientation.East:
                    return new Point(X + Width +3, Y + Height / 2);
                case ConnectorOrientation.North:
                    return new Point(X + Width / 2, Y-3);
                case ConnectorOrientation.South:
                    return new Point(X + Width / 2, Y + Height+3);
            }

            return new Point(0, 0);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
