﻿/*
   Copyright 2011 CrypTool 2 Team <ct2contact@cryptool.org>

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

namespace Cryptool.Plugins.SATSolver
{
    public class SATSolverSettings : ISettings
    {
        #region Private Variables

        // if further sat solvers need to be implemented, just add solver in enum
        // and add in Execute() and buildArgs() switch statements
        public enum Solver {MiniSAT = 0 /*, DummySAT = 1 */};
        public string[] solverExe = {"Data\\SATSolver\\SATSolver_Minisat.exe"}; // executable relative path from CrypPlugins\, indexed by Solver::satString
        private Solver satString = Solver.MiniSAT;

        #region Minisat Options

        private int randomInitHandling = 1;             // 0 or 1, default 1 = "Off"
        private int lubyHandling = 0;                   // 0 or 1, default 0 = "On"
        private string randomFreqHandling = "0";        // 0 - 1, default 0
        private string varDecayHandling = "0.95";       // 0. - 1, default 0.95
        private string clauseDecayHandling = "0.999";   // 0. - 1, default 0.999
        private int phaseSavingHandling = 2;            // 0 - 2, default 2
        private int ccMinHandling = 2;                  // 0 - 2, default 2
        private string rFirstHandling = "100";          // 0 - 2147483647, default 100
        private string rIncHandling = "2";              // 0 - 2147483647, default 2

        private int verbosityHandling = 1;              // 0 - 2, default 1
        private int preprocessHandling = 0;             // 0 or 1, default 0 = "On"
        private int dimacsHandling = 1;                 // 0 or 1, default 1 = "Off"
        private string cpuLimitHandling = "2147483647"; // 0 - 2147483647, default = 2147483647
        private string memLimitHandling = "2147483647"; // 0 - 2147483647, default = 2147483647

        private int elimHandling = 0;                   // 0 or 1, default 0 = "On"
        private string subLimitHandling = "1000";       // 0 - 2147483647, default = 1000
        private int rCheckHandling = 1;                 // 0 or 1, default 1 = "Off"

        private int clearOutputHandling = 0;            // 0 or 1, default 0 = "On"

        #endregion

        #endregion

        #region TaskPane Settings

        
        /*
        [TaskPane("SolverTPCaption", "SolverTPTooltip", null, 0, false, ControlType.ComboBox, 
            new string[] {"SolverMinisatString", "SolverDummysatString"})]
        */
        public Solver SatString
        {
            get
            {
                return satString;
            }
            set
            {
            }
            /*
            {
                if (!satString.Equals(value))
                {
                    satString = value;
                    OnPropertyChanged("satString");
                }
            }
            */
        }

        #region Minisat Output Options

        [TaskPane("ClearOutputCaption", "ClearOutputTooltip", "OutputOptionsGroup", 1, false,
            ControlType.ComboBox, new string[] { "YesStr", "NoStr" })]
        public int ClearOutputHandling
        {
            get { return this.clearOutputHandling; }
            set
            {
                if ((int)value != clearOutputHandling)
                {
                    this.clearOutputHandling = (int)value;
                    OnPropertyChanged("ClearOutputHandling");
                }
            }
        }

        [TaskPane("VerbosityCaption", "VerbosityTooltip", "OutputOptionsGroup", 2, false,
            ControlType.ComboBox, new string[] { "SilentStr", "SomeStr", "MoreStr" })]
        public int VerbosityHandling
        {
            get { return this.verbosityHandling; }
            set
            {
                if ((int)value != verbosityHandling)
                {
                    this.verbosityHandling = (int)value;
                    OnPropertyChanged("VerbosityHandling");
                }
            }
        }

        [TaskPane("DimacsCaption", "DimacsTooltip", "OutputOptionsGroup", 3, false,
            ControlType.ComboBox, new string[] { "OnStr", "OffStr" })]
        public int DimacsHandling
        {
            get { return this.dimacsHandling; }
            set
            {
                if ((int)value != dimacsHandling)
                {
                    this.dimacsHandling = (int)value;
                    OnPropertyChanged("DimacsHandling");
                }
            }
        }

        #endregion

        #region Minisat Configuration Options

        [TaskPane("RFirstCaption", "RFirstTooltip", "CoreOptionsGroup", 4, false,
            ControlType.TextBox)]
        public string RFirstHandling
        {
            get { return this.rFirstHandling; }
            set
            {
                if (value != rFirstHandling)
                {
                    this.rFirstHandling = value;
                    OnPropertyChanged("RFirstHandling");
                }
            }
        }

        [TaskPane("RIncCaption", "RIncTooltip", "CoreOptionsGroup", 5, false,
            ControlType.TextBox)]
        public string RIncHandling
        {
            get { return this.rIncHandling; }
            set
            {
                if (value != rIncHandling)
                {
                    this.rIncHandling = value;
                    OnPropertyChanged("RIncHandling");
                }
            }
        }

        [TaskPane("PhaseSavingCaption", "PhaseSavingTooltip", "CoreOptionsGroup", 6, false,
            ControlType.ComboBox, new string[] { "NoneStr", "LimitedStr", "FullStr" })]
        public int PhaseSavingHandling
        {
            get { return this.phaseSavingHandling; }
            set
            {
                if ((int)value != phaseSavingHandling)
                {
                    this.phaseSavingHandling = (int)value;
                    OnPropertyChanged("PhaseSavingHandling");
                }
            }
        }

        [TaskPane("CCMinCaption", "CCMinTooltip", "CoreOptionsGroup", 7, false,
            ControlType.ComboBox, new string[] { "NoneStr", "BasicStr", "DeepStr" })]
        public int CCMinHandling
        {
            get { return this.ccMinHandling; }
            set
            {
                if ((int)value != ccMinHandling)
                {
                    this.ccMinHandling = (int)value;
                    OnPropertyChanged("CCMinHandling");
                }
            }
        }

        [TaskPane("VarDecayCaption", "VarDecayTooltip", "CoreOptionsGroup", 8, false,
            ControlType.TextBox)]
        public string VarDecayHandling
        {
            get { return this.varDecayHandling; }
            set
            {
                if (value != varDecayHandling)
                {
                    this.varDecayHandling = value;
                    OnPropertyChanged("VarDecayHandling");
                }
            }
        }

        [TaskPane("ClauseDecayCaption", "ClauseDecayTooltip", "CoreOptionsGroup", 9, false,
            ControlType.TextBox)]
        public string ClauseDecayHandling
        {
            get { return this.clauseDecayHandling; }
            set
            {
                if (value != clauseDecayHandling)
                {
                    this.clauseDecayHandling = value;
                    OnPropertyChanged("ClauseDecayHandling");
                }
            }
        }

        [TaskPane("RandomFreqCaption", "RandomFreqTooltip", "CoreOptionsGroup", 10, false,
            ControlType.TextBox)]
        public string RandomFreqHandling
        {
            get { return this.randomFreqHandling; }
            set
            {
                if (value != randomFreqHandling)
                {
                    this.randomFreqHandling = value;
                    OnPropertyChanged("RandomFreqHandling");
                }
            }
        }

        [TaskPane("RandomInitCaption", "RandomInitTooltip", "CoreOptionsGroup", 11, false,
            ControlType.ComboBox, new string[] { "OnStr", "OffStr" })]
        public int RandomInitHandling
        {
            get { return this.randomInitHandling; }
            set
            {
                if ((int)value != randomInitHandling)
                {
                    this.randomInitHandling = (int)value;
                    OnPropertyChanged("RandomInitHandling");
                }
            }
        }

        [TaskPane("LubyCaption", "LubyTooltip", "CoreOptionsGroup", 12, false,
            ControlType.ComboBox, new string[] { "OnStr", "OffStr" })]
        public int LubyHandling
        {
            get { return this.lubyHandling; }
            set
            {
                if ((int)value != lubyHandling)
                {
                    this.lubyHandling = (int)value;
                    OnPropertyChanged("LubyHandling");
                }
            }
        }
      
        // not working on windows, maybe fix this later
        //[TaskPane("CPULimitCaption", "CPULimitTooltip", "MainOptionsGroup", 10, false,
        //    ControlType.TextBox)]
        public string CPULimitHandling
        {
            get { return this.cpuLimitHandling; }
            set
            {
                if (value != cpuLimitHandling)
                {
                    this.cpuLimitHandling = value;
                    OnPropertyChanged("CPULimitHandling");
                }
            }
        }

        // not working on windows, maybe fix this later
        //[TaskPane("MEMLimitCaption", "MEMLimitTooltip", "MainOptionsGroup", 11, false,
        //    ControlType.TextBox)]
        public string MEMLimitHandling
        {
            get { return this.memLimitHandling; }
            set
            {
                if (value != memLimitHandling)
                {
                    this.memLimitHandling = value;
                    OnPropertyChanged("MEMLimitHandling");
                }
            }
        }

        #endregion

        #region Minisat Preprocessor Options

        [TaskPane("PreprocessCaption", "PreprocessTooltip", "PreprocessorOptionsGroup", 15, false,
            ControlType.ComboBox, new string[] { "OnStr", "OffStr" })]
        public int PreprocessHandling
        {
            get { return this.preprocessHandling; }
            set
            {
                if ((int)value != preprocessHandling)
                {
                    this.preprocessHandling = (int)value;
                    OnPropertyChanged("PreprocessHandling");
                }
            }
        }
        [TaskPane("ElimCaption", "ElimTooltip", "PreprocessorOptionsGroup", 16, false,
            ControlType.ComboBox, new string[] { "OnStr", "OffStr" })]
        public int ElimHandling
        {
            get { return this.elimHandling; }
            set
            {
                if ((int)value != elimHandling)
                {
                    this.elimHandling = (int)value;
                    OnPropertyChanged("ElimHandling");
                }
            }
        }

        [TaskPane("SubLimitCaption", "SubLimitTooltip", "PreprocessorOptionsGroup", 17, false,
            ControlType.TextBox)]
        public string SubLimitHandling
        {
            get { return this.subLimitHandling; }
            set
            {
                if (value != subLimitHandling)
                {
                    this.subLimitHandling = value;
                    OnPropertyChanged("SubLimitHandling");
                }
            }
        }

        [TaskPane("RCheckCaption", "RCheckTooltip", "PreprocessorOptionsGroup", 18, false,
            ControlType.ComboBox, new string[] { "OnStr", "OffStr" })]
        public int RCheckHandling
        {
            get { return this.rCheckHandling; }
            set
            {
                if ((int)value != rCheckHandling)
                {
                    this.rCheckHandling = (int)value;
                    OnPropertyChanged("RCheckHandling");
                }
            }
        }



        #endregion

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {
            
        }

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion
    }
}
