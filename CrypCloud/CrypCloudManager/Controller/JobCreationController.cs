using System;
using System.IO;
using CrypCloud.Core;
using CrypCloud.Core.utils;
using CrypCloud.Manager.Screens;
using CrypCloud.Manager.ViewModel;
using WorkspaceManager.Model;
namespace CrypCloud.Manager.Controller
{
    public class JobCreationController : Controller<JobCreation>
    { 
        private readonly CrypCloudCore crypCloudCore;

        public JobCreationController(CrypCloudManager root, JobCreation view)
            : base(view, root)
        {  
            crypCloudCore = CrypCloudCore.Instance;
            View.Controller = this;
        } 

        public void ShowJobList()
        {
            Root.OpenJobListView();
        }

        public void CreateNewJob(NetworkJobVM job)
        {
            var workspaceModel = TryDeserializeWorkspace(job.LocalFilePath);
            if (workspaceModel == null)
            {
                Root.GuiLogMessage("Cannot load workspace from file", Cryptool.PluginBase.NotificationLevel.Warning);
                View.InvalidWorkspace();
                return;
            } 

            var success = crypCloudCore.CreateJob(job.Type, job.Name, job.Description, workspaceModel, job.NumberOfBlocks);
            if (success)
            {
                Root.GuiLogMessage("Job has been created", Cryptool.PluginBase.NotificationLevel.Debug);
                ShowJobList();
            }
            else
            {
                Root.GuiLogMessage("Workspace didnt contain a cloud component", Cryptool.PluginBase.NotificationLevel.Warning);
                View.InvalidWorkspace();
            }
        }

        private WorkspaceModel TryDeserializeWorkspace(string filePath)
        { 
            try
            {
                var pModel = (PersistantModel) XMLSerialization.XMLSerialization.Deserialize(filePath, true);
                return pModel.WorkspaceModel;
            }
            catch (Exception e)
            {
                Root.GuiLogMessage(e.Message, Cryptool.PluginBase.NotificationLevel.Error);
                return null;
            }
        }
    }
}
