﻿/*                              
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

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Cryptool.PluginBase;

namespace Cryptool.SlideAttackOnFeistel
{
    public class SlideAttackOnFeistelSettings : ISettings
    {
        #region Public SlideAttackOnFeistel specific interface

        /// <summary>
        /// We use this delegate to send log messages from the settings class to the SlideAttackOnFeistel plugin
        /// </summary>
        public delegate void SlideAttackOnFeistelLogMessage(string msg, NotificationLevel loglevel);

        public enum SlideAttackOnFeistelMode { Encrypt = 0, Decrypt = 1 };

        /// <summary>
        /// An enumaration for the different modes of dealing with unknown characters
        /// </summary>
      //  public enum UnknownSymbolHandlingMode { Ignore = 0, Remove = 1, Replace = 2 };


        /// <summary>
        /// Feuern, wenn ein neuer Text im Statusbar angezeigt werden soll.
        /// </summary>
        public event SlideAttackOnFeistelLogMessage LogMessage;

        #endregion

        #region Private variables and public constructor

        private SlideAttackOnFeistelMode selectedAction = SlideAttackOnFeistelMode.Encrypt;


        public SlideAttackOnFeistelSettings()
        {

        }

        #endregion

        #region Private methods

        private void OnLogMessage(string msg, NotificationLevel level)
        {
            if (LogMessage != null)
                LogMessage(msg, level);
        }

        //private string removeEqualChars(string value)
        //{
        //    int length = value.Length;

        //    for (int i = 0; i < length; i++)
        //    {
        //        for (int j = i + 1; j < length; j++)
        //        {
        //            if ((value[i] == value[j]) || (!CaseSensitive & (char.ToUpper(value[i]) == char.ToUpper(value[j]))))
        //            {
        //                OnLogMessage("Removing duplicate letter: \'" + value[j] + "\' from alphabet!", NotificationLevel.Warning);
        //                value = value.Remove(j, 1);
        //                j--;
        //                length--;
        //            }
        //        }
        //    }

        //    return value;
        //}

        /// <summary>
        /// Set the new shiftValue and the new shiftString to offset % alphabet.Length
        /// </summary>
        //public void SetKeyByValue(int offset, bool firePropertyChanges = true)
        //{
        //    // making sure the shift value lies within the alphabet range      
        //    shiftValue = ((offset % alphabet.Length) + alphabet.Length) % alphabet.Length;
        //    shiftString = "A -> " + alphabet[shiftValue];

        //    // Anounnce this to the settings pane
        //    if (firePropertyChanges)
        //    {
        //        OnPropertyChanged("ShiftValue");
        //        OnPropertyChanged("ShiftString");
        //    }
        //    // print some info in the log.
        //    OnLogMessage("Accepted new shift value: " + offset, NotificationLevel.Debug);
        //}

        #endregion

        #region Algorithm settings properties (visible in the Settings pane)

        //[PropertySaveOrder(4)]
        [TaskPane("ActionTPCaption", "ActionTPTooltip", null, 1, false, ControlType.ComboBox, new string[] { })]
        public SlideAttackOnFeistelMode Action
        {
            get
            {
               return this.selectedAction;
            }
            set
            {
                if (value != selectedAction)
                {
                    this.selectedAction = value;
                    OnPropertyChanged("Action");
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}
