/*
   Copyright 2018 CrypTool 2 Team <ct2contact@cryptool.org>

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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Controls;
using System.Xml;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using FormatPreservingEncryptionWeydstone;

/*
 * Das Plugin ist momentan nur ein Prototyp und wird im Anschluss der Abgabe fertiggestellt.
 * Die Funktionalität ist aber bei korrekten Eingaben gegeben.
 */

namespace Cryptool.Plugins.FormatPreservingEncryption
{
    // HOWTO: Change author name, email address, organization and URL.
    [Author("Alexander Hirsch", "a.hirsch.ks@arcor.de", "Universität Kassel", "https://www.uni-kassel.de/uni/")]
    // HOWTO: Change plugin caption (title to appear in CT2) and tooltip.
    // You can (and should) provide a user documentation as XML file and an own icon.
    [PluginInfo("Format Preserving Encryption", "Provides the FF1, FF2 and FF3 encryption standards", "FormatPreservingEncryption/userdoc.xml", new[] { "FormatPreservingEncryption/Images/FormatPreservingEncryptionIcon.png" })]


    [ComponentCategory(ComponentCategory.CiphersModernSymmetric)]
    public class FormatPreservingEncryption : ICrypComponent
    {
        #region Private Variables

        private readonly FormatPreservingEncryptionSettings settings = new FormatPreservingEncryptionSettings();

        #endregion

        #region Data Properties


        [PropertyInfo(Direction.InputData, "Alphabet", "The Alphabet", true)]
        public string Alphabet
        {
            get;
            set;
        }

