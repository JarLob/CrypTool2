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
using CrypToolStoreLib.Client;
using CrypToolStoreLib.DataObjects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

namespace CrypToolStoreDeveloperClient.Views
{
    /// <summary>
    /// Interaktionslogik für SourceManagementView.xaml
    /// </summary>
    public partial class SourceManagementView : UserControl
    {
        public MainWindow MainWindow { get; set; }

        private ObservableCollection<Source> Sources = new ObservableCollection<Source>();
        public int PluginId { get; set; }

        public SourceManagementView()
        {
            InitializeComponent();
            SourcesListView.ItemsSource = Sources;
            Sources.Clear();
            IsVisibleChanged += SourceManagementView_IsVisibleChanged;
        }

        /// <summary>
        /// Called, when the UI changes to visible starte
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SourceManagementView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IsVisible)
            {
                return;
            }

            //we fetch the source list in a seperate thread, thus, the ui is not blocked during download of the list
            Thread fetchSourceListThread = new Thread(FetchSourceList);
            fetchSourceListThread.IsBackground = true;
            fetchSourceListThread.Start();
        }

        /// <summary>
        /// Method requests a source list and stores it in the list of the GUI
        /// </summary>
        private void FetchSourceList()
        {
            try
            {
                CrypToolStoreClient client = new CrypToolStoreClient();
                client.ServerAddress = Constants.ServerAddress;
                client.ServerPort = Constants.ServerPort;
                client.Connect();
                client.Login(MainWindow.Username, MainWindow.Password);
                DataModificationOrRequestResult result = client.GetSourceList(PluginId);
                List<Source> sources = (List<Source>)result.DataObject;

                Dispatcher.BeginInvoke(new ThreadStart(() =>
                {
                    try
                    {
                        Sources.Clear();
                        foreach (Source source in sources)
                        {
                            Sources.Add(source);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(String.Format("Exception during adding sources to list: {0}", ex.Message), "Exception");
                    }
                }));
                client.Disconnect();
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Exception during retrieving of list of sources: {0}", ex.Message), "Exception");
            }
        }

        /// <summary>
        /// Deletes the source defined by the clicked button
        /// Then, updates the source list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            int pluginversion = (int)button.CommandParameter;

            MessageBoxResult messageBoxResult = MessageBox.Show(String.Format("Do you really want to delete the source {0}-{1}?", PluginId, pluginversion), String.Format("Delete {0}-{1}", PluginId, pluginversion), MessageBoxButton.YesNo);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                try
                {
                    CrypToolStoreClient client = new CrypToolStoreClient();
                    client.ServerAddress = Constants.ServerAddress;
                    client.ServerPort = Constants.ServerPort;
                    client.Connect();
                    client.Login(MainWindow.Username, MainWindow.Password);
                    DataModificationOrRequestResult result = client.DeleteSource(PluginId, pluginversion);
                    client.Disconnect();

                    if (result.Success)
                    {
                        MessageBox.Show(String.Format("Successfully deleted source {0}-{1}", PluginId, pluginversion), "Source deleted");
                        //we fetch the source list in a seperate thread, thus, the ui is not blocked during download of the list
                        Thread fetchSourceListThread = new Thread(FetchSourceList);
                        fetchSourceListThread.IsBackground = true;
                        fetchSourceListThread.Start();
                    }
                    else
                    {
                        MessageBox.Show(String.Format("Could not delete source: {0}", result.Message), "Deletion not possible");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(String.Format("Exception during deletion of source: {0}", ex.Message), "Exception");
                }
            }
        }

        /// <summary>
        /// Shows a window for updating a source
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Update_Click(object sender, RoutedEventArgs e)
        {
            /*Button button = (Button)sender;
            int id = (int)button.CommandParameter;
            UpdatePluginWindow updatePluginWindow = new UpdatePluginWindow(id);
            updatePluginWindow.MainWindow = MainWindow;
            updatePluginWindow.ShowDialog();
            FetchSourceList();*/
        }

        /// <summary>
        /// Shows a window for creating a new source
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateNewSourceButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult messageBoxResult = MessageBox.Show(String.Format("Do you really want to create a new source for plugin {0}?", PluginId), "Create new source", MessageBoxButton.YesNo);

                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    CrypToolStoreClient client = new CrypToolStoreClient();
                    client.ServerAddress = Constants.ServerAddress;
                    client.ServerPort = Constants.ServerPort;
                    client.Connect();
                    client.Login(MainWindow.Username, MainWindow.Password);

                    //1. compute highest version number for new source
                    DataModificationOrRequestResult result = client.GetSourceList(PluginId);

                    if (result.Success == false)
                    {
                        client.Disconnect();
                        MessageBox.Show(String.Format("Could not get source list for computing new version number: {0}", result.Message), "Creation not possible");                        
                        return;
                    }

                    List<Source> sources = (List<Source>)result.DataObject;

                    int highestVersionNumber = 0;
                    foreach (Source source in sources)
                    {
                        if (source.PluginVersion > highestVersionNumber)
                        {
                            highestVersionNumber = source.PluginVersion;
                        }
                    }

                    //2: create new source
                    Source newsource = new Source();
                    newsource.PluginId = PluginId;
                    newsource.PluginVersion = highestVersionNumber + 1;                    
                    newsource.BuildState = BuildState.UPLOADED.ToString();
                    newsource.BuildLog = String.Format("Created by {0}", MainWindow.Username);

                    result = client.CreateSource(newsource);
                    client.Disconnect();

                    //3. Show result to user
                    if (result.Success == true)
                    {
                        MessageBox.Show(String.Format("Created new source: {0}-{1}", newsource.PluginId, newsource.PluginVersion), "Source created");
                        //we fetch the source list in a seperate thread, thus, the ui is not blocked during download of the list
                        Thread fetchSourceListThread = new Thread(FetchSourceList);
                        fetchSourceListThread.IsBackground = true;
                        fetchSourceListThread.Start();
                    }
                    else
                    {
                        MessageBox.Show(String.Format("Creation not possible: {0}", result.Message), "Source created");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Exception during creation of new source: {0}", ex.Message), "Exception");
            }
        }
    }
}