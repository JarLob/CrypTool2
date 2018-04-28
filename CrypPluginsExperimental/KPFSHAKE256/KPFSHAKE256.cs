/*
   Copyright 2011 CrypTool 2 Team <ct2contact@cryptool.org>

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
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using System.Numerics;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using KPFSHAKE256.Properties;
using Org.BouncyCastle.Crypto.Digests;


namespace Cryptool.Plugins.KPFSHAKE256
{
    // HOWTO: Change author name, email address, organization and URL.
    [Author("Christian Bender", "christian1.bender@student.uni-siegen.de", null, "http://www.uni-siegen.de")]
    // HOWTO: Change plugin caption (title to appear in CT2) and tooltip.
    // You can (and should) provide a user documentation as XML file and an own icon.
    [PluginInfo("KPFSHAKE256.Properties.Resources", "PluginCaption", "KPFSHAKE256Tooltip", "KPFSHAKE256/userdoc.xml", new[] { "KPFSHAKE256/images/icon.png" })]
    // HOWTO: Change category to one that fits to your plugin. Multiple categories are allowed.
    [ComponentCategory(ComponentCategory.HashFunctions)]
    public class KPFSHAKE256 : ICrypComponent
    {
        #region Private Variables

        // HOWTO: You need to adapt the settings class as well, see the corresponding file.
        private readonly KPFSHAKE256Settings settings = new KPFSHAKE256Settings();
        private KPFSHAKE256Pres pres = new KPFSHAKE256Pres();
        private byte[] _skm;
        private byte[] _key;
        private BigInteger _outputBytes;
        private byte[] _keyMaterial;
        private Thread workerThread;
        private int stepsToGo = 0;
        private int curStep = 0;

        #endregion

        #region Methods for calculation

        /// <summary>
        /// Computes the KPFSHAKE256 function. Its like the traditional KDF described in https://eprint.iacr.org/2010/264.pdf This construction does not need a counter.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="key"></param>
        /// <param name="outputBytes"></param>
        /// <returns></returns>
        static byte[] computeKPFSHA256XOF(byte[] msg, byte[] key, int outputBytes)
        {
            //hash object
            var shake256 = new ShakeDigest(256);
            //output byte array
            byte[] result = new byte[outputBytes];
            //array for input of hashfunction
            byte[] input = new byte[key.Length + msg.Length];

            //prepare input
            System.Buffer.BlockCopy(key, 0, input, 0, key.Length);
            System.Buffer.BlockCopy(msg, 0, input, key.Length, msg.Length);

            //update internal state
            shake256.BlockUpdate(input, 0, input.Length);
            //finish the hash.
            shake256.DoFinal(result, 0, outputBytes);

            //DEBUG
            //Console.WriteLine("KM: " + BitConverter.ToString(result).Replace("-", ""));
            return result;
        }

        /// <summary>
        /// Method for refreshing the stepcounter in the presentation
        /// </summary>
        private void refreshStepState()
        {
            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                Paragraph p = new Paragraph();

                //headline of lblExplanationSectionHeading
                p.Inlines.Add(new Run(Resources.PresStepText.Replace("{0}", curStep.ToString()).Replace("{1}", stepsToGo.ToString())));
                p.TextAlignment = TextAlignment.Right;
                pres.txtStep.Document.Blocks.Add(p);
                pres.txtStep.Document.Blocks.Remove(pres.txtStep.Document.Blocks.FirstBlock);
            }, null);
        }

        /// <summary>
        ///  the method to be called in the workerthread
        /// </summary>
        private void tExecute()
        {
            //Label for restart
            Restart:

            //Progessbar adjusting
            ProgressChanged(0, 1);

            //Clean up outputs
            _keyMaterial = Encoding.UTF8.GetBytes("");
            OnPropertyChanged("KeyMaterial");

            //clean steps
            curStep = 0;

            //Check for output length: max 5.242.880 byte = 5 Mb
            if (OutputBytes > 5242880)
            {
                GuiLogMessage(Resources.TooMuchOutputRequestedLogMSG.Replace("{0}", OutputBytes.ToString()), NotificationLevel.Warning);
                OutputBytes = 5242880;
                OnPropertyChanged("OutputBytes");
            }

            //ten stepts takes the presentation
            double steps = 10;
            stepsToGo = (int)steps;
            double prgress_step = ((double)1) / steps;
            double val = 0;

            refreshStepState();

            //Event for start
            AutoResetEvent buttonStartClickedEvent = pres.buttonStartClickedEvent;
            buttonStartClickedEvent = pres.buttonStartClickedEvent;
            buttonStartClickedEvent.Reset();

            //clean up for starting
            pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
            {
                pres.spStartRestartButtons.Visibility = Visibility.Visible;
                pres.buttonStart.IsEnabled = true;
                pres.buttonRestart.IsEnabled = false;

                //Remarks to the inputs and outputs
                pres.lblExplanationSectionHeading.Visibility = Visibility.Visible;
                pres.txtExplanationSectionText.Visibility = Visibility.Visible;

                //Last
                pres.lblFinishedSectionHeading.Visibility = Visibility.Hidden;
                pres.txtFinished.Visibility = Visibility.Hidden;

                //progress counter
                pres.txtStep.Visibility = Visibility.Visible;

                //Title of Presentation
                pres.lblTitleHeading.Visibility = Visibility.Hidden;
                
                //Introduction
                pres.lblIntroductionSectionHeading.Visibility = Visibility.Hidden;
                pres.lblIntroductionHeading.Visibility = Visibility.Hidden;
                pres.txtIntroductionText.Visibility = Visibility.Hidden;

                //Construction
                pres.lblConstructionSectionHeading.Visibility = Visibility.Hidden;
                pres.lblConstructionHeading.Visibility = Visibility.Hidden;
                pres.txtConstructionScheme.Visibility = Visibility.Hidden;
                pres.txtConstructionText1.Visibility = Visibility.Hidden;
                pres.txtConstructionText2.Visibility = Visibility.Hidden;
                pres.txtConstructionText3.Visibility = Visibility.Hidden;
                pres.txtConstructionText4.Visibility = Visibility.Hidden;
                pres.imgConstructionSponge.Visibility = Visibility.Hidden;

                //Calculation
                pres.lblCalculationSectionHeading.Visibility = Visibility.Hidden;
                pres.imgCalculation.Visibility = Visibility.Hidden;
                pres.txtCalculationRounds.Visibility = Visibility.Hidden;
                pres.lblCalculationHeading.Visibility = Visibility.Hidden;
                
                //Calculation finished
                pres.lblCalculationSectionHeading.Visibility = Visibility.Hidden;

                //Last
                pres.lblFinishedSectionHeading.Visibility = Visibility.Hidden;
                pres.txtFinished.Visibility = Visibility.Hidden;

                //Error
                pres.txtError.Visibility = Visibility.Hidden;

                //Buttons
                pres.spButtons.Visibility = Visibility.Hidden;
                pres.buttonSkipIntro.IsEnabled = false;
                pres.buttonSkipCalc.IsEnabled = false;
                pres.buttonNext.IsEnabled = false;
                pres.SkipChapter = false;

            }, null);

            buttonStartClickedEvent = pres.buttonStartClickedEvent;
            buttonStartClickedEvent.WaitOne();

            AutoResetEvent buttonNextClickedEvent = pres.buttonNextClickedEvent;
            buttonNextClickedEvent = pres.buttonNextClickedEvent;
            buttonNextClickedEvent.Reset();

            //Check if presentation shall be displayed
            if (settings.DisplayPres)
            {
                //clean up last round
                pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                {
                    //Remarks to the inputs and outputs
                    pres.lblExplanationSectionHeading.Visibility = Visibility.Hidden;
                    pres.txtExplanationSectionText.Visibility = Visibility.Hidden;

                    //Title of Presentation
                    pres.lblTitleHeading.Visibility = Visibility.Visible;

                    //Introduction
                    pres.lblIntroductionSectionHeading.Visibility = Visibility.Hidden;
                    pres.lblIntroductionHeading.Visibility = Visibility.Hidden;
                    pres.txtIntroductionText.Visibility = Visibility.Hidden;

                    //Construction
                    pres.lblConstructionSectionHeading.Visibility = Visibility.Hidden;
                    pres.lblConstructionHeading.Visibility = Visibility.Hidden;
                    pres.txtConstructionScheme.Visibility = Visibility.Hidden;
                    pres.txtConstructionText1.Visibility = Visibility.Hidden;
                    pres.txtConstructionText2.Visibility = Visibility.Hidden;
                    pres.txtConstructionText3.Visibility = Visibility.Hidden;
                    pres.txtConstructionText4.Visibility = Visibility.Hidden;
                    pres.imgConstructionSponge.Visibility = Visibility.Hidden;

                    //Calculation
                    pres.lblCalculationSectionHeading.Visibility = Visibility.Hidden;
                    pres.imgCalculation.Visibility = Visibility.Hidden;
                    pres.txtCalculationRounds.Visibility = Visibility.Hidden;
                    pres.lblCalculationHeading.Visibility = Visibility.Hidden;

                    //Last
                    pres.lblFinishedSectionHeading.Visibility = Visibility.Hidden;
                    pres.txtFinished.Visibility = Visibility.Hidden;

                    //Error
                    pres.txtError.Visibility = Visibility.Hidden;

                    //Buttons
                    pres.spStartRestartButtons.Visibility = Visibility.Hidden;
                    pres.buttonStart.IsEnabled = false;
                    pres.buttonRestart.IsEnabled = false;
                    pres.spButtons.Visibility = Visibility.Visible;
                    pres.buttonSkipIntro.IsEnabled = false;
                    pres.buttonSkipCalc.IsEnabled = false;
                    pres.buttonNext.IsEnabled = false;
                    pres.SkipChapter = false;

                }, null);

                if (!pres.SkipChapter)
                {
                    //Enable buttons
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.buttonNext.IsEnabled = true;
                        pres.buttonSkipIntro.IsEnabled = true;

                    }, null);

                    buttonNextClickedEvent = pres.buttonNextClickedEvent;
                    buttonNextClickedEvent.WaitOne();
                }

                val += prgress_step;
                ProgressChanged(val, 1);
                curStep++;
                refreshStepState();

                pres.SkipChapter = false;

                //Block: Introduction section heading  
                if (!pres.SkipChapter)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.lblTitleHeading.Visibility = Visibility.Hidden;
                        pres.lblIntroductionSectionHeading.Visibility = Visibility.Visible;

                    }, null);

                    buttonNextClickedEvent = pres.buttonNextClickedEvent;
                    buttonNextClickedEvent.WaitOne();
                }

                val += prgress_step;
                ProgressChanged(val, 1);
                curStep++;
                refreshStepState();
                
                //Block: Introduction section
                if (!pres.SkipChapter)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.lblIntroductionSectionHeading.Visibility = Visibility.Hidden;
                        pres.lblIntroductionHeading.Visibility = Visibility.Visible;
                        pres.txtIntroductionText.Visibility = Visibility.Visible;

                    }, null);

                    buttonNextClickedEvent = pres.buttonNextClickedEvent;
                    buttonNextClickedEvent.WaitOne();
                }

                val += prgress_step;
                ProgressChanged(val, 1);
                curStep++;
                refreshStepState();

                pres.SkipChapter = false;

                //Block: Construction section heading
                if (!pres.SkipChapter)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.lblIntroductionSectionHeading.Visibility = Visibility.Hidden;
                        pres.lblTitleHeading.Visibility = Visibility.Hidden;
                        pres.lblIntroductionHeading.Visibility = Visibility.Hidden;
                        pres.txtIntroductionText.Visibility = Visibility.Hidden;
                        pres.lblConstructionSectionHeading.Visibility = Visibility.Visible;

                    }, null);

                    buttonNextClickedEvent = pres.buttonNextClickedEvent;
                    buttonNextClickedEvent.WaitOne();
                }

                val += prgress_step;
                ProgressChanged(val, 1);
                curStep++;
                refreshStepState();

                //Block: Construction section part 1
                if (!pres.SkipChapter)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.lblConstructionSectionHeading.Visibility = Visibility.Hidden;
                        pres.lblConstructionHeading.Visibility = Visibility.Visible;
                        pres.txtConstructionText1.Visibility = Visibility.Visible;
                        pres.txtConstructionScheme.Visibility = Visibility.Visible;

                    }, null);

                    buttonNextClickedEvent = pres.buttonNextClickedEvent;
                    buttonNextClickedEvent.WaitOne();
                }

                val += prgress_step;
                ProgressChanged(val, 1);
                curStep++;
                refreshStepState();

                //Block: Construction section part 2
                if (!pres.SkipChapter)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.lblConstructionSectionHeading.Visibility = Visibility.Hidden;
                        pres.lblConstructionHeading.Visibility = Visibility.Visible;
                        pres.txtConstructionText1.Visibility = Visibility.Visible;
                        pres.txtConstructionText2.Visibility = Visibility.Visible;
                        pres.txtConstructionScheme.Visibility = Visibility.Visible;


                    }, null);

                    buttonNextClickedEvent = pres.buttonNextClickedEvent;
                    buttonNextClickedEvent.WaitOne();
                }

                val += prgress_step;
                ProgressChanged(val, 1);
                curStep++;
                refreshStepState();

                //Block: Construction section part 3
                if (!pres.SkipChapter)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.lblConstructionSectionHeading.Visibility = Visibility.Hidden;
                        pres.txtConstructionScheme.Visibility = Visibility.Hidden;
                        pres.lblConstructionHeading.Visibility = Visibility.Visible;
                        pres.txtConstructionText1.Visibility = Visibility.Hidden;
                        pres.txtConstructionText2.Visibility = Visibility.Hidden;
                        pres.txtConstructionText3.Visibility = Visibility.Visible;
                        pres.imgConstructionSponge.Visibility = Visibility.Visible;


                    }, null);

                    buttonNextClickedEvent = pres.buttonNextClickedEvent;
                    buttonNextClickedEvent.WaitOne();
                }

                val += prgress_step;
                ProgressChanged(val, 1);
                curStep++;
                refreshStepState();

                //Block: Construction section part 4
                if (!pres.SkipChapter)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.lblConstructionSectionHeading.Visibility = Visibility.Hidden;
                        pres.txtConstructionScheme.Visibility = Visibility.Hidden;
                        pres.lblConstructionHeading.Visibility = Visibility.Visible;
                        pres.txtConstructionText1.Visibility = Visibility.Hidden;
                        pres.txtConstructionText2.Visibility = Visibility.Hidden;
                        pres.txtConstructionText3.Visibility = Visibility.Hidden;
                        pres.txtConstructionText4.Visibility = Visibility.Visible;
                        pres.imgConstructionSponge.Visibility = Visibility.Visible;


                    }, null);

                    buttonNextClickedEvent = pres.buttonNextClickedEvent;
                    buttonNextClickedEvent.WaitOne();
                }

                val += prgress_step;
                ProgressChanged(val, 1);
                curStep++;
                refreshStepState();

                pres.SkipChapter = false;

                //Block: Calculation section heading  
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {

                    pres.SkipChapter = false;

                    //Title of Presentation
                    pres.lblTitleHeading.Visibility = Visibility.Hidden;
                    //Introduction
                    pres.lblIntroductionSectionHeading.Visibility = Visibility.Hidden;
                    pres.lblIntroductionHeading.Visibility = Visibility.Hidden;
                    pres.txtIntroductionText.Visibility = Visibility.Hidden;

                    //Construction
                    pres.lblConstructionSectionHeading.Visibility = Visibility.Hidden;
                    pres.lblConstructionHeading.Visibility = Visibility.Hidden;
                    pres.txtConstructionScheme.Visibility = Visibility.Hidden;
                    pres.txtConstructionText1.Visibility = Visibility.Hidden;
                    pres.txtConstructionText2.Visibility = Visibility.Hidden;
                    pres.txtConstructionText3.Visibility = Visibility.Hidden;
                    pres.txtConstructionText4.Visibility = Visibility.Hidden;
                    pres.imgConstructionSponge.Visibility = Visibility.Hidden;

                    //Calculation
                    pres.lblCalculationSectionHeading.Visibility = Visibility.Visible;

                    //Buttons
                    pres.buttonSkipIntro.IsEnabled = false;
                    pres.buttonSkipCalc.IsEnabled = true;
                    
                }, null);

                buttonNextClickedEvent = pres.buttonNextClickedEvent;
                buttonNextClickedEvent.WaitOne();

                //Block: Iteration section 
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {

                    pres.lblCalculationSectionHeading.Visibility = Visibility.Hidden;
                    pres.lblCalculationHeading.Visibility = Visibility.Visible;
                    pres.txtCalculationRounds.Visibility = Visibility.Visible;
                    pres.imgCalculation.Visibility = Visibility.Visible;

                    Paragraph p = new Paragraph();
                    p.Inlines.Add(new Run(Resources.PresCalculationText.Replace("{1}", System.Text.Encoding.UTF8.GetString(_skm)).Replace("{2}", System.Text.Encoding.UTF8.GetString(_key))));
                    p.TextAlignment = TextAlignment.Left;
                    pres.txtCalculationRounds.Document.Blocks.Clear();
                    pres.txtCalculationRounds.Document.Blocks.Add(p);

                }, null);

                val += prgress_step;
                ProgressChanged(val, 1);
                curStep++;
                refreshStepState();
            }
            else
            {
                val += prgress_step * 9;
                ProgressChanged(val, 1);
                curStep = 9;
                refreshStepState();
            }

            try
            {

                byte[] result = computeKPFSHA256XOF(_skm, _key, (int)_outputBytes);

                if (result == null)
                {
                    return;
                }

                //Save to file if configured
                if (settings.SaveToFile && !string.IsNullOrEmpty(settings.FilePath))
                {
                    System.IO.StreamWriter file = new System.IO.StreamWriter(settings.FilePath);
                    int i = 0;
                    foreach (byte b in result)
                    {
                        if (i % 31 == 0)
                        {
                            file.Write("\n");
                        }
                        file.Write(b.ToString("X2"));
                        i++;
                    }
                    file.Close();
                }

                if (!pres.SkipChapter && settings.DisplayPres)
                {
                    buttonNextClickedEvent = pres.buttonNextClickedEvent;
                    buttonNextClickedEvent.WaitOne();
                }

                _keyMaterial = result;
                OnPropertyChanged("KeyMaterial");

            }
            catch (System.OutOfMemoryException ex)
            {
                GuiLogMessage(ex.Message + " " + Resources.ExSystemOutOfMemory, NotificationLevel.Error);
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.lblCalculationSectionHeading.Visibility = Visibility.Hidden;
                    pres.lblCalculationHeading.Visibility = Visibility.Hidden;
                    pres.txtCalculationRounds.Visibility = Visibility.Hidden;
                    pres.imgCalculation.Visibility = Visibility.Hidden;
                    pres.txtError.Visibility = Visibility.Visible;

                }, null);
                return;
            }

            /*
            if (settings.DisplayPres)
            {
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.lblFinishedSectionHeading.Visibility = Visibility.Visible;
                    pres.txtFinished.Visibility = Visibility.Visible;

                    pres.buttonNext.IsEnabled = false;
                    pres.buttonSkipCalc.IsEnabled = false;
                    pres.lblCalculationHeading.Visibility = Visibility.Hidden;
                    pres.txtCalculationRounds.Visibility = Visibility.Hidden;

                    pres.imgCalculation.Visibility = Visibility.Hidden;
                }, null);
            }
            */

            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {

                //Remarks to the inputs and outputs
                pres.lblExplanationSectionHeading.Visibility = Visibility.Hidden;
                pres.txtExplanationSectionText.Visibility = Visibility.Hidden;

                //Title of Presentation
                pres.lblTitleHeading.Visibility = Visibility.Hidden;

                //Introduction
                pres.lblIntroductionSectionHeading.Visibility = Visibility.Hidden;
                pres.lblIntroductionHeading.Visibility = Visibility.Hidden;
                pres.txtIntroductionText.Visibility = Visibility.Hidden;

                //Construction
                pres.lblConstructionSectionHeading.Visibility = Visibility.Hidden;
                pres.lblConstructionHeading.Visibility = Visibility.Hidden;
                pres.txtConstructionScheme.Visibility = Visibility.Hidden;
                pres.txtConstructionText1.Visibility = Visibility.Hidden;
                pres.txtConstructionText2.Visibility = Visibility.Hidden;
                pres.txtConstructionText3.Visibility = Visibility.Hidden;
                pres.txtConstructionText4.Visibility = Visibility.Hidden;
                pres.imgConstructionSponge.Visibility = Visibility.Hidden;

                //Calculation
                pres.lblCalculationSectionHeading.Visibility = Visibility.Hidden;
                pres.imgCalculation.Visibility = Visibility.Hidden;
                pres.txtCalculationRounds.Visibility = Visibility.Hidden;
                pres.lblCalculationHeading.Visibility = Visibility.Hidden;

                //Last
                pres.lblFinishedSectionHeading.Visibility = Visibility.Visible;
                pres.txtFinished.Visibility = Visibility.Visible;

                //Error
                pres.txtError.Visibility = Visibility.Hidden;

                //Buttons
                pres.spStartRestartButtons.Visibility = Visibility.Hidden;
                pres.buttonStart.IsEnabled = false;
                pres.buttonRestart.IsEnabled = false;
                pres.spButtons.Visibility = Visibility.Visible;
                pres.buttonSkipIntro.IsEnabled = false;
                pres.buttonSkipCalc.IsEnabled = false;
                pres.buttonNext.IsEnabled = false;
                pres.SkipChapter = false;

            }, null);

            //Progessbar adjusting
            val += prgress_step;
            ProgressChanged(val, 1);
            curStep++;
            refreshStepState();

            //show elements for restart the system
            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                pres.spStartRestartButtons.Visibility = Visibility.Visible;
                pres.buttonStart.IsEnabled = false;
                pres.buttonRestart.IsEnabled = true;

                pres.spButtons.Visibility = Visibility.Hidden;
            }, null);

            AutoResetEvent buttonRestartClickedEvent = pres.buttonRestartClickedEvent;
            buttonRestartClickedEvent = pres.buttonRestartClickedEvent;
            buttonRestartClickedEvent.Reset();

            buttonRestartClickedEvent = pres.buttonRestartClickedEvent;
            buttonRestartClickedEvent.WaitOne();

            if (pres.Restart)
            {
                goto Restart;
            }
        }

        #endregion

        #region Data Properties

        /// <summary>
        /// Input for source key material
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputSKMCaption", "InputSKMToolTip", true)]
        public byte[] SKM
        {
            get
            {
                return _skm;
            }
            set
            {
                _skm = value;
            }
        }

        /// <summary>
        /// Input for the key
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputKeyCaption", "InputKeyToolTip", true)]
        public byte[] Key
        {
            get
            {
                return _key;
            }
            set
            {
                _key = value;
            }
        }

        /// <summary>
        /// Input for outputlength
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputOutputLengthCaption", "InputOutputLengthToolTip", true)]
        public BigInteger OutputBytes
        {
            get
            {
                return _outputBytes;
            }
            set
            {
                _outputBytes = value;
            }
        }

        /// <summary>
        /// Output for key material
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputKeyMaterialCaption", "OutputKeyMaterialToolTip")]
        public byte[] KeyMaterial
        {
            get
            {
                return _keyMaterial;
            }
            set
            {
                _keyMaterial = value;
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
            get { return pres; }
        }

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            //Implementation with threads: this approach handles an inputchange in a better way
            if (workerThread == null)
            {
                workerThread = new Thread(new ThreadStart(tExecute));
                workerThread.IsBackground = true;
                workerThread.Start();
            }
            else
            {
                if (workerThread.IsAlive)
                {
                    workerThread.Abort();
                    workerThread = new Thread(new ThreadStart(tExecute));
                    workerThread.IsBackground = true;
                    workerThread.Start();
                }
                else
                {
                    workerThread = new Thread(new ThreadStart(tExecute));
                    workerThread.IsBackground = true;
                    workerThread.Start();
                }
            }
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
            if (workerThread.IsAlive)
            {
                workerThread.Abort();
            }

            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {

                AutoResetEvent buttonNextClickedEvent = pres.buttonNextClickedEvent;
                buttonNextClickedEvent = pres.buttonNextClickedEvent;
                buttonNextClickedEvent.Set();

                //Remarks to the inputs and outputs
                pres.lblExplanationSectionHeading.Visibility = Visibility.Visible;
                pres.txtExplanationSectionText.Visibility = Visibility.Visible;

                //Title of Presentation
                pres.lblTitleHeading.Visibility = Visibility.Hidden;

                
                //Introduction
                pres.lblIntroductionSectionHeading.Visibility = Visibility.Hidden;
                pres.lblIntroductionHeading.Visibility = Visibility.Hidden;
                pres.txtIntroductionText.Visibility = Visibility.Hidden;
                
                //Construction
                pres.lblConstructionSectionHeading.Visibility = Visibility.Hidden;
                pres.lblConstructionHeading.Visibility = Visibility.Hidden;
                pres.txtConstructionScheme.Visibility = Visibility.Hidden;
                pres.txtConstructionText1.Visibility = Visibility.Hidden;
                pres.txtConstructionText2.Visibility = Visibility.Hidden;
                pres.txtConstructionText3.Visibility = Visibility.Hidden;
                pres.txtConstructionText4.Visibility = Visibility.Hidden;
                pres.imgConstructionSponge.Visibility = Visibility.Hidden;

                //Calculation
                pres.lblCalculationSectionHeading.Visibility = Visibility.Hidden;
                pres.imgCalculation.Visibility = Visibility.Hidden;
                pres.txtCalculationRounds.Visibility = Visibility.Hidden;
                pres.lblCalculationHeading.Visibility = Visibility.Hidden;

                
                //Calculation finished
                pres.lblCalculationSectionHeading.Visibility = Visibility.Hidden;

                //Last
                pres.lblFinishedSectionHeading.Visibility = Visibility.Hidden;
                pres.txtFinished.Visibility = Visibility.Hidden;

                //Error
                pres.txtError.Visibility = Visibility.Hidden;
                
                //Buttons
                pres.spButtons.Visibility = Visibility.Hidden;
                pres.buttonSkipIntro.IsEnabled = false;
                pres.buttonSkipCalc.IsEnabled = false;
                pres.buttonNext.IsEnabled = false;
                pres.SkipChapter = false;
                pres.spStartRestartButtons.Visibility = Visibility.Visible;
                pres.buttonStart.IsEnabled = false;
                pres.buttonRestart.IsEnabled = false;

                //progress counter
                pres.txtStep.Visibility = Visibility.Hidden;

            }, null);
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
            Paragraph p = new Paragraph();

            //headline of lblExplanationSectionHeading
            p.Inlines.Add(new Run(Resources.PresExplanationSectionHeading));
            pres.lblExplanationSectionHeading.Document.Blocks.Add(p);
            pres.lblExplanationSectionHeading.Document.Blocks.Remove(pres.lblExplanationSectionHeading.Document.Blocks.FirstBlock);

            //headline of lblTitleHeading
            p = new Paragraph();
            p.Inlines.Add(new Run(Resources.PresTitleHeading));
            p.TextAlignment = TextAlignment.Center;
            pres.lblTitleHeading.Document.Blocks.Add(p);
            pres.lblTitleHeading.Document.Blocks.Remove(pres.lblTitleHeading.Document.Blocks.FirstBlock);

            //headline of lblIntroductionSectionHeading
            p = new Paragraph();
            p.Inlines.Add(new Run(Resources.PresIntroductionSectionHeadingNum));
            p.TextAlignment = TextAlignment.Center;
            pres.lblIntroductionSectionHeading.Document.Blocks.Add(p);
            pres.lblIntroductionSectionHeading.Document.Blocks.Remove(pres.lblIntroductionSectionHeading.Document.Blocks.FirstBlock);

            //headline of lblIntroductionHeading
            p = new Paragraph();
            p.Inlines.Add(new Run(Resources.PresIntroductionSectionHeading));
            p.TextAlignment = TextAlignment.Center;
            pres.lblIntroductionHeading.Document.Blocks.Add(p);
            pres.lblIntroductionHeading.Document.Blocks.Remove(pres.lblIntroductionHeading.Document.Blocks.FirstBlock);

            //headline of PresConstructionSectionHeadingNum
            p = new Paragraph();
            p.Inlines.Add(new Run(Resources.PresConstructionSectionHeadingNum));
            p.TextAlignment = TextAlignment.Center;
            pres.lblConstructionSectionHeading.Document.Blocks.Add(p);
            pres.lblConstructionSectionHeading.Document.Blocks.Remove(pres.lblConstructionSectionHeading.Document.Blocks.FirstBlock);

            //headline of lblConstructionHeading
            p = new Paragraph();
            p.Inlines.Add(new Run(Resources.PresConstructionSectionHeading));
            p.TextAlignment = TextAlignment.Center;
            pres.lblConstructionHeading.Document.Blocks.Add(p);
            pres.lblConstructionHeading.Document.Blocks.Remove(pres.lblConstructionHeading.Document.Blocks.FirstBlock);

            //headline of lblCalculationSectionHeading
            p = new Paragraph();
            p.Inlines.Add(new Run(Resources.PresCalculationSectionHeadingNum));
            p.TextAlignment = TextAlignment.Center;
            pres.lblCalculationSectionHeading.Document.Blocks.Add(p);
            pres.lblCalculationSectionHeading.Document.Blocks.Remove(pres.lblCalculationSectionHeading.Document.Blocks.FirstBlock);

            //headline of lblCalculationHeading
            p = new Paragraph();
            p.Inlines.Add(new Run(Resources.PresCalculationSectionHeading));
            p.TextAlignment = TextAlignment.Center;
            pres.lblCalculationHeading.Document.Blocks.Add(p);
            pres.lblCalculationHeading.Document.Blocks.Remove(pres.lblCalculationHeading.Document.Blocks.FirstBlock);

            //headline of lblFinishedSectionHeading
            p = new Paragraph();
            p.Inlines.Add(new Run(Resources.PresFinishedSectionHeading));
            p.TextAlignment = TextAlignment.Center;
            pres.lblFinishedSectionHeading.Document.Blocks.Add(p);
            pres.lblFinishedSectionHeading.Document.Blocks.Remove(pres.lblFinishedSectionHeading.Document.Blocks.FirstBlock);

            //text of txtIntroductionText
            p = new Paragraph();
            p.Inlines.Add(new Run(Resources.PresIntroductionPart1Text));
            p.TextAlignment = TextAlignment.Left;
            pres.txtIntroductionText.Document.Blocks.Add(p);
            pres.txtIntroductionText.Document.Blocks.Remove(pres.txtIntroductionText.Document.Blocks.FirstBlock);

            //text of txtConstructionText1
            p = new Paragraph();
            p.Inlines.Add(new Run(Resources.PresConstructionPart1Text));
            p.TextAlignment = TextAlignment.Left;
            pres.txtConstructionText1.Document.Blocks.Add(p);
            pres.txtConstructionText1.Document.Blocks.Remove(pres.txtConstructionText1.Document.Blocks.FirstBlock);

            //text of txtConstructionScheme
            p = new Paragraph();
            p.Inlines.Add(new Run(Resources.PresConstructionScheme));
            p.TextAlignment = TextAlignment.Left;
            pres.txtConstructionScheme.Document.Blocks.Add(p);
            pres.txtConstructionScheme.Document.Blocks.Remove(pres.txtConstructionScheme.Document.Blocks.FirstBlock);

            //text of txtConstructionText2
            p = new Paragraph();
            p.Inlines.Add(new Run(Resources.PresConstructionPart2Text));
            p.TextAlignment = TextAlignment.Left;
            pres.txtConstructionText2.Document.Blocks.Add(p);
            pres.txtConstructionText2.Document.Blocks.Remove(pres.txtConstructionText2.Document.Blocks.FirstBlock);

            //text of txtError
            p = new Paragraph();
            p.Inlines.Add(new Run(Resources.PresErrorText));
            p.TextAlignment = TextAlignment.Left;
            pres.txtError.Document.Blocks.Add(p);
            pres.txtError.Document.Blocks.Remove(pres.txtError.Document.Blocks.FirstBlock);

            //text of txtFinished
            p = new Paragraph();
            p.Inlines.Add(new Run(Resources.PresFinishedText));
            p.TextAlignment = TextAlignment.Left;
            pres.txtFinished.Document.Blocks.Add(p);
            pres.txtFinished.Document.Blocks.Remove(pres.txtFinished.Document.Blocks.FirstBlock);















            //for formatting the text 
            var parts = Resources.PresSectionIntroductionText.Split(new[] { "<Bold>", "</Bold>" }, StringSplitOptions.None);
            p = new Paragraph();
            bool isBold = false;
            foreach (var part in parts)
            {
                if (isBold)
                {
                    //pres.txtExplanationSectionText.Inlines.Add(new Bold(new Run(part)));
                    p.Inlines.Add(new Bold(new Run(part)));
                }
                else
                {
                    //pres.txtExplanationSectionText.Inlines.Add(new Run(part));
                    p.Inlines.Add(new Run(part));
                }
                isBold = !isBold;
            }
            pres.txtExplanationSectionText.Document.Blocks.Add(p);
            pres.txtExplanationSectionText.Document.Blocks.Remove(pres.txtExplanationSectionText.Document.Blocks.FirstBlock);

            //for formatting the text 
            parts = Resources.PresConstructionPart3Text.Split(new[] { "<Bold>", "</Bold>" }, StringSplitOptions.None);
            p = new Paragraph();
            isBold = false;
            foreach (var part in parts)
            {
                if (isBold)
                {
                    //pres.txtConstructionText3.Inlines.Add(new Bold(new Run(part)));
                    p.Inlines.Add(new Bold(new Run(part)));
                }
                else
                {
                    //pres.txtConstructionText3.Inlines.Add(new Run(part));
                    p.Inlines.Add(new Run(part));
                }
                isBold = !isBold;
            }
            pres.txtConstructionText3.Document.Blocks.Add(p);
            pres.txtConstructionText3.Document.Blocks.Remove(pres.txtConstructionText3.Document.Blocks.FirstBlock);

            //for formatting the text 
            parts = Resources.PresConstructionPart4Text.Split(new[] { "<Bold>", "</Bold>" }, StringSplitOptions.None);
            p = new Paragraph();
            isBold = false;
            foreach (var part in parts)
            {
                if (isBold)
                {
                    //pres.txtConstructionText4.Inlines.Add(new Bold(new Run(part)));
                    p.Inlines.Add(new Bold(new Run(part)));
                }
                else
                {
                    //pres.txtConstructionText4.Inlines.Add(new Run(part));
                    p.Inlines.Add(new Run(part));
                }
                isBold = !isBold;
            }
            pres.txtConstructionText4.Document.Blocks.Add(p);
            pres.txtConstructionText4.Document.Blocks.Remove(pres.txtConstructionText4.Document.Blocks.FirstBlock);
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
    }
}
