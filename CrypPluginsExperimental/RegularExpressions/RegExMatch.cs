﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;

using Cryptool.PluginBase;
using Cryptool.PluginBase.Validation;
using Cryptool;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;

using System.Text.RegularExpressions;

namespace Cryptool.Plugins.RegularExpressions
{
    [Author("Daniel Kohnen", "kohnen@cryptool.org", "Universität Duisburg Essen", "http://www.uni-due.de")]
    [PluginInfo("Regular Expression Match", "Matching Regular Expression", "RegularExpressions/Description/RegexMatchDescript.xaml", new[] { "RegularExpressions/icons/regmatchicon.png"})]
    [ComponentCategory(ComponentCategory.ToolsDataflow)]
    public class RegExMatch : ICrypComponent
    {
        #region Private variables

        private RegExMatchSettings settings;
        private string input;
        private string pattern;
        private string outputText;
        private Boolean outputBool = false;

        # endregion

        # region Public Methods

        public RegExMatch()
        {
                this.settings = new RegExMatchSettings();
        }

        # endregion

        # region Properties

        [PropertyInfo(Direction.InputData, "Input", "Input a string to be processed by the RegEx Matcher", true)]
        public string Input
        {
            get
            {
                return this.input;
            }

            set
            {
                this.input = value;
                OnPropertyChange("Input");
            }
        }

        [PropertyInfo(Direction.InputData, "Pattern", "Pattern for the RegEx", true)]
        public string Pattern
        {
            get
            {
                return this.pattern;
            }

            set
            {
                this.pattern = value;
                OnPropertyChange("Pattern");
            }
        }

        [PropertyInfo(Direction.OutputData, "Output", "Output")]
        public string OutputText
        {
            get
            {
                    return this.outputText;
            }
            set
            {
                this.outputText = value;
                OnPropertyChange("OutputText");
            }
        }

        [PropertyInfo(Direction.OutputData, "Output Bool", "True: pattern matches. False: it does not.")]
        public Boolean OutputBool
        {
            get
            {
                return this.outputBool;       
            }
            set
            {
                this.outputBool = value;
                OnPropertyChange("OutputBool");
            }
        }

        # endregion

        #region IPlugin Member

        public void Dispose()
        {
          
        }

        public void Execute()
        {
            try
            {
                if (input != null && pattern != null)
                {
                    Match match = Regex.Match(input, pattern);
                    if (match.Success)
                    {
                        OutputBool = true;
                        OutputText = match.Value;

                    }

                    else
                    {
                        OutputBool = false;
                        OutputText = "";

                    }
                }

                ProgressChanged(1, 1);
            }
            catch (Exception e)
            {
                //GuiLogMessage("Regular Expression is not valid.", NotificationLevel.Warning);
            }
        }

        public void Initialize()
        {
           
        }

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }
        
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event StatusChangedEventHandler OnPluginStatusChanged;
        
        private void OnPropertyChange(String propertyname)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(propertyname));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public void PostExecution()
        {
            
        }

        public void PreExecution()
        {
            
        }

        public System.Windows.Controls.UserControl Presentation
        {
            get { return null; }
        }

        public ISettings Settings
        {
            get { return this.settings;}
            set { this.settings = (RegExMatchSettings)value; }
        }

        public void Stop()
        {
           
        }

        #endregion

        #region INotifyPropertyChanged Member

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
