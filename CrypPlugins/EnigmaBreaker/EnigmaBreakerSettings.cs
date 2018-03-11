﻿/*
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

//additionally needed libs
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows;

// Cryptool 2.0 specific includes
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using Cryptool.PluginBase.Attributes;


namespace Cryptool.EnigmaBreaker
{
    public class EnigmaBreakerSettings : ISettings
    {
        #region Private variables

        private ObservableCollection<string> rotorAStrings = new ObservableCollection<string>();
        private ObservableCollection<string> rotorBStrings = new ObservableCollection<string>();
        private ObservableCollection<string> reflectorStrings = new ObservableCollection<string>();
        private int model = 3;
        private int unknownSymbolHandling = 0; // 0=ignore, leave unmodified
        private int caseHandling = 0; // 0=preserve, 1, convert all to upper, 2= convert all to lower
        private string _initialRotorPos = "AAA";
        private string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        private bool analyzeRotors = true;
        private bool _analyzeInitialRotorPos = true;
        private bool analyzeRings = true;
        private bool analyzePlugs = true;
        private bool analysisUseRotorI = true;
        private bool analysisUseRotorII = true;
        private bool analysisUseRotorIII = true;
        private bool analysisUseRotorIV = true;
        private bool analysisUseRotorV = true;
        private bool analysisUseRotorVI = true;
        private bool analysisUseRotorVII = true;
        private bool analysisUseRotorVIII = true;

        private int _SearchMethod = 0;
        private int maxSearchedPlugs = 10;
        private int plugSearchMethod = 2;

        private int rotor1 = 0;
        private int rotor2 = 1;
        private int rotor3 = 2;
        private int rotor4 = 0;

        private int ring1 = 1; // 01 = A, 02 = B ...
        private int ring2 = 1;
        private int ring3 = 1;
        private int ring4 = 1;

        private int reflector = 0;

        private StringBuilder plugBoard = new StringBuilder("ABCDEFGHIJKLMNOPQRSTUVWXYZ");

        // EVALUATION!
        private bool _stopIfPercentReached = false;
        private int _comparisonFrequency = 1;

        #endregion

        #region Private methods

        private void checkRotorChange(int rotor, int was, int becomes)
        {
            switch (rotor)
            {
                case 1:
                    if (rotor2 == becomes) { rotor2 = was; OnPropertyChanged("Rotor2"); }
                    if (rotor3 == becomes) { rotor3 = was; OnPropertyChanged("Rotor3"); }
                    break;
                case 2:
                    if (rotor1 == becomes) { rotor1 = was; OnPropertyChanged("Rotor1"); }
                    if (rotor3 == becomes) { rotor3 = was; OnPropertyChanged("Rotor3"); }
                    break;
                case 3:
                    if (rotor1 == becomes) { rotor1 = was; OnPropertyChanged("Rotor1"); }
                    if (rotor2 == becomes) { rotor2 = was; OnPropertyChanged("Rotor2"); }
                    break;
            }
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

        private void setSettingsVisibility()
        {
            switch (this.model)
            {
                case 2: // Enigma Rocket - supports analysis

                    //hide unused elements
                    hideSettingsElement("Rotor4");
                    hideSettingsElement("Ring4");

                    // show elements common for analysis and encryption
                    showSettingsElement("Reflector");

                    showSettingsElement("AnalyzeInitialRotorPos");
                    showSettingsElement("AnalyzeRotors");
                    showSettingsElement("AnalysisUseRotorI");
                    showSettingsElement("AnalysisUseRotorII");
                    showSettingsElement("AnalysisUseRotorIII");
                    showSettingsElement("AnalyzeRings");
                    showSettingsElement("SearchMethod");
                            
                    // hide unused rotors
                    hideSettingsElement("AnalysisUseRotorIV");
                    hideSettingsElement("AnalysisUseRotorV");
                    hideSettingsElement("AnalysisUseRotorVI");
                    hideSettingsElement("AnalysisUseRotorVII");
                    hideSettingsElement("AnalysisUseRotorVIII");

                    // make sure, the hidden rotors are not selected
                    analysisUseRotorIV      = false;
                    analysisUseRotorV       = false;
                    analysisUseRotorVI      = false;
                    analysisUseRotorVII     = false;
                    analysisUseRotorVIII    = false;   
                         
                    // hide possibility to analyze plugboard, since the Rocket did not have a plugboard
                    hideSettingsElement("AnalyzePlugs");

                    //make sure that anaylzing plugboard is not selected
                    analyzePlugs            = false;

                    // make sure, the plgboard is not visible and it is reset (technically, there would be no problem using the plugboard with the Rocket)
                    hidePlugBoard();
                    ResetPlugboard();

                    // hide also settings related to the plugboard
                    hideSettingsElement("MaxSearchedPlugs");
                    hideSettingsElement("PlugSearchMethod");

 
                    // now check which analysis options are active and hide those settings which are automatically determined
                    if (this._analyzeInitialRotorPos)
                    {
                        hideSettingsElement("InitialRotorPos");
                    }
                    else
                    {
                        showSettingsElement("InitialRotorPos");
                    }

                    if (this.analyzeRotors)
                    {
                        hideSettingsElement("Rotor1");
                        hideSettingsElement("Rotor2");
                        hideSettingsElement("Rotor3");
                    }
                    else
                    {
                        showSettingsElement("Rotor1");
                        showSettingsElement("Rotor2");
                        showSettingsElement("Rotor3");
                    }

                    if (this.analyzeRings)
                    {
                        hideSettingsElement("Ring1");
                        hideSettingsElement("Ring2");
                        hideSettingsElement("Ring3");
                    }
                    else
                    {
                        showSettingsElement("Ring1");
                        showSettingsElement("Ring2");
                        showSettingsElement("Ring3");
                    }
                    break;

                case 3: // Enigma M3 - supports analysis

                    //hide unused elements
                    hideSettingsElement("Rotor4");
                    hideSettingsElement("Ring4");

                    // show elements common for analysis and encryption
                    showSettingsElement("Reflector");
                    
                    showSettingsElement("AnalyzeInitialRotorPos");
                    showSettingsElement("AnalyzeRotors");
                    showSettingsElement("AnalysisUseRotorI");
                    showSettingsElement("AnalysisUseRotorII");
                    showSettingsElement("AnalysisUseRotorIII");
                    showSettingsElement("AnalysisUseRotorIV");
                    showSettingsElement("AnalysisUseRotorV");
                    showSettingsElement("AnalysisUseRotorVI");
                    showSettingsElement("AnalysisUseRotorVII");
                    showSettingsElement("AnalysisUseRotorVIII");
                    showSettingsElement("AnalyzeRings");
                    showSettingsElement("SearchMethod");
                    showSettingsElement("AnalyzePlugs");
                    showSettingsElement("MaxSearchedPlugs");
                    showSettingsElement("PlugSearchMethod");

                    // now check which analysis options are active and hide those settings which are automatically determined
                    if (this._analyzeInitialRotorPos)
                    {
                        hideSettingsElement("InitialRotorPos");
                    }
                    else
                    {
                        showSettingsElement("InitialRotorPos");
                    }

                    if (this.analyzeRotors)
                    {
                        hideSettingsElement("Rotor1");
                        hideSettingsElement("Rotor2");
                        hideSettingsElement("Rotor3");
                    }
                    else
                    {
                        showSettingsElement("Rotor1");
                        showSettingsElement("Rotor2");
                        showSettingsElement("Rotor3");
                    }

                    if (this.analyzeRings)
                    {
                        hideSettingsElement("Ring1");
                        hideSettingsElement("Ring2");
                        hideSettingsElement("Ring3");
                    }
                    else
                    {
                        showSettingsElement("Ring1");
                        showSettingsElement("Ring2");
                        showSettingsElement("Ring3");
                    }

                    if (this.analyzePlugs)
                    {
                        hidePlugBoard();
                    }
                    else
                    {
                        showPlugBoard();
                    }
                    break;

                default:
                    // make sure, that everything is visible
                    showSettingsElement("InitialRotorPos");
                    showSettingsElement("Rotor1");
                    showSettingsElement("Rotor2");
                    showSettingsElement("Rotor3");
                    showSettingsElement("Ring1");
                    showSettingsElement("Ring2");
                    showSettingsElement("Ring3");

                    // hide all options related to analysis
                    hideSettingsElement("AnalyzeInitialRotorPos");
                    hideSettingsElement("AnalyzeRotors");
                    hideSettingsElement("AnalysisUseRotorI");
                    hideSettingsElement("AnalysisUseRotorII");
                    hideSettingsElement("AnalysisUseRotorIII");
                    hideSettingsElement("AnalysisUseRotorIV");
                    hideSettingsElement("AnalysisUseRotorV");
                    hideSettingsElement("AnalysisUseRotorVI");
                    hideSettingsElement("AnalysisUseRotorVII");
                    hideSettingsElement("AnalysisUseRotorVIII");
                    hideSettingsElement("AnalyzeRings");
                    hideSettingsElement("SearchMethod");
                    hideSettingsElement("AnalyzePlugs");
                    hideSettingsElement("MaxSearchedPlugs");
                    hideSettingsElement("PlugSearchMethod");
                    break;
            }
        }

        private void hidePlugBoard()
        {
            foreach (char c in this.alphabet)
            {
                hideSettingsElement("PlugBoard" + c);
            }
            hideSettingsElement("PlugBoard");
            hideSettingsElement("ResetPlugboard");
        }

        private void showPlugBoard()
        {
            foreach (char c in this.alphabet)
            {
                showSettingsElement("PlugBoard" + c);
            }
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

        #endregion

        #region Initialisation / Contructor

        public EnigmaBreakerSettings()
        {
            SetList(rotorAStrings, "RotorA1", "RotorA2", "RotorA3", "RotorA4", "RotorA5", "RotorA6", "RotorA7", "RotorA8");
            SetList(rotorBStrings, "RotorB1");
            SetList(reflectorStrings, "Reflector1", "Reflector2", "Reflector3");
        }

        public void Initialize()
        {
            hideSettingsElement("Rotor4"); hideSettingsElement("Ring4");
            setSettingsVisibility();
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
                coll.Add(typeof(EnigmaBreaker).GetPluginStringResource(key));
        }
        
        [TaskPane( "ModelTPCaption", "ModelTPTooltip",
            null, 0, false, ControlType.ComboBox,
            new string[] { "ModelList1", "ModelList2", "ModelList3", "ModelList4", "ModelList5", "ModelList6", "ModelList7" })]
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
                    case 2: // Reichsbahn (Rocket)
                        SetList(rotorAStrings, "RotorA15", "RotorA16", "RotorA17"); // "RotorA4"); //you must add a  Rotor 4 for the challenge on MTC3 (Cascading encryption - Part 3/3)
                        SetList(rotorBStrings, "RotorB1");
                        SetList(reflectorStrings, "Reflector5");

                        if (_initialRotorPos.Length > 3) _initialRotorPos = _initialRotorPos.Remove(0, 1);
                        rotor1 = 0; rotor2 = 1; rotor3 = 2; rotor4 = 0;
                        reflector = 0;

                        setSettingsVisibility();
                        break;
                    case 3: // Enigma I / M3
                        SetList(rotorAStrings, "RotorA1", "RotorA2", "RotorA3", "RotorA4", "RotorA5", "RotorA6", "RotorA7", "RotorA8");
                        SetList(rotorBStrings, "RotorB1");
                        SetList(reflectorStrings, "Reflector1", "Reflector2", "Reflector3");

                        if (_initialRotorPos.Length > 3) _initialRotorPos = _initialRotorPos.Remove(0, 1);
                        rotor1 = 0; rotor2 = 1; rotor3 = 2; rotor4 = 0;
                        reflector = 0;

                        setSettingsVisibility();

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


        [TaskPane("InitialRotorPosCaption", "InitialRotorPosTooltip",
            null, 1, false, ControlType.TextBox, ValidationType.RegEx, "^[A-Za-z]{3,4}$")]
        public string InitialRotorPos
        {
            get { return this._initialRotorPos; }
            set
            {
                if (value != _initialRotorPos)
                {
                    this._initialRotorPos = value;
                    OnPropertyChanged("InitialRotorPos");   
                }
            }
        }

        #region Text options
        
        [TaskPane( "UnknownSymbolHandlingCaption", "UnknownSymbolHandlingTooltip",
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
        
        [TaskPane( "CaseHandlingCaption", "CaseHandlingTooltip",
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

        #region Analysis options

        [TaskPane("AnalyzeInitialRotorPosCaption", "AnalyzeInitialRotorPosTooltip",
            "AnalysisOptionsGroup", 6, false, ControlType.CheckBox, "", null)]
        public bool AnalyzeInitialRotorPos
        {
            get { return _analyzeInitialRotorPos; }
            set
            {
                if (value != _analyzeInitialRotorPos)
                {
                    _analyzeInitialRotorPos = value;
                    OnPropertyChanged("AnalyzeInitialRotorPos");
                    setSettingsVisibility();
                }
            }
        }

        [TaskPane( "AnalyzeRotorsCaption", "AnalyzeRotorsTooltip",
            "AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool AnalyzeRotors
        {
            get { return analyzeRotors; }
            set
            {
                if (value != analyzeRotors)
                {
                    analyzeRotors = value;
                    OnPropertyChanged("AnalyzeRotors");
                    setSettingsVisibility();
                }
            }
        }

        [SettingsFormat(1, "Normal", "Normal")]
        [TaskPane( "AnalysisUseRotorICaption", "AnalysisUseRotorITooltip",
            "AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool AnalysisUseRotorI
        {
            get { return analysisUseRotorI; }
            set
            {
                if (value != analysisUseRotorI)
                {
                    analysisUseRotorI = value;
                    OnPropertyChanged("AnalysisUseRotorI");
                }
            }
        }

        [SettingsFormat(1, "Normal", "Normal")]
        [TaskPane( "AnalysisUseRotorIICaption", "AnalysisUseRotorIITooltip",
            "AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool AnalysisUseRotorII
        {
            get { return analysisUseRotorII; }
            set
            {
                if (value != analysisUseRotorII)
                {
                    analysisUseRotorII = value;
                    OnPropertyChanged("AnalysisUseRotorII");
                }
            }
        }

        [SettingsFormat(1, "Normal", "Normal")]
        [TaskPane( "AnalysisUseRotorIIICaption", "AnalysisUseRotorIIITooltip",
            "AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool AnalysisUseRotorIII
        {
            get { return analysisUseRotorIII; }
            set
            {
                if (value != analysisUseRotorIII)
                {
                    analysisUseRotorIII = value;
                    OnPropertyChanged("AnalysisUseRotorIII");
                }
            }
        }

        [SettingsFormat(1, "Normal", "Normal")]
        [TaskPane( "AnalysisUseRotorIVCaption", "AnalysisUseRotorIVTooltip",
            "AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool AnalysisUseRotorIV
        {
            get { return analysisUseRotorIV; }
            set
            {
                if (value != analysisUseRotorIV)
                {
                    analysisUseRotorIV = value;
                    OnPropertyChanged("AnalysisUseRotorIV");
                }
            }
        }


        [SettingsFormat(1, "Normal", "Normal")]
        [TaskPane( "AnalysisUseRotorVCaption", "AnalysisUseRotorVTooltip",
            "AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool AnalysisUseRotorV
        {
            get { return analysisUseRotorV; }
            set
            {
                if (value != analysisUseRotorV)
                {
                    analysisUseRotorV = value;
                    OnPropertyChanged("AnalysisUseRotorV");
                }
            }
        }

        [SettingsFormat(1, "Normal", "Normal")]
        [TaskPane( "AnalysisUseRotorVICaption", "AnalysisUseRotorVITooltip",
            "AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool AnalysisUseRotorVI
        {
            get { return analysisUseRotorVI; }
            set
            {
                if (value != analysisUseRotorVI)
                {
                    analysisUseRotorVI = value;
                    OnPropertyChanged("AnalysisUseRotorVI");
                }
            }
        }


        [SettingsFormat(1, "Normal", "Normal")]
        [TaskPane( "AnalysisUseRotorVIICaption", "AnalysisUseRotorVIITooltip",
            "AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool AnalysisUseRotorVII
        {
            get { return analysisUseRotorVII; }
            set
            {
                if (value != analysisUseRotorVII)
                {
                    analysisUseRotorVII = value;
                    OnPropertyChanged("AnalysisUseRotorVII");
                }
            }
        }


        [SettingsFormat(1, "Normal", "Normal")]
        [TaskPane( "AnalysisUseRotorVIIICaption", "AnalysisUseRotorVIIITooltip",
            "AnalysisOptionsGroup", 7, false, ControlType.CheckBox, "", null)]
        public bool AnalysisUseRotorVIII
        {
            get { return analysisUseRotorVIII; }
            set
            {
                if (value != analysisUseRotorVIII)
                {
                    analysisUseRotorVIII = value;
                    OnPropertyChanged("AnalysisUseRotorVIII");
                }
            }
        }


        [TaskPane( "AnalyzeRingsCaption", "AnalyzeRingsTooltip",
            "AnalysisOptionsGroup", 8, false, ControlType.CheckBox, "", null)]
        public bool AnalyzeRings
        {
            get { return analyzeRings; }
            set
            {
                if (value != analyzeRings)
                {
                    analyzeRings = value;
                    OnPropertyChanged("AnalyzeRings");
                    setSettingsVisibility();
                }
            }
        }

        [TaskPane("SearchMethodCaption", "SearchMethodTooltip", "AnalysisOptionsGroup", 8, false, ControlType.ComboBox, new string[] { "SearchMethodList1", "SearchMethodList2", "SearchMethodList3", "SearchMethodList4", "SearchMethodList5", "SearchMethodList6" })]
        public int SearchMethod
        {
            get { return this._SearchMethod; }
            set
            {
                if (value != _SearchMethod)
                {
                    _SearchMethod = value;
                    OnPropertyChanged("SearchMethod");
                }
            }
        }

        [TaskPane( "AnalyzePlugsCaption", "AnalyzePlugsTooltip",
            "AnalysisOptionsGroup", 9, false, ControlType.CheckBox, "", null)]
        public bool AnalyzePlugs
        {
            get { return analyzePlugs; }
            set
            {
                if (value != analyzePlugs)
                {
                    analyzePlugs = value;
                    OnPropertyChanged("AnalyzePlugs");
                    setSettingsVisibility();
                }
            }
        }

        [TaskPane( "MaxSearchedPlugsCaption", "MaxSearchedPlugsTooltip",
            "AnalysisOptionsGroup", 9, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 26)]
        public int MaxSearchedPlugs
        {
            get { return this.maxSearchedPlugs; }
            set
            {
                if (value != maxSearchedPlugs)
                {
                    maxSearchedPlugs = value;
                    OnPropertyChanged("MaxSearchedPlugs");
                }
            }
        }

        [TaskPane("PlugSearchMethodCaption", "PlugSearchMethodTooltip", "AnalysisOptionsGroup", 9, false, ControlType.ComboBox, new string[] { "SearchMethodList1", "SearchMethodList2", "SearchMethodList3", "SearchMethodList4", "SearchMethodList5", "SearchMethodList6" })]
        public int PlugSearchMethod
        {
            get { return this.plugSearchMethod; }
            set
            {
                if (value != plugSearchMethod)
                {
                    plugSearchMethod = value;
                    OnPropertyChanged("PlugSearchMethod");
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
        [TaskPane( "A", "PlugBoardLetterTooltip", "PlugboardGroup", 40, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardA
        {
            get { return alphabet.IndexOf(this.plugBoard[0]); }
            set { setPlugBoard(0, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Eins")]
        [TaskPane( "B", "PlugBoardLetterTooltip", "PlugboardGroup", 41, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardB
        {
            get { return alphabet.IndexOf(this.plugBoard[1]); }
            set { setPlugBoard(1, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Eins")]
        [TaskPane( "C", "PlugBoardLetterTooltip", "PlugboardGroup", 42, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardC
        {
            get { return alphabet.IndexOf(this.plugBoard[2]); }
            set { setPlugBoard(2, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane( "D", "PlugBoardLetterTooltip", "PlugboardGroup", 43, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardD
        {
            get { return alphabet.IndexOf(this.plugBoard[3]); }
            set { setPlugBoard(3, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane( "E", "PlugBoardLetterTooltip", "PlugboardGroup", 44, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardE
        {
            get { return alphabet.IndexOf(this.plugBoard[4]); }
            set { setPlugBoard(4, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane( "F", "PlugBoardLetterTooltip", "PlugboardGroup", 45, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardF
        {
            get { return alphabet.IndexOf(this.plugBoard[5]); }
            set { setPlugBoard(5, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Drei")]
        [TaskPane( "G", "PlugBoardLetterTooltip", "PlugboardGroup", 46, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardG
        {
            get { return alphabet.IndexOf(this.plugBoard[6]); }
            set { setPlugBoard(6, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Drei")]
        [TaskPane( "H", "PlugBoardLetterTooltip", "PlugboardGroup", 47, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardH
        {
            get { return alphabet.IndexOf(this.plugBoard[7]); }
            set { setPlugBoard(7, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Drei")]
        [TaskPane( "I", "PlugBoardLetterTooltip", "PlugboardGroup", 48, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardI
        {
            get { return alphabet.IndexOf(this.plugBoard[8]); }
            set { setPlugBoard(8, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Vier")]
        [TaskPane( "J", "PlugBoardLetterTooltip", "PlugboardGroup", 49, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardJ
        {
            get { return alphabet.IndexOf(this.plugBoard[9]); }
            set { setPlugBoard(9, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Vier")]
        [TaskPane( "K", "PlugBoardLetterTooltip", "PlugboardGroup", 50, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardK
        {
            get { return alphabet.IndexOf(this.plugBoard[10]); }
            set { setPlugBoard(10, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Vier")]
        [TaskPane( "L", "PlugBoardLetterTooltip", "PlugboardGroup", 51, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardL
        {
            get { return alphabet.IndexOf(this.plugBoard[11]); }
            set { setPlugBoard(11, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Fuenf")]
        [TaskPane( "M", "PlugBoardLetterTooltip", "PlugboardGroup", 52, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardM
        {
            get { return alphabet.IndexOf(this.plugBoard[12]); }
            set { setPlugBoard(12, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Fuenf")]
        [TaskPane( "N", "PlugBoardLetterTooltip", "PlugboardGroup", 53, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardN
        {
            get { return alphabet.IndexOf(this.plugBoard[13]); }
            set { setPlugBoard(13, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Fuenf")]
        [TaskPane( "O", "PlugBoardLetterTooltip", "PlugboardGroup", 54, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardO
        {
            get { return alphabet.IndexOf(this.plugBoard[14]); }
            set { setPlugBoard(14, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sechs")]
        [TaskPane( "P", "PlugBoardLetterTooltip", "PlugboardGroup", 55, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardP
        {
            get { return alphabet.IndexOf(this.plugBoard[15]); }
            set { setPlugBoard(15, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sechs")]
        [TaskPane( "Q", "PlugBoardLetterTooltip", "PlugboardGroup", 56, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardQ
        {
            get { return alphabet.IndexOf(this.plugBoard[16]); }
            set { setPlugBoard(16, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sechs")]
        [TaskPane( "R", "PlugBoardLetterTooltip", "PlugboardGroup", 57, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardR
        {
            get { return alphabet.IndexOf(this.plugBoard[17]); }
            set { setPlugBoard(17, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sieben")]
        [TaskPane( "S", "PlugBoardLetterTooltip", "PlugboardGroup", 58, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardS
        {
            get { return alphabet.IndexOf(this.plugBoard[18]); }
            set { setPlugBoard(18, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sieben")]
        [TaskPane( "T", "PlugBoardLetterTooltip", "PlugboardGroup", 59, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardT
        {
            get { return alphabet.IndexOf(this.plugBoard[19]); }
            set { setPlugBoard(19, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sieben")]
        [TaskPane( "U", "PlugBoardLetterTooltip", "PlugboardGroup", 60, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardU
        {
            get { return alphabet.IndexOf(this.plugBoard[20]); }
            set { setPlugBoard(20, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Acht")]
        [TaskPane( "V", "PlugBoardLetterTooltip", "PlugboardGroup", 61, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardV
        {
            get { return alphabet.IndexOf(this.plugBoard[21]); }
            set { setPlugBoard(21, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Acht")]
        [TaskPane( "W", "PlugBoardLetterTooltip", "PlugboardGroup", 62, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardW
        {
            get { return alphabet.IndexOf(this.plugBoard[22]); }
            set { setPlugBoard(22, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Acht")]
        [TaskPane( "X", "PlugBoardLetterTooltip", "PlugboardGroup", 63, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardX
        {
            get { return alphabet.IndexOf(this.plugBoard[23]); }
            set { setPlugBoard(23, value); }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Neun")]
        [TaskPane( "Y", "PlugBoardLetterTooltip", "PlugboardGroup", 64, false, ControlType.ComboBox,
            new String[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardY
        {
            get { return alphabet.IndexOf(this.plugBoard[24]); }
            set { setPlugBoard(24, value); }
        }

        [SettingsFormat(1, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Neun")]
        [TaskPane( "Z", "PlugBoardLetterTooltip", "PlugboardGroup", 65, false, ControlType.ComboBox,
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
                OnPropertyChanged("PlugBoard" + c);

            OnPropertyChanged("PlugBoard");

            // Are the following needed? For the presentation? indeed
            //OnPropertyChanged("PlugBoardDisplay");
            //OnPropertyChanged("Remove all Plugs");
        }

        #endregion

        #region Evaluation settings

        // EVALUATION!
        [CryptoBenchmark()]
        [TaskPane("Stop current analysis if percent reached", "Stop the current analysis in the cryptanalytic component if entered percentage reached", null, 7, false, ControlType.CheckBox)]
        public bool StopIfPercentReached
        {
            get
            {
                return this._stopIfPercentReached;
            }
            set
            {
                this._stopIfPercentReached = value;
                OnPropertyChanged("StopIfPercentReached");
            }
        }

        // EVALUATION!
        [CryptoBenchmark()]
        [TaskPane("ComparisonFrequencyCaption", "ComparisonFrequencyTooltip", null, 8, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 10000)]
        public int ComparisonFrequency
        {
            get
            {
                return _comparisonFrequency;
            }
            set
            {
                if (value != _comparisonFrequency)
                {
                    _comparisonFrequency = value;
                    OnPropertyChanged("ComparisonFrequency");
                }
            }
        }

        #endregion

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