using System.Windows;
using System.Windows.Controls;
using Cryptool.MD5.Algorithm;

namespace Cryptool.MD5.Presentation
{
    /// <summary>
    /// Interaktionslogik für PresentationContainer.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("Cryptool.Plugins.MD5.Properties.Resources")]
    public partial class PresentationContainer : UserControl
    {
        private PresentableMD5 md5;

        public PresentationContainer(PresentableMD5 presentableMd5)
        {
            DataContext = md5 = presentableMd5;

            InitializeComponent();

            Width = double.NaN;
            Height = double.NaN;
        }

        private void nextStepButton_Click(object sender, RoutedEventArgs e)
        {
            md5.NextStep();
        }

        private void previousStepButton_Click(object sender, RoutedEventArgs e)
        {
            md5.PreviousStep();
        }

        private void endOfRoundButton_Click(object sender, RoutedEventArgs e)
        {
            md5.NextStepUntilRoundEnd();
        }

        private void endOfCompressionButton_Click(object sender, RoutedEventArgs e)
        {
            md5.NextStepUntilBlockEnd();
        }
    }
}
