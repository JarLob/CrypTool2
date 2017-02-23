/*                              
   Aditya Deshpande, University of Mannheim

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

using System.Text;
using Cryptool.PluginBase;

using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.PluginBase.Miscellaneous;
using System;
using System.Collections.Generic;

namespace Cryptool.Feistel
{
    [Author("Aditya Deshpande", "adeshpan@mail.uni-mannheim.de", "Universität Mannheim", "https://www.uni-mannheim.de/1/")]
    [PluginInfo("Feistel.Properties.Resources", "PluginCaption", "PluginTooltip", "Feistel/userdoc.xml", "Feistel/Images/Feistel.jpg")]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class Feistel : ICrypComponent
    {
        #region Private elements

        private readonly FeistelSettings settings;
        private bool isPlayMode = false;

        #endregion

        #region Public interface

        /// <summary>
        /// Constructor
        /// </summary>
        public Feistel()
        {
            this.settings = new FeistelSettings();
            this.settings.LogMessage += GuiLogMessage;
        }

        /// <summary>
        /// Get or set all settings for this algorithm.
        /// </summary>
        public ISettings Settings
        {
            get { return this.settings; }
        }

        private string _inputString;
        private string _key;
        private int _numberOfRounds;

        [PropertyInfo(Direction.InputData, "InputStringCaption", "InputStringTooltip", true)]
        public string InputString
        {
            get { return _inputString; }
            set { _inputString = value; }
        }

        [PropertyInfo(Direction.InputData, "InputKeyCaption", "InputKeyTooltip", true)]
        public string Key
        {
            get { return _key; }
            set { _key = value; }
        }

        [PropertyInfo(Direction.InputData, "InputRoundsCaption", "InputRoundsTooltip", true)]
        public int numberOfRounds
        {
            get { return _numberOfRounds; }
            set { _numberOfRounds = value; }
        }

        [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", false)]
        public string OutputString
        {
            get;
            set;
        }



        #endregion

        #region IPlugin members
        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        /// <summary>
        /// Fires events to indicate progress bar changes.
        /// </summary>
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        /// <summary>
        /// Fires events to indicate log messages.
        /// </summary>
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        /// <summary>
        /// No algorithm visualization
        /// </summary>
        public UserControl Presentation
        {
            get { return null; }
        }

        public void Stop()
        {
        }

        public void PostExecution()
        {
            isPlayMode = false;
        }

        public void PreExecution()
        {
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Handles re-execution events from settings class
        /// </summary>
        private void Feistel_ReExecute()
        {
            if (isPlayMode)
            {
                Execute();
            }
        }

        #endregion

        #region IPlugin Members

#pragma warning disable 67
        public event StatusChangedEventHandler OnPluginStatusChanged;
#pragma warning restore
        static int[,] s_box = new int[16, 16];


        public void Execute()
        {
            isPlayMode = true;
            string cipherText;
            char[] inputChars = new char[InputString.Length];
            char[] keyChars = new char[Key.Length];
            char[] cipherChars = new char[InputString.Length];
            int[] tempChars = new int[InputString.Length / 2];
            List<int> left = new List<int>();
            List<int> right = new List<int>();            
            char[] outputChars = new char[InputString.Length];

            cipherText = InputString;


            for (int i = 0; i < InputString.Length/2; i++)
            {
             left.Add(InputString[i]);
            }

            for (int i = InputString.Length / 2; i < InputString.Length; i++)
            {
                right.Add(InputString[i]);
            }

            if (!string.IsNullOrEmpty(InputString))
            {
                for (int j = 0; j < numberOfRounds; j++)
                {
                    //char temp = cipherText[0];
                    for(int i=0;i<left.Count;i++)
                    {
                        left[i] = left[i] ^ ((right[i] + Key[i]) % 256);
                    }

                    for (int i = 0; i < left.Count; i++)
                    {
                        tempChars[i] = left[i];
                    }

                    for (int i = 0; i < right.Count; i++)
                    {
                        left[i] = right[i];
                    }

                    for (int i = 0; i < tempChars.Length; i++)
                    {
                        right[i] = tempChars[i];
                    }
                   

                    if (true)
                      {
                            switch (settings.Action)
                           {
                                case FeistelSettings.FeistelMode.Encrypt:

                                    break;
                                    //case FeistelSettings.FeistelMode.Decrypt:

                           }

                       }

                        // Show the progress.
                        ProgressChanged(j, numberOfRounds-1);

                    }
                    
                for(int k=0;k<left.Count;k++)
                {
                    outputChars[k] = (char)left[k];
                }
                int p = left.Count;
                for ( int k=0;k<right.Count;k++)
                {                    
                    outputChars[p] = (char)right[k];
                    p = p + 1;
                }


                }
                string output = new string(outputChars);
                OutputString = output;
                OnPropertyChanged("OutputString");

            }
        }

       

        #endregion

    }

