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
using AESVisualisation;
using System.Threading;
using Cryptool.PluginBase.IO;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Media;
using System.Collections;
using System.Diagnostics;


namespace Cryptool.Plugins.AESVisualisation
{
    [Author("Matthias Becher", "matthias.becher2193@mail.com", "Universität Mannheim", "http://cryptool2.vs.uni-due.de")]
    [PluginInfo("AESVisualisation.Properties.Resources", "PluginCaption", "PluginTooltip", "AESVisualisation/userdoc.xml", new[] { "CrypWin/images/default.png" })]
    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class AESVisualisation : ICrypComponent
    {
        #region Private Variables
       
        private readonly AESVisualisationSettings settings = new AESVisualisationSettings();
        private byte[] text;
        private byte[] key;
        private byte[][] keyList = new byte[15][];
        private string output =  "ASDDASF";
        private byte[][] sBox = new byte[16][];
        private int action = 1;
        private int roundNumber = 1;
        private byte[][] states = new byte[56][];
        private byte[][] roundConstant = new byte[12][];
        private AESPresentation pres;
        private CStreamWriter outputStreamWriter = new CStreamWriter();
        static Random rnd = new Random();
        private Boolean execute = true;
        private Boolean aborted = false;
        private int language;
        int keysize;
        Thread presThread;
        Thread executeThread;
        Thread newPresentationThread;



        AutoResetEvent buttonNextClickedEvent;

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "Text input", "Input the key", true)]
        public byte[] Key
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

        [PropertyInfo(Direction.InputData, "Text input", "Input the text to be encrypted", true)]
        public byte[] Text
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

        /// <summary>
        /// HOWTO: Output interface to write the output data.
        /// You can add more output properties ot other type if needed.
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputStreamCaption", "OutputStreamTooltip", true)]
        public ICryptoolStream OutputStream
        {
            get
            {
                return outputStreamWriter;
            }
            set
            {
                // empty
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
            executeThread = new Thread(execution);
            executeThread.Start();
            //if (aborted)
            //{
            //    outputStreamWriter.Write();
            //    outputStreamWriter.Close();
            //    buttonNextClickedEvent = pres.buttonNextClickedEvent;
            //    ProgressChanged(1, 1);
            //}
        }

        public void PostExecution()
        {
            executeThread.Abort();
            //pres = new AESPresentation();
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
            aborted = true;
            presThread.Abort();          
            executeThread.Abort();
            newPresentationThread = new Thread(newPresentation);
            newPresentationThread.SetApartmentState(ApartmentState.STA);
            newPresentationThread.Start();
            //pres.cleanUp();
            //pres.reset();
            outputStreamWriter.Close();
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
            pres = new AESPresentation();
            presThread = new Thread(pres.exectue);
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion

        #region Methods
        private void setSBox()
        {
            int x = 0;
            while (x < 16)
            {
                this.sBox[x] = new byte[16];
                x++;
            }
            x = 0;
            List<int> temp = new List<int>();
            while (x < 256)
            {
                temp.Add(x);
                x++;
            }
            int y = 0;
            x = 0;
            int z;
            while (y < 16)
            {
                while (x < 16)
                {
                    z = rnd.Next(temp.Count);
                    sBox[y][x] = Convert.ToByte(temp[z]);
                    temp.RemoveAt(z);
                    x++;
                }
                y++;
                x = 0;
            }
            x = 0;
            y = 0;
            List<TextBlock> blockList = pres.textBlockList[2];
            foreach (TextBlock tb in blockList)
            {

                tb.Text = sBox[y][x].ToString("X2");
                x++;
                if (x > 15)
                {
                    x = 0;
                    y++;
                }
                if (y > 15)
                {
                    break;
                }
            }
        }
        private int getSBoxXPosition(byte temp)
        {
            int x = 0;
            string tempString = temp.ToString("X2");
            tempString = tempString.Substring(0, 1);
            switch (tempString)
            {
                case "0":
                    x = 0;
                    break;
                case "1":
                    x = 1;
                    break;
                case "2":
                    x = 2;
                    break;
                case "3":
                    x = 3;
                    break;
                case "4":
                    x = 4;
                    break;
                case "5":
                    x = 5;
                    break;
                case "6":
                    x = 6;
                    break;
                case "7":
                    x = 7;
                    break;
                case "8":
                    x = 8;
                    break;
                case "9":
                    x = 9;
                    break;
                case "A":
                    x = 10;
                    break;
                case "B":
                    x = 11;
                    break;
                case "C":
                    x = 12;
                    break;
                case "D":
                    x = 13;
                    break;
                case "E":
                    x = 14;
                    break;
                case "F":
                    x = 15;
                    break;
                default:
                    break;
            }
            return x;
        }
        private int getSBoxYPosition(byte temp)
        {
            int x = 0;
            string tempString = temp.ToString("X2");
            tempString = tempString.Substring(1, 1);
            switch (tempString)
            {
                case "0":
                    x = 0;
                    break;
                case "1":
                    x = 1;
                    break;
                case "2":
                    x = 2;
                    break;
                case "3":
                    x = 3;
                    break;
                case "4":
                    x = 4;
                    break;
                case "5":
                    x = 5;
                    break;
                case "6":
                    x = 6;
                    break;
                case "7":
                    x = 7;
                    break;
                case "8":
                    x = 8;
                    break;
                case "9":
                    x = 9;
                    break;
                case "A":
                    x = 10;
                    break;
                case "B":
                    x = 11;
                    break;
                case "C":
                    x = 12;
                    break;
                case "D":
                    x = 13;
                    break;
                case "E":
                    x = 14;
                    break;
                case "F":
                    x = 15;
                    break;
                default:
                    break;
            }
            return x;
        }
        private void setStates()
        {
            int x = 0;
            int y = 0;
            int z = 0;
            byte[] temp;
            byte[] result;
            while (x < (39 + 8 * keysize))
            {
                switch (y)
                {
                    case 0:
                        temp = new byte[16];
                        temp = states[x];
                        result = new byte[16];
                        foreach (byte b in temp)
                        {
                            result[z] = pres.sBox[getSBoxXPosition(temp[z])][getSBoxYPosition(temp[z])];
                            z++;
                        }
                        z = 0;
                        x++;
                        states[x] = result;
                        y = 1;
                        break;
                    case 1:
                        temp = new byte[16];
                        temp = states[x];
                        result = new byte[16];
                        result[0] = temp[0];
                        result[1] = temp[5];
                        result[2] = temp[10];
                        result[3] = temp[15];
                        result[4] = temp[4];
                        result[5] = temp[9];
                        result[6] = temp[14];
                        result[7] = temp[3];
                        result[8] = temp[8];
                        result[9] = temp[13];
                        result[10] = temp[2];
                        result[11] = temp[7];
                        result[12] = temp[12];
                        result[13] = temp[1];
                        result[14] = temp[6];
                        result[15] = temp[11];
                        x++;
                        states[x] = result;
                        y = 2;
                        break;
                    case 2:
                        temp = new byte[16];
                        result = new byte[16];
                        if (x < (38 + 8 * keysize))
                        {
                            z = 0;
                            result = mixColumn(states[x]);
                            x++;
                            states[x] = result;
                        }
                        y = 3;
                        break;
                    case 3:
                        temp = new byte[16];
                        result = new byte[16];
                        result = addKey(states[x], keyList[roundNumber]);
                        x++;
                        states[x] = result;
                        if (x < (39 + 8 * keysize))
                        {
                            y = 0;
                        }
                        roundNumber++;
                        break;
                    default:
                        break;
                }
            }
        }
        private byte[] addKey(byte[] block, byte[] key)
        {
            byte[] temp = new byte[16];
            int y = 0;
            while (y < 16)
            {
                temp[y] = (byte)(block[y] ^ key[y]);
                y++;
            }
            return temp;

        }
        public List<TextBlock> createTextBlockList(int textBlockList)
        {
            List<TextBlock> list = new List<TextBlock>();
            int x;
            string temp;
            switch (textBlockList)
            {
                case 0:
                    list.Add(pres.keyTextBlock1);
                    list.Add(pres.keyTextBlock2);
                    list.Add(pres.keyTextBlock3);
                    list.Add(pres.keyTextBlock4);
                    list.Add(pres.keyTextBlock5);
                    list.Add(pres.keyTextBlock6);
                    list.Add(pres.keyTextBlock7);
                    list.Add(pres.keyTextBlock8);
                    list.Add(pres.keyTextBlock9);
                    list.Add(pres.keyTextBlock10);
                    list.Add(pres.keyTextBlock11);
                    list.Add(pres.keyTextBlock12);
                    list.Add(pres.keyTextBlock13);
                    list.Add(pres.keyTextBlock14);
                    list.Add(pres.keyTextBlock5);
                    list.Add(pres.keyTextBlock6);
                    break;
                case 1:
                    list.Add(pres.keyTextBlock7);
                    list.Add(pres.keyTextBlock8);
                    list.Add(pres.keyTextBlock9);
                    list.Add(pres.keyTextBlock20);
                    list.Add(pres.keyTextBlock21);
                    list.Add(pres.keyTextBlock22);
                    list.Add(pres.keyTextBlock23);
                    list.Add(pres.keyTextBlock24);
                    list.Add(pres.keyTextBlock25);
                    list.Add(pres.keyTextBlock26);
                    list.Add(pres.keyTextBlock27);
                    list.Add(pres.keyTextBlock28);
                    list.Add(pres.keyTextBlock29);
                    list.Add(pres.keyTextBlock30);
                    list.Add(pres.keyTextBlock31);
                    list.Add(pres.keyTextBlock32);
                    break;
                case 2:
                    list.Add(pres.keyTextBlock33);
                    list.Add(pres.keyTextBlock34);
                    list.Add(pres.keyTextBlock35);
                    list.Add(pres.keyTextBlock36);
                    list.Add(pres.keyTextBlock37);
                    list.Add(pres.keyTextBlock38);
                    list.Add(pres.keyTextBlock39);
                    list.Add(pres.keyTextBlock40);
                    list.Add(pres.keyTextBlock41);
                    list.Add(pres.keyTextBlock42);
                    list.Add(pres.keyTextBlock43);
                    list.Add(pres.keyTextBlock44);
                    list.Add(pres.keyTextBlock45);
                    list.Add(pres.keyTextBlock46);
                    list.Add(pres.keyTextBlock47);
                    list.Add(pres.keyTextBlock48);
                    break;
                case 3:
                    x = 19;
                    temp = "sTextBlock";
                    while (x < 306)
                    {
                        if (x % 18 != 0 && (x + 1) % 18 != 0)
                        {
                            string y = temp + x;
                            list.Add((TextBlock)pres.FindName(y));
                            x++;
                        }
                        else
                        {
                            x++;
                        }
                    }
                    break;
                case 4:
                    x = 1;
                    temp = "sStateTextBlock";
                    while (x < 17)
                    {
                        string y = temp + x;
                        list.Add((TextBlock)pres.FindName(y));
                        x++;
                    }
                    break;
                case 5:
                    x = 1;
                    temp = "sResultTextBlock";
                    while (x < 17)
                    {
                        string y = temp + x;
                        list.Add((TextBlock)pres.FindName(y));
                        x++;
                    }
                    break;
                case 6:
                    x = 1;
                    temp = "mStateTextBlock";
                    while (x < 17)
                    {
                        string y = temp + x;
                        list.Add((TextBlock)pres.FindName(y));
                        x++;
                    }
                    break;
                case 7:
                    x = 1;
                    temp = "mTransitionTextBlock";
                    while (x < 17)
                    {
                        string y = temp + x;
                        list.Add((TextBlock)pres.FindName(y));
                        x++;
                    }
                    break;
                case 8:
                    x = 1;
                    temp = "mResultTextBlock";
                    while (x < 17)
                    {
                        string y = temp + x;
                        list.Add((TextBlock)pres.FindName(y));
                        x++;
                    }
                    break;
                default:
                    break;
            }

            return list;
        }
        public void subBytes()
        {
            
            List<TextBlock> sState = pres.textBlockList[4];
            List<TextBlock> sResult = pres.textBlockList[5];
            List<Border> tempBordes = new List<Border>();
            int r;
            int x = 0;
            int y = 0;
            int z = 0;
            foreach (TextBlock tb in sState)
            {
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    tb.Background = Brushes.Green;
                }, null);
                wait();
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.sTransitionTextBlock3.Text = tb.Text;
                    pres.sTransitionTextBlock3.Background = Brushes.Green;
                    pres.sTransitionBorder3.Visibility = Visibility.Visible;
                }, null);
                wait();
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    tb.Background = Brushes.Transparent;
                    pres.sTransitionTextBlock3.Background = Brushes.Transparent;
                }, null);
                wait();
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.sTransitionBorder3.Visibility = Visibility.Hidden;
                    pres.sTransitionTextBlock1.Text = pres.sTransitionTextBlock3.Text.Substring(0, 1);
                    pres.sTransitionTextBlock2.Text = pres.sTransitionTextBlock3.Text.Substring(1, 1);
                    pres.sTransitionTextBlock3.Text = "";
                    pres.sTransitionBorder1.Visibility = Visibility.Visible;
                    pres.sTransitionBorder2.Visibility = Visibility.Visible;
                }, null);
                wait();
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.sTransitionTextBlock2.Background = Brushes.Transparent;
                    pres.sTransitionTextBlock1.Background = Brushes.Green;
                }, null);
                wait();               
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    switch (pres.sTransitionTextBlock1.Text)
                    {
                        case "0":
                            x = 0;
                            pres.sBorder18.Background = Brushes.Green;
                            tempBordes.Add(pres.sBorder18);
                            break;
                        case "1":
                            x = 1;
                            pres.sBorder36.Background = Brushes.Green;
                            tempBordes.Add(pres.sBorder36);
                            break;
                        case "2":
                            x = 2;
                            pres.sBorder54.Background = Brushes.Green;
                            tempBordes.Add(pres.sBorder54);
                            break;
                        case "3":
                            x = 3;
                            pres.sBorder72.Background = Brushes.Green;
                            tempBordes.Add(pres.sBorder72);
                            break;
                        case "4":
                            x = 4;
                            pres.sBorder90.Background = Brushes.Green;
                            tempBordes.Add(pres.sBorder90);
                            break;
                        case "5":
                            x = 5;
                            pres.sBorder108.Background = Brushes.Green;
                            tempBordes.Add(pres.sBorder108);
                            break;
                        case "6":
                            x = 6;
                            pres.sBorder126.Background = Brushes.Green;
                            tempBordes.Add(pres.sBorder126);
                            break;
                        case "7":
                            x = 7;
                            pres.sBorder144.Background = Brushes.Green;
                            tempBordes.Add(pres.sBorder144);
                            break;
                        case "8":
                            x = 8;
                            pres.sBorder162.Background = Brushes.Green;
                            tempBordes.Add(pres.sBorder162);
                            break;
                        case "9":
                            x = 9;
                            pres.sBorder180.Background = Brushes.Green;
                            tempBordes.Add(pres.sBorder180);
                            break;
                        case "A":
                            x = 10;
                            pres.sBorder198.Background = Brushes.Green;
                            tempBordes.Add(pres.sBorder198);
                            break;
                        case "B":
                            x = 11;
                            pres.sBorder216.Background = Brushes.Green;
                            tempBordes.Add(pres.sBorder216);
                            break;
                        case "C":
                            x = 12;
                            pres.sBorder234.Background = Brushes.Green;
                            tempBordes.Add(pres.sBorder234);
                            break;
                        case "D":
                            x = 13;
                            pres.sBorder252.Background = Brushes.Green;
                            tempBordes.Add(pres.sBorder252);
                            break;
                        case "E":
                            x = 14;
                            pres.sBorder270.Background = Brushes.Green;
                            tempBordes.Add(pres.sBorder270);
                            break;
                        case "F":
                            x = 15;
                            pres.sBorder288.Background = Brushes.Green;
                            tempBordes.Add(pres.sBorder288);
                            break;
                        default:
                            break;
                    }
                    pres.sTransitionTextBlock1.Background = Brushes.Transparent;
                }, null);
                wait();
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pres.sTransitionTextBlock2.Background = Brushes.Green;
                }, null);
                wait();
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    switch (pres.sTransitionTextBlock2.Text)
                    {
                        case "0":
                            y = 0;
                            pres.sBorder1.Background = Brushes.Green;
                            tempBordes.Add(pres.sBorder1);
                            break;
                        case "1":
                            y = 1;
                            pres.sBorder2.Background = Brushes.Green;
                            tempBordes.Add(pres.sBorder2);
                            break;
                        case "2":
                            y = 2;
                            pres.sBorder3.Background = Brushes.Green;
                            tempBordes.Add(pres.sBorder3);
                            break;
                        case "3":
                            y = 3;
                            pres.sBorder4.Background = Brushes.Green;
                            tempBordes.Add(pres.sBorder4);
                            break;
                        case "4":
                            y = 4;
                            pres.sBorder5.Background = Brushes.Green;
                            tempBordes.Add(pres.sBorder5);
                            break;
                        case "5":
                            y = 5;
                            pres.sBorder6.Background = Brushes.Green;
                            tempBordes.Add(pres.sBorder6);
                            break;
                        case "6":
                            y = 6;
                            pres.sBorder7.Background = Brushes.Green;
                            tempBordes.Add(pres.sBorder7);
                            break;
                        case "7":
                            y = 7;
                            pres.sBorder8.Background = Brushes.Green;
                            tempBordes.Add(pres.sBorder8);
                            break;
                        case "8":
                            y = 8;
                            pres.sBorder9.Background = Brushes.Green;
                            tempBordes.Add(pres.sBorder9);
                            break;
                        case "9":
                            y = 9;
                            pres.sBorder10.Background = Brushes.Green;
                            tempBordes.Add(pres.sBorder10);
                            break;
                        case "A":
                            y = 10;
                            pres.sBorder11.Background = Brushes.Green;
                            tempBordes.Add(pres.sBorder11);
                            break;
                        case "B":
                            y = 11;
                            pres.sBorder12.Background = Brushes.Green;
                            tempBordes.Add(pres.sBorder12);
                            break;
                        case "C":
                            y = 12;
                            pres.sBorder13.Background = Brushes.Green;
                            tempBordes.Add(pres.sBorder13);
                            break;
                        case "D":
                            y = 13;
                            pres.sBorder14.Background = Brushes.Green;
                            tempBordes.Add(pres.sBorder14);
                            break;
                        case "E":
                            y = 14;
                            pres.sBorder15.Background = Brushes.Green;
                            tempBordes.Add(pres.sBorder15);
                            break;
                        case "F":
                            y = 15;
                            pres.sBorder16.Background = Brushes.Green;
                            tempBordes.Add(pres.sBorder16);
                            break;
                        default:
                            break;
                    }
                    pres.sTransitionTextBlock2.Background = Brushes.Transparent;
                }, null);
                wait();
                r = (x + 1) * 18 + y + 1 - 19 - 2*x;                                    
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {                  
                    pres.textBlockList[3][r].Background = Brushes.Green;
                }, null);
                wait();              
                pres.Dispatcher.Invoke(DispatcherPriority.Normal,(SendOrPostCallback)delegate
                {
                    sResult[z].Text = sBox[x][y].ToString("X2");
                    sResult[z].Background = Brushes.Green;
                    pres.sTransitionBorder1.Visibility = Visibility.Hidden;
                    pres.sTransitionBorder2.Visibility = Visibility.Hidden;
                }, null);
                wait();
                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    sResult[z].Background = Brushes.Transparent;
                    foreach (Border br in tempBordes)
                    {
                        br.Background = Brushes.Yellow;
                    }
                    tempBordes.Clear();
                    z++;
                    pres.sTransitionTextBlock1.Text = "";
                    pres.sTransitionTextBlock2.Text = "";
                    pres.textBlockList[3][r].Background = Brushes.Transparent;
                }, null);
                wait();
            }
        }

        private void wait()
        {
            if (!pres.autostep)
            {
                buttonNextClickedEvent = pres.buttonNextClickedEvent;
                buttonNextClickedEvent.WaitOne();
            }
            else
            {
                buttonNextClickedEvent = pres.buttonNextClickedEvent;
                buttonNextClickedEvent.WaitOne(pres.autostepSpeed);
            }
        }

        private byte[] mixColumn(byte[] state)
        {
            byte[] result = new byte[16];
            state = arrangeText(state);
            BitArray calc = new BitArray(8);
            calc[3] = true;
            calc[4] = true;
            calc[1] = true;
            calc[0] = true;
            int z = 0;
            int y = 0;
            BitArray tempBit;
            BitArray tempBit1;
            BitArray tempBit2;
            BitArray tempBit3;
            BitArray tempBit4;
            BitArray tempBit5;
            bool add;
            while(z < 4)
            {
                switch (z)
                {
                    case 0:
                        tempBit = new BitArray(new byte[] { state[0] });
                        tempBit1 = new BitArray(new byte[] { state[4] });
                        tempBit2 = new BitArray(new byte[] { state[8] });
                        tempBit3 = new BitArray(new byte[] { state[12] });
                        y = 0;
                        while (y < 4)
                        {
                            switch (y)
                            {
                                case 0:
                                    add = tempBit[7];
                                    tempBit4 = leftShift(tempBit);
                                    if (add)
                                    {
                                        tempBit4.Xor(calc);
                                    }
                                    add = tempBit1[7];
                                    tempBit5 = leftShift(tempBit1);
                                    if (add)
                                    {
                                        tempBit5.Xor(calc);
                                    }
                                    tempBit5.Xor(tempBit1);
                                    tempBit5.Xor(tempBit4);
                                    tempBit5.Xor(tempBit2);
                                    tempBit5.Xor(tempBit3);
                                    result[0] = convertToByte(tempBit5);
                                    y++;
                                    break;
                                case 1:
                                    add = tempBit1[7];
                                    tempBit4 = leftShift(tempBit1);
                                    if (add)
                                    {
                                        tempBit4.Xor(calc);
                                    }
                                    add = tempBit2[7];
                                    tempBit5 = leftShift(tempBit2);
                                    if (add)
                                    {
                                        tempBit5.Xor(calc);
                                    }
                                    tempBit5.Xor(tempBit2);
                                    tempBit5.Xor(tempBit4);
                                    tempBit5.Xor(tempBit3);
                                    tempBit5.Xor(tempBit);
                                    result[4] = convertToByte(tempBit5);
                                    y++;
                                    break;
                                case 2:
                                    add = tempBit2[7];
                                    tempBit4 = leftShift(tempBit2);
                                    if (add)
                                    {
                                        tempBit4.Xor(calc);
                                    }
                                    add = tempBit3[7];
                                    tempBit5 = leftShift(tempBit3);
                                    if (add)
                                    {
                                        tempBit5.Xor(calc);
                                    }
                                    tempBit5.Xor(tempBit3);
                                    tempBit5.Xor(tempBit4);
                                    tempBit5.Xor(tempBit);
                                    tempBit5.Xor(tempBit1);
                                    result[8] = convertToByte(tempBit5);
                                    y++;
                                    break;
                                case 3:
                                    add = tempBit3[7];
                                    tempBit4 = leftShift(tempBit3);
                                    if (add)
                                    {
                                        tempBit4.Xor(calc);
                                    }
                                    add = tempBit[7];
                                    tempBit5 = leftShift(tempBit);
                                    if (add)
                                    {
                                        tempBit5.Xor(calc);
                                    }
                                    tempBit5.Xor(tempBit);
                                    tempBit5.Xor(tempBit4);
                                    tempBit5.Xor(tempBit1);
                                    tempBit5.Xor(tempBit2);
                                    result[12] = convertToByte(tempBit5);
                                    y++;
                                    break;
                                default:
                                    y++;
                                    break;
                            }
                        }
                        z++;
                        break;
                    case 1:
                        tempBit = new BitArray(new byte[] { state[1] });
                        tempBit1 = new BitArray(new byte[] { state[5] });
                        tempBit2 = new BitArray(new byte[] { state[9] });
                        tempBit3 = new BitArray(new byte[] { state[13] });
                        y = 0;
                        while (y < 4)
                        {
                            switch (y)
                            {
                                case 0:
                                    add = tempBit[7];
                                    tempBit4 = leftShift(tempBit);
                                    if (add)
                                    {
                                        tempBit4.Xor(calc);
                                    }
                                    add = tempBit1[7];
                                    tempBit5 = leftShift(tempBit1);
                                    if (add)
                                    {
                                        tempBit5.Xor(calc);
                                    }
                                    tempBit5.Xor(tempBit1);
                                    tempBit5.Xor(tempBit4);
                                    tempBit5.Xor(tempBit2);
                                    tempBit5.Xor(tempBit3);
                                    result[1] = convertToByte(tempBit5);
                                    y++;
                                    break;
                                case 1:
                                    add = tempBit1[7];
                                    tempBit4 = leftShift(tempBit1);
                                    if (add)
                                    {
                                        tempBit4.Xor(calc);
                                    }
                                    add = tempBit2[7];
                                    tempBit5 = leftShift(tempBit2);
                                    if (add)
                                    {
                                        tempBit5.Xor(calc);
                                    }
                                    tempBit5.Xor(tempBit2);
                                    tempBit5.Xor(tempBit4);
                                    tempBit5.Xor(tempBit3);
                                    tempBit5.Xor(tempBit);
                                    result[5] = convertToByte(tempBit5);
                                    y++;
                                    break;
                                case 2:
                                    add = tempBit2[7];
                                    tempBit4 = leftShift(tempBit2);
                                    if (add)
                                    {
                                        tempBit4.Xor(calc);
                                    }
                                    add = tempBit3[7];
                                    tempBit5 = leftShift(tempBit3);
                                    if (add)
                                    {
                                        tempBit5.Xor(calc);
                                    }
                                    tempBit5.Xor(tempBit3);
                                    tempBit5.Xor(tempBit4);
                                    tempBit5.Xor(tempBit);
                                    tempBit5.Xor(tempBit1);
                                    result[9] = convertToByte(tempBit5);
                                    y++;
                                    break;
                                case 3:
                                    add = tempBit3[7];
                                    tempBit4 = leftShift(tempBit3);
                                    if (add)
                                    {
                                        tempBit4.Xor(calc);
                                    }
                                    add = tempBit[7];
                                    tempBit5 = leftShift(tempBit);
                                    if (add)
                                    {
                                        tempBit5.Xor(calc);
                                    }
                                    tempBit5.Xor(tempBit);
                                    tempBit5.Xor(tempBit4);
                                    tempBit5.Xor(tempBit1);
                                    tempBit5.Xor(tempBit2);
                                    result[13] = convertToByte(tempBit5);
                                    y++;
                                    break;
                                default:
                                    y++;
                                    break;
                            }
                        }
                        z++;
                        break;
                    case 2:
                        tempBit = new BitArray(new byte[] { state[2] });
                        tempBit1 = new BitArray(new byte[] { state[6] });
                        tempBit2 = new BitArray(new byte[] { state[10] });
                        tempBit3 = new BitArray(new byte[] { state[14] });
                        y = 0;
                        while (y < 4)
                        {
                            switch (y)
                            {
                                case 0:
                                    add = tempBit[7];
                                    tempBit4 = leftShift(tempBit);
                                    if (add)
                                    {
                                        tempBit4.Xor(calc);
                                    }
                                    add = tempBit1[7];
                                    tempBit5 = leftShift(tempBit1);
                                    if (add)
                                    {
                                        tempBit5.Xor(calc);
                                    }
                                    tempBit5.Xor(tempBit1);
                                    tempBit5.Xor(tempBit4);
                                    tempBit5.Xor(tempBit2);
                                    tempBit5.Xor(tempBit3);
                                    result[2] = convertToByte(tempBit5);
                                    y++;
                                    break;
                                case 1:
                                    add = tempBit1[7];
                                    tempBit4 = leftShift(tempBit1);
                                    if (add)
                                    {
                                        tempBit4.Xor(calc);
                                    }
                                    add = tempBit2[7];
                                    tempBit5 = leftShift(tempBit2);
                                    if (add)
                                    {
                                        tempBit5.Xor(calc);
                                    }
                                    tempBit5.Xor(tempBit2);
                                    tempBit5.Xor(tempBit4);
                                    tempBit5.Xor(tempBit3);
                                    tempBit5.Xor(tempBit);
                                    result[6] = convertToByte(tempBit5);
                                    y++;
                                    break;
                                case 2:
                                    add = tempBit2[7];
                                    tempBit4 = leftShift(tempBit2);
                                    if (add)
                                    {
                                        tempBit4.Xor(calc);
                                    }
                                    add = tempBit3[7];
                                    tempBit5 = leftShift(tempBit3);
                                    if (add)
                                    {
                                        tempBit5.Xor(calc);
                                    }
                                    tempBit5.Xor(tempBit3);
                                    tempBit5.Xor(tempBit4);
                                    tempBit5.Xor(tempBit);
                                    tempBit5.Xor(tempBit1);
                                    result[10] = convertToByte(tempBit5);
                                    y++;
                                    break;
                                case 3:
                                    add = tempBit3[7];
                                    tempBit4 = leftShift(tempBit3);
                                    if (add)
                                    {
                                        tempBit4.Xor(calc);
                                    }
                                    add = tempBit[7];
                                    tempBit5 = leftShift(tempBit);
                                    if (add)
                                    {
                                        tempBit5.Xor(calc);
                                    }
                                    tempBit5.Xor(tempBit);
                                    tempBit5.Xor(tempBit4);
                                    tempBit5.Xor(tempBit1);
                                    tempBit5.Xor(tempBit2);
                                    result[14] = convertToByte(tempBit5);
                                    y++;
                                    break;
                                default:
                                    y++;
                                    break;
                            }
                        }
                        z++;
                        break;
                    case 3:
                        tempBit = new BitArray(new byte[] { state[3] });
                        tempBit1 = new BitArray(new byte[] { state[7] });
                        tempBit2 = new BitArray(new byte[] { state[11] });
                        tempBit3 = new BitArray(new byte[] { state[15] });
                        y = 0;
                        while (y < 4)
                        {
                            switch (y)
                            {
                                case 0:
                                    add = tempBit[7];
                                    tempBit4 = leftShift(tempBit);
                                    if (add)
                                    {
                                        tempBit4.Xor(calc);
                                    }
                                    add = tempBit1[7];
                                    tempBit5 = leftShift(tempBit1);
                                    if (add)
                                    {
                                        tempBit5.Xor(calc);
                                    }
                                    tempBit5.Xor(tempBit1);
                                    tempBit5.Xor(tempBit4);
                                    tempBit5.Xor(tempBit2);
                                    tempBit5.Xor(tempBit3);
                                    result[3] = convertToByte(tempBit5);
                                    y++;
                                    break;
                                case 1:
                                    add = tempBit1[7];
                                    tempBit4 = leftShift(tempBit1);
                                    if (add)
                                    {
                                        tempBit4.Xor(calc);
                                    }
                                    add = tempBit2[7];
                                    tempBit5 = leftShift(tempBit2);
                                    if (add)
                                    {
                                        tempBit5.Xor(calc);
                                    }
                                    tempBit5.Xor(tempBit2);
                                    tempBit5.Xor(tempBit4);
                                    tempBit5.Xor(tempBit3);
                                    tempBit5.Xor(tempBit);
                                    result[7] = convertToByte(tempBit5);
                                    y++;
                                    break;
                                case 2:
                                    add = tempBit2[7];
                                    tempBit4 = leftShift(tempBit2);
                                    if (add)
                                    {
                                        tempBit4.Xor(calc);
                                    }
                                    add = tempBit3[7];
                                    tempBit5 = leftShift(tempBit3);
                                    if (add)
                                    {
                                        tempBit5.Xor(calc);
                                    }
                                    tempBit5.Xor(tempBit3);
                                    tempBit5.Xor(tempBit4);
                                    tempBit5.Xor(tempBit);
                                    tempBit5.Xor(tempBit1);
                                    result[11] = convertToByte(tempBit5);
                                    y++;
                                    break;
                                case 3:
                                    add = tempBit3[7];
                                    tempBit4 = leftShift(tempBit3);
                                    if (add)
                                    {
                                        tempBit4.Xor(calc);
                                    }
                                    add = tempBit[7];
                                    tempBit5 = leftShift(tempBit);
                                    if (add)
                                    {
                                        tempBit5.Xor(calc);
                                    }
                                    tempBit5.Xor(tempBit);
                                    tempBit5.Xor(tempBit4);
                                    tempBit5.Xor(tempBit1);
                                    tempBit5.Xor(tempBit2);
                                    result[15] = convertToByte(tempBit5);
                                    y++;
                                    break;
                                case 4:
                                    return result;
                                default:
                                    y++;
                                    break;
                            }
                        }
                        z++;
                        break;
                    default:
                        break;
                }
            }
            result = rearrangeText(result);         
            return result;
        }

        private BitArray leftShift(BitArray temp)
        {
            BitArray result = new BitArray(8);
            result[0] = false;
            result[1] = temp[0];
            result[2] = temp[1];
            result[3] = temp[2];
            result[4] = temp[3];
            result[5] = temp[4];
            result[6] = temp[5];
            result[7] = temp[6];
            return result;
        }

        private byte convertToByte(BitArray bits)
        {
            byte[] bytes = new byte[1];
            bits.CopyTo(bytes, 0);
            return bytes[0];
        }

        private byte[] arrangeText(byte[] input)
        {
            byte[] result = new byte[16];
            result[0] = input[0];
            result[4] = input[1];
            result[8] = input[2];
            result[12] = input[3];
            result[1] = input[4];
            result[5] = input[5];
            result[9] = input[6];
            result[13] = input[7];
            result[2] = input[8];
            result[6] = input[9];
            result[10] = input[10];
            result[14] = input[11];
            result[3] = input[12];
            result[7] = input[13];
            result[11] = input[14];
            result[15] = input[15];
            return result;
        }

        private byte[] rearrangeText(byte[] input)
        {
            byte[] result = new byte[16];
            result[0] = input[0];
            result[1] = input[4];
            result[2] = input[8];
            result[3] = input[12];
            result[4] = input[1];
            result[5] = input[5];
            result[6] = input[9];
            result[7] = input[13];
            result[8] = input[2];
            result[9] = input[6];
            result[10] = input[10];
            result[11] = input[14];
            result[12] = input[3];
            result[13] = input[7];
            result[14] = input[11];
            result[15] = input[15];
            return result;
        }

        private void expandKey()
        {
            byte[] calc = new byte[4];
            for(int x = 1; x < 11; x++)
            {
                byte[] roundConst = roundConstant[x - 1];
                byte[] prevKey = {  keyList[x - 1][13], keyList[x - 1][14], keyList[x - 1][15], keyList[x - 1][12] };
                byte a;
                byte b;
                int z = 0;
                byte[] temp = new byte[16];
                while (z < 4)
                {
                    calc[z] = pres.sBox[getSBoxXPosition(prevKey[z])][getSBoxYPosition(prevKey[z])];
                    z++;
                }
                z = 0;
                while(z < 4)
                {
                    prevKey[z] = (byte)(calc[z] ^ roundConst[z]);
                    z++;
                }
                a = keyList[x - 1][0];
                b = prevKey[0];
                temp[0] = (byte)(a ^ b);
                temp[1] = (byte)(keyList[x - 1][1] ^ prevKey[1]);
                temp[2] = (byte)(keyList[x - 1][2] ^ prevKey[2]);
                temp[3] = (byte)(keyList[x - 1][3] ^ prevKey[3]);
                temp[4] = (byte)(temp[0] ^ keyList[x - 1][4]);
                temp[5] = (byte)(temp[1] ^ keyList[x - 1][5]);
                temp[6] = (byte)(temp[2] ^ keyList[x - 1][6]);
                temp[7] = (byte)(temp[3] ^ keyList[x - 1][7]);
                temp[8] = (byte)(temp[4] ^ keyList[x - 1][8]);
                temp[9] = (byte)(temp[5] ^ keyList[x - 1][9]);
                temp[10] = (byte)(temp[6] ^ keyList[x - 1][10]);
                temp[11] = (byte)(temp[7] ^ keyList[x - 1][11]);
                temp[12] = (byte)(temp[8] ^ keyList[x - 1][12]);
                temp[13] = (byte)(temp[9] ^ keyList[x - 1][13]);
                temp[14] = (byte)(temp[10] ^ keyList[x - 1][14]);
                temp[15] = (byte)(temp[11] ^ keyList[x - 1][15]);
                keyList[x] = temp;
            }
        }

        private void expandKey192()
        {
            byte[] tempkey = new byte[216];
            tempkey[0] = keyList[0][0];
            tempkey[1] = keyList[0][1];
            tempkey[2] = keyList[0][2];
            tempkey[3] = keyList[0][3];
            tempkey[4] = keyList[0][4];
            tempkey[5] = keyList[0][5];
            tempkey[6] = keyList[0][6];
            tempkey[7] = keyList[0][7];
            tempkey[8] = keyList[0][8];
            tempkey[9] = keyList[0][9];
            tempkey[10] = keyList[0][10];
            tempkey[11] = keyList[0][11];
            tempkey[12] = keyList[0][12];
            tempkey[13] = keyList[0][13];
            tempkey[14] = keyList[0][14];
            tempkey[15] = keyList[0][15];
            tempkey[16] = keyList[1][0];
            tempkey[17] = keyList[1][1];
            tempkey[18] = keyList[1][2];
            tempkey[19] = keyList[1][3];
            tempkey[20] = keyList[1][4];
            tempkey[21] = keyList[1][5];
            tempkey[22] = keyList[1][6];
            tempkey[23] = keyList[1][7];
            byte[] calc = new byte[4];
            int x = 23;
            int y = 0;
            int z = 0;
            byte[] roundConst;
            byte[] temp = new byte[4];
            while (x < 192)
            {
                roundConst = roundConstant[y];
                calc[0] = pres.sBox[getSBoxXPosition(tempkey[x - 2])][getSBoxYPosition(tempkey[x - 2])];
                calc[1] = pres.sBox[getSBoxXPosition(tempkey[x - 1])][getSBoxYPosition(tempkey[x - 1])];
                calc[2] = pres.sBox[getSBoxXPosition(tempkey[x])][getSBoxYPosition(tempkey[x])];
                calc[3] = pres.sBox[getSBoxXPosition(tempkey[x - 3])][getSBoxYPosition(tempkey[x - 3])];
                z = 0;
                while (z < 4)
                {
                    temp[z] = (byte)(calc[z] ^ roundConst[z]);
                    z++;
                }
                tempkey[x + 1] = (byte)(temp[0] ^ tempkey[x - 23]);
                tempkey[x + 2] = (byte)(temp[1] ^ tempkey[x - 22]);
                tempkey[x + 3] = (byte)(temp[2] ^ tempkey[x - 21]);
                tempkey[x + 4] = (byte)(temp[3] ^ tempkey[x - 20]);
                tempkey[x + 5] = (byte)(tempkey[x + 1] ^ tempkey[x - 19]);
                tempkey[x + 6] = (byte)(tempkey[x + 2] ^ tempkey[x - 18]);
                tempkey[x + 7] = (byte)(tempkey[x + 3] ^ tempkey[x - 17]);
                tempkey[x + 8] = (byte)(tempkey[x + 4] ^ tempkey[x - 16]);
                tempkey[x + 9] = (byte)(tempkey[x + 5] ^ tempkey[x - 15]);
                tempkey[x + 10] = (byte)(tempkey[x + 6] ^ tempkey[x - 14]);
                tempkey[x + 11] = (byte)(tempkey[x + 7] ^ tempkey[x - 13]);
                tempkey[x + 12] = (byte)(tempkey[x + 8] ^ tempkey[x - 12]);
                tempkey[x + 13] = (byte)(tempkey[x + 9] ^ tempkey[x - 11]);
                tempkey[x + 14] = (byte)(tempkey[x + 10] ^ tempkey[x - 10]);
                tempkey[x + 15] = (byte)(tempkey[x + 11] ^ tempkey[x - 9]);
                tempkey[x + 16] = (byte)(tempkey[x + 12] ^ tempkey[x - 8]);
                tempkey[x + 17] = (byte)(tempkey[x + 13] ^ tempkey[x - 7]);
                tempkey[x + 18] = (byte)(tempkey[x + 14] ^ tempkey[x - 6]);
                tempkey[x + 19] = (byte)(tempkey[x + 15] ^ tempkey[x - 5]);
                tempkey[x + 20] = (byte)(tempkey[x + 16] ^ tempkey[x - 4]);
                tempkey[x + 21] = (byte)(tempkey[x + 17] ^ tempkey[x - 3]);
                tempkey[x + 22] = (byte)(tempkey[x + 18] ^ tempkey[x - 2]);
                tempkey[x + 23] = (byte)(tempkey[x + 19] ^ tempkey[x - 1]);
                tempkey[x + 24] = (byte)(tempkey[x + 20] ^ tempkey[x]);
                x += 24;
                y++;
            }
            x = 0;
            y = 0;
            z = 0;
            pres.keyBytes = tempkey;
            while(x < 208)
            {
                while(y < 16)
                {
                    if(keyList[z] == null){
                        keyList[z] = new byte[16];
                    }
                    keyList[z][y] = tempkey[x];
                    x++;
                    y++;
                }
                y = 0;
                z++;
            }        
        }

        private void expandKey256()
        {
            byte[] tempkey = new byte[350];
            int x = 0;
            int y = 0;
            for (int r = 0; r < 32; r++)
            {
                tempkey[r] = keyList[x][y];
                y++;
                if(y == 16)
                {
                    y = 0;
                    x++;
                }
            }
            byte[] calc = new byte[4];
            x = 31;
            y = 0;
            int z = 0;
            byte[] roundConst;
            byte[] temp = new byte[4];
            while (x < 256)
            {
                roundConst = roundConstant[y];
                calc[0] = pres.sBox[getSBoxXPosition(tempkey[x - 2])][getSBoxYPosition(tempkey[x - 2])];
                calc[1] = pres.sBox[getSBoxXPosition(tempkey[x - 1])][getSBoxYPosition(tempkey[x - 1])];
                calc[2] = pres.sBox[getSBoxXPosition(tempkey[x])][getSBoxYPosition(tempkey[x])];
                calc[3] = pres.sBox[getSBoxXPosition(tempkey[x - 3])][getSBoxYPosition(tempkey[x - 3])];
                z = 0;
                while (z < 4)
                {
                    temp[z] = (byte)(calc[z] ^ roundConst[z]);
                    z++;
                }
                tempkey[x + 1] = (byte)(temp[0] ^ tempkey[x - 31]);
                tempkey[x + 2] = (byte)(temp[1] ^ tempkey[x - 30]);
                tempkey[x + 3] = (byte)(temp[2] ^ tempkey[x - 29]);
                tempkey[x + 4] = (byte)(temp[3] ^ tempkey[x - 28]);
                tempkey[x + 5] = (byte)(tempkey[x + 1] ^ tempkey[x - 27]);
                tempkey[x + 6] = (byte)(tempkey[x + 2] ^ tempkey[x - 26]);
                tempkey[x + 7] = (byte)(tempkey[x + 3] ^ tempkey[x - 25]);
                tempkey[x + 8] = (byte)(tempkey[x + 4] ^ tempkey[x - 24]);
                tempkey[x + 9] = (byte)(tempkey[x + 5] ^ tempkey[x - 23]);
                tempkey[x + 10] = (byte)(tempkey[x + 6] ^ tempkey[x - 22]);
                tempkey[x + 11] = (byte)(tempkey[x + 7] ^ tempkey[x - 21]);
                tempkey[x + 12] = (byte)(tempkey[x + 8] ^ tempkey[x - 20]);
                tempkey[x + 13] = (byte)(tempkey[x + 9] ^ tempkey[x - 19]);
                tempkey[x + 14] = (byte)(tempkey[x + 10] ^ tempkey[x - 18]);
                tempkey[x + 15] = (byte)(tempkey[x + 11] ^ tempkey[x - 17]);
                tempkey[x + 16] = (byte)(tempkey[x + 12] ^ tempkey[x - 16]);
                calc[0] = pres.sBox[getSBoxXPosition(tempkey[x + 13])][getSBoxYPosition(tempkey[x + 13])];
                calc[1] = pres.sBox[getSBoxXPosition(tempkey[x + 14])][getSBoxYPosition(tempkey[x + 14])];
                calc[2] = pres.sBox[getSBoxXPosition(tempkey[x + 15])][getSBoxYPosition(tempkey[x + 15])];
                calc[3] = pres.sBox[getSBoxXPosition(tempkey[x + 16])][getSBoxYPosition(tempkey[x + 16])];
                tempkey[x + 17] = (byte)(calc[0] ^ tempkey[x - 15]);
                tempkey[x + 18] = (byte)(calc[1] ^ tempkey[x - 14]);
                tempkey[x + 19] = (byte)(calc[2] ^ tempkey[x - 13]);
                tempkey[x + 20] = (byte)(calc[3] ^ tempkey[x - 12]);
                tempkey[x + 21] = (byte)(tempkey[x + 17] ^ tempkey[x - 11]);
                tempkey[x + 22] = (byte)(tempkey[x + 18] ^ tempkey[x - 10]);
                tempkey[x + 23] = (byte)(tempkey[x + 19] ^ tempkey[x - 9]);
                tempkey[x + 24] = (byte)(tempkey[x + 20] ^ tempkey[x - 8]);
                tempkey[x + 25] = (byte)(tempkey[x + 21] ^ tempkey[x - 7]);
                tempkey[x + 26] = (byte)(tempkey[x + 22] ^ tempkey[x - 6]);
                tempkey[x + 27] = (byte)(tempkey[x + 23] ^ tempkey[x - 5]);
                tempkey[x + 28] = (byte)(tempkey[x + 24] ^ tempkey[x - 4]);
                tempkey[x + 29] = (byte)(tempkey[x + 25] ^ tempkey[x - 3]);
                tempkey[x + 30] = (byte)(tempkey[x + 26] ^ tempkey[x - 2]);
                tempkey[x + 31] = (byte)(tempkey[x + 27] ^ tempkey[x - 1]);
                tempkey[x + 32] = (byte)(tempkey[x + 28] ^ tempkey[x]);
                x += 32;
                y++;               
            }
            x = 0;
            y = 0;
            z = 0;
            pres.keyBytes = tempkey;
            while (x < 240)
            {
                while (y < 16)
                {
                    if (keyList[z] == null)
                    {
                        keyList[z] = new byte[16];
                    }
                    keyList[z][y] = tempkey[x];
                    x++;
                    y++;
                }
                y = 0;
                z++;
            }
        }

        private void setRoundConstant()
        {          
            roundConstant[0] = new byte[] { 1, 0, 0, 0 };
            roundConstant[1] = new byte[] { 2, 0, 0, 0 };
            roundConstant[2] = new byte[] { 4, 0, 0, 0 };
            roundConstant[3] = new byte[] { 8, 0, 0, 0 };
            roundConstant[4] = new byte[] { 16, 0, 0, 0 };
            roundConstant[5] = new byte[] { 32, 0, 0, 0 };
            roundConstant[6] = new byte[] { 64, 0, 0, 0 };
            roundConstant[7] = new byte[] { 128, 0, 0, 0 };
            roundConstant[8] = new byte[] { 27, 0, 0, 0 };
            roundConstant[9] = new byte[] { 54, 0, 0, 0 };
        }

        private void checkKeysize()
        {
            if (key.Length != 16 + 8 * keysize)
            {
                byte[] temp = new byte[16 + 8 * keysize];
                int x = 0;
                if (key.Length < 16 + 8 * keysize)
                {
                    foreach (byte b in key)
                    {
                        temp[x] = b;
                        x++;
                    }
                    while (x < 16 + 8 * keysize)
                    {
                        temp[x] = 0;
                        x++;
                    }
                }
                else
                {
                    while (x < 16 + 8 * keysize)
                    {
                        temp[x] = key[x];
                        x++;
                    }
                }
                key = temp;
            }
        }

        private void checkTextLength()
        {
            if(text.Length != 16)
            {
                byte[] temp = new byte[16];
                int x = 0;
                if (text.Length < 16)
                {
                    foreach (byte b in text)
                    {
                        temp[x] = b;
                        x++;
                    }
                    while (x < 16)
                    {
                        temp[x] = 0;
                        x++;
                    }
                }
                else
                {
                    while(x < 16)
                    {
                        temp[x] = text[x];
                        x++;
                    }
                }
                text = temp;
            }
        }

        private void execution()
        {
            presThread = new Thread(pres.exectue);
            keysize = settings.Keysize;
            pres.keysize = keysize;
            language = settings.Language;
            pres.language = language;
            pres.setLanguage();
            checkKeysize();
            checkTextLength();
            outputStreamWriter = new CStreamWriter();
            roundNumber = 1;
            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                pres.expansionEncryptionTextBlock.Visibility = Visibility.Visible;
                pres.invisible();
                pres.buttonVisible();
                pres.hideButton();
            }, null);
            ProgressChanged(0, 1);
            OutputStream = outputStreamWriter;
            OnPropertyChanged("OutputStream");
            AutoResetEvent buttonNextClickedEvent = pres.buttonNextClickedEvent;
            setRoundConstant();
            byte[] tempState = text;
            int r = 0;
            int t = 0;
            foreach (byte b in key)
            {
                if (keyList[r] == null)
                {
                    keyList[r] = new byte[16];
                }
                keyList[r][t] = b;
                t++;
                if (t == 16)
                {
                    t = 0;
                    r++;
                }
            }
            states[0] = addKey(tempState, keyList[0]);
            pres.tempState = tempState;
            pres.roundConstant = this.roundConstant;
            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                pres.createSBox();
                pres.StartCanvas.Visibility = Visibility.Hidden;
                pres.showButton();
            }, null);
            switch (keysize)
            {
                case 0:
                    expandKey();
                    break;
                case 1:
                    expandKey192();
                    break;
                case 2:
                    expandKey256();
                    break;
                default:
                    break;
            }
            setStates();
            roundNumber = 1;
            pres.states = states;
            pres.keyList = keyList;
            presThread.Start();
            while (presThread.IsAlive)
            {
                ProgressChanged(pres.progress, 1);
            }
            outputStreamWriter.Write(states[39 + 8 * keysize]);
            outputStreamWriter.Close();
            buttonNextClickedEvent = pres.buttonNextClickedEvent;
            ProgressChanged(1, 1);
        }

        private void newPresentation()
        {
            pres = null;
            pres = new AESPresentation();
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
