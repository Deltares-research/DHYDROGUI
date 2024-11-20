using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.ViewModels
{
    /// <summary>
    /// View model for the main domain specific editor view
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged"/>
    /// <seealso cref="System.IDisposable"/>
    public sealed class MainDomainSpecificDataViewModel : INotifyPropertyChanged, IDisposable
    {
        private bool disposed = false;
        private ObservableCollection<DomainSpecificSettingsViewModel> domainSpecificDataViewModelsList = new ObservableCollection<DomainSpecificSettingsViewModel>();
        private DomainSpecificSettingsViewModel selectedViewModel;
        private IWaveDomainData rootDomain;

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Constructor for setting the RootDomain
        /// </summary>
        /// <param name="outerDomain"></param>
        public MainDomainSpecificDataViewModel(IWaveDomainData outerDomain)
        {
            RootDomain = outerDomain;
            SelectedViewModel = DomainSpecificDataViewModelsList.FirstOrDefault();
        }

        /// <summary>
        /// Gets or sets the domain specific data view models list.
        /// </summary>
        /// <value>
        /// The domain specific data view models list.
        /// </value>
        public ObservableCollection<DomainSpecificSettingsViewModel> DomainSpecificDataViewModelsList
        {
            get => domainSpecificDataViewModelsList;
            set
            {
                domainSpecificDataViewModelsList = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the selected view model of the view.
        /// </summary>
        /// <value>
        /// The selected view model.
        /// </value>
        public DomainSpecificSettingsViewModel SelectedViewModel
        {
            get => selectedViewModel;
            set
            {
                selectedViewModel = value;
                OnPropertyChanged();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private IWaveDomainData RootDomain
        {
            get => rootDomain;
            set
            {
                if (rootDomain == value)
                {
                    return;
                }

                if (rootDomain != null)
                {
                    UnSubscribe();
                }

                rootDomain = value;
                Subscribe();
                Update(rootDomain);
            }
        }

        private void UnSubscribe()
        {
            ((INotifyPropertyChanged) RootDomain).PropertyChanged -= DomainsPropertyChanged;
            RootDomain.SubDomains.CollectionChanged -= DomainsCollectionChanged;
        }

        private void Subscribe()
        {
            ((INotifyPropertyChanged) RootDomain).PropertyChanged += DomainsPropertyChanged;
            RootDomain.SubDomains.CollectionChanged += DomainsCollectionChanged;
        }

        private void DomainsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is IWaveDomainData waveDomainData && e.PropertyName == nameof(IWaveDomainData.SuperDomain))
            {
                // Removing exterior domain
                if (waveDomainData.SuperDomain == null && RootDomain.SubDomains.Contains(waveDomainData))
                {
                    RootDomain = waveDomainData;
                    return;
                }

                // Adding new exterior domain
                if (DomainSpecificDataViewModelsList.All(vm => vm.DomainName != waveDomainData.SuperDomain.Name))
                {
                    RootDomain = waveDomainData.SuperDomain;
                }
            }
        }

        private void DomainsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Adding/Removing subdomains
            Update(RootDomain);
        }

        /// <summary>
        /// Recreates the sub view models list when an event is fired in the root domain data.
        /// </summary>
        /// <param name="superDomain">The super domain.</param>
        private void Update(IWaveDomainData superDomain)
        {
            if (superDomain == null)
            {
                return;
            }

            List<DomainSpecificSettingsViewModel> viewModelsList = CreateNewDomainSpecificSettingsViewModels();

            DomainSpecificSettingsViewModel selectedViewModelInNewList = FindSelectedViewModelInNewList(viewModelsList);

            DomainSpecificDataViewModelsList = new ObservableCollection<DomainSpecificSettingsViewModel>(viewModelsList);

            SelectedViewModel = selectedViewModelInNewList ?? DomainSpecificDataViewModelsList.FirstOrDefault();
        }

        private void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private DomainSpecificSettingsViewModel FindSelectedViewModelInNewList(List<DomainSpecificSettingsViewModel> viewModelsList)
        {
            DomainSpecificSettingsViewModel selectedViewModelInNewList = null;
            if (SelectedViewModel != null)
            {
                selectedViewModelInNewList =
                    viewModelsList.FirstOrDefault(
                        vm => vm.DomainName == SelectedViewModel.DomainName);
            }

            return selectedViewModelInNewList;
        }

        private List<DomainSpecificSettingsViewModel> CreateNewDomainSpecificSettingsViewModels()
        {
            IList<IWaveDomainData> allDomains = WaveDomainHelper.GetAllDomains(rootDomain);
            return allDomains.Select(domain => new DomainSpecificSettingsViewModel(domain)).ToList();
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                UnSubscribe();
            }

            disposed = true;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="MainDomainSpecificDataViewModel"/> class.
        /// </summary>
        ~MainDomainSpecificDataViewModel()
        {
            Dispose(false);
        }
    }
}