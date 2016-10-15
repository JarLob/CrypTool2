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
        private byte[] key;
        private ICryptoolStream unchangedCipher;
        private ICryptoolStream outputStream;
        private AES aes = new AES();
        private DES des;
        private AvalanchePresentation pres = new AvalanchePresentation();
        private CStreamWriter outputStreamWriter = new CStreamWriter();
        private bool textChanged = false;
       



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
                        pres.mode = 0;


                        string inputMessage = Encoding.Default.GetString(text);



                        if (textChanged && !pres.singleBitChange)
                        {
                            aes.text = text;

                            //pres.myMethod(aes);
                            // GuiLogMessage(pres.decimalAsString(text), NotificationLevel.Info);

                            /* pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                             {
                                 //pres.modifiedMsg.Text = inputMessage;                               
                                 pres.getTextBoxContent();
                                 //pres.loadInitialState(temporary, false);
                                 //pres.modifyTxtBlock.Visibility = Visibility.Hidden;

                             }, null);*/

                            pres.textB = text;
                            byte[] temporary = aes.checkTextLength();
                            aes.executeAES(false);


                            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                            {
                                pres.loadInitialState(temporary, false);
                                pres.setAndLoadButtons();



                            }, null);

                            pres.statesB = aes.statesB;

                        }
                        else if (!textChanged && !pres.singleBitChange)
                        {
                            textChanged = true;

                           aes.text = text;
                           aes.key = key;

                            pres.keysize = settings.KeyLength;


                            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                            {
                                pres.comparisonPane();
                                //pres.originalMsg.Text = inputMessage;


                            }, null);


                            pres.textA = text;
                            pres.key = key;

                            AES.keysize = settings.KeyLength;
                            //pres.keysize = settings.KeyLength;
                            aes.checkKeysize();
                            byte[] temporary = aes.checkTextLength();
                            aes.executeAES(true);
                            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                            {
                                
                                pres.loadInitialState(temporary, true);


                            }, null);

                            pres.states = aes.states;
                            pres.keyList = aes.keyList;

                        }

                        if (!running)
                            return;

                        // GuiLogMessage("Output" + outputStreamWriter.ToString(), NotificationLevel.Info);
                    }

                    if (settings.Subcategory == 1)
                    {
                        pres.mode = 1;

                        if (textChanged)
                        {
                            des.inputMessage = text;
                            des.textChanged = true;
                            des.DESProcess();

                            pres.lrDataB = des.lrDataB;
                            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                            {

                                pres.setAndLoadButtons();

                            }, null);

                            // MessageBox.Show(pres.lrDataB[1, 0]);
                        }
                        else
                        {


                            des.inputKey = key;
                            des.inputMessage = text;
                            des.DESProcess();
                            pres.lrData = des.lrData;
                            textChanged = true;
                            pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                            {
                                pres.comparisonPane();

                                //MessageBox.Show(des.lrData[1, 0]);

                            }, null);
                            //MessageBox.Show(pres.lrData[1, 0]);
                        }


                        if (!running)
                            return;

                    }

                    else
                    {
                        goto case AvalancheVisualizationSettings.Category.Hash;
                    }
                    break;



                case AvalancheVisualizationSettings.Category.Hash:

                    pres.mode = 2;

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
                        pres.mode = 2;

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
