/*
   Copyright 2011 Matth�us Wander, University of Duisburg-Essen

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
using Cryptool.PluginBase.Attributes;

namespace Cryptool.Progress
{
    [Author("Sven Rech", "rech@cryptool.org", "University of Kassel", "http://www.uni-kassel.de/eecs/fachgebiete/uc")]
    [PluginInfo("Cryptool.Progress.Properties.Resources", "PluginCaption", "PluginTooltip", "Progress/DetailedDescription/doc.xml",
      "Progress/Images/icon.png")]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    [ComponentVisualAppearance(ComponentVisualAppearance.VisualAppearanceEnum.Opened)]
    [AutoAssumeFullEndProgress(false)]
    public class Progress : ICrypComponent
    {
        private int _value;
        private int _max;
        private ProgressPresentation _progressPresentation = new ProgressPresentation();

        /// <summary>
        /// Get or set all settings for this algorithm
        /// </summary>
        public ISettings Settings
        {
            get { return null; }
        }

        [PropertyInfo(Direction.InputData, "ValueCaption", "ValueTooltip", true)]
        public int Value
        {
            get { return this._value; }
            set
            {
                this._value = value;
                OnPropertyChanged("Value");
            }
        }

        [PropertyInfo(Direction.InputData, "MaxCaption", "MaxTooltip", true)]
        public int Max
        {
            get { return this._max; }
            set
            {
                this._max = value;
                OnPropertyChanged("Max");
            }
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        /// <summary>
        /// Fire, if progress bar has to be updated
        /// </summary>
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private void ProgressChange(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        /// <summary>
        /// Fire, if new message has to be shown in the status bar
        /// </summary>
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public UserControl Presentation
        {
            get { return _progressPresentation; }
        }

        public void Stop()
        {
            _progressPresentation.Set(0, 1);
        }

        public void PostExecution()
        {
        }

        public void PreExecution()
        {
            _progressPresentation.Set(0, 1);
        }

#pragma warning disable 67
        public event StatusChangedEventHandler OnPluginStatusChanged;
#pragma warning restore

        public void Execute()
        {
            _progressPresentation.Set(Value, Max);
            ProgressChange(Value,Max);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        private void GuiLogMessage(string msg, NotificationLevel logLevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(msg, this, logLevel));
            }
        }
    }
}
