using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BitcoinBlockViewer
{
    /// <summary>
    /// Logische Komponente für den BitcoinBlockViewer
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("BitcoinBlockViewer.Properties.Resources")]
    public partial class BitcoinBlockViewerPresentation : UserControl
    {

        public ObservableCollection<Transaction> entries = new ObservableCollection<Transaction>();
        public event EventHandler getTransactionHash;
        public event MouseButtonEventHandler getPrevBlockNumber;
        public event MouseButtonEventHandler getNextBlockNumber;


        public BitcoinBlockViewerPresentation()
        {
            InitializeComponent();
            this.DataContext = entries;
        }

        //helping method to get out the transaction hash
        public string TransactionToString(Transaction transaction)
        {
            return transaction.Hash;
        }

        //Get the transaction out of the list that was clicked
        public void ContextMenuHandler(Object sender, EventArgs eventArgs)
        {
            MenuItem menu = (MenuItem)((RoutedEventArgs)eventArgs).Source;
            Transaction hash = (Transaction)menu.CommandParameter;

            if (hash == null)
            {
                return;
            }
            else
            {
                List<string> lines = new List<string>();
                foreach (var e in entries) lines.Add(TransactionToString(e));
                Clipboard.SetText(String.Join("\n",lines));
            }
        }
        
        /*
         * These are the methods for the handlers
         * to pass the content to the main class
         * 
         */ 
        public void HandleDoubleClick(Object sender, EventArgs eventArgs)
        {
            getTransactionHash(sender, eventArgs);
        }

        private void PrevDoubleKlick(object sender, MouseButtonEventArgs eventArgs)
        {
            getPrevBlockNumber(sender, eventArgs);
        }


        private void NextDoubleKlick(object sender, MouseButtonEventArgs eventArgs)
        {
            getNextBlockNumber(sender, eventArgs);
        }


    }
 
}
