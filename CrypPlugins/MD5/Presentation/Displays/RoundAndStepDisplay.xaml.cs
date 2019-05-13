using System.Windows.Controls;

namespace Cryptool.MD5.Presentation.Displays
{
    /// <summary>
    /// Interaktionslogik für RoundAndStepDisplay.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("Cryptool.Plugins.MD5.Properties.Resources")]
    public partial class RoundAndStepDisplay : UserControl
    {
        public RoundAndStepDisplay()
        {
            InitializeComponent();

            Width = double.NaN;
            Height = double.NaN;
        }
    }
}
