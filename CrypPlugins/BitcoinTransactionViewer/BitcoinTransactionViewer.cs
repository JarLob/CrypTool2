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
using System.ComponentModel;
using System.Windows.Controls;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;
using Newtonsoft.Json.Linq;
using System.Windows.Threading;
using System.Threading;
using System;

namespace BitcoinTransactionViewer
{
    [Author("Dominik Vogt", "dvogt@posteo.de", null, null)]
    [PluginInfo("BitcoinTransactionViewer.Properties.Resources", "BitcoinTransactionViewerCaption", "BitcoinTransactionViewerTooltip", "BitcoinTransactionViewer/userdoc.xml", new[] { "BitcoinTransactionViewer/Images/BC_Logo_.png" })]
    [ComponentCategory(ComponentCategory.Protocols)]
    public class BitcoinTransactionViewer : ICrypComponent
    {
        #region Private Variables

        private readonly BitcoinTransactionViewerSettings settings = new BitcoinTransactionViewerSettings();
        private BitcoinTransactionViewerPresentation myPresentation;

        double vinCoins = 0;
        double voutCoins = 0;

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public BitcoinTransactionViewer()
        {
            settings = new BitcoinTransactionViewerSettings();
            myPresentation = new BitcoinTransactionViewerPresentation();
            Presentation = myPresentation;


        }

        #region Data Properties

        /// <summary>
        /// This interface requires the transaction data
        /// </summary>
        private string _inputTransactionData;
        [PropertyInfo(Direction.InputData, "InputTransactionDataInputCaption", "InputTransactionDataInputToolTip", true)]
        public string InputTransactionData
        {
            get { return _inputTransactionData; }
            set { _inputTransactionData = value; }
        }

        /// <summary>
        /// This interface requires the transaction tx out data
        /// </summary>
        private string _txOutInput;
        [PropertyInfo(Direction.InputData, "InputTransactionTXDataInputCaption", "InputTransactionTXDataInputToolTip", true)]
        public string TxOutInput
        {
            get { return _txOutInput; }
            set { _txOutInput = value; }
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
            //parsing the transaction data
            JObject result = JObject.Parse(InputTransactionData);
            JObject transactionData = JObject.Parse(result.GetValue("result").ToString());
            JArray voutDataView = new JArray();
            JArray vinDataView = new JArray();
            
            //preparing the transaction outputs for the view
            try
            {
                JArray voutData = JArray.Parse(transactionData.GetValue("vout").ToString());
                
                foreach (JObject vout in voutData)
                {
                    JObject joe = new JObject();
                    JObject buffer = JObject.Parse(vout.GetValue("scriptPubKey").ToString());
                    joe.Add(new JProperty("address", buffer.GetValue("addresses").ToString()));
                    joe.Add(new JProperty("value", vout.GetValue("value").ToString()));
                    voutCoins += double.Parse(vout.GetValue("value").ToString());
                    voutDataView.Add(joe);
                }
            }
            catch (Exception e)
            {
                GuiLogMessage("Error while preparing the transaction outputs: " + e.Message, NotificationLevel.Error);
            }

            //preparing the transaction inputs for the view
            try
            {
                if (!TxOutInput.Equals("[]"))
                {
                    JArray vinData = JArray.Parse(TxOutInput);

                    foreach (JObject vin in vinData)
                    {
                        vinCoins += double.Parse(vin.GetValue("value").ToString());
                        vinDataView.Add(vin);
                    }
                }
                else
                {
                    JObject joe = new JObject();
                    joe.Add(new JProperty("address", "No entries"));
                    joe.Add(new JProperty("value", ""));
                    vinDataView.Add(joe);
                }

            }
            catch (Exception e)
            {
                GuiLogMessage("Error: " + e.Message, NotificationLevel.Error);
            }



            ((BitcoinTransactionViewerPresentation)Presentation).Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                //var culture = System.Threading.Thread.CurrentThread.CurrentUICulture;

                //Fill the transaction view with the main information
                myPresentation.TXID.Content = "txid: " + transactionData.GetValue("txid").ToString();
                myPresentation.Blockhash.Content = transactionData.GetValue("blockhash").ToString();
                myPresentation.Confirmations.Content = transactionData.GetValue("confirmations").ToString();
                myPresentation.Size.Content = transactionData.GetValue("size").ToString();
                double fee = Math.Round(vinCoins - voutCoins,8);
                if (fee < 0) { fee = 0; }
                myPresentation.Fee.Content = fee.ToString();
                //calculate the UTC Time from the long value
                DateTime dateTime = UnixTimeStampToDateTime(long.Parse(transactionData.GetValue("time").ToString()));
                myPresentation.Time.Content = dateTime.ToString() + " UTC";
                
                //fill the transaction view list for the inputs of the transaction
                try
                {
                    myPresentation.vins.Clear();
                    foreach (JObject joe in vinDataView)
                    {
                        Vin vin = new Vin();
                        string s = joe.GetValue("address").ToString();
                        vin.Address = ConvertHashValue(s);
                        vin.Value = joe.GetValue("value").ToString();
                        myPresentation.vins.Add(vin);
                    }
                }
                catch (Exception e)
                {
                    GuiLogMessage("Error while displaying the transaction inputs: " +e.Message, NotificationLevel.Error);
                }

                //fill the transaction view list for the outputs of the transaction
                try
                {
                    myPresentation.vouts.Clear();
                    foreach (JObject joe in voutDataView)
                    {
                        Vout vout = new Vout();
                        string s = joe.GetValue("address").ToString();
                        vout.Address = ConvertHashValue(s);
                        vout.Value = joe.GetValue("value").ToString();
                        myPresentation.vouts.Add(vout);
                    }
                }
                catch (Exception e)
                {
                    GuiLogMessage("Error while displaying the transaction outputs: " + e.Message, NotificationLevel.Error);
                }

                vinCoins = 0;
                voutCoins = 0;
            }
            , null);

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
            ((BitcoinTransactionViewerPresentation)Presentation).Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                myPresentation.TXID.Content = "txid: ";
                myPresentation.Blockhash.Content = "";
                myPresentation.Confirmations.Content = "";
                myPresentation.Size.Content = "";
                myPresentation.Fee.Content = "";
                myPresentation.Time.Content = "";
                myPresentation.vins.Clear();
                myPresentation.vouts.Clear();
            }
            , null);
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
        //helping method to convert string values
        public string ConvertHashValue(string hash)
        {
            try
            {
                int beginning = hash.IndexOf('"');
                int ending = hash.LastIndexOf('"');
                return hash.Substring(beginning + 1, ending - beginning - 1);
            }catch(Exception e)
            {
                return hash;
            }
        }

        //helping method to transform the long value to an DateTime value
        private static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);
            return dtDateTime;
        }
    }



    public class Vin
    {
        public string Address { get; set; }
        public string Value { get; set; }
    }

    public class Vout
    {
        public string Address { get; set; }
        public string Value { get; set; }
    }

}
