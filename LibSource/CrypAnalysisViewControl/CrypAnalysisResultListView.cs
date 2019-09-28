﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cryptool.CrypAnalysisViewControl
{
    public partial class CrypAnalysisResultListView : ListView
    {
        public static RoutedCommand ClickContextMenuCopyValue = new RoutedCommand("ClickContextMenuCopyValue", typeof(RoutedCommand));
        public static RoutedCommand ClickContextMenuCopyKey = new RoutedCommand("ClickContextMenuCopyKey", typeof(RoutedCommand));
        public static RoutedCommand ClickContextMenuCopyText = new RoutedCommand("ClickContextMenuCopyText", typeof(RoutedCommand));
        public static RoutedCommand ClickContextMenuCopyLine = new RoutedCommand("ClickContextMenuCopyLine", typeof(RoutedCommand));
        public static RoutedCommand ClickContextMenuCopyAll = new RoutedCommand("ClickContextMenuCopyAll", typeof(RoutedCommand));

        public delegate void ResultItemActionHandler(ICrypAnalysisResultListEntry item);
        public event ResultItemActionHandler ResultItemAction;

        public CrypAnalysisResultListView()
        {
            CommandBindings.Add(new CommandBinding(ClickContextMenuCopyValue, HandleContextMenuCopyValue));
            CommandBindings.Add(new CommandBinding(ClickContextMenuCopyKey, HandleContextMenuCopyKey));
            CommandBindings.Add(new CommandBinding(ClickContextMenuCopyText, HandleContextMenuCopyText));
            CommandBindings.Add(new CommandBinding(ClickContextMenuCopyLine, HandleContextMenuCopyLine));
            CommandBindings.Add(new CommandBinding(ClickContextMenuCopyAll, HandleContextMenuCopyAll));

            Loaded += CrypAnalysisResultListView_Loaded;
        }

        private void CrypAnalysisResultListView_Loaded(object sender, RoutedEventArgs e)
        {
            //Add mouse double click event handler to item style:
            var itemContainerStyle = new Style(typeof(ListViewItem), ItemContainerStyle);
            itemContainerStyle.Setters.Add(new EventSetter(MouseDoubleClickEvent, new MouseButtonEventHandler(ItemDoubleClickHandler)));
            ItemContainerStyle = itemContainerStyle;
        }

        private void ItemDoubleClickHandler(object sender, MouseButtonEventArgs e)
        {
            var viewItem = sender as ListViewItem;
            if (viewItem?.Content is ICrypAnalysisResultListEntry item)
            {
                ResultItemAction?.Invoke(item);
            }
        }

        private ICrypAnalysisResultListEntry GetCurrentEntry(EventArgs eventArgs)
            => (eventArgs as ExecutedRoutedEventArgs)?.Parameter as ICrypAnalysisResultListEntry;

        private void HandleContextMenuCopyValue(object sender, EventArgs eventArgs)
        {
            SetClipboard(GetCurrentEntry(eventArgs)?.ClipboardValue ?? "");
        }

        private void HandleContextMenuCopyKey(object sender, EventArgs eventArgs)
        {
            SetClipboard(GetCurrentEntry(eventArgs)?.ClipboardKey ?? "");
        }

        private void HandleContextMenuCopyText(object sender, EventArgs eventArgs)
        {
            SetClipboard(GetCurrentEntry(eventArgs)?.ClipboardText ?? "");
        }

        private void HandleContextMenuCopyLine(object sender, EventArgs eventArgs)
        {
            SetClipboard(GetCurrentEntry(eventArgs)?.ClipboardEntry ?? "");
        }

        private void HandleContextMenuCopyAll(object sender, EventArgs eventArgs)
        {
            var listView = sender as CrypAnalysisResultListView;
            var entryStrings = listView?.Items.OfType<ICrypAnalysisResultListEntry>().Select(item => item.ClipboardEntry);
            if (entryStrings != null)
            {
                SetClipboard(string.Join(Environment.NewLine, entryStrings));
            }
        }

        private void SetClipboard(string text)
        {
            text = text.Replace(Convert.ToChar(0x0).ToString(), "");    //Remove null chars in order to avoid problems in clipboard
            Clipboard.SetText(text);
        }
    }
}
