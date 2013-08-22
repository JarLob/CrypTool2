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
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using BB84KeyGenerator;
using System.Windows.Threading;
using System.Threading;
using System;
using System.Text;

namespace Cryptool.Plugins.BB84KeyGenerator
{
    [Author("Benedict Beuscher", "benedict.beuscher@stud.uni-due.de", "Uni Duisburg-Essen", "http://www.uni-due.de/")]

    [PluginInfo("Cryptool.Plugins.BB84KeyGenerator.Properties.Resources","res_GeneratorCaption", "res_GeneratorTooltip", "BB84KeyGenerator/userdoc.xml", new[] { "CrypWin/images/default.png" })]
    [ComponentCategory(ComponentCategory.Protocols)]
    public class BB84KeyGenerator : ICrypComponent
    {
        #region Private Variables

        private readonly BB84KeyGeneratorSettings settings = new BB84KeyGeneratorSettings();
        private string inputKey;
        private string inputBasesFirst;
        private string inputBasesSecond;
        private string outputCommonKey;
        private BB84KeyGeneratorPresentation myPresentation;
        
        #endregion
      

        public BB84KeyGenerator()
        {
            myPresentation = new BB84KeyGeneratorPresentation();
            myPresentation.UpdateProgess += new EventHandler(update_progress);
            Presentation = myPresentation;
            
        }
        private void update_progress(object sender, EventArgs e)
        {
            ProgressChanged(myPresentation.Progress, 3000);
        }

        #region Data Properties

        [PropertyInfo(Direction.InputData, "res_InputKeyCaption", "res_InputKeyTooltip")]
        public string InputKey
        {
            get
            {
                return this.inputKey;
            }

            set
            {
                string newInput = filterValidInput(value);
                this.inputKey = newInput;
            }
        }

        private string filterValidInput(string value)
        {
            StringBuilder outputString = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i].Equals('0') || value[i].Equals('1'))
                {
                    outputString.Append(value[i]);
                }
            }

            return outputString.ToString();
        }

        [PropertyInfo(Direction.InputData, "res_InputFirstBasesCaption", "res_InputFirstBasesTooltip")]
        public string InputBasesFirst
        {
            get
            {
                return this.inputBasesFirst;
            }

            set
            {
                this.inputBasesFirst = value;
            }
        }

        [PropertyInfo(Direction.InputData, "res_InputSecondBasesCaption", "res_InputSecondBaseTooltip")]
        public string InputBasesSecond
        {
            get
            {
                return this.inputBasesSecond;
            }

            set
            {
                this.inputBasesSecond = value;
            }
        }

        [PropertyInfo(Direction.InputData, "res_PhotonInputCaption", "res_PhotonInputTooltip", true)]
        public string InputPhotons
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        [PropertyInfo(Direction.OutputData, "res_CommonKeyCaption", "res_CommonKeyTooltip")]
        public string OutputCommonKey
        {
            get
            {
               return this.outputCommonKey;
            }
            set { } //read-only

        }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings
        {
            get { return settings; }
        }

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation
        {
            get;
            private set;
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            myPresentation.speed = settings.SpeedSetting;
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);

            StringBuilder tempOutput = new StringBuilder();

            char[] tempBasesFirst = inputBasesFirst.ToCharArray();
            char[] tempBasesSecond = inputBasesSecond.ToCharArray();
            char[] tempKey = inputKey.ToCharArray();

            for (int i = 0; i < inputKey.Length; i++)
            {
                if (tempBasesFirst[i].Equals(tempBasesSecond[i]))
                {
                    tempOutput.Append(tempKey[i]);
                }
            }

            ProgressChanged(1, 1);
            outputCommonKey = tempOutput.ToString();

            if (Presentation.IsVisible)
            {
                Presentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    myPresentation.StartPresentation(inputBasesFirst, inputBasesSecond, inputKey);
                }, null);
            

                while (!myPresentation.hasFinished)
                {
                
                }

            }
            OnPropertyChanged("OutputCommonKey");
        }

       
        public void PostExecution()
        {
            Presentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            { myPresentation.StopPresentation(); }, null);
        }

        public void Stop()
        {
            Presentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            { myPresentation.StopPresentation(); }, null);
        }


        public void Initialize()
        {
            settings.SpeedSetting = 1.0;
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
