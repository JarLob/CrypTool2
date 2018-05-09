using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using CrypCloud.Core;
using CrypCloud.Manager.Properties;
using CrypCloud.Manager.ViewModels.Helper;
using CrypCloud.Manager.ViewModels.Pocos;
using KeySearcher.CrypCloud.statistics;
using MessageBox = System.Windows.MessageBox;
using UserControl = System.Windows.Controls.UserControl;
using VoluntLib2;
using VoluntLib2.ManagementLayer;

namespace CrypCloud.Manager.ViewModels
{
    public class JobListVM : BaseViewModel
    {
        private readonly CrypCloudCore crypCloudCore = CrypCloudCore.Instance;
        public CrypCloudManager Manager { get; set; } 

        public ObservableCollection<NetworkJobItem> RunningJobs { get; set; } 
        private NetworkJobItem selectedJob;
        public NetworkJobItem SelectedJob
        {
            get { return selectedJob; }
            set
            {
                if (selectedJob == value)
                    return;

                selectedJob = value;
            }
        }
        public RelayCommand RefreshJobListCommand { get; set; }
        public RelayCommand CreateNewJobCommand { get; set; }
        public RelayCommand OpenJobCommand { get; set; }
        public RelayCommand DeleteJobCommand { get; set; }
        public RelayCommand DownloadWorkspaceCommand { get; set; }
        public RelayCommand LogOutCommand { get; set; }
        public RelayCommand DoubleClickOnEntryCommand { get; set; }
        public string Username { get; set; }

        public JobListVM()
        {
            CreateNewJobCommand = new RelayCommand(it => OpenJobCreation());
            RefreshJobListCommand = new RelayCommand(it => RefreshJobs());
            LogOutCommand = new RelayCommand(it => Logout());
            OpenJobCommand = new RelayCommand(OpenJob);
            DeleteJobCommand = new RelayCommand(DeleteJob);
            DownloadWorkspaceCommand = new RelayCommand(DownloadJob);
            DoubleClickOnEntryCommand = new RelayCommand(DoubleClickOnEntry);

            RunningJobs = new ObservableCollection<NetworkJobItem>();
            crypCloudCore.JobListChanged += (s, e) => RunInUiContext(UpdateJobList);
            crypCloudCore.JobStateChanged +=  (s, e) => RunInUiContext(UpdateJobList);
           
        }

        protected override void HasBeenActivated()
        {
            base.HasBeenActivated(); 
            UpdateJobList();
            if (RunningJobs.Count > 0)
            {
                selectedJob = RunningJobs.First();
                RaisePropertyChanged("SelectedJob");
            }

            Username = CrypCloudCore.Instance.GetUsername();
            RaisePropertyChanged("Username");
        }

        private void UpdateJobList()
        {
            BigInteger selectedID = new BigInteger(0);
            if (SelectedJob != null)
            {
                selectedID = selectedJob.Id;
            }

            var jobItems = crypCloudCore.GetJobs()
                .Select(ConvertToListItem)
                .Distinct(NetworkJobItem.IdComparer)
                .ToList();

            RunningJobs.Clear();
            foreach (var networkJobItem in jobItems)
            {
                RunningJobs.Add(networkJobItem);
            }  

            SetSelectionByJobId(selectedID);
        }

