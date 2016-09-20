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
using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System;
using System.Collections.Generic;
using System.Threading;
using Cryptool.PluginBase.IO;
using System.Windows.Threading;
using System.Collections;
using System.Text;
using AvalancheVisualization;
using System.Linq;
using System.Windows;

namespace Cryptool.Plugins.AvalancheVisualization
{
    // HOWTO: Change author name, email address, organization and URL.
    [Author("Camilo Echeverri", "cechever@mail.uni-mannheim.de", "University of Mannheim", "http://cryptool2.vs.uni-due.de")]
    // HOWTO: Change plugin caption (title to appear in CT2) and tooltip.
    // You can (and should) provide a user documentation as XML file and an own icon.
    [PluginInfo("AvalancheVisualization", "Tests the avalanche effect property of some cryptographic algorithms", "AvalancheVisualization/userdoc.xml", new[] { "AvalancheVisualization/Images/Avalanche.png" })]
    // HOWTO: Change category to one that fits to your plugin. Multiple categories are allowed.
    [ComponentCategory(ComponentCategory.ToolsMisc)]
    public class AvalancheVisualization : ICrypComponent
    {

        // HOWTO: You need to adapt the settings class as well, see the corresponding file.


        #region Private Variables

        private readonly AvalancheVisualizationSettings settings = new AvalancheVisualizationSettings();
        private byte[] text;
        private ICryptoolStream unchangedCipher;
        private ICryptoolStream modifiedCipher;
        private ICryptoolStream outputStream;
        private byte[] key;
        private byte[][] keyList = new byte[15][];
        private byte[][] sBox = new byte[16][];
        private int action = 1;
        private int roundNumber = 1;
        private byte[][] states = new byte[56][];
        private byte[][] statesB = new byte[56][];
        private byte[][] roundConstant = new byte[12][];
        private AvalanchePresentation pres = new AvalanchePresentation();
        private CStreamWriter outputStreamWriter = new CStreamWriter();
        static Random rnd = new Random();
        private Boolean execute = true;
        private Boolean suspended = false;
        private bool textChanged = false;
        int keysize;


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

        [PropertyInfo(Direction.InputData, "Text input", "original encrypted text", false)]
        public ICryptoolStream UnchangedCipher
        {
            get
            {
                return unchangedCipher;
            }
            set
            {

                this.unchangedCipher = value;
                OnPropertyChanged("UnchangedCipher");

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
                return outputStream;
            }
            set
            {
                this.outputStream = value;
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
            byte[] buffer = new byte[UnchangedCipher.Length];



            switch (settings.SelectedCategory)
            {
                case AvalancheVisualizationSettings.Category.Modern:

                    if (settings.Subcategory == 0)
                    {

                        string originalMessage = Encoding.Default.GetString(text);
                        //running = true;


                        if (textChanged)
                        {
                            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                            {


                                //pres.originalMsg.Text = originalMessage;
                                pres.modifiedMsg.Text = originalMessage;
                                pres.getTextBoxContent();
                                pres.modifyTxtBlock.Visibility = Visibility.Hidden;
                            }, null);
                            pres.textB = text;
                            checkTextLength();
                            roundNumber = 1;

                            // ProgressChanged(0, 1);

                            byte[] tempState = text;
                            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                            {
                                setRoundConstant();
                                pres.loadInitialState(tempState, false);

                            }, null);


                            statesB[0] = addKey(tempState, keyList[0]);

                            //pres.tempState = tempState;

                            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                            {
                                pres.createSBox();


                            }, null);

                            //

                            //expandKey();
                            setStatesTest(false);
                            //roundNumber = 1;

                            pres.statesB = statesB;



                            // ProgressChanged(1, 1);



                            //pres.keyList = keyList;

                        }
                        else
                        {
                            textChanged = true;
                            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                            {

                                pres.comparisonPaneAES();
                                pres.originalMsg.Text = originalMessage;

                            }, null);


                            pres.textA = text;

                            var encoding = Encoding.GetEncoding(437);
                            keysize = settings.KeyLength;

                            pres.keysize = keysize;
                            checkKeysize();
                            //padding PKCS7
                            checkTextLength();



                            roundNumber = 1;

                            // ProgressChanged(0, 1);
                            setRoundConstant();
                            byte[] tempState = text;
                            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                            {

                                pres.loadInitialState(tempState, true);

                            }, null);


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

                            //   
                            /* CStreamWriter writer = new CStreamWriter();
                             OutputStream = writer;
                             writer.Write(text);
                             writer.Close();*/

                            //OnPropertyChanged("OutputStream");
                            states[0] = addKey(tempState, keyList[0]);

                            pres.tempState = tempState;

                            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                            {
                                pres.createSBox();


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
                            




                            // ProgressChanged(1, 1);

                            setStatesTest(true);
                            roundNumber = 1;
                            pres.states = states;


                            pres.keyList = keyList;


                        }








