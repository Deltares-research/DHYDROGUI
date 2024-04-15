using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Validation.Common;
using DelftTools.Utils.Validation.NameValidation;
using DeltaShell.Plugins.CommonTools.Gui.Property;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "CulvertProperties_DisplayName")]
    public class CulvertProperties : ObjectProperties<ICulvert>
    {
        private NameValidator nameValidator = NameValidator.CreateDefault();
        
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Name")]
        [PropertyOrder(0)]
        public string Name
        {
            get { return data.Name; }
            set
            {
                if (nameValidator.ValidateWithLogging(value))
                {
                    data.Name = value;
                }
            }
        }
        
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Long name")]
        [PropertyOrder(1)]
        public string LongName
        {
            get { return data.LongName; }
            set { data.LongName = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [Description("Length of the culvert (along the branch).")]
        [DisplayName("Length")]
        [PropertyOrder(2)]
        [DynamicVisible]
        public double Length
        {
            get { return data.Length; }
            set { data.Length = value; }
        }

        [Category(PropertyWindowCategoryHelper.ShapeCategory)]
        [Description("Width of the culvert.")]
        [DisplayName("Width")]
        [PropertyOrder(3)]
        [DynamicReadOnly]
        [DynamicVisible]
        public double Width
        {
            get { return data.Width; }
            set { data.Width = value; }
        }

        [Category(PropertyWindowCategoryHelper.ShapeCategory)]
        [DisplayName("Height")]
        [Description("Height of the culvert.")]
        [PropertyOrder(4)]
        [DynamicReadOnly]
        [DynamicVisible]
        public double Height
        {
            get { return data.Height; }
            set { data.Height = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Roughness type")]
        [Description("Roughness type.")]
        [PropertyOrder(5)]
        public CulvertFrictionType FrictionType
        {
            get { return data.FrictionType; }
            set
            {
                if (data.FrictionType != value)
                {
                    data.FrictionType = value;
                    data.Friction = RoughnessHelper.GetDefault(value);
                }
            }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Roughness")]
        [Description("Roughness.")]
        [PropertyOrder(6)]
        public double Friction
        {
            get { return data.Friction; }
            set { data.Friction = value; }
        }
        
        [Category(PropertyWindowCategoryHelper.GroundLayerCategory)]
        [DisplayName("Ground layer roughness")]
        [Description("Ground layer roughness.")]
        [PropertyOrder(6)]
        [DynamicReadOnly]
        [Browsable(false)]// not browsable because not yet implemented in kernel
        public double GroundlayerRoughness
        {
            get { return data.GroundLayerRoughness; }
            set { data.GroundLayerRoughness = value; }
        }

        [Category(PropertyWindowCategoryHelper.GroundLayerCategory)]
        [DisplayName("Ground layer thickness")]
        [Description("Ground layer thickness.")]
        [PropertyOrder(6)]
        [DynamicReadOnly]
        [Browsable(false)]// not browsable because not yet implemented in kernel
        public double GroundlayerThickness
        {
            get { return data.GroundLayerThickness; }
            set { data.GroundLayerThickness = value; }
        }

        [Category(PropertyWindowCategoryHelper.GroundLayerCategory)]
        [DisplayName("Ground layer enabled")]
        [Description("Ground layer enabled.")]
        [PropertyOrder(6)]
        [Browsable(false)]// not browsable because not yet implemented in kernel
        public bool GroundlayerEnabled
        {
            get { return data.GroundLayerEnabled; }
            set { data.GroundLayerEnabled = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Allow negative flow")]
        [Description("Is negative flow possible.")]
        [PropertyOrder(7)]
        [DynamicReadOnly]
        public bool AllowNegativeFlow
        {
            get { return data.AllowNegativeFlow; }
            set { data.AllowNegativeFlow = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Allow positive flow")]
        [Description("Is positive flow possible.")]
        [PropertyOrder(8)]
        public bool AllowPositiveFlow
        {
            get { return data.AllowPositiveFlow; }
            set { data.AllowPositiveFlow = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Sub type")]
        [Description("The culvert can be inverted siphon or culvert.")]
        [PropertyOrder(11)]
        public CulvertType CulvertType
        {
            get { return data.CulvertType; }
            set { data.CulvertType = value; }
        }
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Attributes")]
        [Description("All the (custom) attributes for this object.")]
        [PropertyOrder(99)]
        [TypeConverter(typeof(AttributeArrayConverter<object>))]
        public AttributeProperties<object>[] Attributes
        {
            get { return data.Attributes.Select(x => new AttributeProperties<object>(data.Attributes, x.Key)).ToArray(); }
        }

        [Category(PropertyWindowCategoryHelper.CalculationCategory)]
        [DisplayName("Inlet loss coefficient")]
        [Description("Inlet loss.")]
        [PropertyOrder(12)]
        public double InletLoss
        {
            get { return data.InletLossCoefficient; }
            set { data.InletLossCoefficient = value; }
        }

        [Category(PropertyWindowCategoryHelper.CalculationCategory)]
        [DisplayName("Inlet level")]
        [Description("Inlet level.")]
        [PropertyOrder(13)]
        public double InletLevel
        {
            get { return data.InletLevel; }
            set { data.InletLevel = value; }
        }

        [Category(PropertyWindowCategoryHelper.CalculationCategory)]
        [DisplayName("Outlet loss coefficient")]
        [Description("Outlet loss.")]
        [PropertyOrder(14)]
        public double OutletLoss
        {
            get { return data.OutletLossCoefficient; }
            set { data.OutletLossCoefficient = value; }
        }

        [Category(PropertyWindowCategoryHelper.CalculationCategory)]
        [DisplayName("Outlet level")]
        [Description("Outlet level.")]
        [PropertyOrder(15)]
        public double OutletLevel
        {
            get { return data.OutletLevel; }
            set { data.OutletLevel = value; }
        }

        [Category(PropertyWindowCategoryHelper.CalculationCategory)]
        [DisplayName("Bend loss coefficient")]
        [Description("BendLoss coefficient of the culvert.")]
        [PropertyOrder(16)]
        [DynamicReadOnly]
        public double BendLossCoefficient
        {
            get { return data.BendLossCoefficient; }
            set { data.BendLossCoefficient = value; }
        }

        [Category(PropertyWindowCategoryHelper.AdministrationCategory)]
        [DisplayName("Branch")]
        [Description("Channel in which the compound structure is located.")]
        [PropertyOrder(17)]
        public string Channel
        {
            get { return data.Branch.ToString(); }
        }

        [Category(PropertyWindowCategoryHelper.AdministrationCategory)]
        [DisplayName("Compound structure")]
        [Description("Compound structure in which the structure is located.")]
        [PropertyOrder(18)]
        public string CompositeStructure
        {
            get { return data.ParentStructure.ToString(); }
        }

        [Category(PropertyWindowCategoryHelper.AdministrationCategory)]
        [DisplayName("Chainage (map)")]
        [Description("Chainage of the culvert in the channel on the map.")]
        [PropertyOrder(19)]
        public double Chainage
        {
            get { return NetworkHelper.MapChainage(data.ParentStructure); }
        }

        [Category(PropertyWindowCategoryHelper.AdministrationCategory)]
        [DisplayName("Chainage")]
        [Description("Chainage of the culvert in the channel as used in the simulation.")]
        [PropertyOrder(20)]
        public double CompuChainage
        {
            get { return data.ParentStructure.Chainage; }
            set { HydroRegionEditorHelper.MoveBranchFeatureTo(data, value); }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Y offset")]
        [Description("OffsetY of the weir in the cross section profile.")]
        [PropertyOrder(21)]
        public string YOffSet
        {
            get { return string.Format("{0:0.##}", data.OffsetY); }
            set { data.OffsetY = double.Parse(value); }
        }

        [Category(PropertyWindowCategoryHelper.CalculationCategory)]
        [DisplayName("Is gated")]
        [PropertyOrder(22)]
        public bool IsGated
        {
            get { return data.IsGated; }
            set { data.IsGated = value; }
        }

        [Category(PropertyWindowCategoryHelper.CalculationCategory)]
        [DisplayName("Gate initial opening")]
        [Description("Initial opening of the gate.")]
        [PropertyOrder(23)]
        [DynamicReadOnly]
        public double GateInitialOpening
        {
            get { return data.GateInitialOpening; }
            set { data.GateInitialOpening = value; }
        }

        [Category(PropertyWindowCategoryHelper.CalculationCategory)]
        [DisplayName("Gate lower edge")]
        [Description("Initial opening of the gate.")]
        [PropertyOrder(24)]
        public double GateLowerEdgeLevel
        {
            get { return data.GateLowerEdgeLevel; }
        }
        
        [DisplayName("Shape")]
        [Category(PropertyWindowCategoryHelper.ShapeCategory)]
        [PropertyOrder(1)]
        public CulvertGeometryType GeometryType
        {
            get { return data.GeometryType; }
            set { data.GeometryType = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Arc height")]
        [Description("ArcHeight of the culvert (if shape arch).")]
        [Category(PropertyWindowCategoryHelper.ShapeCategory)]
        [PropertyOrder(25)]
        [DynamicVisible]
        public double ArcHeight
        {
            get { return data.ArcHeight; }
            set { data.ArcHeight = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Diameter")]
        [Description("Diameter of the culvert (if shape round).")]
        [Category(PropertyWindowCategoryHelper.ShapeCategory)]
        [PropertyOrder(25)]
        [DynamicVisible]
        public double Diameter
        {
            get { return data.Diameter; }
            set { data.Diameter = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Radius")]
        [Description("Radius of the culvert (if shape steel cunette).")]
        [Category(PropertyWindowCategoryHelper.ShapeCategory)]
        [PropertyOrder(25)]
        [DynamicVisible]
        public double Radius
        {
            get { return data.Radius; }
            set { data.Radius = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Radius1")]
        [Description("Radius1 of the culvert (if shape steel cunette).")]
        [Category(PropertyWindowCategoryHelper.ShapeCategory)]
        [PropertyOrder(25)]
        [DynamicVisible]
        public double Radius1
        {
            get { return data.Radius1; }
            set { data.Radius1 = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Radius2")]
        [Description("Radius2 of the culvert (if shape steel cunette).")]
        [Category(PropertyWindowCategoryHelper.ShapeCategory)]
        [PropertyOrder(25)]
        [DynamicVisible]
        public double Radius2
        {
            get { return data.Radius2; }
            set { data.Radius2 = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Radius3")]
        [Description("Radius3 of the culvert (if shape steel cunette).")]
        [Category(PropertyWindowCategoryHelper.ShapeCategory)]
        [PropertyOrder(25)]
        [DynamicVisible]
        public double Radius3
        {
            get { return data.Radius3; }
            set { data.Radius3 = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Angle")]
        [Description("Angle of the culvert (if shape steel cunette).")]
        [Category(PropertyWindowCategoryHelper.ShapeCategory)]
        [PropertyOrder(25)]
        [DynamicVisible]
        public double Angle
        {
            get { return data.Angle; }
            set { data.Angle = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Angle1")]
        [Description("Angle1 of the culvert (if shape steel cunette).")]
        [Category(PropertyWindowCategoryHelper.ShapeCategory)]
        [PropertyOrder(25)]
        [DynamicVisible]
        public double Angle1
        {
            get { return data.Angle1; }
            set { data.Angle1 = value; }
        }

        [Category("Cross section properties")]
        [PropertyOrder(26)]
        [DisplayName("Closed profile")]
        [DynamicVisible]
        public bool Closed
        {
            set { data.Closed = value; }
            get { return data.Closed; }
        }

        [DynamicVisibleValidationMethod]
        public bool IsVisible(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(Closed):
                    return data.GeometryType == CulvertGeometryType.Rectangle;
                case nameof(Width):
                    return GeometryType == CulvertGeometryType.SteelCunette ||
                           GeometryType == CulvertGeometryType.Cunette ||
                           GeometryType == CulvertGeometryType.Arch ||
                           GeometryType == CulvertGeometryType.Ellipse ||
                           GeometryType == CulvertGeometryType.Rectangle ||
                           GeometryType == CulvertGeometryType.Egg ||
                           GeometryType == CulvertGeometryType.InvertedEgg ;
                case nameof(Height):
                        return GeometryType == CulvertGeometryType.Cunette ||
                               GeometryType == CulvertGeometryType.Arch ||
                               GeometryType == CulvertGeometryType.Rectangle ||
                               GeometryType == CulvertGeometryType.Ellipse ||
                               GeometryType == CulvertGeometryType.Egg ||
                               GeometryType == CulvertGeometryType.InvertedEgg;
                case nameof(ArcHeight):
                        return GeometryType == CulvertGeometryType.Arch || GeometryType == CulvertGeometryType.UShape;
                case nameof(Diameter):
                        return GeometryType == CulvertGeometryType.Round;

                case nameof(Radius):
                case nameof(Radius1):
                case nameof(Radius2):
                case nameof(Radius3):
                case nameof(Angle):
                case nameof(Angle1):
                        return GeometryType == CulvertGeometryType.SteelCunette;
                default:
                    return true;
            }
        }

        [DynamicReadOnlyValidationMethod]
        public bool IsReadOnly(string propertyName)
        {
            if (propertyName == nameof(GateInitialOpening))
            {
                return !IsGated;
            }

            if (propertyName == nameof(GroundlayerThickness) || propertyName == nameof(GroundlayerRoughness))
            {
                return !data.GroundLayerEnabled;
            }

            if (propertyName == nameof(Width))
            {
                return GeometryType == CulvertGeometryType.SteelCunette ||
                         GeometryType == CulvertGeometryType.Tabulated ||
                         GeometryType == CulvertGeometryType.Round;
            }

            if (propertyName == nameof(Height))
            {
                return GeometryType == CulvertGeometryType.Tabulated || GeometryType == CulvertGeometryType.Round;
            }

            if (propertyName == nameof(ArcHeight))
            {
                return GeometryType != CulvertGeometryType.Arch;
            }

            if (propertyName == nameof(Diameter))
            {
                return GeometryType != CulvertGeometryType.Round;
            }

            if (propertyName == nameof(Radius) || propertyName == nameof(Radius1) || propertyName == nameof(Radius2) || propertyName == nameof(Radius3) ||
                propertyName == nameof(Angle) || propertyName == nameof(Angle1))
            {
                return GeometryType != CulvertGeometryType.SteelCunette;
            }

            if (propertyName == nameof(BendLossCoefficient))
            {
                return CulvertType != CulvertType.InvertedSiphon;
            }

            return false;
        }
        
        /// <summary>
        /// Get or set the <see cref="NameValidator"/> for this instance.
        /// Property is initialized with a default name validator. 
        /// </summary>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="value"/> is <c>null</c>.
        /// </exception>
        public NameValidator NameValidator
        {
            get => nameValidator;
            set
            {
                Ensure.NotNull(value, nameof(value));
                nameValidator = value;
            }
        }
    }
}
