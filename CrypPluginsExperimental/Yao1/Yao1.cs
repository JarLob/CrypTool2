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
using System.ComponentModel;
using System.Windows.Controls;
using System.Numerics;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.Yao1
{
    [Author("Ondřej Skowronek, Armin Krauß", "xskowr00@stud.fit.vutbr.cz", "Brno University of Technology", "https://www.vutbr.cz")]
    [PluginInfo("Yao1.Properties.Resources", "PluginCaption", "PluginTooltip", "Yao1/userdoc.xml", new[] { "Yao1/icon.png" })]
    [ComponentCategory(ComponentCategory.Protocols)]
    public class Yao1 : ICrypComponent
    {
        #region Data Properties

        [PropertyInfo(Direction.InputData, "D", "D")]
        public BigInteger D
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "maxMoneyCaption", "maxMoneyTooltip")]
        public int maxMoney
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "YCaption", "YTooltip")]
        public BigInteger Y
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members

        public ISettings Settings
        {
            get { return null; }
        }

        public UserControl Presentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
        }

        public void Execute()
        {
            ProgressChanged(0, maxMoney);

            for (int i = 0; i < maxMoney; i++)
            {
                Y = D + i;
                OnPropertyChanged("Y");
                // The queue in ConnectorModel.cs only holds 10 property changes.
                // If too many property changes per time unit are generated, they are thrown away, which is fatal for this component.
                // Waiting a short time span between property changes fixes this problem.
                System.Threading.Thread.Sleep(5);
                ProgressChanged(i+1, maxMoney);
            }
        }

        public void PostExecution()
        {
        }

        public void Stop()
        {
        }

        public void Initialize()
        {
        }

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