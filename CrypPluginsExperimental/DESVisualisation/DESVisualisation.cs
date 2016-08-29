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

using System;
using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.Threading;
using System.Windows.Threading;

namespace Cryptool.DESVisualisation
{

    // HOWTO: Change author name, email address, organization and URL.
    [Author("Lars Hoffmann", "lars.hoff94@gmail.com", "institute", "http://cryptool2.vs.uni-due.de")]
    // HOWTO: Change plugin caption (title to appear in CT2) and tooltip.
    [PluginInfo("DESVisualisation.Properties.Resources", "DESVisualisationCaption", "DESVisualisationTooltip", "DESVisualisation/userdoc.xml", new[] { "DESVisualisation/images/icon.png" })]
    // HOWTO: Change category to one that fits to your plugin. Multiple categories are allowed.
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class DESVisualisation : ICrypComponent
    {
        
        // Constructor
        public DESVisualisation()
        {
            pres = new DESPresentation();
            isRunning = false;
        }

        #region Private Variables

        private byte[] text;
        private byte[] key;
        private byte[] output;
        private DESPresentation pres;
        private bool isRunning;

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "inputKeyName", "inputKeyDescription", true)]
        public byte[] Key
        {
            get
            {
                return key;
            }
            set
            {
                this.key = value;
                OnPropertyChanged("Key");
            }
        }

        [PropertyInfo(Direction.InputData, "inputTextName", "inputTextDescription", true)]
        public byte[] Text
        {
            get
            {
                return text;
            }
            set
            {
                this.text = value;
                OnPropertyChanged("Text");
            }
        }

        [PropertyInfo(Direction.OutputData, "outputCiphertextName", "outputCiphertextDescription", false)]
        public byte[] Ciphertext
        {
            get
            {
                return output;
            }
            set
            {
                // empty
            }
        }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings
        {
            get { return null; }
        }

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation
        {
            get { return pres; }
            private set { pres = (DESPresentation)value; }
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
            isRunning = true;
            pres.encOriginal = new DESImplementation(key, text);
            try
            {
                pres.encOriginal.DES();
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.ActivateNavigationButtons(true);
                    //pres.ShowChapterScreen(1);
                }, null);
                output = pres.encOriginal.outputCiphertext;
                Ciphertext = new byte[8];
                output.CopyTo(Ciphertext,0);
                OnPropertyChanged("Ciphertext");
                if (pres.IsVisible)
                {
                    while (isRunning)
                    {
                        ProgressChanged(pres.progress, 1);
                        if (pres.nextScreenID == 20)
                        {
                            isRunning = false;
                        }
                    }
                }

            }
            catch (Exception e)
            {
                GuiLogMessage(e.Message,NotificationLevel.Error);
            }

            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
            ProgressChanged(0, 1);
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                if (pres.playTimer.IsEnabled)
                {
                    pres.playTimer.Stop();
                    pres.AutoTButton.IsChecked = false;
                }
                pres.SetInitialState();

            }, null);
            isRunning = false;
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
