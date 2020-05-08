﻿/*
   Copyright 2020 Nils Kopal <Nils.Kopal<at>CrypTool.org

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
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Cryptool.PluginBase.Utils;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Cryptool.CrypAnalysisViewControl;

namespace Cryptool.Plugins.HomophonicSubstitutionAnalyzer
{
    [PluginBase.Attributes.Localization("Cryptool.Plugins.HomophonicSubstitutionAnalyzer.Properties.Resources")]
    public partial class HomophoneSubstitutionAnalyzerPresentation : UserControl
    {
        private const int MaxBestListEntries = 100;
        private int _keylength = 0;
        private string PlainAlphabetText = null; //obtained by language statistics
        private readonly string CipherAlphabetText;// = "ABCDEFGHIJKLMNOPQRSTUVWXYZÄÜÖabcdefghijklmnopqrstuvwxyzäüöß1234567890ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩАБВГДЂЕЄЖЗЅИІЈКЛЉМНЊОПРСТЋУФХЦЧЏШЪЫЬЭЮЯ!§$%&=?#ㄱㄴㄷㄹㅁㅂㅅㅇㅈㅊㅋㅌㅍㅎㄲㄸㅃㅆㅉㅏㅑㅓㅕㅗㅛㅜㅠㅡㅣㅐㅒㅔㅖㅚㅟㅢㅘㅝㅙㅞ";
        private HillClimber _hillClimber;
        private WordFinder _wordFinder;
        private SymbolLabel[,] _ciphertextLabels = new SymbolLabel[0,0];
        private SymbolLabel[,] _plaintextLabels = new SymbolLabel[0,0];
        private TextBox[] _minTextBoxes = new TextBox[0];
        private TextBox[] _maxTextBoxes = new TextBox[0];
        public AnalyzerConfiguration AnalyzerConfiguration { get; private set; }
        private Grams _grams;

        //cache for loaded n-grams
        private static Dictionary<string, Grams> NGramCache = new Dictionary<string, Grams>();

        private string _ciphertext = null;
        private char _separator;
        private CiphertextFormat _ciphertextFormat;
        private bool _running = false;

        public event EventHandler<ProgressChangedEventArgs> Progress;
        public event EventHandler<NewBestValueEventArgs> NewBestValue;
        public event EventHandler<UserChangedTextEventArgs> UserChangedText;

        private ObservableCollection<ResultEntry> BestList { get; } = new ObservableCollection<ResultEntry>();
        private int _restart = 0;

        private List<string> _originalCiphertextSymbols = new List<string>();

        public HomophoneSubstitutionAnalyzerPresentation()
        {
            InitializeComponent();
            DisableUIAndStop();

            //create ciphertext alphabet symbols
            StringBuilder builder = new StringBuilder();            
            for (int i = 41; i < 1041; i++)
            {
                builder.Append((char)i);
            }
            CipherAlphabetText = builder.ToString();            
        }

        /// <summary>
        /// Initializes the ui with a new ciphertext
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="ciphertextFormat"></param>
        /// <param name="separator"></param>
        /// <param name="costFactorMultiplicator"></param>
        /// <param name="fixedTemperature"></param>
        public void AddCiphertext(string ciphertext, CiphertextFormat ciphertextFormat, char separator, int costFactorMultiplicator, int fixedTemperature, bool useNulls)
        {
            _ciphertext = ciphertext;
            _separator = separator;
            _ciphertextFormat = ciphertextFormat;
            int[] numbers = ConvertCiphertextToNumbers(_ciphertext, _separator);
            _originalCiphertextSymbols = ConvertToList(_ciphertext, _separator);
            int homophoneNumber = Tools.Distinct(numbers).Length;
            _keylength = (int)(homophoneNumber * 1.3);
            AnalyzerConfiguration = new AnalyzerConfiguration(_keylength, new Text(Tools.ChangeToConsecutiveNumbers(numbers)));
            AnalyzerConfiguration.PlaintextMapping = PlainAlphabetText;
            AnalyzerConfiguration.CiphertextAlphabet = CipherAlphabetText;
            AnalyzerConfiguration.TextColumns = 60;
            AnalyzerConfiguration.Cycles = 50000;
            AnalyzerConfiguration.KeyLetterLimits = new List<LetterLimits>();
            AnalyzerConfiguration.MinWordLength = 8;
            AnalyzerConfiguration.MaxWordLength = 10;
            AnalyzerConfiguration.WordCountToFind = 3;
            AnalyzerConfiguration.Separator = separator;
            AnalyzerConfiguration.CostFunctionMultiplicator = costFactorMultiplicator;
            AnalyzerConfiguration.FixedTemperature = fixedTemperature;
            AnalyzerConfiguration.UseNulls = useNulls;
            _hillClimber = new HillClimber(AnalyzerConfiguration);
            _hillClimber.Grams = _grams;
            _hillClimber.NewBestValue += HillClimberNewBestValue;
            _hillClimber.Progress += HillClimberProgress;                
        }

        /// <summary>
        /// Generates the plaintext and the ciphertext grids
        /// </summary>
        public void GenerateGrids()
        {
            int[] numbers = ConvertCiphertextToNumbers(_ciphertext, _separator);
            int homophoneNumber = Tools.Distinct(numbers).Length;
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                if (AnalyzerConfiguration.KeepLinebreaks)
                {
                    GenerateCiphertextGridWithLinebreaks(AnalyzerConfiguration.Ciphertext);
                    GeneratePlaintextGridWithLinebreaks(AnalyzerConfiguration.Ciphertext);
                }
                else
                {
                    GenerateCiphertextGrid(AnalyzerConfiguration.Ciphertext, AnalyzerConfiguration.TextColumns);
                    GeneratePlaintextGrid(AnalyzerConfiguration.Ciphertext, AnalyzerConfiguration.TextColumns);
                }
                ProgressBar.Value = 0;
                ProgressText.Content = String.Empty;
                BestList.Clear();
                BestListView.DataContext = BestList;
                InfoTextLabel.Content = String.Format(Properties.Resources.DifferentHomophones, numbers.Length, homophoneNumber);
            }, null);
        }

        /// <summary>
        /// Converts string text into numbers depending on ciphertextformat
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <returns></returns>
        private int[] ConvertCiphertextToNumbers(string ciphertext, char separator)
        {
            switch (_ciphertextFormat)
            {
                case CiphertextFormat.NumberGroups:
                    return Tools.MapHomophoneTextNumbersIntoNumberSpace(ciphertext);
                case CiphertextFormat.CommaSeparated:
                    return Tools.MapHomophoneCommaSeparatedIntoNumberSpace(ciphertext, separator);
                case CiphertextFormat.SingleLetters:
                default:
                    return Tools.MapHomophonesIntoNumberSpace(ciphertext);
            }
        }

        /// <summary>
        /// Converts string text to list of strings
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <returns></returns>
        private List<string> ConvertToList(string ciphertext, char separator)
        {
            switch (_ciphertextFormat)
            {
                case CiphertextFormat.NumberGroups:
                    return Tools.ConvertHomophoneTextNumbersToListOfStrings(ciphertext);
                case CiphertextFormat.CommaSeparated:
                    return Tools.ConvertHomophoneCommaSeparatedToListOfStrings(ciphertext, separator);
                case CiphertextFormat.SingleLetters:
                default:
                    return Tools.ConvertHomophonesToListOfString(ciphertext);
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
        private void GenerateCiphertextGrid(Text ciphertext, int columns)
        {
            CiphertextGrid.Children.Clear();
            
            int rows = (int)Math.Ceiling((double)ciphertext.GetSymbolsCount() / columns);
            _ciphertextLabels = new SymbolLabel[columns, rows];
            string text = Tools.MapNumbersIntoTextSpace(ciphertext.ToIntegerArray(), CipherAlphabetText);

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
                    if (offset == ciphertext.GetSymbolsCount())
                    {
                        break;
                    }
                    SymbolLabel label = new SymbolLabel();
                    label.X = x;
                    label.Y = y;
                    label.SymbolOffset = offset;
                    if (offset < text.Length)
                    {
                        label.Symbol = text.Substring(offset, 1);
                    }
                    else
                    {
                        label.Symbol = "?";
                    }
                    _ciphertextLabels[x, y] = label;
                    label.Width = 30  + (_originalCiphertextSymbols[offset].Length  - 1) * 5;
                    label.Height = 30;
                    label.FontSize = 20;
                    label.FontFamily = new FontFamily("Courier New");
                    //label.Content = text.Substring(offset, 1);
                    label.Content = _originalCiphertextSymbols[offset];
                    label.HorizontalAlignment = HorizontalAlignment.Center;
                    label.HorizontalContentAlignment = HorizontalAlignment.Center;
                    offset++;
                    Grid.SetRow(label, y);
                    Grid.SetColumn(label, x);
                    CiphertextGrid.Children.Add(label);
                }
            }
        }

        /// <summary>
        /// Generates the Grid for the ciphertext and fills in the symbols (keeping the linebreaks)
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="columns"></param>
        private void GenerateCiphertextGridWithLinebreaks(Text ciphertext)
        {
            CiphertextGrid.Children.Clear();
            int columns = 0;
            int offset = 0;
            int distance;
            foreach (var number in AnalyzerConfiguration.LinebreakPositions)
            {
                distance = number - offset;
                offset = offset + distance;
                if(distance > columns)
                {
                    columns = distance;
                }
            }
            distance = ciphertext.GetLettersCount() - offset;
            if (distance > columns)
            {
                columns = distance;
            }
            int rows = AnalyzerConfiguration.LinebreakPositions.Count + 1;

            _ciphertextLabels = new SymbolLabel[columns, rows];
            string text = Tools.MapNumbersIntoTextSpace(ciphertext.ToIntegerArray(), CipherAlphabetText);

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

            offset = 0;
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    if (offset == ciphertext.GetSymbolsCount())
                    {
                        break;
                    }                   
                    SymbolLabel label = new SymbolLabel();
                    label.X = x;
                    label.Y = y;
                    label.SymbolOffset = offset;
                    if (offset < text.Length)
                    {
                        label.Symbol = text.Substring(offset, 1);
                    }
                    else
                    {
                        label.Symbol = "?";
                    }
                    _ciphertextLabels[x, y] = label;
                    label.Width = 30 + (_originalCiphertextSymbols[offset].Length - 1) * 5;
                    label.Height = 30;
                    label.FontSize = 20;
                    label.FontFamily = new FontFamily("Courier New");
                    //label.Content = text.Substring(offset, 1);
                    label.Content = _originalCiphertextSymbols[offset];
                    label.HorizontalAlignment = HorizontalAlignment.Center;
                    label.HorizontalContentAlignment = HorizontalAlignment.Center;
                    offset++;
                    Grid.SetRow(label, y);
                    Grid.SetColumn(label, x);
                    CiphertextGrid.Children.Add(label);
                    if (AnalyzerConfiguration.LinebreakPositions.Contains(offset))
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Generates the Grid for the plaintext and fills in the symbols
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="columns"></param>
        private void GeneratePlaintextGrid(Text plaintext, int columns)
        {
            PlaintextGrid.Children.Clear();

            int rows = (int)Math.Ceiling((double)plaintext.GetSymbolsCount() / columns);
            _plaintextLabels = new SymbolLabel[columns, rows];
            string text = Tools.MapNumbersIntoTextSpace(plaintext.ToIntegerArray(), CipherAlphabetText);

            for (int column = 0; column < columns; column++)
            {
                PlaintextGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            PlaintextGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(int.MaxValue, GridUnitType.Star) });

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
                    if (offset == plaintext.GetSymbolsCount())
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
                    label.Width = 30 + (_originalCiphertextSymbols[offset].Length - 1) * 5;
                    label.Height = 30;
                    label.FontSize = 20;
                    label.FontFamily = new FontFamily("Courier New");
                    label.Content = text.Substring(offset, 1);
                    label.HorizontalAlignment = HorizontalAlignment.Center;
                    label.HorizontalContentAlignment = HorizontalAlignment.Center;
                    offset++;
                    Grid.SetRow(label, y);
                    Grid.SetColumn(label, x);
                    PlaintextGrid.Children.Add(label);
                }
            }
        }

        /// <summary>
        /// Generates the Grid for the ciphertext and fills in the symbols (keeping the linebreaks)
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="columns"></param>
        private void GeneratePlaintextGridWithLinebreaks(Text plaintext)
        {
            PlaintextGrid.Children.Clear();

            int columns = 0;
            int offset = 0;
            int distance;
            foreach (var number in AnalyzerConfiguration.LinebreakPositions)
            {
                distance = number - offset;
                offset = offset + distance;
                if (distance > columns)
                {
                    columns = distance;
                }
            }
            distance = plaintext.GetLettersCount() - offset;
            if (distance > columns)
            {
                columns = distance;
            }
            AnalyzerConfiguration.TextColumns = columns;
            int rows = AnalyzerConfiguration.LinebreakPositions.Count + 1;

            _plaintextLabels = new SymbolLabel[columns, rows];
            string text = Tools.MapNumbersIntoTextSpace(plaintext.ToIntegerArray(), CipherAlphabetText);

            for (int column = 0; column < columns; column++)
            {
                PlaintextGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            PlaintextGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(int.MaxValue, GridUnitType.Star) });

            for (int row = 0; row < rows; row++)
            {
                PlaintextGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength() });
            }
            PlaintextGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            offset = 0;
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    if (offset == plaintext.GetSymbolsCount())
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
                    label.Width = 30 + (_originalCiphertextSymbols[offset].Length - 1) * 5;
                    label.Height = 30;
                    label.FontSize = 20;
                    label.FontFamily = new FontFamily("Courier New");
                    label.Content = text.Substring(offset, 1);
                    label.HorizontalAlignment = HorizontalAlignment.Center;
                    label.HorizontalContentAlignment = HorizontalAlignment.Center;
                    offset++;
                    Grid.SetRow(label, y);
                    Grid.SetColumn(label, x);
                    PlaintextGrid.Children.Add(label);
                    if (AnalyzerConfiguration.LinebreakPositions.Contains(offset))
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Generates the tab for the selection of the key letter distribution
        /// </summary>
        public void GenerateKeyLetterLimitsListView()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback) delegate
            {
                KeyLetterListView.Items.Clear();
                _minTextBoxes = new TextBox[AnalyzerConfiguration.KeyLetterLimits.Count];
                _maxTextBoxes = new TextBox[AnalyzerConfiguration.KeyLetterLimits.Count];

                Grid grid = new Grid();
                grid.Width = 500;
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition());

                Label letterLabel = new Label();
                letterLabel.Content = Properties.Resources.LetterLabel;
                Grid.SetRow(letterLabel, 0);
                Grid.SetColumn(letterLabel, 0);
                letterLabel.VerticalContentAlignment = VerticalAlignment.Center;
                letterLabel.HorizontalContentAlignment = HorizontalAlignment.Center;

                Label minLabel = new Label();
                minLabel.Content = Properties.Resources.MinLabel;
                Grid.SetRow(minLabel, 0);
                Grid.SetColumn(minLabel, 1);
                minLabel.VerticalContentAlignment = VerticalAlignment.Center;
                minLabel.HorizontalContentAlignment = HorizontalAlignment.Center;

                Label maxLabel = new Label();
                maxLabel.Content = Properties.Resources.MaxLabel; Grid.SetRow(letterLabel, 0);
                Grid.SetRow(maxLabel, 0);
                Grid.SetColumn(maxLabel, 2);
                maxLabel.VerticalContentAlignment = VerticalAlignment.Center;
                maxLabel.HorizontalContentAlignment = HorizontalAlignment.Center;

                grid.Children.Add(letterLabel);
                grid.Children.Add(minLabel);
                grid.Children.Add(maxLabel);

                KeyLetterListView.Items.Add(grid);

                int index = 0;
                foreach (LetterLimits limits in AnalyzerConfiguration.KeyLetterLimits)
                {
                    grid = new Grid();
                    grid.Width = 500;
                    grid.ColumnDefinitions.Add(new ColumnDefinition());
                    grid.ColumnDefinitions.Add(new ColumnDefinition());
                    grid.ColumnDefinitions.Add(new ColumnDefinition());

                    Label label = new Label();
                    label.FontSize = 16;
                    label.Width = 50;                    
                    label.Content = String.Format("\"{0}\"", Tools.MapNumbersIntoTextSpace(new int[] { limits.Letter }, AnalyzerConfiguration.PlaintextMapping));
                    
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
                
                if (eventArgs.Terminated && AnalyzerConfiguration.AnalysisMode == AnalysisMode.SemiAutomatic)
                {
                    _running = false;
                    AnalyzeButton.Content = "Analyze";
                }

                //in fullautomatic analysis mode with 100% we restart by resetting locked letters
                if (eventArgs.Terminated && AnalyzerConfiguration.AnalysisMode == AnalysisMode.FullAutomatic)
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal,
                    (SendOrPostCallback) delegate
                    {
                        //reset all locked letters
                        for (int i = 0; i < _hillClimber.AnalyzerConfiguration.LockedHomophoneMappings.Length; i++)
                        {
                            _hillClimber.AnalyzerConfiguration.LockedHomophoneMappings[i] = -1;
                        }
                        MarkLockedHomophones();
                    }, null);
                }

            }, null);
            if (Progress != null)
            {
                if (AnalyzerConfiguration.AnalysisMode == AnalysisMode.FullAutomatic)
                {
                    //in fullautomatic analysis mode the progress is calculated using the restarts
                    eventArgs.Percentage = (double) _restart / (double) AnalyzerConfiguration.Restarts;
                }

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
            AutoResetEvent waitHandle = new AutoResetEvent(false);
            bool newTopEntry = false;
            Dictionary<int, int> wordPositions = null;            
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    int column = 0;
                    int row = 0;
                    int offset = 0;
                    foreach (var letter in eventArgs.Plaintext)
                    {
                        _plaintextLabels[column, row].Content = letter;
                        _plaintextLabels[column, row].Symbol = "" + letter;
                        column++;
                        offset++;
                        if ((AnalyzerConfiguration.KeepLinebreaks && AnalyzerConfiguration.LinebreakPositions.Contains(offset)) ||
                            (!AnalyzerConfiguration.KeepLinebreaks && column == AnalyzerConfiguration.TextColumns))
                        {
                            column = 0;
                            row++;
                        }
                    }
                    CipherAlphabetTextBox.Text = eventArgs.CiphertextAlphabet;
                    PlainAlphabetTextBox.Text = eventArgs.PlaintextMapping;
                    CostTextBox.Text = String.Format(Properties.Resources.CostValue_0, Math.Round(eventArgs.CostValue, 2));
                    MarkLockedHomophones();
                    wordPositions = AutoLockWords(AnalyzerConfiguration.WordCountToFind, eventArgs.PlaintextAsNumbers);
                    MarkFoundWords(wordPositions);
                    newTopEntry = AddNewBestListEntry(eventArgs.PlaintextMapping, eventArgs.CostValue, eventArgs.Plaintext);
                    if (newTopEntry)
                    {
                        var substitutionKey = GenerateSubstitutionKey();
                        eventArgs.SubstitutionKey = substitutionKey;
                    }
                }
                catch (Exception)
                {
                    //if auto-lock fails, we just continue
                }
                finally
                {
                    waitHandle.Set();
                }
            }, null);

            //wait here for auto-locker to finish
            waitHandle.WaitOne();

            if (NewBestValue != null)
            {
                if (newTopEntry && wordPositions != null && wordPositions.Count > 0)
                {
                    eventArgs.FoundWords = new List<string>();
                    //if we have a new top entry, we also output the found words
                    foreach (KeyValuePair<int,int> positionLength in wordPositions)
                    {
                        var word = eventArgs.Plaintext.Substring(positionLength.Key, positionLength.Value);
                        eventArgs.FoundWords.Add(word);
                    }
                }
                eventArgs.NewTopEntry = newTopEntry;
                NewBestValue.Invoke(sender, eventArgs);
            }
        }

        /// <summary>
        /// Generates the current substitution key which can be used by
        /// the Substitution component with the nomenclature templates
        /// </summary>
        /// <returns></returns>
        private string GenerateSubstitutionKey()
        {
            var keyDictionary = new Dictionary<string, List<string>>();
            foreach (var ciphertextLabel in _ciphertextLabels)
            {
                if(ciphertextLabel == null)
                {
                    continue;
                }
                var x = ciphertextLabel.X;
                var y = ciphertextLabel.Y;
                var plaintextLabel = _plaintextLabels[x, y];

                if (plaintextLabel != null)
                {
                    var plainletter = plaintextLabel.Symbol;
                    var cipherletter = _originalCiphertextSymbols[ciphertextLabel.SymbolOffset];

                    if (!keyDictionary.ContainsKey(plainletter))
                    {
                        keyDictionary.Add(plainletter, new List<string>());
                    }
                    if (!keyDictionary[plainletter].Contains(cipherletter))
                    {
                        keyDictionary[plainletter].Add(cipherletter);
                    }
                }
            }
            var builder = new StringBuilder();
            foreach (var keyValuePair in keyDictionary)
            {
                builder.Append(String.Format("[{0}];", keyValuePair.Key));
                var list = keyValuePair.Value;
                for (var i = 0; i < list.Count; i++)
                {
                    var symbol = list[i];
                    if (i == 0)
                    {
                        builder.Append("[");
                        builder.Append(symbol);
                    }
                    else if (i < list.Count - 1)
                    {
                        builder.Append("|");
                        builder.Append(symbol);
                    }
                    else
                    {
                        builder.Append("|");
                        builder.Append(symbol);
                        builder.AppendLine("]");
                    }
                }
                if(list.Count == 1)
                {
                    //if we have only one element, we have to close the tag
                    builder.AppendLine("]");
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Marks the found words in blue color
        /// </summary>
        /// <param name="wordPositions"></param>
        private void MarkFoundWords(Dictionary<int, int> wordPositions)
        {
            if(wordPositions == null)
            {
                return;
            }
            //Color the found words in blue
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
                            label.Background = Brushes.LightSkyBlue;
                            _plaintextLabels[label.X, label.Y].Background = Brushes.LightSkyBlue;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds a new entry to the bestlist
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="text"></param>
        private bool AddNewBestListEntry(string key, double value, string text)
        {
            var entry = new ResultEntry
            {
                Key = key,
                Text = text,
                Value = Math.Round(value, 2)
            };            
            bool newTopEntry = false;         
            try
            {
                if (BestList.Count > 0 && entry.Value <= BestList.Last().Value)
                {
                    return false;
                }
                if (BestList.Count > 0 && entry.Value > BestList.First().Value)
                {
                    newTopEntry = true;
                }


                //Insert new entry at correct place to sustain order of list:
                var insertIndex = BestList.TakeWhile(e => e.Value > entry.Value).Count();
                BestList.Insert(insertIndex, entry);

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
            }
            catch (Exception e)
            {
                //wtf?
            }        
            return newTopEntry;
        }


        /// <summary>
        /// Left mouse button down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="mouseButtonEventArgs"></param>
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
        /// <param name="mouseButtonEventArgs"></param>
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

                string newSymbol = Tools.MapNumbersIntoTextSpace(new int[] { _hillClimber.AnalyzerConfiguration.LockedHomophoneMappings[index] }, PlainAlphabetText);

                //Update plainalphabet textbox
                PlainAlphabetTextBox.Text = PlainAlphabetTextBox.Text.Remove(index, 1);
                PlainAlphabetTextBox.Text = PlainAlphabetTextBox.Text.Insert(index, newSymbol);
                                
                //Update all plaintext labels               
                foreach (var label in _ciphertextLabels)
                {
                    if (label == null)
                    {
                        continue;
                    }
                    if (label.Symbol.Equals(symbol))
                    {
                        var plaintextLabel = _plaintextLabels[label.X, label.Y];
                        plaintextLabel.Symbol = newSymbol;
                        plaintextLabel.Content = newSymbol;
                    }
                }

                //Create new plaintext from labels
                if (UserChangedText != null)
                {
                    StringBuilder plaintextBuilder = new StringBuilder();
                    int column = 0;
                    int row = 0;
                    int offset = 0;
                    foreach(var letter in _ciphertext)
                    {                        
                        plaintextBuilder.Append(_plaintextLabels[column, row].Symbol);                      
                        column++;
                        offset++;
                        if ((AnalyzerConfiguration.KeepLinebreaks && AnalyzerConfiguration.LinebreakPositions.Contains(offset)) ||
                            (!AnalyzerConfiguration.KeepLinebreaks && column == AnalyzerConfiguration.TextColumns))
                        {
                            column = 0;
                            row++;
                            plaintextBuilder.AppendLine();
                        }                        
                    }
                    //Fire event that the user changed the plaintext
                    UserChangedTextEventArgs args = new UserChangedTextEventArgs() { Plaintext = plaintextBuilder.ToString() };
                    args.SubstitutionKey = GenerateSubstitutionKey();
                    UserChangedText.Invoke(this, args);
                }
            }
        }

        /// <summary>
        /// Locks a mapping of a single mapping of a homophone
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="auto"></param>
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
                        AnalyzeButton.Content = Properties.Resources.Analyze;                        
                    }
                    else
                    {                        
                        AnalyzeButton.Content = Properties.Resources.Stop;
                        ProgressBar.Value = 0;
                        ProgressText.Content = string.Empty;
                        StartAnalysis();
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
        /// Starts the analysis
        /// </summary>
        public void StartAnalysis()
        {
            if (_running)
            {
                return;
            }
            _running = true;
            
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                UpdateKeyLetterLimits();
            },
            null);

            if (AnalyzerConfiguration.AnalysisMode == AnalysisMode.SemiAutomatic)
            {              
                ThreadStart threadStart = () => _hillClimber.Execute();
                var thread = new Thread(threadStart);
                thread.IsBackground = true;
                thread.Start();
            }
            else
            {
                ThreadStart threadStart = () =>
                {
                    for (_restart = 0; _restart < AnalyzerConfiguration.Restarts; _restart++)
                    {
                        _hillClimber.Execute();
                        if (_running == false)
                        {
                            return;
                        }
                    }
                    
                };
                var thread = new Thread(threadStart);
                thread.IsBackground = true;
                thread.Start();
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
            StringBuilder plaintextBuilder = new StringBuilder();
            int rows = _plaintextLabels.Length / AnalyzerConfiguration.TextColumns + (_plaintextLabels.Length % AnalyzerConfiguration.TextColumns > 0 ? 1 : 0);
            for (var y = 0; y < rows; y++)
            {
                for (var x = 0; x < AnalyzerConfiguration.TextColumns; x++)
                {
                    SymbolLabel label = _plaintextLabels[x, y];
                    if (label == null)
                    {
                        continue;
                    }
                    plaintextBuilder.Append(label.Symbol);
                }
            }
            var foundWords = AutoLockWords(1, Tools.MapIntoNumberSpace(plaintextBuilder.ToString(), PlainAlphabetText));
            MarkFoundWords(foundWords);
        }

        /// <summary>
        /// Automatically locks words
        /// Only works, if a WordFinder has previously been created
        /// </summary>
        private Dictionary<int, int> AutoLockWords(int minCount, int[] plaintext)
        {
            if (_wordFinder == null)
            {
                return null;
            }           

            Dictionary<int, int> wordPositions = _wordFinder.FindWords(plaintext);
            if (wordPositions.Count < minCount)
            {
                return null;
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
            return wordPositions;
        }

        /// <summary>
        /// Enables the UI for the user to work with
        /// </summary>
        public void EnableUI()
        {            
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                if (AnalyzerConfiguration.AnalysisMode == AnalysisMode.SemiAutomatic)
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
                AnalyzeButton.Content = "Analyze";
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
        public void LoadLangStatistics(int language, bool useSpaces, bool useNulls, int ngramsize = 5)
        {
            lock (this)
            {
                //we use a cache for each language, thus, we do not need to load and load it again
                string key = String.Format("{0}-{1}-{2}", language, useSpaces, ngramsize);
                if (!NGramCache.ContainsKey(key))
                {
                    //this is a "fallback" mechanism; it tries to load ngramsize,...,5,4,3-grams, then it fails
                    bool loaded = false;
                    while(loaded == false)
                    {
                        try
                        {
                            switch (ngramsize)
                            {
                                default:
                                case 5:
                                    PentaGrams pentaGrams = new PentaGrams(LanguageStatistics.LanguageCode(language), useSpaces);
                                    NGramCache.Add(key, pentaGrams);
                                    loaded = true;
                                    break;
                                case 4:
                                    QuadGrams quadGrams = new QuadGrams(LanguageStatistics.LanguageCode(language), useSpaces);
                                    NGramCache.Add(key, quadGrams);
                                    loaded = true;
                                    break;
                                case 3:
                                    TriGrams triGrams = new TriGrams(LanguageStatistics.LanguageCode(language), useSpaces);
                                    NGramCache.Add(key, triGrams);
                                    loaded = true;
                                    break;                                                                    
                            }                           
                        }
                        catch (Exception)
                        {
                            ngramsize--;
                            if(ngramsize == 2)
                            {
                                throw new ArgumentException(String.Format("Could not load any ngrams for language='{0}' useSpaces={1}", LanguageStatistics.LanguageCode(language),useSpaces));
                            }
                        }
                    }
                }
                _grams = NGramCache[key];
                PlainAlphabetText = _grams.Alphabet;
                if (useNulls)
                {
                    PlainAlphabetText = PlainAlphabetText + "#";
                }
            }
        }

        private void HandleResultItemAction(ICrypAnalysisResultListEntry item)
        {
            if (item is ResultEntry resultItem)
            {
            }
        }

        /// <summary>
        /// When ciphertext scroll viewer is scrolled, plaintext scroll viewer is adapted accordingly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CiphertextScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if(CiphertextScrollViewer.VerticalOffset != PlaintextScrollViewer.VerticalOffset)
            {
                PlaintextScrollViewer.ScrollToVerticalOffset(CiphertextScrollViewer.VerticalOffset);
            }
            if (CiphertextScrollViewer.HorizontalOffset != PlaintextScrollViewer.HorizontalOffset)
            {
                PlaintextScrollViewer.ScrollToHorizontalOffset(CiphertextScrollViewer.HorizontalOffset);
            }
        }

        /// <summary>
        /// When plaintext scroll viewer is scrolled, ciphertext scroll viewer is adapted accordingly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlaintextScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (CiphertextScrollViewer.VerticalOffset != PlaintextScrollViewer.VerticalOffset)
            {
                CiphertextScrollViewer.ScrollToVerticalOffset(PlaintextScrollViewer.VerticalOffset);
            }
            if (CiphertextScrollViewer.HorizontalOffset != PlaintextScrollViewer.HorizontalOffset)
            {
                CiphertextScrollViewer.ScrollToHorizontalOffset(PlaintextScrollViewer.HorizontalOffset);
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

    /// <summary>
    /// ResultEntry of best list
    /// </summary>
    public class ResultEntry : ICrypAnalysisResultListEntry, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int ranking;
        public int Ranking
        {
            get => ranking;
            set
            {
                ranking = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Ranking)));
            }
        }

        public double Value { get; set; }
        public string Key { get; set; }
        public string Text { get; set; }


        public string ClipboardValue => Value.ToString();
        public string ClipboardKey => Key;
        public string ClipboardText => Text;
        public string ClipboardEntry =>
            "Rank: " + Ranking + Environment.NewLine +
            "Value: " + Value + Environment.NewLine +
            "Key: " + Key + Environment.NewLine +
            "Text: " + Text;
    }
}
