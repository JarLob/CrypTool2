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
using System.Linq;

namespace Cryptool.Plugins.M_138
{
    [Author("Nils Rehwald", "nilsrehwald@gmail.com", "Uni Kassel", "http://www.uni-kassel.de/eecs/fachgebiete/ais/")]
    [PluginInfo("M-138.Properties.Resources", "PluginCaption", "PluginCaptionTooltip", "M_138/userdoc.xml", new[] { "CrypWin/images/default.png" })]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class M_138 : ICrypComponent
    {
        #region Private Variables

        private readonly M_138Settings settings = new M_138Settings();
        private readonly M138Visualisation visualisation = new M138Visualisation();
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
            int _rows = TextNumbers.Length;
            Console.Write(_rows + " Zeilen \n");
            int _columns = stripes[0].Length;
            Console.Write(_columns + " Spalten \n");
            int[] output = new int[_rows];
            toVisualize = new String[_rows + 1, _columns + 2];

            for (int r = 0; r < _stripNumbers.Length; r++) //Create a List of all used Stripes mapped to numbers instead of characters
            {
                numStripes.Add(MapTextIntoNumberSpace(stripes[_stripNumbers[r]], alphabet));
            }
            if (_stripNumbers.Length > 25)
            {
                GuiLogMessage("Number of stripes used should not exceed 25", NotificationLevel.Warning);
            }
           
            for(int r=0; r<_rows; r++) {
                int _usedStrip = r % _stripNumbers.Length;
                toVisualize[r + 1, 0] = (_stripNumbers[r]+1).ToString(); //Fill first column of Visualisation
                toVisualize[r+1, _columns + 1] = (r+1).ToString(); //Fill last column of Visualisation
                int[] currentStrip = numStripes[_usedStrip];
                int isAt = Array.IndexOf(currentStrip, TextNumbers[r]); //Location of the Plaintext letter
                //NEW
                for (int c = 0; c < _columns; c++)
                {
                    toVisualize[0, c+1] = (c+1).ToString(); //First row of Visualisation
                    toVisualize[r + 1, c + 1] = alphabet[currentStrip[(isAt + c) % currentStrip.Length]].ToString(); //Rest of Visualisation
                }
                output[r] = currentStrip[(isAt + _offset) % alphabet.Length];
            }
            toVisualize[0, 0] = "Stripnumber"; ; //Top Left field
            toVisualize[0, _columns + 1] = "Row"; //Top right field

            /*
            List<List<string>> l = new List<List<string>>();
            
            for (int i = 0; i < _rows+1; i++)
            {
                List<string> q = new List<String>();
                for (int j = 0; j < _columns + 2; j++)
                {
                    q.Add(toVisualize[i, j]);
                }
                l.Add(q);
            }*/
            /*
            Console.Write("als liste-----------------");
            foreach(List<String> t in l) {
                foreach (String s in t)
                {
                    Console.Write(s+"\t");
                }
                Console.Write("\n");

            }
            */
            string[] colNames = new string[_columns+2];
            for(int i=0; i<_columns+2; i++) {
                colNames[i] = toVisualize[0,i];
            }
            string[,] tmpToVis = new string[_rows, _columns+2];
            for(int i=0; i<_rows;i++) {
                for(int j=0; j<_columns+2; j++) {
                    tmpToVis[i,j] = toVisualize[i+1,j];
                }
            }
 

            //printArray(toVisualize, _rows + 1, _columns + 2);
            TextOutput = MapNumbersIntoTextSpace(output, alphabet);

            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate {
                try
                {
                    visualisation.DataContext = this;
                    Binding2DArrayToListView(visualisation.lvwArray, tmpToVis, colNames);
                    
                    //visualisation.c_dataGrid.ItemsSource = l;
                }
                catch (Exception e)
                {
                    GuiLogMessage(e.StackTrace, NotificationLevel.Error);
                }
            }, null);
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
        private void Binding2DArrayToListView (ListView listview, string[,] data, string[] columnNames)
        {
            Check2DArrayMatchColumnNames(data, columnNames);

            DataTable dt = Convert2DArrayToDataTable(data, columnNames);

            GridView gv = new GridView();
            for (int i = 0; i < data.GetLength(1); i++)
            {
                GridViewColumn col = new GridViewColumn();
                col.Header = columnNames[i];
                col.DisplayMemberBinding = new Binding("[" + i + "]");
                gv.Columns.Add(col);
            }

            listview.View = gv;
            listview.ItemsSource = dt.Rows;
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

        private DataTable Convert2DArrayToDataTable(string[,] data, string[] columnNames)
        {
            int len1d = data.GetLength(0);
            int len2d = data.GetLength(1);
            Check2DArrayMatchColumnNames(data, columnNames);

            DataTable dt = new DataTable();
            for (int i = 0; i < len2d; i++)
            {
                dt.Columns.Add(columnNames[i], typeof(string));
            }

            for (int row = 0; row < len1d; row++)
            {
                DataRow dr = dt.NewRow();
                for (int col = 0; col < len2d; col++)
                {
                    dr[col] = data[row, col];
                }
                dt.Rows.Add(dr);
            }

            return dt;
        }

        private void Check2DArrayMatchColumnNames(string[,] data, string[] columnNames)
        {
            int len2d = data.GetLength(1);

            if (len2d != columnNames.Length)
            {
                throw new Exception("The second dimensional length must equals column names.");
            }
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
    public class TableToVisualize
    {
        public string[,] Zeile { get; set; }
    }

}
