﻿/*
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
    /// Interaktionslogik für UserManagementView.xaml
    /// </summary>
    public partial class UserManagementView : UserControl
    {        
        public MainWindow MainWindow { get; set; }

        private ObservableCollection<Developer> Developers = new ObservableCollection<Developer>();

        public UserManagementView()
        {
            InitializeComponent();            
            DevelopersListView.ItemsSource = Developers;
            Developers.Clear();
            IsVisibleChanged += UserManagementView_IsVisibleChanged;
        }

        /// <summary>
        /// Called, when the UI changes to visible starte
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserManagementView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IsVisible)
            {
                return;
            }

            //we fetch the user list in a seperate thread, thus, the ui is not blocked during download of the list67
            Thread fetchUserListThread = new Thread(FetchUserList);
            fetchUserListThread.IsBackground = true;
            fetchUserListThread.Start();
            
        }

        /// <summary>
        /// Method requests a user list and stores it in the list of the GUI
        /// </summary>
        private void FetchUserList()
        {
            try
            {
                CrypToolStoreClient client = new CrypToolStoreClient();                
                client.ServerAddress = Constants.ServerAddress;
                client.ServerPort = Constants.ServerPort;
                client.Connect();
                client.Login(MainWindow.Username, MainWindow.Password);
                DataModificationOrRequestResult result = client.GetDeveloperList();
                List<Developer> developers = (List<Developer>)result.DataObject;

                Dispatcher.BeginInvoke(new ThreadStart(() =>
                {
                    try
                    {
                        Developers.Clear();
                        foreach (Developer developer in developers)
                        {
                            Developers.Add(developer);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(String.Format("Exception during adding developers to list: {0}", ex.Message), "Exception");
                    }
                }));
                client.Disconnect();
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Exception during retrieving of list of developers: {0}", ex.Message), "Exception");
            }         
        }

        /// <summary>
        /// Deletes the user defined by the clicked button
        /// Then, updates the user list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string username = (string)button.CommandParameter;

            MessageBoxResult messageBoxResult = MessageBox.Show(String.Format("Do you really want to delete the developer {0}?", username), String.Format("Delete {0}", username), MessageBoxButton.YesNo);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                try
                {
                    CrypToolStoreClient client = new CrypToolStoreClient();
                    client.ServerAddress = Constants.ServerAddress;
                    client.ServerPort = Constants.ServerPort;
                    client.Connect();
                    client.Login(MainWindow.Username, MainWindow.Password);
                    DataModificationOrRequestResult result = client.DeleteDeveloper(username);
                    client.Disconnect();

                    if (result.Success)
                    {
                        MessageBox.Show(String.Format("Successfully deleted {0}", username), "Developer deleted");
                        FetchUserList();
                    }
                    else
                    {
                        MessageBox.Show(String.Format("Could not delete developer: {0}", result.Message), "Deletion not possible");
                    }                                        
                }
                catch (Exception ex)
                {
                    MessageBox.Show(String.Format("Exception during deletion of developer: {0}", ex.Message), "Exception");
                }         
            }
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string username = (string)button.CommandParameter;
            UpdateUserWindow updateUserWindow = new UpdateUserWindow(username);
            updateUserWindow.MainWindow = MainWindow;
            updateUserWindow.ShowDialog();
            FetchUserList();
        }

        /// <summary>
        /// Shows a window to create a new user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateNewUserButton_Click(object sender, RoutedEventArgs e)
        {
            CreateNewUserWindow createNewUserWindow = new CreateNewUserWindow();
            createNewUserWindow.MainWindow = MainWindow;
            createNewUserWindow.ShowDialog();
            FetchUserList();
        }

    }
}