        //TODO rename
        [PropertyInfo(Direction.InputData, "Plaintext", "The Plaintext as String", true)]
        public string Plaintext
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "Key", "The Key", true)]
        public byte[] Key
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "Tweak", "The Tweak", false)]
        public byte[] Tweak
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "Tweak Radix", "The Tweak Radix", false)]
        public int TweakRadix
        {
            get;
            set;
        }

        //TODO rename
        [PropertyInfo(Direction.OutputData, "Ciphertext", "Ciphertext as String")]
        public string Ciphertext
        {
            get;
            set;
        }

        //TODO rename
        [PropertyInfo(Direction.OutputData, "Info", "Info")]
        public string Info
        {
            get;
            set;
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
            get { return null; }
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
            // HOWTO: Use this to show the progress of a plugin algorithm execution in the editor.
            ProgressChanged(0, 1);
            //check Key

            //check Tweak
            if(Tweak == null)
            {
                Tweak = new byte[] { };
            }
            //check Plaintext

            //switch mode
            if (settings.Mode == (int)Modes.Normal)
            {
                GuiLogMessage("Normal Mode", NotificationLevel.Debug);
                //validate Alphabet and create mapping
                if (Alphabet == null || Alphabet.Length < 2) {
                    GuiLogMessage("Alphabet is missing or too short (length < 2)", NotificationLevel.Error);
                    return;
                }

                if (!validateAlphabet(Alphabet))
                {
                    //TODO Error
                    return;
                }

                char[] mapping = Alphabet.ToCharArray();

                int radix = mapping.Length;
                GuiLogMessage("Radix is " + radix, NotificationLevel.Debug);

                //validate Plaintext
                if (Plaintext == null || Plaintext.Length < 2)
                {
                    GuiLogMessage("Plaintext is missing or too short (length < 2)", NotificationLevel.Error);
                    return;
                }

                //Convert Plaintext
                int[] intPlaintext = new int[Plaintext.Length];

                for(int i = 0; i < Plaintext.Length; i++)
                {
                    int index = Alphabet.IndexOf(Plaintext[i]);

                    if(index < 0)
                    {
                        GuiLogMessage("Something went wrong during plaintext conversion", NotificationLevel.Error);
                        return;
                    }

                    intPlaintext[i] = index;
                }
                int[] intCiphertext = { };

                switch (settings.Algorithm)
                {
                    case (int)Algorithms.FF1:
                        {

                            GuiLogMessage("FF1", NotificationLevel.Debug);

                            FF1 ff1 = new FF1(radix, Constants.MAXLEN);
                            ff1.OutputChanged += OutputChanged;
                            if (settings.Action == (int)Actions.Encrypt)
                            {
                                intCiphertext = ff1.encrypt(Key, Tweak, intPlaintext);
                            }
                            else if (settings.Action == (int)Actions.Decrypt)
                            {
                                intCiphertext = ff1.decrypt(Key, Tweak, intPlaintext);
                            }
                            string stringCiphertext = "";
                            for (int i = 0; i < intCiphertext.Length; i++)
                            {
                                int value = intCiphertext[i];
                                stringCiphertext += mapping[value];
                            }
                            GuiLogMessage("string ciphertext is " + stringCiphertext, NotificationLevel.Debug);
                            Ciphertext = stringCiphertext;
                            break;
                        }
                    case (int)Algorithms.FF2:
                        {

                            GuiLogMessage("FF2", NotificationLevel.Debug);
                            

                            FF2 ff2 = new FF2(radix, TweakRadix);
                            ff2.OutputChanged += OutputChanged;
                            //ff2.OutputChanged += OutputChanged;
                            if (settings.Action == (int)Actions.Encrypt)
                            {
                                intCiphertext = ff2.encrypt(Key, Tweak, intPlaintext);
                            }
                            else if (settings.Action == (int)Actions.Decrypt)
                            {
                                intCiphertext = ff2.decrypt(Key, Tweak, intPlaintext);
                            }
                            string stringCiphertext = "";
                            for (int i = 0; i < intCiphertext.Length; i++)
                            {
                                int value = intCiphertext[i];
                                stringCiphertext += mapping[value];
                            }
                            GuiLogMessage("string ciphertext is " + stringCiphertext, NotificationLevel.Debug);
                            Ciphertext = stringCiphertext;
                            break;
                        }
                    case (int)Algorithms.FF3:
                        {
                            GuiLogMessage("FF3", NotificationLevel.Debug);
                            FF3 ff3 = new FF3(radix);
                            ff3.OutputChanged += OutputChanged;

                            if (settings.Action == (int)Actions.Encrypt) {
                                intCiphertext = ff3.encrypt(Key, Tweak, intPlaintext);
                            }
                            else if (settings.Action == (int)Actions.Decrypt)
                            {
                                intCiphertext = ff3.decrypt(Key, Tweak, intPlaintext);
                            }
                            
                            string stringCiphertext = "";
                            for (int i = 0; i < intCiphertext.Length; i++)
                            {
                                int value = intCiphertext[i];
                                stringCiphertext += mapping[value];
                            }
                            GuiLogMessage("string ciphertext is " + stringCiphertext, NotificationLevel.Debug);
                            Ciphertext = stringCiphertext;
                            break;
                        }
                    default:
                        {
                            GuiLogMessage("Something went wrong. Algorithm not supported", NotificationLevel.Error);
                            break;
                        }
                }
            }
            else if(settings.Mode == (int)Modes.XML)
            {

                StringWriter sws = new StringWriter();
                XmlDocument xmlDo = new XmlDocument();
                xmlDo.LoadXml(Plaintext);
                xmlDo.Save(sws);
                Ciphertext = sws.ToString();
                OnPropertyChanged("Ciphertext");


                string[] tasks = Alphabet.Split(new char[]{';'});
                foreach(string task in tasks)
                {
                    if(task.Length<=1 || !task.Contains("#"))
                    {
                        GuiLogMessage("Task is invalid. Too short or Alphabet missing", NotificationLevel.Error);
                        return;
                    }
                    string[] splits = task.Split(new char[] { '#' });
                    string xpath = splits[0];
                    GuiLogMessage("xpath is: " + xpath, NotificationLevel.Debug);
                    string alphabet = splits[1];
                    GuiLogMessage("Alphabet is: " + alphabet, NotificationLevel.Debug);


                    //validate Alphabet and create mapping
                    if (alphabet == null || alphabet.Length < 2)
                    {
                        GuiLogMessage("Alphabet is missing or too short (length < 2)", NotificationLevel.Error);
                        continue;
                    }

                    if (!validateAlphabet(alphabet))
                    {
                        //TODO Error
                        continue;
                    }

                    char[] mapping = alphabet.ToCharArray();

                    int radix = mapping.Length;
                    GuiLogMessage("Radix is " + radix, NotificationLevel.Debug);



                    //validate Plaintext
                    if (Plaintext == null || Plaintext.Length < 2)
                    {
                        GuiLogMessage("Plaintext is missing or too short (length < 2)", NotificationLevel.Error);
                        return;
                    }

                    string xmlString = Plaintext; 

                    //xmlString = xmlString.Replace(" ", "");
                    try
                    {


                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(xmlString);
                        XmlNodeList nodes = xmlDoc.SelectNodes(xpath);
                        foreach (XmlNode node in nodes)
                        {
                            if (node.InnerText.Length > 0)
                            {
                                GuiLogMessage("Innertext is: ", NotificationLevel.Debug);
                                GuiLogMessage(node.InnerText, NotificationLevel.Debug);



                                int[] intPlaintext = new int[node.InnerText.Length];

                                for (int i = 0; i < node.InnerText.Length; i++)
                                {
                                    int index = alphabet.IndexOf(node.InnerText[i]);
                                    if (index < 0)
                                    {
                                        GuiLogMessage("Something went wrong during plaintext conversion", NotificationLevel.Error);
                                        return;
                                    }

                                    intPlaintext[i] = index;
                                }

                                int[] intCiphertext = { };

                                switch (settings.Algorithm)
                                {
                                    case (int)Algorithms.FF1:
                                        {

                                            GuiLogMessage("FF1", NotificationLevel.Debug);


                                            FF1 ff1 = new FF1(radix, Constants.MAXLEN);
                                            ff1.OutputChanged += OutputChanged;

                                            if (settings.Action == (int)Actions.Encrypt)
                                            {
                                                intCiphertext = ff1.encrypt(Key, Tweak, intPlaintext);
                                            }
                                            else if (settings.Action == (int)Actions.Decrypt)
                                            {
                                                intCiphertext = ff1.decrypt(Key, Tweak, intPlaintext);
                                            }

                                            string stringCiphertext = "";
                                            for (int i = 0; i < intCiphertext.Length; i++)
                                            {
                                                int value = intCiphertext[i];
                                                stringCiphertext += mapping[value];
                                            }
                                            GuiLogMessage("string ciphertext is " + stringCiphertext, NotificationLevel.Debug);
                                            node.InnerText = stringCiphertext;

                                            xmlDoc.ImportNode(node, true);

                                            break;
                                        }
                                    case (int)Algorithms.FF3:
                                        {
                                            GuiLogMessage("FF3", NotificationLevel.Debug);


                                            FF3 ff3 = new FF3(radix);
                                            ff3.OutputChanged += OutputChanged;

                                            if (settings.Action == (int)Actions.Encrypt)
                                            {
                                                intCiphertext = ff3.encrypt(Key, Tweak, intPlaintext);
                                            }
                                            else if (settings.Action == (int)Actions.Decrypt)
                                            {
                                                intCiphertext = ff3.decrypt(Key, Tweak, intPlaintext);
                                            }

                                            string stringCiphertext = "";
                                            for (int i = 0; i < intCiphertext.Length; i++)
                                            {
                                                int value = intCiphertext[i];
                                                stringCiphertext += mapping[value];
                                            }
                                            GuiLogMessage("string ciphertext is " + stringCiphertext, NotificationLevel.Debug);
                                            node.InnerText = stringCiphertext;

                                            xmlDoc.ImportNode(node, true);
                                            break;
                                        }
                                    case (int)Algorithms.FF2:
                                        {
                                            GuiLogMessage("FF2", NotificationLevel.Debug);


                                            FF2 ff2 = new FF2(radix, TweakRadix);
                                            ff2.OutputChanged += OutputChanged;

                                            if (settings.Action == (int)Actions.Encrypt)
                                            {
                                                intCiphertext = ff2.encrypt(Key, Tweak, intPlaintext);
                                            }
                                            else if (settings.Action == (int)Actions.Decrypt)
                                            {
                                                intCiphertext = ff2.decrypt(Key, Tweak, intPlaintext);
                                            }

                                            string stringCiphertext = "";
                                            for (int i = 0; i < intCiphertext.Length; i++)
                                            {
                                                int value = intCiphertext[i];
                                                stringCiphertext += mapping[value];
                                            }
                                            GuiLogMessage("string ciphertext is " + stringCiphertext, NotificationLevel.Debug);
                                            node.InnerText = stringCiphertext;

                                            xmlDoc.ImportNode(node, true);
                                            break;
                                        }


                                    default:
                                        {
                                            GuiLogMessage("Something went wrong. Algorithm not supported", NotificationLevel.Error);
                                            break;
                                        }
                                }
                            }
                        }

                        GuiLogMessage("new xml doc is: " + xmlDoc.ToString(), NotificationLevel.Debug);

                        nodes = xmlDoc.SelectNodes(xpath);
                        foreach (XmlNode node in nodes)
                        {
                            if (node.InnerText.Length > 0)
                            {
                                GuiLogMessage("NEW Innertext is: ", NotificationLevel.Debug);
                                GuiLogMessage(node.InnerText, NotificationLevel.Debug);
                            }
                        }

                        StringWriter sw = new StringWriter();
                        xmlDoc.Save(sw);
                        Ciphertext = sw.ToString();
                        Plaintext = sw.ToString();

                    }
                    catch (Exception e)
                    {
                        GuiLogMessage("source: " + e.Source + "text: " + e.Message, NotificationLevel.Debug);
                    }
                }
            }
            else
            {
                GuiLogMessage("Mode not supported", NotificationLevel.Error);
            }

            OnPropertyChanged("Ciphertext");
            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
            GuiLogMessage("FPE: PostExecution called", NotificationLevel.Debug);
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
            GuiLogMessage("FPE: Initialize called", NotificationLevel.Debug);
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

        private void OutputChanged(object sender, OutputChangedEventArgs e)
        {
            AddToInfoString(e.Text);
        }

        #endregion
        #region methods

        private void AddToInfoString(string s)
        {
            Info = s;
            OnPropertyChanged("Info");
        }

        private bool validateAlphabet(string Alphabet)
        {
            char[] mapping = Alphabet.ToCharArray();

                //validate each char is unique
                for (int i = 0; i<mapping.Length-1; i++)
                {
                    char c = mapping[i];
                    for (int j = i+1; j<mapping.Length; j++)
                    {
                        if (c == mapping[j])
                        {
                            GuiLogMessage("Alphabet must consist of distinct values. Found multiple occurrences of character '" + c + "' in Alphabet " + mapping.ToString(), NotificationLevel.Error);
                            return false;
                        }
}
                }
            return true;
        }


        #endregion
    }
}