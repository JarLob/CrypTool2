 
using System.Windows.Controls;
using System.Windows.Input;
using CrypCloud.Manager.ViewModels;

namespace CrypCloud.Manager.Screens
{
    [Cryptool.PluginBase.Attributes.Localization("CrypCloud.Manager.Properties.Resources")]
    public partial class JobList : UserControl
    {
        public JobList()
        {
            InitializeComponent();
        }

        private void JobListView_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ((JobListVM) DataContext).DoubleClickOnEntryCommand.Execute(sender);
        }
    }
}
