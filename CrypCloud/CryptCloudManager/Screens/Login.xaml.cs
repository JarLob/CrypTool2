using System;
using System.Collections.Generic; 
using System.Threading;
using System.Windows;
using System.Windows.Controls; 
using System.Windows.Media; 
using System.Windows.Threading; 

namespace CryptCloud.Manager.Screens
{
    public partial class Login : UserControl
    {
        public Login()
        {
            InitializeComponent(); 
        } 

        public void SetSuggestetUsernames(List<string> usernames)
        {
            foreach (var username in usernames)
            {
                UsernamesListBox.Items.Add(username);
            }
        } 

        private void LoginButtonClick(object sender, RoutedEventArgs e)
        {
            if ( ! ValidLoginData(UsernameInput.Text, PasswordInput.Password))
            {
                ShowMessage("no. NONONONONONONONONONO! ", true);
            }
        }

        private void ForgotPasswordLabel_Click(object sender, RoutedEventArgs e)
        {
            // CryptCloudManagerPresentation.ShowForgotPasswordView();
        }

        private void Username_TextChanged(object sender, TextChangedEventArgs e)
        {
            PopupUsernames.IsOpen = false;
        }
      
   
        private void ShowMessage(string message, bool error = false)
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


        private void UsernamesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UsernameInput.Text = (string)UsernamesListBox.SelectedItem;
            PasswordInput.Password = "";
        }

        private void PopupUsernames_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PopupUsernames.IsOpen = false;
        }


        public event Func<string, string, bool> LoginClicked;

        protected virtual bool ValidLoginData(string name, string password)
        {
            var handler = LoginClicked;
            if (handler != null)
            {
                return handler.Invoke(name, password);
            }
            return false;
        }
    }
}
