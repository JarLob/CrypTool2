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
using Cryptool.PluginBase.Utils;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Cryptool.PluginBase.Miscellaneous;

namespace Cryptool.Plugins.HomophonicSubstitutionAnalyzer
{
    /// <summary>
    /// Interaktionslogik für HomophoneSubstitutionAnalyzerPresentation.xaml
    /// </summary>
    [PluginBase.Attributes.Localization("Cryptool.Plugins.HomophonicSubstitutionAnalyzer.Properties.Resources")]
    public partial class HomophoneSubstitutionAnalyzerPresentation : UserControl
    {
        private const int MaxBestListEntries = 100;
        private int _keylength = 0;
        private string PlainAlphabetText = null; //obtained by language statistics
        private string CipherAlphabetText = "ABCDEFGHIJKLMNOPQRSTUVWXYZÄÜÖabcdefghijklmnopqrstuvwxyzäüöß1234567890ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩАБВГДЂЕЄЖЗЅИІЈКЛЉМНЊОПРСТЋУФХЦЧЏШЪЫЬЭЮЯ!§$%&=?#ㄱㄴㄷㄹㅁㅂㅅㅇㅈㅊㅋㅌㅍㅎㄲㄸㅃㅆㅉㅏㅑㅓㅕㅗㅛㅜㅠㅡㅣㅐㅒㅔㅖㅚㅟㅢㅘㅝㅙㅞ";
        private HillClimber _hillClimber;
        private WordFinder _wordFinder;
        private SymbolLabel[,] _ciphertextLabels = new SymbolLabel[0,0];
        private SymbolLabel[,] _plaintextLabels = new SymbolLabel[0,0];
        private TextBox[] _minTextBoxes = new TextBox[0];
        private TextBox[] _maxTextBoxes = new TextBox[0];
        public AnalyzerConfiguration AnalyzerConfiguration { get; private set; }
        private PentaGrams _pentagrams;        

        //cache for loaded pentagrams
        private static Dictionary<string, PentaGrams> PentagramsCache = new Dictionary<string, PentaGrams>();

        private string _ciphertext = null;
        private CiphertextFormat _ciphertextFormat;
        private bool _running = false;

        public event EventHandler<ProgressChangedEventArgs> Progress;
        public event EventHandler<NewBestValueEventArgs> NewBestValue;

        private ObservableCollection<ResultEntry> BestList = new ObservableCollection<ResultEntry>();

        public HomophoneSubstitutionAnalyzerPresentation()
        {
            InitializeComponent();
            DisableUIAndStop();            
        }
 

