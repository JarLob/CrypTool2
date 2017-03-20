﻿using System;
using System.ComponentModel;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.GrainV0.Attack
{
    public class GrainV0AttackSettings : ISettings
    {
        #region Private Variables
        //variable for NFSR source (false->external, true-> C# random number generator)
        private bool generator = false;

        #endregion

        #region TaskPane Settings
        //property for CheckBox

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

