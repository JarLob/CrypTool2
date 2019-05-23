/*
   Copyright 2019 Nils Kopal <Nils.Kopal<at>CrypTool.org

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using Cryptool.PluginBase.Attributes;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Cryptool.Plugins.DECODEDatabaseTools
{
    [Localization("Cryptool.Plugins.DECODEDatabaseTools.Properties.Resources")]
    [SettingsTab("DECODESettingsTab", "/MainSettings/")]
    public partial class DECODESettingsTab : UserControl
    {
        private static RNGCryptoServiceProvider _rNGCryptoServiceProvider = new RNGCryptoServiceProvider();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settingsStyle"></param>
        public DECODESettingsTab(Style settingsStyle)
        {
            InitializeComponent();
            Resources.Add("settingsStyle", settingsStyle);
            UsernameTextbox.Text = Properties.Settings.Default.Username;
            if (Properties.Settings.Default.Password != null) 
            {
                try
                {
                    byte[] iv = Convert.FromBase64String(Properties.Settings.Default.PasswordIV);
                    PasswordTextbox.Password = UTF8Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(Properties.Settings.Default.Password), iv, DataProtectionScope.CurrentUser));
                }
                catch (Exception ex)
                {
                    //An exception occured during restore of password
                }
            }
        }

        /// <summary>
        /// Called every time the username textbox has been changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UsernameTextbox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Properties.Settings.Default.Username = UsernameTextbox.Text;
            Properties.Settings.Default.Save();
            JsonDownloaderAndConverter.LogOut();
        }

        /// <summary>
        /// Called every time the password textbox has been changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PasswordTextbox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            byte[] iv = new byte[16];
            _rNGCryptoServiceProvider.GetBytes(iv);
            Properties.Settings.Default.PasswordIV = Convert.ToBase64String(iv);
            Properties.Settings.Default.Password = Convert.ToBase64String(ProtectedData.Protect(UTF8Encoding.UTF8.GetBytes(PasswordTextbox.Password), iv, DataProtectionScope.CurrentUser));            
            Properties.Settings.Default.Save();
            JsonDownloaderAndConverter.LogOut();
        }

        /// <summary>
        /// Returns the username of the DECODE user
        /// </summary>
        /// <returns></returns>
        internal static string GetUsername()
        {
            return Properties.Settings.Default.Username;
        }

        /// <summary>
        /// Returns the password of the DECODE user
        /// </summary>
        /// <returns></returns>
        internal static string GetPassword()
        {
            byte[] iv = Convert.FromBase64String(Properties.Settings.Default.PasswordIV);
            return UTF8Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(Properties.Settings.Default.Password), iv, DataProtectionScope.CurrentUser));

        }

        /// <summary>
        /// Method to test login data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TestLoginDataButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var loginOk = JsonDownloaderAndConverter.Login(GetUsername(), GetPassword());
                if (loginOk)
                {
                    MessageBox.Show(Properties.Resources.CredentialsOK, null, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(Properties.Resources.CredentialsWrong,null,MessageBoxButton.OK,MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
