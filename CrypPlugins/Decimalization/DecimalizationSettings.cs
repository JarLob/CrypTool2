﻿/*
   Copyright 2011 CrypTool 2 Team <ct2contact@cryptool.org>

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

namespace Cryptool.Plugins.Decimalization
{
    public class DecimalizationSettings : ISettings
    {
        #region Private Variables

        private int mode = 0;
        private int quant = 0;
        private string assocString= "Association Table";
        private int ibmA = 0, ibmB = 1, ibmC = 2, ibmD = 3, ibmE = 4, ibmF = 5;


        #endregion

        #region Initialization / Constructor

        public void Initialize()
        {
            switch (mode)
            {
                case 0:
                case 1:
                case 2:
                    hideSettingsElement("AssocString");
                    hideSettingsElement("IbmA");hideSettingsElement("IbmB");hideSettingsElement("IbmC");hideSettingsElement("IbmD");hideSettingsElement("IbmE");hideSettingsElement("IbmF");
                    break;
                case 3:
                    showSettingsElement("AssocString");
                    showSettingsElement("IbmA");showSettingsElement("IbmB");showSettingsElement("IbmC");showSettingsElement("IbmD");showSettingsElement("IbmE");showSettingsElement("IbmF");
                    break;
                default:
                    break;
            }

        }

        #endregion
        
        #region TaskPane Settings

        [PropertySaveOrder(1)]
        [TaskPane("ModeCaption", "ModeCaptionToolTip", null, 1, false, ControlType.ComboBox, new String[] { "ModeList1", "ModeList2", "ModeList3", "ModeList4" })]
        public int Mode
        {
            get
            {
                return mode;
            }
            set
            {
                if (mode != value)
                {
                    mode = value;

                    switch (mode)
                    { 
                        case 0:
                        case 1:
                        case 2:
                            hideSettingsElement("AssocString");
                            hideSettingsElement("IbmA");hideSettingsElement("IbmB");hideSettingsElement("IbmC");hideSettingsElement("IbmD");hideSettingsElement("IbmE");hideSettingsElement("IbmF");
                            break;
                        case 3:
                            showSettingsElement("AssocString");
                            showSettingsElement("IbmA");showSettingsElement("IbmB");showSettingsElement("IbmC");showSettingsElement("IbmD");showSettingsElement("IbmE");showSettingsElement("IbmF");
                            break;
                        default:
                            break;
                    }

                    OnPropertyChanged("Mode");
                }
            }
        }

        [PropertySaveOrder(2)]
        [TaskPane("QuantCaption", "QuantCaptionToolTip", null, 2, false, ControlType.NumericUpDown,ValidationType.RangeInteger,1,Int32.MaxValue)]
        public int Quant
        {
            get
            {
                return quant;
            }
            set
            {
                if (quant != value)
                {
                    quant = value;
                    OnPropertyChanged("Quant");
                }
            }
        }

        [PropertySaveOrder(3)]
        [TaskPaneAttribute("AssocStringCaption", "AssocStringToolTip", null, 3, true, ControlType.TextBoxReadOnly)]
        public string AssocString
        {
            get { return assocString; }
            set
            {
                /*if (!assocString.Equals(value))
                {
                    assocString = value;
                    OnPropertyChanged("AssocString");
                }*/
            }
        }
       
        [PropertySaveOrder(4)]
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Eins")]
        [TaskPane("IBMACaption", "IBMAToolTip", null, 41, false, ControlType.ComboBox, new string[] { "NumberList0", "NumberList1", "NumberList2", "NumberList3", "NumberList4", "NumberList5", "NumberList6", "NumberList7", "NumberList8", "NumberList9" })]
        public int IbmA
        {
            get { return ibmA; }
            set 
            {
                if (ibmA != value)
                {
                    ibmA = value;
                    OnPropertyChanged("IbmA");
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Eins")]
        [TaskPane("IBMBCaption", "IBMBToolTip", null, 41, false, ControlType.ComboBox, new string[] { "NumberList0", "NumberList1", "NumberList2", "NumberList3", "NumberList4", "NumberList5", "NumberList6", "NumberList7", "NumberList8", "NumberList9" })]
        public int IbmB
        {
            get { return ibmB; }
            set
            {
                if (ibmB != value)
                {
                    ibmB = value;
                    OnPropertyChanged("IbmB");
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Eins")]
        [TaskPane("IBMCCaption", "IBMCToolTip", null, 41, false, ControlType.ComboBox, new string[] { "NumberList0", "NumberList1", "NumberList2", "NumberList3", "NumberList4", "NumberList5", "NumberList6", "NumberList7", "NumberList8", "NumberList9" })]
        public int IbmC
        {
            get { return ibmC; }
            set
            {
                if (ibmC != value)
                {
                    ibmC = value;
                    OnPropertyChanged("IbmC");
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("IBMDCaption", "IBMDToolTip", null, 41, false, ControlType.ComboBox, new string[] { "NumberList0", "NumberList1", "NumberList2", "NumberList3", "NumberList4", "NumberList5", "NumberList6", "NumberList7", "NumberList8", "NumberList9" })]
        public int IbmD
        {
            get { return ibmD; }
            set
            {
                if (ibmD != value)
                {
                    ibmD = value;
                    OnPropertyChanged("IbmD");
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("IBMECaption", "IBMEToolTip", null, 41, false, ControlType.ComboBox, new string[] { "NumberList0", "NumberList1", "NumberList2", "NumberList3", "NumberList4", "NumberList5", "NumberList6", "NumberList7", "NumberList8", "NumberList9" })]
        public int IbmE
        {
            get { return ibmE; }
            set
            {
                if (ibmE != value)
                {
                    ibmE = value;
                    OnPropertyChanged("IbmE");
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("IBMFCaption", "IBMFToolTip", null, 41, false, ControlType.ComboBox, new string[] { "NumberList0", "NumberList1", "NumberList2", "NumberList3", "NumberList4", "NumberList5", "NumberList6", "NumberList7", "NumberList8", "NumberList9" })]
        public int IbmF
        {
            get { return ibmF; }
            set
            {
                if (ibmF != value)
                {
                    ibmF = value;
                    OnPropertyChanged("IbmF");
                }
            }
        }

        private void showSettingsElement(string element)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Visible)));
            }
        }

        private void hideSettingsElement(string element)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Collapsed)));
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        #endregion

        #region Public Events and Methods

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        #endregion
    }
}
