﻿using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Cryptool.PluginBase;
using Cryptool.PluginBase.Miscellaneous;

namespace FileInput
{
    /// <summary>
    /// Interaction logic for FileInputWPFPresentation.xaml
    /// </summary>
    public partial class FileInputWPFPresentation : UserControl
    {
        private readonly FileInputClass exp;
        public HexBox.HexBox hexBox;
        private Window window;

        public FileInputWPFPresentation(FileInputClass exp)
        {
            InitializeComponent();
            this.exp = exp;
            SizeChanged += sizeChanged;
            hexBox = new HexBox.HexBox();
            hexBox.OnFileChanged += fileChanged;
            MainMain.Children.Add(hexBox);
            this.hexBox.ErrorOccured += new HexBox.HexBox.GUIErrorEventHandler(hexBox_ErrorOccured);
        }

        void hexBox_ErrorOccured(object sender, HexBox.GUIErrorEventArgs ge)
        {
            exp.getMessage(ge.message);
        }

        public void makeUnaccesAble(Boolean b)
        {
            hexBox.makeUnAccesable(b);
            hexBox.IsEnabled = b;
            hexBox.IsManipulationEnabled = b;
        }

        public void CloseFileToGetFileStreamForExecution()
        {
            try
            {
                hexBox.saveData(true, false);
                hexBox.closeFile(false);
                hexBox.openFile((exp.Settings as FileInputSettings).OpenFilename, true);
                //hexBox.IsEnabled = false;
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Error trying to reopen file: {0}", ex), NotificationLevel.Error);
            }
        }

        public void ReopenClosedFile()
        {
            //closedForExecution = false;

            if (File.Exists((exp.Settings as FileInputSettings).OpenFilename))
            {
                // tbFileClosedWhileRunning.Visibility = Visibility.Collapsed;
                // windowsFormsHost.Visibility = Visibility.Visible;
                hexBox.closeFile(false);
                hexBox.openFile((exp.Settings as FileInputSettings).OpenFilename, false);
            }
            //hexBox.IsEnabled = true;
            try
            {
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Error trying to reopen file: {0}", ex), NotificationLevel.Error);
            }
        }


        internal void OpenFile(String fileName)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback) delegate
            {
                try
                {
                    hexBox.openFile(fileName, false);
                }
                catch (Exception ex)
                {
                    GuiLogMessage(string.Format("Error trying to open file: {0}",ex),NotificationLevel.Error);
                }
            }, null);
        }

        public void dispose()
        {
            hexBox.dispose();
        }

        private void fileChanged(Object sender, EventArgs eventArgs)
        {
            (exp.Settings as FileInputSettings).OpenFilename = hexBox.Pfad;
        }

        private void sizeChanged(Object sender, EventArgs eventArgs)
        {           
            hexBox.Width = ActualWidth;
            hexBox.Height = ActualHeight;
        }

        internal void CloseFile()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback) delegate { hexBox.closeFile(true); },
                                   null);
        }

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, exp, new GuiLogEventArgs(message, exp, logLevel));
        }
    }
}