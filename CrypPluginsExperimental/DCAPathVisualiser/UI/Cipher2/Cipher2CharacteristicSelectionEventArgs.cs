using System;
using DCAPathVisualiser.UI.Models;

namespace DCAPathVisualiser.UI.Cipher2
{
    public class Cipher2CharacteristicSelectionEventArgs : EventArgs
    {
        private Cipher2CharacteristicUI _selectedCharacteristic;

        /// <summary>
        /// Constructor
        /// </summary>
        public Cipher2CharacteristicSelectionEventArgs()
        {
            _selectedCharacteristic = new Cipher2CharacteristicUI();
        }

        /// <summary>
        /// Property for _selectedCharacteristic
        /// </summary>
        public Cipher2CharacteristicUI SelectedCharacteristic
        {
            get { return _selectedCharacteristic; }
            set { _selectedCharacteristic = value; }
        }
    }
}
