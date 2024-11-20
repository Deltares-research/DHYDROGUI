using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DelftTools.Controls;
using DelftTools.Hydro;
using Image = System.Drawing.Image;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf
{
    /// <summary>
    /// Initializes a WPF Settings view for a generic IHydroModel that gets its properties from a CSV.
    /// </summary>
    /// <seealso cref="System.Windows.Controls.UserControl"/>
    /// <seealso cref="System.Windows.Markup.IComponentConnector"/>
    /// <seealso cref="IView"/>
    public sealed partial class WpfSettingsView : IAdditionalView
    {
        private bool disposed = false;
        private NotifyPropertyChangedWpfGuiPropertySynchronizer synchronizer;

        public WpfSettingsView()
        {
            InitializeComponent();
        }

        public ObservableCollection<WpfGuiCategory> SettingsCategories
        {
            get
            {
                return ViewModel.SettingsCategories;
            }
            set
            {
                ViewModel.SettingsCategories = value;
            }
        }

        public Func<object, string, string> GetChangedPropertyName { get; set; }

        public string Text { get; set; }

        public object Data
        {
            get
            {
                return ViewModel.DataModel;
            }
            set
            {
                if (ViewModel.DataModel != null)
                {
                    ((INotifyPropertyChanged) ViewModel.DataModel).PropertyChanged -= OnDataPropertyChanged;
                    synchronizer = null;
                }

                ViewModel.DataModel = (IHydroModel) value;

                if (ViewModel.DataModel != null)
                {
                    var observable = (INotifyPropertyChanged) ViewModel.DataModel;
                    observable.PropertyChanged += OnDataPropertyChanged;
                    synchronizer = new NotifyPropertyChangedWpfGuiPropertySynchronizer(observable);
                }
            }
        }

        public Image Image { get; set; }

        public ViewInfo ViewInfo { get; set; }

        /// <summary>
        /// Sets the collection of <see cref="WpfGuiProperty"/> properties to synchronize with state changes in <see cref="Data"/>.
        /// </summary>
        /// <param name="properties">The collection of <see cref="WpfGuiProperty"/> to synchronise.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="properties"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the synchronizer is not set due to incompatible values of
        /// <see cref="Data"/>.
        /// </exception>
        public void SetSynchronizedProperties(IEnumerable<WpfGuiProperty> properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            if (synchronizer == null)
            {
                throw new InvalidOperationException("Cannot synchronize properties when private field synchronizer is null.");
            }

            synchronizer.SynchronizeProperties(properties);
        }

        /// <summary>
        /// Makes object visible in the view if possible
        /// </summary>
        /// <param name="item"></param>
        public void EnsureVisible(object item)
        {
            var settings = (WpfSettingsViewModel) DataContext;
            if (item is string && settings != null)
            {
                string tabName = (item as string).ToLowerInvariant();
                int selectedTab =
                    settings.SettingsCategories.IndexOf(settings.SettingsCategories.FirstOrDefault(c => c.CategoryName
                                                                                                         .ToLowerInvariant().Equals(tabName)));
                MainTabControl.SelectedIndex = selectedTab;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                if (ViewModel?.DataModel != null)
                {
                    ((INotifyPropertyChanged) ViewModel.DataModel).PropertyChanged -= OnDataPropertyChanged;
                }

                synchronizer?.Dispose();
                ViewModel?.Dispose();
            }

            disposed = true;
        }

        private void OnDataPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            string propertyName = GetChangedPropertyName(sender, propertyChangedEventArgs.PropertyName);
            if (string.IsNullOrEmpty(propertyName))
            {
                return;
            }

            ViewModel.UpdatePropertyValue(propertyName);
        }

        private void MainTabControlOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Fixes known issue for committing value when tab selection changes
            // https://stackoverflow.com/questions/10208861/wpf-data-bound-tabcontrol-doesnt-commit-changes-when-new-tab-is-selected

            Keyboard.FocusedElement?.RaiseEvent(new RoutedEventArgs(LostFocusEvent));
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="WpfSettingsView"/> class.
        /// </summary>
        ~WpfSettingsView()
        {
            Dispose(false);
        }
    }
}