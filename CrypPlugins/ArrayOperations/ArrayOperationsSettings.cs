/*                              
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

namespace ArrayOperations
{
    class ArrayOperationsSettings : ISettings
    {
        private ArrayOperationType _stringOperationType;
        private readonly Dictionary<ArrayOperationType, List<string>> _operationVisibility = new Dictionary<ArrayOperationType, List<string>>();
        private readonly List<string> _operationList = new List<string>();
        private Array _array1 = Array.Empty<Object>();
        private Array _array2 = Array.Empty<Object>();

        private int _value1 = int.MinValue;
        private int _value2 = int.MinValue;
        private object _object1 = null;

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {
            _operationList.Add("Array1");
            _operationList.Add("Array2");
            _operationList.Add("Value1");
            _operationList.Add("Value2");
            _operationList.Add("Object1");

//            _operationVisibility[ArrayOperationType.Sum].Add("Array2");
            UpdateTaskPaneVisibility();
        }

        #endregion

        public ArrayOperationsSettings()
        {
            foreach (ArrayOperationType name in Enum.GetValues(typeof(ArrayOperationType)))
            {
                _operationVisibility[name] = new List<string>();
            }
        }

        [TaskPane("OperationCaption", "OperationTooltip", null, 1, false, ControlType.ComboBox, new string[] { "OperationList1", "OperationList2", "OperationList3", "OperationList4", "OperationList5", "OperationList6", "OperationList7", "OperationList8", "OperationList9", "OperationList10"} ) ]
        public ArrayOperationType Operation
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

        [TaskPane("Array1Caption", "Array1Tooltip", null, 2, false, ControlType.TextBox)]
        public Array Array1
        {
            get
            {
                return _array1;
            }
            set
            {
                _array1 = value;
                OnPropertyChanged("Array1");
            }
        }

        [TaskPane("Array2Caption", "Array2Tooltip", null, 2, false, ControlType.TextBox)]
        public Array Array2
        {
            get
            {
                return _array2;
            }
            set
            {
                _array2 = value;
                OnPropertyChanged("Array2");
            }
        }


        [TaskPane("Value1Caption", "Value1Tooltip", null, 5, false, ControlType.NumericUpDown)]
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

        [TaskPane("Value2Caption", "Value2Tooltip", null, 6, false, ControlType.NumericUpDown)]
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
        
        [TaskPane("Object1Caption", "Object1Tooltip", null, 6, false, ControlType.NumericUpDown)]
        public object Object1
        {
            get
            {
                return _object1;
            }
            set
            {
                _object1 = value;
                OnPropertyChanged("Object1");
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

    enum ArrayOperationType
    {
        Sum,
        Union, 
        Difference, 
        Concatenation, 
        Unique, 
        Length, 
        Equals, 
        IndexOf, 
        Replace, 
        Sort
    }
}
