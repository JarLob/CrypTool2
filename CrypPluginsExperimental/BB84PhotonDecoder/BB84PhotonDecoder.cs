﻿/*
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
using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System;
using BB84PhotonDecoder;
using System.Windows.Threading;
using System.Threading;
using System.Text;
using System.Security.Cryptography;

namespace Cryptool.Plugins.BB84PhotonDecoder
{
    [Author("Benedict Beuscher", "benedict.beuscher@stud.uni-due.de", "Uni Duisburg-Essen", "http://www.uni-due.de/")]

    [PluginInfo("Cryptool.Plugins.BB84PhotonDecoder.Properties.Resources", "res_photonDecodingCaption", "res_photonDecodingTooltip", "BB84PhotonDecoder/userdoc.xml", new[] { "CrypWin/images/default.png" })]
    
    [ComponentCategory(ComponentCategory.Protocols)]
    public class BB84PhotonDecoder : ICrypComponent
    {
        #region Private Variables

        public bool synchron;

        public bool listened;
        public bool faster;

        private readonly BB84PhotonDecoderSettings settings = new BB84PhotonDecoderSettings();
        private string inputPhotons;
        private string inputBases;
        private string outputKey;
        private System.Random newRandom;
        private BB84PhotonDecoderPresentation myPresentation;
        
        private double errorRatio;
        private RNGCryptoServiceProvider sRandom;

        #endregion

        public BB84PhotonDecoder()
        {
            synchron = true;
            myPresentation = new BB84PhotonDecoderPresentation();
            myPresentation.UpdateProgess += new EventHandler(update_progress);
            Presentation = myPresentation;
            errorRatio = 0.12; //###
        }
        private void update_progress(object sender, EventArgs e)
        {
            ProgressChanged(myPresentation.Progress, 3000);
        }
        #region Data Properties

        [PropertyInfo(Direction.InputData, "res_photonInputCaption", "res_photonInputTooltip")]
        public string InputPhotons
        {
            get
            {
                return this.inputPhotons;
            }
            set
            {
                this.inputPhotons = value;
            }
        }

        [PropertyInfo(Direction.InputData, "res_basesInputCaption", "res_basesInputTooltip")]
        public string InputBases
        {
            get
            {
                return this.inputBases;
            }
            set
            {
                this.inputBases = value;
            }
        }

        [PropertyInfo(Direction.OutputData, "res_keyOutputCaption", "res_keyOutputTooltip")]
        public string OutputKey
        {
            get
            {
                return this.outputKey;
            }
            set
            { } //readonly
        }
        #endregion

        #region IPlugin Members

        public UserControl Presentation
        {
            get;
            private set;
        }

        public ISettings Settings
        {
            get { return settings; }
        }


        public void PreExecution()
        {
            myPresentation.SpeedFactor = settings.SpeedSetting;
        }

        public void Execute()
        {
            ProgressChanged(0, 1);
           
            checkIfListened();
           

            if (settings.ErrorsEnabled == 0 || (int)Math.Round(inputPhotons.Length * errorRatio) == 0)
            {
                doNormalDecoding();
                
            }
                
            else
            {
                doDecodingWithErrors();
            }

            OnPropertyChanged("OutputKey");


            showPresentationIfVisible();

            

            ProgressChanged(1, 1);
        }

        private void showPresentationIfVisible()
        {
            if (Presentation.IsVisible)
            {
                Presentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        try
                        {
                            if (!myPresentation.hasFinished)
                            {
                                myPresentation.StopPresentation();
                            }

                            if (synchron)
                            {
                                if (!faster)
                                {
                                    myPresentation.StartPresentation("WW" + outputKey, "WW" + inputPhotons, "WW" + inputBases);
                                }
                                else
                                {
                                    myPresentation.StartPresentation("W" + outputKey, "W" + inputPhotons, "W" + inputBases);
                                }
                            }
                            else
                            {
                                myPresentation.StartPresentation(outputKey, inputPhotons, inputBases);
                            }

                            

                        }
                        catch (Exception e)
                        {
                            GuiLogMessage("Problem beim Ausführen des Dispatchers :" + e.Message, NotificationLevel.Error);
                        }
                
                    }, null);

                
                while (!myPresentation.hasFinished)
                {
                    ProgressChanged(myPresentation.animationRepeats, inputBases.Length);
                }

                

            }
        }

        private void checkIfListened()
        {
            if (inputPhotons[inputPhotons.Length-1].Equals('L'))
            {
                listened = true;
                faster = false;
                inputPhotons = inputPhotons.Substring(0, inputPhotons.Length - 1);
            }
            else if (inputPhotons[inputPhotons.Length-1].Equals('S'))
            {
                listened = false;
                faster = false;
                inputPhotons = inputPhotons.Substring(0, inputPhotons.Length - 1);
            }
            else if (inputPhotons[inputPhotons.Length - 1].Equals('X'))
            {
                listened = false;
                faster = true;
            }

           

            
            
        }

        private void doDecodingWithErrors()
        {
            StringBuilder tempOutput = new StringBuilder();
            newRandom = new System.Random(DateTime.Now.Millisecond);
            newRandom.Next(2);
            char[] tempBases = inputBases.ToCharArray();
            char[] tempPhotons = inputPhotons.ToCharArray();

            int fail = (int)Math.Round(inputPhotons.Length * errorRatio);

            for (int i = 0; i < inputPhotons.Length; i++)
            {

                if (i % fail == 0)
                {
                    tempOutput.Append(getRandomBinary());
                }
                else
                {
                    if (tempBases.Length > i && tempPhotons.Length > i)
                    {
                        if (tempBases[i].Equals('+'))
                        {
                            if (tempPhotons[i].Equals('|'))
                            {
                                tempOutput.Append(settings.PlusVerticallyDecoding);
                            }
                            else if (tempPhotons[i].Equals('-'))
                            {
                                tempOutput.Append(settings.PlusHorizontallyDecoding);
                            }
                            else
                                tempOutput.Append(getRandomBinary());
                        }
                        else if (tempBases[i].Equals('x'))
                        {
                            if (tempPhotons[i].Equals('\\'))
                            {
                                tempOutput.Append(settings.XTopLeftDiagonallyDecoding);
                            }
                            else if (tempPhotons[i].Equals('/'))
                            {
                                tempOutput.Append(settings.XTopRightDiagonallyDecoding);
                            }
                            else
                                tempOutput.Append(getRandomBinary());
                        }
                    }
                }
            }
            this.outputKey = tempOutput.ToString();
        }

        private void doNormalDecoding()
        {
            StringBuilder tempOutput= new StringBuilder();
            newRandom = new System.Random(DateTime.Now.Millisecond);
            newRandom.Next(2);
            char[] tempBases = inputBases.ToCharArray();
            char[] tempPhotons = inputPhotons.ToCharArray();

            for (int i = 0; i < inputPhotons.Length; i++)
            {
                if (tempBases.Length > i && tempPhotons.Length > i)
                {
                    if (tempBases[i].Equals('+'))
                    {
                        if (tempPhotons[i].Equals('|'))
                        {
                            tempOutput.Append(settings.PlusVerticallyDecoding);
                        }
                        else if (tempPhotons[i].Equals('-'))
                        {
                            tempOutput.Append(settings.PlusHorizontallyDecoding);
                        }
                        else
                            tempOutput.Append(getRandomBinary());
                    }
                    else if (tempBases[i].Equals('x'))
                    {
                        if (tempPhotons[i].Equals('\\'))
                        {
                            tempOutput.Append(settings.XTopLeftDiagonallyDecoding);
                        }
                        else if (tempPhotons[i].Equals('/'))
                        {
                            tempOutput.Append(settings.XTopRightDiagonallyDecoding);
                        }
                        else
                            tempOutput.Append(getRandomBinary());
                    }
                }
            }
            this.outputKey = tempOutput.ToString();
        }

        private string getRandomBinary()
        {     
                sRandom = new RNGCryptoServiceProvider();
                byte[] buffer = new byte[4];
                sRandom.GetBytes(buffer);
                int result = BitConverter.ToInt32(buffer, 0);
                string returnString = "" + new Random(result).Next(2);
                return returnString;

        }

        public void PostExecution()
        {
            Presentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    myPresentation.StopPresentation();
                }
                catch (Exception e)
                {
                    GuiLogMessage("Problem beim Ausführen des Dispatchers :" + e.Message, NotificationLevel.Error);
                }
            }, null);
        }

        public void Stop()
        {
            Presentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    myPresentation.StopPresentation();
                }
                catch (Exception e)
                {
                    GuiLogMessage("Problem beim Ausführen des Dispatchers :" + e.Message, NotificationLevel.Error);
                }
            }, null);
            
        }

        public void Initialize()
        {
            settings.PlusVerticallyDecoding = "0";
            settings.XTopRightDiagonallyDecoding = "0";
            settings.XTopLeftDiagonallyDecoding = "1";
            settings.PlusHorizontallyDecoding = "1";
            settings.SpeedSetting = 1;
        }

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
