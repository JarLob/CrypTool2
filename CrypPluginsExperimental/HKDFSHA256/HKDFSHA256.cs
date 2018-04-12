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
using HKDFSHA256.Properties;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;

namespace Cryptool.Plugins.HKDFSHA256
{
    [Author("Christian Bender", "christian1.bender@student.uni-siegen.de", null, "http://www.uni-siegen.de")]
    [PluginInfo("HKDFSHA256.Properties.Resources", "PluginCaption", "HKDFSHA256Tooltip", "HKDFSHA256/userdoc.xml", new[] { "HKDFSHA256/images/icon.png" })]
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class HKDFSHA256 : ICrypComponent
    {
        #region Private Variables

        private readonly HKDFSHA256Settings settings = new HKDFSHA256Settings();
        private HKDFSHA256Pres pres = new HKDFSHA256Pres();
        private string _skm;
        private string _salt;
        private string _ctxInfo;
        private int _outputBytes;
        private string _keyMaterial;
        private string _keyMaterialDebug;
        private Thread workerThread;
        private double prgs_val = 0.8;

        #endregion

        #region Methods for calculation

        /// <summary>
        /// Computes the HKDF with SHA256 and a 32 bit counter. Its like the Extract-then-Expand KDF described in https://eprint.iacr.org/2010/264.pdf. Its implemented in the RFC style: https://tools.ietf.org/html/rfc5869 but has a 32-Bit counter
        /// Bigger Counter is for computing bigger outputs <= 2^32 * hashlen
        /// </summary>
        /// <param name="skm"></param>
        /// <param name="ctxInfo"></param>
        /// <param name="salt"></param>
        /// <param name="outputBytes"></param>
        /// <returns></returns>
        byte[] computeHKDFSHA256_32BitCTR(byte[] skm, byte[] ctxInfo, byte[] salt, int outputBytes, bool show, AutoResetEvent buttonEvent)
        {
            //skm = sourcekeymaterial, ctxinfo = context information, salt = key for hmac, outputbytes = wanted outputbytes as KM
            //hmac object
            var hmac = new HMac(new Sha256Digest());
            //counter as hex beginning with one byte
            int CTR = 0x01;
            //calculates the ceil(iteration) rounds
            var N = Math.Ceiling(Convert.ToDouble(outputBytes) / hmac.GetMacSize());
            //output byte array for all rounds of the iteration
            byte[] km = new byte[Convert.ToInt32(N) * hmac.GetMacSize()];
            //prk for hkdf
            byte[] prk = new byte[hmac.GetMacSize()];
            //array for input in the iteration
            byte[] input = new byte[prk.Length + ctxInfo.Length + sizeof(int)];
            //output byte array for the function. in case of truncated output
            byte[] result = new byte[outputBytes];
            //output array for temp output for debug in the ui
            byte[] tmp_result = new byte[hmac.GetMacSize()];
            double prgs_step = prgs_val / (N + 1);
            double prgs_Curval = 0.1;

            if (N > 4294967296)
            {
                throw new TooMuchOutputRequestedException(Resources.ExToMuchOutputRequested.Replace("{0}", outputBytes.ToString()).Replace("{1}", (4294967295 * hmac.GetMacSize()).ToString()));
            }

            //if salt is not provided, set it to 0x00 * hashlength
            if (salt == null)
            {
                //creates zeroed byte array
                salt = new byte[hmac.GetMacSize()];
            }

            //prepare hmac with salt as key
            hmac.Init(new KeyParameter(salt));

            //Extract
            //update internal state
            hmac.BlockUpdate(skm, 0, skm.Length);
            //finish the hmac: leaves the state resetted for the next round
            hmac.DoFinal(prk, 0);

            //DEBUG
            //Console.WriteLine("PRK:  " + BitConverter.ToString(prk).Replace("-", ""));

            //IF DEBUG
            System.Buffer.BlockCopy(prk, 0, tmp_result, 0, tmp_result.Length);
            StringBuilder strBuilderDebug = new StringBuilder();
            StringBuilder strBuilderPresDebug = new StringBuilder();
            strBuilderDebug.Append(Resources.PRKDebugTextTemplate);
            strBuilderPresDebug.Append(Resources.PresPRKDebugTextTemplate);

            pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
            {
                pres.txtIterationRounds.Text = "";
                pres.txtIterationRounds.Inlines.Add(new Run(Resources.PresIterationPRKCalc.Replace("{0}", System.Text.Encoding.UTF8.GetString(skm)).Replace("{1}", System.Text.Encoding.UTF8.GetString(salt))));
                pres.imgIterationPRK.Visibility = Visibility.Visible;
            }, null);

            if (show && !pres.SkipIntro)
            {
                buttonEvent = pres.buttonNextClickedEvent;
                buttonEvent.WaitOne();
            }

            //Generate formatted output for debug output textfield
            string tmp = "";
            for (int j = 1, k = 1; j <= hmac.GetMacSize(); j++)
            {
                tmp += BitConverter.ToString(tmp_result, (j - 1), 1).Replace("-", "") + " ";
                if (j % 8 == 0)
                {
                    strBuilderDebug.Replace("{" + k + "}", tmp);
                    strBuilderPresDebug.Replace("{" + k + "}", tmp);
                    k++;
                    tmp = "";
                }
            }

            pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
            {
                pres.txtIterationDebugOutput.Text = strBuilderPresDebug.ToString();
            }, null);

