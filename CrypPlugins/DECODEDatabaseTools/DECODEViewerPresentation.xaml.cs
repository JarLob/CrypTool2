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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="plugin"></param>
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
            catch (Exception)
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
            catch (Exception)
            {
                //wtf?
            }
        }
        

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        /// <summary>
        /// Forward MouseWheel event to parent ui element (ScrollViewer) 
        /// to enable the user to scroll through the image list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageList_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }

        /// <summary>
        /// Handler for the context menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextMenuHandler(object sender, RoutedEventArgs eventArgs)
        {
            try
            {
                MenuItem menu = (MenuItem)((RoutedEventArgs)eventArgs).Source;
                string tag = (string)menu.Tag;

                if(Record == null)
                {
                    Clipboard.SetText("");
                    return;
                }

                StringBuilder builder = new StringBuilder();

                builder.Append(Properties.Resources.Name);
                builder.AppendLine(Record.metadata.name);
                builder.Append(Properties.Resources.Id);
                builder.AppendLine(Record.record_id.ToString());

                if (tag == "copy_content" || tag == "copy_all")
                {
                    builder.AppendLine();                    
                    builder.AppendLine(Properties.Resources.Content);
                    
                    builder.Append(Properties.Resources.Type);
                    builder.AppendLine(" " + Record.metadata.content.type);

                    builder.Append(Properties.Resources.CipherType);
                    builder.AppendLine(" " + Record.metadata.content.cipher_type);

                    builder.Append(Properties.Resources.SymbolSet);
                    builder.AppendLine(" " + Record.metadata.content.symbol_set);

                    builder.Append(Properties.Resources.NumberOfPages);
                    builder.AppendLine(" " + Record.metadata.content.number_of_pages.ToString());

                    builder.Append(Properties.Resources.InlinePlaintext);
                    builder.AppendLine(" " + Record.metadata.content.inline_plaintext);

                    builder.Append(Properties.Resources.InlineCleartext);
                    builder.AppendLine(" " + Record.metadata.content.inline_cleartext);

                    builder.Append(Properties.Resources.CleartextLanguage);
                    builder.AppendLine(" " + Record.metadata.content.cleartext_language);

                    builder.Append(Properties.Resources.PlaintextLanguage);
                    builder.AppendLine(" " + Record.metadata.content.plaintext_language);

                }
                if (tag == "copy_origin"|| tag == "copy_all")
                {
                    builder.AppendLine();
                    builder.AppendLine(Properties.Resources.Origin);

                    builder.Append(Properties.Resources.Author);
                    builder.AppendLine(" " + Record.metadata.origin.author);

                    builder.Append(Properties.Resources.Sender);
                    builder.AppendLine(" " + Record.metadata.origin.sender);

                    builder.Append(Properties.Resources.Receiver);
                    builder.AppendLine(" " + Record.metadata.origin.receiver);

                    builder.Append(Properties.Resources.Dating);
                    builder.AppendLine(" " + Record.metadata.origin.dating);

                    builder.Append(Properties.Resources.Region);
                    builder.AppendLine(" " + Record.metadata.origin.region);

                    builder.Append(Properties.Resources.City);
                    builder.AppendLine(" " + Record.metadata.origin.city);
                }
                if (tag == "copy_format"|| tag == "copy_all")
                {
                    builder.AppendLine();
                    builder.AppendLine(Properties.Resources.Format);

                    builder.Append(Properties.Resources.Paper);
                    builder.AppendLine(" " + Record.metadata.format.paper);

                    builder.Append(Properties.Resources.InkType);
                    builder.AppendLine(" " + Record.metadata.format.ink_type);
                }                
                Clipboard.SetText(builder.ToString());
            }
            catch (Exception)
            {
                Clipboard.SetText("");
            }
        }
    }
}
