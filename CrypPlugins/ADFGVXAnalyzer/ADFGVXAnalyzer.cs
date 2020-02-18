﻿/*
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
using common;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Cryptool.CrypAnalysisViewControl;

namespace ADFGVXAnalyzer
{
    [Author("Dominik Vogt", "dvogt@posteo.de", null, null)]
    [PluginInfo("ADFGVXAnalyzer.Properties.Resources", "ADFGVXAnalyzerCaption", "ADFGVXAnalyzerToolTip", "ADFGVXAnalyzer/userdoc.xml", new[] { "ADFGVXAnalyzer/icon.png" })]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class ADFGVXAnalyzer : ICrypComponent
    {
        #region Private Variables

        // HOWTO: You need to adapt the settings class as well, see the corresponding file.
        private readonly ADFGVXANalyzerSettings settings;
        private ADFGVXAnalyzerPresentation myPresentation;
        private readonly Logger log;

        private const int MaxBestListEntries = 10;
        private DateTime startTime;
        private DateTime endTime;
        private String[] messages;
        private String separator;
        private int keylength = 0;
        private int threads = 0;
        private List<Thread> ThreadList;
        private int keysPerSecond;

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public ADFGVXAnalyzer()
        {
            settings = new ADFGVXANalyzerSettings();
            myPresentation = new ADFGVXAnalyzerPresentation();
            myPresentation.getTranspositionResult += this.getTranspositionResult;
            Presentation = myPresentation;
            log = new Logger();

        }

        #region Data Properties
     
        [PropertyInfo(Direction.InputData, "MessagesCaption", "MessagesToolTip")]
        public string Messages
        {
            get;
            set;
        }
        
        private string transpositionResult;
        [PropertyInfo(Direction.OutputData, "TranspositionResultCaption", "TranspositionResultToolTip")]
        public string TranspositionResult
        {
            get
            {
                return this.transpositionResult;
            }
            set
            {
                this.transpositionResult = value;
                OnPropertyChanged("TranspositionResult");
            }
        }

        private string logText;
        [PropertyInfo(Direction.OutputData, "LogTextCaption", "LogTextToolTip")]
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

        private string transpositionkey;
        [PropertyInfo(Direction.OutputData, "TranspositionkeyCaption", "TranspositionkeyToolTip")]
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
            try
            {
                startTime = new DateTime();
                endTime = new DateTime();
                keylength = settings.KeyLength;
                threads = settings.CoresUsed + 1;
                ThreadList = new List<Thread>();
                LogText = "";
            }
            catch (Exception ex)
            {
                GuiLogMessage("PreExecution: " + ex.Message, NotificationLevel.Error);
            }

        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);
            try
            {
                if (!CheckAlphabetLength()) { return; }

                separator = ChooseSeparator(settings.Separator);

                messages = Messages.Split(new[] { separator }, StringSplitOptions.None);

                if (!CheckMessages()) { return; }
            }
            catch (Exception ex)
            {
                GuiLogMessage("ExecuteChecks: " + ex.Message, NotificationLevel.Error);
            }

            ClearDisplay();

            UpdateDisplayStart();

            try
            {
                ThreadingHelper threadingHelper = new ThreadingHelper(threads, this);
                for (int j = 1; j <= threads; j++)
                {
                    Thread thread = new Thread(AlgorithmThread);
                    ThreadList.Add(thread);
                    thread.IsBackground = true;
                    thread.Start(new object[] { keylength, messages, j, threadingHelper, settings });
                    LogText += Environment.NewLine + "Starting Thread: " + j;
                }
                foreach (Thread t in ThreadList)
                {
                    t.Join();
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage("Execute: " + ex.Message, NotificationLevel.Error);
            }
        }


        private void AlgorithmThread(object parametersObject)
        {

            object[] parameters = (object[])parametersObject;
            int i = (int)parameters[0];
            String[] messages = (string[])parameters[1];
            int j = (int)parameters[2];
            ThreadingHelper threadingHelper = (ThreadingHelper)parameters[3];
            ADFGVXANalyzerSettings settings = (ADFGVXANalyzerSettings)parameters[4];
            Algorithm a = new Algorithm(i, messages, log, j, threadingHelper, settings, this);
            a.SANgramsIC();


        }

        /// <summary>
        /// Adds an entry to the BestList
        /// </summary>
        /// <param name="score"></param>
        /// <param name="IoC1"></param>
        /// <param name="IoC2"></param>
        /// <param name="transkey"></param>
        /// <param name="transpositionResult"></param>
        public void AddNewBestListEntry(double score, double IoC1, double IoC2, String transkey, String transpositionResult)
        {
            try
            {
                var entry = new ResultEntry
                {
                    Score = score,
                    Ic1 = IoC1,
                    Ic2 = IoC2,
                    TransKey = transkey,
                    TranspositionResult = transpositionResult
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

                        //Insert new entry at correct place to sustain order of list:
                        var insertIndex = myPresentation.BestList.TakeWhile(e => e.Score > entry.Score).Count();
                        myPresentation.BestList.Insert(insertIndex, entry);

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
                    }
                    // ReSharper disable once EmptyGeneralCatchClause
                    catch (Exception e)
                    {

                    }
                }, null);
            }
            catch (Exception ex)
            {
                GuiLogMessage("AddNewBestListEntry: " + ex.Message, NotificationLevel.Error);
            }
        }




        /// <summary>
        /// Clear the UI
        /// </summary>
        private void ClearDisplay()
        {
            try
            {
                Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    ((ADFGVXAnalyzerPresentation)Presentation).StartTime.Value = "";
                    ((ADFGVXAnalyzerPresentation)Presentation).EndTime.Value = "";
                    ((ADFGVXAnalyzerPresentation)Presentation).ElapsedTime.Value = "";
                    ((ADFGVXAnalyzerPresentation)Presentation).CurrentAnalysedKeylength.Value = "";
                    ((ADFGVXAnalyzerPresentation)Presentation).Keys.Value = "";
                    ((ADFGVXAnalyzerPresentation)Presentation).MessageCount.Value = "";
                    ((ADFGVXAnalyzerPresentation)Presentation).BestList.Clear();
                }, null);
            }
            catch (Exception ex)
            {
                GuiLogMessage("ClearDisplay: " + ex.Message, NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Set start time in UI
        /// </summary>
        private void UpdateDisplayStart()
        {
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                startTime = DateTime.Now;
                ((ADFGVXAnalyzerPresentation)Presentation).StartTime.Value = "" + startTime;
                ((ADFGVXAnalyzerPresentation)Presentation).EndTime.Value = "";
                ((ADFGVXAnalyzerPresentation)Presentation).ElapsedTime.Value = "";
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
                double totalSeconds = elapsedtime.TotalSeconds;
                keysPerSecond = (int)(decryptions / totalSeconds);
                var elapsedspan = new TimeSpan(elapsedtime.Days, elapsedtime.Hours, elapsedtime.Minutes, elapsedtime.Seconds, 0);
                ((ADFGVXAnalyzerPresentation)Presentation).EndTime.Value = "" + endTime;
                ((ADFGVXAnalyzerPresentation)Presentation).ElapsedTime.Value = "" + elapsedspan;
                ((ADFGVXAnalyzerPresentation)Presentation).CurrentAnalysedKeylength.Value = "" + keylength;
                ((ADFGVXAnalyzerPresentation)Presentation).Keys.Value = "" + keysPerSecond + " (" + decryptions + ")";
                ((ADFGVXAnalyzerPresentation)Presentation).MessageCount.Value = "" + messages.Length;

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
            foreach (Thread t in ThreadList)
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

        #region Helper Methods

        private bool CheckAlphabetLength()
        {
            try
            {
                if (!(settings.Alphabet.Length == Math.Pow(settings.EncryptAlphabet.Length, 2)))
                {
                    GuiLogMessage("Plaintext and ciphertext length do not match", NotificationLevel.Error);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                GuiLogMessage("CheckAlphabetLength: " + ex.Message, NotificationLevel.Error);
                return false;
            }
        }

        private bool CheckMessages()
        {
            try
            {
                foreach (String message in messages)
                {
                    foreach (char c in message)
                    {
                        if (settings.EncryptAlphabet.IndexOf(c) == -1) //if c not even present in the string, this will output value -1
                        {
                            GuiLogMessage("One of the messages contains invalid character", NotificationLevel.Error);
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                GuiLogMessage("CheckMessages: " + ex.Message, NotificationLevel.Error);
                return false;
            }
        }

        private string ChooseSeparator(int separator)
        {
            try
            {
                switch (separator)
                {
                    case 0:
                        return Environment.NewLine;
                        break;
                    case 1:
                        return ",";
                        break;
                    case 2:
                        return ";";
                        break;
                    case 3:
                        return " ";
                        break;
                    default:
                        return null;
                        break;
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage("ChooseSeparator: " + ex.Message, NotificationLevel.Error);
                return null;
            }
        }

        //Method to send a transactionhash by doubleclick
        private void getTranspositionResult(ResultEntry resultEntry)
        {
            try
            {
                TranspositionResult = resultEntry.TranspositionResult;
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
            }
        }

        #endregion       

    }

    #region Helper Classes

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

        public double Score { get; set; }
        public double Ic1 { get; set; }
        public double Ic2 { get; set; }
        public string TransKey { get; set; }
        public string TranspositionResult { get; set; }

        public string ClipboardValue => $"Score: {Score}\tIc1: {Ic1}\tIc2: {Ic2}";
        public string ClipboardKey => TransKey;
        public string ClipboardText => TranspositionResult;
        public string ClipboardEntry =>
            "Ranking: " + Ranking + Environment.NewLine +
            "Score: " + Score + Environment.NewLine +
            "Ic1: " + Ic1 + Environment.NewLine +
            "Ic2: " + Ic2 + Environment.NewLine +
            "TransKey: " + TransKey + Environment.NewLine +
            "Plaintext: " + TranspositionResult;
    }

    #endregion
}


