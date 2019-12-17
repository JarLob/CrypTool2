﻿using System;
using System.Windows;
using System.Windows.Controls;
using KeySearcher;
using System.Globalization;
using System.Threading.Tasks;
using KeySearcher.CrypCloud;
using Cryptool.CrypAnalysisViewControl;

namespace KeySearcherPresentation.Controls
{
    [Cryptool.PluginBase.Attributes.Localization("KeySearcher.Properties.Resources")]
    public partial class P2PQuickWatchPresentation : UserControl
    {
        public KeySearcher.KeySearcher.UpdateOutput UpdateOutputFromUserChoice { get; set; }

        public static readonly DependencyProperty IsVerboseEnabledProperty = DependencyProperty.Register("IsVerboseEnabled", typeof(Boolean), typeof(P2PQuickWatchPresentation), new PropertyMetadata(false));
        public Boolean IsVerboseEnabled
        {
            get { return (Boolean)GetValue(IsVerboseEnabledProperty); }
            set { SetValue(IsVerboseEnabledProperty, value); }
        }
        
        public NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();

        public TaskFactory UiContext { get; set; }
        public P2PPresentationVM ViewModel { get; set; }
     
        public P2PQuickWatchPresentation() 
        {
            InitializeComponent();
            ViewModel = DataContext as P2PPresentationVM;
            UiContext = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext());
            ViewModel.UiContext = UiContext;
        }

        public void UpdateSettings(KeySearcher.KeySearcher keySearcher, KeySearcherSettings keySearcherSettings)
        {
            ViewModel.UpdateSettings(keySearcher, keySearcherSettings);
        }

        private void HandleResultItemAction(ICrypAnalysisResultListEntry item)
        {
            if (item is KeyResultEntry resultItem)
            {
                UpdateOutputFromUserChoice(resultItem.Key, resultItem.Plaintext);
            }
        }
    }
}
