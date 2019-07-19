using System;
using DCAPathVisualiser.UI.Models;

namespace DCAPathVisualiser.UI.Cipher3
{
    public class Cipher3CharacteristicSelectionEventArgs : EventArgs
    {
        private Cipher3CharacteristicUI _selectedCharacteristic;

        /// <summary>
        /// Constructor
        /// </summary>
        public Cipher3CharacteristicSelectionEventArgs()
        {
            _selectedCharacteristic = new Cipher3CharacteristicUI();
        }

        /// <summary>
        /// Property for _selectedCharacteristic
        /// </summary>
        public Cipher3CharacteristicUI SelectedCharacteristic
        {
            get { return _selectedCharacteristic; }
            set { _selectedCharacteristic = value; }
        }
    }
}
