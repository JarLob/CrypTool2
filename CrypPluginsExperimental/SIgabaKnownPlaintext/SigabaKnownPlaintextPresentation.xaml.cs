using Cryptool.CrypAnalysisViewControl;
using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;


namespace SigabaKnownPlaintext
{
   [global::Cryptool.PluginBase.Attributes.Localization("SIgabaKnownPlaintext.Properties.Resources")]
    public partial class SigabaKnownPlaintextPresentaion : UserControl
    {
        public ObservableCollection<ResultEntry> Entries { get; } = new ObservableCollection<ResultEntry>();
        public event Action<ResultEntry> SelectedResultEntry;

        public SigabaKnownPlaintextPresentaion()
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
