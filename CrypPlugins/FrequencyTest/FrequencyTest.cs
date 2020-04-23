using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.Windows.Media;

namespace Cryptool.FrequencyTest
{
    [Author("Georgi Angelov, Danail Vazov, Matthäus Wander, Nils Kopal", "angelov@cryptool.org", "Uni Duisburg", "http://www.uni-duisburg-essen.de")]
    [PluginInfo("Cryptool.FrequencyTest.Properties.Resources",  "PluginCaption", "PluginTooltip", "FrequencyTest/DetailedDescription/doc.xml", "FrequencyTest/icon.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisGeneric)]
    public class FrequencyTest : ICrypComponent
    {
        #region Const and variable definition

        public const int ABSOLUTE = 0;
        public const int PERCENTAGED = 1;
        public const int LOG2 = 2;
        public const int SINKOV = 3;

        private string stringInput;

        private string stringOutput = "";
        private int[] arrayOutput = new int[0];
        private IDictionary<string, double[]> grams = new SortedDictionary<string, double[]>();
        private DataSource data = new DataSource();
        private double presentationScaler = 1.0; // the initial zoom value
        private double presentationBarWidth = 38.0; // the width in pixel of a single chart bar
        private double presentationBarHeightAdd = 8.0 + 2.0 * 26.0; // the additional heigth to a chart bar, comprised of two rectangles (3px, 5px) and two textblocks

        private const string defaultAlphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private string alphabet = defaultAlphabet;


        #endregion

        #region Properties (Inputs/Outputs)

        [PropertyInfo(Direction.InputData, "StringInputCaption", "StringInputTooltip", true)]
        public string StringInput 
        {
            get
            {
                return stringInput;
            }
            set
            {
                stringInput = value;
                OnPropertyChanged("StringInput");
            }
        }

        [PropertyInfo(Direction.InputData, "AlphabetCaption", "AlphabetTooltip", false)]
        public string Alphabet
        {
            get
            {
                return alphabet;
            }
            set
            {
                alphabet = value;
                OnPropertyChanged("Alphabet");
            }
        }


        [PropertyInfo(Direction.OutputData, "StringOutputCaption", "StringOutputTooltip", false)]
        public string StringOutput
        {
            get { return stringOutput; }
        }

        [PropertyInfo(Direction.OutputData , "ArrayOutputCaption", "ArrayOutputTooltip", false)]
        public int[] ArrayOutput
        {
            get { return arrayOutput; }
        }

        [PropertyInfo(Direction.OutputData, "DictionaryOutputCaption", "DictionaryOutputTooltip", false)]
        public IDictionary<string, double[]> DictionaryOutput
        {
            get { return grams; }
        
        }
        #endregion

        #region IPlugin Members

        private FrequencyTestSettings settings;
        public ISettings Settings
        {
            get { return settings; }
            set { settings = (FrequencyTestSettings)value; }
        }
        private FrequencyTestPresentation presentation;
        public FrequencyTest()
        {
            settings = new FrequencyTestSettings();
            presentation = new FrequencyTestPresentation();
            Presentation = presentation;
            presentation.SizeChanged += new System.Windows.SizeChangedEventHandler(presentation_SizeChanged);
            settings.PropertyChanged += new PropertyChangedEventHandler(settings_PropertyChanged);
        }
        
        public UserControl Presentation
        {
            get;
            private set;
        }

        public void PreExecution()
        {
            alphabet = defaultAlphabet;
        }

        public void Execute()
        {
            Progress(0.0, 0.0);

            if (stringInput == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(alphabet))
            {
                GuiLogMessage(Properties.Resources.EmptyAlphabetInvalidMessage, NotificationLevel.Warning);
                return;
            }

            presentation.SetBackground(Brushes.LightYellow);

            string workstring = stringInput;

            // Any change in the word discards and recalculates the output. This is not that effective.
            lock (grams)
            {
                grams.Clear();

                if (settings.BoundaryFragments == 1)
                {
                    foreach (string word in WordTokenizer.tokenize(workstring))
                    {
                        ProcessWord(word);
                    }
                }
                else
                {
                    ProcessWord(workstring);
                }

                double sum = grams.Values.Sum(item => item[ABSOLUTE]);
                GuiLogMessage("Sum of all n-gram counts is: " + sum, NotificationLevel.Debug);

                // calculate scaled values
                foreach (double[] g in grams.Values)
                {
                    g[PERCENTAGED] = g[ABSOLUTE] / sum;
                    g[LOG2] = Math.Log(g[ABSOLUTE], 2);
                    g[SINKOV] = Math.Log(g[PERCENTAGED], Math.E);
                }

                // OUTPUT
                StringBuilder sb = new StringBuilder();
                arrayOutput = new int[grams.Count];

                //here, we sort by frequency occurrence if the user wants so
                if (settings.SortFrequencies)
                {
                    List<KeyValuePair<string, double[]>> list = grams.ToList();
                    list.Sort(delegate(KeyValuePair<string, double[]> a, KeyValuePair<string, double[]> b)
                    {
                        return a.Value[ABSOLUTE] > b.Value[ABSOLUTE] ? -1 : 1;
                    });

                    grams.Clear();

                    foreach (var i in list)
                    {
                        grams.Add(i.Key, i.Value);
                    }
                }                

                for (int i = 0; i < grams.Count; i++)
                {
                    KeyValuePair<string, double[]> item = grams.ElementAt(i);

                    sb.Append(item.Key + ":");
                    sb.Append(item.Value[ABSOLUTE] + ":");
                    sb.Append((item.Value[PERCENTAGED]) + Environment.NewLine);

                    arrayOutput[i] = (int)item.Value[ABSOLUTE];
                }
                stringOutput = sb.ToString();

                // update the presentation data
                updatePresentation();
            }

            OnPropertyChanged("StringOutput");
            OnPropertyChanged("ArrayOutput");
            OnPropertyChanged("DictionaryOutput");

            // Show progress finished.
            Progress(1.0, 1.0);
        }

        private void ProcessWord(string workstring)
        {
            if (settings.ProcessUnknownSymbols == 0)
            {
                workstring = StringUtil.StripUnknownSymbols(alphabet, workstring);
            }

            if (workstring.Length == 0)
            {
                return;
            }

            if (settings.CaseSensitivity == 0)
            {
                workstring = workstring.ToUpper();
            }

            int stepsize = 1;
            if (!settings.CountOverlapping)
            {
                stepsize = settings.GrammLength;
            }

            foreach (string g in GramTokenizer.tokenize(workstring, settings.GrammLength, settings.BoundaryFragments == 1, stepsize))
            {
                if (!grams.ContainsKey(g))
                {
                    grams[g] = new double[] { 1, 0, 0, 0 };
                }
                else
                {
                    grams[g][ABSOLUTE]++;
                }
            }
        }

        private void updatePresentation()
        {

            // remove all entries
            data.ValueCollection.Clear();

            //create header text
            string valueType = Properties.Resources.InPercent;
            if (settings.ShowAbsoluteValues)
            {
                valueType = Properties.Resources.AbsoluteValues;
            }
           
            switch (settings.GrammLength)
            {
                case 1:
                    presentation.SetHeadline(Properties.Resources.UnigramFrequencies + " " + valueType);
                    break;
                case 2:
                    presentation.SetHeadline(Properties.Resources.BigramFrequencies + " " + valueType);
                    break;
                case 3:
                    presentation.SetHeadline(Properties.Resources.TrigramFrequencies + " " + valueType);
                    break;
                case 4:
                    presentation.SetHeadline(Properties.Resources.TetragramFrequencies + " " + valueType);
                    break;
                case 5:
                    presentation.SetHeadline(Properties.Resources.PentagramFrequencies + " " + valueType);
                    break;
                case 6:
                    presentation.SetHeadline(Properties.Resources.HexagramFrequencies + " " + valueType);
                    break;
                case 7:
                    presentation.SetHeadline(Properties.Resources.HeptagramFrequencies + " " + valueType);
                    break;
                case 8:
                    presentation.SetHeadline(Properties.Resources.OctagramFrequencies + " " + valueType);
                    break;
                default:
                    presentation.SetHeadline(settings.GrammLength + Properties.Resources.nGram + " " + valueType);
                    break;
            }

            //update bars
            if (grams.Count > 0 && presentation.ActualWidth > 0)
            {
                // retrieve the maximum value from all grams
                double max = grams.Values.Max(item => item[PERCENTAGED]);
                
                // calculate the needed width for the chart (unscaled) in pixel
                double unscaledChartWidth = (grams.Count < 10 ? 10 : grams.Count + (settings.ShowTotal ? 1 : 0)) * presentationBarWidth + 3;
                if (grams.Count > settings.MaxNumberOfShownNGramms + (settings.ShowTotal ? 1 : 0))
                {
                    unscaledChartWidth = (settings.MaxNumberOfShownNGramms + (settings.ShowTotal ? 1 : 0)) * presentationBarWidth + 3;
                }

                // retrieve the maximum bar height from settings in pixel
                double maxBarHeight = settings.ChartHeight;                
                if (settings.Autozoom)
                {
                    // calculate the scaling-value depeding on the needed width and the current presentation width
                    presentationScaler = presentation.ActualWidth / unscaledChartWidth;
                    settings.Scale = (int)(presentationScaler * 10000.0);

                    //set the maximum bar height to the current heigth of chart-area in presentation (best fill)
                    //maxBarHeight = presentation.chartBars.ActualHeight - presentationBarHeightAdd;
                    maxBarHeight = (presentation.ActualHeight / presentationScaler) - (presentation.chartHeadline.ActualHeight + presentationBarHeightAdd);
                }

                //count all grams and create a total bar
                if (settings.ShowTotal)
                {
                    int sum = (int)grams.Values.Sum(item => item[ABSOLUTE]);
                    var element = new CollectionElement(1.0001 * max * (maxBarHeight / max), sum, 100, "Σ", true);
                    element.ColorA = Colors.LightGreen;
                    element.ColorB = Colors.DarkGreen;
                    data.ValueCollection.Add(element);
                }

                // calculate presentation bars height and add the to our local DataSource
                foreach (KeyValuePair<string, double[]> item in grams)
                {
                    double height = item.Value[PERCENTAGED] * (maxBarHeight / max);
                    CollectionElement row = new CollectionElement(height, (int)item.Value[ABSOLUTE], Math.Round(item.Value[PERCENTAGED] * 100, 2), item.Key, settings.ShowAbsoluteValues);
                    data.ValueCollection.Add(row);
                }

                //add dummy bars
                while (data.ValueCollection.Count + (settings.ShowTotal ? 1 : 0) < 10)
                {
                    data.ValueCollection.Add(new CollectionElement(0, 0, 0, string.Empty, false, System.Windows.Visibility.Visible));
                }
            }

            //finally, update ui
            presentation.ShowData(data, settings.SortFrequencies, settings.MaxNumberOfShownNGramms + (settings.ShowTotal ? 1 : 0));
            
        }

        private void presentation_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {           
            updatePresentation();
        }

        private void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Autozoom":
                case "ChartHeight":
                    updatePresentation();
                    break;
                case "Scale":
                    presentation.SetScaler( (double)settings.Scale / 10000.0);
                    break;
            }
        }


        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        public void PostExecution()
        {
        }

        public void Stop()
        {
            presentation.SetBackground(Brushes.LightGray);
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }
       
        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private void Progress(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion

    }
}