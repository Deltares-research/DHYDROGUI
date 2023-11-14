using System.ComponentModel;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Utils.ComponentModel;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows
{
    /// <summary>
    /// Representation object of a <see cref="IRetention"/> in the
    /// <see cref="DeltaShell.Plugins.SharpMapGis.Gui.Forms.VectorLayerAttributeTableView"/> (MDE).
    /// Order of the properties in the table view is equal to the order of the properties defined in this class.
    /// </summary>
    /// <seealso cref="PropertyChangedPropagator"/>
    public sealed class RetentionRow : PropertyChangedPropagator, IFeatureRowObject
    {
        private readonly IRetention retention;

        /// <summary>
        /// Initialize a new instance of the <see cref="RetentionRow"/> class.
        /// </summary>
        /// <param name="retention"> The retention to be presented. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="retention"/> is <c>null</c>.
        /// </exception>
        public RetentionRow(IRetention retention)
            : base((INotifyPropertyChanged)retention)
        {
            Ensure.NotNull(retention, nameof(retention));
            this.retention = retention;
        }

        [DisplayName("Name")]
        public string Name
        {
            get => retention.Name;
            set => retention.SetNameIfValid(value);
        }

        [DisplayName("Long name")]
        public string LongName
        {
            get => retention.LongName;
            set => retention.LongName = value;
        }

        [DisplayName("Branch")]
        public string Branch => retention.Branch.Name;

        [DisplayName("Chainage")]
        public double Chainage
        {
            get => retention.Chainage;
            set => retention.Chainage = value;
        }

        [DisplayName("Type")]
        public RetentionType Type
        {
            get => retention.Type;
            set => retention.Type = value;
        }

        [DisplayName("Storage area")]
        [DynamicReadOnly]
        public double StorageArea
        {
            get => retention.StorageArea;
            set => retention.StorageArea = value;
        }

        [DisplayName("Bed level")]
        [DynamicReadOnly]
        public double BedLevel
        {
            get => retention.BedLevel;
            set => retention.BedLevel = value;
        }

        [DisplayName("Use a storage table")]
        public bool UseTable
        {
            get => retention.UseTable;
            set => retention.UseTable = value;
        }

        [DynamicReadOnly]
        [DisplayName("Storage table")]
        public IFunction Data
        {
            get => retention.Data;
            set => retention.Data = value;
        }

        [DisplayName("InterpolationType")]
        [DynamicReadOnly]
        public InterpolationType InterpolationType
        {
            get => retention.InterpolationType;
            set => retention.InterpolationType = value;
        }

        /// <summary>
        /// Gets the underlying <see cref="IRetention"/> feature that is represented by this instance.
        /// </summary>
        public IFeature GetFeature()
        {
            return retention;
        }

        [DynamicReadOnlyValidationMethod]
        public bool IsReadOnly(string propertyName)
        {
            if (propertyName == nameof(StorageArea) || propertyName == nameof(BedLevel))
            {
                return UseTable;
            }

            if (propertyName == nameof(Data) || propertyName == nameof(InterpolationType))
            {
                return !UseTable;
            }

            return true;
        }
    }
}