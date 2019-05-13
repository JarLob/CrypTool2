﻿using System.Windows;
using System.Windows.Controls;
using Cryptool.Plugins.MD5Collider.Algorithm;

namespace Cryptool.Plugins.MD5Collider.Presentation
{
    /// <summary>
    /// Interaktionslogik für QuickWatchPresentation.xaml
    /// </summary>
    public partial class QuickWatchPresentation : UserControl
    {
        public static DependencyProperty ColliderProperty = DependencyProperty.Register("Collider", typeof(IMD5ColliderAlgorithm), typeof(QuickWatchPresentation));
        public IMD5ColliderAlgorithm Collider
        {
            get { return (IMD5ColliderAlgorithm)GetValue(ColliderProperty); }
            set { SetValue(ColliderProperty, value); }
        }

        public QuickWatchPresentation()
        {
            InitializeComponent();
        }
    }
}
