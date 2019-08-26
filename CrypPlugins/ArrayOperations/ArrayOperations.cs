/*                              
   Copyright 2019 Simon Leischnig

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
using System.Text;
using System.Text.RegularExpressions;
using Cryptool.PluginBase;
using ArrayOperations.Properties;
using System.Collections.Generic;

namespace ArrayOperations
{
    [Author("Simon Leischnig", "simon-jena@gmx.net", "", "https://simlei.github.io/hi/")]
    [PluginInfo("ArrayOperations.Properties.Resources", "PluginCaption", "PluginTooltip", "ArrayOperations/DetailedDescription/doc.xml", "ArrayOperations/icon.png")]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class ArrayOperations : ICrypComponent
    {
        private ArrayOperationsSettings _settings = null;

        private Array _array1;
        private Array _array2;
        private Object _object1;
        private int _value1;
        private int _value2;
        private Array _outputArray;
        private int _outputValue;

        public ArrayOperations()
        {
            _settings = new ArrayOperationsSettings();            
        }

        #region IPlugin Members

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public ISettings Settings
        {
            get { return _settings; }
        }

        public System.Windows.Controls.UserControl Presentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
            _array1 = null;
            _array2 = null;
            _value1 = int.MinValue;
            _value2 = int.MinValue;

        }

        public void Execute()
        {

            //If connector values are not set, maybe the user set these values in the settings
            //So we replace the connector values with the setting values:
            if (_array1 == null)
            {
                _array1 = _settings.Array1;
            }
            if (_array2 == null)
            {
                _array2 = _settings.Array2;
            }
            if (_value1 == int.MaxValue)
            {
                _value1 = _settings.Value1;
            }
            if (_value2 == int.MaxValue)
            {
                _value2 = _settings.Value2;
            }
            if (_object1 == null)
            {
                _object1 = null;
            }

            try
            {
                object[] result = new object[0];
                object[] all = new object[0];
                switch (_settings.Operation)
                {
                    case ArrayOperationType.Sum:
                        all = new object[_array1.Length + _array2.Length];
                        result = new object[_array1.Length + _array2.Length];
                        _array1.CopyTo(all, 0);
                        _array2.CopyTo(all, _array1.Length);

                        int curidx = 0;
                        int uniques = 0;
                        for (int i = 0; i < all.Length; i++)
                        {
                            for (int j = 0; j < uniques; j++)
                            {
                                if (all[i] == result[j])
                                {
                                    break;
                                }
                            }

                            result[uniques] = all[i];
                            uniques = uniques + 1;
                        }
                        object[] endresult = new object[uniques];
                        for (int i=0; i<uniques; i++)
                        {
                            endresult[i] = result[i];
                        }
                        _outputArray = endresult;

                        OnPropertyChanged("OutputArray");
                        break;
                    case ArrayOperationType.Union:
                        break;
                    case ArrayOperationType.Difference:
                        break;
                    case ArrayOperationType.Concatenation:
                        break;
                    case ArrayOperationType.Unique:
                        break;
                    case ArrayOperationType.Length:

                        break;
                    case ArrayOperationType.Equals:
                        bool isEqual = true;
                        for (int i = 0; i < _array1.Length; i++)
                        {
//                            if (_array1[i] != _array2[i])
//                            {
//                                isEqual = false; break;
//                            }
                        }
                        _object1 = isEqual;
                        OnPropertyChanged("OutputValue");
                        break;
                    case ArrayOperationType.IndexOf:
                        break;
                    case ArrayOperationType.Replace:
                        break;
                    case ArrayOperationType.Sort:
                        break;
                }
                ProgressChanged(1, 1);
            }
            catch (Exception ex)
            {
                GuiLogMessage("Error", NotificationLevel.Error);
            }
        }


        public void PostExecution()
        {

        }

        public void Stop()
        {

        }

        public void Initialize()
        {
            _settings.UpdateTaskPaneVisibility();
        }

        public void Dispose()
        {

        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion

        [PropertyInfo(Direction.InputData, "Array1Caption", "Array1Tooltip", false)]
        public Array Array1
        {
            get { return _array1; }
            set { _array1 = value; }
        }

        [PropertyInfo(Direction.InputData, "Array2Caption", "Array2Tooltip", false)]
        public Array Array2
        {
            get { return _array2; }
            set { _array2 = value; }
        }

        [PropertyInfo(Direction.InputData, "Value1Caption", "Value1Tooltip", false)]
        public int Value1
        {
            get { return _value1; }
            set { _value1 = value; }
        }

        [PropertyInfo(Direction.InputData, "Value2Caption", "Value2Tooltip", false)]
        public int Value2
        {
            get { return _value2; }
            set { _value2 = value; }
        }

        [PropertyInfo(Direction.InputData, "Object1Caption", "Object1Tooltip", false)]
        public Object Object1
        {
            get { return _object1; }
            set { _object1 = value; }
        }

        [PropertyInfo(Direction.OutputData, "OutputArrayCaption", "OutputArrayTooltip", false)]
        public Array OutputArray
        {
            get { return _outputArray; }
        }

        [PropertyInfo(Direction.OutputData, "OutputValueCaption", "OutputValueTooltip", false)]
        public Object OutputValue
        {
            get { return _outputValue; }
        }

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, logLevel));
            }
        }

        public void ProgressChanged(double value, double max)
        {
            if (OnPluginProgressChanged != null)
            {
                OnPluginProgressChanged(this, new PluginProgressEventArgs(value, max));
            }
        }
    }
}
