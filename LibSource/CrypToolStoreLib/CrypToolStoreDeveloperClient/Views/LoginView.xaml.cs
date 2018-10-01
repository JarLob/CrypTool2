using CrypToolStoreLib.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CrypToolStoreDeveloperClient.Views
{
    /// <summary>
    /// Interaktionslogik für LoginView.xaml
    /// </summary>
    public partial class LoginView : UserControl
    {
        public MainWindow MainWindow { get; set; }

        public LoginView()
        {
            InitializeComponent();
            Loaded += LoginView_Loaded;
        }

        private void LoginView_Loaded(object sender, RoutedEventArgs e)
        {
            Username.Focus();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            lock (this)
            {
                try
                {
                    if (MainWindow.IsLoggedIn == true)
                    {
                        return;
                    }

                    if (String.IsNullOrEmpty(Username.Text))
                    {
                        MessageBox.Show("You have to enter a username to login.", "No username entered");
                        return;
                    }
                    if (String.IsNullOrEmpty(Password.Password))
                    {
                        MessageBox.Show("You have to enter a password to login.", "No username entered");
                        return;
                    }

                    CrypToolStoreClient client = new CrypToolStoreClient();
                    client.ServerAddress = Constants.ServerAddress;
                    client.ServerPort = Constants.ServerPort;
                    client.Connect();

                    if (client.Login(Username.Text, Password.Password))
                    {
                        MainWindow.IsLoggedIn = true;
                        MainWindow.Username = Username.Text;
                        MainWindow.Password = Password.Password;
                        MainWindow.HeaderView.Username = Username.Text;
                        client.Disconnect();
                        ((MainWindow)((Grid)this.Parent).Parent).ChangeScreen(UiState.MainMenu);
                    }
                    else
                    {
                        MainWindow.IsLoggedIn = false;
                        MessageBox.Show("Username or Password wrong", "Login failed");
                    }


                }
                catch (Exception ex)
                {
                    MessageBox.Show(String.Format("Exception during login: {0}", ex.Message), "Login failed");
                }
            }
        }

        private void Password_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                LoginButton_Click(sender, e);
            }
        }
    }
}
