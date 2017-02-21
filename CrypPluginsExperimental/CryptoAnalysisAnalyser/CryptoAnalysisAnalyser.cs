/*
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

namespace Cryptool.Plugins.CryptoAnalysisAnalyser
{
    [Author("Bastian Heuser", "bhe@student.uni-kassel.de", "Uni Kassel", "http://www.uni-kassel.de/eecs/fachgebiete/ais/")]
    // HOWTO: Change plugin caption (title to appear in CT2) and tooltip.
    // You can (and should) provide a user documentation as XML file and an own icon.
    [PluginInfo("CryptoAnalysisAnalyser", "Subtract one number from another", "CryptoAnalysisAnalyser/userdoc.xml", new[] { "CrypWin/images/default.png" })]
    // HOWTO: Change category to one that fits to your plugin. Multiple categories are allowed.
    [ComponentCategory(ComponentCategory.CryptanalysisGeneric)]
    public class TestVectorGenerator : ICrypComponent
    {
        #region Private Variables

        private readonly CryptoAnalysisAnalyserSettings _settings = new CryptoAnalysisAnalyserSettings();
        private string _textInput;
        private int _seedInput;
        private string _plaintextInput;
        private string[] _keyInput;
        private string _bestPlaintextInput;
        private string _bestKeyInput;
        private string _plaintextOutput;
        private string _keyOutput;
        private string _bestPlaintextOutput;
        private string _bestKeyOutput;

        private int _progress;

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "TextInput", "TextInput tooltip description")]
        public string TextInput
        {
            get { return this._textInput; }
            set
            {
                if (_textInput != value)
                {
                    this._textInput = value;
                    OnPropertyChanged("TextInput");
                }
            }
        }

        [PropertyInfo(Direction.InputData, "SeedInput", "SeedInput tooltip description")]
        public string SeedInput
        {
            get { return this._seedInput.ToString(); }
            set
            {
                try
                {
                    int seed = System.Int32.Parse(value);
                    if (_seedInput != seed)
                    {
                        this._seedInput = seed;
                        OnPropertyChanged("SeedInput");
                    }
                }
                catch (System.FormatException)
                {
                    GuiLogMessage(value + ": Bad Format", NotificationLevel.Error);
                }
                catch (System.OverflowException)
                {
                    GuiLogMessage(value + ": Overflow", NotificationLevel.Error);
                }

            }
        }

        [PropertyInfo(Direction.InputData, "KeyInput", "KeyInput tooltip description")]
        public string[] KeyInput
        {
            get { return this._keyInput; }
            set
            {
                // TODO: check if test works and is necessary
                if (_keyInput != value)
                {
                    this._keyInput = value;
                    OnPropertyChanged("KeyInput");
                }
            }
        }

        [PropertyInfo(Direction.InputData, "PlaintextInput", "PlaintextInput tooltip description")]
        public string PlaintextInput
        {
            get { return this._plaintextInput; }
            set
            {
                // TODO: check if test works and is necessary
                if (_plaintextInput != value)
                {
                    this._plaintextInput = value;
                    OnPropertyChanged("PlaintextInput");
                }
            }
        }

        [PropertyInfo(Direction.InputData, "BestKeyInput", "BestKeyInput tooltip description")]
        public string BestKeyInput
        {
            get { return this._bestKeyInput; }
            set
            {
                // TODO: check if test works and is necessary
                if (_bestKeyInput != value)
                {
                    this._bestKeyInput = value;
                    OnPropertyChanged("BestKeyInput");
                }
            }
        }

        [PropertyInfo(Direction.InputData, "BestPlaintextInput", "BestPlaintextInput tooltip description")]
        public string BestPlaintextInput
        {
            get { return this._bestPlaintextInput; }
            set
            {
                // TODO: check if test works and is necessary
                if (_bestPlaintextInput != value)
                {
                    this._bestPlaintextInput = value;
                    OnPropertyChanged("BestPlaintextInput");
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "KeyOutput", "KeyOutput tooltip description")]
        public string KeyOutput
        {
            get { return this._keyOutput; }
            set
            {
                // TODO: check if test works and is necessary
                if (_keyOutput != value)
                {
                    this._keyOutput = value;
                    OnPropertyChanged("KeyOutput");
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "PlaintextOutput", "PlaintextOutput tooltip description")]
        public string PlaintextOutput
        {
            get { return this._plaintextOutput; }
            set
            {
                // TODO: check if test works and is necessary
                if (_plaintextOutput != value)
                {
                    this._plaintextOutput = value;
                    OnPropertyChanged("PlaintextOutput");
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "BestKeyOutput", "BestKeyOutput tooltip description")]
        public string BestKeyOutput
        {
            get { return this._bestKeyOutput; }
            set
            {
                // TODO: check if test works and is necessary
                if (_bestKeyOutput != value)
                {
                    this._bestKeyOutput = value;
                    OnPropertyChanged("BestKeyOutput");
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "BestPlaintextOutput", "BestPlaintextOutput tooltip description")]
        public string BestPlaintextOutput
        {
            get { return this._bestPlaintextOutput; }
            set
            {
                // TODO: check if test works and is necessary
                if (_bestPlaintextOutput != value)
                {
                    this._bestPlaintextOutput = value;
                    OnPropertyChanged("BestPlaintextOutput");
                }
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
            GuiLogMessage("CAA: PreExecution()", NotificationLevel.Balloon);
            _plaintextOutput = "";
            _keyOutput = null;
            _progress = 0;
        }

        public bool checkVariables()
        {
            if (KeyInput.Length == 0 || String.IsNullOrEmpty(KeyInput[0]))
            {
                // TESTING!!!
                List<string> list = new List<string>();
                list.Add("KEYWORDX");
                list.Add("KEYWORD");
                KeyInput = list.ToArray();

                //GuiLogMessage("The key input is empty!", NotificationLevel.Error);
                //return false;
            }

            if (String.IsNullOrEmpty(PlaintextInput))
            {
                GuiLogMessage("The plaintext input is empty!", NotificationLevel.Error);
                return false;
            }

            if (String.IsNullOrEmpty(SeedInput))
            {
                GuiLogMessage("The seed input is empty! It is required for logging purposes.", NotificationLevel.Warning);
            }

            return true;
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            GuiLogMessage("CAA: Execute()", NotificationLevel.Balloon);
            if (!checkVariables())
            {
                GuiLogMessage("CAA: Execute: checkVariables failed!", NotificationLevel.Balloon);
                return;
            }
            ProgressChanged(0, 1);

            GuiLogMessage("CAA: PlaintextInput to PlaintextOutput", NotificationLevel.Debug);
            PlaintextOutput = PlaintextInput;

            foreach (string key in KeyInput)
            {
                GuiLogMessage("Current Key: " + key, NotificationLevel.Info);
                KeyOutput = key;
            }


            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
            GuiLogMessage("Best Key: " + BestKeyInput, NotificationLevel.Info);
            GuiLogMessage("Best Plaintext: " + BestPlaintextInput, NotificationLevel.Info);
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
