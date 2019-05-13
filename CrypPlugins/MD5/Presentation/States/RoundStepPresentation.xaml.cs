using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Cryptool.MD5.Presentation.States
{
    /// <summary>
    /// Interaktionslogik für StartingRoundStepPresentation.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("Cryptool.Plugins.MD5.Properties.Resources")]
    public partial class RoundStepPresentation : UserControl
    {
        public RoundStepPresentation()
        {
            InitializeComponent();
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
                ((Storyboard)FindResource("LineFadeStoryboard")).Begin();
        }
    }
}
