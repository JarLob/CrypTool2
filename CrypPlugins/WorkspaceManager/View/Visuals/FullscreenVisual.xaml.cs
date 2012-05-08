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
using System.Collections.ObjectModel;
using WorkspaceManager.View.Base;
using System.ComponentModel;
using WorkspaceManager.Model;

namespace WorkspaceManager.View.Visuals
{
    /// <summary>
    /// Interaction logic for BinFullscreenVisual.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("WorkspaceManager.Properties.Resources")]
    public partial class FullscreenVisual : UserControl, INotifyPropertyChanged
    {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler Close;
        #endregion

        #region Properties

        public UIElement ActivePresentation
        {
            get
            {
                if (ActiveComponent == null)
                    return null;
                UIElement o = null;

                ActiveComponent.Presentations.TryGetValue(ActiveComponent.FullScreenState, out o);
                return o;
            }
        }



        public bool HasComponentPresentation
        {
            get
            {
                if (ActiveComponent == null)
                    return false;

                UIElement e = null;
                ActiveComponent.Presentations.TryGetValue(BinComponentState.Presentation, out e);
                return e == null ? false : true;
            }
        }

        public bool HasComponentSetting
        {
            get
            {
                if (ActiveComponent == null)
                    return false;

                UIElement e = null;
                ActiveComponent.Presentations.TryGetValue(BinComponentState.Setting, out e);
                return e == null ? false : true;
            }
        }

        private ComponentVisual lastActiveComponent;
        public ComponentVisual LastActiveComponent
        {
            set
            {
                lastActiveComponent = value;
            }
            get
            {
                return lastActiveComponent;
            }
        }

        #endregion

        #region DependencyProperties
        public static readonly DependencyProperty ComponentCollectionProperty = DependencyProperty.Register("ComponentCollection",
            typeof(ObservableCollection<ComponentVisual>), typeof(FullscreenVisual), new FrameworkPropertyMetadata(null, null));

        public ObservableCollection<ComponentVisual> ComponentCollection
        {
            get
            {
                return (ObservableCollection<ComponentVisual>)base.GetValue(ComponentCollectionProperty);
            }
            set
            {
                base.SetValue(ComponentCollectionProperty, value);
            }
        }

        public static readonly DependencyProperty ActiveComponentProperty = DependencyProperty.Register("ActiveComponent",
            typeof(ComponentVisual), typeof(FullscreenVisual), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnActiveComponentChanged)));

        public ComponentVisual ActiveComponent
        {
            get
            {
                return (ComponentVisual)base.GetValue(ActiveComponentProperty);
            }
            set
            {
                base.SetValue(ActiveComponentProperty, value);
            }
        }

        public static readonly DependencyProperty IsFullscreenOpenProperty = DependencyProperty.Register("IsFullscreenOpen",
            typeof(bool), typeof(FullscreenVisual), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsFullscreenOpenChanged)));


        public bool IsFullscreenOpen
        {
            get
            {
                return (bool)base.GetValue(IsFullscreenOpenProperty);
            }
            set
            {
                base.SetValue(IsFullscreenOpenProperty, value);
            }
        }
        #endregion

        #region Constructors
        public FullscreenVisual()
        {
            InitializeComponent();
        }
        #endregion

        #region protected
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        #region EventHandler

        private void CloseClickHandler(object sender, RoutedEventArgs e)
        {
            IsFullscreenOpen = false;
            if (Close != null)
                Close.Invoke(this, new EventArgs());
        }

        private void ActionHandler(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (b == null)
                return;

            if (b.Content is BinComponentState && ActiveComponent != null)
            {
                ActiveComponent.FullScreenState = (BinComponentState)b.Content;
                OnPropertyChanged("ActivePresentation");
                return;
            }
            
            e.Handled = true;
        }

        private static void OnActiveComponentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FullscreenVisual f = (FullscreenVisual)d;
            ComponentVisual newBin = (ComponentVisual)e.NewValue;
            ComponentVisual oldBin = (ComponentVisual)e.OldValue;
            if (newBin != null)
            {
                newBin.IsFullscreen = true;
            }

            if (oldBin != null)
            {
                f.LastActiveComponent = oldBin;
                if(oldBin != newBin)
                    oldBin.IsFullscreen = false;
            }

            f.OnPropertyChanged("HasComponentPresentation");
            f.OnPropertyChanged("HasComponentSetting");
            f.OnPropertyChanged("ActivePresentation");
        }

        private static void OnIsFullscreenOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FullscreenVisual f = (FullscreenVisual)d;
            if ((bool)e.NewValue)
            {
                if(f.LastActiveComponent != null)
                    f.LastActiveComponent.IsFullscreen = true;
            }
            else
            {
                if (f.LastActiveComponent != null)
                    f.LastActiveComponent.IsFullscreen = false;
                f.ActiveComponent = null;
            }
            f.OnPropertyChanged("ActivePresentation");
        }
        #endregion



    }
}
