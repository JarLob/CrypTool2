﻿/*                              
   Copyright 2009 Team CrypTool (Armin Wiefels), Uni Duisburg-Essen

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
using Cryptool;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace RegularExpressions
{
    [Author("Armin Wiefels", "wiefels@cryptool.org", "Uni Due", "http://www.uni-due.de")]
    [PluginInfo("Regular Expression Replace", "", "RegularExpressions/Description/RegexReplaceDescript.xaml", new[] { "RegularExpressions/icons/regreplaceicon.png" })]
    [ComponentCategory(ComponentCategory.ToolsDataflow)]
    public class RegularExpressionReplace : ICrypComponent
    {
                
        private RegularExpressionReplaceSettings settings;
        private String input;
        private String output;
        private String pattern;
        private String replacement;

        public RegularExpressionReplace()
        {
            this.settings = new RegularExpressionReplaceSettings();
        }
        #region IPlugin Member

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public ISettings Settings
        {
            get { return this.settings; }
            set { this.settings = (RegularExpressionReplaceSettings)value; }
        }

        public System.Windows.Controls.UserControl Presentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
        }

        public void Execute()
        {
           // pattern = this.settings.PatternValue;
           // replacement = this.settings.ReplaceValue;

            try
            {
                 if (input != null && settings.PatternValue != null && settings.ReplaceValue != null)
                //if (input != null && pattern != null && replacement != null)
                {
                    pattern = this.settings.PatternValue;
                    replacement = this.settings.ReplaceValue;

                    Output = input;
                    Output = Regex.Replace(input, pattern, replacement);
                    ProgressChanged(1, 1);
                }

                else 
                {
                    Output = input;
                    ProgressChanged(1 , 1);
                }
            }
            catch (Exception)
            {
              //  GuiLogMessage("Regular Expression is not valid.", NotificationLevel.Warning);
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

        #region properties
        [PropertyInfo(Direction.InputData, "Input String", "", true)]
        public String Input
        {
            get { return this.input; }
            set
            {
                if (value != null)
                {
                    this.input = (String)value;
                    OnPropertyChange("Input");
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "Output String", "")]
        public String Output
        {
            get { return this.output; }
            set
            {
                this.output = value;
                OnPropertyChange("Output");
            }
        }

        #endregion

        #region INotifyPropertyChanged Member

        public event PropertyChangedEventHandler PropertyChanged;


        public void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChange(String propertyname)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(propertyname));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }


        #endregion

       
    }
}
