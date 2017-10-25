using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// additional needed libs
using System.Diagnostics;

//Cryptool 2.0 specific includes
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.Threading;
using Cryptool.EnigmaBreaker.Properties;

namespace Cryptool.EnigmaBreaker
{
    public class IntermediateResultEventArgs : EventArgs
    {
        public string Result { get; set; }
    }


    public class EnigmaAnalyzer
    {

        #region Private classes/structs

        private class analysisConfigSettings : IComparable<analysisConfigSettings>
        {
            public int Rotor1;
            public int Rotor2;
            public int Rotor3;
            public int Ring1;
            public int Ring2;
            public int Ring3;
            public string InitialRotorPos;
            public string PlugBoard = "-- no plugs --";
            public double Score;

            #region IComparable<analysisConfigSettings> Member

            public int CompareTo(analysisConfigSettings other)
            {
                return this.Score.CompareTo(other.Score);
            }

            #endregion
        }

        #endregion

        #region Private member variables and constants

        EnigmaBreaker _pluginFacade;
        EnigmaBreakerSettings _settings;
        EnigmaCore _core;
        AssignmentPresentation _presentation;

        private bool _stopped = false;

        private int rotorPos1 = 0;
        private int rotorPos2 = 0;
        private int rotorPos3 = 0;

        private int keys = 0;
        private DateTime lasttime = DateTime.Now;

        private List<analysisConfigSettings> analysisCandidates = new List<analysisConfigSettings>();
        private const int maxAnalysisEntries = 10; // should go in settings under "Analysis Options"
        private const int _maxBestListEntries = 100;

        #endregion

        #region Private member methods

