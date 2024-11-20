using System.ComponentModel;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.ComponentModel;
using DelftTools.Utils.Validation;
using DelftTools.Utils.Validation.NameValidation;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows
{
    /// <summary>
    /// Representation object of a <see cref="OutletCompartment"/> in the
    /// <see cref="DeltaShell.Plugins.SharpMapGis.Gui.Forms.VectorLayerAttributeTableView"/> (MDE).
    /// Order of the properties in the table view is equal to the order of the properties defined in this class.
    /// </summary>
    /// <seealso cref="PropertyChangedPropagator"/>
    public sealed class OutletCompartmentRow : PropertyChangedPropagator, IFeatureRowObject
    {
        private readonly OutletCompartment outletCompartment;
        private readonly NameValidator nameValidator;

        /// <summary>
        /// Initialize a new instance of the <see cref="OutletCompartmentRow"/> class.
        /// </summary>
        /// <param name="outletCompartment"> The outlet compartment to be presented. </param>
        /// <param name="nameValidator"> The name validator to use when the name is set. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="outletCompartment"/> or <paramref name="nameValidator"/> is <c>null</c>.
        /// </exception>
        public OutletCompartmentRow(OutletCompartment outletCompartment, NameValidator nameValidator)
            : base((INotifyPropertyChanged)outletCompartment)
        {
            Ensure.NotNull(outletCompartment, nameof(outletCompartment));
            Ensure.NotNull(nameValidator, nameof(nameValidator));
            
            this.outletCompartment = outletCompartment;
            this.nameValidator = nameValidator;
        }

        [DisplayName("Manhole name")]
        public string ManholeName => outletCompartment.ManholeName;

        [DisplayName("Name")]
        public string Name
        {
            get => outletCompartment.Name;
            set
            {
                if (nameValidator.ValidateWithLogging(value))
                {
                    outletCompartment.Name = value;
                }
            }
        }

        [DisplayName("Shape")]
        public CompartmentShape Shape
        {
            get => outletCompartment.Shape;
            set => outletCompartment.Shape = value;
        }

        [DisplayName("Compartment Storage Type")]
        public CompartmentStorageType CompartmentStorageType
        {
            get => outletCompartment.CompartmentStorageType;
            set => outletCompartment.CompartmentStorageType = value;
        }

        [DynamicReadOnly]
        [DisplayName("Length")]
        public double Length
        {
            get => outletCompartment.ManholeLength;
            set => outletCompartment.ManholeLength = value;
        }

        [DynamicReadOnly]
        [DisplayName("Width")]
        public double Width
        {
            get => outletCompartment.ManholeWidth;
            set => outletCompartment.ManholeWidth = value;
        }

        [DynamicReadOnly]
        [DisplayName("Floodable area")]
        public double FloodableArea
        {
            get => outletCompartment.FloodableArea;
            set => outletCompartment.FloodableArea = value;
        }

        [DynamicReadOnly]
        [DisplayName("Bottom level")]
        public double BottomLevel
        {
            get => outletCompartment.BottomLevel;
            set => outletCompartment.BottomLevel = value;
        }

        [DynamicReadOnly]
        [DisplayName("Surface level")]
        public double SurfaceLevel
        {
            get => outletCompartment.SurfaceLevel;
            set => outletCompartment.SurfaceLevel = value;
        }

        [DisplayName("Use a storage table")]
        public bool UseStorageTable
        {
            get => outletCompartment.UseTable;
            set => outletCompartment.UseTable = value;
        }

        [DynamicReadOnly]
        [DisplayName("Storage table")]
        public IFunction Storage
        {
            get => outletCompartment.Storage;
            set => outletCompartment.Storage = value;
        }

        [DisplayName("InterpolationType")]
        [DynamicReadOnly]
        public InterpolationType InterpolationType
        {
            get => outletCompartment.InterpolationType;
            set => outletCompartment.InterpolationType = value;
        }

        [DisplayName("Surface water level")]
        public double SurfaceWaterLevel
        {
            get => outletCompartment.SurfaceWaterLevel;
            set => outletCompartment.SurfaceWaterLevel = value;
        }

        /// <summary>
        /// Gets the underlying <see cref="OutletCompartment"/> feature that is represented by this instance.
        /// </summary>
        public IFeature GetFeature()
        {
            return outletCompartment;
        }

        [DynamicReadOnlyValidationMethod]
        public bool IsReadOnly(string propertyName)
        {
            if (IsReadOnlyWhenUseStorageTable(propertyName))
            {
                return UseStorageTable;
            }

            if (IsEditableWhenUseStorageTable(propertyName))
            {
                return !UseStorageTable;
            }

            return true;
        }

        private bool IsEditableWhenUseStorageTable(string propertyName)
        {
            return propertyName == nameof(Storage) || propertyName == nameof(InterpolationType);
        }

        private bool IsReadOnlyWhenUseStorageTable(string propertyName)
        {
            return propertyName == nameof(SurfaceLevel) ||
                   propertyName == nameof(FloodableArea) ||
                   propertyName == nameof(Length) ||
                   propertyName == nameof(Width) ||
                   propertyName == nameof(BottomLevel);
        }
    }
}