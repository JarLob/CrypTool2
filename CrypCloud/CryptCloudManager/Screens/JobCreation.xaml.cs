using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CrypCloud.Manager.Controller;
using CrypCloud.Manager.ViewModel;
using Cryptool.PluginBase.Attributes;

namespace CrypCloud.Manager.Screens
{ 
    public partial class JobCreation : UserControl
    {

        private readonly string FileDialogExtention = ".cwm";
        private readonly string FileDialogFilter = "Workspace (.cwm)|*.cwm";

        private NetworkJobVM networkJobVm;
        public JobCreationController Controller { get; set; }

        public JobCreation()
        {
            InitializeComponent();
            CreateNewContext();
        }

        public void InvalidWorkspace()
        {
            filePathLabel.Background = new SolidColorBrush(Color.FromRgb(255,0,0));
        }

        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            Controller.CreateNewJob(networkJobVm);
            CreateNewContext();
        }

        private void BackToList_Click(object sender, RoutedEventArgs e)
        {
            Controller.ShowJobList();
        } 

        private void BrowseFileButton_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog { DefaultExt = FileDialogExtention, Filter = FileDialogFilter };
            var result = dialog.ShowDialog();
            if (result == true)
            { 
                networkJobVm.LocalFilePath = dialog.FileName;
                filePathLabel.Content = TrimPath(networkJobVm.LocalFilePath); 
            }
        }

        private static string TrimPath(string path)
        {
            var result = path;
            var maxChars = 60;
            var remaining = 30;

            if (result.Length > maxChars)
            {
                var firstPart = result.Substring(0, remaining);
                var lastPart = result.Substring(result.Length - remaining);
                result = firstPart + " ... " + lastPart;
            }
            return result;
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
    }
}
