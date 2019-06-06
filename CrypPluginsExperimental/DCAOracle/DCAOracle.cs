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
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using DCAOracle;

namespace Cryptool.Plugins.DCAOracle
{
    [Author("Christian Bender", "christian1.bender@student.uni-siegen.de", null, "http://www.uni-siegen.de")]
    [PluginInfo("DCAOracle.Properties.Resources", "PluginCaption", "PluginTooltip", "DCAPathFinder/userdoc.xml", new[] { "DCAOracle/Images/IC_Oracle.png" })]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class DCAOracle : ICrypComponent
    {
        #region Private Variables

        private readonly DCAOracleSettings _settings = new DCAOracleSettings();
        private readonly Random _random = new Random();
        private int _messsageDifference;
        private int _messagePairsCount;
        private byte[] _messagePairsOutput;

        #endregion

        #region Data Properties

        /// <summary>
        /// Property for the count of message pairs
        /// </summary>
        [PropertyInfo(Direction.InputData, "MessagePairsCountInput", "MessagePairsCountInputToolTip")]
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
            if (MessagePairsCount == 0)
            {
                GuiLogMessage(, NotificationLevel.Warning)
                return;
            }

            double curProgress = 0;
            double stepCount = 1.0 / (MessagePairsCount * 2);
            ProgressChanged(curProgress, 1);

            List<Pair> pairList = new List<Pair>();

            //generate pairs
            int i;
            for (i = 0; i < MessagePairsCount; i++)
            {
                int x = _random.Next(0, ((int) Math.Pow(2, _settings.WordSize) - 1));
                int y = x ^ MessageDifference;

                Pair inputPair = new Pair()
                {
                    LeftMember = x,
                    RightMember = y
                };
           
                pairList.Add(inputPair);

                curProgress += stepCount;
                ProgressChanged(curProgress, 1);
            }

            //each pair consists of 2 int32 and each int32 consists of 4 byte
            _messagePairsOutput = new byte[MessagePairsCount * 2 * 4];

            //convert pairs
            i = 0;
            foreach (Pair curPair in pairList)
            {
                byte[] leftMember = BitConverter.GetBytes(curPair.LeftMember);
                _messagePairsOutput[i] = leftMember[0];
                _messagePairsOutput[i + 1] = leftMember[1];
                _messagePairsOutput[i + 2] = leftMember[2];
                _messagePairsOutput[i + 3] = leftMember[3];

                byte[] rightMember = BitConverter.GetBytes(curPair.RightMember);
                _messagePairsOutput[i + 4] = rightMember[0];
                _messagePairsOutput[i + 5] = rightMember[1];
                _messagePairsOutput[i + 6] = rightMember[2];
                _messagePairsOutput[i + 7] = rightMember[3];

                i += 8;
                curProgress += stepCount;
                ProgressChanged(curProgress, 1);
            }

            //finished: inform output
            OnPropertyChanged("MessagePairsOutput");
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
