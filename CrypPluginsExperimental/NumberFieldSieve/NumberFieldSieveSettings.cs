﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;

namespace NumberFieldSieve
{
    class NumberFieldSieveSettings : ISettings
    {
        private bool _useCuda;
        private int _numCores;
        private int _numThreadsPerCore;

        public NumberFieldSieveSettings()
        {
            NumCores = Environment.ProcessorCount - 1;
        }

        [TaskPane("UseCUDACaption", "UseCUDATooltip", null, 0, false, ControlType.CheckBox)]
        [DontSave]
        public bool UseCUDA
        {
            get { return _useCuda; }
            set
            {
                _useCuda = value;
                OnPropertyChanged("UseCUDA");
            }
        }

        [TaskPane("NumCoresCaption", "NumCoresTooltip", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 32)]
        [DontSave]
        public int NumCores
        {
            get { return _numCores; }
            set
            {
                _numCores = value;
                OnPropertyChanged("NumCores");
            }
        }

        [TaskPane("NumThreadsPerCoreCaption", "NumThreadsPerCoreTooltip", null, 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 32)]
        [DontSave]
        public int NumThreadsPerCore
        {
            get { return _numThreadsPerCore; }
            set
            {
                _numThreadsPerCore = value;
                OnPropertyChanged("NumThreadsPerCore");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {
            
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
