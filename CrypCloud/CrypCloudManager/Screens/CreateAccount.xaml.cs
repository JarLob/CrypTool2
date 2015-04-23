using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CrypCloud.Manager.ViewModels;

namespace CrypCloud.Manager.Screens
{

    [Cryptool.PluginBase.Attributes.Localization("CrypCloud.Manager.Properties.Resources")]
    public partial class CreateAccount : UserControl
    {
        public CreateAccount()
        {
            InitializeComponent();
        }

        //prevents memory leaking of the secure password input
        private void PasswordBoxChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            {
                ((CreateAccountVM)DataContext).Password = ((PasswordBox)sender).SecurePassword;
            }
        }

        //prevents memory leaking of the secure password input
        private void PasswordConfirmBoxChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            {
                ((CreateAccountVM)DataContext).PasswordConfirm = ((PasswordBox)sender).SecurePassword;
            }
        }
    }
}
