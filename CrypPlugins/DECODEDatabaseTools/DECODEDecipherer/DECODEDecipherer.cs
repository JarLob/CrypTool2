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
using System;
using System.ComponentModel;
using System.Text;
using System.Windows.Controls;

namespace Cryptool.Plugins.DECODEDatabaseTools
{
    [Author("Nils Kopal", "nils.kopal@cryptool.org", "CrypTool 2 Team", "https://www.cryptool.org")]
    [PluginInfo("Cryptool.Plugins.DECODEDatabaseTools.Properties.Resources", "DECODEDeciphererCaption", "DECODEDeciphererTooltip", "DECODEDatabaseTools/userdoc.xml", "DECODEDatabaseTools/icon.png")]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    public class DECODEDecipherer: ICrypComponent
    {
        #region Private Variables

        private string _DECODETextDocument;
        private string _DECODEKeyDocument;
        private string _outputText;
        private DECODEDeciphererPresentation _presentation = new DECODEDeciphererPresentation();
        private DECODEDeciphererSettings _settings = new DECODEDeciphererSettings();

        #endregion

        #region Constructor

        public DECODEDecipherer()
        {
            _presentation.OnGuiLogNotificationOccured += ForwardGuiLogNotification;
        }

        #endregion

        #region Data Properties

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
        /// Input of DECODEKeyDocument (key file)
        /// </summary>
        [PropertyInfo(Direction.InputData, "DECODEKeyDocumentCaption", "DECODEKeyTooltip")]
        public string DECODEKeyDocument
        {
            get
            {
                return _DECODEKeyDocument;
            }
            set
            {
                _DECODEKeyDocument = value;
            }
        }

        /// <summary>
        /// Output of text (parsed or parsed + decoded)
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputTextCaption", "OutputTextTooltip")]
        public string OutputText
        {
            get
            {
                return _outputText;
            }
            set
            {
                _outputText = value;
            }
        }

        public ISettings Settings
        {
            get
            {
                return _settings;
            }
        }

        public UserControl Presentation
        {
            get
            {
                return _presentation;
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
            Decoder decoder = null;
            if (DECODEKeyDocument != null)
            {
                decoder = new Decoder(DECODEKeyDocument);
                decoder.OnGuiLogNotificationOccured += ForwardGuiLogNotification;
            }
            Parser parser = null;
            switch (_settings.ParserType)
            {                
                case ParserType.NoVocabularyParser:
                    parser = new NoVocabularyParser(2);
                    break;
                case ParserType.Vocabulary3DigitsEndingWithNull1DigitParser:
                    parser = new Vocabulary3DigitsEndingWithNull1DigitParser(_settings.GetNulls());
                    break;
                case ParserType.Vocabulary3DigitsEndingWithNull2DigitsParser:
                    parser = new Vocabulary3DigitsEndingWithNull1DigitParser(_settings.GetNulls());
                    break;
                case ParserType.SimpleSingleTokenParser:
                default:
                    parser = new SimpleSingleTokenParser();
                    break;
            }            
                        
            parser.OnGuiLogNotificationOccured += ForwardGuiLogNotification;
            parser.DECODETextDocument = DECODETextDocument;
            DateTime startTime = DateTime.Now;
            var document = parser.GetDocument();
            if(document == null)
            {
                return;
            }
            GuiLogMessage(String.Format("Parsed document in {0}ms", (DateTime.Now - startTime).TotalMilliseconds), NotificationLevel.Info);
            
            if (decoder != null)
            {
                startTime = DateTime.Now;                
                foreach(var page in document.Pages)
                {
                    foreach(var line in page.Lines)
                    {
                        decoder.Decode(line);
                    }
                }
                GuiLogMessage(String.Format("Decoded document in {0}ms", (DateTime.Now - startTime).TotalMilliseconds), NotificationLevel.Info);
            }
            _presentation.ShowDocument(document);

            if (decoder != null)
            {
                StringBuilder outputBuilder = new StringBuilder();

                foreach (var page in document.Pages)
                {
                    foreach (var line in page.Lines)
                    {
                        if (line.LineType == LineType.Comment)
                        {
                            outputBuilder.AppendLine(line.ToString());
                        }
                        else
                        {
                            foreach(var token in line.Tokens)
                            {
                                if (token.TokenType == TokenType.Tag)
                                {
                                    outputBuilder.Append(token.Text);
                                }
                                else
                                {
                                    outputBuilder.Append(token.DecodedText);
                                }
                            }
                            outputBuilder.AppendLine();
                        }                        
                    }
                }
                _outputText = outputBuilder.ToString();
            }
            else
            {
                _outputText = document.ToString();
            }
            OnPropertyChanged("OutputText");
        }

        /// <summary>
        /// Forwards the gui message of the parsers to CrypTool 2
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ForwardGuiLogNotification(IPlugin sender, GuiLogEventArgs args)
        {
            GuiLogMessage(args.Message, args.NotificationLevel);
        }

        public void Initialize()
        {
            
        }

        public void PostExecution()
        {
            
        }

        public void PreExecution()
        {
            DECODETextDocument = null;
            DECODEKeyDocument = null;            
        }

        public void Stop()
        {
            
        }

        #endregion

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
