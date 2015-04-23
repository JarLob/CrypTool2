using System;
using System.Collections.Generic;
using System.Security;
using CertificateLibrary.Network;
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

        public LoginVM()
        {
            AvailableCertificates = new List<string>(CertificateHelper.GetNamesOfKnownCertificates());
            CreateNewAccountCommand = new RelayCommand(it => Navigator.ShowScreenWithPath(ScreenPaths.CreateAccount));
            ResetPasswordCommand = new RelayCommand(it => Navigator.ShowScreenWithPath(ScreenPaths.ResetPassword));

            LoginCommand = new RelayCommand(it => GetCertificateAndLogin());
        }

        /// <summary>
        /// Is called when the user clicks the login button
        /// </summary>
        private void GetCertificateAndLogin()
        {
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
