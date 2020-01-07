/*
   Copyright 2019 George Lasry, Nils Kopal, CrypTool 2 Team

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
using Cryptool.PluginBase;
using System.ComponentModel;
using Cryptool.PluginBase.Miscellaneous;
using System.Windows.Controls;
using System.Threading;
using Cryptool.Plugins.Ipc.Messages;
using System.Windows.Threading;
using System.IO;
using System.Text;
using Cryptool.CrypAnalysisViewControl;
using PlayfairAnalysis.Common;
using PlayfairAnalysis;

namespace Cryptool.PlayfairAnalyzer
{
    public delegate void PluginProgress(double current, double maximum);
    public delegate void UpdateOutput(String keyString, String plaintextString, string score);

    [Author("George Lasry, Nils Kopal", "George.Lasry@cryptool.org", "Uni Kassel", "https://www.cryptool.org")]
    [PluginInfo("Cryptool.PlayfairAnalyzer.Properties.Resources", "PluginCaption", "PluginTooltip", "PlayfairAnalyzer/DetailedDescription/doc.xml", "PlayfairAnalyzer/icon.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class PlayfairAnalyzer : ICrypComponent
    {    
        public const long fileSizeResource11bin = 617831552; //this is the file size of "Resource-1-1.bin", which is downloaded from CrypToolStore

        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PropertyChangedEventHandler PropertyChanged;
        
        private bool _Running = false;

        private string _Ciphertext = null;
        private string _Crib = null;
        private string _Plaintext = null;
        private string _Key = null;
        private string _Score = null;

        private const string PLAYFAIR_ALPHABET = "ABCDEFGHIKLMNOPQRSTUVWXYZ";

        private PlayfairAnalyzerSettings _settings = new PlayfairAnalyzerSettings();
        private AssignmentPresentation _presentation = new AssignmentPresentation();
        private DateTime _startTime;
        private bool _alreadyExecuted = false;
        private CancellationTokenSource _cancellationTokenSource;

        [PropertyInfo(Direction.InputData, "CiphertextCaption", "CiphertextTooltip", true)]
        public string Ciphertext
        {
            get => _Ciphertext;
            set
            {
                if (_Ciphertext != value)
                {
                    _Ciphertext = value;
                    if (!String.IsNullOrEmpty(_Ciphertext))
                    {
                        _Ciphertext = RemoveInvalidSymbols(_Ciphertext);
                    }
                }
            }
        }

        /// <summary>
        /// Removes all invalid symbols, which are not part of the PLAYFAIR_ALPHABET
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string RemoveInvalidSymbols(string value)
        {
            value = value.ToUpper();
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                if (PLAYFAIR_ALPHABET.Contains(value.Substring(i, 1)))
                {
                    builder.Append(value.Substring(i, 1));
                }
            }
            return builder.ToString();
        }

        [PropertyInfo(Direction.InputData, "CribCaption", "CribTooltip", false)]
        public string Crib
        {
            get => _Crib;
            set
            {
                if (_Crib != value)
                {
                    _Crib = value;
                }
            }
        }        

        [PropertyInfo(Direction.OutputData, "PlaintextCaption", "PlaintextTooltip", false)]
        public string Plaintext
        {
            get { return _Plaintext; }
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    _Plaintext = value;
                    OnPropertyChanged("Plaintext");
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "KeyCaption", "KeyTooltip", false)]
        public string Key
        {
            get { return _Key; }
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    _Key = value;
                    OnPropertyChanged("Key");
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "ScoreCaption", "ScoreTooltip", false)]
        public string Score
        {
            get { return _Score; }
            set
            {
                if (!String.IsNullOrEmpty(value))
                {
                    _Score = value;
                    OnPropertyChanged("Score");
                }
            }
        }

        public void PreExecution()
        {
            //reset inputs, outputs, and sending queue
            _Plaintext = String.Empty;
            _alreadyExecuted = false;
        }

        public void PostExecution()
        {
            
        }          

        public ISettings Settings
        {
            get { return _settings; }
        }

        public UserControl Presentation
        {
            get { return _presentation; }
        }

        public void Execute()
        {       
            // some checks:

            if (_alreadyExecuted)
            {
                GuiLogMessage(Properties.Resources.AlreadyExecuted, NotificationLevel.Error);
                return;
            }

            _alreadyExecuted = true;
            string hexfilename = null;

            try
            {
                // Step -2: Download and reference language statistics file
                switch (_settings.Language)
                {
                    case Language.English:
                        // Current English file is Resource-1-1
                        hexfilename = CrypToolStore.ResourceHelper.GetResourceFile(1, 1, this, "To work with the Playfair Analyzer, a special language statistics file (English log hexagrams) has to be downloaded. If you do not download this file, the Playfair Analyzer won't work. Please press the download button to start the download.");
                        if (string.IsNullOrEmpty(hexfilename))
                        {
                            GuiLogMessage("The Playfair Analyzer cannot work without English hexagram statistics. Please download the statistics first. The download dialog will automatically popup the next time you start the Playfair Analyzer.", NotificationLevel.Warning);
                            return;
                        }
                        break;
                    default:
                        throw new NotImplementedException(String.Format("Language {0} has not been implemented", _settings.Language.ToString()));
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(String.Format("An error occurred during execution of CrypToolStore resource download: {0}", ex.Message), NotificationLevel.Error);
                return;
            }

            try
            {
                //Step -1: Check for correct file size; if wrong => delete file
                long length = new FileInfo(hexfilename).Length;
                if (length != fileSizeResource11bin)
                {
                    GuiLogMessage(String.Format("The filesize of the statistics file is wrong ({0} byte). Expected {1} byte. Delete file now. Please restart the workspace to download the file again", length, fileSizeResource11bin), NotificationLevel.Error);
                    File.Delete(hexfilename);
                    return;
                }
            }
            catch(Exception ex)
            {
                GuiLogMessage(String.Format("An error occurred during file size check of resource file: {0}", ex.Message), NotificationLevel.Error);
                return;
            }

            try
            {                
                //Step 0: Set running true :-)
                _Running = true;

                //set start time in presentation; remove elapsedTime and endTime
                _startTime = DateTime.Now;
                _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    try
                    {
                        _presentation.StartTime.Value = _startTime.ToString();
                        _presentation.EndTime.Value = string.Empty;
                        _presentation.ElapsedTime.Value = string.Empty;
                        _presentation.BestList.Clear();
                    }
                    catch (Exception)
                    {
                        //wtf?
                    }
                }, null);

                //Step 1: Start analysis
                CtAPI.reset();

                _cancellationTokenSource = new CancellationTokenSource();
                var ct = _cancellationTokenSource.Token;

                if (!Stats.readHexagramStatsFile(hexfilename, ct))
                {
                    if (ct.IsCancellationRequested)
                    {
                        return;
                    }
                    throw new Exception($"Error trying to read hexagram stats file: {hexfilename}");
                }

                var cipherText = Utils.getText(Ciphertext);
                if (cipherText.Length % 2 != 0)
                {
                    throw new Exception($"Ciphertext length must be even - found {cipherText.Length} characters.");
                }

                var threads = (_settings.Threads + 1);  //index starts at 0; thus +1
                SolvePlayfair.solveMultithreaded(cipherText, Crib ?? "", threads, _settings.Cycles, null, ct);
                GuiLogMessage("Starting analysis now.", NotificationLevel.Debug);

                DateTime lastUpdateTime = DateTime.Now;
                //Step 6: while both pipes are connected, we busy wait
                while (!ct.IsCancellationRequested)
                {
                    Thread.Sleep(100);
                    if (!_Running)
                    {
                        return;
                    }
                    if (DateTime.Now >= lastUpdateTime.AddSeconds(1))
                    {
                        lastUpdateTime = DateTime.Now;
                        //set elapsed time in presentation
                        _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                        {
                            try
                            {              
                                var elapsedTime =  (DateTime.Now - _startTime);
                                _presentation.ElapsedTime.Value = new TimeSpan(elapsedTime.Hours, elapsedTime.Minutes, elapsedTime.Seconds).ToString();
                            }
                            catch (Exception)
                            {
                                //wtf?
                            }
                        }, null);
                    }
                }
            }
            finally
            {
                _cancellationTokenSource?.Cancel();
                _Running = false;

                //set end time in presentation
                _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    try
                    {
                        _presentation.EndTime.Value = DateTime.Now.ToString();
                        var elapsedTime = (DateTime.Now - _startTime);
                        _presentation.ElapsedTime.Value = new TimeSpan(elapsedTime.Hours, elapsedTime.Minutes, elapsedTime.Seconds).ToString();
                    }
                    catch (Exception)
                    {
                        //wtf?
                    }
                }, null);

            }            
        }

        /// <summary>
        /// Handles incoming best list entries
        /// </summary>
        /// <param name="value"></param>
        private void HandleIncomingBestList(string value)
        {
            // it is allowed to send an empty string as well as a -
            // if we receive that, we just ignore it
            if (String.IsNullOrEmpty(value) || value.Equals("-"))
            {
                return;
            }

            try
            {                
                _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    try
                    {
                        _presentation.BestList.Clear();
                    }
                    catch (Exception ex)
                    {
                        GuiLogMessage(String.Format("Error occured while clearing best list: {0}", ex.Message), NotificationLevel.Error);
                    }
                }, null);
                string[] lines = value.Trim().Split('\n', '\r');
                foreach (string line in lines)
                {
                    string[] values = line.Trim().Split(';');
                    if (values.Length != 5)
                    {
                        GuiLogMessage(String.Format("Received invalid best list entry: {0}", line), NotificationLevel.Warning);
                        continue;
                    }
                    ResultEntry resultEntry = new ResultEntry();
                    resultEntry.Ranking = int.Parse(values[0]);
                    resultEntry.Value = values[1];
                    resultEntry.Key = values[2];
                    resultEntry.Text = values[3];
                    resultEntry.Info = values[4];
                    _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        try
                        {
                            _presentation.BestList.Add(resultEntry);
                        }
                        catch (Exception ex)
                        {
                            GuiLogMessage(String.Format("Error occured while adding new entry to best list: {0}", ex.Message), NotificationLevel.Error);
                        }
                    }, null);
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(String.Format("Error occured while handling new best list: {0}", ex.Message), NotificationLevel.Error);
            }
        }

        public void Stop()
        {
            _Running = false;
            _cancellationTokenSource?.Cancel();
        }

        public void Initialize()
        {
            _presentation.UpdateOutputFromUserChoice += UpdateOutput;

            CtAPI.BestListChangedEvent += HandleIncomingBestList;
            CtAPI.BestResultChangedHandlerEvent += bestResult =>
            {
                UpdateOutput(bestResult.keyString, bestResult.plaintextString, string.Format("{0,12:N0}", bestResult.score));
            };
            CtAPI.ProgressChangedEvent += (currentValue, maxValue) => OnProgressChanged(currentValue, maxValue);
            CtAPI.GoodbyeEvent += () =>
            {
                _cancellationTokenSource?.Cancel();
            };
        }

        public void Dispose()
        {
            
        }

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void OnProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        private void UpdateOutput(string keyString, string plaintextString, string score)
        {
            Score = score;
            Plaintext = plaintextString;
            Key = keyString;
        }
    }

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

        public string Value { get; set; }
        public string Key { get; set; }
        public string Text { get; set; }
        public string Info { get; set; }

        public string ClipboardValue => Value.ToString();
        public string ClipboardKey => Key;
        public string ClipboardText => Text;
        public string ClipboardEntry =>
            "Rank: " + Ranking + Environment.NewLine +
            "Value: " + Value + Environment.NewLine +
            "Key: " + Key + Environment.NewLine +
            "Text: " + Text + Environment.NewLine +
            "Info: " + Info;

    }
}
