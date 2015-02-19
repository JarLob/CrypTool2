using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Documents;
using CrypCloud.Core;
using CrypCloud.Manager.Screens;
using CrypCloud.Manager.ViewModel;
using voluntLib.common;

namespace CrypCloud.Manager.Controller
{
    public class JobListController : Controller<JobList>
    { 
        private readonly CrypCloudCore crypCloudCore;

        public JobListController(CrypCloudManager root, JobList view)
            : base(view, root)
        {  
            crypCloudCore = CrypCloudCore.Instance;
            View.Controller = this;
        }

        public override void Activate()
        {
            crypCloudCore.JobListChanged += FetchJobsAndUpdateList;
            ShowView();
            FetchJobsAndUpdateList();
        }

        public override void Deactivate()
        {
            crypCloudCore.JobListChanged -= FetchJobsAndUpdateList;
            HideView(); 
        }
        
        private void FetchJobsAndUpdateList()
        {
            var networkJobs = crypCloudCore.GetJobs();
            var viewModel = networkJobs.Select(ConvertToVmObject).ToList();
            View.UpdateJobList(viewModel);
        }

        private static NetworkJobVM ConvertToVmObject(NetworkJob networkJob)
        {
            return new NetworkJobVM
            {
                Id = networkJob.JobID,
                Name = networkJob.JobName,
                Type = networkJob.JobType,
                Creator = networkJob.Creator
            };
        }

        public void ShowJobCreation()
        {
            Root.OpenJobCreationView();
        }  
    }
}
