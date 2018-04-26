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
using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.Net;
using System.Runtime.Serialization.Json;
using Cryptool.Plugins.DECODEDatabaseTools.DataObjects;
using System.IO;
using System.Collections.Generic;
using System.Windows.Threading;
using System.Threading;
using System;
using Cryptool.PluginBase.IO;
using System.Windows.Media.Imaging;

namespace Cryptool.Plugins.DECODEDatabaseTools
{
    [Author("Nils Kopal", "nils.kopal@cryptool.org", "CrypTool 2 Team", "https://www.cryptool.org")]
    [PluginInfo("Cryptool.Plugins.DECODEDatabaseTools.Properties.Resources", "DecodeViewerPluginCaption", "DecodeViewerPluginTooltip", "DECODEDatabaseTools/userdoc.xml", "DECODEDatabaseTools/icon.png")]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    public class DECODEViewer : ICrypComponent
    {
        #region Private Variables
        private DECODEViewerSettings settings;
        private DECODEViewerPresentation presentation;
        private bool running = false;
        #endregion

        public DECODEViewer()
        {
            settings = new DECODEViewerSettings();
            presentation = new DECODEViewerPresentation(this);
        }

        #region Data Properties

        
        /// <summary>
        /// Input of a json record of the DECODE database
        /// </summary>
        [PropertyInfo(Direction.InputData, "DecodeRecordCaption", "DecodeRecordTooltip")]
        public string DECODERecord
        {
            get;
            set;
        }

        /// <summary>
        /// Outputs a selected Image in a CrypToolStream
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputImageCaption", "OutputImageTooltip")]
        public ICryptoolStream OutputImage
        {
            get;
            set;
        }

        /// <summary>
        /// Outputs a selected document as byte array
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputDocumentCaption", "OutputDocumentTooltip")]
        public byte[] OutputDocument
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings
        {
            get { return settings; }
        }

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation
        {
            get { return presentation; }
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            this.presentation.Record = new Record();
            presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    presentation.ImageList.Items.Clear();
                    presentation.DocumentList.Items.Clear();
                }
                catch(Exception)
                {
                    //wtf?
                }
            }, null);
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);
            Record record;
            try
            {
                record = JsonDownloaderAndConverter.GetRecordFromString(DECODERecord);
            }
            catch (Exception ex)
            {
                GuiLogMessage(String.Format("Could not download or convert data from DECODE database: {0}", ex.Message), NotificationLevel.Error);
                return;
            }
            presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    presentation.Record = record;
                    //add all images to the ListView of images
                    presentation.ImageList.Items.Clear();
                    foreach (DataObjects.Image image in record.images)
                    {
                        presentation.ImageList.Items.Add(image);
                    }
                    //add all documents to the ListView of documents
                    presentation.DocumentList.Items.Clear();
                    foreach (DataObjects.Document document in record.documents.AllDocuments)
                    {
                        presentation.DocumentList.Items.Add(document);
                    }
                }
                catch (Exception ex)
                {
                    GuiLogMessage(String.Format("Error while adding data:{0}", ex.Message), NotificationLevel.Error);
                    return;
                }
            }, null);
            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {            
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
            running = false;
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion        
    
        /// <summary>
        /// Download and output an image
        /// </summary>
        /// <param name="image"></param>
        internal void DownloadImage(DataObjects.Image image)
        {
            try
            {
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.QualityLevel = 100;
                using (MemoryStream stream = new MemoryStream())
                {
                    encoder.Frames.Add(BitmapFrame.Create(image.GetFullImage));
                    encoder.Save(stream);
                    byte[] data = stream.ToArray();
                    stream.Close();
                    OutputImage = new CStreamWriter(data);
                    OnPropertyChanged("OutputImage");
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(String.Format("Exception downloading and converting image: {0}", ex.Message), NotificationLevel.Error);
            }       
        }

        /// <summary>
        /// Download and output a document
        /// </summary>
        /// <param name="document"></param>
        internal void DownloadDocument(Document document)
        {
            try
            {
                byte[] documentData = document.GetDocument;
                OutputDocument = documentData;
                OnPropertyChanged("OutputDocument");
            }
            catch (Exception ex)
            {
                GuiLogMessage(String.Format("Exception downloading document: {0}", ex.Message), NotificationLevel.Error);
            }     
        }
    }
}
