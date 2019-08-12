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
using Cryptool.PluginBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Cryptool.Plugins.DECODEDatabaseTools.Util;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.DECODEDatabaseTools.DECODEClusterer
{
    [Author("Nils Kopal", "nils.kopal@cryptool.org", "CrypTool 2 Team", "https://www.cryptool.org")]
    [PluginInfo("Cryptool.Plugins.DECODEDatabaseTools.Properties.Resources", "DECODEClustererCaption", "DECODEClustererTooltip", "DECODEDatabaseTools/userdoc.xml", "DECODEDatabaseTools/icon.png")]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class DECODEClusterer : ICrypComponent
    {
        private ClusterSet _clusterset;

        /// <summary>
        /// Input of a json record of the DECODE database
        /// </summary>
        [PropertyInfo(Direction.InputData, "TextDocumentCaption", "TextDocumentTooltip")]
        public string TextDocument
        {
            get;
            set;
        }


        public ISettings Settings
        {
            get;
            set;
        }

        public UserControl Presentation
        {
            get;
            set;
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PropertyChangedEventHandler PropertyChanged;

        public void Dispose()
        {

        }

        public void Execute()
        {
            if(TextDocument != null)
            {
                SimpleSingleTokenParser parser = new SimpleSingleTokenParser();
                parser.DECODETextDocument = TextDocument;
                var document = parser.GetDocument();
                _clusterset.AddDocument(document);
                int documentCount = _clusterset.Documents.Count;
                int clusterCount = _clusterset.Clusters.Count;
                GuiLogMessage(String.Format("We have now {0} documents in the cluster set and {1} different clusters", documentCount, clusterCount), NotificationLevel.Info);
            }
            TextDocument = null;
        }

        public void Initialize()
        {

        }

        public void PostExecution()
        {

        }

        public void PreExecution()
        {
            _clusterset = new ClusterSet();
        }

        public void Stop()
        {

        }

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }
    }
}
