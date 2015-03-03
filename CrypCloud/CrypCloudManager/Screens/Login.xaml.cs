
using System.Windows;
using System.Windows.Controls;
using CrypCloud.Manager.ViewModels;
using Cryptool.PluginBase.Attributes;

namespace CrypCloud.Manager.Screens
{
    [Localization("CrypCloud.Manager.Properties.Resources")]
    public partial class Login : UserControl
    { 
        public Login()
        {
            InitializeComponent();
        }

        //prevents memory leaking of the sequre password imput
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            {
                ((LoginVM)DataContext).Password = ((PasswordBox)sender).SecurePassword;
            }
        }
    }
}
