using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media; 

namespace CrypCloud.Manager.Screens
{ 
    public partial class JobCreation : UserControl
    {
       

        public JobCreation()
        {
            InitializeComponent();
        }

        /*
        public void InvalidWorkspace()
        {
            filePathLabel.Background = new SolidColorBrush(Color.FromRgb(255,0,0));
        }

     

        public void Show()
        {
            Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            Visibility = Visibility.Collapsed;
        }


        public void CreateNewContext()
        {
            networkJobVm = new NetworkJobVM();
            DataContext = networkJobVm;
            filePathLabel.Content = "";
        }
         * */
    }
}
