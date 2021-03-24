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
using System.Windows;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;

namespace CrypTool.Plugins.DECRYPTTools
{
    public enum Mode
    {
        Manual = 0,
        Automatic = 1
    }

    public class DECRYPTDownloaderSettings : ISettings
    {
        private Mode _mode = Mode.Manual;

        public event PropertyChangedEventHandler PropertyChanged;
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        public void Initialize()
        {
            switch (Mode)
            {
                default:
                case Mode.Manual:
                    HideSettingsElement("DownloadButton");
                    break;
                case Mode.Automatic:
                    ShowSettingsElement("DownloadButton");
                    break;
            }
        }

        [TaskPane("DECRYPTDownloaderModeCaption", "DECRYPTDownloaderModeTooltip", null, 1, false, ControlType.ComboBox, 
            new string[]
            {
                "Manual",
                "Automatic",
            })]
        public Mode Mode
        {
            get
            {
                return _mode;
            }
            set
            {
                if ((value) != _mode)
                {
                    _mode = value;
                    OnPropertyChanged("Mode");
                    switch (Mode)
                    {
                        default:
                        case Mode.Manual:
                            HideSettingsElement("DownloadButton");
                            break;
                        case Mode.Automatic:
                            ShowSettingsElement("DownloadButton");
                            break;
                    }
                }
            }
        }

        [TaskPane("DownloadButtonCaption", "DownloadButtonTooltip", null, 1, true, ControlType.Button)]
        public void DownloadButton()
        {                     
            OnPropertyChanged("DownloadButton");
        }

        private void ShowSettingsElement(string element)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Visible)));
            }
        }

        private void HideSettingsElement(string element)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Collapsed)));
            }
        }
    }
}
