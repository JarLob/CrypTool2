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
using CrypCloud.Core;
using Cryptool.Core; 
using Cryptool.PluginBase;
using Cryptool.PluginBase.Attributes;
using Cryptool.PluginBase.Editor;
using voluntLib;

namespace CryptCloud.Manager
{ 
    [TabColor("orange")]
    [EditorInfo("CryptCloud", false, true, false, false, true)]
    [Author("Paul Lelgemann", "lelgemann@cryptool.org", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("CryptCloudManager.Properties.Resources", "PluginCaption", "PluginTooltip", "CryptCloudManager/DetailedDescription/Description.xaml", "CryptCloudManager/images/icon.png")]
    public class CryptCloudManager : IEditor
    {
        private CrypCloudCore crypCloudCore;
        public CryptCloudManagerPresentation Screens
        {
            get { return (Presentation as CryptCloudManagerPresentation); }
            set { throw new NotImplementedException(); }
        }

        public CryptCloudManager()
        {
            Presentation = new CryptCloudManagerPresentation();
        }

        public void New()
        {
            crypCloudCore = CrypCloudCore.Instance;
            CheckOrCreateCertificatFolder();
            Try(InitPresentation);
        }

        private void InitPresentation()
        {
            Screens.Login.SetSuggestetUsernames(CertificatHelper.GetNamesOfKnownCertificats());
            Screens.Login.LoginClicked += LoginUser;

        }

        protected bool LoginUser(String username, String certificatPasswort)
        {
            if (CertificatHelper.UserCertificatIsUnknown(username))
            {
                GuiLogMessage("Certificat for user: " + username + " is not known", NotificationLevel.Warning);
                return false;
            }

            var certificat = CertificatHelper.LoadPrivateCertificat(username, certificatPasswort);
            if(certificat == null)
            {
                GuiLogMessage("Unable to open certificat of " + username, NotificationLevel.Warning);
                return false;
            }

            var loginSuccessful = crypCloudCore.Login(certificat);
            if(loginSuccessful)
            {
                Screens.ShowJobListView();
                return true;
            }

            GuiLogMessage("Unable to join network", NotificationLevel.Error);
            return false;
        }


        private static void CheckOrCreateCertificatFolder()
        {
            if ( ! CertificatHelper.CertificatDirectoryExists())
            {
                CertificatHelper.CreateCertificatDirectory();
            }
        }
        
        private void Try(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                GuiLogMessage(ex.Message, NotificationLevel.Error);
            }
        }


        public void Open(string fileName)
        {
            GuiLogMessage("CryptCloudManager: Open(" + fileName + ")", NotificationLevel.Debug);
            if (OnFileLoaded != null)
            {
                OnFileLoaded(this, fileName);
            }
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
