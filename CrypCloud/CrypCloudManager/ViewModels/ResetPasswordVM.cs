using System; 
using System.Numerics;
using System.Security;
using CrypCloud.Core; 
using CrypCloud.Manager.Services;
using CrypCloud.Manager.ViewModels.Helper;
using WorkspaceManager.Model;

namespace CrypCloud.Manager.ViewModels
{
    public class ResetPassword : ScreenViewModel
    {
      
        public String Name { get; set; }

        public RelayCommand VerifyCommand { get; set; }
        public RelayCommand BackCommand { get; set; } 
        public RelayCommand ResetRequestCommand { get; set; }

        public ResetPassword()
        {
            ResetRequestCommand = new RelayCommand(it => ResetAccount());
            BackCommand = new RelayCommand(it => Navigator.ShowScreenWithPath(ScreenPaths.Login));
            VerifyCommand = new RelayCommand(it => VerifyAccount());
        }

        private void ResetAccount()
        {
            //TODO
        }

        private void VerifyAccount()
        {
            //TODO 
        }
 
    
    }
}
