﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for StorageControl.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("Wizard.Properties.Resources")]
    public partial class StorageControl : UserControl
    {
        public delegate void CloseEventDelegate();
        public event CloseEventDelegate CloseEvent;

        private readonly Action<string> _setValueDelegate;
        private ICollectionView _view;

        public StorageControl(string defaultValue, string defaultKey, Action<string> setValueDelegate, bool applyColumnVisible)
        {
            InitializeComponent();
            if (!applyColumnVisible)
            {
                EntriesGridView.Columns.Remove(ApplyEntryColumn);   //Hide apply column
            }
            
            _setValueDelegate = setValueDelegate;
            StoreValue.Text = defaultValue;
            StoreKey.Text = defaultKey;
            RefreshSource();
            Cryptool.PluginBase.Properties.Settings.Default.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args)
                                                                                   {
                                                                                       if (args.PropertyName == "Wizard_Storage")
                                                                                       {
                                                                                           RefreshSource();
                                                                                       }
                                                                                   };
        }

        public StorageControl() : this(null, null, null, false)
        {
            CancelButton.Visibility = Visibility.Collapsed;     //Hide cancel button
        }

        private void ApplyButtonClicked(object sender, RoutedEventArgs e)
        {
            var entryToLoad = (StorageEntry)((Button)sender).Tag;
            Debug.Assert(entryToLoad != null);
            _setValueDelegate(entryToLoad.Value);
            OnCloseEvent();
        }

        private void AddButtonClicked(object sender, RoutedEventArgs e)
        {
            var newEntry = new StorageEntry(StoreKey.Text, StoreValue.Text, StoreDescription.Text);
            var storage = Cryptool.PluginBase.Properties.Settings.Default.Wizard_Storage ?? new ArrayList();
            storage.Add(newEntry);

            SaveAndClose(storage);
            KeyListBox.SelectedItem = newEntry;
            KeyListBox.ScrollIntoView(newEntry);
        }

        private void ModifyButtonClicked(object sender, RoutedEventArgs e)
        {
            var entry = KeyListBox.SelectedItem as StorageEntry;
            Debug.Assert(entry != null);
            entry.Key = StoreKey.Text;
            entry.Value = StoreValue.Text;
            entry.Description = StoreDescription.Text;

            Save(Cryptool.PluginBase.Properties.Settings.Default.Wizard_Storage);
            KeyListBox.SelectedItem = entry;
            KeyListBox.ScrollIntoView(entry);
        }

        private void RemoveButtonClick(object sender, RoutedEventArgs e)
        {
            var entryToRemove = (StorageEntry)((Button)sender).Tag;
            Debug.Assert(entryToRemove != null);
            var storage = Cryptool.PluginBase.Properties.Settings.Default.Wizard_Storage;
            Debug.Assert(storage != null);

            var res = MessageBox.Show(Properties.Resources.RemoveEntryQuestion, Properties.Resources.RemoveEntry, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                storage.Remove(entryToRemove);
                Save(storage);
                RefreshSource();
            }
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            OnCloseEvent();
        }

        private void RefreshSource()
        {
            if (_view != null)
            {
                _view.Refresh();
            }
            else if (Cryptool.PluginBase.Properties.Settings.Default.Wizard_Storage != null)
            {
                _view = CollectionViewSource.GetDefaultView(Cryptool.PluginBase.Properties.Settings.Default.Wizard_Storage);
                if (_view.GroupDescriptions.Count == 0)
                {
                    _view.GroupDescriptions.Add(new PropertyGroupDescription("Key"));
                    _view.SortDescriptions.Add(new SortDescription("Created", ListSortDirection.Ascending));
                }
                KeyListBox.ItemsSource = _view;
            }
        }

        private void SaveAndClose(ArrayList storage)
        {
            Save(storage);
            OnCloseEvent();
        }

        private static void Save(ArrayList storage)
        {
            Cryptool.PluginBase.Properties.Settings.Default.Wizard_Storage = storage;
            Cryptool.PluginBase.Properties.Settings.Default.Save();
        }

        private void KeyListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (KeyListBox.SelectedItem != null)
            {
                var selectedEntry = ((StorageEntry) KeyListBox.SelectedItem);
                StoreKey.Text = selectedEntry.Key;
                StoreValue.Text = selectedEntry.Value;
                StoreDescription.Text = selectedEntry.Description;
            }
        }

        private void OnCloseEvent()
        {
            if (CloseEvent != null)
            {
                CloseEvent();
            }
        }
    }
}