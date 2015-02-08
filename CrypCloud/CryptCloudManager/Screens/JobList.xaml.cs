using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls; 
using System.Windows.Input; 
using System.Windows.Threading;

namespace CryptCloud.Manager.Screens
{
    
    public partial class JobList : UserControl
    {
        private Timer refreshListTimer;
        private readonly DispatcherTimer updateJobDetailsTimer;
      
        private bool participating;
        public bool Participating
        {
            get { return participating; }
            set
            {
                participating = value;
                OnPropertyChanged("Participating");
            }
        }

        public JobList()
        {
            InitializeComponent();

        //    updateJobDetailsTimer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(5)};
        //    updateJobDetailsTimer.Tick += UpdateJobDetailsTimerElapsed;
        //    updateJobDetailsTimer.Start();

        }


        void UpdateJobDetailsTimerElapsed(object sender, EventArgs eventArgs)
        {
           
        }

        void UpdateTaskRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        }

        void P2PManager_OnP2PConnectionStateChangeOccurred(object sender, bool newState)
        {
            UpdateRefreshTimerSettings(newState);
        }

        private void UpdateRefreshTimerSettings(bool isConnected)
        {
        }

        void RefreshListTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (!IsVisible) return;
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(UpdateJobList));
        }

        private void RefreshButtonClick(object sender, RoutedEventArgs e)
        {
            UpdateJobList();
        }

        public void UpdateJobList()
        {
        }

        void HandleRefreshedJobList(object sender, RunWorkerCompletedEventArgs e)
        {
          
        }

        private void ParticipateButtonClick(object sender, RoutedEventArgs e)
        {
            ParticipateInSelectedJob();
        }

        private void ParticipateItemHandler(object sender, MouseButtonEventArgs e)
        {
            ParticipateInSelectedJob();
        }

        private void ParticipateInSelectedJob()
        {
          
        }

        private void JobParticipationWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Participating = false;
        }

        private void DeleteButtonClick(object sender, RoutedEventArgs e)
        {
            DeleteSelectedJob();
        }

        private void JobListBoxKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    ParticipateInSelectedJob();
                    break;
                case Key.Delete:
                    DeleteSelectedJob();
                    break;
            }
            
        }

        private void DeleteSelectedJob()
        {
        
        }

        private void BackgroundDeletionWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            //this.CryptCloudManagerPresentation.ShowJobCreationView();
        }
    }
}
