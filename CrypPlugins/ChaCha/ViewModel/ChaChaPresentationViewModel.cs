﻿using Cryptool.Plugins.ChaCha.Helper;
using Cryptool.Plugins.ChaCha.ViewModel.Components;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Windows.Input;

namespace Cryptool.Plugins.ChaCha.ViewModel
{
    internal class ChaChaPresentationViewModel : ViewModelBase, IChaCha
    {
        public ChaChaPresentationViewModel(ChaCha chaCha)
        {
            ChaCha = chaCha;
            ChaCha.PropertyChanged += new PropertyChangedEventHandler(OnPluginPropertyChanged);
            // Add available pages
            Pages.Add(new StartViewModel());
            Pages.Add(new OverviewViewModel(this));
            Pages.Add(new DiffusionViewModel(this));
            Pages.Add(new StateMatrixInitViewModel(this));
            Pages.Add(new ChaChaHashViewModel(this));

            // Set starting page
            CurrentPage = Pages[0];
        }

        /// <summary>
        /// This is called if a property on the plugin has changed.
        /// </summary>
        private void OnPluginPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("NavigationEnabled");

            ChangePage(Pages[0]);
        }

        #region Commands

        private ICommand _changePageCommand; public ICommand ChangePageCommand
        {
            get
            {
                if (_changePageCommand == null)
                {
                    _changePageCommand = new RelayCommand(
                        p => ChangePage((INavigation)p),
                        p => p is INavigation);
                }

                return _changePageCommand;
            }
        }

        private List<INavigation> _pages; public List<INavigation> Pages
        {
            get
            {
                if (_pages == null)
                    _pages = new List<INavigation>();

                return _pages;
            }
        }

        private INavigation _currentPage; public INavigation CurrentPage
        {
            get
            {
                return _currentPage;
            }
            set
            {
                if (_currentPage != value)
                {
                    _currentPage = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion Commands

        #region Binding Properties

        public bool NavigationEnabled
        {
            get => ChaCha.ExecutionFinished && ChaCha.IsValid;
        }

        #endregion Binding Properties

        #region Binding Properties (Diffusion)

        private DiffusionViewModel DiffusionViewModel
        {
            get => (DiffusionViewModel)Pages[2];
        }

        public byte[] DiffusionInputKey
        {
            get => DiffusionViewModel.DiffusionKey;
        }

        public byte[] DiffusionInputIV
        {
            get => DiffusionViewModel.DiffusionIV;
        }

        public BigInteger DiffusionInitialCounter
        {
            get => DiffusionViewModel.DiffusionInitialCounter;
        }

        public bool DiffusionActive
        {
            get => DiffusionViewModel.DiffusionActive;
        }

        private bool _showXOR; public bool ShowXOR
        {
            get
            {
                return _showXOR;
            }
            set
            {
                _showXOR = value;
                OnPropertyChanged();
            }
        }

        #endregion Binding Properties (Diffusion)

        #region Methods

        private void ChangePage(INavigation viewModel)
        {
            CurrentPage.Teardown();
            if (!Pages.Contains(viewModel))
                Pages.Add(viewModel);

            CurrentPage = Pages
                .FirstOrDefault(vm => vm == viewModel);
            CurrentPage.Setup();
        }

        #endregion Methods

        #region IChaCha

        public ChaChaPresentationViewModel PresentationViewModel { get => this; }
        public ChaCha ChaCha { get; set; }
        public ChaChaSettings Settings { get => (ChaChaSettings)ChaCha.Settings; }

        #endregion IChaCha
    }
}