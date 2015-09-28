/*
   Copyright 2011 CrypTool 2 Team <ct2contact@cryptool.org>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using CrypCloud.Core;
using CrypCloud.Manager.Services;
using CrypCloud.Manager.ViewModels;
using Cryptool.Core; 
using Cryptool.PluginBase;
using Cryptool.PluginBase.Attributes;
using Cryptool.PluginBase.Editor;
using WorkspaceManager.Model;

namespace CrypCloud.Manager
{
    public enum ScreenPaths { Login, JobList, JobCreation, ResetPassword, CreateAccount, ConfirmAccount };

    [TabColor("white")]
    [EditorInfo("CrypCloud", true, false, false, false, true)]
    [Author("Christopher Konze", "c.konze@uni.de", "Universität Kassel", "")]
    [PluginInfo("CrypCloud.Manager.Properties.Resources", "PluginCaption", "PluginTooltip", "CrypCloudManager/DetailedDescription/doc.xml", "CrypCloudManager/images/icon.png")]
    public class CrypCloudManager : IEditor
    {
        private readonly ScreenNavigator screenNavigator = new ScreenNavigator();
        public static string DefaultTabName = "CrypCloud Project";

        public CrypCloudManager()
        {
            var crypCloudPresentation = new CrypCloudPresentation();
            Presentation = crypCloudPresentation;
            AddScreensToNavigator(crypCloudPresentation);

            var jobListVM = (JobListVM) crypCloudPresentation.JobList.DataContext;
            jobListVM.Manager = this;
        }

        private void AddScreensToNavigator(CrypCloudPresentation crypCloudPresentation)
        {
            //viewmodels are created in the xaml file due to autocomplete and typesafty reasons
            var loginVm = (BaseViewModel) crypCloudPresentation.Login.DataContext;
            screenNavigator.AddScreenWithPath(loginVm, ScreenPaths.Login);

            var jobListVm = (BaseViewModel) crypCloudPresentation.JobList.DataContext;
            screenNavigator.AddScreenWithPath(jobListVm, ScreenPaths.JobList);

            var jobCreateVm = (BaseViewModel)crypCloudPresentation.JobCreation.DataContext;
            screenNavigator.AddScreenWithPath(jobCreateVm, ScreenPaths.JobCreation);

            var createAccountVm = (BaseViewModel)crypCloudPresentation.CreateAccount.DataContext;
            screenNavigator.AddScreenWithPath(createAccountVm, ScreenPaths.CreateAccount);

            var resetPasswordVm = (BaseViewModel)crypCloudPresentation.ResetPassword.DataContext;
            screenNavigator.AddScreenWithPath(resetPasswordVm, ScreenPaths.ResetPassword);
        }

        public void OpenWorkspaceInNewTab(WorkspaceModel model)
        {
            if (OnOpenEditor == null) return; // cant open tab 

            var tabInfo = new TabInfo()
            {
                Title = DefaultTabName
            };

            var currentManager = (WorkspaceManager.WorkspaceManagerClass)OnOpenEditor(typeof(WorkspaceManager.WorkspaceManagerClass), tabInfo);
            currentManager.Open(model);
        }

        public void New()
        {
            if (!CertificateHelper.DoesDirectoryExists())
            {
                CertificateHelper.CreateDirectory();
            }
       
            screenNavigator.ShowScreenWithPath(CrypCloudCore.Instance.IsRunning ? ScreenPaths.JobList
                                                                                : ScreenPaths.Login);
        }

        public void GuiLogMessage(string message, NotificationLevel notificationLevel)
        {
            if (OnGuiLogNotificationOccured == null)
            {
                return;
            }

            var guiLogEvent = new GuiLogEventArgs(message, this, notificationLevel) { Title = "-" };
            OnGuiLogNotificationOccured(this, guiLogEvent);
        }
        
        public void Open(string fileName)
        {
            GuiLogMessage("CryptCloudManager: OpenFileDialog(" + fileName + ")", NotificationLevel.Debug);
            if (OnFileLoaded != null)
            {
                OnFileLoaded(this, fileName);
            }
        }

        public void SendOpenProjectFileEvent(string filename)
        {
            if (OnOpenProjectFile != null)
            {
                OnOpenProjectFile(this, filename);
            }
        }
        
        #region not utilized IEditor Members

        public event SelectedPluginChangedHandler OnSelectedPluginChanged;

        public event ProjectTitleChangedHandler OnProjectTitleChanged;

        public event OpenProjectFileHandler OnOpenProjectFile;

        public event FileLoadedHandler OnFileLoaded;


        public event OpenTabHandler OnOpenTab;
        public event OpenEditorHandler OnOpenEditor;

        public void AddText()
        {
            throw new NotImplementedException();
        }

        public void AddImage()
        {
            throw new NotImplementedException();
        }

        public void Save(string fileName)
        {
            GuiLogMessage("CryptCloudManager: Save(" + fileName + ")", NotificationLevel.Debug);
        }

        public void Add(Type type)
        {
            GuiLogMessage("CryptCloudManager: Add(" + type + ")", NotificationLevel.Debug);
        }

        public void Undo()
        {
            GuiLogMessage("CryptCloudManager: Undo()", NotificationLevel.Debug);
        }

        public void Redo()
        {
            GuiLogMessage("CryptCloudManager: Redo()", NotificationLevel.Debug);
        }

        public void Cut()
        {
            throw new NotImplementedException();
        }

        public void Copy()
        {
            throw new NotImplementedException();
        }

        public void Paste()
        {
            throw new NotImplementedException();
        }

        public void Remove()
        {
            throw new NotImplementedException();
        }

        public void Print()
        {
            throw new NotImplementedException();
        }

        public void ShowSelectedEntityHelp()
        {
        }

        public bool CanUndo
        {
            get { return false; }
        }

        public bool CanRedo
        {
            get { return false; }
        }

        public bool CanCut
        {
            get { return false; }
        }

        public bool CanCopy
        {
            get { return false; }
        }

        public bool CanPaste
        {
            get { return false; }
        }

        public bool CanRemove
        {
            get { return false; }
        }

        public bool CanExecute
        {
            get { return false; }
        }

        public bool CanStop
        {
            get { return false; }
        }

        public bool HasChanges
        {
            get { return false; }
        }

        public bool CanPrint
        {
            get { return false; }
        }

        public bool CanSave
        {
            get { return false; }
        }

        public string CurrentFile
        {
            get { return null; }
        }

        public string SamplesDir
        {
            set { }
        }

        public bool ReadOnly { get; set; }

        public PluginManager PluginManager
        {
            get { return null;}
            set { }
        }

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public ISettings Settings { get; private set; }

        public UserControl Presentation { get; private set; }

        public void Execute()
        {
        }

        public void Stop()
        {
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;
         

        #endregion

    } 
   
}
