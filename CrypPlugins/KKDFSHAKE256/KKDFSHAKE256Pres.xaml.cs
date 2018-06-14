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

namespace Cryptool.Plugins.KKDFSHAKE256
{
    /// <summary>
    /// Interaktionslogik für KKDFSHAKE256Pres.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("KKDFSHAKE256.Properties.Resources")]
    public partial class KKDFSHAKE256Pres : UserControl
    {
        public AutoResetEvent buttonNextClickedEvent;
        public AutoResetEvent buttonPrevClickedEvent;
        public AutoResetEvent buttonStartClickedEvent;
        public AutoResetEvent buttonRestartClickedEvent;
        private bool _skipChapter;
        private bool _restart;
        private bool _next;
        private bool _prev;
        

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

        public bool Next
        {
            get
            {
                return _next;
            }
            set
            {
                _next = value;
            }
        }

        public bool Prev
        {
            get
            {
                return _prev;
            }
            set
            {
                _prev = value;
            }
        }

        public KKDFSHAKE256Pres()
        {
            InitializeComponent();
            buttonNextClickedEvent = new AutoResetEvent(false);
            buttonPrevClickedEvent = new AutoResetEvent(false);
            buttonStartClickedEvent = new AutoResetEvent(false);
            buttonRestartClickedEvent = new AutoResetEvent(false);
            _skipChapter = false;
            _next = false;
            _prev = false;
  
    }

        private void buttonRestart_Click(object sender, RoutedEventArgs e)
        {
            _restart = true;
            buttonRestartClickedEvent.Set();
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            _restart = false;
            buttonStartClickedEvent.Set();
        }

        private void buttonPrev_Click(object sender, RoutedEventArgs e)
        {
            _prev = true;
            buttonPrevClickedEvent.Set();

            Console.WriteLine("ButtonPrev fired");
        }

        private void buttonNext_Click(object sender, RoutedEventArgs e)
        {
            _next = true;
            buttonNextClickedEvent.Set();
            

            Console.WriteLine("ButtonNext fired");
        }

        private void buttonSkip_Click(object sender, RoutedEventArgs e)
        {
            _skipChapter = true;
            _next = true;
            buttonNextClickedEvent.Set();

            Console.WriteLine("ButtonSkip fired");
        }

    }
}
