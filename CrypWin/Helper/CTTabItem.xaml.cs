﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections;

namespace Cryptool.CrypWin.Helper
{
    /// <summary>
    /// Interaction logic for CTTabItem.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("Cryptool.CrypWin.Properties.Resources")]
    public partial class CTTabItem : TabItem
    {
        public event EventHandler RequestBigViewFrame;

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(
            "Icon",
            typeof(ImageSource),
            typeof(CTTabItem),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, null));

        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set
            {
                SetValue(IconProperty, value);
            }
        }

        public static readonly DependencyProperty HasChangesProperty =
            DependencyProperty.Register(
            "HasChanges",
            typeof(bool),
            typeof(CTTabItem),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, null));

        public bool HasChanges
        {
            get { return (bool)GetValue(HasChangesProperty); }
            set
            {
                SetValue(HasChangesProperty, value);
            }
        }

        public delegate void OnCloseHandler();
        public event OnCloseHandler OnClose;

        public CTTabItem()
        {
            InitializeComponent();
        }

        public CTTabItem(PluginBase.Editor.IEditor parent)
        {
            this.Editor = parent;
            InitializeComponent();
        }

        public void Close()
        {
            if (OnClose != null)
                OnClose();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            TabControl tabctrl = (TabControl)this.Parent;
            while (tabctrl.Items.Count > 0)
            {
                tabctrl.Items.Cast<CTTabItem>().ToList<CTTabItem>()[0].Close();
            }
        }

        private void DockPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
                Close();
            if (e.LeftButton == MouseButtonState.Pressed && e.ClickCount == 2)
                RequestBigViewFrame.Invoke(this, new EventArgs());
        }

        private void Button_Click3(object sender, RoutedEventArgs e)
        {
            TabControl tabctrl = (TabControl)this.Parent;
            var list = tabctrl.Items.Cast<CTTabItem>().ToList().Where(a => a != this);

            foreach (CTTabItem o in list)
                o.Close();
        }

        public PluginBase.Editor.IEditor Editor { get; set; }

        private void CopyToClipboard(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Header.ToString());
        }
    }
}
