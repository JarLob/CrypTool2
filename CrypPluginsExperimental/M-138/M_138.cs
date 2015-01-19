/*
   Copyright 2014 Nils Rehwald

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
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.Collections.Generic;
using System.Text;
using System;
using Cryptool.PluginBase.IO;
using System.IO;
using M_138;
using System.Threading;
using System.Windows.Threading;
using System.Data;
using System.Windows.Data;
using System.Windows;

namespace Cryptool.Plugins.M_138
{
    [Author("Nils Rehwald", "nilsrehwald@gmail.com", "Uni Kassel", "http://www.uni-kassel.de/eecs/fachgebiete/ais/")]
    [PluginInfo("M-138.Properties.Resources", "PluginCaption", "PluginCaptionTooltip", "M_138/userdoc.xml", new[] { "CrypWin/images/default.png" })]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class M_138 : ICrypComponent
    {
        #region Private Variables

        private readonly M_138Settings settings = new M_138Settings();
        enum Commands { Encrypt, Decryp };
        private bool _stopped = true;
        private string[,] toVisualize;
        private const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private List<string> stripes = new List<string>();
        private int _numberOfStripes = 0;
        private int[] TextNumbers;
        private char _separatorStripes = ',';
        private char _separatorOffset = '/';
        int _offset;
        int[] _stripNumbers;
        private List<int[]> numStripes = new List<int[]>();
        public M138Visualisation visualisation;
        private string[,] visualStripes;


        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "TextInputCaption", "TextInputDescription")]
        public string TextInput
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "KeyCaption", "KeyDescription")]
        public string Key
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "TextOutputCaption", "TextOutputDescription")]
        public string TextOutput
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings
        {
            get { return settings; }
        }

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation
        {
            get {
                visualisation = new M138Visualisation();
                return visualisation;
            }
            //get { return null; }
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            _stopped = false;
            readStripes();
            setSeparator();
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);
            TextInput = RemoveInvalidChars(TextInput.ToUpper(), alphabet);
            TextNumbers = MapTextIntoNumberSpace(TextInput, alphabet);
            splitKey();
            switch (settings.ModificationType) {
                case (int) Commands.Encrypt:
                    Encrypt();
                    break;
                case (int) Commands.Decryp:
                    Decrypt();
                    break;
                default:
                    GuiLogMessage("Invalid Selection", NotificationLevel.Error);
                    return; 
            }
            OnPropertyChanged("TextOutput");
            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
            _stopped = true;
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
            _stopped = true;
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion

        #region Helpers
        private void readStripes()
        {
            using (var fileStream = new FileStream(Path.Combine(DirectoryHelper.DirectoryCrypPlugins, "stripes.txt"), FileMode.Open, FileAccess.Read))
            {
                //System.IO.StreamReader file = new System.IO.StreamReader("../../CrypPluginsExperimental/M-138/stripes.txt"); //TODO: Path? Oder lokal?
                using (var file = new StreamReader(fileStream))
                {
                    string line = "";
                    while ((line = file.ReadLine()) != null)
                    {
                        stripes.Add(line);
                    }
                    file.Close();
                }
            }
        }


        private string RemoveInvalidChars(string text, string alphabet)
        {
            var builder = new StringBuilder();
            foreach (char c in text)
            {
                if (alphabet.Contains(c.ToString()))
                {
                    builder.Append(c);
                }
            }
            return builder.ToString();
        }

        private int[] MapTextIntoNumberSpace(string text, string alphabet)
        {
            var numbers = new int[text.Length];
            var position = 0;
            foreach (char c in text)
            {
                numbers[position] = alphabet.IndexOf(c);
                position++;
            }
            return numbers;
        }

        private string MapNumbersIntoTextSpace(int[] numbers, string alphabet)
        {
            var builder = new StringBuilder();
            foreach (char c in numbers)
            {
                builder.Append(alphabet[c]);
            }
            return builder.ToString();
        }

        private void Encrypt()
        {
            int _textlen = TextNumbers.Length;
            int[] output = new int[_textlen];
            //NEW
            visualStripes = new string[_textlen,stripes[0].Length];
            //
            for (int i = 0; i < _stripNumbers.Length; i++) //Create a List of all used Stripes mapped to numbers instead of characters
            {
                numStripes.Add(MapTextIntoNumberSpace(stripes[_stripNumbers[i]], alphabet));
            }
            if (_stripNumbers.Length > 25)
            {
                GuiLogMessage("Number of stripes used should not exceed 25", NotificationLevel.Warning);
            }
            for(int i=0; i<_textlen; i++) {
                int _usedStrip = i % _stripNumbers.Length;
                int[] currentStrip = numStripes[_usedStrip];
                int isAt = Array.IndexOf(currentStrip, TextNumbers[i]);
                //NEW
                for (int j = 0; j < currentStrip.Length; j++)
                {
                    visualStripes[i, j] = alphabet[currentStrip[(isAt + j) % currentStrip.Length]].ToString();
                }
                //
                output[i] = currentStrip[(isAt + _offset) % alphabet.Length];
            }
            
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    /*
                    toVisualize = fillArray(_textlen, stripes[0].Length, _stripNumbers);
                    Bindng2DArrayToListview2(visualisation._dataGrid, toVisualize);
                     * */
                    visualisation.c_dataGrid.ItemsSource = GetBindable2DArray<string>(toVisualize);
                }
                catch
                {

                }
            });
             
            //NEW
            /*
            visualisation.setStripes(visualStripes);
            visualisation.fillArray(_textlen, stripes[0].Length, _stripNumbers);
            visualisation.setOffset(_offset);
             */
            //
            TextOutput = MapNumbersIntoTextSpace(output, alphabet);
        }

        private void Decrypt()
        {
            int _textlen = TextNumbers.Length;
            int[] output = new int[_textlen];
            for (int i = 0; i < _stripNumbers.Length; i++) //Create a List of all used Stripes mapped to numbers instead of characters
            {
                numStripes.Add(MapTextIntoNumberSpace(stripes[_stripNumbers[i]], alphabet));
            }
            if (_stripNumbers.Length > 25)
            {
                GuiLogMessage("Number of stripes used should not exceed 25", NotificationLevel.Warning);
            }
            for (int i = 0; i < _textlen; i++)
            {
                int _usedStrip = i % _stripNumbers.Length;
                int[] currentStrip = numStripes[_usedStrip];
                int isAt = Array.IndexOf(currentStrip, TextNumbers[i]);
                output[i] = currentStrip[(isAt - _offset + alphabet.Length) % alphabet.Length];
            }
            TextOutput = MapNumbersIntoTextSpace(output, alphabet);
        }

        private void setSeparator()
        {
            switch (settings.SeperatorStripChar)
            {
                case 0:
                    _separatorStripes = ',';
                    break;
                case 1:
                    _separatorStripes = '.';
                    break;
                case 2:
                    _separatorStripes = '/';
                    break;
                default:
                    break;
            }

            switch (settings.SeperatorOffChar)
            {
                case 0:
                    _separatorOffset = '/';
                    break;
                case 1:
                    _separatorOffset = ',';
                    break;
                case 2:
                    _separatorOffset = '.';
                    break;
            }
        }

        private void splitKey()
        {
            string[] splitted;
            splitted = Key.Split(_separatorOffset);
            _offset = Convert.ToInt32(splitted[1]);
            string[] s1 = splitted[0].Split(_separatorStripes);
            _stripNumbers = new int[s1.Length];
            for ( int i=0; i < s1.Length; i++) {
                _stripNumbers[i] = Convert.ToInt32(s1[i]) - 1;
            }
        }

        public string[,] fillArray(int r, int c, int[] stripes)
        {
            toVisualize = new string[r + 1, c + 2];
            for (int i = 0; i < c; i++) // Fill first Row
            {
                toVisualize[0, i] = i.ToString();
            }
            for (int i = 0; i < r + 1; i++) // Fill last column
            {
                toVisualize[i, c + 1] = i.ToString();
            }
            // Fill last Row
            for (int i = 1; i < r + 1; i++)
            {
                toVisualize[i, 0] = (stripes[i - 1] + 1).ToString();
            }
            // Fill rest of Array
            for (int i = 1; i < r + 1; i++)
            {
                for (int j = 1; j < c + 1; j++)
                {
                    toVisualize[i, j] = visualStripes[i - 1, j - 1];
                }
            }
            toVisualize[0, 0] = "Stripnumber";
            toVisualize[0, c + 1] = "Row";
            printArray(toVisualize, r + 1, c + 2);
            return toVisualize;
        }

        private void Bindng2DArrayToListview2 (ListView listview, string[,] data)
        {
            GridView gv = new GridView();
            for (int i = 0; i < data.GetLength(1); i++)
            {
                GridViewColumn col = new GridViewColumn();
                col.Header = data[0, i];
                col.DisplayMemberBinding = new System.Windows.Data.Binding("[" + i + "]");
                gv.Columns.Add(col);
            }

            ArrayVisitor arrayVisitor = new ArrayVisitor(data);
            listview.View = gv;
            listview.ItemsSource = arrayVisitor;
        }

        private void printArray(string[,] a, int r, int c)
        {
            for (int i = 0; i < r; i++)
            {
                for (int j = 0; j < c; j++)
                {
                    Console.Write(a[i, j] + "\t");
                }
                Console.Write("\n");
            }
        }

        public static DataView GetBindable2DArray<T>(T[,] array)
        {
            DataTable dataTable = new DataTable();
            for (int i = 0; i < array.GetLength(1); i++)
            {
                dataTable.Columns.Add(i.ToString(), typeof(Ref<T>));
            }
            for (int i = 0; i < array.GetLength(0); i++)
            {
                DataRow dataRow = dataTable.NewRow();
                dataTable.Rows.Add(dataRow);
            }
            DataView dataView = new DataView(dataTable);
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    int a = i;
                    int b = j;
                    Ref<T> refT = new Ref<T>(() => array[a, b], z => { array[a, b] = z; });
                    dataView[i][j] = refT;
                }
            }
            return dataView;
        }

        #endregion
    }
    class ArrayVisitor : IEnumerable<string[]>
    {
        private string[,] _data;

        public ArrayVisitor()
        {
        }

        public ArrayVisitor(string[,] data)
        {
            _data = data;
        }

        public string[,] Data
        {
            get { return _data; }
            set { _data = value; }
        }

        #region IEnumerable<string[]> Members

        public IEnumerator<string[]> GetEnumerator()
        {
            if (_data == null)
                throw new ArgumentException("Data cannot be null.", "Data");

            int len2d = _data.GetLength(1);

            for (int i = 0; i < _data.GetLength(0); i++)
            {
                string[] arr = new string[len2d];
                for (int j = 0; j < len2d; j++)
                {
                    arr[j] = _data[i, j];
                }

                yield return arr;
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
    public class Ref<T>
    {
        private readonly Func<T> getter;
        private readonly Action<T> setter;
        public Ref(Func<T> getter, Action<T> setter)
        {
            this.getter = getter;
            this.setter = setter;
        }
        public T Value { get { return getter(); } set { setter(value); } }
    } 
}
