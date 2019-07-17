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
    public class WpfSettingsViewModel
    {
        private ObservableCollection<WpfGuiCategory> settingsCategories;

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
            get { return settingsCategories; }
            set
            {
                if (value != null)
                {
                    settingsCategories = new ObservableCollection<WpfGuiCategory>(value.Where(cat => cat.IsVisible));
                    RemovedCategories = value.Where(cat => !cat.IsVisible).ToList();
                    settingsCategories.CollectionChanged += SettingsCategoriesOnCollectionChanged;
                    settingsCategories.ForEach(gp => gp.PropertyChanged += OnPropertyChanged);
                }
            }
        }

        public void UpdatePropertyValue(string propertyName)
        {
            if (UpdatingProperties) return;

            var firstOrDefault = SettingsCategories.SelectMany(sc => sc.Properties).FirstOrDefault(p => p.Name != null && p.Name.Equals(propertyName));

            if (firstOrDefault!= null)
            {
                UpdatingProperties = true;
                firstOrDefault.RaisePropertyChangedEvents();
                UpdatingProperties = false;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WpfSettingsViewModel"/> class.
        /// </summary>
        public WpfSettingsViewModel()
        {
            SettingsCategories = new ObservableCollection<WpfGuiCategory>(new List<WpfGuiCategory>());
            RemovedCategories = new List<WpfGuiCategory>();
        }

        private void SettingsCategoriesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (SettingsCategories != null && !UpdatingProperties)
            {
                UpdatingProperties = true;
                SettingsCategories.ForEach(gp => gp.PropertyChanged += OnPropertyChanged);
                UpdateVisibleCategories();
                UpdatingProperties = false;
            }
        }

        private IList<WpfGuiCategory> RemovedCategories { get; set; }

        private bool UpdatingProperties { get; set; }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (UpdatingProperties) return;
            UpdatingProperties = true;

            //Notify to all properties with custom controls to update.
            //For tab visibility.
            UpdateVisibleCategories();
            SettingsCategories.SelectMany( sc => sc.Properties.Where( p => p.HasCustomControl)).ForEach( p => p.RaisePropertyChangedEvents());
            
            UpdatingProperties = false;
        }

        private void UpdateVisibleCategories()
        {
            var notVisible = SettingsCategories.Where(sc => !sc.IsVisible).ToList();
            if (notVisible.Any())
            {
                RemovedCategories.AddRange(notVisible);
                //Unsubscribe removed categories.
                notVisible.ForEach(rc => rc.PropertyChanged -= OnPropertyChanged);
                SettingsCategories.RemoveAllWhere(sc => !sc.IsVisible);
            }

            if (RemovedCategories.Any(rc => rc.IsVisible))
            {
                var guiCategories = RemovedCategories.Where(rc => rc.IsVisible);
                SettingsCategories.AddRange(guiCategories);
                RemovedCategories.RemoveAllWhere(rc => rc.IsVisible);
            }
        }
    }
}