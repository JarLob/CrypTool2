﻿/*                              
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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using Cryptool.PluginBase;

namespace Cryptool.Plugins.Converter
{
    public class ConverterSettings : ISettings
    {
        #region private variables
        private int converter = 9; // 0 = String, 1 = int, 2 = short, 3 = byte, 4 = double, 5 = bigInteger, 6= Int[] , 7=Byte[], 8=CryptoolStream, 9 = default
        private bool hasChanges;
        private bool numeric = false;
        private bool formatAmer = false;
        public enum EncodingTypes { Default = 0, Unicode = 1, UTF7 = 2, UTF8 = 3, UTF32 = 4, ASCII = 5, BigEndianUnicode = 6 };
        public enum PresentationFormat { Text, Hex, Base64 }
        private EncodingTypes encoding = EncodingTypes.Default;
        private PresentationFormat presentation = PresentationFormat.Text;

        public PresentationFormat Presentation
        {
            get { return this.presentation; }
            set
            {
                if (this.presentation != value) hasChanges = true;
                this.presentation = value;
                OnPropertyChanged("Presentation");
            }
        }
        #endregion

        #region taskpane
     
        [ContextMenu("Stream encoding", "Choose the expected encoding of the byte array and stream.", 1, DisplayLevel.Experienced, ContextMenuControlType.ComboBox, null, new string[] { "Default system encoding", "Unicode", "UTF-7", "UTF-8", "UTF-32", "ASCII", "Big endian unicode" })]
        [TaskPane("Stream encoding", "Choose the expected encoding of the byte array and stream.", "", 1, false, DisplayLevel.Experienced, ControlType.RadioButton, new string[] { "Default system encoding", "Unicode", "UTF-7", "UTF-8", "UTF-32", "ASCII", "Big endian unicode" })]
        public int EncodingSetting
        {
            get
            {
                return (int)this.encoding;
            }
            set
            {
                if (this.Encoding != (EncodingTypes)value)
                {
                    hasChanges = true;
                    this.Encoding = (EncodingTypes)value;
                    OnPropertyChanged("EncodingSetting");
                    HasChanges = true;
                }
            }
        }
        [TaskPane("Converter", "Choose the output type", null, 1, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "string", "int", "short", "byte", "double", "BigInteger", "int[]", "byte[]","Cryptoolstream" })]
        
      
        public int Converter
        {
            get { return this.converter; }
            set
            {
                if (value != this.converter)
                {
                    this.converter = value;
                    if (TaskPaneAttributeChanged != null)
                    {
                        switch (Converter)
                        {
                            case 0:
                                {
                                   
                                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Numeric", Visibility.Collapsed)));
                                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("EncodingSetting", Visibility.Collapsed)));
                                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("PresentationFormatSetting", Visibility.Collapsed)));
                                    //TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Format", Visibility.Collapsed)));
                                    break;
                                }
                            case 1:
                                {
                                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Numeric", Visibility.Collapsed)));
                                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("EncodingSetting", Visibility.Collapsed)));
                                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Format", Visibility.Collapsed)));
                                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("PresentationFormatSetting", Visibility.Collapsed)));
                                    break;
                                }
                            case 2:
                                {
                                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("PresentationFormatSetting", Visibility.Collapsed)));
                                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("EncodingSetting", Visibility.Collapsed)));
                                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Numeric", Visibility.Collapsed)));
                                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Format", Visibility.Collapsed)));
                                    break;
                                }
                            case 3:
                                {
                                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("PresentationFormatSetting", Visibility.Collapsed)));
                                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("EncodingSetting", Visibility.Collapsed)));
                                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Numeric", Visibility.Collapsed)));
                                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Format", Visibility.Collapsed)));
                                    break;
                                }
                            case 4:
                                {
                                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("PresentationFormatSetting", Visibility.Collapsed)));
                                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("EncodingSetting", Visibility.Collapsed)));
                                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Numeric", Visibility.Collapsed)));
                                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Format", Visibility.Collapsed)));
                                    break;
                                }
                            case 5:
                                {
                                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("PresentationFormatSetting", Visibility.Collapsed)));
                                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("EncodingSetting", Visibility.Collapsed)));
                                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Numeric", Visibility.Collapsed)));
                                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Format", Visibility.Collapsed)));
                                    break;
                                }
                            case 6:
                                {
                                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("PresentationFormatSetting", Visibility.Collapsed)));
                                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("EncodingSetting", Visibility.Collapsed)));
                                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Numeric", Visibility.Collapsed)));
                                    //TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Format", Visibility.Collapsed)));
                                    break;
                                }
                            case 7:
                                {
                                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("PresentationFormatSetting", Visibility.Visible)));
                                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Numeric", Visibility.Visible)));
                                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("EncodingSetting", Visibility.Visible)));
                                    //TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Format", Visibility.Collapsed)));
                                    break;
                                }
                            case 8:
                                {
                                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("PresentationFormatSetting", Visibility.Collapsed)));
                                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("EncodingSetting", Visibility.Collapsed)));
                                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Numeric", Visibility.Collapsed)));
                                    //TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("Format", Visibility.Collapsed)));
                                    break;
                                }
                        }
                    }
                    OnPropertyChanged("Converter");
                    HasChanges = true;

                   ChangePluginIcon(converter+1);
                }
            }
        }
        [TaskPane("Numeric", "Choose whether inputs are interpreted as numeric values if possible", null, 1, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "no", "yes" })]
        public bool Numeric
        {
            get { return this.numeric; }
            set
            {
                if (value != this.numeric)
                {
                    this.numeric = value;
                    OnPropertyChanged("Numeric");
                    HasChanges = true;
                }
            }
        }

        [TaskPane("Format", "Choose whether double values are recognized via german or american syntax. German: \"123.345.34,34\" American: \"123,345,34.34 ", null, 1, false, DisplayLevel.Beginner, ControlType.ComboBox, new string[] { "german", "american" })]
        public bool FormatAmer
        {
            get { return this.formatAmer; }
            set
            {
                if (value != this.formatAmer)
                {
                    this.formatAmer = value;
                    OnPropertyChanged("Format");
                    HasChanges = true;
                }
            }
        }

        [TaskPane("PresentationFormatSetting", "Choose the format that will be used te present the output data.", null, 1, false, DisplayLevel.Beginner, ControlType.RadioButton, new string[] { "Text", "Hex", "Base64" })]
        public int PresentationFormatSetting
        {
            get
            {
                return (int)this.presentation;
            }
            set
            {
                if (this.presentation != (PresentationFormat)value) HasChanges = true;
                this.presentation = (PresentationFormat)value;
                OnPropertyChanged("PresentationFormatSetting");
            }
        }
     
        #endregion

        #region ISettings Member

        public bool HasChanges
        {
            get { return hasChanges; }
            set { hasChanges = value; }

        }
        public EncodingTypes Encoding
        {
            get { return this.encoding; }
            set
            {
                if (this.encoding != value)
                {
                    hasChanges = true;
                    this.encoding = value;
                    OnPropertyChanged("EncodingSetting");
                }
            }
        }


        #endregion

        #region INotifyPropertyChanged Member

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {

        }
        public event StatusChangedEventHandler OnPluginStatusChanged;
        private void ChangePluginIcon(int iconIndex)
        {
            if (OnPluginStatusChanged != null) OnPluginStatusChanged(null, new StatusEventArgs(StatusChangedMode.ImageUpdate, iconIndex));
        }
        #endregion
    }
}
