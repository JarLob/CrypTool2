﻿/*                              
   Copyright 2011 Nils Kopal, Uni Duisburg-Essen

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
using System;
using System.ComponentModel;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Threading;
using System.Threading;

namespace PictureOutput
{
    [Author("Nils Kopal", "nils.kopal@cryptool.org", "Uni Duisburg", "http://www.uni-duisburg-essen.de")]
    [PluginInfo("PictureOutput.Properties.Resources", "PluginCaption", "PluginTooltip", "PictureOutput/documentation.xml", "PictureOutput/icon.png")]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    public class PictureOutput : ICrypComponent
    {
        private PictureOutputSettings _settings = null;
        private PictureOutputPresentation _presentation = null;
        private byte[] _data = null;
        private ICryptoolStream _stream = null;

        public PictureOutput()
        {
            _settings = new PictureOutputSettings();
            _presentation = new PictureOutputPresentation();
        }

        #region IPlugin Members

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public ISettings Settings
        {
            get { return _settings; }
        }

        public System.Windows.Controls.UserControl Presentation
        {
            get { return _presentation; }
        }

        public void PreExecution()
        {
            
        }

        public void Execute()
        {
            try
            {
                if(_data == null && _stream == null)
                    return;

                if (_stream != null)
                {
                    var reader = _stream.CreateReader();
                    _data = reader.ReadFully();
                }

                _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    try{
                        var decoder = BitmapDecoder.Create( new MemoryStream(_data),
                        BitmapCreateOptions.PreservePixelFormat,
                        BitmapCacheOption.None);

                        if (decoder != null && decoder.Frames.Count > 0)
                        {
                            _presentation.Picture.Source = decoder.Frames[0];
                        }
                    }
                    catch (Exception ex)
                    {
                        GuiLogMessage("Could not display picture: " + ex.Message, NotificationLevel.Error);
                        return;
                    }
                }, null);
                ProgressChanged(1, 1);
            }
            catch(Exception ex)
            {
                GuiLogMessage("Could not display picture: " + ex.Message, NotificationLevel.Error);
            }
        }

        public void PostExecution()
        {
            
        }

        public void Stop()
        {
            
        }

        public void Initialize()
        {
            
        }

        public void Dispose()
        {
            
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion

        [PropertyInfo(Direction.InputData, "pictureInputCaption", "pictureInputTooltip", false, QuickWatchFormat.Text, null)]
        public byte[] PictureInput
        {           
            set { _data = value; }
        }

        [PropertyInfo(Direction.InputData, "pictureInputCaption", "pictureInputTooltip", false, QuickWatchFormat.Text, null)]
        public ICryptoolStream PictureStream
        {
            set { _stream = value; }
        }     

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, logLevel));
            }
        }

        public void ProgressChanged(double value, double max)
        {
            if (OnPluginProgressChanged != null)
            {
                OnPluginProgressChanged(this, new PluginProgressEventArgs(value, max));
            }
        }
    }
}
