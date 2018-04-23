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
    public partial class DECODEDownloaderPresentation : UserControl
    {
        private DECODEDownloader Plugin;        
        public ObservableCollection<RecordsRecord> RecordsList = new ObservableCollection<RecordsRecord>();        

        public DECODEDownloaderPresentation(DECODEDownloader plugin)
        {
            InitializeComponent();
            DataContext = RecordsList;
            Plugin = plugin;
        }

        public void HandleDoubleClick(Object sender, EventArgs eventArgs)
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
    }
}
