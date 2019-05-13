/*
   Copyright 2008-2017, Arno Wacker, University of Kassel

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
using System.Linq;
using System.Text;

//additionally needed libs
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows;

// Cryptool 2.0 specific includes
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;


namespace Cryptool.Enigma
{
    public class EnigmaSettings : ISettings
    {
        #region Private variables

        private ObservableCollection<string> rotorAStrings = new ObservableCollection<string>();
        private ObservableCollection<string> rotorBStrings = new ObservableCollection<string>();
        private ObservableCollection<string> reflectorStrings = new ObservableCollection<string>();

        private int model = 3;
        private int unknownSymbolHandling = 0; // 0 = ignore, 1 = leave unmodified
        private int caseHandling = 0; // 0 = preserve, 1 = convert all to upper, 2 = convert all to lower
        private string _initialRotorPos = "AAA";
        private string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        private int rotor1 = 0;
        private int rotor2 = 1;
        private int rotor3 = 2;
        private int rotor4 = 0;

        private int ring1 = 1; // 01 = A, 02 = B ...
        private int ring2 = 1;
        private int ring3 = 1;
        private int ring4 = 1;

        private int reflector = 0;

        private int Presentation_Speed = 1;

        private StringBuilder plugBoard = new StringBuilder("ABCDEFGHIJKLMNOPQRSTUVWXYZ");

        #endregion

        #region Private methods

        private void checkRotorChange(int rotor, int was, int becomes)
        {
            if (rotor1 == becomes) { rotor1 = was; OnPropertyChanged("Rotor1"); }
            if (rotor2 == becomes) { rotor2 = was; OnPropertyChanged("Rotor2"); }
            if (rotor3 == becomes) { rotor3 = was; OnPropertyChanged("Rotor3"); }
        }

        private void setPlugBoard(int letterPos, int newIndex)
        {
            if (newIndex != alphabet.IndexOf(plugBoard[letterPos]))
            {
                char newChar = alphabet[newIndex];
                //int newCharIndex = plugBoard.ToString().IndexOf(newChar);
                char currentChar = plugBoard[letterPos];
                int currentIndex = alphabet.IndexOf(currentChar);
                int preconnect = alphabet.IndexOf(this.plugBoard[newIndex]);

                if (this.plugBoard[preconnect] != alphabet[preconnect])
                {
                    this.plugBoard[preconnect] = alphabet[preconnect];
                    OnPropertyChanged("PlugBoard" + alphabet[preconnect]);  
                }
                this.plugBoard[newIndex] = alphabet[letterPos];
                OnPropertyChanged("PlugBoard" + alphabet[newIndex]);
                if (this.plugBoard[letterPos] != alphabet[letterPos])
                {
                    this.plugBoard[currentIndex] = alphabet[currentIndex];
                    OnPropertyChanged("PlugBoard" + alphabet[currentIndex]);
                
                }
                this.plugBoard[letterPos] = newChar;
                OnPropertyChanged("PlugBoard" + alphabet[letterPos]);
                //OnPropertyChanged("PlugBoardDisplay");
                OnPropertyChanged("PlugBoard");
                
            }
        }

        private void hidePlugBoard()
        {
            foreach (char c in this.alphabet)
                hideSettingsElement("PlugBoard" + c);

            hideSettingsElement("PlugBoard");
            hideSettingsElement("ResetPlugboard");
        }

        private void showPlugBoard()
        {
            foreach (char c in this.alphabet)
                showSettingsElement("PlugBoard" + c);

            showSettingsElement("PlugBoard");
            showSettingsElement("ResetPlugboard");
        }

        private void showSettingsElement(string element)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Visible)));
            }
        }

        private void hideSettingsElement(string element)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Collapsed)));
            }
        }

        #endregion

        #region Public Events and Methods

        /// <summary>
        /// This event is needed in order to render settings elements visible/invisible
        /// </summary>
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        public int AlphabetIndexOf(char c)
        {
            return c - 'A';
        }

        public void HideAllBasicKeySettings()
        {
            hideSettingsElement("Rotor1");
            hideSettingsElement("Rotor2");
            hideSettingsElement("Rotor3");
            hideSettingsElement("Rotor4");
            hideSettingsElement("Ring1");
            hideSettingsElement("Ring2");
            hideSettingsElement("Ring3");
            hideSettingsElement("Ring4");
            hidePlugBoard();
        }

        public void SetKeySettings(string inputKey)
        {
            // delete the spaces
            inputKey = inputKey.Replace(" ", String.Empty);
            inputKey = inputKey.Replace("\n", String.Empty);
            inputKey = inputKey.Replace("\r", String.Empty);

            // count slashes
            int slashes = inputKey.Count(x => x == '/');

            // search slash indices
            int firstSlashIndex = inputKey.IndexOf('/');
            int secondSlashIndex = 0;
            if (slashes > 1)
                secondSlashIndex = (inputKey.Substring(firstSlashIndex + 1)).IndexOf('/') + firstSlashIndex + 1;
            int thirdSlashIndex = 0;
            if (slashes > 2)
                thirdSlashIndex = (inputKey.Substring(secondSlashIndex + 1)).IndexOf('/') + secondSlashIndex + 1;
            
            // trim different parts of the key
            string rotorString = inputKey.Substring(0, firstSlashIndex);
            string ringString = inputKey.Substring(firstSlashIndex + 1);
            if (slashes > 1)
                ringString = inputKey.Substring(firstSlashIndex + 1, secondSlashIndex - firstSlashIndex - 1);

            string plugBoardString = "";
            if (slashes > 1)
                plugBoardString = inputKey.Substring(secondSlashIndex + 1);

            string initialRotorPosString = "";
            if (slashes > 2)
            {
                plugBoardString = inputKey.Substring(secondSlashIndex + 1, thirdSlashIndex - secondSlashIndex - 1);
                initialRotorPosString = inputKey.Substring(thirdSlashIndex + 1);
            }

            // set settings by strings
            SetRotorsByString(rotorString);
            SetRingByString(ringString);
            if (slashes > 1)
                SetPlugBoardByString(plugBoardString);
            if (slashes > 2)
            {
                _initialRotorPos = initialRotorPosString;
                OnPropertyChanged("InitialRotorPos");
            }
        }

        private void SetRotorsByString(string rotorString)
        {
            string[] rotors = rotorString.Split(',');
            rotor1 = GetRotorIndexFromString(rotors[0]);
            rotor2 = GetRotorIndexFromString(rotors[1]);
            rotor3 = GetRotorIndexFromString(rotors[2]);
            if (rotors.Length > 3)
            {
                rotor4 = GetRotorIndexFromString(rotors[3]);
                OnPropertyChanged("Rotor4");
            }

            OnPropertyChanged("Rotor1");
            OnPropertyChanged("Rotor2");
            OnPropertyChanged("Rotor3");
        }

        string[] rotorNames = { "I", "II", "III", "IV", "V", "VI", "VII", "VIII" };
        private int GetRotorIndexFromString(string indexString)
        {
            return Array.IndexOf(rotorNames, indexString);
        }

        private void SetRingByString(string ringString)
        {
            string[] rings = ringString.Split(',');
            
            int value1, value2, value3, value4 = -1;

            if (!Int32.TryParse(rings[0], out value1) ||
                !Int32.TryParse(rings[1], out value2) ||
                !Int32.TryParse(rings[2], out value3) ||
                rings.Length > 3 && !Int32.TryParse(rings[3], out value4))
            {
                Console.WriteLine("Error parsing the InputKey ring settings!");
                return;
            }

            ring1 = value1;
            ring2 = value2;
            ring3 = value3;
            if (rings.Length > 3)
            {
                ring4 = value4;
                OnPropertyChanged("Ring4");
            }

            OnPropertyChanged("Ring1");
            OnPropertyChanged("Ring2");
            OnPropertyChanged("Ring3");
        }

        private void SetPlugBoardByString(string plugBoardString)
        {
            ResetPlugboard();
            string[] plugBoardStringArray = plugBoardString.Split(',');

            for (int i = 0; i < plugBoardStringArray.Length; i++)
            {
                int indexLetterOne = alphabet.IndexOf(plugBoardStringArray[i].Substring(0, 1));
                int indexLetterTwo = alphabet.IndexOf(plugBoardStringArray[i].Substring(1, 1));
                setPlugBoard(indexLetterOne, indexLetterTwo);
                setPlugBoard(indexLetterTwo, indexLetterOne);
            }
        }

        #endregion

        #region Initialisation / Contructor

        public EnigmaSettings()
        {
            SetList(rotorAStrings, "RotorA1", "RotorA2", "RotorA3", "RotorA4", "RotorA5", "RotorA6", "RotorA7", "RotorA8");
            SetList(rotorBStrings, "RotorB1");
            SetList(reflectorStrings, "Reflector1", "Reflector2", "Reflector3");
        }

        public void Initialize()
        {
            hideSettingsElement("Rotor4"); hideSettingsElement("Ring4");
        }

        #endregion

        #region Public properties

        public string Alphabet
        {
            get { return alphabet; }
            set { alphabet = value; }
        }

        #endregion

        #region Taskpane settings

        private void SetList(ObservableCollection<string> coll, params string[] keys)
        {
            coll.Clear();
            foreach (string key in keys)
                coll.Add(typeof(Enigma).GetPluginStringResource(key));
        }
        
        [TaskPane( "ModelTPCaption", "ModelTPTooltip", null, 0, false, ControlType.ComboBox, new string[] { "ModelList1", "ModelList2", "ModelList3", "ModelList4", "ModelList5", "ModelList6", "ModelList7" })]
        [PropertySaveOrder(1)]
        public int Model
        {
            get { return this.model; }
            set
            {
                if (value == model)
                    return;

                this.model = value;
                OnPropertyChanged("Model");

                switch (this.model)
                {
                    case 0: // Enigma A/B
                        SetList(rotorAStrings, "RotorA9", "RotorA10", "RotorA11");
                        SetList(rotorBStrings, "RotorB1");
                        SetList(reflectorStrings, "Reflector10");

                        if (_initialRotorPos.Length > 3) _initialRotorPos = _initialRotorPos.Remove(0, 1);
                        rotor1 = 0; rotor2 = 1; rotor3 = 2; rotor4 = 0;
                        reflector = 0;
                        hideSettingsElement("Rotor4"); hideSettingsElement("Ring4"); hideSettingsElement("Reflector");
                        break;

                    case 1: // Enigma D
                        SetList(rotorAStrings, "RotorA12", "RotorA13", "RotorA14");
                        SetList(rotorBStrings, "RotorB1");
                        SetList(reflectorStrings, "Reflector4");

                        if (_initialRotorPos.Length > 3) _initialRotorPos = _initialRotorPos.Remove(0, 1);
                        rotor1 = 0; rotor2 = 1; rotor3 = 2; rotor4 = 0;
                        reflector = 0;
                        hideSettingsElement("Rotor4"); hideSettingsElement("Ring4"); showSettingsElement("Reflector");
                        break;

                    case 2: // Reichsbahn (Rocket)
                        SetList(rotorAStrings, "RotorA15", "RotorA16", "RotorA17"); // "RotorA4"); //you must add a  Rotor 4 for the challenge on MTC3 (Cascading encryption - Part 3/3)
                        SetList(rotorBStrings, "RotorB1");
                        SetList(reflectorStrings, "Reflector5");

                        if (_initialRotorPos.Length > 3) _initialRotorPos = _initialRotorPos.Remove(0, 1);
                        rotor1 = 0; rotor2 = 1; rotor3 = 2; rotor4 = 0;
                        reflector = 0;
                        break;

                    case 3: // Enigma I / M3
                        SetList(rotorAStrings, "RotorA1", "RotorA2", "RotorA3", "RotorA4", "RotorA5", "RotorA6", "RotorA7", "RotorA8");
                        SetList(rotorBStrings, "RotorB1");
                        SetList(reflectorStrings, "Reflector1", "Reflector2", "Reflector3");

                        if (_initialRotorPos.Length > 3) _initialRotorPos = _initialRotorPos.Remove(0, 1);
                        rotor1 = 0; rotor2 = 1; rotor3 = 2; rotor4 = 0;
                        reflector = 0;
                        break;

                    case 4: // Enigma M4 "Shark"
                        SetList(rotorAStrings, "RotorA1", "RotorA2", "RotorA3", "RotorA4", "RotorA5", "RotorA6", "RotorA7", "RotorA8");
                        SetList(rotorBStrings, "RotorB2", "RotorB3");
                        SetList(reflectorStrings, "Reflector6", "Reflector7");

                        if (_initialRotorPos.Length < 4) _initialRotorPos = "A" + _initialRotorPos;
                        rotor1 = 0; rotor2 = 1; rotor3 = 2; rotor4 = 0;
                        reflector = 0;

                        showSettingsElement("Rotor4"); showSettingsElement("Ring4"); showSettingsElement("Reflector");
                        showPlugBoard();
                        break;

                    case 5: // Enigma K-Model
                        SetList(rotorAStrings, "RotorA18", "RotorA19", "RotorA20");
                        SetList(rotorBStrings, "RotorB1");
                        SetList(reflectorStrings, "Reflector8");

                        if (_initialRotorPos.Length > 3) _initialRotorPos = _initialRotorPos.Remove(0, 1); 
                        rotor1 = 0; rotor2 = 1; rotor3 = 2; rotor4 = 0; 
                        reflector = 0; 

                        hideSettingsElement("Rotor4"); hideSettingsElement("Ring4"); showSettingsElement("Reflector");
                        break;

                    case 6: // Enigam G / Abwehr
                        SetList(rotorAStrings, "RotorA21", "RotorA22", "RotorA23");
                        SetList(rotorBStrings, "RotorB1");
                        SetList(reflectorStrings, "Reflector9");

                        if (_initialRotorPos.Length > 3) _initialRotorPos = _initialRotorPos.Remove(0, 1);
                        rotor1 = 0; rotor2 = 1; rotor3 = 2; rotor4 = 0;
                        reflector = 0;

                        hideSettingsElement("Rotor4"); hideSettingsElement("Ring4"); showSettingsElement("Reflector");
                        break;
                }

                OnPropertyChanged("InitialRotorPos");
                OnPropertyChanged("Rotor1");
                OnPropertyChanged("Rotor2");
                OnPropertyChanged("Rotor3");
                OnPropertyChanged("Rotor4");
                OnPropertyChanged("Reflector");
            }
        }


        [TaskPane( "InitialRotorPosCaption", "InitialRotorPosTooltip",
            null, 1, false, ControlType.TextBox, ValidationType.RegEx, "^[A-Za-z]{3,4}$")]
        public string InitialRotorPos
        {
            get { return this._initialRotorPos.ToUpper(); }
            set
            {
                if (value != _initialRotorPos)
                {
                    this._initialRotorPos = value.ToUpper();
                    OnPropertyChanged("InitialRotorPos");   
                }
            }
        }

        #endregion

        #region Used rotor settings

        [TaskPane( "Rotor1Caption", "Rotor1Tooltip",
            "UsedRotorsGroup", 10, false, ControlType.DynamicComboBox, new string[] { "RotorAStrings" })]
        public int Rotor1
        {
            get { return this.rotor1; }
            set
            {
                if (((int)value) != rotor1)
                {
                    checkRotorChange(1, this.rotor1, value);
                    this.rotor1 = value;
                    OnPropertyChanged("Rotor1");   
                }
            }
        }

        [TaskPane( "Rotor2Caption", "Rotor2Tooltip",
            "UsedRotorsGroup", 11, false, ControlType.DynamicComboBox, new string[] { "RotorAStrings" })]
        public int Rotor2
        {
            get { return this.rotor2; }
            set
            {
                if (((int)value) != rotor2)
                {
                    checkRotorChange(2, this.rotor2, value);
                    this.rotor2 = (int)value;
                    OnPropertyChanged("Rotor2");   
                }
            }
        }

        [TaskPane( "Rotor3Caption", "Rotor3Tooltip",
            "UsedRotorsGroup", 12, false, ControlType.DynamicComboBox, new string[] { "RotorAStrings" })]
        public int Rotor3
        {
            get { return this.rotor3; }
            set
            {
                if (((int)value) != rotor3)
                {
                    checkRotorChange(3, this.rotor3, value);
                    this.rotor3 = (int)value;
                    OnPropertyChanged("Rotor3");   
                }
            }
        }

        [TaskPane( "Rotor4Caption", "Rotor4Tooltip",
            "UsedRotorsGroup", 13, false, ControlType.DynamicComboBox, new string[] { "RotorBStrings" })]
        public int Rotor4
        {
            get { return this.rotor4; }
            set
            {
                if (((int)value) != rotor4)
                {
                    this.rotor4 = (int)value;
                    OnPropertyChanged("Rotor4");   
                }
            }
        }


        [TaskPane( "ReflectorCaption", "ReflectorTooltip",
            "UsedRotorsGroup", 14, false, ControlType.DynamicComboBox, new string[] { "ReflectorStrings" })]
        public int Reflector
        {
            get { return this.reflector; }
            set
            {
                if (((int)value) != reflector)
                {
                    this.reflector = (int)value;
                    OnPropertyChanged("Reflector");   
                }
            }
        }

        /// <summary>
        /// This collection contains the values for the Rotor 1-3 comboboxes.
        /// </summary>
        public ObservableCollection<string> RotorAStrings
        {
            get { return rotorAStrings; }
            set
            {
                if (value != rotorAStrings)
                {
                    rotorAStrings = value;
                    OnPropertyChanged("RotorAStrings");
                }
            }
        }

        /// <summary>
        /// This collection contains the values for the Rotor 4 combobox.
        /// </summary>
        public ObservableCollection<string> RotorBStrings
        {
            get { return rotorBStrings; }
            set
            {
                if (value != rotorBStrings)
                {
                    rotorBStrings = value;
                    OnPropertyChanged("RotorBStrings");
                }
            }
        }

        /// <summary>
        /// This collection contains the values for the Rotor 1-3 comboboxes.
        /// </summary>
        public ObservableCollection<string> ReflectorStrings
        {
            get { return reflectorStrings; }
            set
            {
                if (value != reflectorStrings)
                {
                    reflectorStrings = value;
                    OnPropertyChanged("ReflectorStrings");
                }
            }
        }

        #endregion

        #region Used ring settings

        [TaskPane( "Ring1Caption", "Ring1Tooltip", "RingSettingsGroup", 20, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 26)]
        public int Ring1
        {
            get { return ring1; }
            set
            {
                if (value < ring1)
                {
                    if (value + 1 == ring1 && false) // TODO: always false
                    {
                        ring1 = value;
                        OnPropertyChanged("Ring1down");
                    }
                    else
                    {
                        ring1 = value;
                        OnPropertyChanged("Ring1NewValue");
                    }
                }
                if (value > ring1)
                {
                    if (value == ring1 + 1 && false) // TODO: always false
                    {
                        ring1 = value;
                        OnPropertyChanged("Ring1up");
                    }
                    else
                    {
                        ring1 = value;
                        OnPropertyChanged("Ring1NewValue");
                    }
                }
                OnPropertyChanged("Ring1");
            }
        }

        [TaskPane( "Ring2Caption", "Ring2Tooltip", "RingSettingsGroup", 21, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 26)]
        public int Ring2
        {
            get { return ring2; }
            set
            {
                if (value < ring2)
                {
                    if (value + 1 == ring2 && false) // TODO: always false
                    {
                        ring2 = value;
                        OnPropertyChanged("Ring2down");
                    }
                    else
                    {
                        ring2 = value;
                        OnPropertyChanged("Ring2NewValue");

                    }

                }
                if (value > ring2)
                {
                    if (value == ring2 + 1 && false) // TODO: always false
                    {
                        ring2 = value;
                        OnPropertyChanged("Ring2up");
                    }
                    else
                    {
                        ring2 = value;
                        OnPropertyChanged("Ring2NewValue");
                    }
                }
                OnPropertyChanged("Ring2");
            }
        }

        [TaskPane( "Ring3Caption", "Ring3Tooltip", "RingSettingsGroup", 22, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 26)]
        public int Ring3
        {
            get { return ring3; }
            set
            {
                if (value < ring3)
                {
                    if (value + 1 == ring3 && false) // TODO: always false
                    {
                        ring3 = value;
                        OnPropertyChanged("Ring3down");
                    }
                    else
                    {
                        ring3 = value;
                        OnPropertyChanged("Ring3NewValue");
                    }
                }
                if (value > ring3)
                {
                    if (value == ring3 + 1 && false) // TODO: always false
                    {
                        ring3 = value;
                        OnPropertyChanged("Ring3up");
                    }
                    else
                    {
                        ring3 = value;
                        OnPropertyChanged("Ring3NewValue");
                    }
                }
                OnPropertyChanged("Ring3");
            }
        }

        [TaskPane( "Ring4Caption", "Ring4Tooltip", "RingSettingsGroup", 23, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 26)]
        public int Ring4
        {
            get { return ring4; }
            set
            {
                if (value < ring4)
                {
                    ring4 = value;
                    OnPropertyChanged("Ring4down");
                }
                if (value > ring4)
                {
                    ring4 = value;
                    OnPropertyChanged("Ring4up");
                }
            }
        }

        #endregion

        #region Plugboard settings

        [TaskPane( "PlugBoardCaption", "PlugBoardTooltip", "PlugboardGroup", 30, false, ControlType.TextBoxReadOnly)]
        public string PlugBoard
        {
            get { return plugBoard.ToString(); }
            set {
                    plugBoard = new StringBuilder(value);
                    OnPropertyChanged("PlugBoard");
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Eins")]
        [TaskPane( "A=", "PlugBoardLetterTooltip", "PlugboardGroup", 40, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardA
        {
            get { return alphabet.IndexOf(this.plugBoard[0]); }
            set { setPlugBoard(0, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Eins")]
        [TaskPane( "B=", "PlugBoardLetterTooltip", "PlugboardGroup", 41, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardB
        {
            get { return alphabet.IndexOf(this.plugBoard[1]); }
            set { setPlugBoard(1, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Eins")]
        [TaskPane( "C=", "PlugBoardLetterTooltip", "PlugboardGroup", 42, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardC
        {
            get { return alphabet.IndexOf(this.plugBoard[2]); }
            set { setPlugBoard(2, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane( "D=", "PlugBoardLetterTooltip", "PlugboardGroup", 43, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardD
        {
            get { return alphabet.IndexOf(this.plugBoard[3]); }
            set { setPlugBoard(3, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane( "E=", "PlugBoardLetterTooltip", "PlugboardGroup", 44, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardE
        {
            get { return alphabet.IndexOf(this.plugBoard[4]); }
            set { setPlugBoard(4, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane( "F=", "PlugBoardLetterTooltip", "PlugboardGroup", 45, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardF
        {
            get { return alphabet.IndexOf(this.plugBoard[5]); }
            set { setPlugBoard(5, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Drei")]
        [TaskPane( "G=", "PlugBoardLetterTooltip", "PlugboardGroup", 46, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardG
        {
            get { return alphabet.IndexOf(this.plugBoard[6]); }
            set { setPlugBoard(6, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Drei")]
        [TaskPane( "H=", "PlugBoardLetterTooltip", "PlugboardGroup", 47, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardH
        {
            get { return alphabet.IndexOf(this.plugBoard[7]); }
            set { setPlugBoard(7, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Drei")]
        [TaskPane( "I=", "PlugBoardLetterTooltip", "PlugboardGroup", 48, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardI
        {
            get { return alphabet.IndexOf(this.plugBoard[8]); }
            set { setPlugBoard(8, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Vier")]
        [TaskPane( "J=", "PlugBoardLetterTooltip", "PlugboardGroup", 49, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardJ
        {
            get { return alphabet.IndexOf(this.plugBoard[9]); }
            set { setPlugBoard(9, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Vier")]
        [TaskPane( "K=", "PlugBoardLetterTooltip", "PlugboardGroup", 50, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardK
        {
            get { return alphabet.IndexOf(this.plugBoard[10]); }
            set { setPlugBoard(10, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Vier")]
        [TaskPane( "L=", "PlugBoardLetterTooltip", "PlugboardGroup", 51, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardL
        {
            get { return alphabet.IndexOf(this.plugBoard[11]); }
            set { setPlugBoard(11, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Fuenf")]
        [TaskPane( "M=", "PlugBoardLetterTooltip", "PlugboardGroup", 52, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardM
        {
            get { return alphabet.IndexOf(this.plugBoard[12]); }
            set { setPlugBoard(12, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Fuenf")]
        [TaskPane( "N=", "PlugBoardLetterTooltip", "PlugboardGroup", 53, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardN
        {
            get { return alphabet.IndexOf(this.plugBoard[13]); }
            set { setPlugBoard(13, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Fuenf")]
        [TaskPane( "O=", "PlugBoardLetterTooltip", "PlugboardGroup", 54, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardO
        {
            get { return alphabet.IndexOf(this.plugBoard[14]); }
            set { setPlugBoard(14, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sechs")]
        [TaskPane( "P=", "PlugBoardLetterTooltip", "PlugboardGroup", 55, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardP
        {
            get { return alphabet.IndexOf(this.plugBoard[15]); }
            set { setPlugBoard(15, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sechs")]
        [TaskPane( "Q=", "PlugBoardLetterTooltip", "PlugboardGroup", 56, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardQ
        {
            get { return alphabet.IndexOf(this.plugBoard[16]); }
            set { setPlugBoard(16, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sechs")]
        [TaskPane( "R=", "PlugBoardLetterTooltip", "PlugboardGroup", 57, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardR
        {
            get { return alphabet.IndexOf(this.plugBoard[17]); }
            set { setPlugBoard(17, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sieben")]
        [TaskPane( "S=", "PlugBoardLetterTooltip", "PlugboardGroup", 58, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardS
        {
            get { return alphabet.IndexOf(this.plugBoard[18]); }
            set { setPlugBoard(18, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sieben")]
        [TaskPane( "T=", "PlugBoardLetterTooltip", "PlugboardGroup", 59, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardT
        {
            get { return alphabet.IndexOf(this.plugBoard[19]); }
            set { setPlugBoard(19, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sieben")]
        [TaskPane( "U=", "PlugBoardLetterTooltip", "PlugboardGroup", 60, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardU
        {
            get { return alphabet.IndexOf(this.plugBoard[20]); }
            set { setPlugBoard(20, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Acht")]
        [TaskPane( "V=", "PlugBoardLetterTooltip", "PlugboardGroup", 61, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardV
        {
            get { return alphabet.IndexOf(this.plugBoard[21]); }
            set { setPlugBoard(21, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Acht")]
        [TaskPane( "W=", "PlugBoardLetterTooltip", "PlugboardGroup", 62, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardW
        {
            get { return alphabet.IndexOf(this.plugBoard[22]); }
            set { setPlugBoard(22, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Acht")]
        [TaskPane( "X=", "PlugBoardLetterTooltip", "PlugboardGroup", 63, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardX
        {
            get { return alphabet.IndexOf(this.plugBoard[23]); }
            set { setPlugBoard(23, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Neun")]
        [TaskPane( "Y=", "PlugBoardLetterTooltip", "PlugboardGroup", 64, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardY
        {
            get { return alphabet.IndexOf(this.plugBoard[24]); }
            set { setPlugBoard(24, value); }
        }

        [SettingsFormat(1, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Neun")]
        [TaskPane( "Z=", "PlugBoardLetterTooltip", "PlugboardGroup", 65, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardZ
        {
            get { return alphabet.IndexOf(this.plugBoard[25]); }
            set { setPlugBoard(25, value); }
        }


        [TaskPane( "ResetPlugboardCaption", "ResetPlugboardTooltip", "PlugboardGroup", 70, false, ControlType.Button)]
        public void ResetPlugboard()
        {
            plugBoard = new StringBuilder("ABCDEFGHIJKLMNOPQRSTUVWXYZ");            
            foreach (char c in this.alphabet)
            {
                OnPropertyChanged("PlugBoard" + c);
            }

            OnPropertyChanged("PlugBoard");

            // Are the following needed? For the presentation? indeed
            //OnPropertyChanged("PlugBoardDisplay");
            //OnPropertyChanged("Remove all Plugs");
        }

        [TaskPane( "PresentationSpeedCaption", "PresentationSpeedTooltip", "PresentationGroup", 71, true, ControlType.Slider, 2, 25)]
        public int PresentationSpeed
        {
            get { return (int)Presentation_Speed; }
            set
            {
                if ((value) != Presentation_Speed)
                {
                    Presentation_Speed = value;
                    OnPropertyChanged("PresentationSpeed");   
                }
            }
        }

        #endregion

        #region Text options

        [TaskPane("UnknownSymbolHandlingCaption", "UnknownSymbolHandlingTooltip",
            "TextOptionsGroup", 3, false, ControlType.ComboBox,
            new string[] { "UnknownSymbolHandlingList1", "UnknownSymbolHandlingList2", "UnknownSymbolHandlingList3" })]
        public int UnknownSymbolHandling
        {
            get { return this.unknownSymbolHandling; }
            set
            {
                if ((int)value != unknownSymbolHandling)
                {
                    this.unknownSymbolHandling = (int)value;
                    OnPropertyChanged("UnknownSymbolHandling");
                }
            }
        }
        
        [TaskPane("CaseHandlingCaption", "CaseHandlingTooltip",
            "TextOptionsGroup", 4, false, ControlType.ComboBox,
            new string[] { "CaseHandlingList1", "CaseHandlingList2", "CaseHandlingList3" })]
        public int CaseHandling
        {
            get { return this.caseHandling; }
            set
            {
                if ((int)value != caseHandling)
                {
                    this.caseHandling = (int)value;
                    OnPropertyChanged("CaseHandling");
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Member

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}