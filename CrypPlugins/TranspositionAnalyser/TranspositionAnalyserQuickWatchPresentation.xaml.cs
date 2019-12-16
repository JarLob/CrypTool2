using System;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using Cryptool.CrypAnalysisViewControl;

namespace TranspositionAnalyser
{
    [Cryptool.PluginBase.Attributes.Localization("TranspositionAnalyser.Properties.Resources")]
    public partial class TranspositionAnalyserQuickWatchPresentation : UserControl
    {
        public ObservableCollection<ResultEntry> Entries { get; } = new ObservableCollection<ResultEntry>();
        public event Action<ResultEntry> SelectedResultEntry;

        public TranspositionAnalyserQuickWatchPresentation()
        {
            InitializeComponent();
            this.DataContext = Entries;
        }
        
        private void HandleResultItemAction(ICrypAnalysisResultListEntry item)
        {
            if (item is ResultEntry resultItem)
            {
                SelectedResultEntry(resultItem);
            }
        }
    }
}
