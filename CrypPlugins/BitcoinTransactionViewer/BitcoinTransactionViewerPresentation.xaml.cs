using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;


namespace BitcoinTransactionViewer
{
    /// <summary>
    /// logical component for the BitcoinTransactionViewer
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("BitcoinTransactionViewer.Properties.Resources")]
    public partial class BitcoinTransactionViewerPresentation : UserControl
    {

        public ObservableCollection<Vout> vouts = new ObservableCollection<Vout>();
        public ObservableCollection<Vin> vins = new ObservableCollection<Vin>();

        //The data content must be defined in order to be able to access the two tables
        public BitcoinTransactionViewerPresentation()
        {
            InitializeComponent();
            this.VOUT.DataContext = vouts;
            this.VIN.DataContext = vins;
        }

        /*
         * The two methods convert an Object Vin or Vout into a string
         */
        string entryToText(Vout vout)
        {
            return "Address: " + vout.Address + "\n" +
                   "Value: " + vout.Value + "\n";

        }

        string entryToText(Vin vin)
        {
            return "Address: " + vin.Address + "\n" +
                   "Value: " + vin.Value + "\n";

        }



        //Enter the Transaction outputs
        public void ContextMenuHandler(Object sender, EventArgs eventArgs)
        {
            MenuItem menu = (MenuItem)((RoutedEventArgs)eventArgs).Source;
            Vout entry = (Vout)menu.CommandParameter;

            if ((string)(menu.Tag) == "copy_address")
            {
                Clipboard.SetText(entry.Address);
            }
            else if ((string)(menu.Tag) == "copy_value")
            {
                Clipboard.SetText(entry.Value);
            }
            else if ((string)(menu.Tag) == "copy_all")
            {
                List<string> lines = new List<string>();
                foreach (Vout e in vouts) lines.Add(entryToText(e));
                Clipboard.SetText(String.Join("\n\n", lines));
            }
        }

        //Enter the Transaction inputs
        public void VinContextMenuHandler(Object sender, EventArgs eventArgs)
        {
            MenuItem menu = (MenuItem)((RoutedEventArgs)eventArgs).Source;
            Vin entry = (Vin)menu.CommandParameter;

            if ((string)(menu.Tag) == "copy_address")
            {
                Clipboard.SetText(entry.Address);
            }
            else if ((string)(menu.Tag) == "copy_value")
            {
                Clipboard.SetText(entry.Value);
            }
            else if ((string)(menu.Tag) == "copy_all")
            {
                List<string> lines = new List<string>();
                foreach (Vin e in vins) lines.Add(entryToText(e));
                Clipboard.SetText(String.Join("\n\n", lines));
            }
        }


    }
 
}
