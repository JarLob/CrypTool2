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
using System.Windows.Input;
using System.Windows.Media;

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
        List<int> _characterCases;
        private string[,] toVisualize;
        private static string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
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
        string[,] tmpToVis;
        string[] colNames;



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
            get
            {
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
            try
            {
                Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    Presentation.Visibility = Visibility.Visible;
                }, null);
            }
            catch (Exception e)
            {
            }
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);

            //Invalid character handling
            _invalidChar = settings.InvalidCharacterHandling;
            if (_invalidChar == 0) //Remove
            {
                TextInput = RemoveInvalidChars(TextInput, alphabet);
            }
            else
            {
                _ignoredCharacters = new List<string>();
            }

            //Case sensitivity
            _isCaseSensitive = settings.CaseSensitivity;
            if (_isCaseSensitive)
            {
                _characterCases = new List<int>(); //Save Cases of characters. 0 -> Lower case, 1 -> upper case
                int i = 0;
                foreach (char c in TextInput.ToArray())
                {
                    if (Char.IsUpper(c))
                    {
                        _characterCases.Add(1);
                    }
                    else if (Char.IsLower(c))
                    {
                        _characterCases.Add(0);
                    }
                    else
                    {
                        _characterCases.Add(2);
                        //Special Characters are neither upper nor lower case
                    }
                    i++;
                }
            }
            TextInput = TextInput.ToUpper(); //Just use upper cases internally

            setSeparator();
            TextNumbers = MapTextIntoNumberSpace(TextInput, alphabet, _invalidChar);
            splitKey();

            if (_offset > alphabet.Length)
            {
                GuiLogMessage("Offset " + _offset + " is larger than strip length " + alphabet.Length + " and will be truncated", NotificationLevel.Warning);
                _offset = _offset % alphabet.Length;
            }

            switch (settings.ModificationType)
            {
                case (int)Commands.Encrypt:
                    Encrypt();
                    break;
                case (int)Commands.Decryp:
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
            readStripes();
            try
            {
                Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    Presentation.Visibility = Visibility.Hidden;
                }, null);
            }
            catch (Exception e)
            {
            }
            visualisation.IsVisibleChanged += visibilityHasChanged;
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
                if (alphabet.Contains(c.ToString()) | alphabet.Contains(c.ToString().ToUpper()))
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
                    if (alphabet.Contains(c.ToString()))
                    {
                        numbers[position] = alphabet.IndexOf(c);
                    }
                    else
                    {
                        numbers[position] = -1;
                        if (inv == 1)
                        {
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

            numStripes.Clear();
            for (int r = 0; r < _stripNumbers.Length; r++) //Create a List of all used Stripes mapped to numbers instead of characters
            {
                numStripes.Add(MapTextIntoNumberSpace(stripes[_stripNumbers[r]], alphabet, _invalidChar));
            }

            int r_counter = 0;
            for (int r = 0; r < _rows; r++)
            {
                int _usedStrip = r_counter % _stripNumbers.Length;
                toVisualize[r + 1, 0] = (r + 1).ToString(); //Fill first column of Visualisation
                toVisualize[r + 1, 1] = _stripNumbers[_usedStrip].ToString(); //Fill second column of Visualisation
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

                for (int c = 0; c < _columns; c++)
                {
                    toVisualize[0, c + 2] = c.ToString(); //First row of Visualisation
                    if (deOrEncrypt == (int)Commands.Encrypt)
                    {
                        if (isAt != -1)
                        {
                            toVisualize[r + 1, c + 2] = alphabet[currentStrip[(isAt + c) % currentStrip.Length]].ToString(); //Rest of Visualisation
                            r_counter++;
                        }
                        else
                        {
                            toVisualize[r + 1, c + 2] = "?"; //Can't show strips for invalid characters
                        }
                    }
                    else if (deOrEncrypt == (int)Commands.Decryp)
                    {
                        if (isAt != -1)
                        {
                            toVisualize[r + 1, c + 2] = alphabet[currentStrip[(isAt - c + alphabet.Length) % currentStrip.Length]].ToString(); //Rest of Visualisation
                            r_counter++;
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
                    case (int)Commands.Encrypt:
                        if (isAt != -1)
                        {
                            output[r] = currentStrip[(isAt + _offset) % alphabet.Length];
                        }
                        else
                        {
                            output[r] = -1;
                        }
                        break;
                    case (int)Commands.Decryp:
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

            //Column Headers for Visualisation
            colNames = new string[_columns + 2];
            for (int i = 0; i < _columns + 2; i++)
            {
                colNames[i] = toVisualize[0, i];
            }

            tmpToVis = new string[_rows, _columns + 2];
            for (int i = 0; i < (_rows); i++)
            {
                for (int j = 0; j < _columns + 2; j++)
                {
                    tmpToVis[i, j] = toVisualize[i + 1, j];
                }
            }

            String tmpOutput = MapNumbersIntoTextSpace(output, alphabet, _invalidChar);
            if (_isCaseSensitive)
            {
                StringBuilder tmpStringBuilder = new StringBuilder();
                int i = 0;

                foreach (char c in tmpOutput.ToArray())
                {
                    if (_characterCases[i] == 0)
                    {
                        tmpStringBuilder.Append(Char.ToLower(tmpOutput[i]));
                    }
                    else if (_characterCases[i] == 1)
                    {
                        tmpStringBuilder.Append(Char.ToUpper(tmpOutput[i]));
                    }
                    else
                    {
                        tmpStringBuilder.Append(tmpOutput[i]);
                    }
                    i++;
                }
                tmpOutput = tmpStringBuilder.ToString();
            }

            TextOutput = tmpOutput;

            if (visualisation.IsVisible)
            {
                UpdateGUI();
            }
        }

        private void UpdateGUI()
        {

            try
            {
                Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    try
                    {
                        Binding2DArrayToListView(visualisation.lvwArray, tmpToVis, colNames);
                    }
                    catch (Exception e)
                    {
                        //GuiLogMessage(e.StackTrace, NotificationLevel.Error);
                    }
                }, null);
            }
            catch (Exception e)
            {
            }

        }

        private void visibilityHasChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (visualisation.IsVisible)
            {
                UpdateGUI();
            }
        }

        private void Encrypt()
        {
            DeEnCrypt((int)Commands.Encrypt);
        }
        private void Decrypt()
        {
            DeEnCrypt((int)Commands.Decryp);
        }

        private void setSeparator()
        {
            switch (settings.SeparatorStripChar)
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
            }

            switch (settings.SeparatorOffChar)
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
            if (s1[s1.Length - 1].Contains("/") || s1[s1.Length - 1].Contains(",") || s1[s1.Length - 1].Contains(";") || s1[s1.Length - 1].Contains(".") || s1[s1.Length - 1].Equals(""))
            {
                return;
            }
            _stripNumbers = new int[s1.Length];
            for (int i = 0; i < s1.Length; i++)
            {
                _stripNumbers[i] = Convert.ToInt32(s1[i]);
                if (_stripNumbers[i] > stripes.Count)
                {
                    GuiLogMessage("Selected strip " + _stripNumbers[i] + " is larger than the ammount of available stripes " + stripes.Count + ". Using default strip 1 instead", NotificationLevel.Error);
                    _stripNumbers[i] = 1;
                }
            }
        }
        private void Binding2DArrayToListView(DataGrid dataGrid, string[,] data, string[] columnNames)
        {
            dataGrid.AutoGeneratingColumn += dgvMailingList_AutoGeneratingColumn;
            Check2DArrayMatchColumnNames(data, columnNames);
            DataTable dt = Convert2DArrayToDataTable(data, columnNames);
            dataGrid.ItemsSource = dt.DefaultView;

        }

        private void dgvMailingList_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column.Header.Equals("0"))
            {
                e.Column.CellStyle = new Style(typeof(DataGridCell));
                e.Column.CellStyle.Setters.Add(new Setter(DataGridCell.BackgroundProperty, new SolidColorBrush(Colors.Green)));
                e.Column.CellStyle.Setters.Add(new Setter(DataGridCell.FontWeightProperty, FontWeights.Bold));
                e.Column.CellStyle.Setters.Add(new Setter(DataGridCell.PaddingProperty, new Thickness(10)));
                e.Column.CellStyle.Setters.Add(new Setter(DataGridCell.HorizontalAlignmentProperty, HorizontalAlignment.Stretch));
                e.Column.CellStyle.Setters.Add(new Setter(DataGridCell.VerticalAlignmentProperty, VerticalAlignment.Stretch));
                e.Column.CellStyle.Setters.Add(new Setter(DataGridCell.HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
                e.Column.CellStyle.Setters.Add(new Setter(DataGridCell.VerticalContentAlignmentProperty, VerticalAlignment.Center));
                e.Column.CellStyle.Setters.Add(new Setter(DataGridCell.MinHeightProperty, Double.Parse("30")));
                e.Column.CellStyle.Setters.Add(new Setter(DataGridCell.MinWidthProperty, Double.Parse("30")));
            }
            else if (e.Column.Header.Equals(_offset.ToString()))
            {
                e.Column.CellStyle = new Style(typeof(DataGridCell));
                e.Column.CellStyle.Setters.Add(new Setter(DataGridCell.BackgroundProperty, new SolidColorBrush(Colors.Red)));
                e.Column.CellStyle.Setters.Add(new Setter(DataGridCell.FontWeightProperty, FontWeights.Bold));
                e.Column.CellStyle.Setters.Add(new Setter(DataGridCell.PaddingProperty, new Thickness(10)));
                e.Column.CellStyle.Setters.Add(new Setter(DataGridCell.HorizontalAlignmentProperty, HorizontalAlignment.Stretch));
                e.Column.CellStyle.Setters.Add(new Setter(DataGridCell.VerticalAlignmentProperty, VerticalAlignment.Stretch));
                e.Column.CellStyle.Setters.Add(new Setter(DataGridCell.HorizontalContentAlignmentProperty, HorizontalAlignment.Center));
                e.Column.CellStyle.Setters.Add(new Setter(DataGridCell.VerticalContentAlignmentProperty, VerticalAlignment.Center));
            }
            return;
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
}
