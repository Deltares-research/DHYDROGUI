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
    /// Representation object of a <see cref="Compartment"/> in the
    /// <see cref="DeltaShell.Plugins.SharpMapGis.Gui.Forms.VectorLayerAttributeTableView"/> (MDE).
    /// Order of the properties in the table view is equal to the order of the properties defined in this class.
    /// </summary>
    /// <seealso cref="PropertyChangedPropagator"/>
    public sealed class CompartmentRow : PropertyChangedPropagator, IFeatureRowObject
    {
        private readonly Compartment compartment;
        private readonly NameValidator nameValidator;

        /// <summary>
        /// Initialize a new instance of the <see cref="CompartmentRow"/> class.
        /// </summary>
        /// <param name="compartment"> The compartment to be presented. </param>
        /// <param name="nameValidator"> The name validator to use when the name is set. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="compartment"/> or <paramref name="nameValidator"/> is <c>null</c>.
        /// </exception>
        public CompartmentRow(Compartment compartment, NameValidator nameValidator)
            : base((INotifyPropertyChanged)compartment)
        {
            Ensure.NotNull(compartment, nameof(compartment));
            Ensure.NotNull(nameValidator, nameof(nameValidator));
            
            this.compartment = compartment;
            this.nameValidator = nameValidator;
        }

        [DisplayName("Manhole name")]
        public string ManholeName => compartment.ManholeName;

        [DisplayName("Name")]
        public string Name
        {
            get => compartment.Name;
            set
            {
                if (nameValidator.ValidateWithLogging(value))
                {
                    compartment.Name = value;
                }
            }
        }

        [DisplayName("Shape")]
        public CompartmentShape Shape
        {
            get => compartment.Shape;
            set => compartment.Shape = value;
        }

        [DisplayName("Compartment Storage Type")]
        public CompartmentStorageType CompartmentStorageType
        {
            get => compartment.CompartmentStorageType;
            set => compartment.CompartmentStorageType = value;
        }

        [DynamicReadOnly]
        [DisplayName("Length")]
        public double Length
        {
            get => compartment.ManholeLength;
            set => compartment.ManholeLength = value;
        }

        [DynamicReadOnly]
        [DisplayName("Width")]
        public double Width
        {
            get => compartment.ManholeWidth;
            set => compartment.ManholeWidth = value;
        }

        [DynamicReadOnly]
        [DisplayName("Floodable area")]
        public double FloodableArea
        {
            get => compartment.FloodableArea;
            set => compartment.FloodableArea = value;
        }

        [DynamicReadOnly]
        [DisplayName("Bottom level")]
        public double BottomLevel
        {
            get => compartment.BottomLevel;
            set => compartment.BottomLevel = value;
        }

        [DynamicReadOnly]
        [DisplayName("Surface level")]
        public double SurfaceLevel
        {
            get => compartment.SurfaceLevel;
            set => compartment.SurfaceLevel = value;
        }

        [DisplayName("Use a storage table")]
        public bool UseStorageTable
        {
            get => compartment.UseTable;
            set => compartment.UseTable = value;
        }

        [DynamicReadOnly]
        [DisplayName("Storage table")]
        public IFunction Storage
        {
            get => compartment.Storage;
            set => compartment.Storage = value;
        }

        [DisplayName("InterpolationType")]
        [DynamicReadOnly]
        public InterpolationType InterpolationType
        {
            get => compartment.InterpolationType;
            set => compartment.InterpolationType = value;
        }

        /// <summary>
        /// Gets the underlying <see cref="Compartment"/> feature that is represented by this instance.
        /// </summary>
        public IFeature GetFeature()
        {
            return compartment;
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