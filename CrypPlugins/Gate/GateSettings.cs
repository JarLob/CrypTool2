﻿/*
   Copyright 2009 Matthäus Wander, Universität Duisburg-Essen

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
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace Gate
{
    public enum Trigger
    {
        AlwaysOpen, AlwaysClosed, TrueValue, FalseValue, AnyEdge, PositiveEdge, NegativeEdge
    };

    public class GateSettings : ISettings
    {
        private Trigger trigger = 0;

        [TaskPane( "TriggerCaption", "TriggerTooltip", null, 1, true, ControlType.RadioButton,
            new string[] { "TriggerList1", "TriggerList2", "TriggerList3", "TriggerList4", "TriggerList5", "TriggerList6", "TriggerList7" })]
        public Trigger Trigger
        {
            get
            {
                return trigger;
            }
            set
            {
                if (trigger != value)
                {
                    trigger = value;
                    OnPropertyChanged("Trigger");
                }
            }
        }
        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string p)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, p);
        }

        #endregion
    }
}
