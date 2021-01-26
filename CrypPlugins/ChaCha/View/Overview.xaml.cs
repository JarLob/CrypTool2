using System.Windows.Controls;

namespace Cryptool.Plugins.ChaCha.View
{
    /// <summary>
    /// Interaction logic for Overview.xaml
    /// </summary>
    [PluginBase.Attributes.Localization("Cryptool.Plugins.ChaCha.Properties.Resources")]
    public partial class Overview : UserControl
    {
        public Overview()
        {
            InitializeComponent();
            ActionViewBase.LoadLocaleResources(this);
        }
    }
}