using System;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using KeySearcher;
using KeySearcher.KeyPattern;
using System.Globalization;

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

        public P2PQuickWatchPresentation()
        {
            InitializeComponent();
        }
        
        public void UpdateSettings(KeySearcher.KeySearcher keySearcher, KeySearcherSettings keySearcherSettings)
        {
            IsVerboseEnabled = false;

            if (keySearcher.Pattern == null || !keySearcher.Pattern.testWildcardKey(keySearcherSettings.Key) || keySearcherSettings.NumberOfBlocks == 0)
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
            if (keyPatternPool.Length > 9999999999)
            {
                TotalAmountOfChunks.Content = keyPatternPool.Length.ToString().Substring(0, 10) + "...";
            }
            else
            {
                TotalAmountOfChunks.Content = keyPatternPool.Length;
            }
            KeysPerChunk.Content = keysPerChunk;

            TestedBits.Content = Math.Ceiling(Math.Log((double) BigInteger.Multiply(keyPatternPool.Length, keysPerChunk), 2));
        }

        
    }
}
