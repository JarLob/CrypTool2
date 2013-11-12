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
using System.ComponentModel;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.BB84PhotonEncoder
{
    public class BB84PhotonEncoderSettings : ISettings
    {
        #region Private Variables

        private int plusZeroEncoding;
        private int plusOneEncoding;
        private int xZeroEncoding;
        private int xOneEncoding;


        #endregion

        public BB84PhotonEncoderSettings()
        {
            Cryptool.PluginBase.Properties.Settings.Default.PropertyChanged += new PropertyChangedEventHandler(Default_PropertyChanged);
        }

        private void Default_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName.Equals("BB84_AnimationSpeed"))
            {
                OnPropertyChanged("SpeedSetting");
            }
        }


        #region TaskPane Settings

        [TaskPane("res_settings1", "res_settings1Tooltip", null, 1, false, ControlType.ComboBox, new string[] { "|", "-" })]
        public int PlusZeroEncoding
        {
            get
            {
                return plusZeroEncoding;
            }
            set
            {
                if (plusZeroEncoding != value)
                {
                    if (value == 0)
                    {
                        this.plusZeroEncoding = 0;
                        this.PlusOneEncoding = 1;
                        OnPropertyChanged("PlusOneEncoding");
                    }
                    else
                    {
                        this.plusZeroEncoding = 1;
                        this.plusOneEncoding = 0;
                        OnPropertyChanged("PlusOneEncoding");
                    }                
                }
            }
        }
        [TaskPane("res_settings2", "res_settings2Tooltip", null, 2, false, ControlType.ComboBox, new string[] { "|", "-" })]
        public int PlusOneEncoding
        {
            get
            {
                return plusOneEncoding;
            }
            set
            {
                if (plusOneEncoding != value)
                {
                    if (value==0)
                    {
                        this.plusOneEncoding = 0;
                        this.plusZeroEncoding = 1;
                        OnPropertyChanged("PlusZeroEncoding");
                    }
                    else
                    {
                        this.plusOneEncoding = 1;
                        this.plusZeroEncoding = 0;
                        OnPropertyChanged("PlusZeroEncoding");
                    }
                }
            }
        }
        [TaskPane("res_settings3", "res_settings3Tooltip", null, 3, false, ControlType.ComboBox, new string[] { "\\", "/" })]
        public int XZeroEncoding
        {
            get
            {
                return xZeroEncoding;
            }
            set
            {
                if (xZeroEncoding != value)
                {
                    if (value==0)
                    {
                        this.xZeroEncoding = 0;
                        this.xOneEncoding = 1;
                        OnPropertyChanged("XOneEncoding");
                    }
                    else
                    {
                        this.xZeroEncoding = 1;
                        this.xOneEncoding = 0;
                        OnPropertyChanged("XOneEncoding");
                    }                 
                }
            }
        }
        [TaskPane("res_settings4", "res_settings4Tooltip", null, 4, false, ControlType.ComboBox, new string[] { "\\", "/" })]
        public int XOneEncoding
        {
            get
            {
                return xOneEncoding;
            }
            set
            {
                if (xOneEncoding != value)
                {
                    if (value==0)
                    {
                        this.xOneEncoding = 0;
                        this.xZeroEncoding = 1;
                        OnPropertyChanged("XZeroEncoding");
                    }
                    else
                    {
                        this.xOneEncoding = 1;
                        this.xZeroEncoding = 0;
                        OnPropertyChanged("XZeroEncoding");
                    }
                }
            }
        }

        [TaskPane("res_animationSpeed", "res_animationSpeedTooltip", null, 5, false, ControlType.Slider, 0.5, 10.0)]
        public double SpeedSetting
        {
            get
            {
                return Cryptool.PluginBase.Properties.Settings.Default.BB84_AnimationSpeed;
            }
            set
            {
                if (Cryptool.PluginBase.Properties.Settings.Default.BB84_AnimationSpeed != value)
                {
                    Cryptool.PluginBase.Properties.Settings.Default.BB84_AnimationSpeed = value;
                    OnPropertyChanged("SpeedSetting");
                }
            }
        }

  
        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {
            
        }

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion
    }
}
