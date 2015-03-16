using System;
using System.Collections.Generic;
using System.Security;
using CertificateLibrary.Certificates;
using CertificateLibrary.Network;
using CrypCloud.Core;
using CrypCloud.Manager.ViewModels.Helper;

namespace CrypCloud.Manager.ViewModels
{
    public class LoginVM : ScreenViewModel
    {
        public List<string> AvailableCertificates { get; set; }

        public string Username { get; set; }
        public SecureString Password { private get; set; }

        public RelayCommand LoginCommand { get; set; }
        public RelayCommand CreateNewAccountCommand { get; set; }
        public RelayCommand ResetPasswordCommand { get; set; }

        public LoginVM()
        {
            AvailableCertificates = new List<string>(CertificateHelper.GetNamesOfKnownCertificates());
            LoginCommand = new RelayCommand(it => TryLogin());
            CreateNewAccountCommand = new RelayCommand(it => Navigator.ShowScreenWithPath(ScreenPaths.CreateAccount));
            ResetPasswordCommand = new RelayCommand(it => Navigator.ShowScreenWithPath(ScreenPaths.ResetPassword));
        }

        private void TryLogin()
        {
            if (CertificateHelper.UserCertificateIsUnknown(Username))
            {
                ErrorMessage = "Certificate for user: " + Username + " is not known"; 
                return;
            }

            var certificate = CertificateHelper.LoadPrivateCertificate(Username, Password);
            if (certificate == null)
            {
                ErrorMessage = "Unable to open certificate of " + Username; 
                return;
            }

            var loginSuccessful = CrypCloudCore.Instance.Login(certificate);
            if (loginSuccessful)
            {
                CrypCloudCore.Instance.RefreshJobList();
                Navigator.ShowScreenWithPath(ScreenPaths.JobList);
            }

            ErrorMessage = "";
            RaisePropertyChanged("ErrorMessage");
        }

    }
}
