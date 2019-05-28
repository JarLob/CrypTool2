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

using System;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using ToyCiphers.UI;


namespace Cryptool.Plugins.ToyCiphers
{
    [Author("Christian Bender", "christian1.bender@student.uni-siegen.de", null, "http://www.uni-siegen.de")]
    [PluginInfo("ToyCiphers.Properties.Resources", "PluginCaption", "PluginTooltip", "ToyCiphers/userdoc.xml", new[] { "ToyCiphers/Images/IC_ToyCiphers.png" })]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class ToyCiphers : ICrypComponent
    {
        #region Private Variables

        private readonly ToyCiphersSettings settings = new ToyCiphersSettings();
        private byte[] _singleMessageInput;
        private byte _singleMessageOutput;
        private byte[] _key;
        private ToyCiphersPres _activePresentation = new ToyCiphersPres();

        #endregion

        #region Data Properties

        /// <summary>
        /// Input for a single message
        /// </summary>
        [PropertyInfo(Direction.InputData, "SingleMessageInput", "SingleMessageInputTooltip")]
        public byte[] SingleMessageInput
        {
            get { return _singleMessageInput; }
            set
            {
                _singleMessageInput = value;
                OnPropertyChanged("SingleMessageInput");
            }
        }

        /// <summary>
        /// Output for a single message
        /// </summary>
        [PropertyInfo(Direction.OutputData, "SingleMessageOutput", "SingleMessageOutputTooltip")]
        public byte SingleMessageOutput
        {
            get { return _singleMessageOutput; }
            set
            {
                _singleMessageOutput = value;
                OnPropertyChanged("SingleMessageOutput");
            }
        }

        /// <summary>
        /// Input for the key
        /// </summary>
        [PropertyInfo(Direction.InputData, "KeyInput", "KeyInputTooltip", true)]
        public byte[] KeyInput
        {
            get { return _key; }
            set
            {
                _key = value;
                OnPropertyChanged("KeyInput");
            }
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
            get { return _activePresentation; }
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

        #region methods

        public ToyCiphers()
        {
            settings.PropertyChanged += new PropertyChangedEventHandler(SettingChangedListener);
        }

        /// <summary>
        /// Handles changes within the settings class
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingChangedListener(object sender, PropertyChangedEventArgs e)
        {
            //Listen for changes of the current chosen algorithm
            if (e.PropertyName == "CurrentAlgorithm")
            {
                //Check specific algorithm and invoke the selection into the UI class
                if (settings.CurrentAlgorithm == Algorithms.Cipher1)
                {
                    //dispatch action: clear the active grid and add the specific algorithm visualization
                    _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                    {
                        _activePresentation.MainGrid.Children.Clear();
                        _activePresentation.MainGrid.Children.Add(new Cipher1Pres());
                    }, null);
                }
                else if (settings.CurrentAlgorithm == Algorithms.Cipher2)
                {
                    _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                    {
                        _activePresentation.MainGrid.Children.Clear();
                        _activePresentation.MainGrid.Children.Add(new Cipher2Pres());
                    }, null);
                }
                else if (settings.CurrentAlgorithm == Algorithms.Cipher3)
                {
                    _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                    {
                        _activePresentation.MainGrid.Children.Clear();
                        _activePresentation.MainGrid.Children.Add(new Cipher3Pres());
                    }, null);
                }
                else if (settings.CurrentAlgorithm == Algorithms.Cipher4)
                {
                    _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                    {
                        _activePresentation.MainGrid.Children.Clear();
                        _activePresentation.MainGrid.Children.Add(new Cipher4Pres());
                    }, null);
                }
            }

        }

        #endregion
    }







    /// <summary>
    /// Cipher1 = 16 bit blocksize, 2 subkeys, 32 bit key
    /// Cipher2 = 16 bit blocksize, 4 subkeys, 64 bit key
    /// Cipher3 = 16 bit blocksize, 6 subkeys, 96 bit key
    /// Cipher4 = 4 bit blocksize, 4 subkeys, 16 bit key
    /// </summary>
    public enum Algorithms
    {
        Cipher1,
        Cipher2,
        Cipher3,
        Cipher4
    }
}
