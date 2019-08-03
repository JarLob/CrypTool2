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
using DCAKeyRecovery;
using DCAKeyRecovery.Properties;

namespace Cryptool.Plugins.DCAKeyRecovery
{
    public class DCAKeyRecoverySettings : ISettings
    {
        #region Private Variables

        private string _choiceOfAlgorithm;
        private Algorithms _currentAlgorithm;
        private bool _automaticMode;
        private bool _UIUpdateWhileExecution;
        private int _maxThreads = Environment.ProcessorCount;
        private int _threadCount;

        #endregion

        #region Properties

        public Algorithms CurrentAlgorithm
        {
            get { return _currentAlgorithm; }
            set
            {
                _currentAlgorithm = value;
                OnPropertyChanged("CurrentAlgorithm");
            }
        }

        #endregion

        #region TaskPane Settings

        /// <summary>
        /// setting to specify the number of threads to use in key recovery
        /// </summary>
        [TaskPane("ThreadCount", "ThreadCountToolTip", "PerformanceSettingsGroup", 3, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 64)]
        public int ThreadCount
        {
            get
            {
                return _threadCount;
            }
            set
            {
                if (value <= _maxThreads)
                {
                    _threadCount = value;
                    OnPropertyChanged("ThreadCount");
                }
                else
                {
                    SettingsErrorMessagsEventArgs e = new SettingsErrorMessagsEventArgs()
                    {
                        message = Resources.ThreadSettingError.Replace("{0}", _maxThreads.ToString())
                    };

                    if (SettingsErrorOccured != null)
                    {
                        SettingsErrorOccured.Invoke(this, e);
                    }

                    ThreadCount = _maxThreads;
                }
            }
        }

        /// <summary>
        /// Checkbox to disable ui refresh during execution
        /// </summary>
        [TaskPane("UIUpdateWhileExecution", "UIUpdateWhileExecutionToolTip", "PerformanceSettingsGroup", 2, false, ControlType.CheckBox)]
        public bool UIUpdateWhileExecution
        {
            get { return _UIUpdateWhileExecution; }
            set
            {
                _UIUpdateWhileExecution = value;
                OnPropertyChanged("UIUpdateWhileExecution");
            }
        }

        /// <summary>
        /// checkbox to activate the automatic mode (no user interaction needed)
        /// </summary>
        [TaskPane("AutomaticMode", "AutomaticModeToolTip", "PerformanceSettingsGroup", 1, false, ControlType.CheckBox)]
        public bool AutomaticMode
        {
            get { return _automaticMode; }
            set
            {
                _automaticMode = value;
                OnPropertyChanged("AutomaticMode");
            }
        }

        /// <summary>
        /// Selection of the toy cipher algorithm
        /// </summary>
        [TaskPane("ChoiceOfAlgorithm", "ChoiceOfAlgorithmToolTop", "TutorialSettingsGroup", 1, false, ControlType.ComboBox, new string[] { "Cipher1", "Cipher2", "Cipher3" })]
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
                            hideSettingsElement("ThreadCount");
                            break;
                        case "1":
                            CurrentAlgorithm = Algorithms.Cipher2;
                            showSettingsElement("ThreadCount");
                            break;
                        case "2":
                            CurrentAlgorithm = Algorithms.Cipher3;
                            showSettingsElement("ThreadCount");
                            break;
                        case "3":
                            CurrentAlgorithm = Algorithms.Cipher4;
                            showSettingsElement("ThreadCount");
                            break;
                    }
                    OnPropertyChanged("ChoiceOfAlgorithm");
                }
            }
        }

        #endregion

        #region Events

        public event EventHandler<SettingsErrorMessagsEventArgs> SettingsErrorOccured;

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
            //check what cipher is activated to hide impossible settings
            if (_choiceOfAlgorithm == "0")
            {
                hideSettingsElement("ThreadCount");
            }
            else
            {
                showSettingsElement("ThreadCount");
            }
        }
    }
}
