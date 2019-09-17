using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cryptool.CrypAnalysisViewControl
{
    public partial class CrypAnalysisResultListView : ListView
    {
        public static RoutedCommand ClickContextMenu = new RoutedCommand("ClickContextMenu", typeof(RoutedCommand));

        public CrypAnalysisResultListView()
        {
            CommandBindings.Add(new CommandBinding(ClickContextMenu, ContextMenuHandler));
            InitializeComponent();
        }

        static CrypAnalysisResultListView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CrypAnalysisResultListView), new FrameworkPropertyMetadata(typeof(CrypAnalysisResultListView)));
        }

        private static void ContextMenuHandler(object sender, ExecutedRoutedEventArgs e)
        {
        }
    }
}
