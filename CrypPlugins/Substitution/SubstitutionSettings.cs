/*
   Copyright 2013 Nils Kopal, Universit�t Kassel

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

using Cryptool.PluginBase;
using System.ComponentModel;
using System.Windows;

namespace Cryptool.Substitution
{
    public class SubstitutionSettings : ISettings
    {
        public SubstitutionSettings()
        {
        }

        private UnknownSymbolHandling _unknownSymbolHandling;
        private SymbolChoice _symbolChoice;
        private string _replacementSymbol = "?";
        private int _action = 0;

        [TaskPane("UnknownSymbolHandlingCaption", "UnknownSymbolHandlingTooltip", null, 1, false, ControlType.ComboBox, new string[] { "UnknownSymbolHandlingList1", "UnknownSymbolHandlingList2", "UnknownSymbolHandlingList3" })]
        public UnknownSymbolHandling UnknownSymbolHandling
        {
            get { return _unknownSymbolHandling; }
            set
            {
                if (value != _unknownSymbolHandling)
                {
                    _unknownSymbolHandling = value;
                    OnPropertyChanged("UnknownSymbolHandling");
                    UpdateTaskPaneVisibility();
                }
            }
        }

        [TaskPane("SymbolChoiceCaption", "SymbolChoiceTooltip", null, 1, false, ControlType.ComboBox, new string[] { "SymbolChoiceList1", "SymbolChoiceList2" })]
        public SymbolChoice SymbolChoice
        {
            get { return _symbolChoice; }
            set
            {
                if (value != _symbolChoice)
                {
                    _symbolChoice = value;
                    OnPropertyChanged("SymbolChoice");
                }
            }
        }

        [TaskPane("ReplacementSymbolCaption", "ReplacementSymbolTooltip", null, 1, false, ControlType.TextBox)]
        public string ReplacementSymbol
        {
            get { return _replacementSymbol; }
            set
            {
                if (_replacementSymbol != value)
                {
                    _replacementSymbol = value;
                    OnPropertyChanged("ReplacementSymbol");
                }
            }
        }

        public void UpdateTaskPaneVisibility ()
        {

            if (TaskPaneAttributeChanged == null)
                return;

            switch (UnknownSymbolHandling)
            {
                case UnknownSymbolHandling.Replace:
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("ReplacementSymbol", Visibility.Visible)));
                    break;

                default:
                    TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer("ReplacementSymbol", Visibility.Collapsed)));
                    break;
            }
        }

        [TaskPane("ActionCaption", "ActionTooltip", null, 1, false, ControlType.ComboBox, new[] { "Encrypt", "Decrypt" })]
        public int Action
        {
            get
            {
                return _action;
            }
            set
            {
                _action = value;
                OnPropertyChanged("Action");
            }
        }

        #region INotifyPropertyChanged Members

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }

    public enum UnknownSymbolHandling
    {
        LeaveAsIs,
        Replace,
        Remove
    }

    public enum SymbolChoice
    {
        RoundRobin,
        Random
    }
}
