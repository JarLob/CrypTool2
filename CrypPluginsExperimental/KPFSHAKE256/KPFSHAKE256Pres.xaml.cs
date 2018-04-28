using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cryptool.Plugins.KPFSHAKE256
{
    /// <summary>
    /// Interaktionslogik für KPFSHAKE256Pres.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("KPFSHAKE256.Properties.Resources")]
    public partial class KPFSHAKE256Pres : UserControl
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

        public KPFSHAKE256Pres()
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
