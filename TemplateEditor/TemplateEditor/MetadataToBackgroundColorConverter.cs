﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace TemplateEditor
{
    [ValueConversion(typeof(bool), typeof(Brush))]
    public class MetadataToBackgroundConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
                              CultureInfo culture)
        {
            if (value is TemplateInfo)
            {
                var tempInfo = (TemplateInfo) value;
                if (!tempInfo.HasMetadata || !tempInfo.LocalizedTemplateData.ContainsKey("en"))
                {
                    return Brushes.Red;
                }

                bool hasEngKeywords = true;
                foreach (var localizedTemp in tempInfo.LocalizedTemplateData.Values)
                {
                    if (localizedTemp.Lang == null || localizedTemp.Title == null || localizedTemp.Description == null)
                    {
                        return Brushes.Yellow;
                    }
                    if ((localizedTemp.Lang == "en") && (localizedTemp.Keywords == null))
                    {
                        hasEngKeywords = false;
                    }
                }

                if (hasEngKeywords && (tempInfo.RelevantPlugins != null))
                {
                    return Brushes.Green;
                }
                return Brushes.GreenYellow;
            }
            else
            {
                return Brushes.Black;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}