﻿using System;
using System.Windows.Controls;

namespace Cryptool.Plugins.QuadraticSieve
{
    /// <summary>
    /// Interaction logic for QuadraticSievePresentation.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("QuadraticSieve.Properties.Resources")]
    public partial class QuadraticSievePresentation : UserControl
    {
        private ProgressRelationPackages progressRelationPackages;
        public ProgressRelationPackages ProgressRelationPackages
        {
            get { return progressRelationPackages; }
        }

        public QuadraticSievePresentation()
        {
            InitializeComponent();

            progressRelationPackages = new ProgressRelationPackages(peer2peerScrollViewer);
            peer2peerScrollViewer.Content = progressRelationPackages;
            progressRelationPackages.MaxWidth = 620 - 30;
        }

        public void SelectFirstComposite()
        {
            foreach (String item in factorList.Items)
            {
                if (item.StartsWith("Composite"))
                {
                    factorList.SelectedItem = item;
                    factorList.ScrollIntoView(item);
                    return;
                }
            }
        }
    }
}