            _keyMaterialDebug = strBuilderDebug.ToString();
            OnPropertyChanged("KeyMaterialDebug");

            ProgressChanged(0.1, 1);

            if (show && !pres.SkipIntro)
            {
                buttonEvent = pres.buttonNextClickedEvent;
                buttonEvent.WaitOne();
            }

            //Expand
            //prepare input array
            System.Buffer.BlockCopy(ctxInfo, 0, input, 0, ctxInfo.Length);
            System.Buffer.BlockCopy(BitConverter.GetBytes(CTR), 0, input, ctxInfo.Length, sizeof(int));
            hmac.Init(new KeyParameter(prk));
            //update internal state
            hmac.BlockUpdate(input, 0, ctxInfo.Length + sizeof(int));
            //finish the hmac: leaves the state resetted for the next round
            hmac.DoFinal(km, 0);

            //IF DEBUG
            System.Buffer.BlockCopy(km, 0, tmp_result, 0, tmp_result.Length);
            strBuilderDebug = new StringBuilder();
            strBuilderPresDebug = new StringBuilder();
            StringBuilder strBuilderPresTxt = new StringBuilder();
            strBuilderDebug.Append(Resources.KeyMaterialDebugTextTemplate);
            strBuilderPresDebug.Append(Resources.PresKeyMaterialDebugTextTemplate);
            strBuilderPresTxt.Append(Resources.PresIterationRounds);

            //Generate formatted output for debug output textfield
            tmp = "";
            for (int j = 1, k = 1; j <= hmac.GetMacSize(); j++)
            {
                tmp += BitConverter.ToString(tmp_result, (j - 1), 1).Replace("-", "") + " ";
                if (j % 8 == 0)
                {
                    strBuilderDebug.Replace("{" + k + "}", tmp);
                    strBuilderPresDebug.Replace("{" + k + "}", tmp);

                    k++;
                    tmp = "";
                }
            }

            prgs_Curval += prgs_step;
            ProgressChanged(prgs_Curval, 1);

            pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
            {
                pres.imgIterationPRK.Visibility = Visibility.Hidden;
                pres.imgIterationKM1.Visibility = Visibility.Visible;
                pres.txtIterationRounds.Text = "";
                pres.txtIterationRounds.Inlines.Add(new Run(Resources.PresIterationRounds.Replace("{0}", "1").Replace("{1}", N.ToString()).Replace("{2}", BitConverter.ToString(prk).Replace("-", ""))
                    .Replace("{3}", System.Text.Encoding.UTF8.GetString(skm)).Replace("{4}", System.Text.Encoding.UTF8.GetString(ctxInfo)).Replace("{5}", CTR.ToString())));
                pres.txtIterationDebugOutput.Text = strBuilderPresDebug.ToString().Replace("{0}", "1");
            }, null);

            _keyMaterialDebug = strBuilderDebug.ToString().Replace("{0}", "1");
            OnPropertyChanged("KeyMaterialDebug");

            if (show && !pres.SkipIntro)
            {
                buttonEvent = pres.buttonNextClickedEvent;
                buttonEvent.WaitOne();
            }

            prgs_Curval += prgs_step;
            ProgressChanged(prgs_Curval, 1);

            CTR++;

            //DEBUG
            //Console.WriteLine("K(1): " + BitConverter.ToString(km, 0, 32).Replace("-", ""));

            for (int i = 1; i < N; i++, CTR++)
            {
                ProgressChanged(prgs_Curval, 1);
                prgs_Curval += prgs_step;
                //prepare input for next round
                System.Buffer.BlockCopy(km, (i - 1) * hmac.GetMacSize(), input, 0, hmac.GetMacSize());
                System.Buffer.BlockCopy(ctxInfo, 0, input, hmac.GetMacSize(), ctxInfo.Length);
                System.Buffer.BlockCopy(BitConverter.GetBytes(CTR), 0, input, ctxInfo.Length + hmac.GetMacSize(), sizeof(int));

                //calc hmac
                hmac.Init(new KeyParameter(prk));
                //update internal state
                hmac.BlockUpdate(input, 0, input.Length);
                //finish the hmac: leaves the state resetted for the next round
                hmac.DoFinal(km, i * hmac.GetMacSize());

                //DEBUG
                //Console.WriteLine("CTR: " + CTR + "\nHash: " + BitConverter.ToString(km, i * hmac.GetMacSize(), hmac.GetMacSize()).Replace("-", ""));

                System.Buffer.BlockCopy(km, i * hmac.GetMacSize(), tmp_result, 0, tmp_result.Length);
                strBuilderDebug = new StringBuilder();
                strBuilderPresDebug = new StringBuilder();
                strBuilderPresTxt = new StringBuilder();
                strBuilderDebug.Append(Resources.KeyMaterialDebugTextTemplate);
                strBuilderPresDebug.Append(Resources.PresKeyMaterialDebugTextTemplate);
                strBuilderPresTxt.Append(Resources.PresIterationRounds);

                //Generate formatted output for debug output textfield
                tmp = "";
                for (int j = 1, k = 1; j <= hmac.GetMacSize(); j++)
                {
                    tmp += BitConverter.ToString(tmp_result, (j - 1), 1).Replace("-", "") + " ";
                    if (j % 8 == 0)
                    {
                        strBuilderDebug.Replace("{" + k + "}", tmp);
                        strBuilderPresDebug.Replace("{" + k + "}", tmp);

                        k++;
                        tmp = "";
                    }
                }

                pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                {
                    pres.imgIterationKM1.Visibility = Visibility.Hidden;
                    pres.imgIterationKM2.Visibility = Visibility.Visible;
                    pres.txtIterationRounds.Text = "";
                    pres.txtIterationRounds.Inlines.Add(new Run(Resources.PresIterationRounds.Replace("{0}", (i + 1).ToString()).Replace("{1}", N.ToString()).Replace("{2}", BitConverter.ToString(prk).Replace("-", ""))
                        .Replace("{3}", System.Text.Encoding.UTF8.GetString(skm)).Replace("{4}", System.Text.Encoding.UTF8.GetString(ctxInfo)).Replace("{5}", CTR.ToString())));
                    pres.txtIterationDebugOutput.Text = strBuilderPresDebug.ToString().Replace("{0}", (i + 1).ToString());
                }, null);

                _keyMaterialDebug = strBuilderDebug.ToString().Replace("{0}", (i + 1).ToString());
                OnPropertyChanged("KeyMaterialDebug");

                if (show && !pres.SkipIntro && !(i == (N - 1)))
                {
                    buttonEvent = pres.buttonNextClickedEvent;
                    buttonEvent.WaitOne();

                }
            }

            //truncated output
            System.Buffer.BlockCopy(km, 0, result, 0, outputBytes);
            //DEBUG
            //Console.WriteLine("KM: " + BitConverter.ToString(result).Replace("-", ""));

            return result;
        }

        /// <summary>
        /// Computes the HKDF with SHA256 and a 8 bit counter (like in the specification). Its like the Extract-then-Expand KDF described in https://eprint.iacr.org/2010/264.pdf. Its implemented in the RFC style: https://tools.ietf.org/html/rfc5869
        /// </summary>
        /// <param name="skm"></param>
        /// <param name="ctxInfo"></param>
        /// <param name="salt"></param>
        /// <param name="outputBytes"></param>
        /// <returns></returns>
        byte[] computeHKDFSHA256_8BitCTR(byte[] skm, byte[] ctxInfo, byte[] salt, int outputBytes, bool show, AutoResetEvent buttonEvent)
        {
            //skm = sourcekeymaterial, ctxinfo = context information, salt = key for hmac, outputbytes = wanted outputbytes as KM
            //hmac object
            var hmac = new HMac(new Sha256Digest());
            //counter as hex beginning with one byte
            byte CTR = 0x01;
            //calculates the ceil(iteration) rounds
            var N = Math.Ceiling(Convert.ToDouble(outputBytes) / hmac.GetMacSize());
            //output byte array for all rounds of the iteration
            byte[] km = new byte[Convert.ToInt32(N) * hmac.GetMacSize()];
            //prk for hkdf
            byte[] prk = new byte[hmac.GetMacSize()];
            //array for input in the iteration
            byte[] input = new byte[prk.Length + 1 + ctxInfo.Length];
            //output byte array for the function. in case of truncated output
            byte[] result = new byte[outputBytes];
            //output array for temp output for debug in the ui
            byte[] tmp_result = new byte[hmac.GetMacSize()];
            double prgs_step = prgs_val / (N+1);
            double prgs_Curval = 0.1;

            if (N > 255)
            {
                throw new TooMuchOutputRequestedException(Resources.ExToMuchOutputRequested.Replace("{0}", outputBytes.ToString()).Replace("{1}", (255 * hmac.GetMacSize()).ToString()));
            }

            //if salt is not provided, set it to 0x00 * hashlength
            if (salt == null)
            {
                //creates zeroed byte array
                salt = new byte[hmac.GetMacSize()];
            }

            //prepare hmac with salt as key
            hmac.Init(new KeyParameter(salt));

            //Extract
            //update internal state
            hmac.BlockUpdate(skm, 0, skm.Length);
            //finish the hmac: leaves the state resetted for the next round
            hmac.DoFinal(prk, 0);

            //DEBUG
            //Console.WriteLine("PRK:  " + BitConverter.ToString(prk).Replace("-", ""));

            //IF DEBUG
            System.Buffer.BlockCopy(prk, 0, tmp_result, 0, tmp_result.Length);
            StringBuilder strBuilderDebug = new StringBuilder();
            StringBuilder strBuilderPresDebug = new StringBuilder();
            strBuilderDebug.Append(Resources.PRKDebugTextTemplate);
            strBuilderPresDebug.Append(Resources.PresPRKDebugTextTemplate);

            pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
            {
                pres.txtIterationRounds.Text = "";
                pres.txtIterationRounds.Inlines.Add(new Run(Resources.PresIterationPRKCalc.Replace("{0}", System.Text.Encoding.UTF8.GetString(skm)).Replace("{1}", System.Text.Encoding.UTF8.GetString(salt))));
                pres.imgIterationPRK.Visibility = Visibility.Visible;
            }, null);

            if (show && !pres.SkipIntro)
            {
                buttonEvent = pres.buttonNextClickedEvent;
                buttonEvent.WaitOne();
            }

            //Generate formatted output for debug output textfield
            string tmp = "";
            for (int j = 1, k = 1; j <= hmac.GetMacSize(); j++)
            {
                tmp += BitConverter.ToString(tmp_result, (j - 1), 1).Replace("-", "") + " ";
                if (j % 8 == 0)
                {
                    strBuilderDebug.Replace("{" + k + "}", tmp);
                    strBuilderPresDebug.Replace("{" + k + "}", tmp);
                    k++;
                    tmp = "";
                }
            }

            pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
            {
                pres.txtIterationDebugOutput.Text = strBuilderPresDebug.ToString();
            }, null);

            _keyMaterialDebug = strBuilderDebug.ToString();
            OnPropertyChanged("KeyMaterialDebug");

            ProgressChanged(0.1, 1);

            if (show && !pres.SkipIntro)
            {
                buttonEvent = pres.buttonNextClickedEvent;
                buttonEvent.WaitOne();
            }

            //Expand
            //prepare input array
            System.Buffer.BlockCopy(ctxInfo, 0, input, 0, ctxInfo.Length);
            input[ctxInfo.Length] = CTR;
            hmac.Init(new KeyParameter(prk));
            //update internal state
            hmac.BlockUpdate(input, 0, ctxInfo.Length + 1);
            //finish the hmac: leaves the state resetted for the next round
            hmac.DoFinal(km, 0);

            //IF DEBUG
            System.Buffer.BlockCopy(km, 0, tmp_result, 0, tmp_result.Length);
            strBuilderDebug = new StringBuilder();
            strBuilderPresDebug = new StringBuilder();
            StringBuilder strBuilderPresTxt = new StringBuilder();
            strBuilderDebug.Append(Resources.KeyMaterialDebugTextTemplate);
            strBuilderPresDebug.Append(Resources.PresKeyMaterialDebugTextTemplate);
            strBuilderPresTxt.Append(Resources.PresIterationRounds);

            //Generate formatted output for debug output textfield
            tmp = "";
            for (int j = 1, k = 1; j <= hmac.GetMacSize(); j++)
            {
                tmp += BitConverter.ToString(tmp_result, (j - 1), 1).Replace("-", "") + " ";
                if (j % 8 == 0)
                {
                    strBuilderDebug.Replace("{" + k + "}", tmp);
                    strBuilderPresDebug.Replace("{" + k + "}", tmp);

                    k++;
                    tmp = "";
                }
            }

            prgs_Curval += prgs_step;
            ProgressChanged(prgs_Curval, 1);

            pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
            {
                pres.imgIterationPRK.Visibility = Visibility.Hidden;
                pres.imgIterationKM1.Visibility = Visibility.Visible;
                pres.txtIterationRounds.Text = "";
                pres.txtIterationRounds.Inlines.Add(new Run(Resources.PresIterationRounds.Replace("{0}", "1").Replace("{1}", N.ToString()).Replace("{2}", BitConverter.ToString(prk).Replace("-", ""))
                    .Replace("{3}", System.Text.Encoding.UTF8.GetString(skm)).Replace("{4}", System.Text.Encoding.UTF8.GetString(ctxInfo)).Replace("{5}", CTR.ToString())));
                pres.txtIterationDebugOutput.Text = strBuilderPresDebug.ToString().Replace("{0}", "1");
            }, null);

            _keyMaterialDebug = strBuilderDebug.ToString().Replace("{0}", "1");
            OnPropertyChanged("KeyMaterialDebug");

            if (show && !pres.SkipIntro)
            {
                buttonEvent = pres.buttonNextClickedEvent;
                buttonEvent.WaitOne();
            }

            prgs_Curval += prgs_step;
            ProgressChanged(prgs_Curval, 1);

            CTR++;
            //DEBUG
            //Console.WriteLine("K(1): " + BitConverter.ToString(km, 0, 32).Replace("-", ""));

            for (int i = 1; i < N; i++, CTR++)
            {
                ProgressChanged(prgs_Curval, 1);
                prgs_Curval += prgs_step;
                //prepare input for next round
                System.Buffer.BlockCopy(km, (i - 1) * hmac.GetMacSize(), input, 0, hmac.GetMacSize());
                System.Buffer.BlockCopy(ctxInfo, 0, input, hmac.GetMacSize(), ctxInfo.Length);
                input[input.Length - 1] = CTR;

                //calc hmac
                hmac.Init(new KeyParameter(prk));
                //update internal state
                hmac.BlockUpdate(input, 0, input.Length);
                //finish the hmac: leaves the state resetted for the next round
                hmac.DoFinal(km, i * hmac.GetMacSize());


                System.Buffer.BlockCopy(km, i * hmac.GetMacSize(), tmp_result, 0, tmp_result.Length);
                strBuilderDebug = new StringBuilder();
                strBuilderPresDebug = new StringBuilder();
                strBuilderPresTxt = new StringBuilder();
                strBuilderDebug.Append(Resources.KeyMaterialDebugTextTemplate);
                strBuilderPresDebug.Append(Resources.PresKeyMaterialDebugTextTemplate);
                strBuilderPresTxt.Append(Resources.PresIterationRounds);

                //Generate formatted output for debug output textfield
                tmp = "";
                for (int j = 1, k = 1; j <= hmac.GetMacSize(); j++)
                {
                    tmp += BitConverter.ToString(tmp_result, (j - 1), 1).Replace("-", "") + " ";
                    if (j % 8 == 0)
                    {
                        strBuilderDebug.Replace("{" + k + "}", tmp);
                        strBuilderPresDebug.Replace("{" + k + "}", tmp);

                        k++;
                        tmp = "";
                    }
                }

                pres.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                {
                    pres.imgIterationKM1.Visibility = Visibility.Hidden;
                    pres.imgIterationKM2.Visibility = Visibility.Visible;
                    pres.txtIterationRounds.Text = "";
                    pres.txtIterationRounds.Inlines.Add(new Run(Resources.PresIterationRounds.Replace("{0}", (i + 1).ToString()).Replace("{1}", N.ToString()).Replace("{2}", BitConverter.ToString(prk).Replace("-", ""))
                        .Replace("{3}", System.Text.Encoding.UTF8.GetString(skm)).Replace("{4}", System.Text.Encoding.UTF8.GetString(ctxInfo)).Replace("{5}", CTR.ToString())));
                    pres.txtIterationDebugOutput.Text = strBuilderPresDebug.ToString().Replace("{0}", (i + 1).ToString());
                }, null);

                _keyMaterialDebug = strBuilderDebug.ToString().Replace("{0}", (i + 1).ToString());
                OnPropertyChanged("KeyMaterialDebug");

                if (show && !pres.SkipIntro && !(i == (N - 1)))
                {
                    buttonEvent = pres.buttonNextClickedEvent;
                    buttonEvent.WaitOne();

                }

                //DEBUG
                //Console.WriteLine("CTR: " + CTR + "\nHash: " + BitConverter.ToString(km, i * hmac.GetMacSize(), hmac.GetMacSize()).Replace("-", ""));
            }

            //truncated output
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
                pres.imgConstructionStep1.Visibility = Visibility.Hidden;
                pres.imgConstructionStep2.Visibility = Visibility.Hidden;

                //Iterationphase
                pres.lblIterationSectionHeading.Visibility = Visibility.Hidden;
                pres.lblIterationHeading.Visibility = Visibility.Hidden;
                pres.txtIterationDebugOutput.Visibility = Visibility.Hidden;

                //Calculation finished
                pres.lblFinishedSectionHeading.Visibility = Visibility.Hidden;
                pres.lblIterationHeading.Visibility = Visibility.Hidden;
                pres.txtIterationRounds.Visibility = Visibility.Hidden;
                pres.imgIterationPRK.Visibility = Visibility.Hidden;
                pres.imgIterationKM1.Visibility = Visibility.Hidden;
                pres.imgIterationKM2.Visibility = Visibility.Hidden;

                //Last 
                pres.lblFinishedSectionHeading.Visibility = Visibility.Hidden;
                pres.txtFinished.Visibility = Visibility.Hidden;

                //Error
                pres.txtError.Visibility = Visibility.Hidden;
                /**/
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
                        pres.txtConstructionText1.Visibility = Visibility.Hidden;
                        pres.txtConstructionText2.Visibility = Visibility.Visible;
                        pres.txtConstructionScheme.Visibility = Visibility.Hidden;
                        pres.imgConstructionStep1.Visibility = Visibility.Visible;


                    }, null);

                    buttonNextClickedEvent = pres.buttonNextClickedEvent;
                    buttonNextClickedEvent.WaitOne();
                }

                //Block: Construction section part 2
                if (!pres.SkipIntro)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.lblConstructionHeading.Visibility = Visibility.Visible;
                        pres.txtConstructionText2.Visibility = Visibility.Hidden;
                        pres.imgConstructionStep1.Visibility = Visibility.Hidden;
                        pres.txtConstructionText3.Visibility = Visibility.Visible;


                    }, null);

                    buttonNextClickedEvent = pres.buttonNextClickedEvent;
                    buttonNextClickedEvent.WaitOne();
                }

                //Block: Construction section part 3
                if (!pres.SkipIntro)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.lblConstructionHeading.Visibility = Visibility.Visible;
                        pres.imgConstructionStep1.Visibility = Visibility.Hidden;
                        pres.txtConstructionText2.Visibility = Visibility.Hidden;
                        pres.txtConstructionText3.Visibility = Visibility.Hidden;
                        pres.txtConstructionText4.Visibility = Visibility.Visible;


                    }, null);

                    buttonNextClickedEvent = pres.buttonNextClickedEvent;
                    buttonNextClickedEvent.WaitOne();
                }

                //Block: Construction section part 4
                if (!pres.SkipIntro)
                {
                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        pres.lblConstructionHeading.Visibility = Visibility.Visible;
                        pres.imgConstructionStep2.Visibility = Visibility.Visible;

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
                    pres.txtConstructionText4.Visibility = Visibility.Hidden;
                    pres.imgConstructionStep1.Visibility = Visibility.Hidden;
                    pres.imgConstructionStep2.Visibility = Visibility.Hidden;

                    //Buttons
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
                    pres.txtIterationRounds.Text = "";
                    pres.txtIterationRounds.Visibility = Visibility.Visible;
                    pres.txtIterationDebugOutput.Text = "";
                    pres.txtIterationDebugOutput.Visibility = Visibility.Visible;

                }, null);

            }

            //Convert inputstrings into byte arrays
            byte[] skm = Encoding.UTF8.GetBytes(_skm);
            byte[] salt = Encoding.UTF8.GetBytes(_salt);
            byte[] ctxinfo = Encoding.UTF8.GetBytes(_ctxInfo);
            byte[] result;
            try
            {
                
                if (settings.InfinityOutput)
                {
                    result = computeHKDFSHA256_8BitCTR(skm, ctxinfo, salt, _outputBytes, settings.DisplayPres, buttonNextClickedEvent);
                }
                else
                {
                    result = computeHKDFSHA256_32BitCTR(skm, ctxinfo, salt, _outputBytes, settings.DisplayPres, buttonNextClickedEvent);
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
            catch (TooMuchOutputRequestedException ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.imgIterationPRK.Visibility = Visibility.Hidden;
                    pres.imgIterationKM1.Visibility = Visibility.Hidden;
                    pres.imgIterationKM2.Visibility = Visibility.Hidden;
                    pres.lblIterationSectionHeading.Visibility = Visibility.Hidden;
                    pres.lblIterationHeading.Visibility = Visibility.Hidden;
                    pres.txtIterationRounds.Visibility = Visibility.Hidden;
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
                    pres.imgIterationPRK.Visibility = Visibility.Hidden;
                    pres.imgIterationKM1.Visibility = Visibility.Hidden;
                    pres.imgIterationKM2.Visibility = Visibility.Hidden;
                    pres.lblIterationSectionHeading.Visibility = Visibility.Hidden;
                    pres.lblIterationHeading.Visibility = Visibility.Hidden;
                    pres.txtIterationRounds.Visibility = Visibility.Hidden;
                    pres.txtIterationDebugOutput.Visibility = Visibility.Hidden;
                    pres.txtError.Visibility = Visibility.Visible;
                }, null);
                return;
            }

            if (settings.DisplayPres)
            {
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.imgIterationPRK.Visibility = Visibility.Hidden;
                    pres.imgIterationKM1.Visibility = Visibility.Hidden;
                    pres.imgIterationKM2.Visibility = Visibility.Hidden;
                    pres.buttonNext.IsEnabled = false;
                    pres.buttonSkipCalc.IsEnabled = false;
                    pres.lblIterationHeading.Visibility = Visibility.Hidden;
                    pres.txtIterationRounds.Visibility = Visibility.Hidden;
                    pres.txtIterationDebugOutput.Visibility = Visibility.Hidden;
                    pres.lblFinishedSectionHeading.Visibility = Visibility.Visible;
                    pres.txtFinished.Visibility = Visibility.Visible;
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
        /// Input for ctxInfo
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputCtxInfoCaption", "InputCtxInfoToolTip", true)]
        public string CTXInfo
        {
            get
            {
                return _ctxInfo;
            }
            set
            {
                _ctxInfo = value;
            }
        }

        /// <summary>
        /// Input for the salt
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputSaltCaption", "InputSaltToolTip", true)]
        public string Salt
        {
            get
            {
                return _salt;
            }
            set
            {
                _salt = value;
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
                pres.txtConstructionText4.Visibility = Visibility.Hidden;
                pres.imgConstructionStep1.Visibility = Visibility.Hidden;
                pres.imgConstructionStep2.Visibility = Visibility.Hidden;
                
                //Iterationphase
                pres.lblIterationSectionHeading.Visibility = Visibility.Hidden;
                pres.txtIterationRounds.Visibility = Visibility.Hidden;
                pres.lblIterationHeading.Visibility = Visibility.Hidden;
                pres.txtIterationDebugOutput.Visibility = Visibility.Hidden;
                pres.imgIterationPRK.Visibility = Visibility.Hidden;
                pres.imgIterationKM1.Visibility = Visibility.Hidden;
                pres.imgIterationKM2.Visibility = Visibility.Hidden;

                //Calculation finished
                pres.lblIterationSectionHeading.Visibility = Visibility.Hidden;

                //Last
                pres.lblFinishedSectionHeading.Visibility = Visibility.Hidden;
                pres.txtFinished.Visibility = Visibility.Hidden;

                //Error
                pres.txtError.Visibility = Visibility.Hidden;
                /**/
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
