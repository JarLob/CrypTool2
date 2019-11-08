
using CrypCloud.Manager.ViewModels;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace CrypCloud.Manager.Screens
{
    [Cryptool.PluginBase.Attributes.Localization("CrypCloud.Manager.Properties.Resources")]
    public partial class JobList : UserControl
    {
        public JobList()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Copies job information to the clipboard
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyJobInfo_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                if(DataContext == null)
                {
                    return;
                }
                JobListVM jobListVM = (JobListVM)DataContext;
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("Job id: " + BitConverter.ToString(jobListVM.SelectedJob.JobId.ToByteArray()).Replace("-", ""));
                stringBuilder.AppendLine("Job name: " + jobListVM.SelectedJob.JobName);
                stringBuilder.AppendLine("Job size (bytes): " + jobListVM.SelectedJob.JobSize);
                stringBuilder.AppendLine("Creation date: " + jobListVM.SelectedJob.CreationDate);
                stringBuilder.AppendLine("Creator name: " + jobListVM.SelectedJob.CreatorName);
                stringBuilder.AppendLine("Job description: " + jobListVM.SelectedJob.JobDescription);
                stringBuilder.AppendLine("Number of blocks: " + jobListVM.SelectedJob.NumberOfBlocks);
                stringBuilder.AppendLine("Number of calculated blocks: " + jobListVM.SelectedJob.NumberOfCalculatedBlocks);
                Clipboard.SetText(stringBuilder.ToString());
            }
            catch (Exception)
            {
                //do nothing
            }
        }
    }
}
