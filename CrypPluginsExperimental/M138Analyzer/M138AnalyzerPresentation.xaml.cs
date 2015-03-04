using Cryptool.M138Analyzer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cryptool.M138Analyzer
{
    /// <summary>
    /// Interaktionslogik für M138AnalyzerPresentation.xaml
    /// </summary>
    [PluginBase.Attributes.Localization("Cryptool.M138Analyzer.Properties.Resources")]
    public partial class M138AnalyzerPresentation : UserControl
    {
        public ObservableCollection<ResultEntry> BestList = new ObservableCollection<ResultEntry>();

        public M138AnalyzerPresentation()
        {
            InitializeComponent();
        }


        public void ContextMenuHandler(Object sender, EventArgs eventArgs)
        {
            /*
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
            } */
        }
    }
}
