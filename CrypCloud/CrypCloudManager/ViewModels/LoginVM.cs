using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using CrypCloud.Core;
using CrypCloud.Manager.Services;
using CrypCloud.Manager.ViewModels.Helper;
using PeersAtPlay.CertificateLibrary.Network;

namespace CrypCloud.Manager.ViewModels
{
    public class LoginVM : BaseViewModel
    {
        public List<string> AvailableCertificates { get; set; }
        public string Username { get; set; }
        public SecureString Password { private get; set; }
         
        public RelayCommand LoginCommand { get; set; }
        public RelayCommand CreateNewAccountCommand { get; set; }
        public RelayCommand ResetPasswordCommand { get; set; }

        private bool rememberPassword;
        public bool RememberPassword
        {
            get { return rememberPassword; }
            set
            {
                rememberPassword = value;
                if (rememberPassword)
                {
                    RememberUserData();
                }
                else
                {
                    Settings.Default.rememberedUsername = "";
                    Settings.Default.rememberedPassword = "";
                    Settings.Default.Save();
                }

                RaisePropertyChanged("RememberPassword");
            }
        }

      
        public LoginVM()
        {
            AvailableCertificates = new List<string>(CertificateHelper.GetNamesOfKnownCertificates());
            CreateNewAccountCommand = new RelayCommand(it => Navigator.ShowScreenWithPath(ScreenPaths.CreateAccount));
            ResetPasswordCommand = new RelayCommand(it => Navigator.ShowScreenWithPath(ScreenPaths.ResetPassword));

            LoginCommand = new RelayCommand(it => GetCertificateAndLogin());

            var rememberedUsername = Settings.Default.rememberedUsername;
            if (!string.IsNullOrEmpty(rememberedUsername))
            {
                Username = rememberedUsername;
                Password = LoadPassword();
                rememberPassword = true;

                RaisePropertyChanged("rememberPassword");
                RaisePropertyChanged("Username");
                RaisePropertyChanged("Password");
            }
        }

        #region remember me

        private static SecureString LoadPassword()
        {
            var userData = Convert.FromBase64String(Settings.Default.rememberedPassword);
            var unprotect = ProtectedData.Unprotect(userData, new byte[0], DataProtectionScope.CurrentUser);
            return new SecureString().FromString(Encoding.UTF8.GetString(unprotect));
        }
        private void RememberUserData()
        {
            Settings.Default.rememberedUsername = Username;
            var bytes = Encoding.UTF8.GetBytes(Password.ToUnsecuredString());
            var protectedPassword = ProtectedData.Protect(bytes, new byte[0], DataProtectionScope.CurrentUser);
            Settings.Default.rememberedPassword = Convert.ToBase64String(protectedPassword);
            Settings.Default.Save();
        }

        #endregion

        /// <summary>
        ///     Is called when the user clicks the login button
        /// </summary>
        private void GetCertificateAndLogin()
        {
            RememberUserData();
            if (CertificateHelper.UserCertificateIsUnknown(Username))
                LoadRemoteCertificateAndLogin();
            else
                LoadLocalCertificateAndLogin();
        }

        #region local certificate

        private void LoadLocalCertificateAndLogin()
        {
            var certificate = CertificateHelper.LoadPrivateCertificate(Username, Password);
            if (certificate == null)
            {
                ErrorMessage = "Unable to open certificate of " + Username;
                return;
            }

            if (CrypCloudCore.Instance.IsBannedCertificate(certificate))
            {
                ErrorMessage = "Your Certificate has been banned";
                return;  
            }


            if (CrypCloudCore.Instance.Login(certificate))
            {
                CrypCloudCore.Instance.RefreshJobList();
                Navigator.ShowScreenWithPath(ScreenPaths.JobList);
            }


            ErrorMessage = "";
        }

        #endregion

        #region remote certificate

        private void LoadRemoteCertificateAndLogin()
        {
            var errorAction = new Action<string>(msg => ErrorMessage = msg);
            var request = new CertificateRequest(Username, null, Password.ToUnsecuredString());

            CAServerHelper.RequestCertificate(request, OnCertificateReceived, HandleProcessingError, errorAction);
        }

        private void OnCertificateReceived(CertificateReceivedEventArgs arg)
        {
            CertificateHelper.StoreCertificate(arg.Certificate, arg.Password, arg.Certificate.Avatar);
            LoadLocalCertificateAndLogin();
        }

        private void HandleProcessingError(ProcessingErrorEventArgs arg)
        {
            if (arg.Type.Equals(ErrorType.CertificateNotYetAuthorized))
                ErrorMessage = "Certificate has not been authorized. Please try again later";
            else
                ErrorMessage = "Unable to get Certificate for user: " + Username;
        }

        #endregion
    }
}