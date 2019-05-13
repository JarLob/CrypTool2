﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Cryptool.M138Analyzer
{
    /// <summary>
    /// Interaktionslogik für M138AnalyzerPresentation.xaml
    /// </summary>
    [PluginBase.Attributes.Localization("Cryptool.M138Analyzer.Properties.Resources")]
    public partial class M138AnalyzerPresentation : UserControl
    {
        public ObservableCollection<ResultEntry> BestList = new ObservableCollection<ResultEntry>();
        public event EventHandler doppelClick;

        public M138AnalyzerPresentation()
        {
            InitializeComponent();
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
                    Clipboard.SetText(entry.Text);
                }
                else if (tag == "copy_value")
                {
                    Clipboard.SetText("" + entry.Value);
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
                    foreach (var e in BestList) lines.Add(entryToText(e));
                    Clipboard.SetText(String.Join(Environment.NewLine, lines));
                }
            }
            catch (Exception ex)
            {
                Clipboard.SetText("");
            }
        }

        string entryToText(ResultEntry entry)
        {
            return "Rank: " + entry.Ranking + "\n" +
                   "Value: " + entry.Value + "\n" +
                   "Key: " + entry.Key + "\n" +
                   "Text: " + entry.Text;
        }

        public void HandleDoubleClick(Object sender, EventArgs eventArgs)
        {
            doppelClick(sender, eventArgs);
        }
    }
}
