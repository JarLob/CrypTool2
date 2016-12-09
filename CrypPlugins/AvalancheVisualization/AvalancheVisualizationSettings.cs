/*
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
using System.Windows;

namespace Cryptool.Plugins.AvalancheVisualization
{
   
    public class AvalancheVisualizationSettings : ISettings
    {

        #region Private Variables

        private int keyLength;
        private int prepSelection;
        private int unprepSelection;
        private int contrast;
        private Category selectedCategory;


        #endregion


        public enum Category
        {
            Prepared = 0,
            Unprepared = 1,
            //Hash = 2,

        };

        #region TaskPane Settings

        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>


        [TaskPane("Category", "CategoryTooltip", "GroupName", 0, false, ControlType.ComboBox, new String[] { "PreparedCaption", "UnpreparedCaption"})]
        public Category SelectedCategory
        {
            get { return this.selectedCategory; }
            set
            {

                if (value != selectedCategory)
                {
                    this.selectedCategory = value;
                    OnPropertyChanged("SelectedCategory");
                }

                setSettings();

            }
        }

        [TaskPane("Selection", "SelectionTooltipPrep", "GroupName", 1, false, ControlType.ComboBox, new String[] { "AES", "DES"})]
        public int PrepSelection
        {
            get { return this.prepSelection; }
            set
            {

                if (value != prepSelection)
                {
                    this.prepSelection = value;
                    OnPropertyChanged("PrepSelecction");
                }

                setSettings();

            }
        }

        [TaskPane("Selection", "SelectionTooltipUnprep", "GroupName", 2, false, ControlType.ComboBox, new String[] { "HashFunction", "ClassicCipher","ModernCipher" })]
        public int UnprepSelection
        {
            get { return this.unprepSelection; }
            set
            {

                if (value != unprepSelection)
                {
                    this.unprepSelection = value;
                    OnPropertyChanged("UnprepSelecction");
                }

                setSettings();

            }
        }


        [TaskPane("KeyLength", "KeyLengthTooltip", "GroupName", 2, false, ControlType.ComboBox, new String[] { "128 bits", "192 bits", "256 bits" })]
        public int KeyLength
        {
            get { return this.keyLength; }
            set
            {
                if (((int)value) != keyLength)
                {
                    this.keyLength = (int)value;
                    OnPropertyChanged("KeyLength");
                }
            }
        }

        [TaskPane("Contrast", "ContrastTooltip", "GroupName", 3, false, ControlType.ComboBox, new String[] { "red_green", "black_white" })]
        public int Contrast
        {
            get { return this.contrast; }
            set
            {
                this.contrast = value;
                    OnPropertyChanged("Contrast");
                
            }
        }


        #endregion

        #region Private methods

        private void setSettings()
        {

            switch (this.SelectedCategory)
            {
                case Category.Unprepared:

                    disableSettingsElements("KeyLength");
                    disableSettingsElements("PrepSelection");
                    enableSettingsElements("UnprepSelection");

                    break;

                case Category.Prepared:


                    disableSettingsElements("UnprepSelection");
                    enableSettingsElements("PrepSelection");

                    if (prepSelection == 0)
                        enableSettingsElements("KeyLength");
                    else
                        disableSettingsElements("KeyLength");
                   
                            break;

                default:
                    break;
            }
        }
        #endregion

        #region Events and Event handlers

        public event PropertyChangedEventHandler PropertyChanged;
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;


        private void enableSettingsElements(string element)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Visible)));
            }
        }

        private void disableSettingsElements(string element)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Collapsed)));
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }


        #endregion

        public void Initialize()
        {

        }


    }
}
