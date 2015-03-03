using System;
using System.Collections.Generic;
using System.Security;
using CrypCloud.Core;
using CrypCloud.Manager.ViewModels.Helper;

namespace CrypCloud.Manager.ViewModels
{
    public class LoginVM : ScreenViewModel
    {
        public string Username { get; set; }
        public SecureString Password { private get; set; }
        public List<string> AvailableCertificates { get; set; }
        public RelayCommand LoginCommand { get; set; }

        public LoginVM()
        {
            AvailableCertificates = new List<string>(CertificatHelper.GetNamesOfKnownCertificats());
            LoginCommand = new RelayCommand(it => TryLogin());
        }

        private void TryLogin()
        {
            if (CertificatHelper.UserCertificatIsUnknown(Username))
            {
                ErrorMessage = "Certificat for user: " + Username + " is not known"; 
                return;
            }

            var certificat = CertificatHelper.LoadPrivateCertificat(Username, Password);
            if (certificat == null)
            {
                ErrorMessage = "Unable to open certificat of " + Username; 
                return;
            }

            var loginSuccessful = CrypCloudCore.Instance.Login(certificat);
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
