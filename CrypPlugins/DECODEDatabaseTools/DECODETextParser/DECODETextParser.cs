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
using System.ComponentModel;
using System.Windows.Controls;

namespace Cryptool.Plugins.DECODEDatabaseTools
{
    [Author("Nils Kopal", "nils.kopal@cryptool.org", "CrypTool 2 Team", "https://www.cryptool.org")]
    [PluginInfo("Cryptool.Plugins.DECODEDatabaseTools.Properties.Resources", "DecodeTextParserCaption", "DecodeTextParserTooltip", "DECODEDatabaseTools/userdoc.xml", "DECODEDatabaseTools/icon.png")]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    public class DECODETextParser : ICrypComponent
    {
        #region Private Variables

        private string _DECODETextDocument;
        private string _parsedText;

        #endregion

        #region Data Properties

        /// <summary>
        /// Input of a json record of the DECODE database
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
        /// Input of a json record of the DECODE database
        /// </summary>
        [PropertyInfo(Direction.OutputData, "ParsedTextCaption", "ParsedTextTooltip")]
        public string ParsedText
        {
            get
            {
                return _parsedText;
            }
            set
            {
                _parsedText = value;
            }
        }

        public ISettings Settings
        {
            get
            {
                return null;
            }
        }

        public UserControl Presentation
        {
            get
            {
                return null;
            }
        }

        #endregion

        #region Events

        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Methods

        public void Dispose()
        {
            
        }

        public void Execute()
        {
            SimpleSingleTokenParser parser = new SimpleSingleTokenParser();
            parser.DECODETextDocument = DECODETextDocument;
            var document = parser.GetDocument();
            _parsedText = document.ToString();

            OnPropertyChanged("ParsedText");
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

        #endregion

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }
    }
}
