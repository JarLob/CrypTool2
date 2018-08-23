/*
   Copyright 2018 CrypTool 2 Team <ct2contact@cryptool.org>

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
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.BitcoinTransactionDownloader
{
    public class BitcoinTransactionDownloaderSettings : ISettings
    {
        #region Private Variables

        private string hostname = "127.0.0.1";
        private int port = 8080;

        #endregion

        #region TaskPane Settings

        /// <summary>
        /// Server Ip Address for connection
        /// </summary>
        [TaskPane("ServerIP", "This is the server address", null, 1, false, ControlType.TextBox, ValidationType.RegEx, 0, Int32.MaxValue)]
        public string Hostname
        {
            get
            {
                return hostname;
            }
            set
            {
                if (hostname != value)
                {
                    hostname = value;
                    OnPropertyChanged("ServerIP");
                }
            }
        }

        /// <summary>
        /// Server Port for connection
        /// </summary>
        [TaskPane("ServerPort", "This is the server port", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        public int Port
        {
            get
            {
                return port;
            }
            set
            {
                if (port != value)
                {
                    port = value;
                    OnPropertyChanged("ServerPort");
                }
            }
        }


        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion

        public void Initialize()
        {

        }
    }
}
