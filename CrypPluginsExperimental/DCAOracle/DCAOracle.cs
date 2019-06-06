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
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.DCAOracle
{
    [Author("Christian Bender", "christian1.bender@student.uni-siegen.de", null, "http://www.uni-siegen.de")]
    [PluginInfo("DCAOracle.Properties.Resources", "PluginCaption", "PluginTooltip", "DCAPathFinder/userdoc.xml", new[] { "DCAOracle/Images/IC_Oracle.png" })]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class DCAOracle : ICrypComponent
    {
        #region Private Variables

        private readonly DCAOracleSettings _settings = new DCAOracleSettings();
        private int _messsageDifference;
        private int _messagePairsCount;
        private byte[] _messagePairsOutput;

        #endregion

        #region Data Properties

        /// <summary>
        /// Property for the count of message pairs
        /// </summary>
        [PropertyInfo(Direction.InputData, "MessagePairsCountInput", "MessagePairsCountInputToolTip", true)]
        public int MessagePairsCount
        {
            get { return _messagePairsCount; }
            set
            {
                _messagePairsCount = value;
                OnPropertyChanged("MessagePairsCount");
            }
        }

        /// <summary>
        /// Property for the difference of the messages of a pair
        /// </summary>
        [PropertyInfo(Direction.InputData, "MessageDifferenceInput", "MessageDifferenceInputToolTip", false)]
        public int MessageDifference
        {
            get { return _messsageDifference; }
            set
            {
                _messsageDifference = value;
                OnPropertyChanged("MessageDifference");
            }
        }

        /// <summary>
        /// Property for the generated message pairs
        /// </summary>
        [PropertyInfo(Direction.OutputData, "MessagePairsOutput", "MessagePairsOutputToolTip")]
        public byte[] MessagePairsOutput
        {
            get { return _messagePairsOutput; }
            set
            {
                _messagePairsOutput = value;
                OnPropertyChanged("MessagePairsOutput");
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
            get { return null; }
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
            // HOWTO: Use this to show the progress of a plugin algorithm execution in the editor.
            ProgressChanged(0, 1);



            // HOWTO: Make sure the progress bar is at maximum when your Execute() finished successfully.
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
