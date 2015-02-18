using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using CrypCloud.Core;
using CrypCloud.Manager.Controller;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Attributes;

namespace CrypCloud.Manager.Screens
{
    [Localization("CrypCloud.Manager.Properties.Resources")]
    public partial class Login : UserControl
    { 
        public LoginController Controller { get; set; }

        public Login()
        {
            InitializeComponent();
        }
     
        public void SetSuggestetUsernames(List<string> namesOfKnownCertificats)
        {
            foreach (var username in namesOfKnownCertificats)
            {
                UsernameInput.Items.Add(username);
            }
        }

        private void LoginButtonClick(object sender, RoutedEventArgs e)
        {
            Controller.UserWantsToLogIn(UsernameInput.Text, PasswordInput.Password);
        }

        public void ShowMessage(string message, bool error = false)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                if (error)
                {
                    Erroricon.Visibility = Visibility.Visible;
                    MessageLabel.Foreground = Brushes.Red;
                }
                else
                {
                    Erroricon.Visibility = Visibility.Hidden;
                    MessageLabel.Foreground = Brushes.Black;
                }

                MessageLabel.Text = message;
                MessageLabel.Visibility = Visibility.Visible;
                MessageBox.Visibility = Visibility.Visible;
            }, null);
        }

        public void Show()
        {
            Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            Visibility = Visibility.Collapsed;
        }


    }
     
}