        /// <summary>
        /// Initializes the ui with a new ciphertext
        /// </summary>
        /// <param name="ciphertext"></param>
        public void AddCiphertext(string ciphertext, CiphertextFormat ciphertextFormat)
        {
            _ciphertext = ciphertext;
            _ciphertextFormat = ciphertextFormat;
            int[] numbers = ConvertCiphertextToNumbers(ciphertext);
            _keylength = (int)(Tools.Distinct(numbers).Length * 1.3);
            AnalyzerConfiguration = new AnalyzerConfiguration(_keylength, Tools.ChangeToConsecutiveNumbers(numbers));

            AnalyzerConfiguration.PlaintextAlphabet = PlainAlphabetText;
            AnalyzerConfiguration.CiphertextAlphabet = CipherAlphabetText;
            AnalyzerConfiguration.TextColumns = 60;
            AnalyzerConfiguration.Cycles = 50000;
            AnalyzerConfiguration.KeyLetterLimits = new List<LetterLimits>();
            AnalyzerConfiguration.MinWordLength = 8;
            AnalyzerConfiguration.MaxWordLength = 10;
            AnalyzerConfiguration.WordCountToFind = 3;
            _hillClimber = new HillClimber(AnalyzerConfiguration);
            _hillClimber.Pentagrams = _pentagrams;
            _hillClimber.NewBestValue += HillClimberNewBestValue;
            _hillClimber.Progress += HillClimberProgress;

            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                GenerateCiphertextGrid(AnalyzerConfiguration.Ciphertext, AnalyzerConfiguration.TextColumns);
                GeneratePlaintextGrid(AnalyzerConfiguration.Ciphertext, AnalyzerConfiguration.TextColumns);                
                ProgressBar.Value = 0;
                ProgressText.Content = String.Empty;
                BestList.Clear();
                BestListView.DataContext = BestList;
            }, null);          
        }

        /// <summary>
        /// Converts string text into numbers depending on ciphertextformat
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <returns></returns>
        private int[] ConvertCiphertextToNumbers(string ciphertext)
        {
            switch (_ciphertextFormat)
            {
                case CiphertextFormat.NumberGroups:
                    return Tools.MapHomophoneTextNumbersIntoNumberSpace(ciphertext);
                    break;
                case CiphertextFormat.Letters:
                default:
                    return Tools.MapHomophonesIntoNumberSpace(ciphertext);
                    break;
            }
        }

        /// <summary>
        /// Creates the wordfinder that is used during the analysis using a given list of words from a dictionary
        /// if list is null or empty, no wordfinder is created
        /// </summary>
        /// <param name="dictionary"></param>
        public void AddDictionary(string[] dictionary)
        {
            if (dictionary != null && dictionary.Length > 0)
            {
                _wordFinder = new WordFinder(dictionary, AnalyzerConfiguration.MinWordLength, AnalyzerConfiguration.MaxWordLength, PlainAlphabetText);
            }
            else
            {
                _wordFinder = null;
            }
        }

        /// <summary>
        /// Generates the Grid for the ciphertext and fills in the symbols
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="columns"></param>
        private void GenerateCiphertextGrid(int[] ciphertext, int columns)
        {
            CiphertextGrid.Children.Clear();
            
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
                    label.Symbol = text.Substring(offset, 1);
                    _ciphertextLabels[x, y] = label;
                    label.Width = 30;
                    label.Height = 30;
                    label.FontSize = 20;
                    label.FontFamily = new FontFamily("Courier New");
                    label.Content = text.Substring(offset, 1);
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
            PlaintextGrid.Children.Clear();

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
                    label.MouseLeftButtonDown += LabelOnMouseLeftButtonDown;
                    label.MouseRightButtonDown += LabelOnMouseRightButtonDown;
                    _plaintextLabels[x, y] = label;
                    label.X = x;
                    label.Y = y;
                    label.SymbolOffset = offset;
                    label.Symbol = text.Substring(offset, 1);
                    label.Width = 30;
                    label.Height = 30;
                    label.FontSize = 20;
                    label.FontFamily = new FontFamily("Courier New");
                    label.Content = text.Substring(offset, 1);
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
        public void GenerateKeyLetterLimitsListView()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback) delegate
            {
                KeyLetterListView.Items.Clear();
                _minTextBoxes = new TextBox[AnalyzerConfiguration.KeyLetterLimits.Count];
                _maxTextBoxes = new TextBox[AnalyzerConfiguration.KeyLetterLimits.Count];

                int index = 0;
                foreach (LetterLimits limits in AnalyzerConfiguration.KeyLetterLimits)
                {
                    Grid grid = new Grid();
                    grid.Width = 500;
                    grid.ColumnDefinitions.Add(new ColumnDefinition());
                    grid.ColumnDefinitions.Add(new ColumnDefinition());
                    grid.ColumnDefinitions.Add(new ColumnDefinition());

                    Label label = new Label();
                    label.FontSize = 16;
                    label.Width = 50;
                    label.Content = String.Format("\"{0}\"", Tools.MapNumbersIntoTextSpace(new int[] {limits.Letter}, AnalyzerConfiguration.PlaintextAlphabet));
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
            }, null);
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
            if (Progress != null)
            {
                Progress.Invoke(sender, eventArgs);
            }
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
                    if (column == AnalyzerConfiguration.TextColumns)
                    {
                        column = 0;
                        row++;
                    }
                }
                CipherAlphabetTextBox.Text = eventArgs.CiphertextAlphabet;
                PlainAlphabetTextBox.Text = eventArgs.PlaintextAlphabet;
                CostTextBox.Text = String.Format("Cost Value: {0}", Math.Round(eventArgs.CostValue, 2));
                AutoLockWords(AnalyzerConfiguration.WordCountToFind);
                MarkLockedHomophones();
                AddNewBestListEntry(eventArgs.PlaintextAlphabet, eventArgs.CostValue, eventArgs.Plaintext);
            }, null);
            if (NewBestValue != null)
            {
                NewBestValue.Invoke(sender, eventArgs);
            }
        }

        /// <summary>
        /// Adds a new entry to the bestlist
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="text"></param>
        private void AddNewBestListEntry(string key, double value, string text)
        {
            var entry = new ResultEntry
            {
                Key = key,
                Text = text,
                Value = Math.Round(value, 2)
            };
           
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    if (BestList.Count > 0 && entry.Value <= BestList.Last().Value)
                    {
                        return;
                    }
                    BestList.Add(entry);
                    BestList = new ObservableCollection<ResultEntry>(BestList.OrderByDescending(i => i.Value));                    
                    if (BestList.Count > MaxBestListEntries)
                    {
                        BestList.RemoveAt(MaxBestListEntries);
                    }                    
                    var ranking = 1;
                    foreach (var e in BestList)
                    {
                        e.Ranking = ranking;
                        ranking++;
                    }
                    BestListView.DataContext = BestList;
                }               
                catch (Exception e)
                {
                    //wtf?
                }
            }, null);
        }

        /// <summary>
        /// Left mouse button down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LabelOnMouseLeftButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (_running)
            {
                return;
            }
            try
            {
                SymbolLabel label = (SymbolLabel)sender;
                string symbol = _ciphertextLabels[label.X, label.Y].Symbol;
                LockHomophone(symbol);
            }
            catch (Exception)
            {
                //do nothing here
            }
            mouseButtonEventArgs.Handled = true;
        }

        /// <summary>
        /// Right mouse button down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LabelOnMouseRightButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (_running)
            {
                return;
            }
            try
            {
                SymbolLabel label = (SymbolLabel) sender;
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    ChangeHomophone(_ciphertextLabels[label.X, label.Y].Symbol, -1);
                }
                else
                {
                    ChangeHomophone(_ciphertextLabels[label.X, label.Y].Symbol, 1);
                }
            }
            catch (Exception)
            {
                //do nothing here
            }
            mouseButtonEventArgs.Handled = true;
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
                var ciphertext = Tools.ChangeToConsecutiveNumbers(ConvertCiphertextToNumbers(_ciphertext));

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

                var plaintext = Tools.MapNumbersIntoTextSpace(HillClimber.DecryptHomophonicSubstitution(ciphertext, numkey), PlainAlphabetText);
                int column = 0;
                int row = 0;
                foreach (var letter in plaintext)
                {
                    _plaintextLabels[column, row].Content = letter;
                    _plaintextLabels[column, row].Symbol = "" + letter;
                    column++;
                    if (column == AnalyzerConfiguration.TextColumns)
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
            for (int index = 0; index < AnalyzerConfiguration.KeyLetterLimits.Count; index++)
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

                LetterLimits limits = AnalyzerConfiguration.KeyLetterLimits[index];
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
        /// Only works, if a WordFinder has previously been created
        /// </summary>
        private void AutoLockWords(int minCount)
        {
            if (_wordFinder == null)
            {
                return;
            }

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
                if (column == AnalyzerConfiguration.TextColumns)
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

        /// <summary>
        /// Enables the UI for the user to work with
        /// </summary>
        public void EnableUI()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                AnalyzeButton.IsEnabled = true;
                FindLockWordsButton.IsEnabled = true;
                ResetLockedLettersButton.IsEnabled = true;

                foreach (TextBox box in _minTextBoxes)
                {
                    if (box == null)
                    {
                        continue;
                    }
                    box.IsEnabled = true;
                }
                foreach (TextBox box in _maxTextBoxes)
                {
                    if (box == null)
                    {
                        continue;
                    }
                    box.IsEnabled = true;
                }
                foreach (SymbolLabel label in _plaintextLabels)
                {
                    if (label == null)
                    {
                        continue;
                    }
                    label.IsEnabled = true;
                }
                foreach (SymbolLabel label in _ciphertextLabels)
                {
                    if (label == null)
                    {
                        continue;
                    }
                    label.IsEnabled = true;
                }
                AnalyzeButton.Content = "Analyze";                
            }, null);
        }

        /// <summary>
        /// Disables editing and stops everything
        /// </summary>
        public void DisableUIAndStop()
        {
            _running = false;
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                AnalyzeButton.IsEnabled = false;
                FindLockWordsButton.IsEnabled = false;
                ResetLockedLettersButton.IsEnabled = false;

                foreach (TextBox box in _minTextBoxes)
                {
                    if (box == null)
                    {
                        continue;
                    }
                    box.IsEnabled = false;
                }
                foreach (TextBox box in _maxTextBoxes)
                {
                    if (box == null)
                    {
                        continue;
                    }
                    box.IsEnabled = false;
                }
                foreach(SymbolLabel label in _plaintextLabels)
                {
                    if (label == null)
                    {
                        continue;
                    }
                    label.IsEnabled = false;
                }
                foreach(SymbolLabel label in _ciphertextLabels)
                {
                    if (label == null)
                    {
                        continue;
                    }
                    label.IsEnabled = false;
                }
                AnalyzeButton.Content = "Stop";
            }, null);
            
            if (_hillClimber != null)
            {
                _hillClimber.Stop();
            }
        }

        /// <summary>
        /// Returns, if the analyzer is running
        /// </summary>
        /// <returns></returns>
        public bool IsRunning()
        {
            return _running;
        }

        /// <summary>
        /// Loads the language statistics
        /// </summary>
        /// <param name="language"></param>
        /// <param name="useSpaces"></param>
        public void LoadLangStatistics(int language, bool useSpaces)
        {
            lock (this)
            {
                //we use a cache for each language, thus, we do not need to load and load it again
                string key = String.Format("{0}-{1}", language, useSpaces);
                if (!PentagramsCache.ContainsKey(key))
                {
                    PentaGrams pentaGrams = new PentaGrams(LanguageStatistics.LanguageCode(language), useSpaces);
                    PentagramsCache.Add(key, pentaGrams);
                }
                _pentagrams = PentagramsCache[key];
                PlainAlphabetText = _pentagrams.Alphabet;
            }
        }

        /// <summary>
        /// Handler for the copy-context menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextMenuHandler(object sender, RoutedEventArgs routedEventArgs)
        {
            try
            {
                MenuItem menu = (MenuItem)((RoutedEventArgs)routedEventArgs).Source;
                ResultEntry entry = (ResultEntry)menu.CommandParameter;
                if (entry == null) return;
                string tag = (string)menu.Tag;

                if (tag == "copy_text")
                {
                    Clipboard.SetText(entry.Text);
                }
                else if (tag == "copy_value")
                {
                    Clipboard.SetText("" + entry.Value);
                }
                else if (tag == "copy_key")
                {
                    Clipboard.SetText(entry.Key);
                }
                else if (tag == "copy_line")
                {
                    Clipboard.SetText(entryToText(entry));
                }
                else if (tag == "copy_all")
                {
                    List<string> lines = new List<string>();
                    foreach (var e in BestList)
                    {
                        lines.Add(entryToText(e));

                    }
                    Clipboard.SetText(String.Join(Environment.NewLine, lines));
                }
            }
            catch (Exception ex)
            {
                Clipboard.SetText("");
            }
        }
    

        /// <summary>
        /// Converts an entry to text
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        private string entryToText(ResultEntry entry)
        {
            return "Rank: " + entry.Ranking + Environment.NewLine +
                   "Value: " + entry.Value + Environment.NewLine + 
                   "Key: " + entry.Key + Environment.NewLine +
                   "Text: " + entry.Text;
        }

        private void BestListDoubleClick(object sender, MouseButtonEventArgs e)
        {
            
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

    /// <summary>
    /// ResultEntry of best list
    /// </summary>
    public class ResultEntry 
    {
        private int _ranking = 0;

        public int Ranking { get; set; }
        public double Value { get; set; }
        public string Key { get; set; }
        public string Text { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
    }    
}
