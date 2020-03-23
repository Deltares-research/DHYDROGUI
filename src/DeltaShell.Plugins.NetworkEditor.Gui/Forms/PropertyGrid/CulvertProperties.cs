using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.Plugins.CommonTools.Gui.Property;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "CulvertProperties_DisplayName")]
    public class CulvertProperties : ObjectProperties<ICulvert>
    {
        [Category("General")]
        [PropertyOrder(0)]
        public string Name
        {
            get { return data.Name; }
            set { data.Name = value; }
        }
        
        [Category("General")]
        [PropertyOrder(1)]
        public string LongName
        {
            get { return data.LongName; }
            set { data.LongName = value; }
        }

        [Description("Length of the culvert (along the branch).")]
        [Category("General")]
        [DisplayName("Length")]
        [PropertyOrder(2)]
        public double Length
        {
            get { return data.Length; }
            set { data.Length = value; }
        }

        [DynamicReadOnly]
        [Description("Width of the culvert.")]
        [Category("General")]
        [DisplayName("Width")]
        [PropertyOrder(3)]
        public double Width
        {
            get { return data.Width; }
            set { data.Width = value; }
        }

        [DynamicReadOnly]
        [Description("Height of the culvert.")]
        [Category("General")]
        [DisplayName("Height")]
        [PropertyOrder(4)]
        public double Height
        {
            get { return data.Height; }
            set { data.Height = value; }
        }

        [DisplayName("Roughness type")]
        [Description("Roughness type")]
        [Category("General")]
        [PropertyOrder(5)]
        public CulvertFrictionType FrictionType
        {
            get { return data.FrictionType; }
            set
            {
                if (data.FrictionType != value)
                {
                    data.FrictionType = value;
                    data.Friction = RoughnessTypeDefaults.GetDefault(value);
                }
            }
        }

        [DisplayName("Roughness")]
        [Description("Roughness")]
        [Category("General")]
        [PropertyOrder(6)]
        public double Friction
        {
            get { return data.Friction; }
            set { data.Friction = value; }
        }
        
        [DynamicReadOnly]
        [DisplayName("Groundlayer roughness")]
        [Description("Groundlayer roughness")]
        [Category("Groundlayer")]
        [PropertyOrder(6)]
        public double GroundlayerRoughness
        {
            get { return data.GroundLayerRoughness; }
            set { data.GroundLayerRoughness = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Groundlayer thickness")]
        [Description("Groundlayer thickness")]
        [Category("Groundlayer")]
        [PropertyOrder(6)]
        public double GroundlayerThickness
        {
            get { return data.GroundLayerThickness; }
            set { data.GroundLayerThickness = value; }
        }

        [DisplayName("Groundlayer enabled")]
        [Description("Groundlayer enabled")]
        [Category("Groundlayer")]
        [PropertyOrder(6)]
        public bool GroundlayerEnabled
        {
            get { return data.GroundLayerEnabled; }
            set { data.GroundLayerEnabled = value; }
        }

        [Description("Is negative flow possible.")]
        [Category("Designer")]
        [PropertyOrder(7)]
        [DynamicReadOnly]
        public bool AllowNegativeFlow
        {
            get { return data.AllowNegativeFlow; }
            set { data.AllowNegativeFlow = value; }
        }

        [Description("Is positive flow possible.")]
        [Category("Designer")]
        [PropertyOrder(8)]
        public bool AllowPositiveFlow
        {
            get { return data.AllowPositiveFlow; }
            set { data.AllowPositiveFlow = value; }
        }

        [Description("The culvert can be siphon, inverted siphon or culvert.")]
        [Category("General")]
        [DisplayName("Culvert Type")]
        [DynamicReadOnly]
        [PropertyOrder(11)]
        public CulvertType CulvertType
        {
            get { return data.CulvertType; }
            set { data.CulvertType = value; }
        }
        [Category("General")]
        [Description("All the (custom) attributes for this object.")]
        [PropertyOrder(12)]
        [TypeConverter(typeof(AttributeArrayConverter<object>))]
        public AttributeProperties<object>[] Attributes
        {
            get { return data.Attributes.Select(x => new AttributeProperties<object>(data.Attributes, x.Key)).ToArray(); }
        }

        [Description("Inlet loss")]
        [Category("Calculation")]
        [PropertyOrder(12)]
        public double InletLoss
        {
            get { return data.InletLossCoefficient; }
            set { data.InletLossCoefficient = value; }
        }

        [Description("Inlet level.")]
        [Category("Calculation")]
        [PropertyOrder(13)]
        public double InletLevel
        {
            get { return data.InletLevel; }
            set { data.InletLevel = value; }
        }

        [Description("Outlet loss.")]
        [Category("Calculation")]
        [PropertyOrder(14)]
        public double OutletLoss
        {
            get { return data.OutletLossCoefficient; }
            set { data.OutletLossCoefficient = value; }
        }

        [Description("Outlet level.")]
        [Category("Calculation")]
        [PropertyOrder(15)]
        public double OutletLevel
        {
            get { return data.OutletLevel; }
            set { data.OutletLevel = value; }
        }

        [Description("BendLoss coefficient of the culvert.")]
        [Category("Calculation")]
        [PropertyOrder(16)]
        public double BendLossCoefficient
        {
            get { return data.BendLossCoefficient; }
            set { data.BendLossCoefficient = value; }
        }

        [Description("Channel in which the composite structure is located.")]
        [PropertyOrder(17)]
        [Category("Administration")]
        public string Channel
        {
            get { return data.Branch.ToString(); }
        }

        [Description("Composite structure in which the structure is located.")]
        [PropertyOrder(18)]
        [Category("Administration")]
        public string CompositeStructure
        {
            get { return data.ParentStructure.ToString(); }
        }

        [Description("Chainage of the culvert in the channel on the map.")]
        [PropertyOrder(19)]
        [Category("Administration")]
        [DisplayName("Chainage (Map)")]
        public double Chainage
        {
            get { return NetworkHelper.MapChainage(data.ParentStructure); }
        }

        [Description("Chainage of the culvert in the channel as used in the simulation.")]
        [PropertyOrder(20)]
        [Category("Administration")]
        [DisplayName("Chainage")]
        public double CompuChainage
        {
            get { return data.ParentStructure.Chainage; }
            set { HydroRegionEditorHelper.MoveBranchFeatureTo(data, value); }
        }

        [Description("OffsetY of the weir in the cross section profile.")]
        [Category("Designer")]
        [PropertyOrder(21)]
        public string YOffSet
        {
            get { return string.Format("{0:0.##}", data.OffsetY); }
            set { data.OffsetY = double.Parse(value); }
        }

        [DisplayName("Is gated")]
        [Category("Calculation")]
        [PropertyOrder(22)]
        public bool IsGated
        {
            get { return data.IsGated; }
            set { data.IsGated = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Gate initial opening")]
        [Description("Initial opening of the gate")]
        [Category("Calculation")]
        [PropertyOrder(23)]
        public double GateInitialOpening
        {
            get { return data.GateInitialOpening; }
            set { data.GateInitialOpening = value; }
        }

        [DisplayName("Gate lower edge")]
        [Description("Initial opening of the gate")]
        [Category("Calculation")]
        [PropertyOrder(24)]
        public double GateLowerEdgeLevel
        {
            get { return data.GateLowerEdgeLevel; }
        }

        [DisplayName("Siphon on level")]
        [Category("Calculation")]
        [PropertyOrder(25)]
        public double SiphonOnLevel
        {
            get { return data.SiphonOnLevel; }
            set { data.SiphonOnLevel = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Siphon off level")]
        [Category("Calculation")]
        [PropertyOrder(25)]
        public double SiphonOffLevel
        {
            get { return data.SiphonOffLevel; }
            set { data.SiphonOffLevel = value; }
        }

        [DisplayName("Geometry type")]
        [Category("Shape")]
        [PropertyOrder(25)]
        public CulvertGeometryType GeometryType
        {
            get { return data.GeometryType; }
            set { data.GeometryType = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Arc height")]
        [Description("ArcHeight of the culvert (if shape arch)")]
        [Category("Shape")]
        [PropertyOrder(25)]
        public double ArcHeight
        {
            get { return data.ArcHeight; }
            set { data.ArcHeight = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Diameter")]
        [Description("Diameter of the culvert (if shape round)")]
        [Category("Shape")]
        [PropertyOrder(25)]
        public double Diameter
        {
            get { return data.Diameter; }
            set { data.Diameter = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Radius")]
        [Description("Radius of the culvert (if shape steel cunette)")]
        [Category("Shape")]
        [PropertyOrder(25)]
        public double Radius
        {
            get { return data.Radius; }
            set { data.Radius = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Radius1")]
        [Description("Radius1 of the culvert (if shape steel cunette)")]
        [Category("Shape")]
        [PropertyOrder(25)]
        public double Radius1
        {
            get { return data.Radius1; }
            set { data.Radius1 = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Radius2")]
        [Description("Radius2 of the culvert (if shape steel cunette)")]
        [Category("Shape")]
        [PropertyOrder(25)]
        public double Radius2
        {
            get { return data.Radius2; }
            set { data.Radius2 = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Radius3")]
        [Description("Radius3 of the culvert (if shape steel cunette)")]
        [Category("Shape")]
        [PropertyOrder(25)]
        public double Radius3
        {
            get { return data.Radius3; }
            set { data.Radius3 = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Angle")]
        [Description("Angle of the culvert (if shape steel cunette)")]
        [Category("Shape")]
        [PropertyOrder(25)]
        public double Angle
        {
            get { return data.Angle; }
            set { data.Angle = value; }
        }

        [DynamicReadOnly]
        [DisplayName("Angle1")]
        [Description("Angle1 of the culvert (if shape steel cunette)")]
        [Category("Shape")]
        [PropertyOrder(25)]
        public double Angle1
        {
            get { return data.Angle1; }
            set { data.Angle1 = value; }
        }

        [Category("Cross section properties")]
        [PropertyOrder(26)]
        [DisplayName("Open or Closed profile")]
        [DynamicVisible]
        public bool Closed
        {
            get { return data.Closed; }
        }

        [DynamicVisibleValidationMethod]
        public bool IsVisible(string propertyName)
        {
            switch (propertyName)
            {
                case "Closed":
                    return data.GeometryType == CulvertGeometryType.Rectangle;
                default:
                    return true;
            }
        }

        [DynamicReadOnlyValidationMethod]
        public bool DynamicReadOnlyValidationMethod(string propertyName)
        {
            if (propertyName == "SiphonOnLevel" || propertyName == "SiphonOffLevel")
            {
                return !CulvertType.Equals(CulvertType.Siphon);
            }

            if (propertyName == "AllowNegativeFlow")
            {
                return CulvertType.Equals(CulvertType.Siphon);
            }

            if (propertyName == "GateInitialOpening")
            {
                return !IsGated;
            }

            if (propertyName == "GroundLayerRoughness" || propertyName == "GroundLayerThickness")
            {
                return !data.GroundLayerEnabled;
            }

            if (propertyName == "Width")
            {
                return GeometryType == CulvertGeometryType.SteelCunette ||
                         GeometryType == CulvertGeometryType.Tabulated ||
                         GeometryType == CulvertGeometryType.Round;
            }

            if (propertyName == "Height")
            {
                return GeometryType == CulvertGeometryType.Tabulated || GeometryType == CulvertGeometryType.Round;
            }

            if (propertyName == "ArcHeight")
            {
                return GeometryType != CulvertGeometryType.Arch;
            }

            if (propertyName == "Diameter")
            {
                return GeometryType != CulvertGeometryType.Round;
            }

            if (propertyName == "Radius" || propertyName == "Radius1" || propertyName == "Radius2" || propertyName == "Radius3" ||
                propertyName == "Angle" || propertyName == "Angle1")
            {
                return GeometryType != CulvertGeometryType.SteelCunette;
            }

            return false;
        }
    }
}
