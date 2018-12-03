/*
   Copyright 2018 Nils Kopal <Nils.Kopal<AT>Uni-Kassel.de>

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
using Cryptool.Core;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Attributes;
using Cryptool.PluginBase.IO;
using Cryptool.PluginBase.Miscellaneous;
using CrypToolStoreLib.Client;
using CrypToolStoreLib.DataObjects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using System.Xml;
using CrypToolStoreLib.Tools;

namespace Cryptool.CrypToolStore
{
    /// <summary>
    /// Interaktionslogik für CrypToolStorePresentation.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("CrypTool.CrypToolStore.Properties.Resources")]
    public partial class CrypToolStorePresentation : UserControl, INotifyPropertyChanged
    {
        //reference totthe editor
        private readonly CrypToolStoreEditor _crypToolStoreEditor;
        //list of all plugins available in the store
        private ObservableCollection<PluginWrapper> Plugins { get; set; }
        //Current selected plugin in the store
        private PluginWrapper SelectedPlugin { get; set; }
        //Pending changes means, something has been installed or uninstalled
        //thus, CrypTool 2 needs to be restarted to have changes take any effect
        public static bool PendingChanges { get; set; }
        //The reference to the window is needed for modal message boxes
        private readonly Window _windowToBlockForMessageBoxes;

        private readonly SynchronizationContext _context;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="crypToolStoreEditor"></param>
        public CrypToolStorePresentation(CrypToolStoreEditor crypToolStoreEditor)
        {
            InitializeComponent();
            _crypToolStoreEditor = crypToolStoreEditor;
            Plugins = new ObservableCollection<PluginWrapper>();
            PluginListView.ItemsSource = Plugins;
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(PluginListView.ItemsSource);
            view.Filter = UserFilter;
            view.SortDescriptions.Add(new SortDescription("UpdateAvailable", ListSortDirection.Descending));
            view.SortDescriptions.Add(new SortDescription("IsInstalled", ListSortDirection.Descending));
            view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            DataContext = this;
            _windowToBlockForMessageBoxes = Application.Current.MainWindow;
            _context = SynchronizationContext.Current;
        }

        /// <summary>
        /// Filters the plugin list
        /// Only shows plugins that have the search text (given by user in search field) in
        /// Name, ShortDescription, LongDescription, Authornames, or Authorinstitutes
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool UserFilter(object item)
        {
            if (String.IsNullOrEmpty(Filter.Text))
            {
                return true;
            }
            else
            {
                PluginWrapper plugin = (PluginWrapper)item;
                string searchtext = plugin.Name +
                                    plugin.ShortDescription +
                                    plugin.LongDescription +
                                    plugin.Authornames +
                                    plugin.Authorinstitutes;
                return (searchtext.IndexOf(Filter.Text, StringComparison.OrdinalIgnoreCase) >= 0);
            }
        }

        /// <summary>
        /// Called, when ui has been loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Task updateStorePluginListTask = new Task(UpdateStorePluginList);
            updateStorePluginListTask.Start();
        }

        /// <summary>
        /// Starts a thread to retrieve the newest list of plugins from the CrypTooLStoreServer
        /// </summary>
        private void UpdateStorePluginList()
        {
            try
            {
                CrypToolStoreClient client = new CrypToolStoreClient();
                client.ServerCertificate = new X509Certificate2(Properties.Resources.anonymous);
                client.ServerAddress = Cryptool.CrypToolStore.Constants.ServerAddress;
                client.ServerPort = Cryptool.CrypToolStore.Constants.ServerPort;

                //Translate the Ct2BuildType to PublishState
                PublishState publishState;
                switch (AssemblyHelper.BuildType)
                {
                    case Ct2BuildType.Developer:
                        publishState = PublishState.DEVELOPER;
                        break;
                    case Ct2BuildType.Nightly:
                        publishState = PublishState.NIGHTLY;
                        break;
                    case Ct2BuildType.Beta:
                        publishState = PublishState.BETA;
                        break;
                    case Ct2BuildType.Stable:
                        publishState = PublishState.RELEASE;
                        break;
                    default: //if no known version is given, we assume release
                        publishState = PublishState.RELEASE;
                        break;
                }

                //Connect to CrypToolStoreServer
                client.Connect();

                //get list of published plugins
                DataModificationOrRequestResult result = client.GetPublishedPluginList(publishState);

                //Disconnect from CrypToolStoreServer
                client.Disconnect();

                //Display result or in case of error an error message
                if (result.Success)
                {
                    List<PluginAndSource> pluginsAndSources = ((List<PluginAndSource>)result.DataObject);

                    //add elements to observed list to show them in the UI
                    _context.Send(callback =>
                    {
                        try
                        {                      
                            Plugins.Clear();
                            foreach (PluginAndSource pluginAndSource in pluginsAndSources)
                            {
                                PluginWrapper wrapper = new PluginWrapper(pluginAndSource);
                                CheckIfAlreadyInstalled(wrapper);                                
                                Plugins.Add(wrapper);
                            }

                            //Search for old selected plugin and select it
                            if (SelectedPlugin != null)
                            {
                                int counter = 0;
                                foreach (PluginWrapper wrapper in PluginListView.Items)
                                {
                                    if (wrapper.PluginId == SelectedPlugin.PluginId)
                                    {
                                        PluginListView.SelectedIndex = counter;
                                        break;
                                    }
                                    counter++;
                                }
                            }
                            else
                            {
                                //otherwise, select the first plugin in list
                                PluginListView.SelectedIndex = 0;
                            }
                        }
                        catch (Exception ex)
                        {
                            _crypToolStoreEditor.GuiLogMessage(String.Format(Properties.Resources.CrypToolStorePresentation_UpdateStorePluginList_Exception_occured_during_adding_of_current_list_of_plugins_to_the_user_interface___0_, ex.Message), NotificationLevel.Error);
                        }
                    }, null);                    
                }
                else
                {
                    _crypToolStoreEditor.GuiLogMessage(String.Format(Properties.Resources.CrypToolStorePresentation_UpdateStorePluginList_Error_occured_during_retrieval_of_current_list_of_plugins_from_CrypToolStore___0_, result.Message), NotificationLevel.Error);
                }
            }
            catch (Exception ex)
            {
                _crypToolStoreEditor.GuiLogMessage(String.Format(Properties.Resources.CrypToolStorePresentation_UpdateStorePluginList_Exception_occured_during_retrieval_of_current_list_of_plugins_from_CrypToolStore___0_, ex.Message), NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Called when text of search field has been changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Filter_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(PluginListView.ItemsSource).Refresh();
        }

        /// <summary>
        /// Selection in the PluginListView changed, i.e., the user selected a plugin
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PluginListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1)
            {
                //the user selected a new plugin in the PluginListView
                PluginWrapper plugin = (PluginWrapper)e.AddedItems[0];
                SelectedPlugin = plugin;

                //Show selected plugin in the right box of the CrypToolStore UI
                _context.Send(callback =>
                {
                    try
                    {
                        SelectedPluginName.Content = plugin.Name;
                        SelectedPluginShortDescription.Text = plugin.ShortDescription;
                        SelectedPluginLongDescription.Text = plugin.LongDescription;
                        SelectedPluginIcon.Source = plugin.Icon;
                        SelectedPluginAuthorsName.Content = plugin.Authornames;
                        SelectedPluginAuthorsEmail.Content = plugin.Authoremails;
                        SelectedPluginAuthorsInstitutes.Content = plugin.Authorinstitutes;
                        SelectedPluginVersion.Content = plugin.PluginVersion + "." + plugin.BuildVersion;
                        if (SelectedPlugin.IsInstalled)
                        {
                            InstallButton.IsEnabled = false;
                            DeleteButton.IsEnabled = true;
                        }
                        else
                        {
                            InstallButton.IsEnabled = true;
                            DeleteButton.IsEnabled = false;
                        }
                        if (SelectedPlugin.UpdateAvailable)
                        {
                            UpdateButton.IsEnabled = true;
                        }
                        else
                        {
                            UpdateButton.IsEnabled = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        _crypToolStoreEditor.GuiLogMessage(String.Format(Properties.Resources.CrypToolStorePresentation_PluginListView_SelectionChanged_Exception_occured_during_showing_of_selected_plugin_in_the_right_box_of_the_CrypToolStore_UI___0_, ex.Message), NotificationLevel.Error);
                    }
                }, null);
            }
        }

        /// <summary>
        /// Checks, if a plugin is already installed
        /// </summary>
        /// <param name="plugin"></param>
        private void CheckIfAlreadyInstalled(PluginWrapper plugin)
        {
            string xmlfilename = System.IO.Path.Combine(GetPluginsFolder(plugin), String.Format("install-{0}-{1}.xml", plugin.PluginId, plugin.PluginVersion));
            if (Directory.Exists(GetPluginFolder(plugin)) || File.Exists(xmlfilename))
            {
                try
                {
                    string metaxmlfilename = System.IO.Path.Combine(GetPluginFolder(plugin), "pluginmetainfo.xml");
                    XmlDocument xml = new XmlDocument();
                    xml.Load(metaxmlfilename);

                    XmlNode rootNode = xml.SelectSingleNode("xml");
                    XmlNode pluginNode = rootNode.SelectSingleNode("Plugin");
                    XmlNode versionNode = pluginNode.SelectSingleNode("Version");
                    XmlNode buildVersionNode = pluginNode.SelectSingleNode("BuildVersion");

                    int pluginversion = Int32.Parse(versionNode.InnerText);
                    int buildversion = Int32.Parse(buildVersionNode.InnerText);

                    if (plugin.PluginVersion != pluginversion || plugin.BuildVersion != buildversion)
                    {
                        plugin.UpdateAvailable = true;
                    }
                }
                catch (Exception ex)
                {
                    _crypToolStoreEditor.GuiLogMessage(String.Format(Properties.Resources.CrypToolStorePresentation_CheckIfAlreadyInstalled_Exception_occured_during_check_of__0__for_updates___1_,plugin.Name,ex.Message),NotificationLevel.Error);
                }
                plugin.IsInstalled = true;
            }
            else
            {
                plugin.IsInstalled = false;
            }
        }

        /// <summary>
        /// Returns the absolute path to the plugin folder
        /// </summary>
        /// <param name="plugin"></param>
        /// <returns></returns>
        private string GetPluginFolder(PluginWrapper plugin)
        {           
            return System.IO.Path.Combine(GetPluginsFolder(plugin), "plugin-" + plugin.PluginId);
        }

        /// <summary>
        /// Returns the absolute path to the plugins folder
        /// </summary>
        /// <param name="plugin"></param>
        /// <returns></returns>
        private string GetPluginsFolder(PluginWrapper plugin)
        {
            //Translate the Ct2BuildType to a folder name for CrypToolStore plugins                
            string crypToolStoreSubFolder = "";
            switch (AssemblyHelper.BuildType)
            {
                case Ct2BuildType.Developer:
                    crypToolStoreSubFolder = "Developer";
                    break;
                case Ct2BuildType.Nightly:
                    crypToolStoreSubFolder = "Nightly";
                    break;
                case Ct2BuildType.Beta:
                    crypToolStoreSubFolder = "Beta";
                    break;
                case Ct2BuildType.Stable:
                    crypToolStoreSubFolder = "Release";
                    break;
                default: //if no known version is given, we assume developer
                    crypToolStoreSubFolder = "Developer";
                    break;
            }
            string crypToolStorePluginFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), PluginManager.CrypToolStorePluginDirectory);
            crypToolStorePluginFolder = System.IO.Path.Combine(crypToolStorePluginFolder, crypToolStoreSubFolder);
            crypToolStorePluginFolder = System.IO.Path.Combine(crypToolStorePluginFolder, "plugins");
            return crypToolStorePluginFolder;
        }

        /// <summary>
        /// User clicked InstallButton
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedPlugin == null)
            {
                return;
            }
            MessageBoxResult result = MessageBox.Show(_windowToBlockForMessageBoxes, String.Format(Properties.Resources.CrypToolStorePresentation_InstallButton_Click_Do_you_really_want_to_download_and_install___0___from_CrypTool_Store_, SelectedPlugin.Name), String.Format(Properties.Resources.CrypToolStorePresentation_InstallButton_Click_Start_download_and_installation_of___0___, SelectedPlugin.Name), MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                
                Task.Run(() => InstallPlugin());
            }
        }     

        /// <summary>
        /// Installs the plugin in an own thread
        /// </summary>
        private void InstallPlugin()
        {
            bool errorOccured = false;
            string assemblyfilename = System.IO.Path.Combine(GetPluginsFolder(SelectedPlugin), String.Format("assembly-{0}-{1}.zip", SelectedPlugin.PluginId, SelectedPlugin.PluginVersion));
            string xmlfilename = System.IO.Path.Combine(GetPluginsFolder(SelectedPlugin), String.Format("install-{0}-{1}.xml", SelectedPlugin.PluginId, SelectedPlugin.PluginVersion));

            //Step 0: delete files before download
            try
            {
                if (File.Exists(assemblyfilename))
                {
                    File.Delete(assemblyfilename);
                }
                if (File.Exists(assemblyfilename + ".part"))
                {
                    File.Delete(assemblyfilename + ".part");
                }
                if (File.Exists(xmlfilename))
                {
                    File.Delete(xmlfilename);
                }
            }
            catch (Exception ex)
            {
                _crypToolStoreEditor.GuiLogMessage(String.Format(Properties.Resources.CrypToolStorePresentation_InstallPlugin_Exception_occured_while_deleting_old_installation_files___0_, ex.Message), NotificationLevel.Error);
                return;
            }

            //Step 1: Lock everything in the UI, thus, the user can not do anything while downloading
            _context.Send(callback =>
            {
                try
                {
                    PluginListView.IsEnabled = false;
                    InstallButton.IsEnabled = false;
                    DeleteButton.IsEnabled = false;
                    Filter.IsEnabled = false;

                    InstallationProgressBar.Visibility = Visibility.Visible;
                    InstallationProgressText.Visibility = Visibility.Visible;
                }
                catch (Exception ex)
                {
                    _crypToolStoreEditor.GuiLogMessage(String.Format(Properties.Resources.CrypToolStorePresentation_InstallPlugin_Exception_occured_while_locking_of_everything___0_, ex.Message), NotificationLevel.Error);                    
                }
            }, null);

            //Step 2: download assembly
            try
            {               
                //Download assembly zip
                CrypToolStoreClient client = new CrypToolStoreClient();
                client.ServerCertificate = new X509Certificate2(Properties.Resources.anonymous);
                client.ServerAddress = Cryptool.CrypToolStore.Constants.ServerAddress;
                client.ServerPort = Cryptool.CrypToolStore.Constants.ServerPort;
                client.UploadDownloadProgressChanged +=client_UploadDownloadProgressChanged;

                //Translate the Ct2BuildType to PublishState
                PublishState publishState;
                switch (AssemblyHelper.BuildType)
                {
                    case Ct2BuildType.Developer:
                        publishState = PublishState.DEVELOPER;
                        break;
                    case Ct2BuildType.Nightly:
                        publishState = PublishState.NIGHTLY;
                        break;
                    case Ct2BuildType.Beta:
                        publishState = PublishState.BETA;
                        break;
                    case Ct2BuildType.Stable:
                        publishState = PublishState.RELEASE;
                        break;
                    default: //if no known version is given, we assume release
                        publishState = PublishState.RELEASE;
                        break;
                }

                //Connect to CrypToolStoreServer
                client.Connect();
                
                //get list of published plugins
                DataModificationOrRequestResult result = client.GetPublishedPlugin(SelectedPlugin.PluginId,publishState);
                if(result.Success == false)
                {
                    client.Disconnect();
                    string message = String.Format(Properties.Resources.CrypToolStorePresentation_InstallPlugin_Could_not_download_from_CrypToolStore_Server__Message_was___0_, result.Message);
                    _crypToolStoreEditor.GuiLogMessage(message, NotificationLevel.Error);
                    _context.Send(callback => MessageBox.Show(_windowToBlockForMessageBoxes, message, Properties.Resources.CrypToolStorePresentation_InstallPlugin_Error_during_download_, MessageBoxButton.OK), null);
                }

                PluginAndSource pluginAndSource = (PluginAndSource)result.DataObject;                
                bool stop = false;
                result = client.DownloadAssemblyZipFile(pluginAndSource.Source, assemblyfilename, ref stop);
                client.Disconnect();
                if (result.Success == false)
                {
                    client.Disconnect();
                    string message = String.Format(Properties.Resources.CrypToolStorePresentation_InstallPlugin_Could_not_download_from_CrypToolStore_Server__Message_was___0_, result.Message);
                    _crypToolStoreEditor.GuiLogMessage(message, NotificationLevel.Error);
                    _context.Send(callback => MessageBox.Show(_windowToBlockForMessageBoxes, message, Properties.Resources.CrypToolStorePresentation_InstallPlugin_Error_during_download_, MessageBoxButton.OK), null);
                    errorOccured = true;
                    return;
                }
                
                //Step 3: Create installation xml file                
                using (StreamWriter xmlfile = new StreamWriter(xmlfilename))
                {
                    string type = "installation";
                    xmlfile.WriteLine(String.Format("<installation type=\"{0}\">",type));
                    xmlfile.WriteLine("  <plugin>");
                    xmlfile.WriteLine(String.Format("    <name>{0}</name>", SelectedPlugin.Name));
                    xmlfile.WriteLine(String.Format("    <id>{0}</id>", SelectedPlugin.PluginId));
                    xmlfile.WriteLine(String.Format("    <version>{0}</version>",SelectedPlugin.PluginVersion));
                    xmlfile.WriteLine("  </plugin>");
                    xmlfile.WriteLine("</installation>");
                }

                //Step 4: Notify user
                _context.Send(callback => MessageBox.Show(_windowToBlockForMessageBoxes, String.Format(Properties.Resources.CrypToolStorePresentation_InstallPlugin___0___has_been_successfully_downloaded__You_need_to_restart_CrypTool_2_to_complete_installation_, SelectedPlugin.Name), Properties.Resources.CrypToolStorePresentation_InstallPlugin_Download_succeeded_, MessageBoxButton.OK), null);
                PendingChanges = true;                
                OnStaticPropertyChanged("PendingChanges");                
            }
            catch (Exception ex)
            {                
                string message = String.Format(Properties.Resources.CrypToolStorePresentation_InstallPlugin_Exception_occured_while_downloading_and_installing___0_, ex.Message);
                _crypToolStoreEditor.GuiLogMessage(message, NotificationLevel.Error);
                _context.Send(callback => MessageBox.Show(_windowToBlockForMessageBoxes, message, Properties.Resources.CrypToolStorePresentation_InstallPlugin_Exception_occured_, MessageBoxButton.OK), null);
                errorOccured = true;
            }
            finally
            {
                try
                {
                    //if something went wrong, we delete the zip and xml files
                    if (errorOccured)
                    {
                        if (File.Exists(assemblyfilename))
                        {
                            File.Delete(assemblyfilename);
                        }
                        if (File.Exists(assemblyfilename + ".part"))
                        {
                            File.Delete(assemblyfilename + ".part");
                        }
                        if (File.Exists(xmlfilename))
                        {
                            File.Delete(xmlfilename);
                        }
                    }
                }
                catch (Exception)
                {
                    //wtf?
                }

                //Step 5: Unlock everything in the UI
                _context.Send(callback =>
                {
                    try
                    {
                        PluginListView.IsEnabled = true;
                        InstallButton.IsEnabled = true;
                        DeleteButton.IsEnabled = true;
                        Filter.IsEnabled = true;

                        InstallationProgressBar.Visibility = Visibility.Collapsed;
                        InstallationProgressText.Visibility = Visibility.Collapsed;
                    }
                    catch (Exception ex)
                    {
                        _crypToolStoreEditor.GuiLogMessage(String.Format(Properties.Resources.CrypToolStorePresentation_InstallPlugin_Exception_occured_while_unlocking_of_everything___0_, ex.Message), NotificationLevel.Error);
                    }
                }, null);

                //Step 6: Update StorePluginListTask
                Task updateStorePluginListTask = new Task(UpdateStorePluginList);
                updateStorePluginListTask.Start();
            }
        }

        /// <summary>
        /// Updates the download progress bar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void client_UploadDownloadProgressChanged(object sender, UploadDownloadProgressEventArgs e)
        {
            if (e.FileSize <= 0)
            {
                return;
            }
            _context.Send(callback =>
            {
                InstallationProgressBar.Maximum = e.FileSize;
                InstallationProgressBar.Value = e.DownloadedUploaded;                
                double progress = e.DownloadedUploaded / (double)e.FileSize * 100;
                InstallationProgressText.Content = Math.Round(progress, 2) + " % (" + Tools.FormatSpeedString(e.BytePerSecond) + " - " + Tools.RemainingTime(e.BytePerSecond, e.FileSize, e.DownloadedUploaded) + ")";
            }, null);
        }

        /// <summary>
        /// Deletes the selected plugin
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            
            if (SelectedPlugin == null)
            {
                return;
            }
            try
            {
                MessageBoxResult result = MessageBox.Show(_windowToBlockForMessageBoxes,String.Format(Properties.Resources.CrypToolStorePresentation_DeleteButton_Click_Do_you_really_want_to_uninstall___0___from_CrypTool_Store_, SelectedPlugin.Name), Properties.Resources.CrypToolStorePresentation_DeleteButton_Click_Unistallation_, MessageBoxButton.YesNo);                
                if (result == MessageBoxResult.Yes)
                {
                    string assemblyfilename = System.IO.Path.Combine(GetPluginsFolder(SelectedPlugin), String.Format("assembly-{0}-{1}.zip", SelectedPlugin.PluginId, SelectedPlugin.PluginVersion));
                    string xmlfilename = System.IO.Path.Combine(GetPluginsFolder(SelectedPlugin), String.Format("install-{0}-{1}.xml", SelectedPlugin.PluginId, SelectedPlugin.PluginVersion));

                    //Step 0: delete files before creating new xml file
                    try
                    {
                        if (File.Exists(assemblyfilename))
                        {
                            File.Delete(assemblyfilename);
                        }
                        if (File.Exists(xmlfilename))
                        {
                            File.Delete(xmlfilename);
                        }
                    }
                    catch (Exception ex)
                    {
                        _crypToolStoreEditor.GuiLogMessage(String.Format(Properties.Resources.CrypToolStorePresentation_InstallPlugin_Exception_occured_while_deleting_old_installation_files___0_, ex.Message), NotificationLevel.Error);
                        return;
                    }

                    using (StreamWriter xmlfile = new StreamWriter(xmlfilename))
                    {
                        string type = "deletion";
                        xmlfile.WriteLine(String.Format("<installation type=\"{0}\">", type));
                        xmlfile.WriteLine("  <plugin>");
                        xmlfile.WriteLine(String.Format("    <name>{0}</name>", SelectedPlugin.Name));
                        xmlfile.WriteLine(String.Format("    <id>{0}</id>", SelectedPlugin.PluginId));
                        xmlfile.WriteLine(String.Format("    <version>{0}</version>", SelectedPlugin.PluginVersion));
                        xmlfile.WriteLine("  </plugin>");
                        xmlfile.WriteLine("</installation>");
                    }

                    MessageBox.Show(_windowToBlockForMessageBoxes,String.Format(Properties.Resources.CrypToolStorePresentation_DeleteButton_Click___0___has_been_marked_for_uninstallation__You_need_to_restart_CrypTool_2_to_complete_installation_, SelectedPlugin.Name), "Marked for uninstallation.", MessageBoxButton.OK);

                    PendingChanges = true;
                    OnStaticPropertyChanged("PendingChanges");

                    //update StorePluginList
                    Task updateStorePluginListTask = new Task(UpdateStorePluginList);
                    updateStorePluginListTask.Start();
                }
            }
            catch (Exception ex)
            {
                string message = String.Format(Properties.Resources.CrypToolStorePresentation_DeleteButton_Click_Could_not_uninstall__Exception_was___0_, ex.Message);
                _crypToolStoreEditor.GuiLogMessage(message, NotificationLevel.Error);
                MessageBox.Show(_windowToBlockForMessageBoxes,message, Properties.Resources.CrypToolStorePresentation_DeleteButton_Click_Exception_during_uninstallation_, MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// User pressed RestartButton
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int processID = Process.GetCurrentProcess().Id;
                string exePath = Process.GetCurrentProcess().MainModule.FileName;
                string cryptoolFolderPath = System.IO.Path.GetDirectoryName(exePath);
                string updaterPath = System.IO.Path.Combine(DirectoryHelper.BaseDirectory, "CrypUpdater.exe");
                string parameters = "\"dummy\" " + "\"" + cryptoolFolderPath + "\" " + "\"" + exePath + "\" " + "\"" + processID + "\" -JustRestart";
                Process.Start(updaterPath, parameters);
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                string message = String.Format(Properties.Resources.CrypToolStorePresentation_RestartButton_Click_Exception_occured_while_trying_to_restart_CrypTool_2___0_, ex.Message);
                _crypToolStoreEditor.GuiLogMessage(message, NotificationLevel.Error);
                MessageBox.Show(_windowToBlockForMessageBoxes,message, Properties.Resources.CrypToolStorePresentation_RestartButton_Click_Exception_during_restart_, MessageBoxButton.OK);
            }
        }      

        /// <summary>
        /// Static property changed
        /// </summary>
        /// <param name="name"></param>
        private void OnStaticPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(StaticPropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public static event PropertyChangedEventHandler StaticPropertyChanged;       
    }

    /// <summary>
    /// Converts a boolean to Visibility (inverse)
    /// false => Visible
    /// true => Collapsed
    /// </summary>
    public class InverseBooleanToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Boolean && (bool)value)
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Visibility && (Visibility)value == Visibility.Visible)
            {
                return false;
            }
            return true;
        }
    }
}
