/*                              
   Copyright 2009 Team CrypTool (Sven Rech,Dennis Nolte,Raoul Falk,Nils Kopal), Uni Duisburg-Essen

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
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;
using System.Windows;
using System.Collections.ObjectModel;

namespace Cryptool.Plugins.Numbers
{
    class NumberInputSettings : ISettings
    {     
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;
        private ObservableCollection<string> fonts = new ObservableCollection<string>();
        private bool manualFontSettings = false;
        private int font;
        private double fontsize;

        #region Number
        private String number = "";
        public String Number
        {
            get 
            {
                return number;
            }
            set
            {
                number = value;
                OnPropertyChanged("Number");
            }
        }
        #endregion

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

        public NumberInputSettings()
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


        #region ShowDigits
        private bool showDigits = true;
        [TaskPane("ShowDigitsCaption", "ShowDigitsTooltip", "ShowDigitsGroup", 1, true, ControlType.CheckBox, "", null)]
        public bool ShowDigits
        {
            get { return showDigits; }
            set
            {
                if (value != showDigits)
                {
                    showDigits = value;
                    OnPropertyChanged("ShowDigits");
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

        [TaskPane("FontSizeCaption", "FontSizeTooltip", "FontGroup", 5, true, ControlType.NumericUpDown, ValidationType.RangeInteger, 8, 72)]
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

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {
            
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


        private void OnPropertyChanged(string p)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(p));
        }

        #endregion
    }
}
