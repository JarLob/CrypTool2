﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Wizard
{
    /// <summary>
    /// Interaction logic for StorageContainer.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("Wizard.Properties.Resources")]
    public partial class StorageContainer : UserControl
    {
        private readonly Action<StorageControl> _showOverlayAction;
        private Action<string> _setValueDelegate;
        private Func<string> _getValueDelegate;
        private string _defaultKey;
        private bool _showLoadButtons;

        public StorageContainer(Action<StorageControl> showOverlayAction)
        {
            _showOverlayAction = showOverlayAction;
            InitializeComponent();
        }

        public void AddContent(Control content, string defaultKey, bool showStorageButton, bool showLoadButtons, bool showAddButtons)
        {
            _showLoadButtons = showLoadButtons;
            StorageContainerContent.Content = content;
            _defaultKey = defaultKey;
            StorageButton.Visibility = showStorageButton ? Visibility.Visible : Visibility.Collapsed;
            LoadButton.Visibility = showLoadButtons ? Visibility.Visible : Visibility.Collapsed;
            AddButton.Visibility = showAddButtons ? Visibility.Visible : Visibility.Collapsed;
        }

        public void SetValueMethod(Action<string> setValueDelegate)
        {
            _setValueDelegate = setValueDelegate;
        }

        public void GetValueMethod(Func<string> getValueDelegate)
        {
            _getValueDelegate = getValueDelegate;
        }

        private void StorageButtonClicked(object sender, RoutedEventArgs e)
        {
            var storageControl = new StorageControl(_getValueDelegate(), _defaultKey, _setValueDelegate, _showLoadButtons);
            _showOverlayAction(storageControl);
        }

        private void AddButtonClicked(object sender, RoutedEventArgs e)
        {
            var key = _defaultKey;
            var newEntry = new StorageEntry(key, _getValueDelegate(), null);
            var storage = Cryptool.PluginBase.Properties.Settings.Default.Wizard_Storage ?? new ArrayList();
            storage.Add(newEntry);
            Save(storage);
        }

        private static void Save(ArrayList storage)
        {
            Cryptool.PluginBase.Properties.Settings.Default.Wizard_Storage = storage;
            Cryptool.PluginBase.Properties.Settings.Default.Save();
        }

        private void LoadButtonClicked(object sender, RoutedEventArgs e)
        {
            var storage = Cryptool.PluginBase.Properties.Settings.Default.Wizard_Storage;
            if (storage != null)
            {
                var entries = storage.Cast<StorageEntry>().Where(x => x.Key == _defaultKey).OrderBy(x => x.Created).ToList();
                if (entries.Count == 1)
                {
                    _setValueDelegate(entries.First().Value);
                }
                if (entries.Count > 1)
                {
                    PopUpItems.ItemsSource = entries;
                    PopUp.IsOpen = true;
                }
            }
            else
            {
                MessageBox.Show("No stored value available.", "No value", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveButtonClick(object sender, RoutedEventArgs e)
        {
            var entryToRemove = (StorageEntry)((Button)sender).Tag;
            var storage = Cryptool.PluginBase.Properties.Settings.Default.Wizard_Storage;
            Debug.Assert(storage != null);

            var res = MessageBox.Show(Properties.Resources.RemoveEntryQuestion, Properties.Resources.RemoveEntry, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                storage.Remove(entryToRemove);
                Save(storage);
                PopUp.IsOpen = false;
            }
        }

        private void SetValue()
        {
            var entry = PopUpItems.SelectedItem as StorageEntry;
            if (entry != null)
            {
                _setValueDelegate(entry.Value);
            }
            PopUp.IsOpen = false;
        }

        private void PopUpItems_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            SetValue();
        }

        private void PopUpItems_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SetValue();
            }
        }
    }
}
