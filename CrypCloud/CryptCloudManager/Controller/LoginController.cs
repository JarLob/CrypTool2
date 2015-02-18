using CrypCloud.Core;
using CrypCloud.Manager.Screens;
using Cryptool.PluginBase;

namespace CrypCloud.Manager.Controller
{
    public class LoginController : Controller<Login>
    {
        public LoginController(CrypCloudManager root, Login view) : base(view, root)
        {
            view.Controller = this;
        }

        public override void Activate()
        {
            View.SetSuggestetUsernames(CertificatHelper.GetNamesOfKnownCertificats());
            ShowView();
        }
      
        public void UserWantsToLogIn(string username, string password)
        {
            if (CertificatHelper.UserCertificatIsUnknown(username))
            {
                LogAndShowMessage("Certificat for user: " + username + " is not known");
                return;
            }

            var certificat = CertificatHelper.LoadPrivateCertificat(username, password);
            if (certificat == null)
            {
                LogAndShowMessage("Unable to open certificat of " + username);
                return;
            }

            var loginSuccessful = CrypCloudCore.Instance.Login(certificat);
            if (loginSuccessful)
            {
                Root.OpenJobListView();
            }
        }

        private void LogAndShowMessage(string msg)
        {
            View.ShowMessage(msg, true);
            Root.GuiLogMessage(msg, NotificationLevel.Warning);
        }
    }
}
