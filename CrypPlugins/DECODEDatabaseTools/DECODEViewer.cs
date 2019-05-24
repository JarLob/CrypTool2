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
using Cryptool.Plugins.DECODEDatabaseTools.DataObjects;
using System.IO;
using System.Windows.Threading;
using System.Threading;
using System;
using System.Text;
using Cryptool.PluginBase.IO;
using System.Drawing.Imaging;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;

namespace Cryptool.Plugins.DECODEDatabaseTools
{
    [Author("Nils Kopal", "nils.kopal@cryptool.org", "CrypTool 2 Team", "https://www.cryptool.org")]
    [PluginInfo("Cryptool.Plugins.DECODEDatabaseTools.Properties.Resources", "DecodeViewerPluginCaption", "DecodeViewerPluginTooltip", "DECODEDatabaseTools/userdoc.xml", "DECODEDatabaseTools/icon.png")]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    public class DECODEViewer : ICrypComponent
    {
        #region Private Variables
        private readonly DECODEViewerSettings _settings;
        private readonly DECODEViewerPresentation _presentation;
        private bool _running;
        private Thread _workerThread;
        #endregion

        /// <summary>
        /// Creats a new DECODE Viewer
        /// </summary>
        public DECODEViewer()
        {
            _settings = new DECODEViewerSettings();
            _presentation = new DECODEViewerPresentation(this);
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
            get { return _settings; }
        }

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation
        {
            get { return _presentation; }
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            this._presentation.Record = new Record();
            _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    _presentation.ImageList.Items.Clear();
                    _presentation.DocumentList.Items.Clear();
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
            if (!JsonDownloaderAndConverter.IsLoggedIn())
            {
                var username = DECODESettingsTab.GetUsername();
                var password = DECODESettingsTab.GetPassword();

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    username = "anonymous";
                    password = "anonymous";
                }

                var loginSuccess = JsonDownloaderAndConverter.Login(username, password);
                if (!loginSuccess)
                {
                    GuiLogMessage(Properties.Resources.LoginFailed, NotificationLevel.Error);
                    return;
                }
            }

            _running = false;
            if (_workerThread == null)
            {
                //create a new thread if we have none
                _workerThread = new Thread(new ThreadStart(ExecuteThread));
                _workerThread.IsBackground = true;
                _workerThread.Start();
            }
            else
            {
                //wait for current thread to stop
                while (_workerThread.IsAlive)
                {
                    Thread.Sleep(10);
                }
                //start a new one
                _workerThread = new Thread(new ThreadStart(ExecuteThread));
                _workerThread.IsBackground = true;
                _workerThread.Start();
            }
        }

        /// <summary>
        /// Thread for executing viewer
        /// We use this to allow restart during execution
        /// </summary>
        private void ExecuteThread()
        {
            _running = true;
            ProgressChanged(0, 1);
            Record record;
            try
            {
                record = JsonDownloaderAndConverter.ConvertStringToRecord(DECODERecord);
            }
            catch (Exception ex)
            {
                GuiLogMessage(String.Format("Could not convert data from DECODE database: {0}", ex.Message), NotificationLevel.Error);
                return;
            }
            try
            {
                _presentation.Record = record;
                //add all images to the ListView of images
                _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    _presentation.ImageList.Items.Clear();
                }, null);
                ProgressChanged(0, 0);
                for (var i = 0; i < record.images.Count; i++)
                {
                    record.images[i].DownloadThumbnail();
                    _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        try
                        {
                            _presentation.ImageList.Items.Add(record.images[i]);
                        }
                        catch (Exception ex)
                        {
                            GuiLogMessage(String.Format("Exception while adding thumbnail to list: {0}", ex.Message), NotificationLevel.Error);
                        }
                    }, null);
                    ProgressChanged(i, record.images.Count);
                    if (_running == false)
                    {
                        return;
                    }
                }
                ProgressChanged(1, 1);

                //add all documents to the ListView of documents
                _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    _presentation.DocumentList.Items.Clear();
                    foreach (var document in record.documents.AllDocuments)
                    {
                        _presentation.DocumentList.Items.Add(document);
                        if (_running == false)
                        {
                            return;
                        }
                    }
                    //set checkboxes
                    //transcription
                    _presentation.HasTranscriptionCheckbox.IsChecked = record.documents.transcription.Count > 0;
                    //cryptanalysis_statistics
                    _presentation.HasStatisticsCheckbox.IsChecked = record.documents.cryptanalysis_statistics.Count > 0;
                    //deciphered_text
                    _presentation.HasDeciphermentCheckbox.IsChecked = record.documents.deciphered_text.Count > 0;
                    //translation
                    _presentation.HasTranslationCheckbox.IsChecked = record.documents.translation.Count > 0;
                }, null);
            }
            catch (Exception ex)
            {
                GuiLogMessage(String.Format("Error while adding data:{0}", ex.Message), NotificationLevel.Error);
                return;
            }
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
            _running = false;
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
            if (!_running)
            {
                return;
            }
            Task.Run(() =>
            {
                byte[] imageBytes = null;
                try
                {
                    imageBytes = image.GetFullImage;
                }
                catch (Exception ex)
                {
                    GuiLogMessage(String.Format("Exception occured during downloading of image: {0}", ex.Message), NotificationLevel.Error);
                }
                if (imageBytes != null)
                {
                    OutputDownloadedImage(imageBytes);
                }
            });
        }

        /// <summary>
        /// Checks, if the image bytes are an html document (=> display not allowed) or an actual image and returns
        /// this
        /// </summary>
        /// <param name="imageBytes"></param>
        private void OutputDownloadedImage(byte[] imageBytes)
        {
            try
            {
                string text = Encoding.UTF8.GetString(imageBytes);
                //this is a hacky check... if the database returns text instead of an image
                //the user is not allowed to download the image
                //we know, it is text, if it contains html and body texts
                if (text.ToLower().Contains("html") && text.ToLower().Contains("body"))
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        Bitmap bitmap = (Bitmap)Properties.Resources.DECODE_message.Clone();

                        RectangleF rectf = new RectangleF(50, 50, 450, 450);
                        Graphics graphics = Graphics.FromImage(bitmap);
                        graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        graphics.DrawString(Properties.Resources.image_not_available_for_download, new Font("Tahoma", 12), Brushes.Black, rectf);

                        bitmap.Save(stream, ImageFormat.Png);
                        bitmap.Dispose();
                        OutputImage = new CStreamWriter(stream.ToArray());
                        OnPropertyChanged("OutputImage");
                    }
                }
                else
                {
                    OutputImage = new CStreamWriter(imageBytes);
                    OnPropertyChanged("OutputImage");
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(String.Format("Exception occured during creation of image: {0}", ex.Message), NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Download and output a document
        /// </summary>
        /// <param name="document"></param>
        internal void DownloadDocument(Document document)
        {
            if (!_running)
            {
                return;
            }
            Task.Run(() => DoDownloadDocument(document));
        }

        private void DoDownloadDocument(Document document)
        {
            try
            {
                OutputDocument = document.DownloadDocument();
                OnPropertyChanged("OutputDocument");
            }
            catch (Exception ex)
            {
                GuiLogMessage(String.Format("Exception during downloading of document: {0}", ex.Message), NotificationLevel.Error);
            }
        }
    }
}
