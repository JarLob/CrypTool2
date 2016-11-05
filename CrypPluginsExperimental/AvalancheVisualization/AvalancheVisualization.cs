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
        private ICryptoolStream text;
        private ICryptoolStream key;
        byte[] originalText;
        byte[] originalKey;
        private byte[] inputObject = { };
        string msgA;
        string msgB;
        private ICryptoolStream outputStream;
        private AES aes = new AES();
        private DES des = new DES();
        private AvalanchePresentation pres = new AvalanchePresentation();
        private CStreamWriter outputStreamWriter = new CStreamWriter();
        private bool textChanged = false;




        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "Key", "Enter key", false)]
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

        [PropertyInfo(Direction.InputData, "Message", "Enter message", true)]
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

     /*   [PropertyInfo(Direction.InputData, "Text input", "third input", false)]
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
        }*/



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
            byte[] textInput = new byte[Text.Length];
      

            switch (settings.SelectedCategory)
            {
                case AvalancheVisualizationSettings.Category.Prepared:

               
                        byte[] keyInput = new byte[Key.Length];

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
                            pres.mode = 0;



                            string inputMessage = Encoding.Default.GetString(textInput);



                            if (textChanged && pres.canModify)
                            {
                                aes.text = textInput;
                                aes.key = keyInput;

                                //pres.myMethod(aes);
                                // GuiLogMessage(pres.decimalAsString(text), NotificationLevel.Info);

                                /* pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                 {
                                     //pres.modifiedMsg.Text = inputMessage;                               
                                     pres.getTextBoxContent();
                                     //pres.loadInitialState(temporary, false);
                                     //pres.modifyTxtBlock.Visibility = Visibility.Hidden;

                                 }, null);*/


                                // pres.key = key;
                                byte[] temporary = aes.checkTextLength();
                                byte[] tmpKey = aes.checkKeysize();
                                pres.key = tmpKey;
                                pres.textB = temporary;
                                aes.executeAES(false);


                                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                {
                                    pres.setAndLoadButtons();

                                //if (!originalText.Equals(text))
                                    pres.loadChangedMsg(temporary, true);
                                //if (!originalKey.Equals(tmpKey))
                                    pres.loadChangedKey(tmpKey);

                                    pres.coloringText();
                                    pres.coloringKey();
                                    pres.updateDataColor();
                                }, null);

                                pres.statesB = aes.statesB;

                            }
                            else if (!textChanged && !pres.canModify)
                            {
                                textChanged = true;

                                originalText = textInput;

                                aes.text = textInput;
                                aes.key = keyInput;

                                pres.keysize = settings.KeyLength;

                               
                                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                {
                                    if (pres.skip.IsChecked == true)
                                        pres.comparisonPane();
                                    else
                                    pres.instructions();
                                   // pres.comparisonPane();
                                //pres.originalMsg.Text = inputMessage;


                            }, null);


                                // pres.textA = text;
                                //pres.key = key;

                                AES.keysize = settings.KeyLength;
                                //pres.keysize = settings.KeyLength;
                                byte[] tmpKey = aes.checkKeysize();
                                originalKey = tmpKey;
                                pres.key = tmpKey;
                                pres.keyA = tmpKey;
                                byte[] temporary = aes.checkTextLength();
                                pres.textA = temporary;
                                aes.executeAES(true);
                                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                {

                                    pres.loadInitialState(temporary, tmpKey);


                                }, null);

                                pres.states = aes.states;
                                pres.keyList = aes.keyList;

                            }

                            if (!running)
                                return;

                            // GuiLogMessage("Output" + outputStreamWriter.ToString(), NotificationLevel.Info);
                        }
                        // if settings==1
                        else 
                        {
                            pres.mode = 1;

                            bool valid = validSize();

                            if (textChanged && pres.canModify)
                            {

                                des.inputKey = keyInput;
                                des.inputMessage = textInput;

                                des.textChanged = true;
                                des.DESProcess();
                                pres.key = keyInput;
                                pres.textB = textInput;
                                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                {

                                    pres.setAndLoadButtons();


                                    pres.loadChangedMsg(textInput, true);

                                    pres.loadChangedKey(keyInput);

                                    pres.coloringText();
                                    pres.coloringKey();

                                    pres.updateDataColor();
                                }, null);

                           
                                pres.lrDataB = des.lrDataB;


                            }
                            else if (!textChanged && !pres.canModify)
                            {


                                des.inputKey = keyInput;
                                des.inputMessage = textInput;


                                byte[] tmpKey = keyInput;
                                byte[] tmpText = textInput;
                                originalText = tmpText;
                                originalKey = tmpKey;

                                des.textChanged = false;

                                des.DESProcess();

                                //pres.key = tmpKey;
                                pres.keyA = tmpKey;
                                textChanged = true;
                                pres.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                {
                                    if (pres.skip.IsChecked == true)
                                        pres.comparisonPane();
                                    else
                                        pres.instructions();

                                    pres.loadInitialState(textInput, keyInput);
                                //MessageBox.Show(des.lrData[1, 0]);

                            }, null);
                                //MessageBox.Show(pres.lrData[1, 0]);
                                pres.textA = textInput;
                                pres.lrData = des.lrData;
                            }


                            if (!running)
                                return;

                        }
                    
                    
                    break;



                case AvalancheVisualizationSettings.Category.Unprepared:


                 
                   

                    switch (settings.UnprepSelection)
                    {
                        //Hash functions
                        case 0:

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
                                        pres.modifiedMsg.Text = pres.hexaAsString(textInput);
                                        pres.TB2.Text = cipherB;
                                        pres.changedCipher = textInput;
                                        pres.radioHexOthers.IsChecked = true;
                                        pres.comparison();
                                   
                                        
                                       
                                    }
                                    else
                                    {
                                        if (pres.skip.IsChecked == true)
                                            pres.comparisonPane();
                                        else
                                            pres.instructions();

                                        string cipherA = pres.binaryAsString(textInput);
                                        pres.originalMsg.Text = pres.hexaAsString(textInput);
                                        pres.TB1.Text = cipherA;
                                        pres.unchangedCipher = textInput;
                                     
                                        //  string cipherA = pres.binaryAsString(inputObject);
                                        //  pres.originalMsg.Text = pres.hexaAsString(inputObject);
                                    }


                                    /*CStreamWriter writer2 = new CStreamWriter();
                                    OutputStream = writer2;
                                    writer2.Write(buffer);
                                    OnPropertyChanged("OutputStream");
                                    writer2.Close();*/

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
                                    //GuiLogMessage(pres.decimalAsString(buffer), NotificationLevel.Info);



                                    if (textChanged)
                                    {
                                        msgB = otherText;
                                        bool validEntry= checkSize(msgA,msgB);

                                        if (validEntry)
                                        {
                                            string cipherB = pres.binaryAsString(textInput);
                                            pres.modifiedMsg.Text = otherText;
                                            pres.TB2.Text = cipherB;
                                            pres.changedCipher = textInput;
                                            //resize();
                                            pres.comparison();
                                        }else
                                        {
                                            GuiLogMessage("Modification must have same length as the initial input.",NotificationLevel.Warning);
                                        }
                                    }
                                    else
                                    {
                                        if (pres.skip.IsChecked == true)
                                            pres.comparisonPane();
                                        else
                                            pres.instructions();
                                        //GuiLogMessage(pres.decimalAsString(buffer), NotificationLevel.Info);
                                        string cipherA = pres.binaryAsString(textInput);
                                        pres.originalMsg.Text = otherText;
                                        msgA = otherText;
                                        pres.TB1.Text = cipherA;
                                        pres.unchangedCipher = textInput;
                                        // pres.comparisonTxtBlock.Visibility = System.Windows.Visibility.Visible;

                                    }


                                    /*CStreamWriter writer2 = new CStreamWriter();
                                    OutputStream = writer2;
                                    writer2.Write(buffer);
                                    OnPropertyChanged("OutputStream");
                                    writer2.Close();*/

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
        public bool checkSize(string A, string B)
        {
            if(A.Length!=B.Length)
                return false;

            return true;
        }

        public bool validSize()
        {
            if (key.Length != 8)
                throw new Exception("Invalid key");
            if (text.Length != 8)
                throw new Exception("Invalid text");
            if (key.Length == 8 && text.Length == 8)
                return true;

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
