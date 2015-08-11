using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using CrypCloud.Core;
using CrypCloud.Manager.ViewModels.Helper;
using CrypCloud.Manager.ViewModels.Pocos;
using voluntLib.common;

namespace CrypCloud.Manager.ViewModels
{
    public class JobListVM : BaseViewModel
    {
        private readonly CrypCloudCore crypCloudCore = CrypCloudCore.Instance;
        public CrypCloudManager Manager { get; set; } 

        public ObservableCollection<NetworkJobItem> RunningJobs { get; set; }
        public RelayCommand RefreshJobListCommand { get; set; }
        public RelayCommand CreateNewJobCommand { get; set; }
        public RelayCommand OpenJobCommand { get; set; }
        public RelayCommand DeleteJobCommand { get; set; }
        public RelayCommand DownloadWorkspaceCommand { get; set; }
        public RelayCommand LogOutCommand { get; set; } 

        public JobListVM()
        {
            CreateNewJobCommand = new RelayCommand(it => OpenJobCreation());
            RefreshJobListCommand = new RelayCommand(it => RefreshJobs());
            LogOutCommand = new RelayCommand(it => Logout());
            OpenJobCommand = new RelayCommand(OpenJob);
            DeleteJobCommand = new RelayCommand(DeleteJob);

            RunningJobs = new ObservableCollection<NetworkJobItem>();
            crypCloudCore.JobListChanged += (s, e) => RunInUiContext(UpdateJobList);
            crypCloudCore.JobStateChanged +=  (s, e) => RunInUiContext(UpdateJobList);
        }

        private void Logout()
        {
            if (crypCloudCore.IsPartizipationOnJob())
            {
                ErrorMessage = CrypCloud.Manager.Properties.Resources.Stop_Running_Jobs_Before_Logout;
                return;
            }
            
            ErrorMessage = "";
            crypCloudCore.Logout();
            Navigator.ShowScreenWithPath(ScreenPaths.Login);
        }

        protected override void HasBeenActivated()
        {
            base.HasBeenActivated();
            UpdateJobList();
        }

        private void UpdateJobList()
        {
            RunningJobs.Clear();
            var jobs = crypCloudCore.GetJobs();
            jobs.ForEach(it => RunningJobs.Add(ConvertToListItem(it)));
        }

        private void OpenJobCreation()
        {
            Navigator.ShowScreenWithPath(ScreenPaths.JobCreation);
        } 

        private void RefreshJobs()
        {
            UpdateJobList();
            crypCloudCore.RefreshJobList(); 
        }

        private void OpenJob(object it)
        {
            var jobItem = it as NetworkJobItem;
            if (jobItem == null) return; // shoudnt happen anyways

            crypCloudCore.JobListChanged += WaitForWorkspaceAndOpenIt(jobItem.Id);
            crypCloudCore.DownloadWorkspaceOfJob(jobItem.Id);
        }

        private EventHandler WaitForWorkspaceAndOpenIt(BigInteger id)
        {
            EventHandler waitForWorkspace = null;
            waitForWorkspace = (s, e) =>
            {
                var workspaceModel = crypCloudCore.GetWorkspaceOfJob(id);
                if (workspaceModel == null) return;
                
                crypCloudCore.JobListChanged -= waitForWorkspace;
                UiContext.StartNew(() => Manager.OpenWorkspaceInNewTab(workspaceModel));
            };

            return waitForWorkspace;
        }
        
        private void DeleteJob(object it)
        {
            var jobItem = it as NetworkJobItem;
            if (jobItem == null) return; // shoudnt happen anyways
               
            crypCloudCore.DeleteJob(jobItem.Id);
        }

        #region helper

        private NetworkJobItem ConvertToListItem(NetworkJob job)
        {
            var item = new NetworkJobItem
            {
                Name = job.JobName,
                Creator = job.Creator,
                TotalNumberOfBlocks = job.StateConfig.NumberOfBlocks,
                FinishedNumberOfBlocks = crypCloudCore.GetProgressOfJob(job.JobID),
                Id = job.JobID,
                UserCanDeleteJob = crypCloudCore.UserCanDeleteJob(job),
                HasWorkspace = job.HasPayload(),
            };
          
            return item;
        }


        #endregion
    }
}
