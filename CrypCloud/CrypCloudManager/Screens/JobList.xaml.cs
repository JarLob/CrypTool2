using System;
using System.Collections.Generic; 
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;  
using System.Windows.Media;
using System.Windows.Threading;
using CrypCloud.Manager.Controller;
using CrypCloud.Manager.ViewModel;
using Cryptool.PluginBase.Attributes;

namespace CrypCloud.Manager.Screens
{ 
    public partial class JobList : UserControl
    {
        public JobListController Controller { get; set; }

        public JobList(){
            InitializeComponent(); 
        }

        public void UpdateJobList(List<NetworkJobVM> networkJobs)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate{
                if ( ! networkJobs.Any())
                {
                    ShowMessage(Properties.Resources.JobList_NoActiveJobs);
                    JobListBox.Visibility = Visibility.Collapsed;
                    return;
                }

                JobListBox.Items.Clear();
                foreach (var networkJob in networkJobs)
                {
                   JobListBox.Visibility = Visibility.Visible;
                   JobListBox.Items.Add(networkJob);
                }
            }, null);
        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            Controller.ShowJobCreation();
        }

        private void RefreshButtonClick(object sender, RoutedEventArgs e)
        {
            
        } 

        private void ShowMessage(string message, bool error = false)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback) delegate
            {
                Erroricon.Visibility = Visibility.Hidden;
                MessageLabel.Foreground = Brushes.Black;
                if (error)
                {
                    Erroricon.Visibility = Visibility.Visible;
                    MessageLabel.Foreground = Brushes.Red;
                }

                MessageLabel.Text = message;
                MessageLabel.Visibility = Visibility.Visible;
                MessageBox.Visibility = Visibility.Visible;
            }, null);
        }

        public void Show()
        {
            Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            Visibility = Visibility.Collapsed;
        }
         
    }
}
