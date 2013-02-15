﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace Startcenter
{
    /// <summary>
    /// Interaction logic for CTTreeViewItem.xaml
    /// </summary>
    public partial class CTTreeViewItem : TreeViewItem
    {
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(
            "Icon",
            typeof(ImageSource),
            typeof(CTTreeViewItem),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, null));

        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set
            {
                SetValue(IconProperty, value);
            }
        }

        public static readonly DependencyProperty IsDirectoryProperty =
            DependencyProperty.Register(
            "IsDirectory",
            typeof(bool),
            typeof(CTTreeViewItem),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, null));

        public bool IsDirectory
        {
            get { return (bool)GetValue(IsDirectoryProperty); }
            set
            {
                SetValue(IsDirectoryProperty, value);
            }
        }

        public static readonly DependencyProperty DirectoryProperty =
            DependencyProperty.Register(
            "Directory",
            typeof(DirectoryInfo),
            typeof(CTTreeViewItem),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, null));

        public DirectoryInfo Directory
        {
            get { return (DirectoryInfo)GetValue(DirectoryProperty); }
            set
            {
                SetValue(DirectoryProperty, value);
            }
        }
        
        public FileInfo File { get; private set; }
        public string Title { get; private set; }
        public int Order { get; private set; }

        /// <summary>
        /// Constructor only for files
        /// </summary>
        public CTTreeViewItem(FileInfo file, string title, Inline tooltip, ImageSource image)
        {
            this.File = file;
            this.Order = -1;
            this.Title = title;
            this.Icon = image;
            this.IsDirectory = false;
            this.Tag = new KeyValuePair<string, string>(file.FullName, title);
            var tooltipBlock = new TextBlock(tooltip) {TextWrapping = TextWrapping.Wrap, MaxWidth = 400};
            this.ToolTip = tooltipBlock;

            InitializeComponent();
        }

        /// <summary>
        /// Constructor only for directories
        /// </summary>
        public CTTreeViewItem(string title, int order = -1, Inline tooltip = null, ImageSource image = null)
        {
            this.Title = title;
            this.Order = order;
            this.IsDirectory = true;
            if (tooltip != null)
            {
                this.ToolTip = new TextBlock(tooltip) {TextWrapping = TextWrapping.Wrap, MaxWidth = 400};
            }

            if (image != null)
            {
                this.Icon = image;
            }
            else
            {
                this.Icon = new BitmapImage(new Uri("pack://application:,,,/CrypWin;component/images/Open32.png"));
            }
            InitializeComponent();
        }
    }
}
