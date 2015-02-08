using System.Windows;
using System.Windows.Controls;
using CryptCloud.Manager.Screens;
namespace CryptCloud.Manager
{
    public partial class CryptCloudManagerPresentation : UserControl
    {
        
        public CryptCloudManagerPresentation()
        {
            InitializeComponent();
        }

        public Login GetLogin()
        {
            return Login;
        }

        public void ShowJobListView()
        {
            HideAllViews();
            JobList.Visibility = Visibility.Visible;
        }


        internal void HideAllViews()
        {
            Login.Visibility = Visibility.Hidden;
            JobList.Visibility = Visibility.Hidden;
        }
      
    }
}
