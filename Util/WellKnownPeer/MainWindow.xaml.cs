﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Collections.ObjectModel;
using VoluntLib2.ConnectionLayer;
using System.Net;
using System.Threading;
using System.Collections;
using VoluntLib2;
using System.Security.Cryptography.X509Certificates;
using VoluntLib2.Tools;

namespace WellKnownPeer
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private VoluntLib _voluntLib = new VoluntLib();

        private ObservableCollection<LogEntry> _Logs = new ObservableCollection<LogEntry>();

        public MainWindow()
        {
            InitializeComponent();            
            Closing += MainWindow_Closing;

            try
            {
                Logger.SetLogLevel(Logtype.Info);
                Logger.GetLogger().LoggOccured += MainWindow_LoggOccured;                

                X509Certificate2 rootCA = new X509Certificate2(Properties.Settings.Default.RootCertificate);
                X509Certificate2 ownKey = new X509Certificate2(Properties.Settings.Default.OwnKey, Properties.Settings.Default.OwnPassword);
                
                _voluntLib = new VoluntLib() { LocalStoragePath = "Jobs" };

                var wellKnownPeers = Properties.Settings.Default.WellKnownPeers.Split(';');
                if (wellKnownPeers.Length != 0)
                {
                    _voluntLib.WellKnownPeers.AddRange(wellKnownPeers);
                }

                var administrators = Properties.Settings.Default.Administrators.Split(';');
                if (administrators.Length != 0)
                {
                    CertificateService.GetCertificateService().AdminCertificateList.AddRange(administrators);

                }

                var bannedCertificates = Properties.Settings.Default.BannedCertificates.Split(';');
                if (bannedCertificates.Length != 0)
                {
                    CertificateService.GetCertificateService().BannedCertificateList.AddRange(bannedCertificates);

                }

                LogListView.DataContext = _Logs;
                _voluntLib.Start(rootCA, ownKey);

                ContactListView.DataContext = _voluntLib.GetContactList();
                JobList.DataContext = _voluntLib.GetJoblist();

                byte[] peerId = _voluntLib.GetPeerId();
                string id = BitConverter.ToString(peerId);
                if (Application.Current != null)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        try
                        {
                            MyPeerID.Content = "My Peer ID: " + id;
                        }
                        catch (Exception)
                        {
                            //wtf?
                        }
                    }));
                }
                else
                {
                    MyPeerID.Content = "My Peer ID: " + id;
                }            
                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception during startup of WellKnownPeer: " + ex.Message, "Exception during startup!", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Environment.Exit(-1);
            }

        }

        void MainWindow_LoggOccured(object sender, LogEventArgs logEventArgs)
        {
            try
            {
                LogEntry entry = new LogEntry();
                entry.LogTime = DateTime.Now.ToString();
                entry.LogType = logEventArgs.Logtype.ToString();
                entry.Message = logEventArgs.Message;
                if (Application.Current != null)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        try
                        {
                            _Logs.Add(entry);
                            if (_Logs.Count > 100)
                            {
                                _Logs.RemoveAt(0);
                            }
                            LogListView.ScrollIntoView(LogListView.Items[LogListView.Items.Count - 1]);
                        }
                        catch (Exception)
                        {
                            //wtf?
                        }
                    }));
                }
                else
                {
                    _Logs.Add(entry);
                    if (_Logs.Count > 100)
                    {
                        _Logs.RemoveAt(0);
                    }
                    LogListView.ScrollIntoView(LogListView.Items[LogListView.Items.Count - 1]);
                }
            }catch(Exception)
            {
                //wtf;
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                _voluntLib.Stop();
            }
            catch (Exception)
            {
                //wtf?
            }
        }

    }

    public class LogEntry
    {
        public string LogTime { get; set; }
        public string LogType { get; set; }
        public string Message { get; set; }
    }
}
