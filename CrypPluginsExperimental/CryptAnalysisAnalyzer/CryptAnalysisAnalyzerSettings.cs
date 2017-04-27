/*
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
using System.Numerics;
using System.ComponentModel;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.Windows;

namespace Cryptool.Plugins.CryptAnalysisAnalyzer
{
    public enum XAxisPlot { ciphertextLength, keyLength, runtime };
    public enum YAxisPlot { success, decryptedPercent, successAndDecryptedPercent };
    public enum Y2AxisPlot { none, decryptions, restarts, tabuSetSizes, populationSizes };

    public class CryptAnalysisAnalyzerSettings : ISettings
    {
        #region Private Variables

        // general variables
        private const int gnuPlotPaneIndex = generalPaneIndex + 3;
        private double _correctPercentage = 95;
        private int _timeUnit = 10;
        private bool _calculateRuntime = true;
        private const int generalPaneIndex = 1;

        // GnuPlot variables
        private XAxisPlot _xAxis = XAxisPlot.ciphertextLength;
        private YAxisPlot _yAxis = YAxisPlot.successAndDecryptedPercent;
        private Y2AxisPlot _y2Axis = Y2AxisPlot.decryptions;
        private bool _showY2Average = true;

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
                return _correctPercentage;
            }
            set
            {
                _correctPercentage = value;
                OnPropertyChanged("CorrectPercentage");
            }
        }

        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>
        [TaskPane("Time unit size in ms", "This is a parameter tooltip", null, generalPaneIndex + 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, Int32.MaxValue)]
        public int TimeUnit
        {
            get
            {
                return _timeUnit;
            }
            set
            {
                _timeUnit = value;
                OnPropertyChanged("TimeUnit");
            }
        }

        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>
        [TaskPane("Calculate runtime", "Calculate the runtime of the algorithm", null, generalPaneIndex + 2, false, ControlType.CheckBox)]
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
        [TaskPane("X-Axis", "Values to show on the X-Axis", "GnuPlot", gnuPlotPaneIndex, true, ControlType.ComboBox, new String[] { 
            "Ciphertext Length", "Key Length", "Runtime"})]
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
                }
            }
        }

        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>
        [TaskPane("Y-Axis", "Values to show on the Y-Axis", "GnuPlot", gnuPlotPaneIndex + 1, true, ControlType.ComboBox, new String[] { 
            "Success", "Decrypted Percentage", "Success and Decrypted %"})]
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
                }
            }
        }

        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>
        [TaskPane("Second Y-Axis", "Values to show on the second Y-Axis", "GnuPlot", gnuPlotPaneIndex + 2, true, ControlType.ComboBox, new String[] { 
            "None", "Decryptions", "Restarts", "Tabu Set Sizes", "Population Sizes"})]
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
                }
            }
        }

        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>
        [TaskPane("Average of second Y-Axis", "Show the Average of the second Y-Axis", "GnuPlot", gnuPlotPaneIndex + 3, true, ControlType.CheckBox)]
        public bool ShowY2Average
        {
            get { return this._showY2Average; }
            set
            {
                if (value != _showY2Average)
                {
                    this._showY2Average = value;
                    OnPropertyChanged("ShowY2Average");
                }
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
