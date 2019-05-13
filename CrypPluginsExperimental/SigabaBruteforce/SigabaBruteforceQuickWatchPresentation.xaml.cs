using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;


namespace SigabaBruteforce
{
    /// <summary>
    /// Interaction logic for SigabaBruteforceQuickWatchPresentation.xaml
    /// </summary>
    [global::Cryptool.PluginBase.Attributes.Localization("SigabaBruteforce.Properties.Resources")]
    public partial class SigabaBruteforceQuickWatchPresentation : UserControl
    {
        public ObservableCollection<ResultEntry> entries = new ObservableCollection<ResultEntry>();
        public event EventHandler doppelClick;

        

        public SigabaBruteforceQuickWatchPresentation()
        {
            InitializeComponent();
            this.DataContext = entries;
        }
        public void HandleDoubleClick(Object sender, EventArgs eventArgs)
        {
            if(sender!=null)
               doppelClick(sender,eventArgs);
        }
    }
}
