using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Cryptool.Plugins.KPFSHA256
{
    /// <summary>
    /// Interaktionslogik für KPFSHA256Pres.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("KPFSHA256.Properties.Resources")]
    public partial class KPFSHA256Pres : UserControl
    {

        public AutoResetEvent buttonNextClickedEvent;
        public AutoResetEvent buttonStartClickedEvent;
        public AutoResetEvent buttonRestartClickedEvent;
        private bool _skipChapter;
        private bool _restart;

        public bool Restart
        {
            get
            {
                return _restart;
            }
            set
            {
                _restart = value;
            }
        }

        public bool SkipChapter
        {
            get
            {
                return _skipChapter;
            }
            set
            {
                _skipChapter = value;
            }
        }

        public KPFSHA256Pres()
        {
            InitializeComponent();
            buttonNextClickedEvent = new AutoResetEvent(false);
            buttonStartClickedEvent = new AutoResetEvent(false);
            buttonRestartClickedEvent = new AutoResetEvent(false);
            _skipChapter = false;

        }

        private void buttonRestart_Click(object sender, RoutedEventArgs e)
        {
            buttonRestartClickedEvent.Set();
            _restart = true;
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            buttonStartClickedEvent.Set();
            _restart = false;
        }

        private void buttonSkip_Click(object sender, RoutedEventArgs e)
        {
            buttonNextClickedEvent.Set();
            _skipChapter = true;
        }

       private void buttonNext_Click(object sender, RoutedEventArgs e)
       {
            buttonNextClickedEvent.Set();
       }
    }
}
