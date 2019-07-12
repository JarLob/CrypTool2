﻿using Cryptool.Plugins.DECODEDatabaseTools.DataObjects;
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
using System.Windows.Threading;

namespace Cryptool.Plugins.DECODEDatabaseTools
{
    /// <summary>
    /// Interaktionslogik für DECODETextParserPresentation.xaml
    /// </summary>
    public partial class DECODETextParserPresentation : UserControl
    {
        public DECODETextParserPresentation()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Shows the given document in the user interface
        /// </summary>
        /// <param name="document"></param>
        public void ShowDocument(TextDocument document)
        {
            Dispatcher.Invoke(DispatcherPriority.Background, (SendOrPostCallback)delegate
            {
                PageList.Items.Clear();
            }, null);
            foreach (var page in document.Pages)
            {
                Dispatcher.Invoke(DispatcherPriority.Background, (SendOrPostCallback)delegate
                {
                    PageList.Items.Add(page);
                }, null);
            }
        }

        /// <summary>
        /// Forward MouseWheel event to parent ui element (ScrollViewer) 
        /// to enable the user to scroll through the image list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PageList_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }
    }
}
