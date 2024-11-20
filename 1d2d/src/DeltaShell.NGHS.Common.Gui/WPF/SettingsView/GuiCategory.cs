using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using DelftTools.Controls.Swf.DataEditorGenerator.Metadata;
using DelftTools.Utils.Collections;

namespace DeltaShell.NGHS.Common.Gui.WPF.SettingsView
{
    public class GuiCategory : INotifyPropertyChanged
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GuiCategory"/> class. 
        /// Creates all SubCategories <seealso cref="GuiSubCategory"/> and Properties <seealso cref="GuiProperty"/>.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="properties">The properties.</param>
        public GuiCategory(string category, IList<FieldUIDescription> properties)
        {
            UpdatingProperties = true; /*Flag to avoid overflow*/
            CategoryName = category;

            /*Small trick to force the initialization*/
            if (properties == null)
                properties = new List<FieldUIDescription>();

            SubCategories = new ObservableCollection<GuiSubCategory>(
                properties.GroupBy(fd => fd.SubCategory)
                    .Select(gp => new GuiSubCategory(gp.Key, gp.ToList())));
            Properties.ForEach(p => p.PropertyChanged += OnPropertyChanged);

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
        ///   <c>true</c> if this instance has custom control; otherwise, <c>false</c>.
        /// </value>
        public bool HasCustomControl { get { return CustomControl != null; } }

        /// <summary>
        /// Gets a value indicating whether this instance is visible.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is visible; otherwise, <c>false</c>.
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
        public ObservableCollection<GuiSubCategory> SubCategories { get; }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <value>
        /// The properties.
        /// </value>
        public ObservableCollection<GuiProperty> Properties
        {
            get
            {
                return new ObservableCollection<GuiProperty>(SubCategories.SelectMany(sc => sc.Properties));
            }
        }

        /// <summary>
        /// Converts a field UI description into a WpfGuiProperty <seealso cref="GuiProperty"/>.
        /// Adds it to the corresponding SubCategory <seealso cref="GuiSubCategory"/> or creates it if needed.
        /// </summary>
        /// <param name="uiDescription">The FieldUIDescription. <seealso cref="GuiSubCategory"/></param>
        public void AddFieldUiDescription(FieldUIDescription uiDescription)
        {
            var property = new GuiProperty(uiDescription);
            AddWpfGuiProperty(property);
        }

        /// <summary>
        /// Adds the WPF GUI property.
        /// </summary>
        /// <param name="property">The property.</param>
        public void AddWpfGuiProperty(GuiProperty property)
        {
            //Get model from any of the properties, they should all be the same
            if (property.GetModel == null)
                property.GetModel = Properties.FirstOrDefault()?.GetModel;

            var subCategoryName = property.SubCategory;
            var subCategory = SubCategories.FirstOrDefault(sc => sc.SubCategoryName?.ToLower() == subCategoryName?.ToLower());
            if (subCategory == null)
            {
                subCategory = new GuiSubCategory(subCategoryName, new List<FieldUIDescription>());
                SubCategories.Add(subCategory);
            }
            subCategory.Properties.Add(property);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /* Flag to avoid Overflow exception while propagting the event PropertyChanged. */
        private bool UpdatingProperties { get; set; }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (UpdatingProperties) return;
            UpdatingProperties = true;

            //Notify to all properties within this category that they need to be updated.
            Properties.ForEach(p => p.RaisePropertyChangedEvents());
            SubCategories.ForEach(sc => sc.RefreshIsVisible());
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsVisible"));
            UpdatingProperties = false;
        }
    }

}