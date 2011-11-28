﻿/*
   Copyright 2010 Sven Rech

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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Cryptool.CrypWin.Properties;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Attributes;

namespace Cryptool.CrypWin.SettingsTabs
{
    /// <summary>
    /// Interaction logic for NotificationIconSettingsTab.xaml
    /// </summary>
    [SettingsTab("Cryptool.CrypWin.SettingsTabs.Resources.res", "EditorSettings", "/")]
    public partial class EditorSettingsTab : UserControl
    {
        public EditorSettingsTab(Style settingsStyle)
        {
            Resources.Add("settingsStyle", settingsStyle);
            InitializeComponent();

            Settings.Default.SettingChanging += new System.Configuration.SettingChangingEventHandler(SettingChanging);

            var cultures = CultureInfo.GetCultures(CultureTypes.NeutralCultures);
            foreach (var cultureInfo in cultures)
            {
                Culture.Items.Add(cultureInfo);
                if (cultureInfo == CultureInfo.CurrentCulture)
                    Culture.SelectedItem = cultureInfo;
            }
        }

        void SettingChanging(object sender, System.Configuration.SettingChangingEventArgs e)
        {
            //TODO: only call the next line, if change was not triggered by SettingsPresentation
            //SetComponentsToSettings();
        }

        private void Culture_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Doesn't work:
            Thread.CurrentThread.CurrentCulture = (CultureInfo)Culture.SelectedItem;
            Thread.CurrentThread.CurrentUICulture = (CultureInfo)Culture.SelectedItem;
        }
    }
}
