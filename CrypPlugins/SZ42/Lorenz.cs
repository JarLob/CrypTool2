﻿/* HOWTO: Change year, author name and organization.
   Copyright 2010 Your Name, University of Duckburg

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
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using System.Windows.Controls;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Cryptool.Plugins.SZ42
{
    [Author("Wilmer Daza", "mr.wadg@gmail.com", "University of Magdalena", "http://www.unimagdalena.edu.co")]
    [PluginInfo("SZ42.Properties.Resources", false, "PluginCaption", "PluginTooltip", "SZ42/DetailedDescription/doc.xml", "SZ42/Images/sz42.png")]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class Lorenz : ICrypComponent
    {
        #region Private Variables

        private readonly LorenzSettings settings;
        private string inputString;
        private string outputString;
        private string parsedString;
        private bool first = true;
        private bool isPlayMode = false;
        private SZ42 sz42Encrypt, sz42Decrypt, reset;
        private BinaryFormatter bf;
        private MemoryStream ms;

        #endregion

        #region Public Interface

        public Lorenz() 
        {
            settings = new LorenzSettings();

            reset = new SZ42();

            //ConfigMachine(reset);

            settings.LogMessage += Lorenz_LogMessage;
            settings.ReExecute += Lorenz_ReExecute;
        }

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "InputStringCaption", "InputStringTooltip", null, false, false, QuickWatchFormat.Text, null)]
        public string InputString
        {
            get { return this.inputString; }
            set
            {
                if (value != inputString)
                {
                    this.inputString = value;
                    OnPropertyChanged("InputString");
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", "", false, false, QuickWatchFormat.Text, null)]
        public string OutputString
        {
            get { return this.outputString; }
            set
            {
                this.outputString = value;
                OnPropertyChanged("OutputString");
            }
        }

        #endregion

        #region IPlugin Members

        public ISettings Settings
        {
            get { return settings; }
        }

        /// <summary>
        /// HOWTO: You can provide a custom (tabbed) presentation to visualize your algorithm.
        /// Return null if you don't provide one.
        /// </summary>
        public UserControl Presentation
        {
            get { return null; }
        }

        /// <summary>
        /// HOWTO: You can provide custom (quickwatch) presentation to visualize your algorithm.
        /// Return null if you don't provide one.
        /// </summary>
        public UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
            Dispose();
        }

        public void Execute()
        {
            if (first)
            {
                ConfigMachine(reset);
                first = false;
            }

            isPlayMode = true;

            if (!string.IsNullOrEmpty(inputString))
            {
                if (settings.Action == 0)
                {
                    Lorenz_LogMessage("encrypting", NotificationLevel.Info);
                    sz42Encrypt = ResetMachine();
                    Encrypt();
                }
                else if (settings.Action == 1)
                {
                    Lorenz_LogMessage("decrypting", NotificationLevel.Info);
                    sz42Decrypt = ResetMachine();
                    Decrypt();
                }

                OnPropertyChanged("OutputString");
            }
        }

        private void Encrypt() 
        {
            if (settings.Limitation == 1)
                sz42Encrypt.TheresLimitation = false;
            else
                sz42Encrypt.TheresLimitation = true;

            sz42Encrypt.Trace.Clear();

            if (settings.InputParsed)
                parsedString = inputString;
            else
                parsedString = sz42Encrypt.ParseInput(inputString);

            outputString = sz42Encrypt.ActionMachine(parsedString);
        }

        private void Decrypt() 
        {
            if (settings.Limitation == 1)
                sz42Decrypt.TheresLimitation = false;
            else
                sz42Decrypt.TheresLimitation = true;

            sz42Decrypt.Trace.Clear();

            parsedString = sz42Decrypt.ActionMachine(inputString);

            if (settings.OutputParsed)
                outputString = parsedString;
            else
                outputString = sz42Decrypt.ParseOutput(parsedString);
        }

        private SZ42 ResetMachine() 
        {
            ms = new MemoryStream();
            bf = new BinaryFormatter();
            bf.Serialize(ms, reset);
            ms.Position = 0;
            SZ42 sz42 = (SZ42)bf.Deserialize(ms);
            ms.Close();
            return sz42;
        }

        private void ConfigMachine(SZ42 machine)
        {
            string[] positions, patterns;
            int i = 0;

            positions = settings.Positions;
            patterns = settings.Patterns;

            foreach (Wheel wheel in machine.ChiWheels)
            {
                wheel.CurrentPosition = Convert.ToInt32(positions[i]);
                wheel.Pattern = patterns[i].ToCharArray();
                i++;
            }

            foreach (Wheel wheel in machine.PsiWheels)
            {
                wheel.CurrentPosition = Convert.ToInt32(positions[i]);
                wheel.Pattern = patterns[i].ToCharArray();
                i++;
            }

            foreach (Wheel wheel in machine.MuWheels)
            {
                wheel.CurrentPosition = Convert.ToInt32(positions[i]);
                wheel.Pattern = patterns[i].ToCharArray();
                i++;
            }
        }

        /// <summary>
        /// Handles re-execution events from settings class
        /// </summary>
        private void Lorenz_ReExecute()
        {
            if (isPlayMode)
                Execute();
        }

        /// <summary>
        /// Handles log messages from the settings class
        /// </summary>
        private void Lorenz_LogMessage(string msg, NotificationLevel loglevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(msg, this, loglevel));
        }

        public void PostExecution()
        {
            isPlayMode = false;
            Dispose();
        }

        public void Pause()
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
