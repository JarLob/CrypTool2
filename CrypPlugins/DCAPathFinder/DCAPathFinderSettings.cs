/*
   Copyright 2019 Christian Bender christian1.bender@student.uni-siegen.de

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
using DCAPathFinder;
using DCAPathFinder.Logic;

namespace Cryptool.Plugins.DCAPathFinder
{
    public class DCAPathFinderSettings : ISettings
    {
        #region Private Variables

        private int _chosenMessagePairsCount;
        private string _choiceOfAlgorithm;
        private Algorithms _currentAlgorithm;
        private string _choiceOfSearchPolicy;
        private SearchPolicy _currentSearchPolicy;
        private string _choiceOfAbortingPolicy;
        private AbortingPolicy _currentAbortingPolicy;
        private bool _presentationMode;
        private bool _automaticMode;

        #endregion

        #region Properties

        /// <summary>
        /// Property for Algorithm
        /// </summary>
        public Algorithms CurrentAlgorithm
        {
            get { return _currentAlgorithm; }
            set
            {
                _currentAlgorithm = value;
                OnPropertyChanged("CurrentAlgorithm");
            }
        }

        /// <summary>
        /// Property for SearchPolicy
        /// </summary>
        public SearchPolicy CurrentSearchPolicy
        {
            get { return _currentSearchPolicy; }
            set
            {
                _currentSearchPolicy = value;
                OnPropertyChanged("CurrentSearchPolicy");
            }
        }

        /// <summary>
        /// Property for AbortingPolicy
        /// </summary>
        public AbortingPolicy CurrentAbortingPolicy
        {
            get { return _currentAbortingPolicy; }
            set
            {
                _currentAbortingPolicy = value;
                OnPropertyChanged("CurrentAbortingPolicy");
            }
        }

        #endregion

        #region TaskPane Settings

        /// <summary>
        /// textbox to specify the count of chosen message pairs
        /// </summary>
        [TaskPane("ChosenMessagePairsCount", "ChosenMessagePairsCountToolTip", "DCAOptions",1, false, ControlType.TextBox)]
        public int ChosenMessagePairsCount
        {
            get { return _chosenMessagePairsCount; }
            set
            {
                _chosenMessagePairsCount = value;
                OnPropertyChanged("ChosenMessagePairsCount");
            }
        }

        /// <summary>
        /// checkbox to activate the automatic mode (no user interaction needed)
        /// </summary>
        [TaskPane("AutomaticMode", "AutomaticModeToolTip", "ChoiceOfAlgorithmGroup", 3, false, ControlType.CheckBox)]
        public bool AutomaticMode
        {
            get { return _automaticMode; }
            set
            {
                _automaticMode = value;

                if (_automaticMode)
                {
                    hideSettingsElement("PresentationMode");
                    _presentationMode = false;
                }
                else
                {
                    showSettingsElement("PresentationMode");
                }
                
                OnPropertyChanged("AutomaticMode");
            }
        }

        /// <summary>
        /// checkbox to activate the presentation mode
        /// </summary>
        [TaskPane("PresentationMode", "PresentationModeToolTip", "ChoiceOfAlgorithmGroup", 2, false, ControlType.CheckBox)]
        public bool PresentationMode
        {
            get { return _presentationMode; }
            set
            {
                _presentationMode = value;

                if (_presentationMode)
                {
                    hideSettingsElement("AutomaticMode");
                    _automaticMode = false;
                }
                else
                {
                    showSettingsElement("AutomaticMode");
                }

                OnPropertyChanged("PresentationMode");
            }
        }

        /// <summary>
        /// Selection of the search policy
        /// </summary>
        [TaskPane("ChoiceOfSearchPolicy", "ChoiceOfSearchPolicyToolTop", "DCAOptions", 2, false, ControlType.ComboBox, new string[] { "SearchPolicy1", "SearchPolicy2", "SearchPolicy3" })]
        public string ChoiceOfSearchPolicy
        {
            get { return _choiceOfSearchPolicy; }
            set
            {
                if (_choiceOfSearchPolicy != value)
                {
                    _choiceOfSearchPolicy = value;
                    switch (_choiceOfSearchPolicy)
                    {
                        case "0":
                        {
                            CurrentSearchPolicy = SearchPolicy.FirstBestCharacteristicHeuristic;
                            hideSettingsElement("ChoiceOfAbortingPolicy");
                        }
                            break;
                        case "1":
                        {
                            CurrentSearchPolicy = SearchPolicy.FirstBestCharacteristicDepthSearch;
                            showSettingsElement("ChoiceOfAbortingPolicy");
                        }
                            break;
                        case "2":
                        {
                            CurrentSearchPolicy = SearchPolicy.FirstAllCharacteristicsDepthSearch;
                            hideSettingsElement("ChoiceOfAbortingPolicy");
                        }
                            break;
                    }
                    OnPropertyChanged("ChoiceOfSearchPolicy");
                }
            }
        }

        /// <summary>
        /// Selection of the aborting policy
        /// </summary>
        [TaskPane("ChoiceAbortingPolicyPolicy", "ChoiceOfAbortingPolicyToolTop", "DCAOptions", 3, false, ControlType.ComboBox, new string[] { "AbortingPolicy1", "AbortingPolicy2" })]
        public string ChoiceOfAbortingPolicy
        {
            get { return _choiceOfAbortingPolicy; }
            set
            {
                if (_choiceOfAbortingPolicy != value)
                {
                    _choiceOfAbortingPolicy = value;
                    switch (_choiceOfAbortingPolicy)
                    {
                        case "0":
                        {
                            CurrentAbortingPolicy = AbortingPolicy.GlobalMaximum;
                        }
                            break;
                        case "1":
                        {
                            CurrentAbortingPolicy = AbortingPolicy.Threshold;
                        }
                            break;
                    }
                    OnPropertyChanged("ChoiceOfAbortingPolicy");
                }
            }
        }

        /// <summary>
        /// Selection of the toy cipher algorithm
        /// </summary>
        [TaskPane("ChoiceOfAlgorithm", "ChoiceOfAlgorithmToolTop", "ChoiceOfAlgorithmGroup", 1, false, ControlType.ComboBox, new string[] { "Cipher1", "Cipher2", "Cipher3"})]
        public string ChoiceOfAlgorithm
        {
            get
            {
                return _choiceOfAlgorithm;
            }
            set
            {
                if (_choiceOfAlgorithm != value)
                {
                    _choiceOfAlgorithm = value;
                    switch (_choiceOfAlgorithm)
                    {
                        case "0":
                            CurrentAlgorithm = Algorithms.Cipher1;
                            hideSettingsElement("ChosenMessagePairsCount");
                            hideSettingsElement("ChoiceOfSearchPolicy");
                            hideSettingsElement("ChoiceOfAbortingPolicy");
                            break;
                        case "1":
                            CurrentAlgorithm = Algorithms.Cipher2;
                            showSettingsElement("ChosenMessagePairsCount");
                            showSettingsElement("ChoiceOfSearchPolicy");
                            showSettingsElement("ChoiceOfAbortingPolicy");
                            break;
                        case "2":
                            CurrentAlgorithm = Algorithms.Cipher3;
                            showSettingsElement("ChosenMessagePairsCount");
                            showSettingsElement("ChoiceOfSearchPolicy");
                            showSettingsElement("ChoiceOfAbortingPolicy");
                            break;
                        case "3":
                            CurrentAlgorithm = Algorithms.Cipher4;
                            showSettingsElement("ChosenMessagePairsCount");
                            showSettingsElement("ChoiceOfSearchPolicy");
                            showSettingsElement("ChoiceOfAbortingPolicy");
                            break;
                    }
                    OnPropertyChanged("ChoiceOfAlgorithm");
                }
            }
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion

        /// <summary>
        /// shows a hidden settings element
        /// </summary>
        /// <param name="element"></param>
        private void showSettingsElement(string element)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Visible)));
            }
        }

        /// <summary>
        /// hides a settings element
        /// </summary>
        /// <param name="element"></param>
        private void hideSettingsElement(string element)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Collapsed)));
            }
        }

        public void Initialize()
        {
            //ChosenMessagePairsCount
            //ChoiceOfSearchPolicy
            //ChoiceOfAbortingPolicy


            //check what mode is activated to hide impossible settings
            if (_presentationMode)
            {
                showSettingsElement("PresentationMode");
                hideSettingsElement("AutomaticMode");
            }
            else if(_automaticMode)
            {
                showSettingsElement("AutomaticMode");
                hideSettingsElement("PresentationMode");
            }
                        
            if(_choiceOfSearchPolicy == "1")
            {
                showSettingsElement("ChoiceOfAbortingPolicy");
            }
            else
            {
                hideSettingsElement("ChoiceOfAbortingPolicy");
            }

            //check which algorithm is chosen
            switch (CurrentAlgorithm)
            {
                case Algorithms.Cipher1:
                    hideSettingsElement("ChosenMessagePairsCount");
                    hideSettingsElement("ChoiceOfSearchPolicy");
                    hideSettingsElement("ChoiceOfAbortingPolicy");
                    break;
                default:
                    showSettingsElement("ChosenMessagePairsCount");
                    showSettingsElement("ChoiceOfSearchPolicy");
                    if(_choiceOfSearchPolicy == "2")
                    {
                        hideSettingsElement("ChoiceOfAbortingPolicy");
                        
                    }
                    else
                    {
                        showSettingsElement("ChoiceOfAbortingPolicy");
                    }
                    
                    break;
            }
           
        }
    }
}
