using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using KeySearcher;
using KeySearcher.KeyPattern;
using System.Globalization;
using System.Threading.Tasks;
using CrypCloud.Core;
using KeySearcher.CrypCloud;
using System.Text;

namespace KeySearcherPresentation.Controls
{
    [Cryptool.PluginBase.Attributes.Localization("KeySearcher.Properties.Resources")]
    public partial class P2PQuickWatchPresentation : UserControl
    {
        private KeySearcher.KeySearcher.UpdateOutput _updateOutputFromUserChoice;      

        public KeySearcher.KeySearcher.UpdateOutput UpdateOutputFromUserChoice
        {
            get { return _updateOutputFromUserChoice; }
            set { _updateOutputFromUserChoice = value; }
        }

        public static readonly DependencyProperty IsVerboseEnabledProperty = DependencyProperty.Register("IsVerboseEnabled", typeof(Boolean), typeof(P2PQuickWatchPresentation), new PropertyMetadata(false));
        public Boolean IsVerboseEnabled
        {
            get { return (Boolean)GetValue(IsVerboseEnabledProperty); }
            set { SetValue(IsVerboseEnabledProperty, value); }
        }
        
        public NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();

        public TaskFactory UiContext { get; set; }
        public P2PPresentationVM ViewModel { get; set; }
     
        public P2PQuickWatchPresentation() 
        {
            InitializeComponent();
            ViewModel = DataContext as P2PPresentationVM;
            UiContext = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext());
            ViewModel.UiContext = UiContext;
        }

        public void UpdateSettings(KeySearcher.KeySearcher keySearcher, KeySearcherSettings keySearcherSettings)
        {
            ViewModel.UpdateSettings(keySearcher, keySearcherSettings);
        }

        // Strings with nul characters are not displayed correctly in the clipboard
        string removeNuls(string s)
        {
            return s.Replace(Convert.ToChar(0x0).ToString(), "");
        }

        string entryToText(KeyResultEntry entry)
        {
            string key = ByteArrayToString(entry.KeyBytes);
            string plaintext = Encoding.GetEncoding(1252).GetString(entry.Decryption);

            return "Value: " + entry.Costs.ToString() + "\r\n" +
                   "Key: " + key + "\r\n" +
                   "Text: " + removeNuls(plaintext);
        }

        public void ContextMenuHandler(Object sender, EventArgs eventArgs)
        {
            try
            {
                MenuItem menu = (MenuItem)((RoutedEventArgs)eventArgs).Source;
                KeyResultEntry entry = (KeyResultEntry)menu.CommandParameter;
                if (entry == null) return;
                string tag = (string)menu.Tag;

                string key = ByteArrayToString(entry.KeyBytes);
                string plaintext = Encoding.UTF8.GetString(entry.Decryption);

                if (tag == "copy_text")
                {
                    Clipboard.SetText(removeNuls(plaintext));
                }
                else if (tag == "copy_value")
                {
                    Clipboard.SetText(entry.Costs.ToString());
                }
                else if (tag == "copy_key")
                {
                    Clipboard.SetText(key);
                }
                else if (tag == "copy_line")
                {
                    Clipboard.SetText(entryToText(entry));
                }              
            }
            catch (Exception ex)
            {
                Clipboard.SetText("");
            }
        }

        public void HandleDoubleClick(Object sender, EventArgs eventArgs)
        {
            var lvi = sender as ListViewItem;
            var r = lvi.Content as KeyResultEntry;

            if (r != null)
            {
                string key = ByteArrayToString(r.KeyBytes);
                string plaintext = Encoding.GetEncoding(1252).GetString(r.Decryption);
                _updateOutputFromUserChoice(key, plaintext);
            }
        }

        public string ByteArrayToString(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "");
        }
    }
}
