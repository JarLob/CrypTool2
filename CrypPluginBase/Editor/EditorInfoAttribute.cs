﻿/*
   Copyright 2008 Martin Saternus, University of Duisburg-Essen

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

namespace Cryptool.PluginBase.Editor
{
    /// <summary>
    /// The default file-extension for the editor used by CrypWin to display 
    /// Open/Save FileDialog with correct filter.
    /// </summary>
    public class EditorInfoAttribute : Attribute
    {
        public bool Singleton;
        public bool ShowAsNewButton;
        public bool ShowLogPanel;
        public bool ShowSettingsPanel;
        public bool ShowComponentPanel;
        public string DefaultExtension;
        public bool CanEdit;

        // wander 2011-12-13: showSettingsPanel defaults to false in favor of WorkspaceManager parameter panel
        public EditorInfoAttribute(string defaultExtension, bool showAsNewButton = true, bool showLogPanel = true, bool showSettingsPanel = false, bool showComponentPanel = true, bool singleton = false, bool canEdit = false)
        {
            Singleton = singleton;
            ShowAsNewButton = showAsNewButton;
            DefaultExtension = defaultExtension;
            ShowComponentPanel = showComponentPanel;
            ShowLogPanel = showLogPanel;
            ShowSettingsPanel = showSettingsPanel;
            CanEdit = canEdit;
        }
    }
}
