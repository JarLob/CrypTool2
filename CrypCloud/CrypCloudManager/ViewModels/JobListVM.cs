using System.Collections.ObjectModel;
using System.Numerics;
using CrypCloud.Core;
using CrypCloud.Manager.Services;
using CrypCloud.Manager.ViewModels.Helper;
using CrypCloud.Manager.ViewModels.Pocos;
using voluntLib.common;

namespace CrypCloud.Manager.ViewModels
{
    public class JobListVM : ScreenViewModel
    {
        private readonly CrypCloudCore crypCloudCore = CrypCloudCore.Instance;
        private readonly Collection<BigInteger> downloadingJobs = new Collection<BigInteger>();
        
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
            DownloadWorkspaceCommand = new RelayCommand(DownloadWorkspace);

            RunningJobs = new ObservableCollection<NetworkJobItem>();
            crypCloudCore.JobListChanged += RunInUiContext(OnJobListUpdate);
            OnJobListUpdate();
        }
        
        private void OnJobListUpdate()
        {
            RunningJobs.Clear();
            var jobs = crypCloudCore.GetJobs();
            jobs.ForEach(it => RunningJobs.Add(ConvertToListItem(it)));

            //remove downloaded jobs from locale cache
            jobs.FindAll(it => downloadingJobs.Contains(it.JobID) && it.HasPayload())
                .ForEach(it => downloadingJobs.Remove(it.JobID));
        }

        private void OpenJobCreation()
        {
            Navigator.ShowScreenWithPath(ScreenPaths.JobCreation);
        }

        private void RefreshJobs()
        {
            crypCloudCore.RefreshJobList();
        }

        private void DownloadWorkspace(object it)
        {
            var jobItem = it as NetworkJobItem;
            if (jobItem == null) return; // shoudnt happen anyways

            crypCloudCore.DownloadWorkspaceOfJob(jobItem.Id); 
            downloadingJobs.Add(jobItem.Id);
            OnJobListUpdate();
        }

        private void OpenJob(object it)
        {
            var jobItem = it as NetworkJobItem;
            if (jobItem == null) return; // shoudnt happen anyways

            var workspaceModel = crypCloudCore.GetWorkspaceOfJob(jobItem.Id);
            WorkspaceHelper.SaveWorkspaceForJobId(workspaceModel, jobItem.Id);
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
                DownloadingWorkspace = downloadingJobs.Contains(job.JobID)
            };
          
            return item;
        }


        #endregion
    }
}
