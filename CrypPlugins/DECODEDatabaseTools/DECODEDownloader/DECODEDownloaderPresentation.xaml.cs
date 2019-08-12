/*
   Copyright 2019 Nils Kopal <Nils.Kopal<at>CrypTool.org

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Cryptool.Plugins.DECODEDatabaseTools.Util;

namespace Cryptool.Plugins.DECODEDatabaseTools
{
    [PluginBase.Attributes.Localization("Cryptool.Plugins.DECODEDatabaseTools.Properties.Resources")]
    public partial class DECODEDownloaderPresentation : UserControl
    {
        private DECODEDownloader Plugin;        
        public ObservableCollection<RecordsRecord> RecordsList = new ObservableCollection<RecordsRecord>();        
        private GridViewColumnHeader _lastHeaderClicked;
        private ListSortDirection _lastDirection;

        public DECODEDownloaderPresentation(DECODEDownloader plugin)
        {
            InitializeComponent();            
            Plugin = plugin;
            this.ListView.ItemsSource = RecordsList;
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ListView.ItemsSource);
            view.Filter = UserFilter;

        }

        /// <summary>
        /// Filter name using the entered text
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool UserFilter(object item)
        {
            if (String.IsNullOrEmpty(Filter.Text))
            {
                return true;
            }
            else
            {
                return ((item as RecordsRecord).name.IndexOf(Filter.Text, StringComparison.OrdinalIgnoreCase) >= 0);
            }
        }

        /// <summary>
        /// User double-clicked for downloading a record
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void HandleDoubleClick(Object sender, EventArgs eventArgs)
        {
            try
            {
                var lvi = sender as ListViewItem;
                if (lvi != null)
                {
                    var record = lvi.Content as RecordsRecord;
                    if (record != null)
                    {
                        Plugin.Download(record);
                    }
                }
            }
            catch (Exception ex)
            {
                //wtf?
            }
        }

        /// <summary>
        /// Text of filter textfield changed; thus, update the ListView
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Filter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(ListView.ItemsSource).Refresh();
        }

        /// <summary>
        /// Sets the content of the LoginLabel
        /// </summary>
        /// <param name="text"></param>
        public void SetLoginNameLabel(string text)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                LoginNameLabel.Content = text;
            }, null);
        }
        
        /// <summary>
        /// Sorts the list view based on the clicked column header
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            var headerClicked = e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;

            if (headerClicked != null)
            {
                if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                {
                    if (headerClicked != _lastHeaderClicked)
                    {
                        direction = ListSortDirection.Ascending;
                    }
                    else
                    {
                        if (_lastDirection == ListSortDirection.Ascending)
                        {
                            direction = ListSortDirection.Descending;
                        }
                        else
                        {
                            direction = ListSortDirection.Ascending;
                        }
                    }

                    var columnBinding = headerClicked.Column.DisplayMemberBinding as Binding;
                    var sortBy = (columnBinding != null ? (columnBinding.Path.Path != null ? columnBinding.Path.Path : headerClicked.Column.Header) : null)  as string;                    

                    Sort(sortBy, direction);

                    if (direction == ListSortDirection.Ascending)
                    {
                        headerClicked.Column.HeaderTemplate =
                          Resources["HeaderTemplateArrowUp"] as DataTemplate;
                    }
                    else
                    {
                        headerClicked.Column.HeaderTemplate =
                          Resources["HeaderTemplateArrowDown"] as DataTemplate;
                    }

                    // Remove arrow from previously sorted header
                    if (_lastHeaderClicked != null && _lastHeaderClicked != headerClicked)
                    {
                        _lastHeaderClicked.Column.HeaderTemplate = null;
                    }

                    _lastHeaderClicked = headerClicked;
                    _lastDirection = direction;
                }
            }
        }

        private void Sort(string sortBy, ListSortDirection direction)
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(ListView.ItemsSource);
            dataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }
    }
}
