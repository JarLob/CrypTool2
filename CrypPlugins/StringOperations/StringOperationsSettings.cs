﻿/*                              
   Copyright 2011 Nils Kopal, Uni Duisburg-Essen

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
using System.ComponentModel;
using Cryptool.PluginBase;

namespace StringOperations
{
    class StringOperationsSettings : ISettings
    {
        private StringOperationType _stringOperationType;

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion

        [TaskPane("OperationCaption", "OperationCaptionToolTip", null, 1, false, ControlType.ComboBox,
            new[] { "Concatenate", "Substring", "ToLowercase", "ToUppercase", "Length", "CompareTo", "Trim", "IndexOf", "Equals", "Replace", "RegexReplace" })]
        public int Operation
        {
            get
            {
                return (int)_stringOperationType;
            }
            set
            {
                if (_stringOperationType != (StringOperationType)value)
                {
                    _stringOperationType = (StringOperationType) value;
                    OnPropertyChanged("Operation");
                }
            }
        }

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }        
    }

    enum StringOperationType
    {
        Concatenate,
        Substring,
        ToLowercase,
        ToUppercase,
        Length,
        CompareTo,
        Trim, 
        IndexOf,
        Equals,
        Replace,
        RegexReplace
    }
}
