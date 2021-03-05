/*
   Copyright 2021 Nils Kopal <Nils.Kopal<at>CrypTool.org

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
using System.ComponentModel;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.VisualCryptography
{
    public enum VisualPattern
    {
        Horizontal,
        Vertical,
        Diagonal,
        HorizontalVertical,
        HorizontalDiagonal,
        VerticalDiagonal,
        HorizontalVerticalDiagonal
    }

    public class VisualCryptographySettings : ISettings
    {
        #region Private Variables

        private int _charactersPerRow = 15;
        private VisualPattern _visualPattern = VisualPattern.Diagonal;

        #endregion

        #region TaskPane Settings

        [TaskPane("Characters per Row", "How many characters should appear in a single row", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 100)]
        public int CharactersPerRow
        {
            get
            {
                return _charactersPerRow;
            }
            set
            {
                if (_charactersPerRow != value)
                {
                    _charactersPerRow = value;
                    OnPropertyChanged("CharactersPerRow");
                }
            }
        }

        [TaskPane("Visual pattern", "Which visual pattern should be used?", null, 2, false, ControlType.ComboBox, 
            new string[] {
                "Horizontal",
                "Vertical",
                "Diagonal",
                "HorizontalVertical",
                "HorizontalDiagonal",
                "VerticalDiagonal",
                "HorizontalVerticalDiagonal"
        })]
        public VisualPattern VisualPattern
        {
            get
            {
                return _visualPattern;
            }
            set
            {
                if (_visualPattern != value)
                {
                    _visualPattern = value;
                    OnPropertyChanged("VisualPattern");
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
