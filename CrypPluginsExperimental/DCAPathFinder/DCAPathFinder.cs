﻿/*
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
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using DCAPathFinder;
using DCAPathFinder.UI;

namespace Cryptool.Plugins.DCAPathFinder
{
    [Author("Christian Bender", "christian1.bender@student.uni-siegen.de", null, "http://www.uni-siegen.de")]
    [PluginInfo("DCAPathFinder.Properties.Resources", "PluginCaption", "PluginTooltip", "DCAPathFinder/userdoc.xml", new[] { "DCAPathFinder/Images/IC_DCAPathFinder.png" })]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class DCAPathFinder : ICrypComponent
    {
        #region Private Variables

        private readonly DCAPathFinderSettings settings = new DCAPathFinderSettings();
        private readonly DCAPathFinderPres _activePresentation = new DCAPathFinderPres();
        private int _expectedDifferential;
        private int _messageCount;
        private string _path;

        #endregion

        public DCAPathFinder()
        {
            settings.PropertyChanged += new PropertyChangedEventHandler(SettingChangedListener);
        }

        #region Data Properties

        /// <summary>
        /// This output describes the characteristic path
        /// </summary>
        [PropertyInfo(Direction.OutputData, "Path", "PathToolTip")]
        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                OnPropertyChanged("Path");
            }
        }

        /// <summary>
        /// This output describes the expected value
        /// </summary>
        [PropertyInfo(Direction.OutputData, "ExpectedDifferential", "ExpectedDifferentialToolTip")]
        public int ExpectedDifferential
        {
            get { return _expectedDifferential; }
            set
            {
                _expectedDifferential = value;
                OnPropertyChanged("ExpectedDifferential");
            }
        }

        /// <summary>
        /// This output describes the count of messages to generate
        /// </summary>
        [PropertyInfo(Direction.OutputData, "MessageCount", "MessageCountToolTip")]
        public int MessageCount
        {
            get { return _messageCount; }
            set
            {
                _messageCount = value;
                OnPropertyChanged("MessageCount");
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
            //dispatch action: inform ui that workspace is running
            _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
            {
                _activePresentation.WorkspaceRunning = true;
                _activePresentation.PresentationMode = settings.PresentationMode;
                _activePresentation.SlideCounterVisibility = Visibility.Visible;
            }, null);
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            // HOWTO: Use this to show the progress of a plugin algorithm execution in the editor.
            ProgressChanged(0, 1);

            //dispatch action {DEBUG}: show slide 8 to save time
            _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
            {
                _activePresentation.StepCounter = 0;
                _activePresentation.SetupView();
            }, null);


            

            // HOWTO: Make sure the progress bar is at maximum when your Execute() finished successfully.
            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
            //dispatch action: inform ui that workspace is stopped
            _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
            {
                _activePresentation.SlideCounterVisibility = Visibility.Hidden;
                _activePresentation.WorkspaceRunning = false;
            }, null);
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

        #region methods

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
                    //dispatch action: set active tutorial number
                    _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                    {
                        _activePresentation.TutorialNumber = 1;
                    }, null);
                }
                else if (settings.CurrentAlgorithm == Algorithms.Cipher2)
                {
                    //dispatch action: set active tutorial number
                    _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                    {
                        _activePresentation.TutorialNumber = 2;
                    }, null);
                }
                else if (settings.CurrentAlgorithm == Algorithms.Cipher3)
                {
                    //dispatch action: set active tutorial number
                    _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                    {
                        _activePresentation.TutorialNumber = 3;
                    }, null);
                }
                else if (settings.CurrentAlgorithm == Algorithms.Cipher4)
                {
                    //dispatch action: set active tutorial number
                    _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                    {
                        _activePresentation.TutorialNumber = 4;
                    }, null);
                }
            }
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