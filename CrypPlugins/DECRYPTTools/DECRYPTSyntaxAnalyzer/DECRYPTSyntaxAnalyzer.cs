﻿/*
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
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using Cryptool.Plugins.DECRYPTTools.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Cryptool.Plugins.DECRYPTTools.DECRYPTSyntaxAnalyzer
{
    [Author("Nils Kopal", "nils.kopal@cryptool.org", "CrypTool 2 Team", "https://www.cryptool.org")]
    [PluginInfo("Cryptool.Plugins.DECRYPTTools.Properties.Resources", "DECRYPTSyntaxAnalyzer", "DECRYPTSyntaxAnalyzer", "DECRYPTTools/userdoc.xml", "DECRYPTTools/icon.png")]
    [ComponentCategory(ComponentCategory.DECRYPTProjectComponent)]
    class DECRYPTSyntaxAnalyzer : ICrypComponent
    {
        private bool _newRecord = false;
        private bool _newTextDocument = false;
        private string _record;
        private string _textDocument;

        /// <summary>
        /// Input of a text document from the downloader
        /// </summary>
        [PropertyInfo(Direction.InputData, "TextDocumentCaption", "TextDocumentTooltip")]
        public string TextDocument
        {
            get
            {
                return _textDocument;
            }
            set
            {
                _textDocument = value;
                _newTextDocument = true;
            }
        }

        /// <summary>
        /// Input of a json record of the DECRYPT database
        /// </summary>
        [PropertyInfo(Direction.InputData, "RecordCaption", "RecordTooltip")]
        public string Record
        {
            get
            {
                return _record;
            }
            set
            {
                _record = value;
                _newRecord = true;
            }
        }

        /// <summary>
        /// Input of a json record of the DECRYPT database
        /// </summary>
        [PropertyInfo(Direction.OutputData, "ReportOutputCaption", "ReportOutputTooltip")]
        public string ReportOutput
        {
            get;
            set;
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

        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PropertyChangedEventHandler PropertyChanged;

        public void Dispose()
        {
            
        }

        public void Execute()
        {
            if(!_newTextDocument || !_newRecord)
            {
                return;
            }

            //Step 0: get record object
            var record = JsonDownloaderAndConverter.ConvertStringToRecord(Record);

            //Step 1: Parse document                                    
            
            if (!TextDocument.Equals("n/a"))
            {
                ReportOutput = "Parsing document: " + record.record_id;
                OnPropertyChanged("ReportOutput");

                SimpleSingleTokenParser parser = new SimpleSingleTokenParser();
                parser.OnGuiLogNotificationOccured += Parser_OnGuiLogNotificationOccured;
                parser.DECRYPTTextDocument = TextDocument;
                var textDocument = parser.GetTextDocument();

                if (string.IsNullOrEmpty(textDocument.CatalogName) || textDocument.CatalogName.Equals("undefined"))
                {
                    ReportOutput = "- CatalogName is null or empty";
                    OnPropertyChanged("ReportOutput");
                }
                else
                {
                    ReportOutput = "+ CatalogName is " + textDocument.CatalogName;
                    OnPropertyChanged("ReportOutput");
                }

                if (string.IsNullOrEmpty(textDocument.TranscriberName) || textDocument.TranscriberName.Equals("undefined"))
                {
                    ReportOutput = "- TranscriberName is null or empty";
                    OnPropertyChanged("ReportOutput");
                }
                else
                {
                    ReportOutput = "+ TranscriberName is " + textDocument.TranscriberName;
                    OnPropertyChanged("ReportOutput");
                }

                if (string.IsNullOrEmpty(textDocument.DateOfTranscription) || textDocument.DateOfTranscription.Equals("undefined"))
                {
                    ReportOutput = "- DateOfTranscription is null or empty";
                    OnPropertyChanged("ReportOutput");
                }
                else
                {
                    ReportOutput = "+ DateOfTranscription is " + textDocument.DateOfTranscription;
                    OnPropertyChanged("ReportOutput");
                }

                if (string.IsNullOrEmpty(textDocument.TranscriptionMethod) || textDocument.TranscriptionMethod.Equals("undefined"))
                {
                    ReportOutput = "- TranscriptionMethod is null or empty";
                    OnPropertyChanged("ReportOutput");
                }
                else
                {
                    ReportOutput = "+ TranscriptionMethod is " + textDocument.TranscriptionMethod;
                    OnPropertyChanged("ReportOutput");
                }

                if (string.IsNullOrEmpty(textDocument.TranscriptionTime) || textDocument.TranscriptionTime.Equals("undefined"))
                {
                    ReportOutput = "- TranscriptionTime is null or empty";
                    OnPropertyChanged("ReportOutput");
                }
                else
                {
                    ReportOutput = "+ TranscriptionTime is " + textDocument.TranscriptionTime;
                    OnPropertyChanged("ReportOutput");
                }

                if (string.IsNullOrEmpty(textDocument.ImageName) || textDocument.ImageName.Equals("undefined"))
                {
                    ReportOutput = "- ImageName is null or empty";
                    OnPropertyChanged("ReportOutput");
                }
                else
                {
                    ReportOutput = "+ ImageName is " + textDocument.ImageName;
                    OnPropertyChanged("ReportOutput");
                }

                if (string.IsNullOrEmpty(textDocument.Comments) || textDocument.Comments.Equals("undefined"))
                {
                    ReportOutput = "- Comments is null or empty";
                    OnPropertyChanged("ReportOutput");
                }
                else
                {
                    ReportOutput = "+ Comments is " + textDocument.Comments;
                    OnPropertyChanged("ReportOutput");
                }

                ReportOutput = "Finished parsing document: " + record.record_id;
                OnPropertyChanged("ReportOutput");
            }
            
            
            //Step 2: ...

            _newTextDocument = false;
            _newRecord = false;
        }

        private void Parser_OnGuiLogNotificationOccured(IPlugin sender, GuiLogEventArgs args)
        {
            ReportOutput = "- " + args.Message;
            OnPropertyChanged("ReportOutput");
        }

        public void Initialize()
        {
            
        }

        public void PostExecution()
        {
            _newTextDocument = false;
            _newRecord = false;
        }

        public void PreExecution()
        {
            _newTextDocument = false;
            _newRecord = false;
        }

        public void Stop()
        {
            
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }
    }
}
