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
using BitcoinBlockChainAnalyser;
using System.Net.Sockets;
using System;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cryptool.Plugins.BitcoinBlockDownloader
{

    [Author("Dominik Vogt", "dvogt@posteo.de", null, null)]
    [PluginInfo("BitcoinBlockDownloader.Properties.Resources","PluginCaption", "PluginToolTip",
        "BitcoinBlockDownloader/userdoc.xml", new[] { "BitcoinBlockDownloader/Images/BC_Logo_.png" })]

    [ComponentCategory(ComponentCategory.Protocols)]
    public class BitcoinBlockDownloader : ICrypComponent
    {
        #region Private Variables

        private readonly BitcoinBlockDownloaderSettings settings;

        //private variables for server connection
        private TcpClient client = null;
        String hostname = null;
        int port = 0;
        NetworkStream networkStream = null;

        #endregion

        #region Data Properties

        /// <summary>
        /// Constructor
        /// </summary>
        public BitcoinBlockDownloader()
        {
            this.settings = new BitcoinBlockDownloaderSettings();
        }

        /// <summary>
        /// Get or set all settings for this algorithm.
        /// </summary>
        public ISettings Settings
        {
            get { return this.settings; }
        }


        private string _inputBlock;

        /// <summary>
        /// The Downloaeder needs an input value like a number between zero and the highest number in blockchain
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputBlockCaption", "InputBlockTooltip", true)]
        public string InputBlock
        {
            get { return _inputBlock; }
            set { _inputBlock = value; }
        }

        /// <summary>
        /// The Downloader returns the Block information from the response Blocknumber
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", true)]
        public string OutputString
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members

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
            //start an connection to the server with the blockchain
            try
            {
                hostname = settings.Hostname;
                port = settings.Port;
                client = new TcpClient();
                client.Connect(hostname, port);
                client.ReceiveTimeout = 5000;
                networkStream = client.GetStream();


                try
                {
                    //get the actual highest blocknumber
                    string buffer = BlockChainDownloader.HighestBlockDownloader(networkStream);
                    //try to parse the actual highes blochnumber
                    int blockCount;
                    if (!(int.TryParse(buffer, out blockCount))) 
                    {
                        GuiLogMessage("Server has an Error with " + buffer, NotificationLevel.Warning);
                    }
                    else
                    {
                        int blockNumber;
                        if (int.TryParse(InputBlock, out blockNumber))
                        {
                            //catch entries lower then 0 and put the genesis block
                            if (blockNumber < 0)
                            {
                                OutputString = BlockChainDownloader.BlockDownloader(networkStream, blockCount.ToString());
                                GuiLogMessage("The least block has the number 0!", NotificationLevel.Info);
                            }
                            //Download the requested block
                            else if (blockNumber >= 0 & blockNumber <= blockCount)
                            {
                                OutputString = BlockChainDownloader.BlockDownloader(networkStream, InputBlock.ToString());
                            }
                            else
                            {
                                GuiLogMessage("We need a number between 0 and " + blockCount.ToString() + "!", NotificationLevel.Warning);
                            }
                            OnPropertyChanged("OutputString");
                        }
                        else if (InputBlock.Length != 64)
                        {
                            GuiLogMessage("Invalid hash length, 64 characters are expected", NotificationLevel.Info);
                        }
                        else
                        {
                            OutputString = BlockChainDownloader.BlockDownloader(networkStream, InputBlock);
                            OnPropertyChanged("OutputString");
                        }
                    }
               }
                catch (Exception e)
                {
                    GuiLogMessage("Execute error: " + e.Message, NotificationLevel.Error);
                }
            }
            catch (Exception e)
            {
                GuiLogMessage("Connection error: " + e.Message, NotificationLevel.Error);
            }

            ProgressChanged(1, 1);
            client.Close();
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

#pragma warning disable 67
        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
#pragma warning restore



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

        #region Private methods

        #endregion
    }
}
