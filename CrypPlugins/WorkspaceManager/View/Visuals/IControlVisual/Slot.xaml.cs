﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Cryptool.PluginBase;
using WorkspaceManager.Model;
using WorkspaceManagerModel.Model.Operations;

namespace WorkspaceManager.View.Visuals
{
    public partial class Slot : UserControl
    {
        private WorkspaceManagerClass MyEditor { get; set; }

        private readonly IControlMasterElement element;
        private readonly bool loading;

        public static readonly DependencyProperty SelectedTypeProperty = DependencyProperty.Register("SelectedType",
            typeof (SlaveType), typeof (Slot), new FrameworkPropertyMetadata(null, OnSelectedTypeChanged));

        public SlaveType SelectedType
        {
            get { return (SlaveType) GetValue(SelectedTypeProperty); }
            set { SetValue(SelectedTypeProperty, value); }
        }

        public static readonly DependencyProperty ActiveModelProperty = DependencyProperty.Register("ActiveModel",
            typeof (PluginModel), typeof (Slot), new FrameworkPropertyMetadata(null, OnActiveModelChanged));

        public PluginModel ActiveModel
        {
            get { return (PluginModel) GetValue(ActiveModelProperty); }
            set { SetValue(ActiveModelProperty, value); }
        }

        private ConnectionModel activeConnectionModel;

        public static readonly DependencyProperty TypesProperty = DependencyProperty.Register("Types",
            typeof (ObservableCollection<SlaveType>), typeof (Slot), new FrameworkPropertyMetadata(null));

        public ObservableCollection<SlaveType> Types
        {
            get { return (ObservableCollection<SlaveType>) GetValue(TypesProperty); }
            set { SetValue(TypesProperty, value); }
        }

        public Slot()
        {
            InitializeComponent();
        }

        public Slot(IControlMasterElement element)
        {
            loading = true;
            InitializeComponent();
            this.element = element;
            Types = new ObservableCollection<SlaveType>();
            MyEditor = (WorkspaceManagerClass) element.ConnectorModel.WorkspaceModel.MyEditor;
            if (ComponentInformations.PluginsWithSpecificController.ContainsKey(element.ConnectorModel.ConnectorType))
            {
                var list = ComponentInformations.PluginsWithSpecificController[element.ConnectorModel.ConnectorType];

                foreach (var e in list)
                    Types.Add(new SlaveType(e));

                var collection = element.ConnectorModel.GetOutputConnections();
                if (collection.Count != 0)
                {
                    ActiveModel = element.ConnectorModel.GetOutputConnections()[0].To.PluginModel;
                    activeConnectionModel = element.ConnectorModel.GetOutputConnections()[0];
                    SelectedType = Types.Single(a => a.Type.IsAssignableFrom(ActiveModel.PluginType));
                }
            }
            loading = false;
        }

        private static void OnActiveModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var slot = (Slot) d;
            slot.element.PluginModel = slot.ActiveModel;
        }

        private static void OnSelectedTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var slot = (Slot) d;
            SlaveType type;
            PluginChangedEventArgs args;

            var model = slot.element.ConnectorModel;
            if (slot.loading)
                return;

            if ((e.OldValue != null || e.NewValue == null) && slot.ActiveModel != null)
            {
                model.WorkspaceModel.ModifyModel(new DeletePluginModelOperation(slot.ActiveModel));
                model.WorkspaceModel.ModifyModel(new DeleteConnectionModelOperation(slot.activeConnectionModel));
                args = new PluginChangedEventArgs(slot.ActiveModel.Plugin, slot.ActiveModel.GetName(),
                    DisplayPluginMode.Normal);
                slot.MyEditor.onSelectedPluginChanged(args);
                slot.ActiveModel = null;
            }

            if (e.OldValue != null && e.NewValue != null)
            {
                if (((SlaveType) e.OldValue).Type.IsAssignableFrom(((SlaveType) e.NewValue).Type))
                    return;
            }

            if (e.NewValue == null)
                return;

            type = e.NewValue as SlaveType;
            var v =
                (PluginModel)
                    model.WorkspaceModel.ModifyModel(new NewPluginModelOperation(new Point(0, 0), 0, 0, type.Type));
            slot.ActiveModel = v;
            var f =
                v.GetInputConnectors()
                    .Single(a => a.ConnectorType.IsAssignableFrom(slot.element.ConnectorModel.ConnectorType));
            slot.activeConnectionModel =
                (ConnectionModel)
                    model.WorkspaceModel.ModifyModel(new NewConnectionModelOperation(model, f, f.ConnectorType));

            args = new PluginChangedEventArgs(slot.ActiveModel.Plugin, slot.ActiveModel.GetName(),
                DisplayPluginMode.Normal);
            slot.MyEditor.onSelectedPluginChanged(args);
        }

        private void MouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (ActiveModel != null)
            {
                var args = new PluginChangedEventArgs(ActiveModel.Plugin, ActiveModel.GetName(),
                    DisplayPluginMode.Normal);
                MyEditor.onSelectedPluginChanged(args);
                e.Handled = true;
            }
        }

        private void ClickHandler(object sender, RoutedEventArgs e)
        {
            SelectedType = null;
        }
    }

    public class SlaveType
    {
        public Image Icon { get; set; }
        public Type Type { get; set; }
        public string ToolTip { get; set; }
        
        public SlaveType(Type type)
        {
            Icon = type.GetImage(0);
            Type = type;
            ToolTip = type.Name;
        }
    }

    public class CustomPopUp : Popup
    {
        private static readonly double offset = -3.8;

        public static readonly DependencyProperty ItemPanelProperty = DependencyProperty.Register("ItemPanel",
            typeof (Panel), typeof (CustomPopUp), new FrameworkPropertyMetadata(null));

        public Panel ItemPanel
        {
            get { return (Panel) GetValue(ItemPanelProperty); }
            set { SetValue(ItemPanelProperty, value); }
        }

        public static readonly DependencyProperty IndexProperty = DependencyProperty.Register("Index",
            typeof (int), typeof (CustomPopUp), new FrameworkPropertyMetadata(0));

        public int Index
        {
            get { return (int) GetValue(IndexProperty); }
            set { SetValue(IndexProperty, value); }
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            var i = Index;

            if (Index > ItemPanel.Children.Count - 1)
                throw new Exception("Index blub");

            Point point;
            if (Index <= -1)
                point = new Point(52, 52);
            else
                point = ItemPanel.TransformToVisual(ItemPanel.Children[i]).Transform(new Point(0, 0));
            HorizontalOffset = point.X + offset;
        }
    }
}
