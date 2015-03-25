﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using CrypCloud.Core.utils;
using WorkspaceManager.Model;

namespace CrypCloud.Manager.Services
{
    class WorkspaceHelper
    {
        public static readonly string DefaultUserCachedWorkspaceDir = CertificateHelper.CreatePathInAppdata("CryptCloud", "Workspaces");

        private const string FileDialogExtention = ".cwm";
        private const string FileDialogFilter = "Workspace (.cwm)|*.cwm";

        public static void CreateDirectory()
        {
            Directory.CreateDirectory(DefaultUserCachedWorkspaceDir);
        }

        public static bool DoesDirectoryExists() 
        {
            return Directory.Exists(DefaultUserCachedWorkspaceDir);
        }

        public static string OpenFilePickerAndReturnPath()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog { DefaultExt = FileDialogExtention, Filter = FileDialogFilter };
            var result = dialog.ShowDialog();
            if (result == true)
            {
                return dialog.FileName;
            }
            return "";
        } 
  
    }
}