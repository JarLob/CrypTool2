using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;


namespace SigabaKnownPlaintext
{
    /// <summary>
    /// Interaction logic for StampChallenge2Presentation.xaml
    /// </summary>
   [global::Cryptool.PluginBase.Attributes.Localization("SIgabaKnownPlaintext.Properties.Resources")]
    public partial class SigabaKnownPlaintextPresentaion : UserControl
    {
        public ObservableCollection<ResultEntry> entries = new ObservableCollection<ResultEntry>();
        public event EventHandler doppelClick;

        public SigabaKnownPlaintextPresentaion()
        {
            InitializeComponent();
            this.DataContext = entries;
        }

        public void HandleDoubleClick(Object sender, EventArgs eventArgs)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback) delegate
                                                                                       {
                                                                                           doppelClick(sender,
                                                                                                       eventArgs);
                                                                                       } , null);

        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

       
    }
}
