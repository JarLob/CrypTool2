using System;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
using CrypCloud.Core;
using CrypCloud.Manager.Services;
using CrypCloud.Manager.ViewModels.Helper;
using CrypCloud.Manager.ViewModels.Pocos;
using Cryptool.PluginBase;
using voluntLib.common;
using WorkspaceManager.Model;

namespace CrypCloud.Manager.ViewModels
{
    public class JobListVM : ScreenViewModel
    {
        private readonly CrypCloudCore crypCloudCore = CrypCloudCore.Instance;
        public CrypCloudManager Manager { get; set; }

        public ObservableCollection<NetworkJobItem> RunningJobs { get; set; }
        public RelayCommand RefreshJobListCommand { get; set; }
        public RelayCommand CreateNewJobCommand { get; set; }
        public RelayCommand OpenJobCommand { get; set; }
        public RelayCommand DeleteJobCommand { get; set; }
        public RelayCommand DownloadWorkspaceCommand { get; set; } 

        public JobListVM()
        {
            CreateNewJobCommand = new RelayCommand(it => OpenJobCreation());
            RefreshJobListCommand = new RelayCommand(it => RefreshJobs());
            OpenJobCommand = new RelayCommand(OpenJob);
            DeleteJobCommand = new RelayCommand(DeleteJob);

            RunningJobs = new ObservableCollection<NetworkJobItem>();
            crypCloudCore.JobListChanged += RunInUiContext(UpdateJobList);
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
            crypCloudCore.RefreshJobList(); 
        }

        private void OpenJob(object it)
        {
            var jobItem = it as NetworkJobItem;
            if (jobItem == null) return; // shoudnt happen anyways

            crypCloudCore.JobListChanged += WaitForWorkspaceAndOpenIt(jobItem.Id);
            crypCloudCore.DownloadWorkspaceOfJob(jobItem.Id);
        }

        private Action WaitForWorkspaceAndOpenIt(BigInteger id)
        {
            Action waitForWorkspace = null;
            waitForWorkspace = (() =>
            {
                var workspaceModel = crypCloudCore.GetWorkspaceOfJob(id);
                if (workspaceModel == null) 
                    return;
                
                crypCloudCore.JobListChanged -= waitForWorkspace;
                RunInUiContext(() => Manager.OpenWorkspaceInNewTab(workspaceModel));
            });

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
                NumberOfBlocks = job.StateConfig.NumberOfBlocks,
                Id = job.JobID,
                UserCanDeleteJob = crypCloudCore.UserCanDeleteJob(job),
                HasWorkspace = job.HasPayload(),
            };
          
            return item;
        }


        #endregion
    }
}
