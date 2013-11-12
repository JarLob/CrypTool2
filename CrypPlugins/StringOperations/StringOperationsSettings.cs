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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Cryptool.PluginBase;
using System;

namespace StringOperations
{
    class StringOperationsSettings : ISettings
    {
        private StringOperationType _stringOperationType;
        private int _blockSize = 5;
        private int _order = 0;
        private readonly Dictionary<StringOperationType, List<string>> _operationVisibility = new Dictionary<StringOperationType, List<string>>();
        private readonly List<string> _operationList = new List<string>();
        private string _string1 = String.Empty;
        private string _string2 = String.Empty;
        private string _string3 = String.Empty;
        private int _value1 = int.MinValue;
        private int _value2 = int.MinValue;

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {
            
        }

        #endregion

        public StringOperationsSettings()
        {
            foreach (StringOperationType name in Enum.GetValues(typeof(StringOperationType)))
            {
                _operationVisibility[name] = new List<string>();
            }
            _operationList.Add("String1");
            _operationList.Add("String2");
            _operationList.Add("String3");
            _operationList.Add("Value1");
            _operationList.Add("Value2");
            _operationList.Add("Blocksize");
            _operationList.Add("Order");
            _operationVisibility[StringOperationType.Block].Add("Blocksize");
            _operationVisibility[StringOperationType.CompareTo].Add("String2");
            _operationVisibility[StringOperationType.Concatenate].Add("String2");
            _operationVisibility[StringOperationType.Equals].Add("String2");
            _operationVisibility[StringOperationType.IndexOf].Add("String2");            
            _operationVisibility[StringOperationType.RegexReplace].Add("String2");
            _operationVisibility[StringOperationType.RegexReplace].Add("String3");
            _operationVisibility[StringOperationType.Replace].Add("String2");
            _operationVisibility[StringOperationType.Replace].Add("String3");
            _operationVisibility[StringOperationType.Sort].Add("Order");
            _operationVisibility[StringOperationType.Substring].Add("Value1");
            _operationVisibility[StringOperationType.Substring].Add("Value2");                       
            UpdateTaskPaneVisibility();
        }        

        [TaskPane("OperationCaption", "OperationCaptionToolTip", null, 1, false, ControlType.ComboBox,
            new[] { "OperationList1", "OperationList2", "OperationList3", "OperationList4", "OperationList5", "OperationList6", "OperationList7", "OperationList8", "OperationList9", "OperationList10", "OperationList11", "OperationList12", "OperationList13", "OperationList14", "OperationList15", "OperationList16" })]
        public StringOperationType Operation
        {
            get
            {
                return _stringOperationType;
            }
            set
            {
                if (_stringOperationType != value)
                {
                    _stringOperationType = value;
                    UpdateTaskPaneVisibility();
                    OnPropertyChanged("Operation");
                }
            }
        }

        [TaskPane("String1Caption", "String1Tooltip", null, 2, false, ControlType.TextBox)]
        public string String1
        {
            get
            {
                return _string1;
            }
            set
            {
                _string1 = value;
                OnPropertyChanged("String1");
            }
        }

        [TaskPane("String2Caption", "String2Tooltip", null, 2, false, ControlType.TextBox)]
        public string String2
        {
            get
            {
                return _string2;
            }
            set
            {
                _string2 = value;
                OnPropertyChanged("String2");
            }
        }

        [TaskPane("String2Caption", "String2Tooltip", null, 2, false, ControlType.TextBox)]
        public string String3
        {
            get
            {
                return _string3;
            }
            set
            {
                _string3 = value;
                OnPropertyChanged("String3");
            }
        }

        [TaskPane("Value1Caption", "Value1Tooltip", null, 2, false, ControlType.NumericUpDown)]
        public int Value1
        {
            get
            {
                return _value1;
            }
            set
            {
                _value1 = value;
                OnPropertyChanged("Value1");
            }
        }

        [TaskPane("Value2Caption", "Value2Tooltip", null, 2, false, ControlType.NumericUpDown)]
        public int Value2
        {
            get
            {
                return _value2;
            }
            set
            {
                _value2 = value;
                OnPropertyChanged("Value2");
            }
        }

        [TaskPane("BlocksizeCaption", "BlocksizeTooltip", null, 3, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, int.MaxValue)]
        public int Blocksize
        {
            get
            {
                return _blockSize;
            }
            set
            {
                _blockSize = value;
                OnPropertyChanged("Blocksize");
            }
        }

        [TaskPane("OrderCaption", "OrderTooltip", null, 4, false, ControlType.ComboBox, new[] { "Ascending", "Descending" })]
        public int Order
        {
            get
            {
                return _order;
            }
            set
            {
                _order = value;
                OnPropertyChanged("Order");
            }
        }

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        internal void UpdateTaskPaneVisibility()
        {
            if (TaskPaneAttributeChanged == null)
                return;

            foreach (var tpac in _operationList.Select(operation => new TaskPaneAttribteContainer(operation, (_operationVisibility[Operation].Contains(operation)) ? Visibility.Visible : Visibility.Collapsed)))
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(tpac));
            }
        }

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;
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
        RegexReplace,
        Split,
        Block,
        Reverse,
        Sort,
        Distinct
    }
}
