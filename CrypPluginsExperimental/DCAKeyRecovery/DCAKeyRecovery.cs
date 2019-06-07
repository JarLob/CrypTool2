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

using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using DCAKeyRecovery.UI;

namespace Cryptool.Plugins.DCAKeyRecovery
{
    [Author("Christian Bender", "christian1.bender@student.uni-siegen.de", null, "http://www.uni-siegen.de")]
    [PluginInfo("DCAKeyRecovery.Properties.Resources", "PluginCaption", "PluginTooltip", "DCAPathFinder/userdoc.xml", new[] { "DCAKeyRecovery/Images/IC_KeyRecovery.png" })]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class DCAKeyRecovery : ICrypComponent
    {
        #region Private Variables

        private readonly DCAKeyRecoverySettings _settings = new DCAKeyRecoverySettings();
        private readonly KeyRecoveryPres _pres = new KeyRecoveryPres();
        private string _differential;
        private ICryptoolStream _unencryptedMessagePairs;
        private ICryptoolStream _encryptedMessagePairs;
        private int _neededMessageCount;

        private byte[] _roundKeys;

        #endregion

        #region Data Properties

        /// <summary>
        /// Input for the differential
        /// </summary>
        [PropertyInfo(Direction.InputData, "DifferentialInput", "DifferentialInputToolTip")]
        public string Differential
        {
            get { return _differential; }
            set
            {
                _differential = value;
                OnPropertyChanged("Differential");
            }
        }

        /// <summary>
        /// input of the plaintext message pairs
        /// </summary>
        [PropertyInfo(Direction.InputData, "UnencryptedMessagePairsInput", "UnencryptedMessagePairsInputToolTip")]
        public ICryptoolStream UnencryptedMessagePairs
        {
            get { return _unencryptedMessagePairs; }
            set
            {
                _unencryptedMessagePairs = value;
                OnPropertyChanged("UnencryptedMessagePairs");
            }
        }

        /// <summary>
        /// Input if the encrypted message pairs
        /// </summary>
        [PropertyInfo(Direction.InputData, "EncryptedMessagePairsInput", "EncryptedMessagePairsInputToolTip")]
        public ICryptoolStream EncryptedMessagePairs
        {
            get { return _encryptedMessagePairs; }
            set
            {
                _encryptedMessagePairs = value;
                OnPropertyChanged("EncryptedMessagePairs");
            }
        }

        /// <summary>
        /// Output for the round keys
        /// </summary>
        [PropertyInfo(Direction.OutputData, "RoundKeysOutput", "RoundKeysOutputToolTip")]
        public byte[] RoundKeys
        {
            get { return _roundKeys; }
            set
            {
                _roundKeys = value;
                OnPropertyChanged("RoundKeys");
            }
        }

        /// <summary>
        /// Output for the needed message count
        /// </summary>
        [PropertyInfo(Direction.OutputData, "NeededMessageCountOutput", "NeededMessageCountOutputToolTip")]
        public int NeededMessageCount
        {
            get { return _neededMessageCount; }
            set
            {
                _neededMessageCount = value;
                OnPropertyChanged("NeededMessageCount");
            }
        }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings
        {
            get { return _settings; }
        }

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation
        {
            get { return _pres; }
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);

            

            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }
}
