﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ADFGVXAnalyzer
{
    /// <summary>
    /// Interaktionslogik für ADFGVXAnalyzerPresentation.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("ADFGVXAnalyzer.Properties.Resources")]
    public partial class ADFGVXAnalyzerPresentation : UserControl
    {

        public ObservableCollection<ResultEntry> BestList = new ObservableCollection<ResultEntry>();
        public event EventHandler getTranspositionResult;

        public ADFGVXAnalyzerPresentation()
        {
            InitializeComponent();
            DataContext = BestList;
        }

        public void DisableGUI()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                ListView.IsEnabled = false;
            }, null);
        }

        public void EnableGUI()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                ListView.IsEnabled = true;
            }, null);
        }

        // Strings with nul characters are not displayed correctly in the clipboard
        string removeNuls(string s)
        {
            return s.Replace(Convert.ToChar(0x0).ToString(), "");
        }

        string entryToText(ResultEntry entry)
        {
            return "Ranking: " + entry.Ranking + Environment.NewLine +
                   "Score: " + entry.Score + Environment.NewLine + 
                   "Ic1: " + entry.Ic1 + Environment.NewLine +
                   "Ic2: " + entry.Ic2 + Environment.NewLine +
                   "TransKey: " + entry.TransKey + Environment.NewLine +
                   "Plaintext: " + removeNuls(entry.TranspositionResult);
        }

        public void ContextMenuHandler(Object sender, EventArgs eventArgs)
        {
            try
            {
                MenuItem menu = (MenuItem)((RoutedEventArgs)eventArgs).Source;
                ResultEntry entry = (ResultEntry)menu.CommandParameter;
                if (entry == null) return;
                string tag = (string)menu.Tag;

                if (tag == "copy_plaintext")
                {
                    Clipboard.SetText(removeNuls(entry.TranspositionResult));
                }
                else if (tag == "copy_score")
                {
                    Clipboard.SetText("" + entry.Score);
                }
                else if (tag == "copy_ic1")
                {
                    Clipboard.SetText("" + entry.Ic1);
                }
                else if (tag == "copy_ic2")
                {
                    Clipboard.SetText("" + entry.Ic2);
                }
                else if (tag == "copy_transkey")
                {
                    Clipboard.SetText(entry.TransKey);
                }
                else if (tag == "copy_line")
                {
                    Clipboard.SetText(entryToText(entry));
                }
                else if (tag == "copy_all")
                {
                    List<string> lines = new List<string>();
                    foreach (var e in BestList) lines.Add(entryToText(e));
                    Clipboard.SetText(String.Join(Environment.NewLine, lines));
                }
            }
            catch (Exception ex)
            {
                Clipboard.SetText("");
            }
        }

        public void HandleDoubleClick(Object sender, EventArgs eventArgs)
        {
            getTranspositionResult(sender, eventArgs);
        }
    }
}