        /// <summary>
        /// Adds an entry to the BestList
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="ciphertext"></param>
        private void AddNewBestListEntry(string key, double value, string ciphertext)
        {
            var entry = new ResultEntry
            {
                Key = key,
                Text = ciphertext,
                Value = value
            };

            if (_presentation.BestList.Count == 0)
            {
                _pluginFacade.BestPlaintext = entry.Text;
                _pluginFacade.BestKey = entry.Key;
            }
            else if (entry.Value > _presentation.BestList.First().Value)
            {
                _pluginFacade.BestPlaintext = entry.Text;
                _pluginFacade.BestKey = entry.Key;
            }

            _pluginFacade.Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    if (_presentation.BestList.Count > 0 && entry.Value <= _presentation.BestList.Last().Value)
                    {
                        return;
                    }
                    _presentation.BestList.Add(entry);
                    _presentation.BestList = new ObservableCollection<ResultEntry>(_presentation.BestList.OrderByDescending(i => i.Value));
                    if (_presentation.BestList.Count > _maxBestListEntries)
                    {
                        _presentation.BestList.RemoveAt(_maxBestListEntries);
                    }
                    var ranking = 1;
                    foreach (var e in _presentation.BestList)
                    {
                        e.Ranking = ranking;
                        ranking++;
                    }
                    _presentation.ListView.DataContext = _presentation.BestList;
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch (Exception e)
                {
                    //wtf?
                    Console.WriteLine("e3: " + e);
                }
            }, null);
        }

        /// <summary>
        /// This method prints the currently calculated keys per second in the ui.
        /// </summary>
        private void UpdateKeysPerSec()
        {
            if (DateTime.Now >= lasttime.AddMilliseconds(1000))
            {
                var keysDispatcher = keys;
                _pluginFacade.Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    try
                    {
                        _presentation.currentSpeed.Content = string.Format("{0:0,0}", keysDispatcher);
                    }
                    // ReSharper disable once EmptyGeneralCatchClause
                    catch (Exception e)
                    {
                        //wtf?
                        Console.WriteLine("e1: " + e);
                    }
                }, null);
                keys = 0;
                lasttime = DateTime.Now;
            }
        }

        /// <summary>
        /// This method basically brute-forces all possible rotors
        /// </summary>
        /// <param name="text">The ciphertext</param>
        private void analyzeRotors(string text)
        {
            _pluginFacade.UpdateDisplayEnd(Resources.RotorsCaption);

            // Start the stopwatch
            Stopwatch sw = Stopwatch.StartNew();
            //int trials = 0;

            for (int i = 0; i < 8 && !_stopped; i++)
            {
                //Rotor 3 (slowest)
                if (!includeRotor(i)) continue;
                _settings.Rotor3 = i;
                for (int j = 0; j < 8 && !_stopped; j++)
                {
                    // Rotor 2 (middle)
                    if (!includeRotor(j) || j == i) continue;
                    _settings.Rotor2 = j;

                    for (int k = 0; k < 8 && !_stopped; k++)
                    {
                        // Rotor 1 (fastest)
                        if (!includeRotor(k) || k == i || k == j) continue;
                        _settings.Rotor1 = k;

                        //set the internal Config to the new rotors
                        _core.setInternalConfig(k, j, i, 0, _settings.Reflector,
                            _settings.AnalyzeRings ? 1 : _settings.Ring1,
                            _settings.AnalyzeRings ? 1 : _settings.Ring2,
                            _settings.AnalyzeRings ? 1 : _settings.Ring3,
                            _settings.Ring4,
                            _settings.AnalyzePlugs ? _settings.Alphabet : _settings.PlugBoard);

                        
                        if (_settings.AnalyzeInitialRotorPos)
                            analyzeInitialRotorPos(text);
                        else
                            CheckKeyWithInitialRotorPos(text);
                        //trials++;
                        keys++;

                        // print keys/sec in the ui
                        UpdateKeysPerSec();

                        _pluginFacade.ShowProgress(i * Math.Pow(8, 2) + j * 8 + k, Math.Pow(8, 3));
                    } // Rotor 1
                } // Rotor 2

                _pluginFacade.UpdateDisplayEnd(Resources.RotorsCaption);

            } // Rotor 3

            _pluginFacade.UpdateDisplayEnd(Resources.RotorsCaption);

            // Stop the stopwatch
            sw.Stop();

            //string msg = String.Format("Processed {0} rotor permutations in {1}!", trials, sw.Elapsed.ToString());
            //_pluginFacade.LogMessage(msg, NotificationLevel.Info);
        }

        /// <summary>
        /// This method brute-forces all possible rotor positions (i.e. the key)
        /// The rotors themselfs need to be setup prior to calling this method
        /// </summary>
        /// <param name="text">The ciphertext</param>
        private void analyzeInitialRotorPos(string text)
        {
            _pluginFacade.UpdateDisplayEnd(Resources.RotorsCaption);

            /////////////////////////////////////////
            // now run through all rotor positions..

            // Start the stopwatch
            Stopwatch sw = Stopwatch.StartNew();
            int trials = 0;

            // Rotor 1 positions (fastest)
            for (int l = 0; l < 26 && !_stopped; l++)
            {
                for (int m = 0; m < 26 && !_stopped; m++)
                {
                    for (int n = 0; n < 26 && !_stopped; n++)
                    {
                        checkKey(l, m, n, text);
                        trials++;
                        keys++;

                        // print keys/sec in the ui
                        UpdateKeysPerSec();
                    }
                }

                _pluginFacade.UpdateDisplayEnd(Resources.RotorsCaption);

            } // Rotor1 positions

            // Stop the stopwatch
            sw.Stop();

            //string msg = String.Format("Processed {0} rotor positions for {1},{2},{3} in {4}!",
            //    trials, (rotorEnum)_core.Rotor3, (rotorEnum)_core.Rotor2, (rotorEnum)_core.Rotor1, sw.Elapsed.ToString());
            //_pluginFacade.LogMessage(msg, NotificationLevel.Info);
        }

        /// <summary>
        /// This method calls the "checkKey" method with the initial rotor positions and the given text.
        /// </summary>
        /// <param name="text">The ciphertext</param>
        /// <returns>The result of encrypting/decrypting the ciphertext with the given key</returns>
        private string CheckKeyWithInitialRotorPos(string text)
        {
            return checkKey(rotorPos1, rotorPos2, rotorPos3, text);
        }

        /// <summary>
        /// This method performs a trial encryption with the given rotor positions (i.e. the key)
        /// If the trial encryption results in a better result as before encountered, the current settings will
        /// remembered in analysisCandidates-List. 
        /// </summary>
        /// <param name="rotor1Pos">Integer value for rotor 1 position (values range from 0 to 25)</param>
        /// <param name="rotor2Pos">Integer value for rotor 2 position (values range from 0 to 25)</param>
        /// <param name="rotor3Pos">Integer value for rotor 3 position (values range from 0 to 25)</param>
        /// <param name="text">The ciphertext</param>
        /// <returns>The result of encrypting/decrypting the ciphertext with the given key</returns>
        private string checkKey(int rotor1Pos, int rotor2Pos, int rotor3Pos, string text)
        {
            string result = _core.Encrypt(rotor1Pos, rotor2Pos, rotor3Pos, 0, text);
            double newScore = calculateScore(result, _settings.SearchMethod);

            if (analysisCandidates.Count >= maxAnalysisEntries)
            {
                // List is full, check if we need to remove one
                if (newScore > analysisCandidates[0].Score)
                {
                    double currentMax = analysisCandidates[analysisCandidates.Count - 1].Score;

                    analysisConfigSettings csetting = new analysisConfigSettings();
                    csetting.Score = newScore;
                    csetting.Rotor1 = _core.Rotor1;
                    csetting.Rotor2 = _core.Rotor2;
                    csetting.Rotor3 = _core.Rotor3;
                    csetting.Ring1 = _core.Ring1;
                    csetting.Ring2 = _core.Ring2;
                    csetting.Ring3 = _core.Ring3;
                    csetting.PlugBoard = _core.Plugboard;
                    csetting.InitialRotorPos = _settings.Alphabet[rotor3Pos].ToString() + _settings.Alphabet[rotor2Pos].ToString() + _settings.Alphabet[rotor1Pos].ToString();

                    analysisCandidates.Add(csetting);
                    analysisCandidates.Sort();

                    // remove the smallest one
                    analysisCandidates.RemoveAt(0);


                    if (newScore > currentMax)
                    {
                        // new best option
                        string status = String.Format("ANALYSIS: ==> Found better rotor settings: {0},{1},{2}; {3},{4},{5}; Key: {6}; I.C.={7} <==",
                        (rotorEnum)csetting.Rotor3, (rotorEnum)csetting.Rotor2, (rotorEnum)csetting.Rotor1,
                        csetting.Ring3.ToString("00"), csetting.Ring2.ToString("00"), csetting.Ring1.ToString("00"),
                        csetting.InitialRotorPos, newScore.ToString());
                        //_pluginFacade.LogMessage(status, NotificationLevel.Info);
                        
                        string key = String.Format("{0}, {1}, {2} / {3}, {4}, {5} / {6} / {7}",
                        (rotorEnum)csetting.Rotor3, (rotorEnum)csetting.Rotor2, (rotorEnum)csetting.Rotor1,
                        csetting.Ring3.ToString(), csetting.Ring2.ToString(), csetting.Ring1.ToString(),
                        _pluginFacade.pB2String(csetting.PlugBoard), csetting.InitialRotorPos);

                        AddNewBestListEntry(key, newScore, encrypt(csetting, text, csetting.PlugBoard));
                        printBestCandidates();

                        // fire the event, so someting becomes visible..
                        if (OnIntermediateResult != null)
                        {
                            OnIntermediateResult(this, new IntermediateResultEventArgs() { Result = result });
                        }
                    }


                }
            }
            else
            {
                //there is room left, hence add the element

                analysisConfigSettings csetting = new analysisConfigSettings();
                csetting.Score = newScore;
                csetting.Rotor1 = _core.Rotor1;
                csetting.Rotor2 = _core.Rotor2;
                csetting.Rotor3 = _core.Rotor3;
                csetting.Ring1 = _core.Ring1;
                csetting.Ring2 = _core.Ring2;
                csetting.Ring3 = _core.Ring3;
                csetting.PlugBoard = _core.Plugboard;
                csetting.InitialRotorPos = _settings.Alphabet[rotor3Pos].ToString() + _settings.Alphabet[rotor2Pos].ToString() + _settings.Alphabet[rotor1Pos].ToString();

                analysisCandidates.Add(csetting);
                analysisCandidates.Sort();

                if (analysisCandidates.Count == maxAnalysisEntries)
                {
                    printBestCandidates();

                    // current best option
                    analysisConfigSettings bestOption = analysisCandidates[analysisCandidates.Count - 1];

                    string status = String.Format("ANALYSIS: Best candidates is filled. Best option so far: {0},{1},{2}; Key: {3}; I.C.={4}",
                    (rotorEnum)bestOption.Rotor3, (rotorEnum)bestOption.Rotor2, (rotorEnum)bestOption.Rotor1, bestOption.InitialRotorPos, bestOption.Score.ToString());
                    //_pluginFacade.LogMessage(status, NotificationLevel.Debug);

                    string key = String.Format("{0}, {1}, {2} / {3}, {4}, {5} / {6} / {7}",
                        (rotorEnum)csetting.Rotor3, (rotorEnum)csetting.Rotor2, (rotorEnum)csetting.Rotor1,
                        csetting.Ring3.ToString(), csetting.Ring2.ToString(), csetting.Ring1.ToString(),
                        _pluginFacade.pB2String(csetting.PlugBoard), csetting.InitialRotorPos);

                    AddNewBestListEntry(key, newScore, encrypt(csetting, text, csetting.PlugBoard));
                        

                    // fire the event, so someting becomes visible..
                    if (OnIntermediateResult != null)
                    {
                        OnIntermediateResult(this, new IntermediateResultEventArgs() { Result = result });
                    }
                }

            }

            return result;
        }

        /// <summary>
        /// Performs an hill-climbing approach to determine the ring settings
        /// </summary>
        /// <param name="enigmaConfig">A partial enigma configuration to be used</param>
        /// <param name="text">The ciphertext</param>
        private void analyzeRings(analysisConfigSettings enigmaConfig, string text)
        {
            _pluginFacade.UpdateDisplayEnd(Resources.RingsCaption);

            // get the current rotor positions
            int r1pos = _settings.Alphabet.IndexOf(enigmaConfig.InitialRotorPos[2]);
            int r2pos = _settings.Alphabet.IndexOf(enigmaConfig.InitialRotorPos[1]);
            int r3pos = _settings.Alphabet.IndexOf(enigmaConfig.InitialRotorPos[0]);

            if (_settings.AnalyzeInitialRotorPos)
            {
                // turn fast rotor
                for (int i = 1; i <= _settings.Alphabet.Length && !_stopped; i++)
                {
                    _core.setInternalConfig(enigmaConfig.Rotor1, enigmaConfig.Rotor2, enigmaConfig.Rotor3, 0, _settings.Reflector, i, 1, 1, 1, enigmaConfig.PlugBoard);


                    int rotatedR1Pos;
                    if (_settings.AnalyzeInitialRotorPos)
                        // rotate the fast rotor with the ring
                        rotatedR1Pos = (r1pos + (i - 1)) % _settings.Alphabet.Length;
                    else
                        rotatedR1Pos = r1pos;


                    string result = _core.Encrypt(rotatedR1Pos, r2pos, r3pos, 0, text);

                    double newScore = calculateScore(result, _settings.SearchMethod);

                    keys++;

                    if (newScore > enigmaConfig.Score)
                    {
                        //better value, hence update the data
                        enigmaConfig.Score = newScore;
                        enigmaConfig.Ring1 = i;
                        enigmaConfig.InitialRotorPos = _settings.Alphabet[r3pos].ToString() + _settings.Alphabet[r2pos].ToString() + _settings.Alphabet[rotatedR1Pos].ToString();

                        string key = String.Format("{0}, {1}, {2} / {3}, {4}, {5} / {6} / {7}",
                        (rotorEnum)enigmaConfig.Rotor3, (rotorEnum)enigmaConfig.Rotor2, (rotorEnum)enigmaConfig.Rotor1,
                        enigmaConfig.Ring3.ToString(), enigmaConfig.Ring2.ToString(), enigmaConfig.Ring1.ToString(),
                        _pluginFacade.pB2String(enigmaConfig.PlugBoard), enigmaConfig.InitialRotorPos);

                        AddNewBestListEntry(key, newScore, encrypt(enigmaConfig, text, enigmaConfig.PlugBoard));
                    }

                    // print keys/sec in the ui
                    UpdateKeysPerSec();
                }

                _pluginFacade.UpdateDisplayEnd(Resources.RingsCaption);

                // update the current rotor positions (only rotor 1 might have changed)
                r1pos = _settings.Alphabet.IndexOf(enigmaConfig.InitialRotorPos[2]);

                // turn middle rotor
                for (int i = 1; i <= _settings.Alphabet.Length && !_stopped; i++)
                {
                    _core.setInternalConfig(enigmaConfig.Rotor1, enigmaConfig.Rotor2, enigmaConfig.Rotor3, 0, _settings.Reflector, enigmaConfig.Ring1, i, 1, 1, enigmaConfig.PlugBoard);

                    int rotatedR2Pos;
                    if (_settings.AnalyzeInitialRotorPos)
                        // rotate the middle rotor with the ring
                        rotatedR2Pos = (r2pos + (i - 1)) % _settings.Alphabet.Length;
                    else
                        rotatedR2Pos = r2pos;

                    string result = _core.Encrypt(r1pos, rotatedR2Pos, r3pos, 0, text);

                    keys++;

                    double newScore = calculateScore(result, _settings.SearchMethod);

                    if (newScore > enigmaConfig.Score)
                    {
                        //better value, hence update the data
                        enigmaConfig.Score = newScore;
                        enigmaConfig.Ring2 = i;
                        enigmaConfig.InitialRotorPos = _settings.Alphabet[r3pos].ToString() + _settings.Alphabet[rotatedR2Pos].ToString() + _settings.Alphabet[r1pos].ToString();

                        string key = String.Format("{0}, {1}, {2} / {3}, {4}, {5} / {6} / {7}",
                        (rotorEnum)enigmaConfig.Rotor3, (rotorEnum)enigmaConfig.Rotor2, (rotorEnum)enigmaConfig.Rotor1,
                        enigmaConfig.Ring3.ToString(), enigmaConfig.Ring2.ToString(), enigmaConfig.Ring1.ToString(),
                        _pluginFacade.pB2String(enigmaConfig.PlugBoard), enigmaConfig.InitialRotorPos);

                        AddNewBestListEntry(key, newScore, encrypt(enigmaConfig, text, enigmaConfig.PlugBoard));
                    }

                    // print keys/sec in the ui
                    UpdateKeysPerSec();
                }

                _pluginFacade.UpdateDisplayEnd(Resources.RingsCaption);
            }
            else
            {
                // in case the key is fixed, we search all combinations, i.e. 26*26*26
                for (int i = 1; i <= _settings.Alphabet.Length && !_stopped; i++)
                {
                    for (int j = 1; j <= _settings.Alphabet.Length && !_stopped; j++)
                    {
                        for (int k = 1; k <= _settings.Alphabet.Length && !_stopped; k++)
                        {
                            _core.setInternalConfig(enigmaConfig.Rotor1, enigmaConfig.Rotor2, enigmaConfig.Rotor3, 0, _settings.Reflector, k, j, i, 1, enigmaConfig.PlugBoard);
                            string result = _core.Encrypt(r1pos, r2pos, r3pos, 0, text);
                            double newScore = calculateScore(result, _settings.SearchMethod);

                            keys++;

                            if (newScore > enigmaConfig.Score)
                            {
                                //better value, hence update the data
                                enigmaConfig.Score = newScore;
                                enigmaConfig.Ring1 = k;
                                enigmaConfig.Ring2 = j;
                                enigmaConfig.Ring2 = i;
                                enigmaConfig.InitialRotorPos = _settings.Alphabet[r3pos].ToString() + _settings.Alphabet[r2pos].ToString() + _settings.Alphabet[r1pos].ToString();

                                string key = String.Format("{0}, {1}, {2} / {3}, {4}, {5} / {6} / {7}",
                                (rotorEnum)enigmaConfig.Rotor3, (rotorEnum)enigmaConfig.Rotor2, (rotorEnum)enigmaConfig.Rotor1,
                                enigmaConfig.Ring3.ToString(), enigmaConfig.Ring2.ToString(), enigmaConfig.Ring1.ToString(),
                                _pluginFacade.pB2String(enigmaConfig.PlugBoard), enigmaConfig.InitialRotorPos);

                                AddNewBestListEntry(key, newScore, encrypt(enigmaConfig, text, enigmaConfig.PlugBoard));
                            }

                            // print keys/sec in the ui
                            UpdateKeysPerSec();
                        }

                        _pluginFacade.UpdateDisplayEnd(Resources.RingsCaption);
                    }
                }

                _pluginFacade.UpdateDisplayEnd(Resources.RingsCaption);
            }

            // print best option
            string msg = String.Format("ANALYSIS: Best ring setting: {0} | {1},{2},{3} | {4},{5},{6} | {7} | {8}",
                enigmaConfig.Score.ToString(),
                (rotorEnum)enigmaConfig.Rotor3, (rotorEnum)enigmaConfig.Rotor2, (rotorEnum)enigmaConfig.Rotor1,
                enigmaConfig.Ring3.ToString("00"), enigmaConfig.Ring2.ToString("00"), enigmaConfig.Ring1.ToString("00"),
                enigmaConfig.InitialRotorPos, _pluginFacade.pB2String(enigmaConfig.PlugBoard));
            //_pluginFacade.LogMessage(msg, NotificationLevel.Info);

        }

        /// <summary>
        /// Performs a hill-climbing approach to determine the plug-settings
        /// </summary>
        /// <param name="enigmaConfig">A partial enigma configuration to be used</param>
        /// <param name="maxPlugs">The maximum numer of plugs to be searched. Note that if no more improvement can be found the algortihm may terminate earlier</param>
        /// <param name="text">The cipertext</param>
        /// <returns>best decrypted result string</returns>
        private string analyzePlugs(analysisConfigSettings enigmaConfig, int maxPlugs, string text)
        {
            _pluginFacade.UpdateDisplayEnd(Resources.PlugsCaption);

            string tmp;
            bool plugFound = false;
            int trials = 0;
            int keys = 0;
            string bestResult = encrypt(enigmaConfig, text, enigmaConfig.PlugBoard);
            enigmaConfig.Score = calculateScore(bestResult, _settings.PlugSearchMethod);

            for (int n = 0; n < maxPlugs && !_stopped; n++)
            {

                //LogMessage(String.Format("ANALYSIS: ====> Stage 3.{0} - Searching plugs <====",(n+1)), NotificationLevel.Info);

                tmp = enigmaConfig.PlugBoard;
                plugFound = false;
                trials = 0; //reset the counter, so we count each round individually

                for (int i = 0; i < _settings.Alphabet.Length && !_stopped; i++)
                {
                    for (int j = i + 1; j < _settings.Alphabet.Length && !_stopped; j++)
                    {
                        //create a "clean" plugboard
                        StringBuilder plugboard = new StringBuilder(tmp);

                        //if both selected letters are pluged, ignore them
                        if (plugboard[i] != _settings.Alphabet[i] && plugboard[j] != _settings.Alphabet[j])
                            continue;

                        if (plugboard[i] != _settings.Alphabet[i])
                        {
                            plugFound = plugFound | resolvePlugConflict(i, j, enigmaConfig, plugboard.ToString(), text);
                            trials += 3;
                            keys += 3;
                            continue;
                        }

                        if (plugboard[j] != _settings.Alphabet[j])
                        {
                            plugFound = plugFound | resolvePlugConflict(j, i, enigmaConfig, plugboard.ToString(), text);
                            trials += 3;
                            keys += 3;
                            continue;
                        }

                        //swap i with j
                        plugboard[i] = _settings.Alphabet[j];
                        plugboard[j] = _settings.Alphabet[i];

                        string result = encrypt(enigmaConfig, text, plugboard.ToString());
                        double newScore = calculateScore(result, _settings.PlugSearchMethod);
                        trials++;
                        keys++;

                        if (newScore > enigmaConfig.Score)
                        {
                            enigmaConfig.Score = newScore;
                            enigmaConfig.PlugBoard = plugboard.ToString();
                            bestResult = result;
                            plugFound = true;

                            string key = String.Format("{0}, {1}, {2} / {3}, {4}, {5} / {6} / {7}",
                            (rotorEnum)enigmaConfig.Rotor3, (rotorEnum)enigmaConfig.Rotor2, (rotorEnum)enigmaConfig.Rotor1,
                            enigmaConfig.Ring3.ToString(), enigmaConfig.Ring2.ToString(), enigmaConfig.Ring1.ToString(),
                            _pluginFacade.pB2String(enigmaConfig.PlugBoard), enigmaConfig.InitialRotorPos);

                            // default is the IC Score (index of coincidence)
                            double GeneralSearchMethodScore = calculateScore(result, _settings.SearchMethod);

                            AddNewBestListEntry(key, GeneralSearchMethodScore, encrypt(enigmaConfig, text, enigmaConfig.PlugBoard));
                        }

                        // print keys/sec in the ui
                        UpdateKeysPerSec();
                    }

                    _pluginFacade.UpdateDisplayEnd(Resources.PlugsCaption);
                }

                _pluginFacade.UpdateDisplayEnd(Resources.PlugsCaption);

                string msg = String.Format("ANALYSIS: Plugs setting in round {0} after {1} trials: {2} | {3},{4},{5} | {6},{7},{8} | {9} | {10}",
                    (n + 1), trials, enigmaConfig.Score.ToString(),
                    (rotorEnum)enigmaConfig.Rotor3, (rotorEnum)enigmaConfig.Rotor2, (rotorEnum)enigmaConfig.Rotor1,
                    enigmaConfig.Ring3, enigmaConfig.Ring2, enigmaConfig.Ring1,
                    enigmaConfig.InitialRotorPos, _pluginFacade.pB2String(enigmaConfig.PlugBoard));
                //_pluginFacade.LogMessage(msg, NotificationLevel.Info);

                // no plug could lead to a better result, hence abort plug search.
                if (!plugFound || _stopped)
                    break;
            }

            return bestResult;
        }

        private string encrypt(analysisConfigSettings enigmaConfig, string text, string plugboard)
        {
            _core.setInternalConfig(enigmaConfig.Rotor1, enigmaConfig.Rotor2, enigmaConfig.Rotor3, 0, _settings.Reflector, enigmaConfig.Ring1, enigmaConfig.Ring2, enigmaConfig.Ring3, 1, plugboard);

            int r1pos = _settings.Alphabet.IndexOf(enigmaConfig.InitialRotorPos[2]);
            int r2pos = _settings.Alphabet.IndexOf(enigmaConfig.InitialRotorPos[1]);
            int r3pos = _settings.Alphabet.IndexOf(enigmaConfig.InitialRotorPos[0]);
            return _core.Encrypt(r1pos, r2pos, r3pos, 0, text);
        }

        /// <summary>
        /// Determines the best plug-configuration out of three possibilities.
        /// </summary>
        /// <param name="conflictLetterPos">A letter, which already has been plugged with some other letter</param>
        /// <param name="otherLetterPos">The letter we want to test</param>
        /// <param name="enigmaConfig">A partial enigma configuration to be used</param>
        /// <param name="unmodifiedPlugboard">A plugboard which should be used in this stage</param>
        /// <param name="text">The cipertext</param>
        /// <returns>Returns true, if a better plug-combination could be found, false otherwise</returns>
        private bool resolvePlugConflict(int conflictLetterPos, int otherLetterPos, analysisConfigSettings enigmaConfig, string unmodifiedPlugboard, string text)
        {
            bool found = false;

            int pluggedLetterPos = _settings.Alphabet.IndexOf(unmodifiedPlugboard[conflictLetterPos]);


            // plug otherLetter together with pluggedLetter and restore the coflictLetter
            StringBuilder o2pPlugPlugboard = new StringBuilder(unmodifiedPlugboard);
            o2pPlugPlugboard[conflictLetterPos] = _settings.Alphabet[conflictLetterPos]; // restore conflictLetter
            o2pPlugPlugboard[otherLetterPos] = _settings.Alphabet[pluggedLetterPos];     // swap other with
            o2pPlugPlugboard[pluggedLetterPos] = _settings.Alphabet[otherLetterPos];     // plugged


            // plug conflictLetter with otherLetter and restore pluggedLetter one
            StringBuilder c2oPlugboard = new StringBuilder(unmodifiedPlugboard);
            c2oPlugboard[pluggedLetterPos] = _settings.Alphabet[pluggedLetterPos]; // restore pluggedLetter
            c2oPlugboard[conflictLetterPos] = _settings.Alphabet[otherLetterPos];  // swap conflictLetter
            c2oPlugboard[otherLetterPos] = _settings.Alphabet[conflictLetterPos];  // with otherLetter


            // now we habe three different plug-posibilities and need to determine 
            // the best one, which remains set, hence we do 3 trial encryptions


            // get the current key
            int r1pos = _settings.Alphabet.IndexOf(enigmaConfig.InitialRotorPos[2]);
            int r2pos = _settings.Alphabet.IndexOf(enigmaConfig.InitialRotorPos[1]);
            int r3pos = _settings.Alphabet.IndexOf(enigmaConfig.InitialRotorPos[0]);


            // start with the unmodified
            _core.setInternalConfig(enigmaConfig.Rotor1, enigmaConfig.Rotor2, enigmaConfig.Rotor3, 0, _settings.Reflector, enigmaConfig.Ring1, enigmaConfig.Ring2, enigmaConfig.Ring3, 1, unmodifiedPlugboard);
            double unmodifiedScore = calculateScore(_core.Encrypt(r1pos, r2pos, r3pos, 0, text), _settings.PlugSearchMethod);

            // now o2p
            _core.setInternalConfig(enigmaConfig.Rotor1, enigmaConfig.Rotor2, enigmaConfig.Rotor3, 0, _settings.Reflector, enigmaConfig.Ring1, enigmaConfig.Ring2, enigmaConfig.Ring3, 1, o2pPlugPlugboard.ToString());
            double o2pScore = calculateScore(_core.Encrypt(r1pos, r2pos, r3pos, 0, text), _settings.PlugSearchMethod);

            // now c2o
            _core.setInternalConfig(enigmaConfig.Rotor1, enigmaConfig.Rotor2, enigmaConfig.Rotor3, 0, _settings.Reflector, enigmaConfig.Ring1, enigmaConfig.Ring2, enigmaConfig.Ring3, 1, c2oPlugboard.ToString());
            double c2oScore = calculateScore(_core.Encrypt(r1pos, r2pos, r3pos, 0, text), _settings.PlugSearchMethod);

            string bestPlugBoard = enigmaConfig.PlugBoard;
            double newScore;

            if (c2oScore > unmodifiedScore)
            {
                if (c2oScore > o2pScore)
                {
                    bestPlugBoard = c2oPlugboard.ToString();
                    newScore = c2oScore;
                }
                else
                {
                    bestPlugBoard = o2pPlugPlugboard.ToString();
                    newScore = o2pScore;
                }
            }
            else
            {
                if (unmodifiedScore > o2pScore)
                {
                    bestPlugBoard = unmodifiedPlugboard;
                    newScore = unmodifiedScore;
                }
                else
                {
                    bestPlugBoard = o2pPlugPlugboard.ToString();
                    newScore = o2pScore;
                }
            }


            if (newScore > enigmaConfig.Score)
            {
                enigmaConfig.Score = newScore;
                enigmaConfig.PlugBoard = bestPlugBoard;
                found = true;
            }

            //string msg = String.Format("ANALYSIS PLUG CONFLICT: Unmodified [{0}] => {1}; Variant A [{2}] => {3}; Variant B[{4}] => {5} || Selected [{6}]",
            //    pluginFacade.pB2String(unmodifiedPlugboard), unmodifiedScore,
            //    pluginFacade.pB2String(c2oPlugboard.ToString()), c2oScore,
            //    pluginFacade.pB2String(o2pPlugPlugboard.ToString()), o2pScore,
            //    pluginFacade.pB2String(bestPlugBoard));

            //pluginFacade.LogMessage(msg, NotificationLevel.Info);

            return found;
        }

        #region Helper methods

        /// <summary>
        /// This method calculates a trigram log2 score of a given text on the basis of a given grams dictionary.
        /// Case is insensitive.
        /// </summary>
        /// <param name="input">The text to be scored</param>
        /// <param name="length">n-gram length</param>
        /// <returns>The trigram score result</returns>
        private double calculateNGrams(string input, int length, int valueSelection)
        {
            double score = 0;
            IDictionary<string, double[]> corpusGrams = _pluginFacade.GetStatistics(length);

            // FIXME: case handling?

            HashSet<string> inputGrams = new HashSet<string>();

            foreach (string g in GramTokenizer.tokenize(input, length, false))
            {
                // ensure each n-gram is counted only once
                if (inputGrams.Add(g))
                {
                    if (corpusGrams.ContainsKey(g))
                    {
                        score += corpusGrams[g][valueSelection];
                    }
                }

                if (_stopped)
                    break;
            }

            return score;
        }

        private IDictionary<string, double[]> calculateConditional(string input, int length)
        {
            IDictionary<string, double[]> inputGrams = new Dictionary<string, double[]>();

            foreach (string g in GramTokenizer.tokenize(input, length, false))
            {
                if (inputGrams.ContainsKey(g))
                {
                    inputGrams[g][EnigmaBreaker.ABSOLUTE]++;
                }
                else
                {
                    inputGrams[g] = new double[] { 1, 0 };
                }

                if (_stopped)
                    break;
            }

            double sum = inputGrams.Values.Sum(item => item[EnigmaBreaker.ABSOLUTE]);
            foreach (double[] values in inputGrams.Values)
            {
                values[EnigmaBreaker.PERCENTAGED] = values[EnigmaBreaker.ABSOLUTE] / sum;
            }

            return inputGrams;
        }

        private double calculateSinkov(string input, int length)
        {
            double score = 0;
            IDictionary<string, double[]> corpusGrams = _pluginFacade.GetStatistics(length);
            IDictionary<string, double[]> inputGrams = calculateConditional(input, length);

            foreach (KeyValuePair<string, double[]> item in inputGrams)
            {
                if (corpusGrams.ContainsKey(item.Key))
                {
                    score += item.Value[EnigmaBreaker.PERCENTAGED] * corpusGrams[item.Key][EnigmaBreaker.SINKOV];
                }
            }

            return score;
        }

        private double calculateEntropy(string input)
        {
            double score = 0;
            IDictionary<string, double[]> inputGrams = calculateConditional(input, 1);

            foreach (double[] values in inputGrams.Values)
            {
                score += values[EnigmaBreaker.PERCENTAGED] * Math.Log(values[EnigmaBreaker.PERCENTAGED], 2);
            }

            return score;
        }

        /// <summary>
        /// A helper method, which returns true, if the rotor with the given
        /// number should be included in the analysis as determined
        /// by the analysis settings
        /// </summary>
        /// <param name="i">The rotor number as integer</param>
        /// <returns>True if this rotor should be included, false otherwise</returns>
        private bool includeRotor(int i)
        {
            switch (i)
            {
                case 0: return _settings.AnalysisUseRotorI;
                case 1: return _settings.AnalysisUseRotorII;
                case 2: return _settings.AnalysisUseRotorIII;
                case 3: return _settings.AnalysisUseRotorIV;
                case 4: return _settings.AnalysisUseRotorV;
                case 5: return _settings.AnalysisUseRotorVI;
                case 6: return _settings.AnalysisUseRotorVII;
                case 7: return _settings.AnalysisUseRotorVIII;
            }
            return false;
        }

        /// <summary>
        /// Prints on the CrypTool console the currently best options for rotor usage, rotor settings,
        /// ring settings, and plug settings
        /// </summary>
        private void printBestCandidates()
        {
            return;
            StringBuilder message = new StringBuilder("  -- Analysis results --" + Environment.NewLine);
            StringBuilder currentBestKey = new StringBuilder();

            message.AppendLine("=====================================");

            foreach (analysisConfigSettings cfg in analysisCandidates)
            {
                message.AppendFormat("{0} | {1},{2},{3} | {4},{5},{6} | {7} | {8}" + Environment.NewLine,
                    cfg.Score.ToString(),
                    (rotorEnum)cfg.Rotor3, (rotorEnum)cfg.Rotor2, (rotorEnum)cfg.Rotor1,
                    cfg.Ring3.ToString("00"), cfg.Ring2.ToString("00"), cfg.Ring1.ToString("00"),
                    _pluginFacade.pB2String(cfg.PlugBoard), cfg.InitialRotorPos);

                // TODO: take out of foreach loop, run only once at the end of an analysis
                //      also: sort scores and take key of highest score
                currentBestKey = new StringBuilder();
                currentBestKey.AppendFormat("{0}, {1}, {2} / {3}, {4}, {5} / {6} / {7}",
                    (rotorEnum)cfg.Rotor3, (rotorEnum)cfg.Rotor2, (rotorEnum)cfg.Rotor1,
                    cfg.Ring3.ToString(), cfg.Ring2.ToString(), cfg.Ring1.ToString(),
                    _pluginFacade.pB2String(cfg.PlugBoard), cfg.InitialRotorPos);
            }
            //pluginFacade.BestKey = message.ToString();
            _pluginFacade.SetBestKey(currentBestKey.ToString());
            //pluginFacade.LogMessage(message.ToString(), NotificationLevel.Info);
        }

        #endregion

        #endregion

        #region Properties
        #endregion

        #region Public events

        public event EventHandler<IntermediateResultEventArgs> OnIntermediateResult;

        #endregion

        #region Public methods


        /// <summary>
        /// Creates a new EnigmaAnalyzer class
        /// This analyzer performs a ciphertext-only analysis of a given enigma text.
        /// It uses the method as described by James Gillogly in Cryptologia 1995.
        /// </summary>
        /// <param name="facade">The enimga plugin which controls this analyzer</param>
        public EnigmaAnalyzer(EnigmaBreaker facade)
        {
            _pluginFacade = facade;
            _settings = (EnigmaBreakerSettings)_pluginFacade.Settings;
            _core = new EnigmaCore(facade);
            _presentation = (AssignmentPresentation) _pluginFacade.Presentation;
        }

        /// <summary>
        /// This method performs the entire analysis and returns the cleartext as detected by the algorithm.
        /// The provided string should be preformatted, i.e. should not contain uncecessary characters;
        /// unknown characters in the input will slow down the analysis process.
        /// </summary>
        /// <param name="preformatedText">The preformated text for analysis</param>
        /// <returns>The cleartext as decoded with the analyzed key</returns>
        public IEnumerable<string> Analyze(string preformatedText)
        {
            _pluginFacade.LogMessage("=========> ANALYSIS OF ENIGMA MESSAGE STARTED <=========", NotificationLevel.Info);

            // some initialisation
            analysisCandidates.Clear();
            _stopped = false;

            keys = 0;
            lasttime = DateTime.Now;

            AnalyzeRun(preformatedText);

            _pluginFacade.LogMessage("=========> ANALYSIS OF ENIGMA MESSAGE DONE <=========", NotificationLevel.Info);

            // switch back to verbose core
            _core.VerboseLevel = VerboseLevels.VeryVerbose;

            // decrypt all
            return analysisCandidates.Select(cfg => encrypt(cfg, preformatedText, cfg.PlugBoard));
        }

        private void AnalyzeRun(string preformatedText)
        {
            if (_settings.AnalyzeRotors)
            {
                _pluginFacade.LogMessage("ANALYSIS: ====> Stage 1 - Searching used rotors and key <====", NotificationLevel.Info);
                
                rotorPos1 = _settings.Alphabet.IndexOf(_settings.InitialRotorPos[0]);
                rotorPos2 = _settings.Alphabet.IndexOf(_settings.InitialRotorPos[1]);
                rotorPos3 = _settings.Alphabet.IndexOf(_settings.InitialRotorPos[2]);

                analyzeRotors(preformatedText);
            }
            else
            {
                _pluginFacade.LogMessage("ANALYSIS: ====> Stage 1 - Searching key (with fixed rotors) <====", NotificationLevel.Info);
                if (_settings.AnalyzeInitialRotorPos)
                {
                    _core.setInternalConfig(_settings.Rotor1, _settings.Rotor2, _settings.Rotor3, _settings.Rotor4, _settings.Reflector,
                                    _settings.AnalyzeRings ? 1 : _settings.Ring1,
                                    _settings.AnalyzeRings ? 1 : _settings.Ring2,
                                    _settings.AnalyzeRings ? 1 : _settings.Ring3,
                                    _settings.Ring4,
                                    _settings.AnalyzePlugs ? _settings.Alphabet : _settings.PlugBoard
                        );
                    analyzeInitialRotorPos(preformatedText);
                }
                else
                {
                    _pluginFacade.LogMessage("ANALYSIS: ====> Skipping stage 1 - Using rotors/key from settings <====", NotificationLevel.Info);
                    analysisConfigSettings settingsConfig = new analysisConfigSettings();
                    settingsConfig.Rotor1 = _settings.Rotor1;
                    settingsConfig.Rotor2 = _settings.Rotor2;
                    settingsConfig.Rotor3 = _settings.Rotor3;
                    settingsConfig.Ring1 = _settings.AnalyzeRings ? 1 : _settings.Ring1;
                    settingsConfig.Ring2 = _settings.AnalyzeRings ? 1 : _settings.Ring2;
                    settingsConfig.Ring3 = _settings.AnalyzeRings ? 1 : _settings.Ring3;
                    settingsConfig.InitialRotorPos = _settings.InitialRotorPos;

                    analysisCandidates.Add(settingsConfig);
                }
            }

            // just for debugging
            //analysisCandidates[analysisCandidates.Count-1].Rotor1 = 2;
            //analysisCandidates[analysisCandidates.Count - 1].Rotor2 = 0;
            //analysisCandidates[analysisCandidates.Count - 1].Rotor3 = 1;
            //analysisCandidates[analysisCandidates.Count - 1].Key = "BKF";

            printBestCandidates();

            if (_stopped)
                return;

            // put the core in quiet mode, since now many internal changes occur
            _core.VerboseLevel = VerboseLevels.Quiet;

            if (_settings.AnalyzeRings)
            {
                _pluginFacade.LogMessage("ANALYSIS: ====> Stage 2 - Searching ring positions <====", NotificationLevel.Info);

                for (int j = analysisCandidates.Count - 1; j >= 0 && !_stopped; j--)
                {
                    analysisCandidates[j].PlugBoard = _settings.Alphabet; // empty plugs
                    analyzeRings(analysisCandidates[j], preformatedText);
                }

                analysisCandidates.Sort();
            }
            else
            {
                _pluginFacade.LogMessage("ANALYSIS: ====> Skipping stage 2 - Using provided ring settings <====", NotificationLevel.Info);

                for (int j = analysisCandidates.Count - 1; j >= 0 && !_stopped; j--)
                {
                    analysisCandidates[j].PlugBoard = _settings.Alphabet; // empty plugs
                    analysisCandidates[j].Ring1 = _settings.Ring1;
                    analysisCandidates[j].Ring2 = _settings.Ring2;
                    analysisCandidates[j].Ring3 = _settings.Ring3;
                }
            }

            printBestCandidates();

            if (_stopped)
                return;

            if (_settings.AnalyzePlugs)
            {
                _pluginFacade.LogMessage("ANALYSIS: ====> Stage 3 - Searching used plugs <====", NotificationLevel.Info);

                for (int j = analysisCandidates.Count - 1; j >= 0 && !_stopped; j--)
                {
                    string result = analyzePlugs(analysisCandidates[j], _settings.MaxSearchedPlugs, preformatedText);

                    // fire the event, so someting becomes visible..
                    if (OnIntermediateResult != null)
                    {
                        OnIntermediateResult(this, new IntermediateResultEventArgs() { Result = result });
                    }
                }

                analysisCandidates.Sort();
            }
            else
            {
                _pluginFacade.LogMessage("ANALYSIS: ====> Skipping stage 3 - Using provided plugboard <====", NotificationLevel.Info);

                for (int j = analysisCandidates.Count - 1; j >= 0 && !_stopped; j--)
                {
                    analysisCandidates[j].PlugBoard = _settings.PlugBoard;
                }
            }

            printBestCandidates();
        }

        /// <summary>
        /// Stops a currently _running analysis and outputs the result so far
        /// </summary>
        public void StopAnalysis()
        {
            _stopped = true;
        }

        /// <summary>
        /// The method calculates and returns the index of coincidences of a given text
        /// </summary>
        /// <param name="input">The index if coincidences of this text will be calculated</param>
        /// <returns>The index of coincidences</returns>
        public double calculateIC(string input)
        {
            int[] statistics = new int[_settings.Alphabet.Length];
            long cipherTextLength = 0; //input.Length; //n
            long countDoubleCharacters = 0;

            // first count the frequency of (single) letters
            foreach (char c in input)
            {
                int i = _settings.Alphabet.IndexOf(char.ToUpper(c));
                if (i >= 0) statistics[i]++;
            }


            // now calculate the index of coincidences
            for (int i = 0; i < statistics.Length && !_stopped; i++)
            {
                cipherTextLength += statistics[i];
                countDoubleCharacters += (statistics[i] * (statistics[i] - 1));
            }

            return ((double)countDoubleCharacters / (double)(cipherTextLength * (cipherTextLength - 1)));
        }

        public double calculateScore(string input, int searchMethod)
        {
            switch (searchMethod)
            {
                case 0:
                    return calculateIC(input);
                case 1:
                    return calculateNGrams(input, 2, EnigmaBreaker.LOG2);
                case 2:
                    return calculateNGrams(input, 3, EnigmaBreaker.LOG2);
                case 3:
                    return calculateSinkov(input, 1);
                case 4:
                    return calculateSinkov(input, 2);
                case 5:
                    return calculateEntropy(input);
                default:
                    throw new NotSupportedException("Search method not supported");
            }
        }

        #endregion


    }
}
