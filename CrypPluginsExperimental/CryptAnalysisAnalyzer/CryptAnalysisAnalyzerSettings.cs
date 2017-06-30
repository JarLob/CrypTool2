/*
   Copyright 2017 CrypTool 2 Team <ct2contact@cryptool.org>

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
using System.Numerics;
using System.ComponentModel;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.Windows;

namespace Cryptool.Plugins.CryptAnalysisAnalyzer
{
    public enum XAxisPlot { ciphertextLength, keyLength, runtime };
    public enum YAxisPlot { success, percentDecrypted, successAndPercentDecrypted };
    public enum Y2AxisPlot { none, decryptions, restarts, tabuSetSizes, populationSizes, runtime };

    public class CryptAnalysisAnalyzerSettings : ISettings
    {
        public CryptAnalysisAnalyzerSettings(CryptAnalysisAnalyzer CAA)
        {
            this._CAA = CAA;
        }

        #region Private Variables

        // general variables
        private CryptAnalysisAnalyzer _CAA;
        private const int gnuPlotPaneIndex = generalPaneIndex + 2;
        private double _correctPercentage = 95;
        private bool _calculateRuntime = true;
        private const int generalPaneIndex = 1;

        // GnuPlot variables
        private XAxisPlot _xAxis = XAxisPlot.ciphertextLength;
        private YAxisPlot _yAxis = YAxisPlot.successAndPercentDecrypted;
        private Y2AxisPlot _y2Axis = Y2AxisPlot.decryptions;
        private bool _showY2Average = true;
        private int _normalizingFactor = 4;

        #endregion

        #region General Settings

        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>
        [TaskPane("Minimal correct percentage", "This is a parameter tooltip", null, generalPaneIndex, false, ControlType.TextBox, null)]
        public double CorrectPercentage
        {
            get
            {
                return this._correctPercentage;
            }
            set
            {
                this._correctPercentage = value;
                OnPropertyChanged("CorrectPercentage");
            }
        }

        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>
        [TaskPane("Calculate runtime", "Calculate the runtime of the algorithm", null, generalPaneIndex + 1, false, ControlType.CheckBox)]
        public bool CalculateRuntime
        {
            get { return this._calculateRuntime; }
            set
            {
                this._calculateRuntime = value;
                OnPropertyChanged("CalculateRuntime");
            }
        }

        #endregion

        #region GnuPlot Settings

        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>
        [TaskPane("X-axis", "Values to show on the X-axis", "GnuPlot", gnuPlotPaneIndex, true, ControlType.ComboBox, new String[] { 
            "Ciphertext length", "Key length", "Runtime"})]
        public XAxisPlot XAxis
        {
            get
            {
                return this._xAxis;
            }
            set
            {
                if (value != _xAxis)
                {
                    this._xAxis = value;
                    //UpdateKeyFormatVisibility();
                    OnPropertyChanged("XAxis");

                    if (_CAA.GnuPlotScriptOutput != null)
                    {
                        _CAA.SetGnuPlotVariables();
                        _CAA.GenerateGnuPlotDataOutput();
                        _CAA.GenerateGnuPlotScriptOutput();
                        _CAA.RefreshEvaluationOutputs();
                    }
                }
            }
        }

        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>
        [TaskPane("Y-axis", "Values to show on the Y-axis", "GnuPlot", gnuPlotPaneIndex + 1, true, ControlType.ComboBox, new String[] { 
            "Success", "Percent decrypted", "Success and % decrypted"})]
        public YAxisPlot YAxis
        {
            get
            {
                return this._yAxis;
            }
            set
            {
                if (value != _yAxis)
                {
                    this._yAxis = value;
                    //UpdateKeyFormatVisibility();
                    OnPropertyChanged("YAxis");

                    if (_CAA.GnuPlotScriptOutput != null)
                    {
                        _CAA.SetGnuPlotVariables();
                        _CAA.GenerateGnuPlotDataOutput();
                        _CAA.GenerateGnuPlotScriptOutput();
                        _CAA.RefreshEvaluationOutputs();
                    }
                }
            }
        }

        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>
        [TaskPane("Second Y-axis", "Values to show on the second Y-axis", "GnuPlot", gnuPlotPaneIndex + 2, true, ControlType.ComboBox, new String[] { 
            "None", "Decryptions", "Restarts", "Tabu set sizes", "Population sizes", "Runtime"})]
        public Y2AxisPlot Y2Axis
        {
            get
            {
                return this._y2Axis;
            }
            set
            {
                if (value != _y2Axis)
                {
                    this._y2Axis = value;
                    //UpdateKeyFormatVisibility();
                    OnPropertyChanged("Y2Axis");
                    if (value == Y2AxisPlot.none)
                    {
                        this._showY2Average = false;
                        OnPropertyChanged("ShowY2Average");
                    }

                    if (_CAA.GnuPlotScriptOutput != null)
                    {
                        _CAA.SetGnuPlotVariables();
                        _CAA.GenerateGnuPlotDataOutput();
                        _CAA.GenerateGnuPlotScriptOutput();
                        _CAA.RefreshEvaluationOutputs();
                    }
                }
            }
        }

        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>
        [TaskPane("Average of second Y-axis", "Show the average of the second Y-axis", "GnuPlot", gnuPlotPaneIndex + 3, true, ControlType.CheckBox)]
        public bool ShowY2Average
        {
            get { return this._showY2Average; }
            set
            {
                if (_y2Axis != Y2AxisPlot.none &&
                    value != _showY2Average)
                {
                    this._showY2Average = value;
                    OnPropertyChanged("ShowY2Average");

                    if (_CAA.GnuPlotScriptOutput != null)
                    {
                        _CAA.GenerateGnuPlotScriptOutput();
                        _CAA.RefreshEvaluationOutputs();
                    }
                }
            }
        }

        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>
        [TaskPane("Normalize axis ranges factor", "Size factor to start normalizing single high values on the axis ranges", "GnuPlot", gnuPlotPaneIndex + 4, true, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        public int NormalizingFactor
        {
            get
            {
                return _normalizingFactor;
            }
            set
            {
                _normalizingFactor = value;
                OnPropertyChanged("NormalizingFactor");
            }
        }

        #endregion

        #region UI Updates

        /*
        internal void UpdateTaskPaneVisibility()
        {
            settingChanged("KeyFormat", Visibility.Collapsed);

            switch (KeyGeneration)
            {
                case GenerationType.naturalSpeech: // natural speech
                    settingChanged("KeyFormat", Visibility.Visible);
                    break;
                case GenerationType.random: // random generation
                    // TODO: change to invisible when input alphabet or regex is implemented
                    settingChanged("KeyFormat", Visibility.Visible);
                    break;
            }
        }

        private void settingChanged(string setting, Visibility vis)
        {
            if (TaskPaneAttributeChanged != null)
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(setting, vis)));
        }*/

        #endregion

        #region Events

        //public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

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
