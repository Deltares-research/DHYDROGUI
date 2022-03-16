using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using DelftTools.Controls.Swf.DataEditorGenerator.Metadata;
using DelftTools.Utils.Collections;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf
{
    public sealed class WpfGuiCategory : INotifyPropertyChanged, IDisposable
    {
        private bool disposed;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="WpfGuiCategory"/> class.
        /// Creates all SubCategories <seealso cref="WpfGuiSubCategory"/> and Properties <seealso cref="WpfGuiProperty"/>.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="properties">The properties.</param>
        public WpfGuiCategory(string category, IList<FieldUIDescription> properties)
        {
            UpdatingProperties = true; /*Flag to avoid overflow*/
            CategoryName = category;

            /*Small trick to force the initialization*/
            if (properties == null)
            {
                properties = new List<FieldUIDescription>();
            }

            SubCategories = new ObservableCollection<WpfGuiSubCategory>(
                properties.GroupBy(fd => fd.SubCategory)
                          .Select(gp => new WpfGuiSubCategory(gp.Key, gp.ToList())));

            Properties = new ObservableCollection<WpfGuiProperty>(SubCategories.SelectMany(sc => sc.Properties));
            Properties.ForEach(p =>
            {
                p.PropertyChanged += OnPropertyChanged;
                if (p.IsEnumerableSymbol)
                {
                    p.GetBindedProperty = GetPropertyValueInCategory;
                }
            });
            UpdatingProperties = false; /*Flag to avoid overflow*/
        }

        /// <summary>
        /// Gets the name of the category.
        /// </summary>
        /// <value>
        /// The name of the category.
        /// </value>
        public string CategoryName { get; }

        /// <summary>
        /// Gets a value indicating whether this instance has custom control.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has custom control; otherwise, <c>false</c>.
        /// </value>
        public bool HasCustomControl
        {
            get
            {
                return CustomControl != null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is visible.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsVisible
        {
            get
            {
                return CategoryVisibility?.Invoke() ?? true;
            }
        }

        public Func<bool> CategoryVisibility { get; set; }

        /// <summary>
        /// Gets or sets the custom control.
        /// </summary>
        /// <value>
        /// The custom control.
        /// </value>
        public FrameworkElement CustomControl { get; set; }

        /// <summary>
        /// Gets the sub categories.
        /// </summary>
        /// <value>
        /// The sub categories.
        /// </value>
        public ObservableCollection<WpfGuiSubCategory> SubCategories { get; }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <value>
        /// The properties.
        /// </value>
        public ObservableCollection<WpfGuiProperty> Properties { get; }

        /// <summary>
        /// Converts a field UI description into a WpfGuiProperty <seealso cref="WpfGuiProperty"/>.
        /// Adds it to the corresponding SubCategory <seealso cref="WpfGuiSubCategory"/> or creates it if needed.
        /// </summary>
        /// <param name="uiDescription">The FieldUIDescription. <seealso cref="WpfGuiSubCategory"/></param>
        public void AddFieldUiDescription(FieldUIDescription uiDescription)
        {
            var property = new WpfGuiProperty(uiDescription);
            AddWpfGuiProperty(property);
        }

        /// <summary>
        /// Adds the WPF GUI property.
        /// </summary>
        /// <param name="property">The property.</param>
        public void AddWpfGuiProperty(WpfGuiProperty property)
        {
            //Get model from any of the properties, they should all be the same
            if (property.GetModel == null)
            {
                property.GetModel = Properties.FirstOrDefault()?.GetModel;
            }

            string subCategoryName = property.SubCategory;
            WpfGuiSubCategory subCategory = SubCategories.FirstOrDefault(sc => sc.SubCategoryName?.ToLower() == subCategoryName?.ToLower());
            if (subCategory == null)
            {
                subCategory = new WpfGuiSubCategory(subCategoryName, new List<FieldUIDescription>());
                SubCategories.Add(subCategory);
            }

            property.PropertyChanged += OnPropertyChanged;
            property.GetBindedProperty = GetPropertyValueInCategory;
            subCategory.Properties.Add(property);
            Properties.Add(property);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /* Flag to avoid Overflow exception while propagating the event PropertyChanged. */
        private bool UpdatingProperties { get; set; }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (UpdatingProperties)
            {
                return;
            }

            UpdatingProperties = true;

            //Notify to all properties within this category that they need to be updated.
            Properties.ForEach(p => p.RaisePropertyChangedEvents());
            SubCategories.ForEach(sc => sc.UpdateIsVisible());
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsVisible"));
            UpdatingProperties = false;
        }

        private WpfGuiProperty GetPropertyValueInCategory(string propertyName)
        {
            return Properties.FirstOrDefault(p => p.Name.Equals(propertyName));
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                var disposableCustomControl = CustomControl as IDisposable;
                disposableCustomControl?.Dispose();

                SubCategories.Clear();
                Properties.ForEach(p =>
                {
                    p.PropertyChanged -= OnPropertyChanged;
                    p.GetBindedProperty = null;
                });
                Properties.Clear();
            }

            disposed = true;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="WpfGuiProperty"/> class.
        /// </summary>
        ~WpfGuiCategory()
        {
            Dispose(false);
        }
    }
}