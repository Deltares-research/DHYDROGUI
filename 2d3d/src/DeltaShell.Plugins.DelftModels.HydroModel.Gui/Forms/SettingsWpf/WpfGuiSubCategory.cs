using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using DelftTools.Controls.Swf.DataEditorGenerator.Metadata;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf
{
    public sealed class WpfGuiSubCategory : INotifyPropertyChanged
    {
        private bool isVisible;
        private bool expanded;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="WpfGuiSubCategory"/> class.
        /// Creates new WpfGuiProperties <seealso cref="WpfGuiProperty"/>.
        /// </summary>
        /// <param name="subCategory">The sub category.</param>
        /// <param name="properties">The properties.</param>
        public WpfGuiSubCategory(string subCategory, IList<FieldUIDescription> properties)
        {
            SubCategoryName = subCategory;

            expanded = true;

            /*Small trick to force the initialization*/
            if (properties == null)
            {
                properties = new List<FieldUIDescription>();
            }

            Properties = new ObservableCollection<WpfGuiProperty>(properties.Select(p => new WpfGuiProperty(p)));
            Properties.CollectionChanged += (sender, args) => UpdateIsVisible();
            UpdateIsVisible();
        }

        /// <summary>
        /// Gets the name of the sub category.
        /// </summary>
        /// <value>
        /// The name of the sub category.
        /// </value>
        public string SubCategoryName { get; }

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
        /// Gets or sets the custom control.
        /// </summary>
        /// <value>
        /// The custom control.
        /// </value>
        public FrameworkElement CustomControl { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether this instance is visible.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsVisible
        {
            get => isVisible;
            private set
            {
                isVisible = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Updates the <see cref="IsVisible"/> property accordingly.
        /// </summary>
        public void UpdateIsVisible()
        {
            IsVisible = Properties.Any(p => p.IsVisible);
        }

        public bool Expanded
        {
            get
            {
                return expanded;
            }
            set
            {
                expanded = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <value>
        /// The properties.
        /// </value>
        public ObservableCollection<WpfGuiProperty> Properties { get; }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}