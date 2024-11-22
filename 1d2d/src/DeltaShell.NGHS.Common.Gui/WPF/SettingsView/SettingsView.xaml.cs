﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using DelftTools.Controls;
using DelftTools.Hydro;
using Image = System.Drawing.Image;

namespace DeltaShell.NGHS.Common.Gui.WPF.SettingsView
{
    /// <summary>
    /// Initializes a WPF Settings view for a generic IHydroModel that gets its properties from a CSV.
    /// </summary>
    /// <seealso cref="System.Windows.Controls.UserControl" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    /// <seealso cref="DelftTools.Controls.IView" />
    public partial class SettingsView : IAdditionalView
    {
        public SettingsView()
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

        public ObservableCollection<GuiCategory> SettingsCategories
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
            var settings = ((SettingsViewModel)DataContext);
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
        }

        private void OnDataPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            var propertyName = GetChangedPropertyName(sender, null);
            if (string.IsNullOrEmpty(propertyName))
            {
                propertyName = propertyChangedEventArgs.PropertyName;
                if(string.IsNullOrEmpty(propertyName) 
                   || Equals(propertyName, nameof(DelftTools.Utils.Editing.IEditableObject.IsEditing))
                   || Equals(propertyName, nameof(DelftTools.Utils.INameable.Name))
                   || Equals(propertyName, nameof(DelftTools.Shell.Core.Workflow.DataItems.DataItem.ComposedValue))
                   || Equals(propertyName, nameof(DelftTools.Units.Parameter.Value)))
                    return;
            }

            ViewModel.UpdatePropertyValue(propertyName);
        }
        
        // Commit the value when losing keyboard focus. 
        // This is required for situations where the textbox is not losing full focus, for example when saving your model.
        private void WpfSettingsView_Textbox_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var textbox = e.OldFocus as TextBox;
            textbox?.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        }
    }
}