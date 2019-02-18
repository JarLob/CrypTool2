/*
   Copyright 2019 Nils Kopal <Nils.Kopal<at>CrypTool.org

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
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Cryptool.Plugins.HomophonicSubstitutionAnalyzer
{
    /// <summary>
    /// Interaktionslogik für HomophoneSubstitutionAnalyzerPresentation.xaml
    /// </summary>
    public partial class HomophoneSubstitutionAnalyzerPresentation : UserControl
    {        
        private int _keylength = 0;
        private string PlainAlphabetText = "ABCDEFGHIJKLMNOPQRSTUVWXYZ ";
        private string CipherAlphabetText = "ABCDEFGHIJKLMNOPQRSTUVWXYZÄÜÖabcdefghijklmnopqrstuvwxyzäüöß1234567890ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩАБВГДЂЕЄЖЗЅИІЈКЛЉМНЊОПРСТЋУФХЦЧЏШЪЫЬЭЮЯ!§$%&=?#";
        private HillClimber _hillClimber;
        private WordFinder _wordFinder;
        private SymbolLabel[,] _ciphertextLabels;
        private SymbolLabel[,] _plaintextLabels;
        private TextBox[] _minTextBoxes;
        private TextBox[] _maxTextBoxes;
        private AnalyzerConfiguration _analyzerConfiguration = null;
        private string _ciphertext = null;
        private bool _running = false;
        
        public HomophoneSubstitutionAnalyzerPresentation()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes the ui with a new ciphertext
        /// </summary>
        /// <param name="ciphertext"></param>
        public void Initialize(string ciphertext)
        {
            //Statistics.Load5GramsGZ("en-5gram-nocs-sp.gz");
            _ciphertext = ciphertext;
            _keylength = (int)(Tools.Distinct(Tools.MapHomophoneIntoNumberSpace(ciphertext)).Length * 1.3);

            _analyzerConfiguration = new AnalyzerConfiguration(_keylength, Tools.ChangeToConsecutiveNumbers(Tools.MapHomophoneIntoNumberSpace(ciphertext)));
            _analyzerConfiguration.PlaintextAlphabet = PlainAlphabetText;
            _analyzerConfiguration.CiphertextAlphabet = CipherAlphabetText;
            _analyzerConfiguration.TextColumns = 60;
            _analyzerConfiguration.Cycles = 50000;
            _analyzerConfiguration.KeyLetterLimits = new List<LetterLimits>();
            _analyzerConfiguration.KeyLettersDistributionType = KeyLettersDistributionType.LetterLimits;
            _analyzerConfiguration.MinWordLength = 8;
            _analyzerConfiguration.MaxWordLength = 10;
            _analyzerConfiguration.WordCountToFind = 3;
            _analyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 0, MinValue = 3, MaxValue = 5 });   //A
            _analyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 1, MinValue = 1, MaxValue = 2 });   //B
            _analyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 2, MinValue = 1, MaxValue = 2 });   //C
            _analyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 3, MinValue = 1, MaxValue = 2 });   //D
            _analyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 4, MinValue = 4, MaxValue = 6 });   //E
            _analyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 5, MinValue = 1, MaxValue = 2 });   //F
            _analyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 6, MinValue = 1, MaxValue = 2 });   //G
            _analyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 7, MinValue = 1, MaxValue = 2 });   //H
            _analyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 8, MinValue = 3, MaxValue = 5 });   //I
            _analyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 9, MinValue = 1, MaxValue = 2 });   //J
            _analyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 10, MinValue = 1, MaxValue = 2 });   //K
            _analyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 11, MinValue = 1, MaxValue = 2 });   //L
            _analyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 12, MinValue = 1, MaxValue = 2 });   //M
            _analyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 13, MinValue = 2, MaxValue = 3 });   //N
            _analyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 14, MinValue = 3, MaxValue = 5 });   //O
            _analyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 15, MinValue = 1, MaxValue = 2 });   //P
            _analyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 16, MinValue = 1, MaxValue = 2 });   //Q
            _analyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 17, MinValue = 1, MaxValue = 2 });   //R
            _analyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 18, MinValue = 1, MaxValue = 2 });   //S
            _analyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 19, MinValue = 3, MaxValue = 5 });   //T
            _analyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 20, MinValue = 3, MaxValue = 5 });   //U
            _analyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 21, MinValue = 1, MaxValue = 2 });   //V
            _analyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 22, MinValue = 1, MaxValue = 2 });   //W
            _analyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 23, MinValue = 1, MaxValue = 2 });   //X
            _analyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 24, MinValue = 1, MaxValue = 2 });   //Y
            _analyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 25, MinValue = 1, MaxValue = 2 });   //Z
            _analyzerConfiguration.KeyLetterLimits.Add(new LetterLimits() { Letter = 26, MinValue = 2, MaxValue = 3 });   //SPACE            

            _wordFinder = new WordFinder("en-words.txt", _analyzerConfiguration.MinWordLength, _analyzerConfiguration.MaxWordLength, PlainAlphabetText);

            _hillClimber = new HillClimber(_analyzerConfiguration);
            _hillClimber.NewBestValue += HillClimberNewBestValue;
            _hillClimber.Progress += HillClimberProgress;

            GenerateCiphertextGrid(_analyzerConfiguration.Ciphertext, _analyzerConfiguration.TextColumns);
            GeneratePlaintextGrid(_analyzerConfiguration.Ciphertext, _analyzerConfiguration.TextColumns);

            GenerateKeyLetterListView();
        }


        /// <summary>
        /// Generates the Grid for the ciphertext and fills in the symbols
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="columns"></param>
        private void GenerateCiphertextGrid(int[] ciphertext, int columns)
        {
            int rows = (int)Math.Ceiling((double)ciphertext.Length / columns);
            _ciphertextLabels = new SymbolLabel[columns, rows];
            string text = Tools.MapNumbersIntoTextSpace(ciphertext, CipherAlphabetText);

            for (int column = 0; column < columns; column++)
            {
                CiphertextGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            CiphertextGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(int.MaxValue, GridUnitType.Star) });

            for (int row = 0; row < rows; row++)
            {
                CiphertextGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength() });
            }
            CiphertextGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            int offset = 0;
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    if (offset == ciphertext.Length)
                    {
                        break;
                    }

                    SymbolLabel label = new SymbolLabel();
                    label.X = x;
                    label.Y = y;
                    label.SymbolOffset = offset;
                    label.Symbol = "" + text[offset];
                    _ciphertextLabels[x, y] = label;
                    label.Width = 30;
                    label.Height = 30;
                    label.FontSize = 20;
                    label.FontFamily = new FontFamily("Courier New");
                    label.Content = text[offset];
                    offset++;
                    Grid.SetRow(label, y);
                    Grid.SetColumn(label, x);
                    CiphertextGrid.Children.Add(label);
                }
            }
        }

        /// <summary>
        /// Generates the Grid for the plaintext and fills in the symbols
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="columns"></param>
        private void GeneratePlaintextGrid(int[] plaintext, int columns)
        {
            int rows = (int)Math.Ceiling((double)plaintext.Length / columns);
            _plaintextLabels = new SymbolLabel[columns, rows];
            string text = Tools.MapNumbersIntoTextSpace(plaintext, CipherAlphabetText);

            for (int column = 0; column < columns; column++)
            {
                PlaintextGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            PlaintextGrid.ColumnDefinitions.Add(new ColumnDefinition() {Width = new GridLength(int.MaxValue, GridUnitType.Star)});

            for (int row = 0; row < rows; row++)
            {
                PlaintextGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength() });
            }
            PlaintextGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            int offset = 0;
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    if (offset == plaintext.Length)
                    {
                        break;
                    }
                    SymbolLabel label = new SymbolLabel();
                    label.MouseDown += PlainTextBox_MouseDown;
                    _plaintextLabels[x, y] = label;
                    label.X = x;
                    label.Y = y;
                    label.SymbolOffset = offset;
                    label.Symbol = "" + text[offset];
                    label.Width = 30;
                    label.Height = 30;
                    label.FontSize = 20;
                    label.FontFamily = new FontFamily("Courier New");
                    label.Content = text[offset];
                    offset++;
                    Grid.SetRow(label, y);
                    Grid.SetColumn(label, x);
                    PlaintextGrid.Children.Add(label);
                }
            }
        }

        /// <summary>
        /// Generates the tab for the selection of the key letter distribution
        /// </summary>
        /// <param name="PlainAlphabetText"></param>
        private void GenerateKeyLetterListView()
        {
            KeyLetterListView.Items.Clear();

            _minTextBoxes = new TextBox[_analyzerConfiguration.KeyLetterLimits.Count];
            _maxTextBoxes = new TextBox[_analyzerConfiguration.KeyLetterLimits.Count];

            int index = 0;
            foreach (LetterLimits limits in _analyzerConfiguration.KeyLetterLimits)
            {
                Grid grid = new Grid();
                grid.Width = 500;
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition());

                Label label = new Label();
                label.FontSize = 16;
                label.Width = 50;
                label.Content = String.Format("\"{0}\"", Tools.MapNumbersIntoTextSpace(new int[] {limits.Letter}, _analyzerConfiguration.PlaintextAlphabet));
                Grid.SetRow(label, 0);
                Grid.SetColumn(label, 0);

                TextBox minbox = new TextBox();
                _minTextBoxes[index] = minbox;
                minbox.Text = "" + limits.MinValue;
                minbox.VerticalContentAlignment = VerticalAlignment.Center;
                minbox.Width = 150;
                minbox.Height = 25;
                minbox.FontSize = 12;
                Grid.SetRow(minbox, 0);
                Grid.SetColumn(minbox, 1);

                TextBox maxbox = new TextBox();
                _maxTextBoxes[index] = maxbox;
                maxbox.Text = "" + limits.MaxValue;
                maxbox.VerticalContentAlignment = VerticalAlignment.Center;
                maxbox.Width = 150;
                maxbox.Height = 25;
                maxbox.FontSize = 12;
                Grid.SetRow(maxbox, 0);
                Grid.SetColumn(maxbox, 2);

                grid.Children.Add(label);
                grid.Children.Add(minbox);
                grid.Children.Add(maxbox);

                KeyLetterListView.Items.Add(grid);
                index++;
            }
        }

        /// <summary>
        /// Fired each time the progress changed, i.e. the analyzer finished a restart
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void HillClimberProgress(object sender, ProgressChangedEventArgs eventArgs)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                ProgressBar.Value = eventArgs.Percentage * 100;
                ProgressText.Content = String.Format("{0} %", Math.Round(eventArgs.Percentage, 2) * 100);
                if (eventArgs.Percentage >= 1)
                {
                    _running = false;
                    AnalyzeButton.Content = "Analyze";
                }
            }, null);
        }

        /// <summary>
        /// Fired each time the analyzer found a "better" cost value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void HillClimberNewBestValue(object sender, NewBestValueEventArgs eventArgs)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                int column = 0;
                int row = 0;
                foreach (var letter in eventArgs.Plaintext)
                {
                    _plaintextLabels[column, row].Content = letter;
                    _plaintextLabels[column, row].Symbol = "" + letter;
                    column++;
                    if (column == _analyzerConfiguration.TextColumns)
                    {
                        column = 0;
                        row++;
                    }
                }
                CipherAlphabetTextBox.Text = eventArgs.CiphertextAlphabet;
                PlainAlphabetTextBox.Text = eventArgs.PlaintextAlphabet;
                CostTextBox.Text = String.Format("Cost Value: {0}", Math.Round(eventArgs.CostValue, 2));

                AutoLockWords(_analyzerConfiguration.WordCountToFind);
                MarkLockedHomophones();

            }, null);
        }

        /// <summary>
        /// Mouse is down on a label of the plaintext
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="mouseButtonEventArgs"></param>
        private void PlainTextBox_MouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (_running)
            {
                return;
            }
            try
            {
                SymbolLabel label = (SymbolLabel)sender;
                if (mouseButtonEventArgs.LeftButton == MouseButtonState.Pressed)
                {
                    string symbol = _ciphertextLabels[label.X, label.Y].Symbol;
                    LockHomophone(symbol);
                }
                if (mouseButtonEventArgs.RightButton == MouseButtonState.Pressed)
                {
                    if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                    {
                        ChangeHomophone(_ciphertextLabels[label.X, label.Y].Symbol, -1);
                    }
                    else
                    {
                        ChangeHomophone(_ciphertextLabels[label.X, label.Y].Symbol, 1);
                    }
                }
            }
            catch (Exception)
            {
                //do nothing here
            }
        }

        /// <summary>
        /// Changes the mapping of a homophone
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="direction"></param>
        private void ChangeHomophone(string symbol, int direction)
        {
            var key = CipherAlphabetTextBox.Text;
            var index = key.IndexOf(symbol);
            if (index > -1)
            {
                if (_hillClimber.AnalyzerConfiguration.LockedHomophoneMappings[index] == -1)
                {
                    return;
                }

                _hillClimber.AnalyzerConfiguration.LockedHomophoneMappings[index] = (_hillClimber.AnalyzerConfiguration.LockedHomophoneMappings[index] + direction) % PlainAlphabetText.Length;
                if (_hillClimber.AnalyzerConfiguration.LockedHomophoneMappings[index] < 0)
                {
                    _hillClimber.AnalyzerConfiguration.LockedHomophoneMappings[index] = PlainAlphabetText.Length - 1;
                }

                //Update plainalphabet textbox
                PlainAlphabetTextBox.Text = PlainAlphabetTextBox.Text.Remove(index, 1);
                PlainAlphabetTextBox.Text = PlainAlphabetTextBox.Text.Insert(index, Tools.MapNumbersIntoTextSpace(new int[] { _hillClimber.AnalyzerConfiguration.LockedHomophoneMappings[index] }, PlainAlphabetText));

                //decrypt text using the key
                var ciphertext = Tools.ChangeToConsecutiveNumbers(Tools.MapHomophoneIntoNumberSpace(_ciphertext));
                var len = Tools.Distinct(ciphertext).Length;
                for (var i = 0; i < ciphertext.Length; i++)
                {
                    ciphertext[i] = ciphertext[i] % len;
                }
                var numkey = new HomophoneMapping[PlainAlphabetTextBox.Text.Length];
                var cipheralphabet = Tools.MapIntoNumberSpace(CipherAlphabetTextBox.Text, CipherAlphabetText);
                var plainalphabet = Tools.MapIntoNumberSpace(PlainAlphabetTextBox.Text, PlainAlphabetText);
                for (var i = 0; i < _keylength; i++)
                {
                    numkey[i] = new HomophoneMapping(ciphertext, cipheralphabet[i], plainalphabet[i]);
                }

                var plaintext = Tools.MapNumbersIntoTextSpace(HillClimber.DecryptHomophoneCipher(ciphertext, numkey), PlainAlphabetText);
                int column = 0;
                int row = 0;
                foreach (var letter in plaintext)
                {
                    _plaintextLabels[column, row].Content = letter;
                    _plaintextLabels[column, row].Symbol = "" + letter;
                    column++;
                    if (column == _analyzerConfiguration.TextColumns)
                    {
                        column = 0;
                        row++;
                    }
                }
                MarkLockedHomophones();
            }
        }

        /// <summary>
        /// Locks a mapping of a single mapping of a homophone
        /// </summary>
        /// <param name="symbol"></param>
        private void LockHomophone(string symbol, bool auto = false)
        {
            var key = CipherAlphabetTextBox.Text;
            var index = key.IndexOf(symbol);
            if (index > -1)
            {
                if (_hillClimber.AnalyzerConfiguration.LockedHomophoneMappings[index] == -1 || auto)
                {
                    _hillClimber.AnalyzerConfiguration.LockedHomophoneMappings[index] = Tools.MapIntoNumberSpace("" + PlainAlphabetTextBox.Text[index], PlainAlphabetText)[0];
                }
                else
                {
                    _hillClimber.AnalyzerConfiguration.LockedHomophoneMappings[index] = -1;
                }
                MarkLockedHomophones();
            }
        }

        /// <summary>
        /// Marks the locked homophones in the plaintext and ciphertext
        /// </summary>
        private void MarkLockedHomophones()
        {
            StringBuilder lockedElementsStringBuilder = new StringBuilder();
            for (var i = 0; i < _hillClimber.AnalyzerConfiguration.LockedHomophoneMappings.Length; i++)
            {
                if (_hillClimber.AnalyzerConfiguration.LockedHomophoneMappings[i] != -1)
                {
                    lockedElementsStringBuilder.Append(Tools.MapNumbersIntoTextSpace(new int[] { i }, CipherAlphabetText));
                }
            }
            var lockedElementsString = lockedElementsStringBuilder.ToString();

            foreach (var label in _ciphertextLabels)
            {
                if (label == null)
                {
                    continue;
                }
                if (lockedElementsString.Contains(label.Symbol))
                {
                    label.Background = Brushes.LightGreen;
                    _plaintextLabels[label.X, label.Y].Background = Brushes.LightGreen;
                }
                else
                {
                    label.Background = Brushes.White;
                    _plaintextLabels[label.X, label.Y].Background = Brushes.White;
                }
            }

        }

        /// <summary>
        /// Starts and stops the analysis
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Analyze_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                lock (this)
                {
                    if (_running == true)
                    {
                        _running = false;
                        _hillClimber.Stop();
                        AnalyzeButton.Content = "Analyze";
                    }
                    else
                    {
                        _running = true;
                        AnalyzeButton.Content = "Stop";
                        UpdateKeyLetterLimits();
                        ThreadStart threadStart = () => _hillClimber.Execute();
                        var thread = new Thread(threadStart);
                        thread.IsBackground = true;
                        thread.Start();
                    }
                }
            }
            catch (Exception)
            {
                //do nothing
                _running = false;
            }
        }

        /// <summary>
        /// Updates the key letter LetterLimits in the analyzer's config
        /// Also updates the ui; if a non-integer value has been entered, it is set to 0
        /// </summary>
        private void UpdateKeyLetterLimits()
        {
            for (int index = 0; index < _analyzerConfiguration.KeyLetterLimits.Count; index++)
            {
                int minvalue = 0;
                int maxvalue = 0;
                try
                {
                    minvalue = int.Parse(_minTextBoxes[index].Text);
                }
                catch (Exception ex)
                {
                    //do nothing
                }

                try
                {
                    maxvalue = int.Parse(_maxTextBoxes[index].Text);
                }
                catch (Exception ex)
                {
                    //do nothing
                }

                LetterLimits limits = _analyzerConfiguration.KeyLetterLimits[index];
                limits.MinValue = minvalue;
                limits.MaxValue = maxvalue;

                _minTextBoxes[index].Text = "" + minvalue;
                _maxTextBoxes[index].Text = "" + maxvalue;
            }
        }

        /// <summary>
        /// Resets all locked homophone mappings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetLockedLettersButton_Click(object sender, RoutedEventArgs e)
        {
            if (_hillClimber.AnalyzerConfiguration.LockedHomophoneMappings == null || _running == true)
            {
                return;
            }
            for (int i = 0; i < _hillClimber.AnalyzerConfiguration.LockedHomophoneMappings.Length; i++)
            {
                _hillClimber.AnalyzerConfiguration.LockedHomophoneMappings[i] = -1;
            }
            MarkLockedHomophones();
        }

        /// <summary>
        /// Automatically finds and locks words
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FindLockWordsButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_running)
            {
                return;
            }

            AutoLockWords(1);

        }

        /// <summary>
        /// Automatically locks words
        /// </summary>
        private void AutoLockWords(int minCount)
        {
            StringBuilder textBuilder = new StringBuilder();

            int column = 0;
            int row = 0;
            while (true)
            {
                if (_plaintextLabels[column, row] == null)
                {
                    break;
                }
                textBuilder.Append(_plaintextLabels[column, row].Symbol);
                column++;
                if (column == _analyzerConfiguration.TextColumns)
                {
                    column = 0;
                    row++;
                }
            }

            int[] plaintext = Tools.MapIntoNumberSpace(textBuilder.ToString(), PlainAlphabetText);

            Dictionary<int, int> wordPositions = _wordFinder.FindWords(plaintext);
            if (wordPositions.Count < minCount)
            {
                return;
            }
            foreach (var value in wordPositions)
            {
                for (int i = 0; i < value.Value; i++)
                {
                    int position = value.Key + i;
                    foreach (var label in _ciphertextLabels)
                    {
                        if (label == null)
                        {
                            continue;
                        }
                        if (label.SymbolOffset == position)
                        {
                            LockHomophone(label.Symbol, true);
                            break;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// A special label that knows is x and y coordinates in the grid, the offset of its symbol in the text, and the symbol itself
    /// 
    /// </summary>
    public class SymbolLabel : Label
    {
        public string Symbol { get; set; }
        public int SymbolOffset { get; set; }
        public int X { get; set; }
        public int Y { get; set; }        
    }
}