        private void SetSelectionByJobId(BigInteger selectedID)
        {
            if (selectedID == 0) return;

            var newSelectedJob = RunningJobs.FirstOrDefault(networkJobItem => networkJobItem.Id == selectedID);
            if (newSelectedJob != null)
            {
                selectedJob = newSelectedJob;
                RaisePropertyChanged("SelectedJob");
            }
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

        #region open job
        private void DoubleClickOnEntry(object job)
        {
            if (selectedJob == null) return; // shoudnt happen anyways

            if (selectedJob.HasWorkspace)
            {
                OpenJob(selectedJob);
            }
            else
            {
                DownloadJob(selectedJob);
            }
        }

        private void DownloadJob(object it)
        {
            var jobItem = it as NetworkJobItem;
            if (jobItem == null) return; // shoudnt happen anyways

            crypCloudCore.DownloadWorkspaceOfJob(jobItem.Id);
        }


        private void OpenJob(object it)
        {
            var jobItem = it as NetworkJobItem;
            if (jobItem == null) return; // shoudnt happen anyways

            if ( ! jobItem.HasWorkspace)
            {
                ErrorMessage = "Cannot open job, without downloding it first";
                return;
            }

            var workspaceModel = crypCloudCore.GetWorkspaceOfJob(jobItem.Id);
            var workspaceEditor = workspaceModel.MyEditor;
            if (workspaceEditor == null || workspaceEditor.HasBeenClosed)
            {
                UiContext.StartNew(() => Manager.OpenWorkspaceInNewTab(workspaceModel, jobItem.Id));
            }
            else
            {
                ErrorMessage = "Workspace is already open.";
            }
        }

        #endregion

        private void DeleteJob(object it)
        {
            var jobItem = it as NetworkJobItem;
            if (jobItem == null) return; // shoudnt happen anyways

            var confirmResult = MessageBox.Show(Resources._Confirm_Job_Deletion_Text, Resources._Confirm_Job_Deletion_Title, MessageBoxButton.YesNo);
            if (confirmResult == MessageBoxResult.Yes)
            {
                crypCloudCore.DeleteJob(jobItem.Id);
            }
        }

        #region helper

        private NetworkJobItem ConvertToListItem(Job job)
        {
            var epochProgress = 0;

            /* ToDo: update with VoluntLib2
             * 
             * if (job.StateConfig.MaximumEpoch != 0)
            {
                epochProgress = (int) (100 * crypCloudCore.GetEpochOfJob(job).DivideAndReturnDouble(job.StateConfig.MaximumEpoch));
            } */

            var item = new NetworkJobItem
            {
                Name = job.JobName,
                Description = job.JobDescription,
                Creator = job.Creator,
                //TotalNumberOfBlocks = job.StateConfig.NumberOfBlocks,
                FinishedNumberOfBlocks = crypCloudCore.GetCalculatedBlocksOfJob(job.JobID),
                Id = job.JobID,
                UserCanDeleteJob = crypCloudCore.UserCanDeleteJob(job),
                HasWorkspace = job.HasPayload(),
                CreationDate = job.CreationDate.ToLocalTime(),
                //MaxEpoch = job.StateConfig.MaximumEpoch,
                JobSize = CalculateSizeString(job.JobSize),
                Epoch = crypCloudCore.GetEpochOfJob(job),
                EpochProgress = epochProgress                
            };

            if (item.HasWorkspace)
            {
               
            }
            
            var jobStateVisualization = crypCloudCore.GetJobStateVisualization(job.JobID);
            if (jobStateVisualization != null)
            {
                item.Visualization = Bitmap2BitmapImage(jobStateVisualization);
            }

            return item;
        }

        /// <summary>
        /// Converts the given siza as long to a string with
        /// bytes, KiB, or MiB
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        private string CalculateSizeString(long size)
        {
            if (size < 1024)
            {
                return size + " bytes";
            }
            else if (size < 1024 * 1024)
            {
                return Math.Round(size / 1024.0, 2) + " KiB";
            }
            else
            {
                return Math.Round(size / 1024.0 * 1024.0, 2) + " MiB";
            }            
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        private BitmapSource Bitmap2BitmapImage(Bitmap bitmap)
        {
            using (var resizedBitmap = ResizeBitmap(bitmap, 200, 200))
            {
                var hBitmap = resizedBitmap.GetHbitmap();
                try
                {
                    return Imaging.CreateBitmapSourceFromHBitmap(hBitmap,
                                IntPtr.Zero, 
                                Int32Rect.Empty,
                                BitmapSizeOptions.FromWidthAndHeight(200, 200));
                }
                finally
                {
                    DeleteObject(hBitmap);
                }
            }
        }

        private Bitmap ResizeBitmap(Bitmap sourceBMP, int width, int height)
        {
            var result = new Bitmap(width, height);
            using (var g = Graphics.FromImage(result))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(sourceBMP, 0, 0, width, height);
            }
            return result;
        }

        #endregion
    }
}
