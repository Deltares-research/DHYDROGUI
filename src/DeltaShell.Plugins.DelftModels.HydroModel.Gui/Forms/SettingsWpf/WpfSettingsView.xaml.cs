using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow;
using Image = System.Drawing.Image;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf
{
    /// <summary>
    /// Initializes a WPF Settings view for a generic IHydroModel that gets its properties from a CSV.
    /// </summary>
    /// <seealso cref="System.Windows.Controls.UserControl" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    /// <seealso cref="DelftTools.Controls.IView" />
    public partial class WpfSettingsView : IView, IAdditionalView
    {
        public WpfSettingsView()
        {
            InitializeComponent();
        }

        public string Text { get; set; }

        public bool Visible { get; }

        public object Data
        {
            get { return ViewModel.DataModel; }
            set
            {
                if (ViewModel.DataModel != null)
                {
                    ((INotifyPropertyChanged)ViewModel.DataModel).PropertyChanged -= OnDataPropertyChanged;
                }

                ViewModel.DataModel = (IHydroModel)value;

                if (ViewModel.DataModel != null)
                {
                    ((INotifyPropertyChanged)ViewModel.DataModel).PropertyChanged += OnDataPropertyChanged;
                }
            }
        }

        public ObservableCollection<WpfGuiCategory> SettingsCategories
        {
            get { return ViewModel.SettingsCategories; }
            set { ViewModel.SettingsCategories = value; }
        }

        public Image Image
        {
            get; set;
        }
        public ViewInfo ViewInfo { get; set; }

        public Func<object, string, string> GetChangedPropertyName { get; set; }

        /// <summary>
        /// Makes object visible in the view if possible
        /// </summary>
        /// <param name="item"></param>
        public void EnsureVisible(object item)
        {
            var settings = ((WpfSettingsViewModel)DataContext);
            if (item is string && settings != null)
            {
                var tabName = (item as string).ToLowerInvariant();
                var selectedTab =
                    settings.SettingsCategories.IndexOf(settings.SettingsCategories.FirstOrDefault(c => c.CategoryName
                      .ToLowerInvariant().Equals(tabName)));
                MainTabControl.SelectedIndex = selectedTab;
            }
        }

        public void Dispose()
        {
            IModel model = ViewModel.DataModel;
            if (model != null)
            {
                ((INotifyPropertyChanged)model).PropertyChanged -= OnDataPropertyChanged;
            }

            ViewModel.Dispose();
        }

        private void OnDataPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            var propertyName = GetChangedPropertyName(sender, propertyChangedEventArgs.PropertyName);
            if (string.IsNullOrEmpty(propertyName)) return;

            ViewModel.UpdatePropertyValue(propertyName);
        }

        private void MainTabControlOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Fixes known issue for committing value when tab selection changes
            // https://stackoverflow.com/questions/10208861/wpf-data-bound-tabcontrol-doesnt-commit-changes-when-new-tab-is-selected

            Keyboard.FocusedElement?.RaiseEvent(new RoutedEventArgs(LostFocusEvent));
        }
    }
}