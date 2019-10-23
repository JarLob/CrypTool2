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
using Cryptool.PluginBase.Miscellaneous;
using Cryptool.Plugins.DECODEDatabaseTools.Util;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Controls;

namespace Cryptool.Plugins.DECODEDatabaseTools
{
    [Author("Nils Kopal", "nils.kopal@cryptool.org", "CrypTool 2 Team", "https://www.cryptool.org")]
    [PluginInfo("Cryptool.Plugins.DECODEDatabaseTools.Properties.Resources", "DECODESymbolHeatmapCaption", "DECODESymbolHeatmapTooltip", "DECODEDatabaseTools/userdoc.xml", "DECODEDatabaseTools/icon.png")]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    public class DECODESymbolHeatmap : ICrypComponent
    {
        private string _DECODETextDocument;
        private string _alphabet;
        private DECODESymbolHeatmapPresentation _presentation = new DECODESymbolHeatmapPresentation();
        private DECODESymbolHeatmapSettings _settings = new DECODESymbolHeatmapSettings();  

        public ISettings Settings
        {
            get { return _settings; }
        }

        public UserControl Presentation
        {
            get { return _presentation; }
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PropertyChangedEventHandler PropertyChanged;

        public DECODESymbolHeatmap()
        {

        }

        /// <summary>
        /// Input of a DECODETextDocument (cipher file)
        /// </summary>
        [PropertyInfo(Direction.InputData, "DECODETextDocumentCaption", "DECODETextDocumentTooltip")]
        public string DECODETextDocument
        {
            get
            {
                return _DECODETextDocument;
            }
            set
            {
                _DECODETextDocument = value;
            }
        }

        /// <summary>
        /// Input of a DECODETextDocument (cipher file)
        /// </summary>
        [PropertyInfo(Direction.InputData, "AlphabetCaption", "AlphabetTooltip")]
        public string Alphabet
        {
            get
            {
                return _alphabet;
            }
            set
            {
                _alphabet = value;
            }
        }

        public void Dispose()
        {

        }

        public void Execute()
        {
            var parser = new NoNomenclatureParser(1, null);
            parser.DECODETextDocument = _DECODETextDocument;
            var textDocument = parser.GetTextDocument();

            var parser2 = new NoNomenclatureParser(1, null);
            parser2.DECODETextDocument = _alphabet;
            var alphabetTokens = parser2.GetTextDocument().ToList();

            _presentation.GenerateNewHeatmap(textDocument, alphabetTokens, _settings);

        }

        public void Initialize()
        {

        }

        public void PostExecution()
        {

        }

        public void PreExecution()
        {

        }

        public void Stop()
        {

        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }
    }
}
