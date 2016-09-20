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
    // HOWTO: rename class (click name, press F2)
    public class AvalancheVisualizationSettings : ISettings
    {

        #region Private Variables

        private int keyLength;
        private int language;
        private int subcategory;
        private Category selectedCategory;


        #endregion




        public enum Category
        {
            Modern = 0,
            Classic = 1,
            Hash = 2,

        };

        #region TaskPane Settings

        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>


        [TaskPane("Category", "select the category of the algorithm whose avalanche effect you would like to test", "Test avalanche effect of", 0, false, ControlType.ComboBox, new String[] { "Modern ciphers", "Classic ciphers", "Hash functions" })]
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

        [TaskPane("Cryptographic algorithm", "select the algorithm whose avalanche effect you would like to test", "Test avalanche effect of", 1, false, ControlType.ComboBox, new String[] { "AES", "DES", "Other" })]
        public int Subcategory
        {
            get { return this.subcategory; }
            set
            {

                if (value != subcategory)
                {
                    this.subcategory = value;
                    OnPropertyChanged("Subcategory");
                }

                setSettings();

            }
        }


        [TaskPane("Key length", "Select the length of the key to be entered", "Test avalanche effect of", 3, false, ControlType.ComboBox, new String[] { "128 bit", "192 bit", "256 bit" })]
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

        [TaskPane("Language", "Select the desired language", "Plugin language", 3, false, ControlType.ComboBox, new String[] { "Deutsch", "English" })]
        public int Language
        {
            get { return this.language; }
            set
            {
                if (((int)value) != language
                    )
                {
                    this.language = (int)value;
                    OnPropertyChanged("Language");
                }
            }
        }

        #endregion

        #region Private methods

        private void setSettings()
        {

            switch (this.SelectedCategory)
            {
                case Category.Classic:
                case Category.Hash:
                    disableSettingsElements("KeyLength");
                    disableSettingsElements("Subcategory");
                    break;
                case Category.Modern:
                    enableSettingsElements("Subcategory");

                    switch (subcategory)
                    {
                        case 0:
                            enableSettingsElements("KeyLength");
                            break;
                        case 1:
                        case 2:
                            disableSettingsElements("KeyLength");
                            break;
                    }
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
