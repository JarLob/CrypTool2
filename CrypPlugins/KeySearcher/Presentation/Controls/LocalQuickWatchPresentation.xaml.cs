using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using KeySearcher;

namespace KeySearcherPresentation.Controls
{
    [Cryptool.PluginBase.Attributes.Localization("KeySearcher.Properties.Resources")]
    public partial class LocalQuickWatchPresentation
    {
        private KeySearcher.KeySearcher.UpdateOutput _updateOutputFromUserChoice;

        public KeySearcher.KeySearcher.UpdateOutput UpdateOutputFromUserChoice
        {
            get { return _updateOutputFromUserChoice; }
            set { _updateOutputFromUserChoice = value; }
        }

        public ObservableCollection<ResultEntry> entries = new ObservableCollection<ResultEntry>();

        private int amountOfDevices;
        public int AmountOfDevices
        {
            get { return amountOfDevices; }
            set
            {
                amountOfDevices = value;
                Devices.Value = amountOfDevices.ToString();
            }
        }

        public static readonly DependencyProperty IsOpenCLEnabledProperty =
            DependencyProperty.Register("IsOpenCLEnabled",
                typeof(Boolean),
                typeof(LocalQuickWatchPresentation), new PropertyMetadata(false, IsOpenCLEnabledChanged));

        public Boolean IsOpenCLEnabled
        {
            get { return (Boolean)GetValue(IsOpenCLEnabledProperty); }
            set { SetValue(IsOpenCLEnabledProperty, value); }
        }

        public LocalQuickWatchPresentation()
        {
            InitializeComponent();
            this.DataContext = entries;
            OpenCLSection.IsSectionVisible = IsOpenCLEnabled;
        }
        
        // Strings with nul characters are not displayed correctly in the clipboard
        string removeNuls(string s)
        {
            return s.Replace(Convert.ToChar(0x0).ToString(), "");
        }

        string entryToText(ResultEntry entry)
        {
            return "Rank: " + entry.Ranking + "\r\n" +
                   "Value: " + entry.Value + "\r\n" +
                   "Key: " + entry.Key + "\r\n" +
                   "Text: " + removeNuls(entry.FullText);
        }

        public void ContextMenuHandler(Object sender, EventArgs eventArgs)
        {
            try
            {
                MenuItem menu = (MenuItem)((RoutedEventArgs)eventArgs).Source;
                ResultEntry entry = (ResultEntry)menu.CommandParameter;
                if (entry == null) return;
                string tag = (string)menu.Tag;

                if (tag == "copy_text")
                {
                    Clipboard.SetText(removeNuls(entry.Text));
                }
                else if (tag == "copy_value")
                {
                    Clipboard.SetText(entry.Value);
                }
                else if (tag == "copy_key")
                {
                    Clipboard.SetText(entry.Key);
                }
                else if (tag == "copy_line")
                {
                    Clipboard.SetText(entryToText(entry));
                }
                else if (tag == "copy_all")
                {
                    List<string> lines = new List<string>();
                    foreach (var e in entries) lines.Add(entryToText(e));
                    Clipboard.SetText(String.Join("\r\n\r\n", lines));
                }
            } 
            catch(Exception ex)
            {
                Clipboard.SetText("");
            }
        }
        
        private static void IsOpenCLEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = d as LocalQuickWatchPresentation;
            self.OpenCLSection.IsSectionVisible = (bool) e.NewValue;
        }
    }
}
