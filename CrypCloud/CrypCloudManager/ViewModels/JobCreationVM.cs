﻿using System; 
using System.Numerics; 
using CrypCloud.Core; 
using CrypCloud.Manager.Services;
using CrypCloud.Manager.ViewModels.Helper;
using WorkspaceManager.Model;

namespace CrypCloud.Manager.ViewModels
{
    public class JobCreationVM : ScreenViewModel
    {
        private readonly CrypCloudCore crypCloudCore = CrypCloudCore.Instance;

        public string Name { get; set; }
        public string LocalFilePath { get; set; }
        public string Description { get; set; }
        public BigInteger NumberOfBlocks { get; set; }

        public RelayCommand BackToListCmd { get; set; }
        public RelayCommand CreateNewJobCmd { get; set; }
        public RelayCommand SelectWorkspaceFromFilesystemCmd { get; set; }

        public JobCreationVM()
        {
            Description = "";
            Name = "";

            BackToListCmd = new RelayCommand(it => ShowListView());
            CreateNewJobCmd = new RelayCommand(it => CreateNewJob());
            SelectWorkspaceFromFilesystemCmd = new RelayCommand(it => SelectWorkspaceFromFilesystem());
        }

        private void ShowListView()
        {
            Navigator.ShowScreenWithPath(ScreenPaths.JobList);
        }

        private void SelectWorkspaceFromFilesystem()
        {
            LocalFilePath = WorkspaceHelper.OpenFilePickerAndReturnPath();
            RaisePropertyChanged("LocalFilePath");
        }

        public void CreateNewJob()
        {
            var workspaceModel = TryDeserializeWorkspace(LocalFilePath);
            if (workspaceModel == null)
            {
                ErrorMessage = "Cannot load workspace from file";
                return;
            }

            var jobHasBeenCreated = crypCloudCore.CreateJob("CryptoolJob", Name, Description, workspaceModel, NumberOfBlocks);
            if (jobHasBeenCreated)
            {
                Navigator.ShowScreenWithPath(ScreenPaths.JobList); 
            }
            else
            {
                ErrorMessage = "Workspace didnt contain a cloud component";
            }
        }

        private WorkspaceModel TryDeserializeWorkspace(string filePath)
        {
            try
            {
                var pModel = (PersistantModel)XMLSerialization.XMLSerialization.Deserialize(filePath, true);
                return pModel.WorkspaceModel;
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}