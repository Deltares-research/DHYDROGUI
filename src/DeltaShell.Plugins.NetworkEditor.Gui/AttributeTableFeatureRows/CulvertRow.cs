using System.ComponentModel;
using DelftTools.Hydro;
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
    /// Representation object of a <see cref="ICulvert"/> in the
    /// <see cref="DeltaShell.Plugins.SharpMapGis.Gui.Forms.VectorLayerAttributeTableView"/> (MDE).
    /// Order of the properties in the table view is equal to the order of the properties defined in this class.
    /// </summary>
    /// <seealso cref="PropertyChangedPropagator"/>
    public sealed class CulvertRow : PropertyChangedPropagator, IFeatureRowObject
    {
        private readonly ICulvert culvert;
        private readonly NameValidator nameValidator;

        /// <summary>
        /// Initialize a new instance of the <see cref="CulvertRow"/> class.
        /// </summary>
        /// <param name="culvert"> The culvert to be presented. </param>
        /// <param name="nameValidator"> The name validator to use when the name is set. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="culvert"/> or <paramref name="nameValidator"/> is <c>null</c>.
        /// </exception>
        public CulvertRow(ICulvert culvert, NameValidator nameValidator)
            : base((INotifyPropertyChanged)culvert)
        {
            Ensure.NotNull(culvert, nameof(culvert));
            Ensure.NotNull(nameValidator, nameof(nameValidator));
            
            this.culvert = culvert;
            this.nameValidator = nameValidator;
        }

        [DisplayName("Name")]
        public string Name
        {
            get => culvert.Name;
            set
            {
                if (nameValidator.ValidateWithLogging(value))
                {
                    culvert.Name = value;
                }
            }
        }

        [DisplayName("Long name")]
        public string LongName
        {
            get => culvert.LongName;
            set => culvert.LongName = value;
        }

        [DisplayName("Branch")]
        public string Branch => culvert.Branch.Name;

        [DisplayName("Length")]
        public double Length
        {
            get => culvert.Length;
            set => culvert.Length = value;
        }

        [DisplayName("Roughness type")]
        public CulvertFrictionType RoughnessType
        {
            get => culvert.FrictionType;
            set => culvert.FrictionType = value;
        }

        [DisplayName("Roughness")]
        public double Roughness
        {
            get => culvert.Friction;
            set => culvert.Friction = value;
        }

        [DisplayName("Inlet level")]
        public double InletLevel
        {
            get => culvert.InletLevel;
            set => culvert.InletLevel = value;
        }

        [DisplayName("Outlet level")]
        public double OutletLevel
        {
            get => culvert.OutletLevel;
            set => culvert.OutletLevel = value;
        }

        [DisplayName("Inlet loss coefficient")]
        public double InletLossCoefficient
        {
            get => culvert.InletLossCoefficient;
            set => culvert.InletLossCoefficient = value;
        }

        [DisplayName("Outlet loss coefficient")]
        public double OutletLossCoefficient
        {
            get => culvert.OutletLossCoefficient;
            set => culvert.OutletLossCoefficient = value;
        }

        [DynamicReadOnly]
        [DisplayName("Bend loss coefficient")]
        public double BendLossCoefficient
        {
            get => culvert.BendLossCoefficient;
            set => culvert.BendLossCoefficient = value;
        }

        [DisplayName("Flow direction")]
        public FlowDirection FlowDirection
        {
            get => culvert.FlowDirection;
            set => culvert.FlowDirection = value;
        }

        [DisplayName("Gated")]
        public bool Gated
        {
            get => culvert.IsGated;
            set => culvert.IsGated = value;
        }

        [DynamicReadOnly]
        [DisplayName("Gate opening")]
        public double GateOpening
        {
            get => culvert.GateInitialOpening;
            set => culvert.GateInitialOpening = value;
        }

        [DynamicReadOnly]
        [DisplayName("Gate lower edge")]
        public double GateLowerEdge => culvert.GateLowerEdgeLevel;

        [DisplayName("Sub type")]
        public CulvertType SubType
        {
            get => culvert.CulvertType;
            set => culvert.CulvertType = value;
        }

        [DisplayName("Shape")]
        public CulvertGeometryType Shape
        {
            get => culvert.GeometryType;
            set => culvert.GeometryType = value;
        }

        [DisplayName("Width")]
        [DynamicReadOnly]
        public double Width
        {
            get => culvert.Width;
            set => culvert.Width = value;
        }

        [DisplayName("Height")]
        [DynamicReadOnly]
        public double Height
        {
            get => culvert.Height;
            set => culvert.Height = value;
        }

        [DynamicReadOnly]
        [DisplayName("Arc height")]
        public double ArcHeight
        {
            get => culvert.ArcHeight;
            set => culvert.ArcHeight = value;
        }

        [DynamicReadOnly]
        [DisplayName("Diameter")]
        public double Diameter
        {
            get => culvert.Diameter;
            set => culvert.Diameter = value;
        }

        [DynamicReadOnly]
        [DisplayName("Radius")]
        public double Radius
        {
            get => culvert.Radius;
            set => culvert.Radius = value;
        }

        [DynamicReadOnly]
        [DisplayName("Radius 1")]
        public double Radius1
        {
            get => culvert.Radius1;
            set => culvert.Radius1 = value;
        }

        [DynamicReadOnly]
        [DisplayName("Radius 2")]
        public double Radius2
        {
            get => culvert.Radius2;
            set => culvert.Radius2 = value;
        }

        [DynamicReadOnly]
        [DisplayName("Radius 3")]
        public double Radius3
        {
            get => culvert.Radius3;
            set => culvert.Radius3 = value;
        }

        [DynamicReadOnly]
        [DisplayName("Angle")]
        public double Angle
        {
            get => culvert.Angle;
            set => culvert.Angle = value;
        }

        [DynamicReadOnly]
        [DisplayName("Angle 1")]
        public double Angle1
        {
            get => culvert.Angle1;
            set => culvert.Angle1 = value;
        }

        /// <summary>
        /// Gets the underlying <see cref="ICulvert"/> feature that is represented by this instance.
        /// </summary>
        public IFeature GetFeature()
        {
            return culvert;
        }

        [DynamicReadOnlyValidationMethod]
        public bool IsReadOnly(string propertyName)
        {
            if (propertyName == nameof(GateOpening))
            {
                return !Gated;
            }

            if (propertyName == nameof(Width))
            {
                return Shape == CulvertGeometryType.SteelCunette ||
                       Shape == CulvertGeometryType.Tabulated ||
                       Shape == CulvertGeometryType.Round;
            }

            if (propertyName == nameof(Height))
            {
                return Shape == CulvertGeometryType.Tabulated || Shape == CulvertGeometryType.Round;
            }

            if (propertyName == nameof(ArcHeight))
            {
                return Shape != CulvertGeometryType.Arch;
            }

            if (propertyName == nameof(Diameter))
            {
                return Shape != CulvertGeometryType.Round;
            }

            if (propertyName == nameof(Radius) || propertyName == nameof(Radius1) || propertyName == nameof(Radius2) || propertyName == nameof(Radius3) ||
                propertyName == nameof(Angle) || propertyName == nameof(Angle1))
            {
                return Shape != CulvertGeometryType.SteelCunette;
            }

            if (propertyName == nameof(BendLossCoefficient))
            {
                return SubType != CulvertType.InvertedSiphon;
            }

            return false;
        }
    }
}