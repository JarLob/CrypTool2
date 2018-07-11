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
using Newtonsoft.Json.Linq;
using System.Windows.Threading;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Windows.Input;

namespace BitcoinBlockViewer
{
    [Author("Dominik Vogt", "dvogt@posteo.de", null, null)]
    [PluginInfo("BitcoinBlockViewer.Properties.Resources", "BitcoinBlockViewerCaption", "BitcoinBlockViewerTooltip", "BitcoinBlockViewer/userdoc.xml", new[] { "BitcoinBlockViewer/Images/BC_Logo_.png" })]
    [ComponentCategory(ComponentCategory.Protocols)]
    public class BitcoinBlockViewer : ICrypComponent
    {
        #region Private Variables

        private readonly BitcoinBlockViewerSettings settings = new BitcoinBlockViewerSettings();
        private BitcoinBlockViewerPresentation myPresentation;
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public BitcoinBlockViewer()
        {
            settings = new BitcoinBlockViewerSettings();
            myPresentation = new BitcoinBlockViewerPresentation();
            Presentation = myPresentation;
            myPresentation.getTransactionHash += new EventHandler(this.getTransactionHash);
            myPresentation.getPrevBlockNumber += new MouseButtonEventHandler(this.getPrevBlockNumber);
            myPresentation.getNextBlockNumber += new MouseButtonEventHandler(this.getNextBlockNumber);

        }

        #region Data Properties

        /// <summary>
        /// //The BitcoinBlockViewer needs an Input String with the Blockdata
        /// </summary>
        [PropertyInfo(Direction.InputData, "InputBlockDataInputCaption", "InputBlockDataInputToolTip", true)]
        public string BlockData
        {
            get;
            set;
        }

        /// <summary>
        /// //It is possible to output a Transaktionshash
        /// </summary>
        private string output;
        [PropertyInfo(Direction.OutputData, "OutputTransactionHashCaption", "OutputTransactionHashTooltip", false)]
        public string OutputTransactionHash
        {
            get
            {
                return this.output;
            }
            set
            {
                this.output = value;
                OnPropertyChanged("OutputTransactionHash");
            }
        }

        /// <summary>
        /// //It is possible to output a Blockhash
        /// </summary>
        private string outputBlockHash;
        [PropertyInfo(Direction.OutputData, "OutputBlockHashCaption", "OutputBlockHashTooltip", false)]
        public string OutputBlockHash
        {
            get
            {
                return this.outputBlockHash;
            }
            set
            {
                this.outputBlockHash = value;
                OnPropertyChanged("OutputBlockHash");
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
            get;
            private set;
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
            //parsing the input blockdata
            JObject result = JObject.Parse(BlockData);
            JObject blockData = JObject.Parse(result.GetValue("result").ToString());
            //save the transactionhashes in a separately list
            string transactions = blockData.GetValue("tx").ToString();
            List<string> transactionList = transactions.Split(',').ToList();

            //BitcoinBlockViewer Presentation
            ((BitcoinBlockViewerPresentation)Presentation).Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                var culture = System.Threading.Thread.CurrentThread.CurrentUICulture;


                myPresentation.Height.Content = "height: " + blockData.GetValue("height").ToString();
                myPresentation.Blockhash.Content = blockData.GetValue("hash").ToString();

                if (blockData.GetValue("previousblockhash") == null)
                {
                    myPresentation.Previousblockhash.Content = "";
                }
                else
                {
                    myPresentation.Previousblockhash.Content = blockData.GetValue("previousblockhash").ToString();
                }


                if (blockData.GetValue("nextblockhash") == null)
                {
                    myPresentation.Nextblockhash.Content = "";
                }
                else
                {
                    myPresentation.Nextblockhash.Content = blockData.GetValue("nextblockhash").ToString();
                }

                myPresentation.Merkleroot.Content = blockData.GetValue("merkleroot").ToString();
                myPresentation.Nonce.Content = blockData.GetValue("nonce").ToString();
                myPresentation.Weight.Content = blockData.GetValue("weight").ToString();
                myPresentation.Confirmations.Content = blockData.GetValue("confirmations").ToString();
                myPresentation.Strippedsize.Content = blockData.GetValue("strippedsize").ToString();
                myPresentation.Size.Content = blockData.GetValue("size").ToString();
                myPresentation.Difficulty.Content = blockData.GetValue("difficulty").ToString();

                myPresentation.entries.Clear();
                //add all transtractionhashes into the view
                foreach (string t in transactionList)
                {
                    Transaction transaction = new Transaction();
                    transaction.Hash = ConvertHashValue(t);
                    myPresentation.entries.Add(transaction);
                }

                

            }
            , null);


            ProgressChanged(1, 1);
        }

        //Method to send a transactionhash by doubleclick
        private void getTransactionHash(object sender, EventArgs e)
        {
            try
            {
                ListViewItem lvi = sender as ListViewItem;
                Transaction transaction = lvi.Content as Transaction;
                OutputTransactionHash = transaction.Hash;
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
            }
        }

        //Method to send the previous Blockhash 
        private void getPrevBlockNumber(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Button button = sender as Button;
                OutputBlockHash = button.Content.ToString();
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message,NotificationLevel.Error);
            }
        }
        //Method to send the next Blockhash 
        private void getNextBlockNumber(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Button button = sender as Button;
                OutputBlockHash = button.Content.ToString();
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
            }
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

        //convert Hash values 
        public string ConvertHashValue(string hash)
        {
            int beginning = hash.IndexOf('"');
            int ending = hash.LastIndexOf('"');
            return hash.Substring(beginning+1, ending - beginning-1);
        }
    }


    //Needed for the list of transaction hashes
    public class Transaction {
        public string Hash { get; set; }
    }

}
