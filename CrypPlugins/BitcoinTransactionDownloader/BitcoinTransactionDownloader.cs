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
using System.Net.Sockets;
using System;
using BitcoinBlockChainAnalyser;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cryptool.Plugins.BitcoinTransactionDownloader
{
    [Author("Dominik Vogt", "dvogt@posteo.de", null, null)]
    [PluginInfo("BitcoinTransactionDownloader.Properties.Resources","PluginCaption", "PluginToolTip", "BitcoinTransactionDownloader/userdoc.xml", new[] { "BitcoinTransactionDownloader/Images/BC_Logo_.png" })]
    [ComponentCategory(ComponentCategory.Protocols)]
    public class BitcoinTransactionDownloader : ICrypComponent
    {
        #region Private Variables

        private readonly BitcoinTransactionDownloaderSettings settings;

        //Private variables for the server connection
        private TcpClient client = null;
        String hostname = null;
        int port = 0;
        NetworkStream networkStream = null;

        #endregion

        #region Data Properties

        /// <summary>
        /// Constructor
        /// </summary>
        public BitcoinTransactionDownloader()
        {
            this.settings = new BitcoinTransactionDownloaderSettings();
        }

        /// <summary>
        /// Get or set all settings for this algorithm.
        /// </summary>
        public ISettings Settings
        {
            get { return this.settings; }
        }

        private string _inputString;

        /// <summary>
        /// The Downloaeder needs an input value like a number between zero and the highest number in blockchain
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputStringCaption", "InputStringTooltip", true)]
        public string InputString
        {
            get { return _inputString; }
            set { _inputString = value; }
        }

        /// <summary>
        /// The Downloader returns the transaction information from the response transaction hash
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", true)]
        public string OutputString
        {
            get;
            set;
        }

        /// <summary>
        /// The Downloader returns the transaction output information from the response transaction hash
        /// </summary>
        [PropertyInfo(Direction.OutputData, "OutputString2Caption", "OutputString2Tooltip", true)]
        public string OutputString2
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
                networkStream = client.GetStream();
            }
            catch (Exception e)
            {
                GuiLogMessage("Connection error: " + e.Message, NotificationLevel.Error);
            }

            ProgressChanged(0, 1);
            try
            {
                //do nothing if the transaction hash null or empty
                if(InputString != null & !InputString.Equals(""))
                {
                    //Download the Transaction information
                    String response = BlockChainDownloader.TransactionDownloader(networkStream, InputString);
                    try
                    {
                        //parsing the transaction data
                        JObject joe = JObject.Parse(response);
                        JObject transactionData = JObject.Parse(joe.GetValue("result").ToString());
                        //save the transaction input data in separately JArray
                        JArray vinData = JArray.Parse(transactionData.GetValue("vin").ToString());
                        JArray vinOutput = new JArray();

                        /*
                         * The transaction data does not contain relevant information about the transaction 
                         * receipts. These must be downloaded separately.
                         */
                        foreach (JObject vin in vinData)
                        {
                            String s = BlockChainDownloader.TxoutDownloader(networkStream, vin.GetValue("txid").ToString(), (int)vin.GetValue("vout"));
                            JObject buffer = JObject.Parse(s);
                            JObject jObject = new JObject();

                            JObject scriptPubKey = JObject.Parse(buffer.GetValue("scriptPubKey").ToString());

                            jObject.Add(new JProperty("address", scriptPubKey.GetValue("addresses").ToString()));
                            jObject.Add(new JProperty("value", buffer.GetValue("value").ToString()));
                            vinOutput.Add(jObject);
                        }
                        OutputString2 = vinOutput.ToString();

                    }
                    //if the input list empty
                    catch (Exception e)
                    {
                        OutputString2 = "";
                        GuiLogMessage("No entries in transaction: "+e.Message, NotificationLevel.Warning);
                    }
                    OutputString = response;
                }
            }
            catch (Exception e)
            {
                GuiLogMessage(e.Message, NotificationLevel.Error);
            }
            OnPropertyChanged("OutputString");
            OnPropertyChanged("OutputString2");

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

        #region private methods



        #endregion
    }
}
