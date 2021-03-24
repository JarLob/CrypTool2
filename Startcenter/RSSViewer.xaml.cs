﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;

namespace Startcenter
{
    /// <summary>
    /// Interaction logic for RSSViewer.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("Startcenter.Properties.Resources")]
    public partial class RSSViewer : UserControl
    {
        private List<RssItem> _rssItems;
        //exchanged svn logs with our YouTube channel
        private const string RSSUrl = "https://www.youtube.com/feeds/videos.xml?channel_id=UC8_FqvQWJfZYxcSoEJ5ob-Q";

        public static readonly DependencyProperty IsUpdatingProperty =
            DependencyProperty.Register("IsUpdating",
                                        typeof(Boolean),
                                        typeof(RSSViewer), new PropertyMetadata(false));

        public Boolean IsUpdating
        {
            get { return (Boolean)GetValue(IsUpdatingProperty); }
            set { SetValue(IsUpdatingProperty, value); }
        }

        public RSSViewer()
        {
            InitializeComponent();
            IsUpdating = true;
            var updateTimer = new Timer(ReadAndFillRSSItems);
            updateTimer.Change(0, 1000*60);            
        }

        private void ReadAndFillRSSItems(object state)
        {
            try
            {
                _rssItems = ReadRSSItems(RSSUrl);
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    try
                    {
                        IsUpdating = false;
                        rssListBox.DataContext = _rssItems;
                    }
                    catch (Exception)
                    {
                        //Uncritical failure: Do nothing
                    }
                }, null);
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback) delegate
                {
                    IsUpdating = false;
                    var errorRSSFeed = new List<RssItem>(1);
                    errorRSSFeed.Add(new RssItem() {Message = Properties.Resources.RSS_error_Message, Title = Properties.Resources.RSS_error_Message});
                    errorRSSFeed.Add(new RssItem() {Message = ex.Message, Title = Properties.Resources.Exception});
                    rssListBox.DataContext = errorRSSFeed;
                }, null);
            }
        }

        private List<RssItem> ReadRSSItems(string rssFeedURL)
        {
            try
            {
                var req = (HttpWebRequest)WebRequest.Create(rssFeedURL);
                req.Method = "GET";
                req.UserAgent = "CrypTool 2/Startcenter RSS Viewer";
                var rep = req.GetResponse();
                var reader = XmlReader.Create(rep.GetResponseStream());
                var doc = XDocument.Load(reader);
                XNamespace ns = "http://www.w3.org/2005/Atom";
                XNamespace media_ns = "http://search.yahoo.com/mrss/";
                var entries = doc.Root.Elements(ns + "entry");
                var items = from entry in entries
                    select new RssItem()
                    {
                        Title = entry.Element(ns + "title").Value.Trim(),
                        Message = entry.Element(media_ns + "group").Element(media_ns + "description").Value.Trim(),
                        PublishingDate = DateTime.Parse(entry.Element(ns + "published").Value.Trim()),
                        URL = entry.Element(ns + "link").Attribute("href").Value.Trim()
                    };
                return items.ToList();
            }
            catch (Exception)
            {
                return new List<RssItem>();
            }
        }

        private void RSSItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start((string) ((FrameworkElement)sender).Tag);
        }  
    }

    class RssItem
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime PublishingDate { get; set; }
        public string URL { get; set; }
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class FalseToVisibleConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
                              CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
                throw new InvalidOperationException("The target must be of Visibility");

            if ((bool)value)
            {
                return Visibility.Hidden;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class TrueToVisibleConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
                              CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
                throw new InvalidOperationException("The target must be of Visibility");

            if ((bool)value)
            {
                return Visibility.Visible;
            }

            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
