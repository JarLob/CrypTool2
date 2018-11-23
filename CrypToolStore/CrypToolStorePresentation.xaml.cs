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
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Cryptool.CrypToolStore
{
    /// <summary>
    /// Interaktionslogik für CrypToolStorePresentation.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("CrypTool.CrypToolStore.Properties.Resources")]
    public partial class CrypToolStorePresentation : UserControl, INotifyPropertyChanged
    {
        private CrypToolStoreEditor CrypToolStoreEditor;
        private ObservableCollection<PluginWrapper> Plugins { get; set; }
        private PluginWrapper SelectedPlugin { get; set; }

        //Pending changes means, something has been installed or uninstalled
        //thus, CrypTool 2 needs to be restarted to have changes take any effect
        public static bool PendingChanges { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="crypToolStoreEditor"></param>
        public CrypToolStorePresentation(CrypToolStoreEditor crypToolStoreEditor)
        {
            InitializeComponent();
            CrypToolStoreEditor = crypToolStoreEditor;
            Plugins = new ObservableCollection<PluginWrapper>();
            PluginListView.ItemsSource = Plugins;
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(PluginListView.ItemsSource);
            view.Filter = UserFilter;
            view.SortDescriptions.Add(new SortDescription("IsInstalled", ListSortDirection.Descending));
            view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            DataContext = this;
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
                client.ServerAddress = "localhost";                

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
                    Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
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
                            CrypToolStoreEditor.GuiLogMessage(String.Format("Exception occured during adding of current list of plugins to the user interface: {0}", ex.Message), NotificationLevel.Error);
                        }
                    }, null);                    
                }
                else
                {
                    CrypToolStoreEditor.GuiLogMessage(String.Format("Error occured during retrieval of current list of plugins from CrypToolStore: {0}", result.Message), NotificationLevel.Error);
                }
            }
            catch (Exception ex)
            {
                CrypToolStoreEditor.GuiLogMessage(String.Format("Exception occured during retrieval of current list of plugins from CrypToolStore: {0}", ex.Message), NotificationLevel.Error);
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
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
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
                    }
                    catch (Exception ex)
                    {
                        CrypToolStoreEditor.GuiLogMessage(String.Format("Exception occured during showing of selected plugin in the right box of the CrypToolStore UI: {0}", ex.Message), NotificationLevel.Error);
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
            if (Directory.Exists(GetPluginFolder(plugin)))
            {
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
            crypToolStorePluginFolder = System.IO.Path.Combine(crypToolStorePluginFolder, "plugin-" + plugin.PluginId);
            return crypToolStorePluginFolder;
        }

        /// <summary>
        /// User clicked Install-Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedPlugin == null)
            {
                return;
            }
            MessageBoxResult result = MessageBox.Show(String.Format("Do you really want to download and install \"{0}\" from CrypTool Store?", SelectedPlugin.Name), String.Format("Start download and installation of \"{0}\"?", SelectedPlugin.Name), MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                Task InstallTask = new Task(InstallPlugin);
                InstallTask.Start();
            }
        }

        /// <summary>
        /// Installs the plugin in an own thread
        /// </summary>
        private void InstallPlugin()
        {
            bool errorOccured = false;

            //Step 1: Lock everything in the UI, thus, the user can not do anything while downloading
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    PluginListView.IsEnabled = false;
                    InstallButton.IsEnabled = false;
                    DeleteButton.IsEnabled = false;
                    Filter.IsEnabled = false;
                }
                catch (Exception ex)
                {
                    CrypToolStoreEditor.GuiLogMessage(String.Format("Exception occured while locking of everything: {0}", ex.Message), NotificationLevel.Error);
                }
            }, null);

            //Step 2: download component
            try
            {
                //Step 2.1:
                //Create folder
                string folder = GetPluginFolder(SelectedPlugin);
                if(Directory.Exists(folder))
                {
                    Directory.Delete(folder, true);
                }
                Directory.CreateDirectory(folder);
                                
                //Step 2.2
                //Download component zip

                CrypToolStoreClient client = new CrypToolStoreClient();
                client.ServerCertificate = new X509Certificate2(Properties.Resources.anonymous);
                client.ServerAddress = "localhost";

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
                    string message = String.Format("Could not download from CrypToolStore Server. Message was: {0}", result.Message);
                    CrypToolStoreEditor.GuiLogMessage(message, NotificationLevel.Error);
                    MessageBox.Show(message, "Error during download.", MessageBoxButton.OK);
                    errorOccured = true;
                    return;
                }

                PluginAndSource pluginAndSource = (PluginAndSource)result.DataObject;
                string filename = System.IO.Path.Combine(GetPluginFolder(SelectedPlugin),"plugin.zip");
                bool stop = false;
                result = client.DownloadAssemblyZipFile(pluginAndSource.Source, filename, ref stop);
                client.Disconnect();
                if (result.Success == false)
                {
                    client.Disconnect();
                    string message = String.Format("Could not download from CrypToolStore Server. Message was: {0}", result.Message);
                    CrypToolStoreEditor.GuiLogMessage(message, NotificationLevel.Error);
                    MessageBox.Show(message, "Error during download.", MessageBoxButton.OK);
                    errorOccured = true;
                    return;
                }

                //Step 2.3 unzip component zip
                ZipFile.ExtractToDirectory(filename, GetPluginFolder(SelectedPlugin));

                //Step 2.4 delete zip file
                File.Delete(filename);

                MessageBox.Show(String.Format("\"{0}\" has been successfully downloaded and installed.", SelectedPlugin.Name), "Installation succeeded.", MessageBoxButton.OK);

                PendingChanges = true;
                OnStaticPropertyChanged("PendingChanges");                
            }
            catch (Exception ex)
            {                
                string message = String.Format("Exception occured while downloading and installing: {0}", ex.Message);
                CrypToolStoreEditor.GuiLogMessage(message, NotificationLevel.Error);
                MessageBox.Show(message,"Exception occured.",MessageBoxButton.OK);
                errorOccured = true;
            }
            finally
            {
                try
                {
                    //if something went wrong, we delete the folder, if it exists
                    if (errorOccured)
                    {
                        string folder = GetPluginFolder(SelectedPlugin);
                        if (Directory.Exists(folder))
                        {
                            Directory.Delete(folder, true);
                        }
                    }
                }
                catch (Exception)
                {
                    //wtf?
                }

                //Step 3: Unlock everything in the UI
                Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    try
                    {
                        PluginListView.IsEnabled = true;
                        InstallButton.IsEnabled = true;
                        DeleteButton.IsEnabled = true;
                        Filter.IsEnabled = true;
                    }
                    catch (Exception ex)
                    {
                        CrypToolStoreEditor.GuiLogMessage(String.Format("Exception occured while unlocking of everything: {0}", ex.Message), NotificationLevel.Error);
                    }
                }, null);

                //Step 4: Update StorePluginListTask
                Task updateStorePluginListTask = new Task(UpdateStorePluginList);
                updateStorePluginListTask.Start();
            }
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
            MessageBoxResult result = MessageBox.Show(String.Format("Do you really want to uninstall \"{0}\" from CrypTool Store?", SelectedPlugin.Name), String.Format("Start download and installation of \"{0}\"?", SelectedPlugin.Name), MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                //uninstallation/deletion is a simple delete of the folder of the plugin
                string folder = GetPluginFolder(SelectedPlugin);
                if (Directory.Exists(folder))
                {
                    Directory.Delete(folder, true);
                }
                
                MessageBox.Show(String.Format("\"{0}\" has been successfully uninstalled.", SelectedPlugin.Name), "Uninstallation succeeded.", MessageBoxButton.OK);

                PendingChanges = true;
                OnStaticPropertyChanged("PendingChanges");

                //update StorePluginList
                Task updateStorePluginListTask = new Task(UpdateStorePluginList);
                updateStorePluginListTask.Start();
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
                string message = String.Format("Exception occured while trying to restart CrypTool 2: {0}", ex.Message);
                CrypToolStoreEditor.GuiLogMessage(message, NotificationLevel.Error);
                MessageBox.Show(message, "Exception during restart.", MessageBoxButton.OK);
            }
        }

        /// <summary>
        /// Property changed
        /// </summary>
        /// <param name="name"></param>
        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
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