                        if (!running)
                            return;
                        // while (pres.unfinished)
                        //{
                        //  ProgressChanged(0.5, 1);
                        //}

                        //outputStreamWriter.Write(states[39 + 8 * keysize]);


                        GuiLogMessage("Output" + outputStreamWriter.ToString(), NotificationLevel.Info);
                    }

                    if (settings.Subcategory == 2)
                    {
                        goto case AvalancheVisualizationSettings.Category.Hash;
                    }
                    break;
                //TODO
                //else--> DES


                case AvalancheVisualizationSettings.Category.Hash:



                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {

                        using (CStreamReader reader = UnchangedCipher.CreateReader())
                        {
                            reader.Read(buffer);

                        }

                        pres.comparisonPane();
                        //string otherText = Encoding.Default.GetString(buffer);
                        //GuiLogMessage(pres.decimalAsString(buffer), NotificationLevel.Info);

                        //pres.unchangedCipher = buffer;

                        if (textChanged)
                        {
                            string cipherB = pres.binaryAsString(buffer);
                            pres.modifiedCipher.Text = pres.hexaAsString(buffer);
                            pres.TB2.Text = cipherB;
                            pres.changedCipher = buffer;
                            pres.comparison();
                        }
                        else
                        {
                            //GuiLogMessage(pres.decimalAsString(buffer), NotificationLevel.Info);
                            string cipherA = pres.binaryAsString(buffer);
                            pres.originalCipher.Text = pres.hexaAsString(buffer);
                            pres.TB1.Text = cipherA;
                            pres.unchangedCipher = buffer;
                            pres.comparisonTxtBlock.Visibility = System.Windows.Visibility.Visible;
                        }


                        /*CStreamWriter writer2 = new CStreamWriter();
                        OutputStream = writer2;
                        writer2.Write(buffer);
                        OnPropertyChanged("OutputStream");
                        writer2.Close();*/

                    }, null);

                    textChanged = true;


                    break;

                case AvalancheVisualizationSettings.Category.Classic:

                    pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {

                        using (CStreamReader reader = UnchangedCipher.CreateReader())
                        {
                            reader.Read(buffer);

                        }

                        pres.comparisonPane();

                        string otherText = Encoding.Default.GetString(buffer);
                        //GuiLogMessage(pres.decimalAsString(buffer), NotificationLevel.Info);



                        if (textChanged)
                        {
                            //string cipherB = pres.binaryAsString(buffer);
                            pres.modifiedCipher.Text = otherText;
                            //pres.TB2.Text = cipherB;
                            pres.changedCipher = buffer;
                            resize();
                            pres.comparison();
                        }
                        else
                        {
                            GuiLogMessage(pres.decimalAsString(buffer), NotificationLevel.Info);
                            string cipherA = pres.binaryAsString(buffer);
                            pres.originalCipher.Text = otherText;
                            pres.TB1.Text = cipherA;
                            pres.unchangedCipher = buffer;
                            pres.comparisonTxtBlock.Visibility = System.Windows.Visibility.Visible;

                        }


                        /*CStreamWriter writer2 = new CStreamWriter();
                        OutputStream = writer2;
                        writer2.Write(buffer);
                        OnPropertyChanged("OutputStream");
                        writer2.Close();*/

                    }, null);

                    textChanged = true;

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
            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {

                pres.removeElements();
                // pres.modifyTxtBlock.Visibility = Visibility.Visible;
                //textChanged = false;
                //pres.modifiedMsg.IsReadOnly = false;

            }, null);
            //suspended = true;
            // Array.Clear(pres.textA, 0, pres.textA.Length);
            //Array.Clear(pres.textB, 0, pres.textB.Length);


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

        #region Methods
        public void createSBox()
        {
            int x = 0;
            while (x < 16)
            {
                this.sBox[x] = new byte[16];
                x++;
            }
            x = 0;
            byte[] temp = { 99, 124, 119, 123, 242, 107, 111, 197, 48, 1, 103, 43, 254, 215, 171, 118, 202, 130, 201, 125, 250, 89, 71, 240, 173, 212, 162, 175, 156, 164, 114, 192, 183, 253, 147, 38, 54, 63, 247, 204, 52, 165, 229, 241, 113, 216, 49, 21, 4, 199, 35, 195, 24, 150, 5, 154, 7, 18, 128, 226, 235, 39, 178, 117, 9, 131, 44, 26, 27, 110, 90, 160, 82, 59, 214, 179, 41, 227, 47, 132, 83, 209, 0, 237, 32, 252, 177, 91, 106, 203, 190, 57, 74, 76, 88, 207, 208, 239, 170, 251, 67, 77, 51, 133, 69, 249, 2, 127, 80, 60, 159, 168, 81, 163, 64, 143, 146, 157, 56, 245, 188, 182, 218, 33, 16, 255, 243, 210, 205, 12, 19, 236, 95, 151, 68, 23, 196, 167, 126, 61, 100, 93, 25, 115, 96, 129, 79, 220, 34, 42, 144, 136, 70, 238, 184, 20, 222, 94, 11, 219, 224, 50, 58, 10, 73, 6, 36, 92, 194, 211, 172, 98, 145, 149, 228, 121, 231, 200, 55, 109, 141, 213, 78, 169, 108, 86, 244, 234, 101, 122, 174, 8, 186, 120, 37, 46, 28, 166, 180, 198, 232, 221, 116, 31, 75, 189, 139, 138, 112, 62, 181, 102, 72, 3, 246, 14, 97, 53, 87, 185, 134, 193, 29, 158, 225, 248, 152, 17, 105, 217, 142, 148, 155, 30, 135, 233, 206, 85, 40, 223, 140, 161, 137, 13, 191, 230, 66, 104, 65, 153, 45, 15, 176, 84, 187, 22 };
            x = 0;
            int y = 0;
            foreach (byte b in temp)
            {
                sBox[y][x] = b;
                x++;
                if (x == 16)
                {
                    y++;
                    x = 0;
                }
            }

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

        private void setStatesTest(bool original)
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
                        if (original)
                        {
                            temp = states[x];
                        }
                        else
                        {
                            temp = statesB[x];
                        }
                        result = new byte[16];
                        foreach (byte b in temp)
                        {
                            result[z] = pres.sBox[getSBoxXPosition(temp[z])][getSBoxYPosition(temp[z])];
                            z++;
                        }
                        z = 0;
                        x++;
                        if (original)
                        {
                            states[x] = result;
                        }
                        else
                        {
                            statesB[x] = result;
                        }
                        y = 1;
                        break;
                    case 1:
                        temp = new byte[16];
                        if (original)
                        {
                            temp = states[x];
                        }
                        else
                        {
                            temp = statesB[x];
                        }
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
                        if (original)
                        {
                            states[x] = result;
                        }
                        else
                        {
                            statesB[x] = result;
                        }
                        y = 2;
                        break;
                    case 2:
                        temp = new byte[16];
                        result = new byte[16];
                        if (x < (38 + 8 * keysize))
                        {
                            z = 0;
                            if (original)
                            {
                                result = mixColumn(states[x]);
                            }
                            else
                            {
                                result = mixColumn(statesB[x]);
                            }

                            x++;
                            if (original)
                            {
                                states[x] = result;
                            }
                            else
                            {
                                statesB[x] = result;
                            }

                        }
                        y = 3;
                        break;
                    case 3:
                        temp = new byte[16];
                        result = new byte[16];
                        if (original)
                        {
                            result = addKey(states[x], keyList[roundNumber]);
                        }
                        else
                        {
                            result = addKey(statesB[x], keyList[roundNumber]);
                        }

                        x++;
                        if (original)
                        {
                            states[x] = result;
                        }
                        else
                        {
                            statesB[x] = result;
                        }
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

        private void setStatesB()
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
                        temp = statesB[x];
                        result = new byte[16];
                        foreach (byte b in temp)
                        {
                            result[z] = sBox[getSBoxXPosition(temp[z])][getSBoxYPosition(temp[z])];
                            z++;
                        }
                        z = 0;
                        x++;
                        statesB[x] = result;
                        y = 1;
                        break;
                    case 1:
                        temp = new byte[16];
                        temp = statesB[x];
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
                        statesB[x] = result;
                        y = 2;
                        break;
                    case 2:
                        temp = new byte[16];
                        result = new byte[16];
                        if (x < (38 + 8 * keysize))
                        {
                            z = 0;
                            result = mixColumn(statesB[x]);
                            x++;
                            statesB[x] = result;
                        }
                        y = 3;
                        break;
                    case 3:
                        temp = new byte[16];
                        result = new byte[16];
                        result = addKey(statesB[x], keyList[roundNumber]);
                        x++;
                        statesB[x] = result;
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
            while (z < 4)
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
            for (int x = 1; x < 11; x++)
            {
                byte[] roundConst = roundConstant[x - 1];
                byte[] prevKey = { keyList[x - 1][13], keyList[x - 1][14], keyList[x - 1][15], keyList[x - 1][12] };
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
                while (z < 4)
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
            while (x < 208)
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

        private void expandKey256()
        {
            byte[] tempkey = new byte[350];
            int x = 0;
            int y = 0;
            for (int r = 0; r < 32; r++)
            {
                tempkey[r] = keyList[x][y];
                y++;
                if (y == 16)
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
            if (text.Length != 16)
            {
                byte[] temp = new byte[16];
                int x = 0;
                int padding = 16 - text.Length;
                if (text.Length < 16)
                {
                    foreach (byte b in text)
                    {
                        temp[x] = b;
                        x++;
                    }
                    while (x < 16)
                    {
                        temp[x] = (byte)padding;
                        x++;
                    }
                }
                else
                {
                    while (x < 16)
                    {
                        temp[x] = text[x];
                        x++;
                    }
                }
                text = temp;


            }
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
