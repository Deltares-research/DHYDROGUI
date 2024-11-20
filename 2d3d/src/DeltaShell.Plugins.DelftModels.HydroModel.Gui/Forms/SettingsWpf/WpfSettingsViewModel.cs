using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Extensions;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf
{
    /// <summary>
    /// ViewModel containing the conversion of an ObjectUIDescription (extracted CSV Properties) to a WPF Gui view.
    /// </summary>
    [Entity]
    public sealed class WpfSettingsViewModel : IDisposable
    {
        private readonly ObservableCollection<WpfGuiCategory> settingsCategories;
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="WpfSettingsViewModel"/> class.
        /// </summary>
        public WpfSettingsViewModel()
        {
            settingsCategories = new ObservableCollection<WpfGuiCategory>();
            settingsCategories.CollectionChanged += SettingsCategoriesOnCollectionChanged;

            RemovedCategories = new List<WpfGuiCategory>();
        }

        /// <summary>
        /// Gets or sets the data model.
        /// </summary>
        /// <value>
        /// The data model.
        /// </value>
        public IModel DataModel { get; set; }

        /// <summary>
        /// Gets or sets the grouped properties.
        /// </summary>
        /// <value>
        /// The grouped properties.
        /// </value>
        public ObservableCollection<WpfGuiCategory> SettingsCategories
        {
            get => settingsCategories;
            set
            {
                if (value != null)
                {
                    IEnumerable<WpfGuiCategory> visibleCategories = value.Where(cat => cat.IsVisible);
                    settingsCategories.ForEach(gp => gp.PropertyChanged -= OnPropertyChanged);
                    settingsCategories.Clear();
                    settingsCategories.AddRange(visibleCategories);

                    RemovedCategories = value.Where(cat => !cat.IsVisible).ToList();
                    settingsCategories.ForEach(gp => gp.PropertyChanged += OnPropertyChanged);
                }
            }
        }

        public void UpdatePropertyValue(string propertyName)
        {
            if (UpdatingProperties)
            {
                return;
            }

            WpfGuiProperty firstOrDefault = SettingsCategories.SelectMany(sc => sc.Properties).FirstOrDefault(p => p.Name != null && p.Name.Equals(propertyName));

            if (firstOrDefault != null)
            {
                UpdatingProperties = true;
                firstOrDefault.RaisePropertyChangedEvents();
                UpdatingProperties = false;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private IList<WpfGuiCategory> RemovedCategories { get; set; }

        private bool UpdatingProperties { get; set; }

        private void SettingsCategoriesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (settingsCategories != null && !UpdatingProperties)
            {
                UpdatingProperties = true;
                settingsCategories.ForEach(gp => gp.PropertyChanged += OnPropertyChanged);
                UpdateVisibleCategories();
                UpdatingProperties = false;
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (UpdatingProperties)
            {
                return;
            }

            UpdatingProperties = true;

            //Notify to all properties with custom controls to update.
            //For tab visibility.
            UpdateVisibleCategories();

            UpdatingProperties = false;
        }

        private void UpdateVisibleCategories()
        {
            List<WpfGuiCategory> notVisible = SettingsCategories.Where(sc => !sc.IsVisible).ToList();
            if (notVisible.Any())
            {
                RemovedCategories.AddRange(notVisible);
                //Unsubscribe removed categories.
                notVisible.ForEach(rc => rc.PropertyChanged -= OnPropertyChanged);
                SettingsCategories.RemoveAllWhere(sc => !sc.IsVisible);
            }

            if (RemovedCategories.Any(rc => rc.IsVisible))
            {
                IEnumerable<WpfGuiCategory> guiCategories = RemovedCategories.Where(rc => rc.IsVisible);
                settingsCategories.AddRange(guiCategories);
                RemovedCategories.RemoveAllWhere(rc => rc.IsVisible);
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                SettingsCategories.ForEach(c => c.PropertyChanged -= OnPropertyChanged);
                SettingsCategories.ForEach(c => c.Dispose());
                SettingsCategories.Clear();

                RemovedCategories.Clear();
            }

            disposed = true;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="WpfSettingsViewModel"/> class.
        /// </summary>
        ~WpfSettingsViewModel()
        {
            Dispose(false);
        }
    }
}