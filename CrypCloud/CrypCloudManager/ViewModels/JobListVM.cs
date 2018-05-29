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

        public ObservableCollection<Job> RunningJobs { get; set; } 

        private Job selectedJob;
        public Job SelectedJob
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
        public string Username { get; set; }

        public JobListVM()
        {
            CreateNewJobCommand = new RelayCommand(it => OpenJobCreation());
            RefreshJobListCommand = new RelayCommand(it => RefreshJobs());
            LogOutCommand = new RelayCommand(it => Logout());
            OpenJobCommand = new RelayCommand(OpenJob);
            DeleteJobCommand = new RelayCommand(DeleteJob);
            DownloadWorkspaceCommand = new RelayCommand(DownloadJob);            
        }

        protected override void HasBeenActivated()
        {
            base.HasBeenActivated();
            RunningJobs = crypCloudCore.GetJoblist();
            RaisePropertyChanged("RunningJobs");
            if (RunningJobs.Count > 0)
            {
                selectedJob = RunningJobs.First();
                RaisePropertyChanged("SelectedJob");
            }

            Username = CrypCloudCore.Instance.GetUsername();
            RaisePropertyChanged("Username");            
        }

        private void OpenJobCreation()
        {
            Navigator.ShowScreenWithPath(ScreenPaths.JobCreation);
        } 

        private void RefreshJobs()
        {            
            crypCloudCore.RefreshJobList(); 
        }

        private void Logout()
        {
            try
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
            catch (Exception ex)
            {
                ErrorMessage = String.Format("Exception while logging out: {0}", ex.Message);                
            }
        }

        #region open job       

        private void DownloadJob(object obj)
        {
            try
            {
                var job = obj as Job;
                if (job == null) return;

                crypCloudCore.DownloadWorkspaceOfJob(job.JobId);
            }
            catch (Exception ex)
            {
                ErrorMessage = String.Format("Exception while downloading job: {0}", ex.Message);
            }
        }

        private void OpenJob(object it)
        {
            try
            {
                var job = it as Job;
                if (job == null) return; // shoudnt happen anyways

                if (!job.HasPayload)
                {
                    ErrorMessage = "Cannot open job without downloading it before";
                    return;
                }

                var workspaceModel = crypCloudCore.GetWorkspaceOfJob(job.JobId);
                var workspaceEditor = workspaceModel.MyEditor;
                if (workspaceEditor == null || workspaceEditor.HasBeenClosed)
                {                   
                    UiContext.StartNew(() => 
                    {
                        try
                        {
                            Manager.OpenWorkspaceInNewTab(workspaceModel, job.JobId, job.JobName);
                        }
                        catch (Exception ex)
                        {
                            ErrorMessage = String.Format("Exception while opening new tab: {0}", ex.Message);
                        }
                    
                    });
                }
                else
                {
                    ErrorMessage = "Workspace is already open.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = String.Format("Exception while opening job: {0}", ex.Message);
            }
        }

        #endregion

        private void DeleteJob(object it)
        {
            try
            {
                var job = it as Job;
                if (job == null) return; // shoudnt happen anyways

                var confirmResult = MessageBox.Show(Resources._Confirm_Job_Deletion_Text, Resources._Confirm_Job_Deletion_Title, MessageBoxButton.YesNo);
                if (confirmResult == MessageBoxResult.Yes)
                {
                    crypCloudCore.DeleteJob(job.JobId);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = String.Format("Exception while deleting job: {0}", ex.Message);
            }
        }

        #region helper       

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
