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
using KPFSHA256.Properties;
using Org.BouncyCastle.Crypto.Digests;

namespace Cryptool.Plugins.KPFSHA256
{
    [Author("Christian Bender", "christian1.bender@student.uni-siegen.de", null, "http://www.uni-siegen.de")]
    [PluginInfo("KPFSHA256.Properties.Resources", "PluginCaption", "KPFSHA256Tooltip", "KPFSHA256/userdoc.xml", new[] { "KPFSHA256/images/icon.png" })]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class KPFSHA256 : ICrypComponent
    {
        #region Private Variables

        private readonly KPFSHA256Settings settings = new KPFSHA256Settings();
        private KPFSHA256Pres pres = new KPFSHA256Pres();
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
        /// Computes the KPFSHA256 function with a 32 bit counter. Its like the traditional KDF described in https://eprint.iacr.org/2010/264.pdf but has a 32-bit counter for longer outputs
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="key"></param>
        /// <param name="outputBytes"></param>
        /// <param name="show"></param>
        /// <param name="buttonEvent"></param>
        /// <returns></returns>
        private byte[] computeKPFSHA256_IntCTR(byte[] msg, byte[] key, int outputBytes, bool show, AutoResetEvent buttonEvent)
        {
            //hash object
            Sha256Digest sha256 = new Sha256Digest();
            //calculates the ceil(iteration) rounds
            var N = Math.Ceiling(Convert.ToDouble(outputBytes) / sha256.GetDigestSize());

            if (N > 4294967296)
            {
                throw new ToMuchOutputRequestedException(Resources.ExToMuchOutputRequested.Replace("{0}", outputBytes.ToString()).Replace("{1}", (4294967295 * sha256.GetDigestSize()).ToString()));
            }

            //Problem mit System.outofmemoryexception...
            if(Convert.ToInt32(N) * sha256.GetDigestSize() > 99999999)
            {
                //throw new ToMuchOutputRequestedException(Resources.ExToMuchOutputRequested.Replace("{0}", outputBytes.ToString()).Replace("{1}", (4294967295 * sha256.GetDigestSize()).ToString()));
            }

            //counter as hex beginning with zero byte. Counter has a size of 16 bits.
            int CTR = 0;
            //input byte array as: key || ctr || msg
            byte[] input = new byte[key.Length + sizeof(int) + msg.Length];
            //output byte array for all rounds of the iteration
            byte[] km = new byte[Convert.ToInt32(N) * sha256.GetDigestSize()];
            //output byte array for the function. in case of truncated output
            byte[] result = new byte[outputBytes];
            //output array for temp output for debug in the ui
            byte[] tmp_result = new byte[sha256.GetDigestSize()];
            //var for progess
            double prgs_step = prgs_val / N;
            double prgs_Curval = 0.1;

            //prepare input array
            System.Buffer.BlockCopy(key, 0, input, 0, key.Length);
            System.Buffer.BlockCopy(msg, 0, input, key.Length + sizeof(int), msg.Length);

            for (int i = 0; i < N; i++, CTR++)
            {
                ProgressChanged(prgs_Curval, 1);
                prgs_Curval += prgs_step;
                //sets the counter
                System.Buffer.BlockCopy(BitConverter.GetBytes(CTR), 0, input, key.Length, sizeof(int));
                //update internal hash state
                sha256.BlockUpdate(input, 0, input.Length);
                //finish the hashing: leaves the state resetted for the next round
                sha256.DoFinal(km, i * sha256.GetDigestSize());
                //Console.WriteLine("CTR: " + CTR + "\nHash: " + BitConverter.ToString(km).Replace("-", "") + "\n");

                //IF DEBUG
                System.Buffer.BlockCopy(km, i * sha256.GetDigestSize(), tmp_result, 0, tmp_result.Length);
                StringBuilder strBuilderDebug = new StringBuilder();
                StringBuilder strBuilderPres = new StringBuilder();
                strBuilderDebug.Append(Resources.KeyMaterialDebugTextTemplate.Replace("{0}", (i + 1).ToString()));
                strBuilderPres.Append(Resources.PresKeyMaterialDebugTextTemplate.Replace("{0}", (i + 1).ToString()));

                //Generate formatted output for debug output textfield
                string tmp = "";
                for (int j = 1, k = 1; j <= sha256.GetDigestSize(); j++)
                {
                    tmp += BitConverter.ToString(tmp_result, (j - 1), 1).Replace("-", "") + " ";
                    if (j % 8 == 0)
                    {
                        strBuilderDebug.Replace("{" + k + "}", tmp);
                        strBuilderPres.Replace("{" + k + "}", tmp);
                        k++;
                        tmp = "";
                    }
                }

                _keyMaterialDebug = strBuilderDebug.ToString();
                OnPropertyChanged("KeyMaterialDebug");

                if (show && !pres.SkipIntro && !(i == (N - 1)))
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                    {
                        pres.txtIterationRounds.Text = "";
                        pres.txtIterationRounds.Inlines.Add(new Run(Resources.PresIterationRounds.Replace("{0}", (i + 1).ToString()).Replace("{1}", N.ToString())
                            .Replace("{2}", System.Text.Encoding.UTF8.GetString(msg)).Replace("{3}", (i + 1).ToString()).Replace("{4}", System.Text.Encoding.UTF8.GetString(key))));
                        pres.txtIterationDebugOutput.Text = strBuilderPres.ToString();
                    }, null);

                    buttonEvent = pres.buttonNextClickedEvent;
                    buttonEvent.WaitOne();

                }
                else if (show && !pres.SkipIntro && !(i == N))
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                    {
                        pres.txtIterationRounds.Text = "";
                        pres.txtIterationRounds.Inlines.Add(new Run(Resources.PresIterationRounds.Replace("{0}", (i + 1).ToString()).Replace("{1}", N.ToString())
                            .Replace("{2}", System.Text.Encoding.UTF8.GetString(msg)).Replace("{3}", (i + 1).ToString()).Replace("{4}", System.Text.Encoding.UTF8.GetString(key))));
                        pres.txtIterationDebugOutput.Text = strBuilderPres.ToString();
                    }, null);
                }

                if (pres.SkipIntro)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                    {
                        pres.imgIteration.Visibility = Visibility.Hidden;
                    }, null);
                }
                //ENDIF DEBUG
            }

            //truncated output
            System.Buffer.BlockCopy(km, 0, result, 0, outputBytes);
            //DEBUG
            //Console.WriteLine("KM: " + BitConverter.ToString(result).Replace("-", ""));
            return result;
        }


        /// <summary>
        /// Computes the KPFSHA256 function with a 8 bit counter. Its like the traditional KDF described in https://eprint.iacr.org/2010/264.pdf
        /// Attention: Online hasher like https://hashgenerator.de/ are using for every input char a single byte. If you're trying to compare single inputs with a online hasher, you need to know, that they are using 
        /// ASCII encoding for the counter. That means, the string 0 is representated as 0x30, my representation starts with 0x00. Thats why you would need to adapt the input byte in my function to 0x30 if you like to compare.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="key"></param>
        /// <param name="outputBytes"></param>
        /// <returns>
        private byte[] computeKPFSHA256_8BitCTR(byte[] msg, byte[] key, int outputBytes, bool show, AutoResetEvent buttonEvent)
        {
            //hash object
            Sha256Digest sha256 = new Sha256Digest();
            //calculates the ceil(iteration) rounds
            var N = Math.Ceiling(Convert.ToDouble(outputBytes) / sha256.GetDigestSize());
            //counter as hex beginning with zero byte
            byte CTR = 0x00;
            //input byte array as: key || ctr || msg
            byte[] input = new byte[key.Length + 1 + msg.Length];
            //output byte array for all rounds of the iteration
            byte[] km = new byte[Convert.ToInt32(N) * sha256.GetDigestSize()];
            //output byte array for the function. in case of truncated output
            byte[] result = new byte[outputBytes];
            //output array for temp output for debug in the ui
            byte[] tmp_result = new byte[sha256.GetDigestSize()];
            //var for progess
            double prgs_step = prgs_val / N;
            double prgs_Curval = 0.1;

            //prepare input array
            System.Buffer.BlockCopy(key, 0, input, 0, key.Length);
            System.Buffer.BlockCopy(msg, 0, input, key.Length + 1, msg.Length);

            if (N > 255)
            {
                throw new ToMuchOutputRequestedException(Resources.ExToMuchOutputRequested.Replace("{0}" , outputBytes.ToString()).Replace("{1}", (255 * sha256.GetDigestSize()).ToString()));
            }

            for (int i = 0; i < N; i++, CTR++)
            {
                ProgressChanged(prgs_Curval, 1);
                prgs_Curval += prgs_step;
                //sets the counter
                input[key.Length] = CTR;
                //update internal hash state
                sha256.BlockUpdate(input, 0, input.Length);
                //finish the hashing: leaves the state resetted for the next round
                sha256.DoFinal(km, i * sha256.GetDigestSize());
                //Console.WriteLine("CTR: " + CTR + "\nHash: " + BitConverter.ToString(km).Replace("-", "") + "\n");

                //IF DEBUG
                System.Buffer.BlockCopy(km, i * sha256.GetDigestSize(), tmp_result, 0, tmp_result.Length);
                StringBuilder strBuilderDebug = new StringBuilder();
                StringBuilder strBuilderPres = new StringBuilder();
                strBuilderDebug.Append(Resources.KeyMaterialDebugTextTemplate.Replace("{0}", (i + 1).ToString()));
                strBuilderPres.Append(Resources.PresKeyMaterialDebugTextTemplate.Replace("{0}", (i + 1).ToString()));

                //Generate formatted output for debug output textfield
                string tmp = "";
                for(int j = 1, k = 1; j <= sha256.GetDigestSize(); j++)
                {
                    tmp += BitConverter.ToString(tmp_result, (j-1), 1).Replace("-", "") + " ";
                    if (j % 8 == 0)
                    {
                        strBuilderDebug.Replace("{" + k + "}", tmp);
                        strBuilderPres.Replace("{" + k + "}", tmp);
                        k++;
                        tmp = "";
                    }
                }

                _keyMaterialDebug = strBuilderDebug.ToString();
                OnPropertyChanged("KeyMaterialDebug");

                if (show && !pres.SkipIntro && !(i == (N-1)))
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                    {
                        pres.txtIterationRounds.Text = "";
                        pres.txtIterationRounds.Inlines.Add(new Run(Resources.PresIterationRounds.Replace("{0}", (i+1).ToString()).Replace("{1}", N.ToString())
                            .Replace("{2}", System.Text.Encoding.UTF8.GetString(msg)).Replace("{3}", (i+1).ToString()).Replace("{4}", System.Text.Encoding.UTF8.GetString(key))));
                        pres.txtIterationDebugOutput.Text = strBuilderPres.ToString();
                    }, null);

                    buttonEvent = pres.buttonNextClickedEvent;
                    buttonEvent.WaitOne();

                }else if (show && !pres.SkipIntro && !(i == N))
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                    {
                        pres.txtIterationRounds.Text = "";
                        pres.txtIterationRounds.Inlines.Add(new Run(Resources.PresIterationRounds.Replace("{0}", (i + 1).ToString()).Replace("{1}", N.ToString())
                            .Replace("{2}", System.Text.Encoding.UTF8.GetString(msg)).Replace("{3}", (i + 1).ToString()).Replace("{4}", System.Text.Encoding.UTF8.GetString(key))));
                        pres.txtIterationDebugOutput.Text = strBuilderPres.ToString();
                    }, null);
                }

                if (pres.SkipIntro)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                    {
                        pres.imgIteration.Visibility = Visibility.Hidden;
                    }, null);
                }
                //ENDIF DEBUG
            }

            //truncat output
            System.Buffer.BlockCopy(km, 0, result, 0, outputBytes);
            //DEBUG
            //Console.WriteLine("KM: " + BitConverter.ToString(result).Replace("-", ""));
            return result;
        }

        /// <summary>
        /// the method to be called in the workerthread
        /// </summary>
        private void tExecute()
        {
            //Progessbar adjusting
            ProgressChanged(0, 1);
        
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
                pres.imgConstructionKPFSHA256.Visibility = Visibility.Hidden;

                //Iterationphase
                pres.lblIterationSectionHeading.Visibility = Visibility.Hidden;
                pres.lblIterationHeading.Visibility = Visibility.Hidden;
                pres.imgIteration.Visibility = Visibility.Hidden;
                pres.txtIterationDebugOutput.Visibility = Visibility.Hidden;

                //Calculation finished
                pres.lblFinishedSectionHeading.Visibility = Visibility.Hidden;
                pres.lblIterationHeading.Visibility = Visibility.Hidden;
                pres.txtIterationRounds.Visibility = Visibility.Hidden;

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

            _keyMaterialDebug = "";
            OnPropertyChanged("KeyMaterialDebug");

            AutoResetEvent buttonNextClickedEvent = pres.buttonNextClickedEvent;
            buttonNextClickedEvent = pres.buttonNextClickedEvent;
            buttonNextClickedEvent.Reset();

            //Check if presentation shall be displayed
            if (settings.DisplayPres)
            {
                if (!pres.SkipIntro)
                {
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
                        pres.imgConstructionKPFSHA256.Visibility = Visibility.Visible;


                    }, null);

                    buttonNextClickedEvent = pres.buttonNextClickedEvent;
                    buttonNextClickedEvent.WaitOne();
                }

                //Block: Iteration section heading  
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
                    pres.imgConstructionKPFSHA256.Visibility = Visibility.Hidden;

                    pres.lblConstructionHeading.Visibility = Visibility.Hidden;
                    pres.txtConstructionText3.Visibility = Visibility.Hidden;
                    pres.txtConstructionText1.Visibility = Visibility.Hidden;
                    pres.imgConstructionKPFSHA256.Visibility = Visibility.Hidden;
                    pres.lblTitleHeading.Visibility = Visibility.Hidden;

                    pres.buttonSkipIntro.IsEnabled = false;
                    pres.buttonSkipCalc.IsEnabled = true;
                    pres.lblIterationSectionHeading.Visibility = Visibility.Visible;

                }, null);

                buttonNextClickedEvent = pres.buttonNextClickedEvent;
                buttonNextClickedEvent.WaitOne();

                //Block: Iteration section 
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {

                    pres.lblIterationSectionHeading.Visibility = Visibility.Hidden;
                    pres.lblIterationHeading.Visibility = Visibility.Visible;
                    pres.txtIterationRounds.Visibility = Visibility.Visible;
                    pres.imgIteration.Visibility = Visibility.Visible;
                    pres.txtIterationDebugOutput.Visibility = Visibility.Visible;

                }, null);

            }

            //Convert inputstrings into byte arrays
            byte[] skm = Encoding.UTF8.GetBytes(_skm);
            byte[] key = Encoding.UTF8.GetBytes(_key);
            try
            {
                ProgressChanged(0.1, 1);
                byte[] result;
                if (settings.InfinityOutput)
                {
                    result = computeKPFSHA256_8BitCTR(skm, key, _outputBytes, settings.DisplayPres, buttonNextClickedEvent);
                }
                else
                {
                    result = computeKPFSHA256_IntCTR(skm, key, _outputBytes, settings.DisplayPres, buttonNextClickedEvent);
                }

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
            //in case of too long outputs specified
            catch (ToMuchOutputRequestedException ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.lblIterationSectionHeading.Visibility = Visibility.Hidden;
                    pres.lblIterationHeading.Visibility = Visibility.Hidden;
                    pres.txtIterationRounds.Visibility = Visibility.Hidden;
                    pres.imgIteration.Visibility = Visibility.Hidden;
                    pres.txtIterationDebugOutput.Visibility = Visibility.Hidden;
                    pres.txtError.Visibility = Visibility.Visible;
                }, null);
                return;
            }
            catch (System.OutOfMemoryException ex)
            {
                GuiLogMessage(ex.Message + " " + Resources.ExSystemOutOfMemory, NotificationLevel.Error);
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.lblIterationSectionHeading.Visibility = Visibility.Hidden;
                    pres.lblIterationHeading.Visibility = Visibility.Hidden;
                    pres.txtIterationRounds.Visibility = Visibility.Hidden;
                    pres.imgIteration.Visibility = Visibility.Hidden;
                    pres.txtIterationDebugOutput.Visibility = Visibility.Hidden;
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
                    pres.lblIterationHeading.Visibility = Visibility.Hidden;
                    pres.txtIterationRounds.Visibility = Visibility.Hidden;
                    pres.txtIterationDebugOutput.Visibility = Visibility.Hidden;
                    pres.lblFinishedSectionHeading.Visibility = Visibility.Visible;
                    pres.txtFinished.Visibility = Visibility.Visible;
                    pres.imgIteration.Visibility = Visibility.Hidden;
                }, null);
            }

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

        /// <summary>
        /// Output for debug
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputKeyMaterialDebugCaption", "OutputKeyMaterialDebugToolTip")]
        public string KeyMaterialDebug
        {
            get
            {
                return _keyMaterialDebug;
            }
            set
            {
                _keyMaterialDebug = value;
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
                pres.imgConstructionKPFSHA256.Visibility = Visibility.Hidden;

                //Iterationphase
                pres.lblIterationSectionHeading.Visibility = Visibility.Hidden;
                pres.imgIteration.Visibility = Visibility.Hidden;
                pres.txtIterationRounds.Visibility = Visibility.Hidden;
                pres.lblIterationHeading.Visibility = Visibility.Hidden;
                pres.txtIterationDebugOutput.Visibility = Visibility.Hidden;

                //Calculation finished
                pres.lblIterationSectionHeading.Visibility = Visibility.Hidden;

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
            parts = Resources.PresIntroductionPart1Text.Split(new[] { "<Bold>", "</Bold>" }, StringSplitOptions.None);
            isBold = false;
            foreach (var part in parts)
            {
                if (isBold)
                {
                    pres.txtIntroductionText.Inlines.Add(new Bold(new Run(part)));
                }
                else
                {
                    pres.txtIntroductionText.Inlines.Add(new Run(part));
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
