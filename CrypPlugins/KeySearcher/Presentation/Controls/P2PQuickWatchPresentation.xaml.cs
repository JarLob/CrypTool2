using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using KeySearcher;
using KeySearcher.KeyPattern;
using System.Globalization;
using System.Threading.Tasks;
using CrypCloud.Core;
using KeySearcher.CrypCloud;
using voluntLib.common;

namespace KeySearcherPresentation.Controls
{
    [Cryptool.PluginBase.Attributes.Localization("KeySearcher.Properties.Resources")]
    public partial class P2PQuickWatchPresentation : UserControl
    {
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
        }


        public void UpdateSettings(KeySearcher.KeySearcher keySearcher, KeySearcherSettings keySearcherSettings)
        {

            IsVerboseEnabled = false;
            if (CannotUpdateView(keySearcher, keySearcherSettings))
            {
                return;
            }

            var keyPattern = new KeyPattern(keySearcher.ControlMaster.GetKeyPattern()) {WildcardKey = keySearcherSettings.Key};
            var keysPerChunk = keyPattern.size() / BigInteger.Pow(2, keySearcherSettings.NumberOfBlocks);
            if (keysPerChunk < 1)
            {
                keySearcherSettings.NumberOfBlocks = (int) BigInteger.Log(keyPattern.size(), 2);
            }

            var keyPatternPool = new KeyPatternPool(keyPattern, keysPerChunk);
            ViewModel.TotalAmountOfChunks = keyPatternPool.Length;
            ViewModel.KeysPerBlock = keysPerChunk;
            ViewModel.JobID = keySearcher.JobID;

            if (CrypCloudCore.Instance.IsRunning)
            {
                var networkJobs = CrypCloudCore.Instance.GetJobs();
                var networkJob = networkJobs.Find(it => it.JobID == keySearcher.JobID);
                if (networkJob != null)
                {
                    ViewModel.JobName = networkJob.JobName;   
                }
            }
        }

        private static bool CannotUpdateView(KeySearcher.KeySearcher keySearcher, KeySearcherSettings keySearcherSettings)
        {
            return keySearcher.Pattern == null || !keySearcher.Pattern.testWildcardKey(keySearcherSettings.Key) || keySearcherSettings.NumberOfBlocks == 0;
        }

        private void P2PQuickWatch_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
