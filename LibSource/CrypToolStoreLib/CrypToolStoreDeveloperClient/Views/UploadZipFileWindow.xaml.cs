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
using CrypToolStoreLib.Server;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
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
using System.Windows.Shapes;

namespace CrypToolStoreDeveloperClient.Views
{
    /// <summary>
    /// Interaktionslogik für UploadZipFileWindow.xaml
    /// </summary>
    public partial class UploadZipFileWindow : Window
    {
        public MainWindow MainWindow { get; set; }
        private int PluginId { get; set; }
        private int PluginVersion {get;set;}
        private string FileName { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public UploadZipFileWindow(int pluginid, int pluginversion)
        {
            InitializeComponent();
            ResizeMode = ResizeMode.NoResize;
            PluginId = pluginid;
            PluginVersion = pluginversion;
        }

        /// <summary>
        /// Tries to upload a new source
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            //we fetch the source list in a seperate thread, thus, the ui is not blocked during download of the list
            Thread uploadZipFileThread = new Thread(UploadZipfile);
            uploadZipFileThread.IsBackground = true;
            uploadZipFileThread.Start();
            UploadButton.IsEnabled = false;
        }    

        /// <summary>
        /// 
        /// </summary>
        private void UploadZipfile()
        {
            try
            {
                CrypToolStoreClient client = new CrypToolStoreClient();
                client.ServerAddress = Constants.ServerAddress;
                client.ServerPort = Constants.ServerPort;
                client.Connect();
                client.Login(MainWindow.Username, MainWindow.Password);

                Source source = new Source();
                source.PluginId = PluginId;
                source.PluginVersion = PluginVersion;
                source.UploadDate = DateTime.Now;
                source.BuildState = BuildState.UPLOADED.ToString();
                source.BuildLog = String.Format("Uploaded by {0}",MainWindow.Username);

                client.UploadDownloadProgressChanged += client_UploadDownloadProgressChanged;                
                DataModificationOrRequestResult result = client.UploadZipFile(source, FileName);
                
                client.Disconnect();
                
                if (result.Success)
                {
                    MessageBox.Show("Successfully uploaded zip file", "Zipfile uploaded");                    
                    Dispatcher.BeginInvoke(new ThreadStart(() =>
                    {
                        try
                        {
                            Close();
                        }
                        catch (Exception ex)
                        {
                            //wtf?
                        }
                    }));
                }
                else
                {
                    MessageBox.Show(String.Format("Could not upload zip file: {0}", result.Message), "Zipfile upload not possible");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Exception during upload of zip file: {0}", ex.Message), "Exception");
            }
            
            Dispatcher.BeginInvoke(new ThreadStart(() =>
            {
                try
                {
                    UploadButton.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    //wtf?
                }
            }));
        }

        /// <summary>
        /// Updates progress bar every second
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void client_UploadDownloadProgressChanged(object sender, UploadDownloadProgressEventArgs e)
        {
            Dispatcher.Invoke(new ThreadStart(() =>
            {
                try
                {
                    ProgressBar.Maximum = e.FileSize;
                    ProgressBar.Value = e.DownloadedUploaded;
                    double progress = e.DownloadedUploaded / (double)e.FileSize * 100;

                    ProgressText.Text = Math.Round(progress, 2) + " % (" + e.BytePerSecond + " byte/sec)";
                }
                catch (Exception ex)
                {
                   //wtf?
                }
            }));
        }

        /// <summary>
        /// Shows an open file dialog to select a zip file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectZipFileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Select Icon for the Plugin";
                openFileDialog.Filter = "(*.gzip)|*.gzip";
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                bool? dialogResult = openFileDialog.ShowDialog();
                if (dialogResult == true)
                {
                    FileName = openFileDialog.FileName;                 
                }
                Dispatcher.BeginInvoke(new ThreadStart(() =>
                {
                    try
                    {
                        ZipFileName.Text = FileName;
                    }
                    catch (Exception ex)
                    {
                        //wtf?
                    }
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Exception during selecting of zip file: {0}", ex.Message), "Exception");
            }
        }
    }
}
