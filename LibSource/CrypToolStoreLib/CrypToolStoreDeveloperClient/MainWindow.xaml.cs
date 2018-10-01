using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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

namespace CrypToolStoreDeveloperClient
{
    public enum UiState
    {
        LoginScreen,
        MainMenu
    }

    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public string Password;
        public string Username;
        public bool IsLoggedIn;
        public UiState UiState;

        public MainWindow()
        {           
            InitializeComponent();           

            Username = null;
            Password = null;
            IsLoggedIn = false;

            LoginView.MainWindow = this;
            HeaderView.MainWindow = this;
            MainMenuView.MainWindow = this;

            ResizeMode = ResizeMode.CanMinimize;

            ChangeScreen(UiState.LoginScreen);            
        }

        public void ChangeScreen(UiState uiState)
        {
            UiState = uiState;

            HeaderView.Visibility = Visibility.Hidden;
            LoginView.Visibility = Visibility.Hidden;
            MainMenuView.Visibility = Visibility.Hidden;

            switch (uiState)
            {
                case UiState.LoginScreen:
                    LoginView.Visibility = Visibility.Visible;
                    Width = 400;
                    Height = 400;
                    break;
                case UiState.MainMenu:
                    HeaderView.Visibility = Visibility.Visible;
                    MainMenuView.Visibility = Visibility.Visible;
                    break;
            }
        }       
    }
}
