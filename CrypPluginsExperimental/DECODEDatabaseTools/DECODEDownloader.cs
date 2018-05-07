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
using System.Text;

namespace Cryptool.Plugins.DECODEDatabaseTools
{
    [Author("Nils Kopal", "nils.kopal@cryptool.org", "CrypTool 2 Team", "https://www.cryptool.org")]
    [PluginInfo("Cryptool.Plugins.DECODEDatabaseTools.Properties.Resources", "DecodeDownloaderPluginCaption", "DecodeDownloaderPluginTooltip", "DECODEDatabaseTools/userdoc.xml", "DECODEDatabaseTools/icon.png")]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    public class DECODEDownloader : ICrypComponent
    {
        #region Private Variables
        private DECODEDownloaderSettings settings;
        private DECODEDownloaderPresentation presentation;
        private JsonDownloaderAndConverter jsonDownloaderAndConverter;
        private bool running = false;
        #endregion

        /// <summary>
        /// Creats a new DECODE Downloader
        /// </summary>
        public DECODEDownloader()
        {
            settings = new DECODEDownloaderSettings();
            presentation = new DECODEDownloaderPresentation(this);
            jsonDownloaderAndConverter = new JsonDownloaderAndConverter();
            jsonDownloaderAndConverter.DownloadDataCompleted += jsonDownloaderAndConverter_DownloadDataCompleted;
            jsonDownloaderAndConverter.DownloadStringCompleted += jsonDownloaderAndConverter_DownloadStringCompleted;
            jsonDownloaderAndConverter.DownloadProgressChanged += jsonDownloaderAndConverter_DownloadProgressChanged;
        }

        #region Data Properties

        [PropertyInfo(Direction.OutputData, "DecodeRecordCaption", "DecodeRecordTooltip")]
        public string DECODERecord
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
            presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    presentation.ListView.Items.Clear();
                }
                catch (Exception)
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
            try
            {
               jsonDownloaderAndConverter.GetRecordsList(JsonDownloaderAndConverter.DOWNLOAD_URL);
            }
            catch (Exception ex)
            {
                GuiLogMessage(String.Format("Could not download or convert data from DECODE database: {0}", ex.Message), NotificationLevel.Error);
                return;
            }                   
            running = true;
        }
       
        /// <summary>
        /// Download progess changed; we update the plugin progress accordingly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        
        private void jsonDownloaderAndConverter_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs args)
        {
            ProgressChanged(args.BytesReceived, args.TotalBytesToReceive);
        }        

        /// <summary>
        /// Download record list finished
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void jsonDownloaderAndConverter_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs args)
        {
            if (args.UserState.Equals(JsonDownloaderAndConverter.GETRECORDLIST))
            {
                HandleDownloadRecordsFinished(args);
                ProgressChanged(1, 1);
            }            
        }

        /// <summary>
        /// Download record finished
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void jsonDownloaderAndConverter_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs args)
        {
            if (args.UserState.Equals(JsonDownloaderAndConverter.GETRECORDSTRING))
            {
                HandleDownloadRecordFinished(args);
                ProgressChanged(1, 1);
            }
            
        }      

        /// <summary>
        /// We received a records list; now we show it to the user
        /// </summary>
        /// <param name="args"></param>
        private void HandleDownloadRecordsFinished(DownloadDataCompletedEventArgs args)
        {
            if (args.Error != null)
            {
                GuiLogMessage(String.Format("Error during downloading of records list: {0}", args.Error.Message), NotificationLevel.Error);
                return;
            }
            try
            {
                Records records;
                //end of dirty hack
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Records));
                using (MemoryStream stream = new MemoryStream(args.Result))
                {
                    stream.Position = 0;
                    try
                    {
                        records = (Records)serializer.ReadObject(stream);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(String.Format("Could not deserialize json data received from DECODE database: {0}", ex.Message), ex);
                    }
                }
                presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    try
                    {
                        presentation.RecordsList.Clear();
                        if (records != null)
                        {
                            foreach (RecordsRecord record in records.records)
                            {
                                presentation.RecordsList.Add(record);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        GuiLogMessage(String.Format("Exception while adding received data to ListView: {0}", ex.Message), NotificationLevel.Error);
                        return;
                    }
                }, null);
            }
            catch (Exception ex)
            {
                GuiLogMessage(String.Format("Exception while adding received data to ListView: {0}", ex.Message), NotificationLevel.Error);
            }     
        }

        /// <summary>
        /// We downloaded a record json string; now we output it
        /// </summary>
        /// <param name="args"></param>
        private void HandleDownloadRecordFinished(DownloadStringCompletedEventArgs args)
        {
            if (args.Error == null)
            {
                DECODERecord = args.Result;
                OnPropertyChanged("DECODERecord");
            }
            else
            {
                GuiLogMessage(String.Format("Exception while downloading of record: {0}", args.Error.Message), NotificationLevel.Error);
            }
        }

        public void Download(RecordsRecord record)
        {
            if (!running)
            {
                return;
            }
            ProgressChanged(0, 1);
            jsonDownloaderAndConverter.GetRecordString(record);
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
            jsonDownloaderAndConverter.Dispose();
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
    }
}
