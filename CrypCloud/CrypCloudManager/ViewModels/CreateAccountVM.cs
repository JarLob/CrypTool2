using System; 
using System.Numerics;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using CertificateLibrary.Network;
using CrypCloud.Core; 
using CrypCloud.Manager.Services;
using CrypCloud.Manager.ViewModels.Helper;
using PeersAtPlay.CertificateLibrary.Network;
using WorkspaceManager.Model;

namespace CrypCloud.Manager.ViewModels
{
    public class CreateAccountVM : ScreenViewModel
    {
        //from http://www.regular-expressions.info/email.html
        public static readonly Regex EmailRegex = new Regex("^[A-Z0-9._%+-]+@[A-Z0-9.-]+\\.[A-Z]{2,6}$", RegexOptions.IgnoreCase);
        public static readonly int MinUsernameLength = 5;

        public SecureString PasswordConfirm { get; set; }
        public SecureString Password { get; set; }
        public String Username { get; set; }
        public String Email { get; set; }
        public String VerificationCode { get; set; }

        public RelayCommand BackCommand { get; set; } 
        public RelayCommand RequestCommand { get; set; }
        public RelayCommand VerificationCommand { get; set; }
        public RelayCommand GoToVerification { get; set; }

        private readonly CertificateClient certificateClient;

        #region dialog properties

        private bool successDialogVisible;
        private bool inputDialogVisible;
        private bool waitDialogVisible;
        private bool errorDialogVisible;
        private bool verificationDialogVisible;
         
        public bool ShowSuccessDialog
        {
            get { return successDialogVisible; }
            set
            { 
                successDialogVisible = value;
                RaisePropertyChanged("ShowSuccessDialog");
            }
        }

        public bool ShowInputDialog
        {
            get { return inputDialogVisible; }
            set
            {
                inputDialogVisible = value;
                RaisePropertyChanged("ShowInputDialog");
            }
        }

        public bool ShowWaitDialog
        {
            get { return waitDialogVisible; }
            set
            {
                waitDialogVisible = value;
                RaisePropertyChanged("ShowWaitDialog");
            }
        }

        public bool ShowErrorDialog
        {
            get { return errorDialogVisible; }
            set
            {
                errorDialogVisible = value;
                RaisePropertyChanged("ShowErrorDialog");
            }
        }

        public bool ShowVerificationDialog
        {
            get { return verificationDialogVisible; }
            set
            {
                verificationDialogVisible = value;
                RaisePropertyChanged("ShowVerificationDialog");
            }
        }

        #endregion

        public CreateAccountVM()
        {
            BackCommand = new RelayCommand(it => Navigator.ShowScreenWithPath(ScreenPaths.Login));

            RequestCommand = new RelayCommand(it => CreateAccount());
            VerificationCommand = new RelayCommand(it => VerifyAccount());
            GoToVerification = new RelayCommand(it => ShowVerification());

            certificateClient = new CertificateClient();
            certificateClient.ServerErrorOccurred += delegate { ShowErrorMessage("Server Error Occurred"); };
            certificateClient.InvalidCertificateRegistration += InvalidCertificateRequest;
            certificateClient.InvalidEmailVerification += InvalidEmailVerification;

            certificateClient.EmailVerificationRequired += delegate { ShowVerification(); };
            certificateClient.CertificateAuthorizationRequired += delegate { ShowSuccessMessage(); };
            certificateClient.CertificateReceived += ReceivedCertificate;

        }

        protected override void HasBeenActivated()
        {
            HideAll();
            ShowInputDialog = true;
        }


        private void ReceivedCertificate(object sender, CertificateReceivedEventArgs arg)
        {
            CertificateHelper.StoreCertificate(arg.Certificate.CaX509, arg.Password, arg.Certificate.Avatar);
        }

      
        #region create account

        private void CreateAccount()
        {
            if ( ! ValidateModel())
                return;

            ShowInputDialog = false;
            ShowWaitDialog = true;
            var request = new CertificateRegistration
            {
                Avatar = Username,
                Email = Email,
                Password = Password.ToUnsecuredString(),
                World = "Cryptool"
            };
            TryAsyncCommunication(() =>  certificateClient.RegisterCertificate(request));
        }

        private bool ValidateModel()
        {
            if (Username == null || Username.Length < MinUsernameLength)
            {
                ShowMessageBox("Invalid Username. Username has to be at leaset " + MinUsernameLength + " Characters long");
                return false;
            }

            if (Email == null || (!EmailRegex.IsMatch(Email)))
            {
                ShowMessageBox("Invalid Email.");
                return false;
            }

            if (!Password.IsEqualTo(PasswordConfirm))
            {
                ShowMessageBox("Passwords are not equal");
                return false;
            }

            return true;
        }

        #endregion

        #region verifyAccount

        private void VerifyAccount()
        {
            ShowVerificationDialog = false;
            ShowWaitDialog = true;
            TryAsyncCommunication(() => certificateClient.VerifyEmail(new EmailVerification(VerificationCode, false)));
        }

        #endregion

        private void TryAsyncCommunication(Action request)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    request();
                }
                catch (Exception ex)
                {
                    RunInUiContext(
                       () => ShowErrorMessage("There was a communication problem with the server: " + ex.Message + "\nPlease try again later"));
                }
            });
        }

        public void InvalidCertificateRequest(object sender, ProcessingErrorEventArgs args)
        {
            string errorMessage;
            switch (args.Type)
            {
                case ErrorType.AvatarAlreadyExists:
                    errorMessage = "The username already exists. Please chose another one.";
                    break;
                case ErrorType.EmailAlreadyExists:
                    errorMessage ="The email already exists. Please chose another ones.";
                    break;
                default:
                    errorMessage = "Invalid certificate request: " + args.Type;
                    break;
            }

            ShowErrorMessage(errorMessage);
        }

        private void InvalidEmailVerification(object sender, ProcessingErrorEventArgs args)
        {
            string errorMessage;
            switch (args.Type)
            {
                case ErrorType.AlreadyVerified:
                    errorMessage = "Email already verified.";
                    break;
                case ErrorType.WrongCode:
                    errorMessage = "Verification code is not correct.";
                    break;
                default:
                    errorMessage = "Invalid certificate request: " + args.Type;
                    break;
            }

            ShowErrorMessage(errorMessage);
        }

        #region dialog transistions

        private void ShowSuccessMessage()
        {
            HideAll();
            ShowErrorDialog = true;
        }

        private void ShowErrorMessage(string errorMesssage)
        {
            HideAll();
            ShowErrorDialog = true;
            ErrorMessage = errorMesssage;
        }

        private void ShowVerification()
        {
            HideAll();
            ShowVerificationDialog = true;
        }

        private void HideAll()
        {
            ShowSuccessDialog = false;
            ShowInputDialog = false;
            ShowWaitDialog = false;
            ShowErrorDialog = false;
            ShowVerificationDialog = false;
        }

        #endregion
    }
}
