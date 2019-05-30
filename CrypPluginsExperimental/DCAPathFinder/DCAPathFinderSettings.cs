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
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using DCAPathFinder;

namespace Cryptool.Plugins.DCAPathFinder
{
    public class DCAPathFinderSettings : ISettings
    {
        #region Private Variables

        private string _choiceOfAlgorithm;
        private Algorithms _currentAlgorithm;

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
        /// Selection of the toy cipher algorithm
        /// </summary>
        [TaskPane("ChoiceOfAlgorithm", "ChoiceOfAlgorithmToolTop", null, 1, false, ControlType.ComboBox, new string[] { "Cipher1", "Cipher2", "Cipher3", "Cipher4" })]
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
                        {
                            CurrentAlgorithm = Algorithms.Cipher1;
                        }
                            break;
                        case "1":
                        {
                            CurrentAlgorithm = Algorithms.Cipher2;
                        }
                            break;
                        case "2":
                        {
                            CurrentAlgorithm = Algorithms.Cipher3;
                        }
                            break;
                        case "3":
                        {
                            CurrentAlgorithm = Algorithms.Cipher4;
                        }
                            break;
                    }
                    OnPropertyChanged("ChoiceOfAlgorithm");
                }
            }
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

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
