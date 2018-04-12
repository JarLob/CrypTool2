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
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class KPFSHAKE256 : ICrypComponent
    {
        #region Private Variables

        // HOWTO: You need to adapt the settings class as well, see the corresponding file.
        private readonly KPFSHAKE256Settings settings = new KPFSHAKE256Settings();
        private KPFSHAKE256Pres pres = new KPFSHAKE256Pres();
        private string _skm;
        private string _key;
        private int _outputBytes;
        private string _keyMaterial;
        private string _keyMaterialDebug;
        private Thread workerThread;
        private double prgs_val = 0.8;

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
        ///  the method to be called in the workerthread
        /// </summary>
        private void tExecute()
        {
            //Progessbar adjusting
            ProgressChanged(0, 1);

            byte[] skm = Encoding.UTF8.GetBytes(_skm);
            byte[] key = Encoding.UTF8.GetBytes(_key);

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
                pres.spButtons.Visibility = Visibility.Visible;
                pres.buttonSkipIntro.IsEnabled = false;
                pres.buttonSkipCalc.IsEnabled = false;
                pres.buttonNext.IsEnabled = false;
                pres.SkipIntro = false;

            }, null);
     
            //Clean up outputs
            _keyMaterial = "";
            OnPropertyChanged("KeyMaterial");

            AutoResetEvent buttonNextClickedEvent = pres.buttonNextClickedEvent;
            buttonNextClickedEvent = pres.buttonNextClickedEvent;
            buttonNextClickedEvent.Reset();

            //Check if presentation shall be displayed
            if (settings.DisplayPres)
            {
                if (!pres.SkipIntro)
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

                //Block: Introduction section heading  
                if (!pres.SkipIntro)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.lblTitleHeading.Visibility = Visibility.Hidden;
                        pres.lblIntroductionSectionHeading.Visibility = Visibility.Visible;

                    }, null);

                    buttonNextClickedEvent = pres.buttonNextClickedEvent;
                    buttonNextClickedEvent.WaitOne();
                }

                //Block: Introduction section
                if (!pres.SkipIntro)
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

                //Block: Construction section heading
                if (!pres.SkipIntro)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.lblIntroductionHeading.Visibility = Visibility.Hidden;
                        pres.txtIntroductionText.Visibility = Visibility.Hidden;
                        pres.lblConstructionSectionHeading.Visibility = Visibility.Visible;

                    }, null);

                    buttonNextClickedEvent = pres.buttonNextClickedEvent;
                    buttonNextClickedEvent.WaitOne();
                }

                //Block: Construction section part 1
                if (!pres.SkipIntro)
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

                //Block: Construction section part 2
                if (!pres.SkipIntro)
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

                //Block: Construction section part 3
                if (!pres.SkipIntro)
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

                //Block: Construction section part 4
                if (!pres.SkipIntro)
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

                //Block: Calculation section heading  
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {

                    pres.SkipIntro = false;

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
                    pres.txtCalculationRounds.Inlines.Add(new Run(Resources.PresCalculationText.Replace("{1}", System.Text.Encoding.UTF8.GetString(skm)).Replace("{2}", System.Text.Encoding.UTF8.GetString(key))));

                }, null);
            }

            try
            {
                //Progessbar adjusting
                ProgressChanged(0.1, 1);

                byte[] result = computeKPFSHA256XOF(skm, key, _outputBytes);

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

                if (!pres.SkipIntro && settings.DisplayPres)
                {
                    buttonNextClickedEvent = pres.buttonNextClickedEvent;
                    buttonNextClickedEvent.WaitOne();
                }

                _keyMaterial = BitConverter.ToString(result).Replace("-", "");
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

            if (settings.DisplayPres)
            {
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.buttonNext.IsEnabled = false;
                    pres.buttonSkipCalc.IsEnabled = false;
                    pres.lblCalculationHeading.Visibility = Visibility.Hidden;
                    pres.txtCalculationRounds.Visibility = Visibility.Hidden;
                    pres.lblFinishedSectionHeading.Visibility = Visibility.Visible;
                    pres.txtFinished.Visibility = Visibility.Visible;
                    pres.imgCalculation.Visibility = Visibility.Hidden;
                }, null);
            }

            //Progessbar adjusting
            ProgressChanged(1, 1);


            //Progessbar adjusting
            ProgressChanged(1, 1);
        }

        #endregion

        #region Data Properties

        /// <summary>
        /// Input for source key material
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputSKMCaption", "InputSKMToolTip", true)]
        public string SKM
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
        public string Key
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
        public int OutputBytes
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
        public string KeyMaterial
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
                pres.SkipIntro = false;

            }, null);
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
            //for formatting the text 
            var parts = Resources.PresSectionIntroductionText.Split(new[] { "<Bold>", "</Bold>" }, StringSplitOptions.None);
            bool isBold = false;
            foreach (var part in parts)
            {
                if (isBold)
                {
                    pres.txtExplanationSectionText.Inlines.Add(new Bold(new Run(part)));
                }
                else
                {
                    pres.txtExplanationSectionText.Inlines.Add(new Run(part));
                }
                isBold = !isBold;
            }

            //for formatting the text 
            parts = Resources.PresConstructionPart3Text.Split(new[] { "<Bold>", "</Bold>" }, StringSplitOptions.None);
            isBold = false;
            foreach (var part in parts)
            {
                if (isBold)
                {
                    pres.txtConstructionText3.Inlines.Add(new Bold(new Run(part)));
                }
                else
                {
                    pres.txtConstructionText3.Inlines.Add(new Run(part));
                }
                isBold = !isBold;
            }

            //for formatting the text 
            parts = Resources.PresConstructionPart4Text.Split(new[] { "<Bold>", "</Bold>" }, StringSplitOptions.None);
            isBold = false;
            foreach (var part in parts)
            {
                if (isBold)
                {
                    pres.txtConstructionText4.Inlines.Add(new Bold(new Run(part)));
                }
                else
                {
                    pres.txtConstructionText4.Inlines.Add(new Run(part));
                }
                isBold = !isBold;
            }
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
