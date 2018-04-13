/*
   Copyright 2008 Thomas Schmid, University of Siegen

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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows;

namespace Cryptool.TextInput
{
    public class TextInputSettings : ISettings
    {
        public delegate void TextInputLogMessage(string message, NotificationLevel loglevel);
        public event TextInputLogMessage OnLogMessage;
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        private ObservableCollection<string> fonts = new ObservableCollection<string>();
        private bool showLines = true;
        private bool manualFontSettings = false;
        private int font;
        private double fontsize;

        public ObservableCollection<string> Fonts
        {
            get { return fonts; }
            set
            {
                if (value != fonts)
                {
                    fonts = value;
                    OnPropertyChanged("Fonts");
                }
            }
        }

        public TextInputSettings()
        {
            Fonts.Clear();
            ResetFont();
        }

        public void ResetFont()
        {
            int index = -1;
            int i = 0;
            foreach (var font in System.Windows.Media.Fonts.SystemFontFamilies)
            {
                Fonts.Add(font.ToString());
                if (Cryptool.PluginBase.Properties.Settings.Default.FontFamily == font)
                {
                    index = i;
                }
                i++;
            }
            fontsize = Cryptool.PluginBase.Properties.Settings.Default.FontSize;
            if (index != -1)
            {
                font = index;
            }
            OnPropertyChanged("Font");
            OnPropertyChanged("FontSize");
        }


        private void LogMessage(string message, NotificationLevel logLevel)
        {
            if (OnLogMessage != null) OnLogMessage(message, logLevel);
        }

        private string text;
        public string Text
        {
            get { return text; }
            set
            {
                if (value != text)
                {
                    text = value;
                    OnPropertyChanged("Text");
                }
            }
        }

        #region settings

        private bool showChars = true;
        [TaskPane("ShowCharsCaption", "ShowCharsTooltip", "ShowCharsGroup", 1, true, ControlType.CheckBox, "", null)]
        public bool ShowChars
        {
            get { return showChars; }
            set
            {
                if (value != showChars)
                {
                    showChars = value;
                    OnPropertyChanged("ShowChars");
                }
            }
        }

        [TaskPane("ShowLinesCaption", "ShowLinesTooltip", "ShowCharsGroup", 2, true, ControlType.CheckBox, "", null)]
        public bool ShowLines
        {
            get { return showLines; }
            set
            {
                if (value != showLines)
                {
                    showLines = value;
                    OnPropertyChanged("ShowLines");
                }
            }
        }

        [TaskPane("ManualFontSettingsCaption", "ManualFontSettingsTooltip", "FontGroup", 3, true, ControlType.CheckBox, "")]
        public bool ManualFontSettings
        {
            get { return manualFontSettings; }
            set
            {
                if (value != manualFontSettings)
                {

                    if (value == false) 
                    {
                        CollapseSettingsElement("Font");
                        CollapseSettingsElement("FontSize");
                        ResetFont();
                    }
                    else
                    {
                        ShowSettingsElement("Font");
                        ShowSettingsElement("FontSize");
                    }
                    manualFontSettings = value;
                    OnPropertyChanged("ManualFontSettings");
                }
            }
        }

        [TaskPane("FontCaption", "FontTooltip", "FontGroup", 4, true, ControlType.DynamicComboBox, new string[] { "Fonts" })]
        public int Font
        {
            get { return font; }
            set
            {
                if (value != font)
                {
                    if (manualFontSettings)
                    {
                        font = value;    
                    }                    
                    OnPropertyChanged("Font");
                }
            }
        }

        [TaskPane("FontSizeCaption", "FontSizeTooltip", "FontGroup", 5, true, ControlType.NumericUpDown,ValidationType.RangeInteger,8,72)]
        public double FontSize
        {
            get { return fontsize; }
            set
            {
                if (value != fontsize)
                {
                    if (manualFontSettings)
                    {
                        fontsize = value;
                    }                    
                    OnPropertyChanged("FontSize");
                }
            }
        }
       
        #endregion settings

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {
            if (manualFontSettings)
            {
                ShowSettingsElement("Font");
                ShowSettingsElement("FontSize");
            }
            else
            {
                CollapseSettingsElement("Font");
                CollapseSettingsElement("FontSize");
            }
        }

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        private void ShowSettingsElement(string element)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Visible)));
            }
        }

        private void CollapseSettingsElement(string element)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Collapsed)));
            }
        }

        #endregion
    }
}
