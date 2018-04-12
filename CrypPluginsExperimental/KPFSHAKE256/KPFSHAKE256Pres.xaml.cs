﻿using System;
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
        private bool _skipIntro;

        public bool SkipIntro
        {
            get
            {
                return _skipIntro;
            }
            set
            {
                _skipIntro = value;
            }
        }

        public KPFSHAKE256Pres()
        {
            InitializeComponent();
            buttonNextClickedEvent = new AutoResetEvent(false);
            _skipIntro = false;
        }
        private void buttonSkip_Click(object sender, RoutedEventArgs e)
        {
            buttonNextClickedEvent.Set();
            _skipIntro = true;
        }

        private void buttonNext_Click(object sender, RoutedEventArgs e)
        {
            buttonNextClickedEvent.Set();
        }
    }
}
