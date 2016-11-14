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
using System.Collections.Generic;
using System.Threading;
using Cryptool.PluginBase.IO;
using System.Windows.Threading;
using System.Text;
using AvalancheVisualization;
using System.Linq;
using AvalancheVisualization.Properties;

namespace Cryptool.Plugins.AvalancheVisualization
{
    // HOWTO: Change author name, email address, organization and URL.
    [Author("Camilo Echeverri", "caeche20@hotmail.com", "Universität Mannheim", "http://cryptool2.vs.uni-due.de")]
    // HOWTO: Change plugin caption (title to appear in CT2) and tooltip.
    // You can (and should) provide a user documentation as XML file and an own icon.
    [PluginInfo("AvalancheVisualization.Properties.Resources","PluginCaption", "AvalancheTooltip", "AvalancheVisualization/userdoc.xml", new[] { "AvalancheVisualization/Images/Avalanche.png" })]
    // HOWTO: Change category to one that fits to your plugin. Multiple categories are allowed.
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class AvalancheVisualization : ICrypComponent
    {

        // HOWTO: You need to adapt the settings class as well, see the corresponding file.


        #region Private Variables

        private readonly AvalancheVisualizationSettings settings = new AvalancheVisualizationSettings();
        private ICryptoolStream text;
        private ICryptoolStream key;
        private ICryptoolStream outputStream;

        byte[] originalText;
        byte[] originalKey;
        byte[] textInput;
        byte[] keyInput;
        string msgA;
        string msgB;
        

        private AES aes = new AES();
        private DES des = new DES();
        private bool textChanged = false;
        private Thread outputThread;
        private Thread desThread;
        private AvalanchePresentation pres = new AvalanchePresentation();
        private AutoResetEvent buttonNextClickedEvent;
        private AutoResetEvent end;
        private int count = 0;
        private bool isRunning;
        private byte[] initialDES;
        private byte[] current;
        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "InputKey", "InputKeyDescription", false)]
        public ICryptoolStream Key
        {
            get
            {
                return key;
            }
            set
            {
                this.key = value;
                OnPropertyChanged("Key");
            }
        }

        [PropertyInfo(Direction.InputData, "InputMessage", "InputMessageDescription", true)]
        public ICryptoolStream Text
        {
            get
            {
                return text;
            }
            set
            {
                this.text = value;
                OnPropertyChanged("Text");
            }
        }


        [PropertyInfo(Direction.OutputData, "Output", "OutputTooltip", false)]
        public ICryptoolStream OutputStream
        {
            get
            {
                return outputStream;
            }
            set
            {
                this.outputStream = value;
                OnPropertyChanged("OutputStream");
            }
        }



        /// <summary>
        /// HOWTO: Output interface to write the output data.
        /// You can add more output properties ot other type if needed.
        /// </summary>
        /* [PropertyInfo(Direction.OutputData, "OutputStreamCaption", "OutputStreamTooltip", false)]
         public ICryptoolStream OutputStream
         {
             get
             {
                 return outputStream;
             }
             set
             {
                 this.outputStream = value;
                 // empty
             }
         }*/

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




        public static string bytesToHexString(byte[] byteArr)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (byte b in byteArr)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }
        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        /// 
        public void d_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {


            GuiLogMessage("property changed", NotificationLevel.Info);
            //Console.WriteLine(string.Format("Property {0} just changed", e.PropertyName));
        }


        public void Execute()
        {

           
            // byte[] buffer = new byte[UnchangedCipher.Length];
            textInput = new byte[Text.Length];
            isRunning = true;

            switch (settings.SelectedCategory)
            {
                case AvalancheVisualizationSettings.Category.Prepared:

                    buttonNextClickedEvent= pres.buttonNextClickedEvent;
                    end = pres.end;

                    keyInput = new byte[Key.Length];

                    using (CStreamReader reader = Text.CreateReader())
                    {
                        reader.Read(textInput);
                    }

                    using (CStreamReader reader = Key.CreateReader())
                    {
                        reader.Read(keyInput);
                    }



                    if (settings.PrepSelection == 0)
                    {
                        ProgressChanged(0,0);
                        outputThread = new Thread(streamOut);
                        Thread endThread = new Thread(progress);
                        //Thread oThread = new Thread(streamOut);
                        pres.mode = 0;

                        bool valid =validSize();

                        string inputMessage = Encoding.Default.GetString(textInput);

                        if (valid)
                        {
                            //new
                            

                            if (textChanged && pres.canModify)
                            {
                               

                                pres.newText = null;
                                pres.newKey = null;
                               
                                aes.text = textInput;
                                aes.key = keyInput;

                              
                                byte[] temporary = aes.checkTextLength();
                                byte[] tmpKey = aes.checkKeysize();
                                pres.key = tmpKey;
                                pres.textB = temporary;
                                aes.executeAES(false);


                                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback) delegate
                                {
                                    pres.setAndLoadButtons();
                            
                                    pres.loadChangedMsg(temporary, true);
                          
                                    pres.loadChangedKey(tmpKey);

                                    pres.coloringText();
                                    pres.coloringKey();
                                    pres.updateDataColor();
                                }, null);

                                pres.statesB = aes.statesB;

                                buttonNextClickedEvent.Set();


                                
                                Thread oThread = new Thread(streamOut);
                                oThread.Start();
                               
                               
                                
                                //oThread.Start();
                                ///////////////////////////////////////////////////////

                            }
                            else if (!textChanged && !pres.canModify)
                            {
                                

                                textChanged = true;

                                originalText = textInput;

                                aes.text = textInput;
                                aes.key = keyInput;

                                pres.keysize = settings.KeyLength;


                                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback) delegate
                                {
                                    if (pres.skip.IsChecked == true)
                                        pres.comparisonPane();
                                    else
                                        pres.instructions();
                                 


                                }, null);



                                AES.keysize = settings.KeyLength;
                                byte[] tmpKey = aes.checkKeysize();
                                originalKey = tmpKey;
                                pres.key = tmpKey;
                                pres.keyA = tmpKey;
                                byte[] temporary = aes.checkTextLength();
                                pres.textA = temporary;
                                aes.executeAES(true);
                                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback) delegate
                                {

                                    pres.loadInitialState(temporary, tmpKey);


                                }, null);

                                pres.states = aes.states;
                                pres.keyList = aes.keyList;

                                outputThread.Start();
                            }

                            /*  if (pres.IsVisible)
                              {
                                  while (isRunning)
                                      ProgressChanged(0.5, 1);

                                  if (pres.end)
                                  isRunning=false;
                              }*/
                            ProgressChanged(0.5, 1);
                          endThread.Start();
                           
                            if (!running)
                                return;

                        }
                    }
                    // if settings==1
                    else
                    {
                        desThread = new Thread(streamOut);
                        Thread endThread = new Thread(progress);
                        pres.mode = 1;

                        bool valid = validSize();

                        if (valid)
                        {

                            if (textChanged && pres.canModifyDES)
                            {

                      
                                pres.newText = null;
                                pres.newKey = null;

                                des.inputKey = keyInput;
                                des.inputMessage = textInput;

                                des.textChanged = true;
                                des.DESProcess();
                                pres.key = keyInput;
                                pres.textB = textInput;
                                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback) delegate
                                {

                                    pres.setAndLoadButtons();


                                    pres.loadChangedMsg(textInput, true);

                                    pres.loadChangedKey(keyInput);

                                    pres.coloringText();
                                    pres.coloringKey();

                                    pres.updateDataColor();
                                }, null);


                                pres.lrDataB = des.lrDataB;
                                current = des.outputCiphertext;

                                buttonNextClickedEvent.Set();

                                Thread oDesThread = new Thread(streamOut);
                                oDesThread.Start();
                            }

                            else if (!textChanged && !pres.canModifyDES)
                            {


                                des.inputKey = keyInput;
                                des.inputMessage = textInput;


                                byte[] tmpKey = keyInput;
                                byte[] tmpText = textInput;
                                originalText = tmpText;
                                originalKey = tmpKey;

                                des.textChanged = false;

                                des.DESProcess();

                            
                                pres.keyA = tmpKey;
                                textChanged = true;
                                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback) delegate
                                {
                                    if (pres.skip.IsChecked == true)
                                        pres.comparisonPane();
                                    else
                                        pres.instructions();

                                    pres.loadInitialState(textInput, keyInput);
             

                                }, null);
                         
                                pres.textA = textInput;
                                pres.lrData = des.lrData;
                                initialDES = des.outputCiphertext;
                                desThread.Start();
                            }

                            endThread.Start();

                            if (!running)
                                return;
                        }
                    }

                  

                    break;



                case AvalancheVisualizationSettings.Category.Unprepared:





                    switch (settings.UnprepSelection)
                    {
                        //Hash functions
                        case 0:



                            //      MemoryStream mStream = new MemoryStream();


                            if (pres.mode != 4)
                                pres.mode = 2;

                            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                            {

                                using (CStreamReader reader = Text.CreateReader())
                                {
                                    reader.Read(textInput);

                                }


                                //string otherText = Encoding.Default.GetString(buffer);


                                if (textChanged)
                                {
                                    string cipherB = pres.binaryAsString(textInput);

                                    if(pres.radioDecOthers.IsChecked==true)
                                        pres.modifiedMsg.Text = pres.decimalAsString(textInput);
                                    if(pres.radioHexOthers.IsChecked==true)
                                    pres.modifiedMsg.Text = pres.hexaAsString(textInput);

                                    pres.TB2.Text = cipherB;
                                    pres.changedCipher = textInput;
                                  
                                    pres.comparison();



                                    using (CStreamWriter CSWriter2 = new CStreamWriter())
                                    {
                                        OutputStream = CSWriter2;

                                        for (int i = 0; i < 2; i++)
                                            CSWriter2.Write(generatedData(i));

                                        OnPropertyChanged("OutputStream");
                                        CSWriter2.Close();
                                    }


                                }
                                else
                                {
                                    if (pres.skip.IsChecked == true)
                                        pres.comparisonPane();
                                    else
                                        pres.instructions();

                                    originalText = textInput;
                                    string cipherA = pres.binaryAsString(textInput);
                                    pres.originalMsg.Text = pres.hexaAsString(textInput);
                                    pres.TB1.Text = cipherA;
                                    pres.unchangedCipher = textInput;
                                    pres.radioHexOthers.IsChecked = true;

                                }


                            }, null);

                            textChanged = true;
                            break;

                        //classic
                        case 1:

                            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                            {
                              
                                pres.mode = 3;

                                using (CStreamReader reader = Text.CreateReader())
                                {
                                    reader.Read(textInput);

                                }



                                string otherText = Encoding.Default.GetString(textInput);
                               

                                if (textChanged)
                                {
                                    msgB = otherText;
                                    bool validEntry = checkSize(msgA, msgB);

                                    if (validEntry)
                                    {
                                        
                                        string cipherB = pres.binaryAsString(textInput);

                                        if(pres.radioText.IsChecked == true)
                                        pres.modifiedMsg.Text = otherText;
                                        if (pres.radioDecOthers.IsChecked == true)
                                            pres.modifiedMsg.Text = pres.decimalAsString(textInput);
                                        if (pres.radioHexOthers.IsChecked == true)
                                            pres.modifiedMsg.Text = pres.hexaAsString(textInput);

                                        pres.TB2.Text = cipherB;
                                        pres.changedCipher = textInput;
                                       
                                        pres.comparison();

                                        using (CStreamWriter CSWriter2 = new CStreamWriter())
                                        {
                                            OutputStream = CSWriter2;

                                            for (int i = 0; i < 2; i++)
                                                CSWriter2.Write(generatedData(i));

                                            OnPropertyChanged("OutputStream");
                                            CSWriter2.Close();
                                        }
                                    }
                                    else
                                    {
                                        GuiLogMessage(Resources.Warning, NotificationLevel.Warning);
                                    }
                                }
                                else
                                {
                                    if (pres.skip.IsChecked == true)
                                        pres.comparisonPane();
                                    else
                                        pres.instructions();
                                   
                                    string cipherA = pres.binaryAsString(textInput);
                                    pres.originalMsg.Text = otherText;
                                    msgA = otherText;
                                    pres.TB1.Text = cipherA;
                                    pres.unchangedCipher = textInput;
                                    pres.radioText.IsChecked = true;
                                }



                            }, null);

                            textChanged = true;
                            break;

                        //modern
                        case 2:
                            pres.mode = 4;
                            goto case 0;

                        default:
                            break;

                    }

                    break;

                default:
                    break;
            }
        }

        bool running = false;
        bool stop = true;
        public void PostExecution()
        {
            // running = false;
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
            textChanged = false;
            //if (pres.mode == 0 || pres.mode == 1)
            //buttonNextClickedEvent.Set();
             CStreamWriter writer = new CStreamWriter();



            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {

                pres.removeElements();
                // pres.modifyTxtBlock.Visibility = Visibility.Visible;
                //textChanged = false;
                //pres.modifiedMsg.IsReadOnly = false;

            }, null);


            using (writer)
            {

                byte[] outputArr = null;
                OutputStream = writer;


                writer.Write(outputArr);


                OnPropertyChanged("OutputStream");

            }
            writer.Close();
            
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
            //aes.pres = this.pres;

        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion

        #region Methods

        public void progress()
        {
            end.WaitOne();
            GuiLogMessage(Resources.End,NotificationLevel.Info);
            ProgressChanged(1, 1);
        }

        public void streamOut()
        {
            
           

                using (CStreamWriter writer = new CStreamWriter())
                {

                    OutputStream = writer;
                    buttonNextClickedEvent.WaitOne();

                    writer.Write(generatedData(0));
                    writer.Write(generatedData(1));
                writer.Write(generatedData(2));

                OnPropertyChanged("OutputStream");
                    writer.Close();
                }
            

      
        }

        public string[] sequence(Tuple<string, string> strTuple)
        {
            string[] diffBits = new string[strTuple.Item1.Length];


            for (int i = 0; i < strTuple.Item1.Length; i++)
            {
                if (strTuple.Item1[i] != strTuple.Item2[i])
                {
                    diffBits[i] = "X";
                }
                else
                    diffBits[i] = " ";

            }

            string[] differentBits = diffBits;

            return differentBits;
        }

        public Tuple<string, string> tupleDES(int roundDES)
        {
           string encryptionStateA = pres.lrData[roundDES, 0] + pres.lrData[roundDES, 1];
           string encryptionStateB = pres.lrDataB[roundDES, 0] + pres.lrDataB[roundDES, 1];

            var tuple = new Tuple<string, string>(encryptionStateA, encryptionStateB);

            return tuple;
        }

        public byte[] generatedData(int pos)
        {
            List<byte[]> bl = new List<byte[]>();


            switch (settings.SelectedCategory)

            {
                case AvalancheVisualizationSettings.Category.Prepared:

                  

                    if (settings.PrepSelection == 0)
                    {
                        string initialtxt = string.Format(Resources.OutputInitialAESMsg, Environment.NewLine);
                        string modifiedtxt = string.Format(Resources.OutputModifiedAESMsg, Environment.NewLine);
                        string initialkey = string.Format(Resources.OutputInitialAESKey, Environment.NewLine);
                        string modifiedkey = string.Format(Resources.OutputModifiedAESKey, Environment.NewLine);


                        string inputMessage = Encoding.ASCII.GetString(originalText);



                        string initial = string.Format("{0}{1}", inputMessage, Environment.NewLine);
                        string initialk = string.Format("{0}{1}{2}", pres.hexaAsString(originalKey), Environment.NewLine, Environment.NewLine);
                        string modified = "";
                        string modifiedk = "";

                        if (pres.newText != null && pres.newKey != null)

                        {
                            string inputMessageB = Encoding.ASCII.GetString(pres.newText);
                            modified = string.Format("{0}{1}", inputMessageB, Environment.NewLine);
                            modifiedk = string.Format("{0}{1}{2}", pres.hexaAsString(pres.newKey), Environment.NewLine, Environment.NewLine);


                        }

                        else

                        {
                            string inputMessageB = Encoding.ASCII.GetString(textInput);
                            modified = string.Format("{0}{1}", inputMessageB, Environment.NewLine);
                            modifiedk = string.Format("{0}{1}{2}", pres.hexaAsString(keyInput), Environment.NewLine, Environment.NewLine);

                        }


                        byte[] inputArr = Encoding.UTF8.GetBytes(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}", initialtxt, initial, initialkey, initialk, modifiedtxt, modified, modifiedkey, modifiedk));

                        bl.Add(inputArr);

                        List<object> information = new List<object>();
                        List<byte> byteList = new List<byte>();
                        byte[] statsArray = new byte[100];
                        Tuple<string, string> strings;
                        int number = 0;
                        int rounds = 0;

                        switch (settings.KeyLength)
                        {
                            case 0:
                                number = 36;
                               rounds = 11;
                                break;
                            case 1:
                                number = 44;
                                rounds = 13;
                                break;
                            case 2:
                                number = 52;
                                rounds = 15;
                                break;
                            default:
                                break;
                        }

                        for (int aesRound = 0; aesRound <= number; aesRound += 4)
                        {

                            strings = pres.binaryStrings(pres.states[aesRound], pres.statesB[aesRound]);
                            int nrDiffBits = pres.nrOfBitsFlipped(pres.states[aesRound], pres.statesB[aesRound]);
                            double avalanche = pres.calcAvalancheEffect(nrDiffBits, strings);
                            string[] differentBits = sequence(strings);
                            int lengthIdentSequence = pres.longestIdenticalSequence(differentBits);
                            int lengthFlippedSequence = pres.longestFlippedSequence(differentBits);

                            information.Add(nrDiffBits);
                            information.Add(avalanche);
                            information.Add(lengthIdentSequence);
                            information.Add(pres.sequencePosition);
                            information.Add(lengthFlippedSequence);
                            information.Add(pres.flippedSeqPosition);
                        }


                        strings = pres.binaryStrings(pres.states[number + 3], pres.statesB[number + 3]);
                        int nrDiffBits2 = pres.nrOfBitsFlipped(pres.states[number + 3], pres.statesB[number + 3]);
                        double avalanche2 = pres.calcAvalancheEffect(nrDiffBits2, strings);
                        string[] differentBits2 = sequence(strings);
                        int lengthIdentSequence2 = pres.longestIdenticalSequence(differentBits2);
                        int lengthFlippedSequence2 = pres.longestFlippedSequence(differentBits2);

                        information.Add(nrDiffBits2);
                        information.Add(avalanche2);
                        information.Add(lengthIdentSequence2);
                        information.Add(pres.sequencePosition);
                        information.Add(lengthFlippedSequence2);
                        information.Add(pres.flippedSeqPosition);

                        object[] data = information.ToArray();

                        StringBuilder sb = new StringBuilder();

                        int i = 0;

                        for (int round = 0; round < rounds; round++)
                        {

                            sb.AppendFormat(Resources.AfterRound, round, Environment.NewLine);

                            sb.AppendFormat(Resources.OutputStats1, data[i].ToString(), data[i + 1].ToString(), Environment.NewLine);

                            sb.AppendFormat(Resources.OutputStats2, data[i + 2].ToString(), data[i + 3].ToString(), Environment.NewLine);

                            sb.AppendFormat(Resources.OutputStats3, data[i + 4].ToString(), data[i + 5].ToString(), Environment.NewLine);

                            sb.AppendFormat("{0}", Environment.NewLine);

                            i += 6;
                        }



                        string newString = sb.ToString();

                        statsArray = Encoding.UTF8.GetBytes(string.Format("{0}", newString));




                        bl.Add(statsArray);


                        string finalModified = "";
                        string finalInitial = "";
                        string finalAESInitial = string.Format(Resources.AESDerivedFromInit, Environment.NewLine, Environment.NewLine);
                        string finalAESModified = string.Format(Resources.AESDerivedFromMod, Environment.NewLine);

                        finalInitial =  string.Format("{0}{1}", pres.hexaAsString(aes.states[number + 3]), Environment.NewLine);

                        if (pres.newKey!=null)
                            finalModified = string.Format("{0}{1}", pres.hexaAsString(pres.aesDiffusion.statesB[number +3]), Environment.NewLine);
                       else    
                        finalModified = string.Format("{0}{1}", pres.hexaAsString(pres.statesB[number +3]), Environment.NewLine);
                                
                            
        
                        //   if (pres.currentDES != null)
                       //     finalModified = string.Format("{0}{1}", pres.hexaAsString(pres.currentDES), Environment.NewLine);
                        //else
                          



                     byte[] finalArrAES = Encoding.UTF8.GetBytes(string.Format("{0}{1}{2}{3}", finalAESInitial, finalInitial, finalAESModified, finalModified));

                        bl.Add(finalArrAES);
                    }

                    else
                    {
                        string initialtxt = string.Format(Resources.OutputInitialDESMsg, Environment.NewLine);
                        string modifiedtxt = string.Format(Resources.OutputModifiedDESMsg, Environment.NewLine);
                        string initialkey = string.Format(Resources.OutputInitialDESKey, Environment.NewLine);
                        string modifiedkey = string.Format(Resources.OutputModifiedDESKey, Environment.NewLine);
                       
                        string inputMessage = Encoding.ASCII.GetString(originalText);



                        string initial = string.Format("{0}{1}", inputMessage, Environment.NewLine);
                        string initialk = string.Format("{0}{1}{2}", pres.hexaAsString(originalKey), Environment.NewLine, Environment.NewLine);
                        string modified = "";
                        string modifiedk = "";

                        if (pres.newText != null && pres.newKey != null)

                        {
                            string inputMessageB = Encoding.ASCII.GetString(pres.newText);
                            modified = string.Format("{0}{1}", inputMessageB, Environment.NewLine);
                            modifiedk = string.Format("{0}{1}{2}", pres.hexaAsString(pres.newKey), Environment.NewLine, Environment.NewLine);


                        }

                        else

                        {
                            string inputMessageB = Encoding.ASCII.GetString(textInput);
                            modified = string.Format("{0}{1}", inputMessageB, Environment.NewLine);
                            modifiedk = string.Format("{0}{1}{2}", pres.hexaAsString(keyInput), Environment.NewLine, Environment.NewLine);

                        }


                        byte[] inputArr = Encoding.UTF8.GetBytes(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}", initialtxt, initial, initialkey, initialk, modifiedtxt, modified, modifiedkey, modifiedk));

                        bl.Add(inputArr);

                        List<object> information = new List<object>();
                        List<byte> byteList = new List<byte>();
                        byte[] dataArray = new byte[120];
                        Tuple<string, string> strings;
                       

                        for (int desRound = 0; desRound < 17; desRound++)
                        {
                            strings = tupleDES(desRound);
                            pres.toStringArray(desRound);
                            int nrDiffBits = pres.nrOfBitsFlipped(pres.seqA,pres.seqB);
                            double avalanche = pres.calcAvalancheEffect(nrDiffBits, strings);
                            string[] differentBits = sequence(strings);
                            int lengthIdentSequence = pres.longestIdenticalSequence(differentBits);
                            int lengthFlippedSequence = pres.longestFlippedSequence(differentBits);

                            information.Add(nrDiffBits);
                            information.Add(avalanche);
                            information.Add(lengthIdentSequence);
                            information.Add(pres.sequencePosition);
                            information.Add(lengthFlippedSequence);
                            information.Add(pres.flippedSeqPosition);
                        }

                        object[] dat = information.ToArray();

                        StringBuilder sbuilder = new StringBuilder();

                        int j = 0;

                        for (int round = 0; round < 17; round++)
                        {

                            sbuilder.AppendFormat(Resources.AfterRound, round, Environment.NewLine);

                            sbuilder.AppendFormat(Resources.OutputStats1, dat[j].ToString(), dat[j + 1].ToString(), Environment.NewLine);

                            sbuilder.AppendFormat(Resources.OutputStats2, dat[j + 2].ToString(), dat[j + 3].ToString(), Environment.NewLine);

                            sbuilder.AppendFormat(Resources.OutputStats3, dat[j + 4].ToString(), dat[j + 5].ToString(), Environment.NewLine);

                            sbuilder.AppendFormat("{0}", Environment.NewLine);

                            j += 6;
                        }

                        string newStr = sbuilder.ToString();

                        dataArray = Encoding.UTF8.GetBytes(string.Format("{0}", newStr));




                        bl.Add(dataArray);

                        string finalModified = "";
                        string finalDESInitial = string.Format( Resources.DESderivedFromInit,Environment.NewLine, Environment.NewLine);
                        string finalDESModified = string.Format(Resources.DESderivedFromMod, Environment.NewLine);

                        string finalInitial = string.Format("{0}{1}", pres.hexaAsString(initialDES),Environment.NewLine);

                        if(pres.currentDES!= null)
                        finalModified = string.Format("{0}{1}", pres.hexaAsString(pres.currentDES), Environment.NewLine);
                        else
                        finalModified = string.Format("{0}{1}", pres.hexaAsString(current), Environment.NewLine);



                        byte[] finalArr = Encoding.UTF8.GetBytes(string.Format("{0}{1}{2}{3}", finalDESInitial, finalInitial, finalDESModified, finalModified));

                        bl.Add(finalArr);
                    }

                    // return bl[pos].ToArray();
                    break;

                case AvalancheVisualizationSettings.Category.Unprepared:

                    if (settings.UnprepSelection == 0 || settings.UnprepSelection == 2)
                    {

                        string initialCaption = "";
                        string modifiedCaption = "";

                        if (pres.mode == 2)
                        {
                            initialCaption = string.Format(Resources.InitialHashOutput, Environment.NewLine);
                            modifiedCaption = string.Format(Resources.ModHashOutput, Environment.NewLine);
                        }

                        if (pres.mode == 4)
                        {
                            initialCaption = string.Format(Resources.EncryptionInitialMsgOut, Environment.NewLine);
                            modifiedCaption = string.Format(Resources.EncryptionModifiedlMsgOut, Environment.NewLine);

                        }

                        string init = string.Format("{0}{1}", pres.hexaAsString(originalText), Environment.NewLine);
                        string mod = string.Format("{0}{1}{2}", pres.hexaAsString(textInput), Environment.NewLine, Environment.NewLine);

                        byte[] inputArray = Encoding.UTF8.GetBytes(string.Format("{0}{1}{2}{3}", initialCaption, init, modifiedCaption, mod));


                        bl.Add(inputArray);



                        //hash & modern



                        var strings = pres.binaryStrings(pres.unchangedCipher, pres.changedCipher);
                        int bitsFlipped = pres.nrOfBitsFlipped(pres.unchangedCipher, pres.changedCipher);
                        double avalanche = pres.calcAvalancheEffect(bitsFlipped, strings);
                        pres.showBitSequence(strings);
                        int lengthIdentSequence = pres.longestIdenticalSequence(pres.differentBits);
                        int lengthFlippedSequence = pres.longestFlippedSequence(pres.differentBits);

                        string flippedBits = string.Format(Resources.OutputStats1, bitsFlipped, avalanche, Environment.NewLine);
                        string identSeq = string.Format(Resources.OutputStats2, lengthIdentSequence, pres.sequencePosition, Environment.NewLine);
                        string flippedSeq = string.Format(Resources.OutputStats3, lengthFlippedSequence, pres.flippedSeqPosition, Environment.NewLine);

                        byte[] statsArray = Encoding.UTF8.GetBytes(string.Format("{0}{1}{2}", flippedBits, identSeq, flippedSeq));

                        bl.Add(statsArray);
                    }

                    else
                    {
                   
                     
                         string   initialCaption = string.Format(Resources.EncryptionInitialMsgOut, Environment.NewLine);
                         string   modifiedCaption = string.Format(Resources.EncryptionModifiedlMsgOut, Environment.NewLine);

                        

                        string init = string.Format("{0}{1}", msgA, Environment.NewLine);
                        string mod = string.Format("{0}{1}{2}", msgB, Environment.NewLine, Environment.NewLine);

                        byte[] inputArray = Encoding.UTF8.GetBytes(string.Format("{0}{1}{2}{3}", initialCaption, init, modifiedCaption, mod));


                        bl.Add(inputArray);



                        //classic



                        var strings = pres.binaryStrings(pres.unchangedCipher, pres.changedCipher);
                        int nrBytesFlipped = pres.bytesFlipped();
                        double avalanche = pres.avalancheEffectBytes(nrBytesFlipped);
                        pres.showBitSequence(strings);
                        int lengthIdentSequence = pres.longestIdentSequenceBytes();
                        int lengthFlippedSequence = pres.longestFlippedSequenceBytes();

                        string flippedBits = string.Format(Resources.OutputStatsClassic1, nrBytesFlipped, avalanche, Environment.NewLine);
                        string identSeq = string.Format(Resources.OutputStatsClassic2, lengthIdentSequence, pres.sequencePosition, Environment.NewLine);
                        string flippedSeq = string.Format(Resources.OutputStatsClassic3, lengthFlippedSequence, pres.flippedSeqPosition, Environment.NewLine);

                        byte[] statsArray = Encoding.UTF8.GetBytes(string.Format("{0}{1}{2}", flippedBits, identSeq, flippedSeq));

                        bl.Add(statsArray);
                    }

                    break;

                default:
                    break;
            }

            return bl[pos].ToArray();

        }


        public bool checkSize(string A, string B)
        {
            if (A.Length != B.Length)
                return false;

            return true;
        }

        public bool validSize()
        {

            if (pres.mode == 0)
            {
                switch (settings.KeyLength)
                {
                    case 0:
                        if (key.Length != 16)
                            GuiLogMessage(Resources.KeyLength128, NotificationLevel.Warning);
                        else
                            return true;
                        break;
                    case 1:
                        if (key.Length != 24)
                            GuiLogMessage(Resources.KeyLength192, NotificationLevel.Warning);
                        else
                            return true;
                        break;
                    case 2:
                        if (key.Length != 32)
                            GuiLogMessage(Resources.KeyLength256, NotificationLevel.Warning);
                        else
                            return true;
                        break;
                }
                        

                }
            

            if (pres.mode == 1)
                {
                    if(key.Length!=8)
                        GuiLogMessage(Resources.KeyLengthDES, NotificationLevel.Warning);
                    else
                    {
                        return true;
                    }
                }
            
           
                

            return false;
        }



        public void resize()
        {
            List<byte> listA;
            List<byte> listB;

            listA = pres.unchangedCipher.ToList();
            listB = pres.changedCipher.ToList();
            int countA = listA.Count();
            int countB = listB.Count();

            if (countA < countB)
            {
                int arrLength = countB - countA;
                listA.AddRange(fillArray(arrLength));
            }
            else


           if (countB < countA)
            {
                int arrLength = countA - countB;
                listB.AddRange(fillArray(arrLength));
            }


            pres.unchangedCipher = listA.ToArray();
            pres.changedCipher = listB.ToArray();
            pres.TB1.Text = pres.binaryAsString(pres.unchangedCipher);
            pres.TB2.Text = pres.binaryAsString(pres.changedCipher);

        }

        public byte[] fillArray(int arrayLength)
        {
            byte[] byteArr = new byte[arrayLength];
            int i = 0;

            foreach (byte b in byteArr)
            {
                byteArr[i] = 0;
                i++;
            }
            return byteArr;
        }

        public void instantiation()
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
