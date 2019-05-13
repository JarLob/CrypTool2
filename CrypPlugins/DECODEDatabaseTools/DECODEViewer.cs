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
using Cryptool.Plugins.DECODEDatabaseTools.DataObjects;
using System.Windows.Threading;
using System.Threading;
using System;
using Cryptool.PluginBase.IO;

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
        private JsonDownloaderAndConverter documentDownloader;
        private JsonDownloaderAndConverter imageDownloader;
        private bool running = false;
        private bool isDocumentDownloading = false;
        private bool isImageDownloading = false;
        #endregion

        /// <summary>
        /// Creats a new DECODE Viewer
        /// </summary>
        public DECODEViewer()
        {
            settings = new DECODEViewerSettings();
            presentation = new DECODEViewerPresentation(this);
            documentDownloader = new JsonDownloaderAndConverter();
            imageDownloader = new JsonDownloaderAndConverter();
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
            running = true;
            ProgressChanged(0, 1);
            Record record;
            try
            {
                record = documentDownloader.GetRecordFromString(DECODERecord);
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
                    //set checkboxes
                    //transcription
                    presentation.HasTranscriptionCheckbox.IsChecked = record.documents.transcription.Count > 0;
                    //cryptanalysis_statistics
                    presentation.HasStatisticsCheckbox.IsChecked = record.documents.cryptanalysis_statistics.Count > 0;
                    //deciphered_text
                    presentation.HasDeciphermentCheckbox.IsChecked = record.documents.deciphered_text.Count > 0;
                    ///translation
                    presentation.HasTranslationCheckbox.IsChecked = record.documents.translation.Count > 0;
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
            documentDownloader.Dispose();
            imageDownloader.Dispose();
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
            if (!running)
            {
                return;
            }
            lock (this)
            {
                if (isImageDownloading || isDocumentDownloading)
                {
                    return;
                }
                isImageDownloading = true;
            }
            try
            {
                OnPluginProgressChanged(this, new PluginProgressEventArgs(0, 1));
                image.DownloadDataCompleted += image_DownloadDataCompleted;
                image.DownloadProgressChanged += image_DownloadProgressChanged;
                image.DownloadImage();
            }
            catch (Exception ex)
            {
                GuiLogMessage(String.Format("Exception occured during downloading of image: {0}", ex.Message), NotificationLevel.Error);
            }     
        }

        /// <summary>
        /// Called, when image download progress changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void image_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs args)
        {
            OnPluginProgressChanged(this, new PluginProgressEventArgs(args.BytesReceived, args.TotalBytesToReceive));
        }

        /// <summary>
        /// Called when image is completely download
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void image_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs args)
        {
            try
            {
                if (args.Error == null)
                {
                    OutputImage = new CStreamWriter(args.Result);
                    OnPropertyChanged("OutputImage");
                    OnPluginProgressChanged(this, new PluginProgressEventArgs(1, 1));
                }
                else
                {
                    GuiLogMessage(String.Format("Exception occured during downloading of image: {0}", args.Error.Message), NotificationLevel.Error);
                }
                lock (this)
                {
                    isImageDownloading = false;
                }
            }
            catch (Exception ex)
            {
                //wtf?
            }
        }

        /// <summary>
        /// Download and output a document
        /// </summary>
        /// <param name="document"></param>
        internal void DownloadDocument(Document document)
        {
            if (!running)
            {
                return;
            }
            lock (this)
            {
                if (isDocumentDownloading || isImageDownloading)
                {
                    return;
                }
                isDocumentDownloading = true;
            }
            try
            {
                OnPluginProgressChanged(this, new PluginProgressEventArgs(0,1));
                document.DownloadDataCompleted += document_DownloadDataCompleted;
                document.DownloadProgressChanged += document_DownloadProgressChanged;
                document.DownloadDocument();                          
            }
            catch (Exception ex)
            {
                GuiLogMessage(String.Format("Exception during downloading of document: {0}", ex.Message), NotificationLevel.Error);
            }     
        }

        /// <summary>
        /// Called when the progress of the downloader changed; changes the progress of the plugin accordingly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void document_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs args)
        {
            OnPluginProgressChanged(this, new PluginProgressEventArgs(args.BytesReceived, args.TotalBytesToReceive));
        }

        /// <summary>
        /// Called when the download of the document is completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void document_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs args)
        {
            try
            {
                if (args.Error == null)
                {
                    OutputDocument = args.Result;
                    OnPropertyChanged("OutputDocument");
                    OnPluginProgressChanged(this, new PluginProgressEventArgs(1, 1));
                }
                else
                {
                    GuiLogMessage(String.Format("Exception occured during downloading of data: {0}", args.Error.Message), NotificationLevel.Error);
                }
                lock (this)
                {
                    isDocumentDownloading = false;
                }
            }
            catch (Exception ex)
            {
                //wtf?
            }
        }
    }
}
