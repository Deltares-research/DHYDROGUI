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
    [ResourcesDisplayName(typeof(Resources), "BridgeProperties_DisplayName")]
    public class BridgeProperties : ObjectProperties<IBridge>
    {
        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Name")]
        [PropertyOrder(0)]
        public string Name
        {
            get { return data.Name; }
            set { data.Name = value ; }
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
        [DisplayName("Shape")]
        [Description("Cross sectional bridge shape.")]
        [PropertyOrder(2)]
        public BridgeType BridgeType
        {
            get { return data.BridgeType; }
            set { data.BridgeType = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Length")]
        [Description("Length of the bridge (along the branch).")]
        [PropertyOrder(3)]
        [DynamicReadOnly]
        public double Length
        {
            get { return data.Length; }
            set { data.Length= value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Flow direction")]
        [Description("Direction in which flow is allowed.")]
        [PropertyOrder(4)]
        public FlowDirection AllowedFlowDirection
        {
            get { return data.FlowDirection; }
            set { data.FlowDirection= value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Inlet loss coefficient")]
        [Description("Inlet loss.")]
        [PropertyOrder(5)]
        [DynamicReadOnly]
        public double InletLoss
        {
            get { return data.InletLossCoefficient; }
            set { data.InletLossCoefficient= value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Outlet loss coefficient")]
        [Description("Outlet loss.")]
        [PropertyOrder(6)]
        [DynamicReadOnly]
        public double OutletLoss
        {
            get { return data.OutletLossCoefficient; }
            set { data.OutletLossCoefficient = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Roughness type")]
        [Description("Roughness type.")]
        [PropertyOrder(7)]
        [DynamicReadOnly]
        public BridgeFrictionType FrictionType
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
        [PropertyOrder(8)]
        [DynamicReadOnly]
        public double Friction
        {
            get { return data.Friction; }
            set { data.Friction = value; }
        }

        [Category(PropertyWindowCategoryHelper.GroundLayerCategory)]
        [DisplayName("Ground layer enabled")]
        [Description("Ground layer enabled.")]
        [PropertyOrder(6)]
        [DynamicReadOnly]
        [Browsable(false)]
        public bool GroundlayerEnabled
        {
            get { return data.GroundLayerEnabled; }
            set { data.GroundLayerEnabled = value; }
        }

        [Category(PropertyWindowCategoryHelper.GroundLayerCategory)]
        [DisplayName("Ground layer roughness")]
        [Description("Ground layer roughness.")]
        [PropertyOrder(6)]
        [DynamicReadOnly]
        [Browsable(false)]

        public double GroundlayerRoughness
        {
            get { return data.GroundLayerRoughness; }
            set { data.GroundLayerRoughness = value; }
        }

        [Category(PropertyWindowCategoryHelper.GroundLayerCategory)]
        [DisplayName("Ground layer thickness")]
        [Description("Ground layer thickness.")]
        [PropertyOrder(7)]
        [DynamicReadOnly]
        [Browsable(false)]
        public double GroundlayerThickness
        {
            get { return data.GroundLayerThickness; }
            set { data.GroundLayerThickness = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Bed level")]
        [Description("Bed level.")]
        [PropertyOrder(8)]
        [DynamicReadOnly]
        public double BottomLevel
        {
            get { return data.BottomLevel; }
            set { data.BottomLevel = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Width")]
        [Description("Width")]
        [PropertyOrder(9)]
        [DynamicReadOnly]
        public double Width
        {
            get { return data.Width; }
            set { data.Width = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Height")]
        [Description("Height.")]
        [PropertyOrder(10)]
        [DynamicReadOnly]
        public double Height
        {
            get { return data.Height; }
            set { data.Height = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Attributes")]
        [Description("All the (custom) attributes for this object.")]
        [PropertyOrder(11)]
        [TypeConverter(typeof(AttributeArrayConverter<object>))]
        public AttributeProperties<object>[] Attributes
        {
            get { return data.Attributes.Select(x => new AttributeProperties<object>(data.Attributes, x.Key)).ToArray(); }
        }

        [Category(PropertyWindowCategoryHelper.PillarCategory)]
        [DisplayName("Total pillar width")]
        [Description("Total width of all pillars.")]
        [PropertyOrder(1)]
        [DynamicReadOnly]
        [Browsable(false)]//Not yet implemented in the kernel
        public double PillarWidth
        {
            get { return data.PillarWidth; }
            set { data.PillarWidth = value; }
        }

        [Category(PropertyWindowCategoryHelper.PillarCategory)]
        [DisplayName("Shape/Form factor")]
        [Description("Shape/Form factor.")]
        [PropertyOrder(2)]
        [DynamicReadOnly]
        [Browsable(false)]//Not yet implemented in the kernel
        public double ShapeFactor
        {
            get { return data.ShapeFactor; }
            set { data.ShapeFactor = value; }
        }

        [Category(PropertyWindowCategoryHelper.AdministrationCategory)]
        [DisplayName("Branch")]
        [Description("Channel in which the compound structure is located.")]
        [PropertyOrder(9)]
        public string Channel
        {
            get { return data.Branch.ToString(); }
        }

        [Category(PropertyWindowCategoryHelper.AdministrationCategory)]
        [DisplayName("Compound structure")]
        [Description("Compound structure in which the structure is located.")]
        [PropertyOrder(10)]
        public string CompositeStructure
        {
            get { return data.ParentStructure.ToString(); }
        }

        [Category(PropertyWindowCategoryHelper.AdministrationCategory)]
        [DisplayName("Chainage (map)")]
        [Description("Chainage of the bridge in the channel on the map.")]
        [PropertyOrder(11)]
        public double Chainage
        {
            get { return NetworkHelper.MapChainage(data.ParentStructure); }
        }

        [Category(PropertyWindowCategoryHelper.AdministrationCategory)]
        [DisplayName("Chainage")]
        [Description("Chainage of the bridge in the channel as used in the simulation.")]
        [PropertyOrder(12)]
        public double CompuChainage
        {
            get { return data.ParentStructure.Chainage; }
            set { HydroRegionEditorHelper.MoveBranchFeatureTo(data, value); }
        }

        [DynamicReadOnlyValidationMethod]
        public bool DynamicReadOnlyValidationMethod(string propertyName)
        {
            if (propertyName == nameof(Length) || propertyName == nameof(InletLoss) || propertyName == nameof(OutletLoss) || propertyName == nameof(GroundlayerEnabled) || 
                propertyName == nameof(FrictionType) || propertyName == nameof(Friction))
            {
                return data.IsPillar;
            }

            if(propertyName == nameof(GroundlayerThickness) || propertyName == nameof(GroundlayerRoughness))
            {
                return data.IsPillar || !data.GroundLayerEnabled;
            }

            if (propertyName == nameof(PillarWidth) || propertyName == nameof(ShapeFactor))
            {
                return !data.IsPillar;
            }

            if (propertyName == nameof(BottomLevel) || propertyName == nameof(Width) || propertyName == nameof(Height))
            {
                return !data.IsRectangle;
            }

            return false;
        }
    }
}
