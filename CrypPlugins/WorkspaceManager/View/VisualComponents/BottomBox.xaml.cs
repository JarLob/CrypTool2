﻿using System;
using System.Windows;
using System.Windows.Controls;

namespace WorkspaceManager.View.VisualComponents
{
    /// <summary>
    /// Interaktionslogik für Settings.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("WorkspaceManager.Properties.Resources")]
    public partial class BottomBox : UserControl
    {
        public event EventHandler<ImageSelectedEventArgs> AddImage;
        public event EventHandler<AddTextEventArgs> AddText;
        public event EventHandler<FitToScreenEventArgs> FitToScreen;
        public event EventHandler Overview;
        public event EventHandler Sort;

        public BottomBox()
        {
            InitializeComponent();
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn.Name == "ADDIMG")
            {
                System.Windows.Forms.OpenFileDialog diag = new System.Windows.Forms.OpenFileDialog();
                if (diag.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Uri uriLocal = new Uri(diag.FileName);

                    if (AddImage != null)
                        AddImage.Invoke(this, new ImageSelectedEventArgs() { uri = uriLocal });
                }
                return;
            }

            if (btn.Name == "ADDTXT")
            {
                if (AddText != null)
                    AddText.Invoke(this, new AddTextEventArgs());
            }


            if (btn.Name == "F2S")
            {
                if (FitToScreen != null)
                    FitToScreen.Invoke(this, new FitToScreenEventArgs());
            }

            if (btn.Name == "OV")
            {
                if (Overview != null)
                    Overview.Invoke(this, new EventArgs());
            }

            if (btn.Name == "SORT")
            {
                if (Sort != null)
                    Sort.Invoke(this, new EventArgs());
            }
        }
    }

    public class ImageSelectedEventArgs : EventArgs
    {
        public Uri uri;
    }

    public class AddTextEventArgs : EventArgs
    {
    }

    public class FitToScreenEventArgs : EventArgs
    {
    }

}
