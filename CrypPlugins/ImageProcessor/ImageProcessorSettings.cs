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
using System.Windows;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.ImageProcessor
{

    public enum ActionType { flip, gray, smooth, resize, rotate, invert, and, or, xor, create, crop, xorgray, blacknwhite, contrast };

    public class ImageProcessorSettings : ISettings
    {
        #region Private Variables

        private ActionType action = ActionType.flip;
        private int flipType = 0;
        private int smooth = 99;
        private int sliderX1 = 0;
        private int sliderX2 = 0;
        private int sliderY1 = 0;
        private int sliderY2 = 0;
        private int threshold = 0;
        private int contrast = 0;
        private int sizeX = 50;
        private int sizeY = 50;
        private int degrees = 90;
        private int outputFileFormat = 0;
        private static String[] comboAction = new string[] { "Flip Image", 
                                      "Gray Scale",
                                      "Create Image",
                                      "Crop Image"};
        
        #endregion

        #region TaskPane Settings
        [TaskPane("ActionCaption", "ActionTooltip", null, 1, true, ControlType.ComboBox, new String[] { 
            "Flip Image", 
            "Gray Scale",
            "Smooth Image",
            "Resize Image",
            "Rotate Image",
            "Invert Image",
            "And-Connect Images",
            "Or-Connect Images",
            "XOR-Connect Images",
            "Create Image",
            "Crop Image",
            "XOR GrayscaleImages",
            "Black and White",
            "Image Contrast"})]
        public ActionType Action
        {
            get
            {
                return this.action;
            }
            set
            {
                if (value != action)
                {
                    this.action = value;
                    UpdateTaskPaneVisibility();
                    OnPropertyChanged("Action");
                }
            }
        }

        [TaskPane("FlipTypeCaption", "FlipTypeTooltip", null, 1, true, ControlType.ComboBox, new string[] {
            "Horizontal", 
            "Vertical" })]
        public int FlipType
        {
            get
            {
                return this.flipType;
            }
            set
            {
                if (value != flipType)
                {
                    this.flipType = value;
                    OnPropertyChanged("FlipType");
                }
            }
        }

        [TaskPane("OutputFileFormatCaption", "OutputFileFormatTooltip", null, 1, true, ControlType.ComboBox, new string[] { 
            "Bmp", 
            "Png", 
            "Tiff" })]
        public int OutputFileFormat
        {
            get
            {
                return this.outputFileFormat;
            }
            set
            {
                if (value != outputFileFormat)
                {
                    this.outputFileFormat = value;
                    OnPropertyChanged("OutputFileFormat");
                }
            }
        }

        [TaskPane("Smooth", "Enter the smooth value", null, 1, false, ControlType.TextBox, ValidationType.RangeInteger, 0, 5000)]
        public int Smooth
        {
            get
            {
                return smooth;
            }
            set
            {
                if (smooth != value)
                {
                    if (value > 10000)
                    {
                        smooth = 10000;
                    }
                    else
                    {
                        smooth = value;
                    }
                    OnPropertyChanged("smooth");
                }
            }
        }
        [TaskPane("SliderX1", "Enter the value of left margin", null, 1, true, ControlType.Slider, 0, 10000)]
        public int SliderX1
        {
            get
            {
                return sliderX1;
            }
            set
            {
                if (sliderX1 != value)
                {
                    sliderX1 = value;
                    OnPropertyChanged("SliderX1");
                }
            }
        }
        [TaskPane("SliderX2", "Enter the value of right margin", null, 1, true, ControlType.Slider, 0, 10000)]
        public int SliderX2
        {
            get
            {
                return sliderX2;
            }
            set
            {
                if (sliderX2 != value)
                {
                    sliderX2 = value;
                    OnPropertyChanged("SliderX2");
                }
            }
        }
        [TaskPane("SliderY1", "Enter the value of top margin", null, 1, true, ControlType.Slider, 0, 10000)]
        public int SliderY1
        {
            get
            {
                return sliderY1;
            }
            set
            {
                if (sliderY1 != value)
                {
                    sliderY1 = value;
                    OnPropertyChanged("SliderY1");
                }
            }
        }
        [TaskPane("SliderY2", "Enter the value of bottom margin", null, 1, true, ControlType.Slider, 0, 10000)]
        public int SliderY2
        {
            get
            {
                return sliderY2;
            }
            set
            {
                if (sliderY2 != value)
                {
                    sliderY2 = value;
                    OnPropertyChanged("SliderY2");
                }
            }
        }

        [TaskPane("Threshold", "Enter the value of the threshold", null, 1, true, ControlType.Slider, 0, 255)]
        public int Threshold
        {
            get
            {
                return threshold;
            }
            set
            {
                if (threshold != value)
                {
                    threshold = value;
                    OnPropertyChanged("Threshold");
                }
            }
        }

        [TaskPane("Contrast", "Enter the value of the threshold", null, 1, true, ControlType.Slider, 1, 1000)]
        public int Contrast
        {
            get
            {
                return contrast;
            }
            set
            {
                if (contrast != value)
                {
                    contrast = value;
                    OnPropertyChanged("Contrast");
                }
            }
        }

        [TaskPane("SizeX", "Enter the value of horizontal pixels", null, 1, false, ControlType.TextBox, ValidationType.RangeInteger, 0, 5000)]
        public int SizeX
        {
            get
            {
                return sizeX;
            }
            set
            {
                if (sizeX != value)
                {
                    if (value > 4096)
                    {
                        sizeX = 4096;
                    }
                    else
                    {
                        sizeX = value;
                    }
                    OnPropertyChanged("sizeX");
                }
            }
        }

        [TaskPane("SizeY", "Enter the value of vertical pixels", null, 1, false, ControlType.TextBox, ValidationType.RangeInteger, 0, 5000)]
        public int SizeY
        {
            get
            {
                return sizeY;
            }
            set
            {
                if (sizeY != value)
                {
                    if (value > 4096)
                    {
                        sizeY = 4096;
                    }
                    else
                    {
                        sizeY = value;
                    }
                    OnPropertyChanged("sizeY");
                }
            }
        }

        [TaskPane("Degrees", "Enter the degrees for the rotation", null, 1, false, ControlType.TextBox, ValidationType.RangeInteger, 0, 360)]
        public int Degrees
        {
            get
            {
                return degrees;
            }
            set
            {
                if (degrees != value)
                {
                    if (value > 36000)
                    {
                        degrees = 360;
                    }
                    else
                    {
                        degrees = value;
                    }
                    OnPropertyChanged("degrees");
                }
            }
        }

        internal void UpdateTaskPaneVisibility()
        {
            settingChanged("OutputFileFormat", Visibility.Visible);
            settingChanged("FlipType", Visibility.Collapsed);
            settingChanged("Smooth", Visibility.Collapsed);
            settingChanged("SizeX", Visibility.Collapsed);
            settingChanged("SizeY", Visibility.Collapsed);
            settingChanged("Degrees", Visibility.Collapsed);
            settingChanged("CustomizeRegions", Visibility.Visible);
            settingChanged("ShowRegions", Visibility.Collapsed);
            settingChanged("SliderX1", Visibility.Collapsed);
            settingChanged("SliderX2", Visibility.Collapsed);
            settingChanged("SliderY1", Visibility.Collapsed);
            settingChanged("SliderY2", Visibility.Collapsed);
            settingChanged("Threshold", Visibility.Collapsed);
            settingChanged("Contrast", Visibility.Collapsed);
            switch (Action)
            {
                case ActionType.flip: // Fliping
                    settingChanged("FlipType", Visibility.Visible);
                    break;
                case ActionType.smooth: // Smooth Image
                    settingChanged("Smooth", Visibility.Visible);
                    break;
                case ActionType.resize: // Resize Image
                    settingChanged("SizeX", Visibility.Visible);
                    settingChanged("SizeY", Visibility.Visible);
                    break;
                case ActionType.crop: // Resize Image
                    settingChanged("SliderX1", Visibility.Visible);
                    settingChanged("SliderX2", Visibility.Visible);
                    settingChanged("SliderY1", Visibility.Visible);
                    settingChanged("SliderY2", Visibility.Visible);
                    break;
                case ActionType.rotate: // Rotate Image
                    settingChanged("Degrees", Visibility.Visible);
                    break;
                case ActionType.create: // Create Image
                    settingChanged("SizeX", Visibility.Visible);
                    settingChanged("SizeY", Visibility.Visible);
                    break;
                case ActionType.blacknwhite: // Black and White Image
                    settingChanged("Threshold", Visibility.Visible);
                    break;
                case ActionType.contrast: // Black and White Image
                    settingChanged("Contrast", Visibility.Visible);
                    break;
            }
        }

        private void settingChanged(string setting, Visibility vis)
        {
            if (TaskPaneAttributeChanged != null)
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(setting, vis)));
        }

        #endregion

        #region Events


        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        public void Initialize()
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion
    }

}
