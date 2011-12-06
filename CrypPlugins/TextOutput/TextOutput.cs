/*
   Copyright 2008-2011 CrypTool 2 Team <ct2contact@cryptool.org>

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
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Cryptool.PluginBase;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;

namespace TextOutput
{
    [Author("Thomas Schmid", "thomas.schmid@cryptool.org", "Uni Siegen", "http://www.uni-siegen.de")]
    [PluginInfo("TextOutput.Properties.Resources", "PluginCaption", "PluginTooltip", "TextOutput/DetailedDescription/doc.xml", "TextOutput/icon.png")]
    [ComponentCategory(ComponentCategory.ToolsDataInputOutput)]
    public class TextOutput : DependencyObject, ICrypComponent
    {
        #region Fields and properties

        /// <summary>
        /// This dic is used to store error messages while properties are set in PlayMode. The messages
        /// will be sent in the execute method. 
        /// The editor flushes plugin color markers before calling the execute method. 
        /// So these messages would still appear in LogWindow, but the color marker of the 
        /// plugin (red/yellow) would be lost if sending the messages right on property set.
        /// </summary>
        private Dictionary<string, NotificationLevel> dicWarningsAndErros = new Dictionary<string, NotificationLevel>();
        private TextOutputPresentation textOutputPresentation;

        private TextOutputSettings settings;
        public ISettings Settings
        {
            get { return settings; }
            set { settings = (TextOutputSettings)value; }
        }

        private string input;

        [PropertyInfo(Direction.InputData, "InputCaption", "InputTooltip", true)]
        public string Input
        {
            get
            {
                return input;
            }
            set
            {
                try
                {
                    input = value;
                    if (input != null) ShowInPresentation(input);
                    OnPropertyChanged("Input");
                }
                catch(Exception ex)
                {
                    AddMessage(ex.Message, NotificationLevel.Error);
                }
            }
        }

        #endregion

        #region Event handling

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PropertyChangedEventHandler PropertyChanged;
        public event StatusChangedEventHandler OnPluginStatusChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private void settings_OnGuiLogNotificationOccured(IPlugin sender, GuiLogEventArgs args)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(args.Message, this, args.NotificationLevel));
        }

        private void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ( input != null )
                ShowInPresentation(input);
        }

        private void Progress(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        #endregion events

        #region Constructor and implementation

        public TextOutput()
        {
            textOutputPresentation = new TextOutputPresentation();
            settings = new TextOutputSettings(this);
            settings.PropertyChanged += settings_PropertyChanged;
        }

        private void ShowInPresentation(string fillValue)
        {
            if (fillValue == null) return;
            
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                if (settings.Append)
                {
                    // append line breaks only if not first line
                    if (!string.IsNullOrEmpty(textOutputPresentation.textBox.Text))
                    {
                        for (int i = 0; i < settings.AppendBreaks; i++)
                            textOutputPresentation.textBox.AppendText("\n");
                    }
                    textOutputPresentation.textBox.AppendText(fillValue);

                    textOutputPresentation.textBox.ScrollToEnd();
                }
                else
                {
                    textOutputPresentation.textBox.Text = fillValue;
                }

                if (textOutputPresentation.textBox.Text.Length > settings.MaxLength)
                {
                    GuiLogMessage("Text exceeds size limit. Truncating text...", NotificationLevel.Debug);
                    //textOutputPresentation.textBox.Text = string.Empty;
                    textOutputPresentation.textBox.Text = textOutputPresentation.textBox.Text.Substring(0, settings.MaxLength);
                }
                
                int chars = textOutputPresentation.textBox.Text.Length;
                int bytes = Encoding.UTF8.GetBytes(textOutputPresentation.textBox.Text).Length;
                textOutputPresentation.labelBytes.Content = string.Format(Properties.Resources.PresentationFmt, chars, bytes);

            }, fillValue);
        }

        private void AddMessage(string message, NotificationLevel level)
        {
            if (!dicWarningsAndErros.ContainsKey(message))
                dicWarningsAndErros.Add(message, level);
        }

        #endregion

        #region IPlugin Members

        public UserControl Presentation
        {
            get { return textOutputPresentation; }
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        public void Stop()
        {
        }

        public void PreExecution()
        {
            textOutputPresentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                textOutputPresentation.textBox.Text = null;
            }, null);
        }

        public void PostExecution()
        {
        }

        public void Execute()
        {
            Progress(100, 100);
            foreach (KeyValuePair<string, NotificationLevel> kvp in dicWarningsAndErros)
            {
                GuiLogMessage(kvp.Key, kvp.Value);
            }
            dicWarningsAndErros.Clear();
        }

        #endregion
    }
}