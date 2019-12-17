﻿/*
   Copyright 2018 Nils Kopal, Henner Heck, Applied Information Security, Uni Kassel
   https://www.uni-kassel.de/eecs/fachgebiete/ais/mitarbeiter/nils-kopal-m-sc.html

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
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Threading;
using Cryptool.CrypAnalysisViewControl;

namespace Cryptool.ProcessExecutor
{
    [PluginBase.Attributes.Localization("Cryptool.ProcessExecutor.Properties.Resources")]
    public partial class AssignmentPresentation : UserControl
    {

        public ObservableCollection<ResultEntry> BestList { get; } = new ObservableCollection<ResultEntry>();

        #region Variables

        private UpdateOutput _updateOutputFromUserChoice;

        #endregion

        #region Properties

        public UpdateOutput UpdateOutputFromUserChoice
        {
            get { return _updateOutputFromUserChoice; }
            set { _updateOutputFromUserChoice = value; }
        }

        #endregion

        #region constructor

        public AssignmentPresentation()
        {
            InitializeComponent();
            DataContext = BestList;
        }

        #endregion

        #region Main Methods

        public void DisableGUI()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                IsEnabled = false;
            }, null);
        }

        public void EnableGUI()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                IsEnabled = true;
            }, null);
        }

        #endregion

        #region Helper Methods

        private void HandleResultItemAction(ICrypAnalysisResultListEntry item)
        {
            if (item is ResultEntry resultItem)
            {
                _updateOutputFromUserChoice?.Invoke(resultItem.Key, resultItem.Text);
            }
        }

        #endregion
    }
}
