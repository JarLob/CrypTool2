/*
   Copyright CrypTool 2 Team <ct2contact@cryptool.org>

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

using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;

namespace Cryptool.Plugins.ChaCha
{
    public class ChaChaSettings : ISettings
    {
        #region Private Variables

        private int rounds = 20;
        private Version version;

        #endregion Private Variables

        #region TaskPane Settings

        [TaskPane("RoundCaption", "RoundTooltip", null, 0, false, ControlType.ComboBox, new string[] { "8", "12", "20" })]
        public int Rounds
        {
            get { return rounds; }
            set
            {
                // The CT2 environment calls this setter with the index thus we map the indices to the actual round value.
                // The ChaCha Unittest calls this setter with the actual round value,
                // that's why there is a fallthrough with the actual round value for each case.
                switch (value)
                {
                    case 0:
                    case 8:
                        rounds = 8;
                        break;

                    case 1:
                    case 12:
                        rounds = 12;
                        break;

                    case 2:
                    case 20:
                        rounds = 20;
                        break;
                }
                OnPropertyChanged("Rounds");
            }
        }

        [TaskPane("VersionCaption", "VersionTooltip", null, 0, false, ControlType.ComboBox, new string[] { "DJB", "IETF" })]
        public int IntVersion
        {
            get { return Version.Name == Version.DJB.Name ? 0 : 1; }
            set
            {
                Version intVersion = value == 0 ? Version.DJB : Version.IETF;
                if (Version.Name != intVersion.Name)
                {
                    Version = intVersion;
                    OnPropertyChanged("IntVersion");
                }
            }
        }

        public Version Version
        {
            get
            {
                if (version == null)
                {
                    version = Version.DJB;
                }
                return version;
            }
            private set
            {
                if (version.Name != value.Name)
                {
                    version = value;
                    OnPropertyChanged("Version");
                }
            }
        }

        #endregion TaskPane Settings

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion Events

        public void Initialize()
        {
        }

        public override string ToString()
        {
            return string.Format(Properties.Resources.SettingsToString, Rounds, Version.Name);
        }
    }
}