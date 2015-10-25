
using System.IO;
using System.IO.IsolatedStorage;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using CrypCloud.Manager.ViewModels;
using CrypCloud.Manager.ViewModels.Helper;

namespace CrypCloud.Manager.Screens
{

    [Cryptool.PluginBase.Attributes.Localization("CrypCloud.Manager.Properties.Resources")]
    public partial class Login : UserControl

    {  
        public Login()
        {
            InitializeComponent();

            var rememberedUsername = Settings.Default.rememberedUsername;
            if (!string.IsNullOrEmpty(rememberedUsername))
            {
                SecredPasswordInput.Password = "**********";
            }
        }

        //prevents memory leaking of the sequre password imput
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null && !SecredPasswordInput.Password.Equals("**********"))
            {
                ((LoginVM)DataContext).Password = ((PasswordBox)sender).SecurePassword;
            }
        }
    }
}
