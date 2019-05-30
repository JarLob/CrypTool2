/*
   Copyright 2019 Christian Bender christian1.bender@student.uni-siegen.de

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
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using Cryptool.PluginBase.IO;
using ToyCiphers;
using ToyCiphers.Ciphers;
using ToyCiphers.Ciphers.Cipher1;
using ToyCiphers.Ciphers.Cipher2;
using ToyCiphers.Ciphers.Cipher3;
using ToyCiphers.Ciphers.Cipher4;
using ToyCiphers.Properties;
using ToyCiphers.UI;


namespace Cryptool.Plugins.ToyCiphers
{
    [Author("Christian Bender", "christian1.bender@student.uni-siegen.de", null, "http://www.uni-siegen.de")]
    [PluginInfo("ToyCiphers.Properties.Resources", "PluginCaption", "PluginTooltip", "ToyCiphers/userdoc.xml", new[] { "ToyCiphers/Images/IC_ToyCiphers.png" })]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class ToyCiphers : ICrypComponent
    {
        #region Private Variables

        private readonly ToyCiphersSettings settings = new ToyCiphersSettings();
        private readonly ToyCiphersPres _activePresentation = new ToyCiphersPres();
        private ICryptoolStream _messageInput;
        private ICryptoolStream _messageOutput;
        private IEncryption _currentCipher = null;
        private byte[] _key;
        private bool _stop = false;
        private bool _subkeysSatisfied = false;

        #endregion

        /// <summary>
        /// default constructor
        /// </summary>
        public ToyCiphers()
        {
            settings.PropertyChanged += new PropertyChangedEventHandler(SettingChangedListener);
        }

        #region Data Properties

        /// <summary>
        /// Input for messages
        /// </summary>
        [PropertyInfo(Direction.InputData, "MessageInput", "MessageInputTooltip")]
        public ICryptoolStream MessageInput
        {
            get { return _messageInput; }
            set
            {
                _messageInput = value;
                OnPropertyChanged("MessageInput");
            }
        }

        /// <summary>
        /// Output for encrypted messages
        /// </summary>
        [PropertyInfo(Direction.OutputData, "MessageOutput", "MessageOutputTooltip")]
        public ICryptoolStream MessageOutput
        {
            get { return _messageOutput; }
            set
            {
                _messageOutput = value;
                OnPropertyChanged("MessageOutput");
            }
        }

        /// <summary>
        /// Input for the key
        /// </summary>
        [PropertyInfo(Direction.InputData, "KeyInput", "KeyInputTooltip", true)]
        public byte[] KeyInput
        {
            get { return _key; }
            set
            {
                _key = value;
                OnPropertyChanged("KeyInput");
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
            get { return _activePresentation; }
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
            ProgressChanged(0, 1);

            // Check specific algorithm and invoke the selection into the UI class
            if (settings.CurrentAlgorithm == Algorithms.Cipher1)
            {
                //create encryption object for the cipher
                _currentCipher = new Cipher1();
                int[] subKeys = readSubkeys(Cipher1Configuration.KEYNUM);
                if (subKeys != null)
                {
                    _currentCipher.SetKeys(subKeys);
                    _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                    {
                        ((Cipher1Pres)_activePresentation.MainGrid.Children[0]).Keys = subKeys;
                    }, null);
                }
            }
            else if (settings.CurrentAlgorithm == Algorithms.Cipher2)
            {
                //create encryption object for the cipher
                _currentCipher = new Cipher2();
                int[] subKeys = readSubkeys(Cipher2Configuration.KEYNUM);
                if (subKeys != null)
                {
                    _currentCipher.SetKeys(subKeys);
                    _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                    {
                        ((Cipher2Pres)_activePresentation.MainGrid.Children[0]).Keys = subKeys;
                    }, null);
                }
            }
            else if (settings.CurrentAlgorithm == Algorithms.Cipher3)
            {
                //create encryption object for the cipher
                _currentCipher = new Cipher3();
                int[] subKeys = readSubkeys(Cipher3Configuration.KEYNUM);
                if (subKeys != null)
                {
                    _currentCipher.SetKeys(subKeys);
                    _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                    {
                        ((Cipher3Pres)_activePresentation.MainGrid.Children[0]).Keys = subKeys;
                    }, null);
                }
            }
            else if (settings.CurrentAlgorithm == Algorithms.Cipher4)
            {
                //create encryption object for the cipher
                _currentCipher = new Cipher4();
                int[] subKeys = readSubkeys(Cipher4Configuration.KEYNUM);
                if (subKeys != null)
                {
                    _currentCipher.SetKeys(subKeys);
                    _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                    {
                        ((Cipher4Pres)_activePresentation.MainGrid.Children[0]).Keys = subKeys;
                    }, null);
                }
            }

            if (!_subkeysSatisfied)
            {
                GuiLogMessage(Resources.KeyError, NotificationLevel.Error);
                ProgressChanged(1, 1);
                return;
            }

            List<int> messageList = new List<int>();
            List<UInt16> encryptedMessageList = new List<UInt16>();

            //Read all messages
            using (CStreamReader reader = _messageInput.CreateReader())
            {
                if ((reader.Length % 2) != 0)
                {
                    GuiLogMessage(Resources.MessageError, NotificationLevel.Error);
                    ProgressChanged(1, 1);
                    return;
                }

                byte[] inputBlock = new byte[2];
                int message;
                int readcount = 0;
                while ((readcount += reader.Read(inputBlock, readcount, 4 - readcount)) < 4 &&
                       reader.Position < reader.Length && !_stop)
                {

                }
                message = BitConverter.ToUInt16(inputBlock, 0);
                messageList.Add(message);
            }

            //encrypt all messages
            foreach (var message in messageList)
            {
                int encryptedMessage = _currentCipher.EncryptBlock(message);
                encryptedMessageList.Add(Convert.ToUInt16(encryptedMessage));
            }
            
            //write all messages to the output
            using (CStreamWriter writer = new CStreamWriter())
            {
                foreach (var enctyptedMessage in encryptedMessageList)
                {
                    byte[] outputBlock = BitConverter.GetBytes(enctyptedMessage);
                    writer.Write(outputBlock, 0, outputBlock.Length);
                }

                writer.Flush();
                MessageOutput = writer;
            }

            ProgressChanged(1, 1);
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

        #region methods

        /// <summary>
        /// applies the keys specified by the user to the cipher
        /// </summary>
        /// <param name="keycount"></param>
        /// <returns></returns>
        private int[] readSubkeys(int keycount)
        {
            int[] keys = new int[keycount];

            if (settings.CurrentAlgorithm == Algorithms.Cipher1 || settings.CurrentAlgorithm == Algorithms.Cipher2 ||
                settings.CurrentAlgorithm == Algorithms.Cipher2)
            {
                if (KeyInput.Length != (keycount * 2))
                {
                    _subkeysSatisfied = false;
                    return null;
                }

                for (int i = 0; i < (KeyInput.Length / 2); i++)
                {
                    byte[] key = new byte[2];
                    for (int j = 0; j < 2; j++)
                    {
                        key[1 - j] = KeyInput[(i * 2) + j];
                    }
                    keys[i] = BitConverter.ToUInt16(key, 0);
                }

            }
            else
            {
                if (KeyInput.Length != keycount)
                {
                    _subkeysSatisfied = false;
                    return null;
                }

                for (int i = 0; i < (KeyInput.Length); i++)
                {
                    byte[] key = new byte[2];
                    key[0] = KeyInput[i];
  
                    keys[i] = BitConverter.ToUInt16(key, 0);
                }
            }

            _subkeysSatisfied = true;
            return keys;
        } 


        /// <summary>
        /// Handles changes within the settings class
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingChangedListener(object sender, PropertyChangedEventArgs e)
        {
            //Listen for changes of the current chosen algorithm
            if (e.PropertyName == "CurrentAlgorithm")
            {
                //Check specific algorithm and invoke the selection into the UI class
                if (settings.CurrentAlgorithm == Algorithms.Cipher1)
                {
                    //dispatch action: clear the active grid and add the specific algorithm visualization
                    _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                    {
                        _activePresentation.MainGrid.Children.Clear();
                        _activePresentation.MainGrid.Children.Add(new Cipher1Pres());
                    }, null);
                }
                else if (settings.CurrentAlgorithm == Algorithms.Cipher2)
                {
                    _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                    {
                        _activePresentation.MainGrid.Children.Clear();
                        _activePresentation.MainGrid.Children.Add(new Cipher2Pres());
                    }, null);
                }
                else if (settings.CurrentAlgorithm == Algorithms.Cipher3)
                {
                    _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                    {
                        _activePresentation.MainGrid.Children.Clear();
                        _activePresentation.MainGrid.Children.Add(new Cipher3Pres());
                    }, null);
                }
                else if (settings.CurrentAlgorithm == Algorithms.Cipher4)
                {
                    _activePresentation.Dispatcher.Invoke(DispatcherPriority.Send, (SendOrPostCallback)delegate
                    {
                        _activePresentation.MainGrid.Children.Clear();
                        _activePresentation.MainGrid.Children.Add(new Cipher4Pres());
                    }, null);
                }
            }
        }

        #endregion
    }
}
