using Cryptool.Plugins.ChaCha.ViewModel;
using System.Windows.Controls;

namespace Cryptool.Plugins.ChaCha.View
{
    /// <summary>
    /// Interaction logic for ChaChaPresentation.xaml
    /// </summary>
    [PluginBase.Attributes.Localization("Cryptool.Plugins.ChaCha.Properties.Resources")]
    public partial class ChaChaPresentation : UserControl
    {
        public ChaChaPresentation(ChaCha chachaVisualization)
        {
            InitializeComponent();
            this.DataContext = new ChaChaPresentationViewModel(chachaVisualization);
        }
    }
}