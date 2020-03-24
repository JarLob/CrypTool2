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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Linq;
using Cryptool.Core;
using Cryptool.CrypWin.Properties;
using Cryptool.PluginBase.Attributes;
using Cryptool.PluginBase.IO;

namespace Cryptool.CrypWin.SettingsTabs
{
    /// <summary>
    /// Interaction logic for NotificationIconSettingsTab.xaml
    /// </summary>
    [Localization("Cryptool.CrypWin.SettingsTabs.Resources.res")]
    [SettingsTab("MainSettings", "/", 1.0)]
    public partial class MainSettingsTab : UserControl
    {
        private bool initialized = false;
        private static string[] supportedCultures = new string[] {"de", "en-US", "ru"};

        public MainSettingsTab(Style settingsStyle)
        {
            Resources.Add("settingsStyle", settingsStyle);
            InitializeComponent();

            var cultures = supportedCultures.ToDictionary(c => c, c => CultureInfo.CreateSpecificCulture(c));

            foreach (var cultureInfo in cultures.Values.OrderBy(c => c.DisplayName))
            {
                Culture.Items.Add(cultureInfo);
                if (cultureInfo.TextInfo.CultureName == CultureInfo.CurrentUICulture.TextInfo.CultureName)
                {
                    Culture.SelectedItem = cultureInfo;
                }
            }

            // Fallback if culture is not set
            if (Culture.SelectedItem == null && cultures.ContainsKey("en-US"))
            {
                Culture.SelectedItem = cultures["en-US"];
            }

            RecentFileListLengthBox.Text = RecentFileList.GetSingleton().ListLength.ToString();

            initialized = true;

            LogLevel.SelectedIndex = Settings.Default.LogLevel;
        }

        private void Culture_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (initialized)
            {
                var selected = (CultureInfo) Culture.SelectedItem;

                Properties.Settings.Default.Culture = selected.TextInfo.CultureName;
                MainWindow.SaveSettingsSavely();

                RestartLabel.Visibility = Visibility.Visible;
            }
        }

        private bool bindingInitialized = false;
        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!bindingInitialized && IsVisible)
            {
                Binding binding = new Binding("AvailableEditors");
                binding.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(MainWindow), 1);
                binding.NotifyOnTargetUpdated = true;
                EditorSelection.SetBinding(ItemsControl.ItemsSourceProperty, binding);              
                bindingInitialized = true;
            }
        }

        private void EditorSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.Default.preferredEditor = ((Type)EditorSelection.SelectedItem).FullName;
        }

        private void EditorSelection_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            if (Settings.Default.preferredEditor != null)
            {
                foreach (var editor in EditorSelection.Items)
                {
                    if (((Type)editor).FullName == Settings.Default.preferredEditor)
                    {
                        EditorSelection.SelectedItem = editor;
                        break;
                    }
                }
            }
        }

        private void LogLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.Default.LogLevel = LogLevel.SelectedIndex;
        }

        private void LogLevel_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            Settings.Default.LogLevel = LogLevel.SelectedIndex;
        }

        private void RecentFileListLengthBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int l;
            if (int.TryParse(RecentFileListLengthBox.Text, out l))
            {
                RecentFileList.GetSingleton().ChangeListLength(l);
            }
            else
            {
                RecentFileListLengthBox.Text = RecentFileList.GetSingleton().ListLength.ToString();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int processID = Process.GetCurrentProcess().Id;
            string exePath = Process.GetCurrentProcess().MainModule.FileName;
            string cryptoolFolderPath = Path.GetDirectoryName(exePath);
            string updaterPath = Path.Combine(DirectoryHelper.BaseDirectory, "CrypUpdater.exe");

            string parameters = "\"dummy\" " + "\"" + cryptoolFolderPath + "\" " + "\"" + exePath + "\" " + "\"" + processID + "\" -JustRestart";
            Process.Start(updaterPath, parameters);
            App.Current.Shutdown();
        }

        private void ResetCrypTool2Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult result = MessageBox.Show(SettingsTabs.Resources.res.DoYouReallyWantReset,
                    SettingsTabs.Resources.res.ResetToDefaultValues, MessageBoxButton.YesNo);
                
                if (result == MessageBoxResult.No)
                {
                    return;
                }

                //Reset all plugins settings
                PluginBase.Properties.Settings.Default.Reset();
                //Reset WorkspaceManagerModel settings
                WorkspaceManagerModel.Properties.Settings.Default.Reset();
                //reset Crypwin settings
                Settings.Default.Reset();
                //reset Crypcore settings
                Core.Properties.Settings.Default.Reset();

                //restart CT2
                int processID = Process.GetCurrentProcess().Id;
                string exePath = Process.GetCurrentProcess().MainModule.FileName;
                string cryptoolFolderPath = Path.GetDirectoryName(exePath);
                string updaterPath = Path.Combine(DirectoryHelper.BaseDirectory, "CrypUpdater.exe");
                string parameters = "\"dummy\" " + "\"" + cryptoolFolderPath + "\" " + "\"" + exePath + "\" " + "\"" + processID + "\" -JustRestart";
                Process.Start(updaterPath, parameters);
                Application.Current.Shutdown();
            }
            catch (Exception)
            {
                //do nothing 
            }
        }
    }

    [ValueConversion(typeof(bool), typeof(bool))]
    class NegateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
