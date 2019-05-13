using System.Windows;
using System.Windows.Controls;
using Cryptool.PluginBase.Attributes;

namespace Wizard
{
    /// <summary>
    /// Interaction logic for WizardSettingsTab.xaml
    /// </summary>
    [Localization("Wizard.Resources.settingsRes")]
    [SettingsTab("WizardSettings", "/MainSettings/")]
    public partial class WizardSettingsTab : UserControl
    {
        public WizardSettingsTab(Style settingsStyle)
        {
            Resources.Add("settingsStyle", settingsStyle);
            InitializeComponent();

            Cryptool.PluginBase.Properties.Settings.Default.PropertyChanged += delegate { Cryptool.PluginBase.Miscellaneous.ApplicationSettingsHelper.SaveApplicationsSettings(); };
        }
    }
}
