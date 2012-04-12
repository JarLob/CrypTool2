﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Cryptool.PluginBase.Attributes;
using WorkspaceManager.Model;
using WorkspaceManager.View.VisualComponents;

namespace WorkspaceManager
{
    /// <summary>
    /// Interaction logic for WorkspaceManagerSettingsTab.xaml
    /// </summary>
    [Localization("WorkspaceManager.Properties.Resources")]
    [SettingsTab("WorkspaceManagerSettings", "/MainSettings/", 1.1)]
    public partial class WorkspaceManagerSettingsTab : UserControl
    {
        public WorkspaceManagerSettingsTab(Style settingsStyle)
        {
            Resources.Add("settingsStyle", settingsStyle);
            InitializeComponent();
            InitializeColors();
            Cryptool.PluginBase.Properties.Settings.Default.PropertyChanged += delegate { Cryptool.PluginBase.Miscellaneous.ApplicationSettingsHelper.SaveApplicationsSettings(); };
        }

        private void InitializeColors()
        {
            IntegerColor.SelectedColor = (ColorHelper.IntegerColor);
            ByteColor.SelectedColor = (ColorHelper.ByteColor);
            DoubleColor.SelectedColor = (ColorHelper.DoubleColor);
            BoolColor.SelectedColor = (ColorHelper.BoolColor);
            StreamColor.SelectedColor = (ColorHelper.StreamColor);
            StringColor.SelectedColor = (ColorHelper.StringColor);
            ObjectColor.SelectedColor = (ColorHelper.ObjectColor);
            BigIntegerColor.SelectedColor = (ColorHelper.BigIntegerColor);
            DefaultColor.SelectedColor = (ColorHelper.DefaultColor);

            AsymmetricColor.SelectedColor = (ColorHelper.AsymmetricColor);
            ClassicColor.SelectedColor = (ColorHelper.ClassicColor);
            SymmetricblockColor.SelectedColor = (ColorHelper.SymmetricColor);
            ToolsColor.SelectedColor = (ColorHelper.ToolsColor);
            SteganographyColor.SelectedColor = (ColorHelper.SteganographyColor);
            HashColor.SelectedColor = (ColorHelper.HashColor);
            AnalysisGenericColor.SelectedColor = (ColorHelper.AnalysisGenericColor);
            AnalysisSpecificColor.SelectedColor = (ColorHelper.AnalysisSpecificColor);
            ProtocolsColor.SelectedColor = (ColorHelper.ProtocolColor);
        }

        private void ResetColorButton_Click(object sender, RoutedEventArgs e)
        {
            ColorHelper.SetDefaultColors();
            this.InitializeColors();
        }

        private void CrPickerSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            if (sender == IntegerColor)
            {
                ColorHelper.IntegerColor = (IntegerColor.SelectedColor);
            }
            else if (sender == ByteColor)
            {
                ColorHelper.ByteColor = (ByteColor.SelectedColor);
            }
            else if (sender == DoubleColor)
            {
                ColorHelper.DoubleColor = (DoubleColor.SelectedColor);
            }
            else if (sender == BoolColor)
            {
                ColorHelper.BoolColor = (BoolColor.SelectedColor);
            }
            else if (sender == StreamColor)
            {
                ColorHelper.StreamColor = (StreamColor.SelectedColor);
            }
            else if (sender == StringColor)
            {
                ColorHelper.StringColor = (StringColor.SelectedColor);
            }
            else if (sender == ObjectColor)
            {
                ColorHelper.ObjectColor = (ObjectColor.SelectedColor);
            }
            else if (sender == BigIntegerColor)
            {
                ColorHelper.BigIntegerColor = (BigIntegerColor.SelectedColor);
            }
            else if (sender == DefaultColor)
            {
                ColorHelper.DefaultColor = (DefaultColor.SelectedColor);
            }
            else if (sender == AsymmetricColor)
            {
                ColorHelper.AsymmetricColor = (AsymmetricColor.SelectedColor);
            }
            else if (sender == ClassicColor)
            {
                ColorHelper.ClassicColor = (ClassicColor.SelectedColor);
            }
            else if (sender == SymmetricblockColor)
            {
                ColorHelper.SymmetricColor = (SymmetricblockColor.SelectedColor);
            }
            else if (sender == SteganographyColor)
            {
                ColorHelper.SteganographyColor = (SteganographyColor.SelectedColor);
            }
            else if (sender == ProtocolsColor)
            {
                ColorHelper.ProtocolColor = (ProtocolsColor.SelectedColor);
            }
            else if (sender == ToolsColor)
            {
                ColorHelper.ToolsColor = (ToolsColor.SelectedColor);
            }
            else if (sender == HashColor)
            {
                ColorHelper.HashColor = (HashColor.SelectedColor);
            }
            else if (sender == AnalysisGenericColor)
            {
                ColorHelper.AnalysisGenericColor = (AnalysisGenericColor.SelectedColor);
            }
            else if (sender == AnalysisSpecificColor)
            {
                ColorHelper.AnalysisSpecificColor = (AnalysisSpecificColor.SelectedColor);
            }
        }
    }
}
