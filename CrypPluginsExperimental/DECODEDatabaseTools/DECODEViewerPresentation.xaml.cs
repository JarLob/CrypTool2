using Cryptool.PluginBase.Miscellaneous;
/*
   Copyright 2018 Nils Kopal <Nils.Kopal<at>CrypTool.org

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
using Cryptool.Plugins.DECODEDatabaseTools.DataObjects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cryptool.Plugins.DECODEDatabaseTools
{
    [PluginBase.Attributes.Localization("Cryptool.Plugins.DECODEDatabaseTools.Properties.Resources")]
    public partial class DECODEViewerPresentation : UserControl, INotifyPropertyChanged
    {
        private DECODEViewer Plugin;
        private Record record;

        public Record Record
        {
            get
            {
                return record;
            }
            set
            {
                record = value;
                OnPropertyChanged("Record");
            }
        }

        public DECODEViewerPresentation(DECODEViewer plugin)
        {
            InitializeComponent();
            Plugin = plugin;
            DataContext = this;
        }
        
        /// <summary>
        /// User double-clicked on an image thumbnail; thus, we download the image and output it now
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        public void ImageListHandleDoubleClick(Object sender, EventArgs eventArgs)
        {
            try
            {
                var lvi = sender as ListViewItem;
                if (lvi != null)
                {
                    var image = lvi.Content as DataObjects.Image;
                    if (image != null)
                    {
                        Plugin.DownloadImage(image);
                    }
                }
            }
            catch (Exception ex)
            {
                //wtf?
            }
        }

        /// <summary>
        /// User double-clicked on a document; thus, we download the document and output it now
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        public void DocumentListHandleDoubleClick(Object sender, EventArgs eventArgs)
        {
            try
            {
                var lvi = sender as ListViewItem;
                if (lvi != null)
                {
                    var document = lvi.Content as DataObjects.Document;
                    if (document != null)
                    {
                        Plugin.DownloadDocument(document);
                    }
                }
            }
            catch (Exception ex)
            {
                //wtf?
            }
        }
        

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }
    }
}
