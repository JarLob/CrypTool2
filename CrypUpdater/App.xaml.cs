﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Collections;
using System.Threading;
using System.Windows.Threading;
using Ionic.Zip;


namespace CrypUpdater
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {

        private MainWindow m;

        internal static string CryptoolExePath;

        private string ZipFilePath;

        private string CryptoolFolderPath;

        private Process p;

        private List<Process> unwantedProcesses = new List<Process>();


        private void Application_Startup(object sender, StartupEventArgs e)
        {

            m = new CrypUpdater.MainWindow();
            m.Show();
            int CryptoolProcessID;
            

            try
            {
                ZipFilePath = e.Args[0];
                CryptoolFolderPath = e.Args[1];
                CryptoolExePath = e.Args[2];
                CryptoolProcessID = Convert.ToInt32(e.Args[3]);
                p = Process.GetProcessById(CryptoolProcessID);

                if (p.WaitForExit(1000 * 30))
                {
                    unwantedProcesses = FindCrypToolProcesses(CryptoolExePath);
                    if (unwantedProcesses.Count == 0)
                        UnpackZip(ZipFilePath, CryptoolFolderPath);
                    else
                    {
                        AskForLicenseToKill();
                    }

                }
                else
                {
                    MessageBoxButton b = MessageBoxButton.OKCancel;
                    string caption = "Timeout error";
                    MessageBoxResult result;
                    result = MessageBox.Show("CrypTool 2.0 failed to shut down. Kill the process to proceed?", caption, b);
                    if (result == MessageBoxResult.OK)
                    {
                        try
                        {
                            p.Kill();
                            p.WaitForExit();
                            unwantedProcesses = FindCrypToolProcesses(CryptoolExePath);
                            if (unwantedProcesses.Count == 0)
                                UnpackZip(ZipFilePath, CryptoolFolderPath);
                            else
                            {
                                AskForLicenseToKill();
                            }
                        }
                        catch (Exception)
                        {
                            unwantedProcesses = FindCrypToolProcesses(CryptoolExePath);
                            if (unwantedProcesses.Count == 0)
                                UnpackZip(ZipFilePath, CryptoolFolderPath);
                            else
                            {
                                AskForLicenseToKill();
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Update failed. CrypTool 2.0 will be restarted.");
                        RestartCryptool();
                    }
                    

                }

            }
            catch (IndexOutOfRangeException) // parameter not set
            {
                if (CryptoolExePath != null)
                {
                    MessageBox.Show("Update failed. CrypTool 2.0 will be restarted.", "Error");
                    RestartCryptool();
                }
                else
                {
                    UpdateFailure();
                }
            }
            catch (FormatException) // no id was parsable 
            {
                UpdateFailure();
            }
            catch (ArgumentException) // the invoking process has already exited (no such process with this id exists)
            {
                unwantedProcesses = FindCrypToolProcesses(CryptoolExePath);
                if (unwantedProcesses.Count == 0)
                    UnpackZip(ZipFilePath, CryptoolFolderPath);
                else
                {
                    AskForLicenseToKill();
                }
            }

        }

        private void AskForLicenseToKill()
        {
            MessageBoxButton mbb = MessageBoxButton.YesNo;
            string caption = "Error";
            string messagePart1;
            string messagePart2;
            string messagePart3;
            if (unwantedProcesses.Count > 1)
            {
                messagePart1 = "Several instances";
                messagePart2 = "are";
                messagePart3 = "these processes";
            }
            else
            {
                messagePart1 = "Another instance";
                messagePart2 = "is";
                messagePart3 = "this process";
            }
            MessageBoxResult result;
            result = MessageBox.Show(messagePart1 + " of CrypTool 2.0 using the same resources " + messagePart2 + " still running. Kill " + messagePart3 + " to proceed?", caption, mbb);
            if (result == MessageBoxResult.Yes)
            {
                KillOtherProcesses(unwantedProcesses);
                UnpackZip(ZipFilePath, CryptoolFolderPath);
            }
            else
            {
                MessageBox.Show("Update failed. CrypTool 2.0 will be restarted.");
                RestartCryptool();
            }
        }

        private static void KillOtherProcesses(List<Process> unwantedProcesses)
        {
            try
            {
                foreach (Process pr in unwantedProcesses)
                {
                    if (!pr.HasExited)
                    {
                        pr.Kill();
                        pr.WaitForExit();
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Update failed. Not able to remove remaining CrypTool 2.0 instances.", "Error");
                RestartCryptool();
            }
        }

        private static void UpdateFailure()
        {
            MessageBox.Show("Update failed, wrong parameters!", "Error");
            Application.Current.Shutdown();
        }

        private static void RestartCryptool()
        {
            try
            {
                System.Diagnostics.Process.Start(CryptoolExePath);
                Application.Current.Shutdown();
            }
            catch (Exception)
            {
                MessageBoxButton bu = MessageBoxButton.OK;
                string caption2 = "Error";
                MessageBoxResult res = MessageBox.Show("CrypTool 2.0 could not be restarted! Try again later.", caption2, bu);
                if (res == MessageBoxResult.OK)
                {
                    Application.Current.Shutdown();
                }
            }
        }


        private void UnpackZip(string ZipFilePath, string CryptoolFolderPath)
        {

            try
            {
                using (ZipFile zip = ZipFile.Read(ZipFilePath))
                {
                    int count = zip.Entries.Count;
                    int i = 0;
                    int progress = 0;

                    foreach (ZipEntry e in zip)
                    {
                        e.Extract(CryptoolFolderPath, ExtractExistingFileAction.OverwriteSilently);
                        i++;
                        progress = i * 100 / count;
                        m.UpdateProgress(progress);
                    }

                    RestartCryptool();

                }
            }
            catch (Exception)
            {
                MessageBox.Show("Update failed. CrypTool 2.0 will be restarted.", "Error");
                RestartCryptool();
            }

        }


        private List<Process> FindCrypToolProcesses(string cryptoolExePath)
        {
            List<Process> processList = new List<Process>();

            foreach (Process p in Process.GetProcesses())
            {
                if (p.ProcessName == "CrypStartup" && p.MainModule.FileName == cryptoolExePath)
                    processList.Add(p);
            }

            return processList;
        }

    }
}
