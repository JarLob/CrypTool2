﻿/*
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
using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Windows.Threading;
using System.Threading;
using System;
using System.Text;
using Cryptool.Plugins.DECODEDatabaseTools.Util;
using System.Threading.Tasks;

namespace Cryptool.Plugins.DECODEDatabaseTools
{
    [Author("Nils Kopal", "nils.kopal@cryptool.org", "CrypTool 2 Team", "https://www.cryptool.org")]
    [PluginInfo("Cryptool.Plugins.DECODEDatabaseTools.Properties.Resources", "DecodeDownloaderPluginCaption", "DecodeDownloaderPluginTooltip", "DECODEDatabaseTools/userdoc.xml", "DECODEDatabaseTools/icon.png")]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    public class DECODEDownloader : ICrypComponent
    {
        #region Private Variables
        private DECODEDownloaderSettings _settings;
        private DECODEDownloaderPresentation _presentation;
        private bool _running = false;


        #endregion

        /// <summary>
        /// Creats a new DECODE Downloader
        /// </summary>
        public DECODEDownloader()
        {
            _settings = new DECODEDownloaderSettings();
            _settings.PropertyChanged += Settings_PropertyChanged;
            _presentation = new DECODEDownloaderPresentation(this);
            _presentation.OnPluginProgressChanged += _presentation_OnPluginProgressChanged;
            _presentation.OnGuiLogNotificationOccured += presentation_OnGuiLogNotificationOccured;
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
            get { return _settings; }
        }

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation
        {
            get { return _presentation; }
        }

        public bool Running
        {
            get
            {
                return _running;
            }
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    _presentation.RecordsList.Clear();
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
            string username = null;
            string password = null;

            if (!JsonDownloaderAndConverter.IsLoggedIn())
            {
               
                try
                {
                    username = DECODESettingsTab.GetUsername();
                    password = DECODESettingsTab.GetPassword();
                }
                catch (Exception ex)
                {
                    //do nothing
                }

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    username = "anonymous";
                    password = "anonymous";
                }
                try
                {
                    var loginSuccess = JsonDownloaderAndConverter.Login(username, password);
                    if (!loginSuccess)
                    {
                        GuiLogMessage(Properties.Resources.LoginFailed, NotificationLevel.Warning);
                    }
                }
                catch (Exception ex)
                {
                    GuiLogMessage(ex.Message, NotificationLevel.Warning);
                }
            }
            try
            {
               var recordsString = JsonDownloaderAndConverter.GetRecords();
               Records records;
               DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Records));
               using (MemoryStream stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes(recordsString)))
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
               _presentation.Dispatcher.Invoke(DispatcherPriority.Background, (SendOrPostCallback)delegate
               {
                   try
                   {
                       _presentation.RecordsList.Clear();
                       if (records != null)
                       {
                           foreach (RecordsRecord record in records.records)
                           {
                               _presentation.RecordsList.Add(record);
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
                GuiLogMessage(String.Format("Could not download or convert data from DECODE database: {0}", ex.Message), NotificationLevel.Error);
                return;
            }

            _presentation.SetLoginNameLabel(String.Format("You are logged in as {0}", username));

            _running = true;
        }

        public void Download(RecordsRecord record)
        {
            if (!_running)
            {
                return;
            }            
            try
            {
                DECODERecord = JsonDownloaderAndConverter.GetRecord(record.record_id);
                OnPropertyChanged("DECODERecord");
            }
            catch (Exception ex)
            {
                GuiLogMessage(String.Format("Could not download record {0} from DECODE database: {1}", record.record_id, ex.Message), NotificationLevel.Error);
            }            
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

        /// <summary>
        /// If the user clicks the download button, the current filtered record list is downloaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="propertyChangedEventArgs"></param>
        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName.Equals("DownloadButton"))
            {
                Task.Run(() => _presentation.DownloadCurrentRecordList());
            }
        }

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void presentation_OnGuiLogNotificationOccured(IPlugin sender, GuiLogEventArgs args)
        {
            GuiLogMessage(args.Message, args.NotificationLevel);
        }

        private void _presentation_OnPluginProgressChanged(IPlugin sender, PluginProgressEventArgs args)
        {
            ProgressChanged(args.Value, args.Max);
        }

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
