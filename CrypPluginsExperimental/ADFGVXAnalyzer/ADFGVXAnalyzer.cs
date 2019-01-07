/*
   Copyright 2018 CrypTool 2 Team <ct2contact@cryptool.org>

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
using System;
using System.Windows.Threading;
using System.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ADFGVXAnalyzer.Common;

namespace Cryptool.ADFGVXAnalyzer
{
    [Author("Dominik Vogt", "dvogt@posteo.de", null, null)]
    [PluginInfo("Cryptool.ADFGVXAnalyzer.Properties.Resources", "PluginCaption", "PluginTooltip", "ADFGVXAnalyzer/userdoc.xml", "ADFGVXAnalyzer/icon.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class ADFGVXAnalyzer : ICrypComponent
    {
        #region Private Variables

        private readonly ADFGVXAnalyzerSettings settings;
        private ADFGVXAnalyzerPresentation myPresentation;
        private readonly Logger log;

        private const int MaxBestListEntries = 10;
        private DateTime startTime;
        private DateTime endTime;
        private String[] messages;
        private Char separator = ',';
        private int keylengthmin = 0;
        private int keylengthmax = 0;
        private int threads = 0;
        private List<Thread> ThreadList;

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public ADFGVXAnalyzer()
        {
            settings = new ADFGVXAnalyzerSettings();
            myPresentation = new ADFGVXAnalyzerPresentation();
            Presentation = myPresentation;
            log = new Logger();

        }

        private bool CheckAlphabetLength()
        {
            if(!(settings.PlaintextAlphabet.Length == Math.Pow(settings.CiphertextAlphabet.Length, 2)))
            {
                GuiLogMessage(String.Format("The set alphabet length (={0} letters) does not match the given alphabet's length (={1} letters)",settings.PlaintextAlphabet.Length,Math.Pow(settings.CiphertextAlphabet.Length, 2)),NotificationLevel.Error);
                return false;
            }
            return true;
        }

        #region Data Properties

        [PropertyInfo(Direction.InputData, "CiphertextCaption", "CiphertextTooltip")]
        public string Messages
        {
            get;
            set;
        }

        private string plaintext;
        [PropertyInfo(Direction.OutputData, "PlaintextCaption", "PlaintextTooltip")]
        public string Plaintext
        {
            get
            {
                return this.plaintext;
            }
            set
            {
                this.plaintext = value;
                OnPropertyChanged("Plaintext");
            }
        }       

        private string transpositionkey;
        [PropertyInfo(Direction.OutputData, "BestKeyCaption", "BestKeyTooltip")]
        public string Transpositionkey
        {
            get
            {
                return this.transpositionkey;
            }
            set
            {
                this.transpositionkey = value;
                OnPropertyChanged("Transpositionkey");
            }
        }

        private string logText;
        [PropertyInfo(Direction.OutputData, "LogoutputCaption", "LogoutputTooltip")]
        public string LogText
        {
            get
            {
                return this.logText;
            }
            set
            {
                this.logText = value;
                OnPropertyChanged("LogText");
            }
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
            get;
            private set;
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            startTime = new DateTime();
            endTime = new DateTime();
            switch (settings.Separator)
            {
                case 0:
                    separator = ',';
                    break;
                case 1:
                    separator = ';';
                    break;
                case 2:
                    separator = '.';
                    break;
                default:
                    separator = ',';
                    break;

            }
            keylengthmin = settings.KeyLengthFrom;
            keylengthmax = settings.KeyLengthTo;
            threads = settings.Threads;
            ThreadList = new List<Thread>();
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            if (!CheckAlphabetLength()) { return; }
            ProgressChanged(0, 1);

            messages = Messages.Split(separator);

            ClearDisplay();

            UpdateDisplayStart();

            ThreadingHelper threadingHelper = new ThreadingHelper(threads, this);
            for (int i = keylengthmin; i <= keylengthmax; i++)
            {
                for (int j = 1; j <= threads; j++)
                {
                    Thread thread = new Thread(AlgorithmThread);
                    ThreadList.Add(thread);
                    thread.IsBackground = true;
                    thread.Start(new object[] {i,messages, j, threadingHelper});
                    LogText += Environment.NewLine + "Starting Thread: " + j;
                }
                foreach (Thread t in ThreadList)
                {
                    t.Join();
                }

            }
        }

        private void AlgorithmThread(object parametersObject)
        {
            object[] parameters = (object[])parametersObject;
            int i = (int)parameters[0];
            String[] messages = (string[])parameters[1];
            int j = (int)parameters[2];
            ThreadingHelper threadingHelper = (ThreadingHelper)parameters[3];
            Algorithm a = new Algorithm(i, messages, log, j, threadingHelper, this);
            a.SANgramsIC();

        }

        /// <summary>
        /// Adds an entry to the BestList
        /// </summary>
        /// <param name="score"></param>
        /// <param name="IoC1"></param>
        /// <param name="IoC2"></param>
        /// <param name="transkey"></param>
        /// <param name="plaintext"></param>
        public void AddNewBestListEntry(double score, double IoC1, double IoC2, String transkey, String plaintext)
        {
            var entry = new ResultEntry
            {
                Score = score,
                Ic1 = IoC1,
                Ic2 = IoC2,
                TransKey = transkey,
                Plaintext = plaintext
            };

            if (((ADFGVXAnalyzerPresentation)Presentation).BestList.Count == 0)
            {

            }
            else if (entry.Score > ((ADFGVXAnalyzerPresentation)Presentation).BestList.First().Score)
            {

            }

            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    if (((ADFGVXAnalyzerPresentation)Presentation).BestList.Count > 0 && entry.Score <= ((ADFGVXAnalyzerPresentation)Presentation).BestList.Last().Score)
                    {
                        return;
                    }
                    ((ADFGVXAnalyzerPresentation)Presentation).BestList.Add(entry);
                    ((ADFGVXAnalyzerPresentation)Presentation).BestList = new ObservableCollection<ResultEntry>(((ADFGVXAnalyzerPresentation)Presentation).BestList.OrderByDescending(i => i.Score));
                    if (((ADFGVXAnalyzerPresentation)Presentation).BestList.Count > MaxBestListEntries)
                    {
                        ((ADFGVXAnalyzerPresentation)Presentation).BestList.RemoveAt(MaxBestListEntries);
                    }
                    var ranking = 1;
                    foreach (var e in ((ADFGVXAnalyzerPresentation)Presentation).BestList)
                    {
                        e.Ranking = ranking;
                        ranking++;
                    }
                    ((ADFGVXAnalyzerPresentation)Presentation).ListView.DataContext = ((ADFGVXAnalyzerPresentation)Presentation).BestList;
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch (Exception e)
                {

                }
            }, null);
        }

        /// <summary>
        /// Clear the UI
        /// </summary>
        private void ClearDisplay()
        {
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                ((ADFGVXAnalyzerPresentation)Presentation).BestList.Clear();
            }, null);
        }

        /// <summary>
        /// Set start time in UI
        /// </summary>
        private void UpdateDisplayStart()
        {
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                startTime = DateTime.Now;
                ((ADFGVXAnalyzerPresentation)Presentation).startTime.Content = "" + startTime;
                ((ADFGVXAnalyzerPresentation)Presentation).endTime.Content = "";
                ((ADFGVXAnalyzerPresentation)Presentation).elapsedTime.Content = "";
            }, null);

        }

        /// <summary>
        /// Set end time in UI
        /// </summary>
        public void UpdateDisplayEnd(int keylength, long decryptions, long alldecyptions)
        {
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                endTime = DateTime.Now;
                var elapsedtime = endTime.Subtract(startTime);
                var elapsedspan = new TimeSpan(elapsedtime.Days, elapsedtime.Hours, elapsedtime.Minutes, elapsedtime.Seconds, 0);
                ((ADFGVXAnalyzerPresentation)Presentation).endTime.Content = "" + endTime;
                ((ADFGVXAnalyzerPresentation)Presentation).elapsedTime.Content = "" + elapsedspan;
                ((ADFGVXAnalyzerPresentation)Presentation).currentAnalysedKeylength.Content = "" + keylength;
                ((ADFGVXAnalyzerPresentation)Presentation).keys.Content = "" + decryptions + " / " + alldecyptions;
                ((ADFGVXAnalyzerPresentation)Presentation).messageCount.Content = "" + messages.Length;

            }, null);
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
            foreach(Thread t in ThreadList)
            {
                t.Abort();
            }        
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

        public event PropertyChangedEventHandler PropertyChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        public void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion       

    }

    #region Helper Classes

    public class ResultEntry
    {
        public int Ranking { get; set; }
        public double Score { get; set; }
        public double Ic1 { get; set; }
        public double Ic2 { get; set; }
        public string TransKey { get; set; }
        public string SubsKey { get; set; }
        public string Plaintext { get; set; }

    }

    #endregion
}


