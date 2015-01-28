﻿/*
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
using System.Windows.Input;

namespace Cryptool.Plugins.M_138
{
    [Author("Nils Rehwald", "nilsrehwald@gmail.com", "Uni Kassel", "http://www.uni-kassel.de/eecs/fachgebiete/ais/")]
    [PluginInfo("M_138.Properties.Resources", "PluginCaption", "PluginCaptionTooltip", "M_138/userdoc.xml", new[] { "CrypWin/images/default.png" })]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class M_138 : ICrypComponent
    {
        #region Private Variables

        private readonly M_138Settings settings = new M_138Settings();
        private readonly M138Visualisation visualisation = new M138Visualisation();
        enum Commands { Encrypt, Decryp };
        private bool _stopped = true;
        private string[,] toVisualize;
        private string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private List<string> stripes = new List<string>();
        private int _numberOfStripes = 0;
        private int[] TextNumbers;
        private char _separatorStripes = ',';
        private char _separatorOffset = '/';
        int _offset;
        int[] _stripNumbers = null;
        private List<int[]> numStripes = new List<int[]>();
        private int _invalidChar = 0;
        private List<string> _ignoredCharacters = new List<string>();
        private bool _isCaseSensitive = false;


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
            _ignoredCharacters.Clear();
            toVisualize = null;
            _isCaseSensitive = settings.CaseSensitivity;
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);
            readStripes();
            setSeparator();
            _isCaseSensitive = settings.CaseSensitivity;
            if (_isCaseSensitive)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(alphabet.ToUpper());
                sb.Append(alphabet.ToLower());
                alphabet = sb.ToString();
            }
            _invalidChar = settings.InvalidCharacterHandling;
            if (!_isCaseSensitive)
            {
                TextInput = TextInput.ToUpper();
            }
            if (_invalidChar == 0) //Remove
            {
                TextInput = RemoveInvalidChars(TextInput, alphabet);
            }
            else
            {
                TextInput = TextInput;
            }
            TextNumbers = MapTextIntoNumberSpace(TextInput, alphabet, _invalidChar);
            splitKey();
            if (_offset > alphabet.Length)
            {
                GuiLogMessage("Offset "+_offset+" is larger than strip length "+alphabet.Length+" and will be truncated", NotificationLevel.Warning);
                _offset = _offset % alphabet.Length;
            }
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
            StringBuilder sb = new StringBuilder();
            using (var fileStream = new FileStream(Path.Combine(DirectoryHelper.DirectoryCrypPlugins, "stripes.txt"), FileMode.Open, FileAccess.Read))
            {
                using (var file = new StreamReader(fileStream))
                {
                    string line = "";
                    while ((line = file.ReadLine()) != null)
                    {
                        if (!_isCaseSensitive)
                        {
                            stripes.Add(line);
                        }
                        else
                        {
                            sb.Append(line.ToUpper());
                            sb.Append(line.ToLower());
                            stripes.Add(sb.ToString());
                            sb.Clear();
                        }
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

        private int[] MapTextIntoNumberSpace(string text, string alphabet, int inv)
        {
            var numbers = new int[text.Length];
            var position = 0;
            if (inv == 0)
            {
                foreach (char c in text)
                {
                    numbers[position] = alphabet.IndexOf(c);
                    position++;
                }
            }
            else
            {
                foreach (char c in text)
                {
                    if(alphabet.Contains(c.ToString())) {
                        numbers[position] = alphabet.IndexOf(c);
                    }
                    else {
                        numbers[position] = -1;
                        if(inv==1) {
                            _ignoredCharacters.Add(c.ToString());
                        }
                    }
                    position++;
                }
            }
            return numbers;
        }

        private string MapNumbersIntoTextSpace(int[] numbers, string alphabet, int inv)
        {
            var builder = new StringBuilder();
            int counter = 0;
            if (inv == 0)
            {
                foreach (char c in numbers)
                {
                    builder.Append(alphabet[c]);
                }
            }
            else
            {
                foreach (char c in numbers)
                {
                    if (c == 65535)
                    {
                        if (inv == 1)
                        {
                            builder.Append(_ignoredCharacters[counter]);
                            counter++;
                        }
                        else
                        {
                            builder.Append('?');
                        }
                    }
                    else
                    {
                        builder.Append(alphabet[c]);
                    }
                }
            }
            return builder.ToString();
        }

        private void DeEnCrypt(int deOrEncrypt)
        {
            int _rows = TextNumbers.Length;
            int _columns = stripes[0].Length;
            int[] output = new int[_rows];
            toVisualize = new String[_rows + 1, _columns + 2];

            for (int r = 0; r < _stripNumbers.Length; r++) //Create a List of all used Stripes mapped to numbers instead of characters
            {
                numStripes.Add(MapTextIntoNumberSpace(stripes[_stripNumbers[r]], alphabet, _invalidChar));
            }
           
            for(int r=0; r<_rows; r++) {
                int _usedStrip = r % _stripNumbers.Length;
                toVisualize[r + 1, 1] = _stripNumbers[_usedStrip].ToString(); //Fill second column of Visualisation
                toVisualize[r + 1, 0] = (r+1).ToString(); //Fill first column of Visualisation
                int[] currentStrip = numStripes[_usedStrip];
                int isAt;
                int counter = 0;
                if (TextNumbers[r] < 0)
                {
                    isAt = -1;
                }
                else
                {
                    isAt = Array.IndexOf(currentStrip, TextNumbers[r]); //Location of the Plaintext letter
                }
                //NEW
                for (int c = 0; c < _columns; c++)
                {
                    toVisualize[0, c+2] = c.ToString(); //First row of Visualisation
                    if (deOrEncrypt == 1)
                    {
                        if (isAt != -1)
                        {
                            toVisualize[r + 1, c + 2] = alphabet[currentStrip[(isAt + c) % currentStrip.Length]].ToString(); //Rest of Visualisation
                        }
                        else
                        {
                            toVisualize[r+1, c+2] = "?"; //Can't show strips for invalid characters
                        }
                    }
                    else if (deOrEncrypt == 2)
                    {
                        if (isAt != -1)
                        {
                            toVisualize[r + 1, c + 2] = alphabet[currentStrip[(isAt - c + alphabet.Length) % currentStrip.Length]].ToString(); //Rest of Visualisation
                        }
                        else if (_invalidChar == 1)
                        {
                            toVisualize[r + 1, c + 2] = _ignoredCharacters[counter];
                            counter++;
                        }
                        else
                        {
                            toVisualize[r + 1, c + 2] = "?";
                        }
                    }
                    else
                    {
                        //This should never happen
                    }
                    
                }
                switch (deOrEncrypt)
                {
                    case 1:
                        if (isAt != -1)
                        {
                            output[r] = currentStrip[(isAt + _offset) % alphabet.Length];
                        }
                        else
                        {
                            output[r] = -1;
                        }
                        break;
                    case 2:
                        if (isAt != -1)
                        {
                            output[r] = currentStrip[(isAt - _offset + alphabet.Length) % alphabet.Length];
                        }
                        else
                        {
                            output[r] = -1;
                        }
                        break;
                    default:
                        //This should never happen
                        break;
                }
                
            }
            toVisualize[0, 1] = "Strip"; ; //Top Left field
            toVisualize[0, 0] = "Row"; //Top right field

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
 
            TextOutput = MapNumbersIntoTextSpace(output, alphabet, _invalidChar);

            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate {
                try
                {
                    visualisation.DataContext = this;
                    Binding2DArrayToListView(visualisation.lvwArray, tmpToVis, colNames);
                }
                catch (Exception e)
                {
                    GuiLogMessage(e.StackTrace, NotificationLevel.Error);
                }
            }, null);
        }

        private void Encrypt()
        {
            DeEnCrypt(1);
        }
        private void Decrypt()
        {
            DeEnCrypt(2);
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
                _stripNumbers[i] = Convert.ToInt32(s1[i]); //-1
                if (_stripNumbers[i] > stripes.Count)
                {
                    GuiLogMessage("Selected strip " + _stripNumbers[i] + " is larger than the ammount of available stripes " + stripes.Count, NotificationLevel.Error);
                }
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
            //Testing//
            gv.AllowsColumnReorder = false;
            var variableblablabla = gv.Columns;
            
            //Testing//
            listview.View = gv;
            listview.ItemsSource = dt.Rows;
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

        private void OnButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

        }

        #endregion
    }
}
