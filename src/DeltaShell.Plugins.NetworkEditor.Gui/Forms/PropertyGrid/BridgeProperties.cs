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
        [Category("General")]
        [PropertyOrder(0)]
        public string Name
        {
            get { return data.Name; }
            set { data.Name = value ; }
        }
        
        [Category("General")]
        [PropertyOrder(1)]
        public string LongName
        {
            get { return data.LongName; }
            set { data.LongName = value; }
        }

        [Description("Cross sectional bridge shape")]
        [Category("General")]
        [DisplayName("Bridge Type")]
        [PropertyOrder(2)]
        public BridgeType BridgeType
        {
            get { return data.BridgeType; }
            set { data.BridgeType = value; }
        }

        [Description("Length of the bridge (along the branch).")]
        [Category("General")]
        [DisplayName("Length")]
        [PropertyOrder(3)]
        [DynamicReadOnly]
        public double Length
        {
            get { return data.Length; }
            set { data.Length= value; }
        }

        [Description("Direction in which flow is allowed.")]
        [Category("General")]
        [PropertyOrder(4)]
        public FlowDirection AllowedFlowDirection
        {
            get { return data.FlowDirection; }
            set { data.FlowDirection= value; }
        }

        [Description("Inlet loss")]
        [Category("General")]
        [PropertyOrder(5)]
        [DynamicReadOnly]
        public double InletLoss
        {
            get { return data.InletLossCoefficient; }
            set { data.InletLossCoefficient= value; }
        }

        [Description("Outlet loss.")]
        [Category("General")]
        [PropertyOrder(6)]
        [DynamicReadOnly]
        public double OutletLoss
        {
            get { return data.OutletLossCoefficient; }
            set { data.OutletLossCoefficient = value; }
        }

        [DisplayName("Roughness type")]
        [Description("Roughness type")]
        [Category("General")]
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
                    data.Friction = RoughnessTypeDefaults.GetDefault(value);
                }
            }
        }

        [DisplayName("Roughness")]
        [Description("Roughness")]
        [Category("General")]
        [PropertyOrder(8)]
        [DynamicReadOnly]
        public double Friction
        {
            get { return data.Friction; }
            set { data.Friction = value; }
        }

        [DisplayName("Groundlayer enabled")]
        [Description("Groundlayer enabled")]
        [Category("Groundlayer")]
        [PropertyOrder(6)]
        [DynamicReadOnly]
        public bool GroundlayerEnabled
        {
            get { return data.GroundLayerEnabled; }
            set { data.GroundLayerEnabled = value; }
        }

        [DisplayName("Groundlayer roughness")]
        [Description("Groundlayer roughness")]
        [Category("Groundlayer")]
        [PropertyOrder(6)]
        [DynamicReadOnly]
        public double GroundlayerRoughness
        {
            get { return data.GroundLayerRoughness; }
            set { data.GroundLayerRoughness = value; }
        }

        [DisplayName("Groundlayer thickness")]
        [Description("Groundlayer thickness")]
        [Category("Groundlayer")]
        [PropertyOrder(6)]
        [DynamicReadOnly]
        public double GroundlayerThickness
        {
            get { return data.GroundLayerThickness; }
            set { data.GroundLayerThickness = value; }
        }

        [DisplayName("Bed level")]
        [Description("Bed level")]
        [Category("General")]
        [PropertyOrder(8)]
        [DynamicReadOnly]
        public double BottomLevel
        {
            get { return data.BottomLevel; }
            set { data.BottomLevel = value; }
        }

        [Description("Width")]
        [Category("General")]
        [PropertyOrder(9)]
        [DynamicReadOnly]
        public double Width
        {
            get { return data.Width; }
            set { data.Width = value; }
        }

        [Description("Height")]
        [Category("General")]
        [PropertyOrder(10)]
        [DynamicReadOnly]
        public double Height
        {
            get { return data.Height; }
            set { data.Height = value; }
        }

        [Category("General")]
        [Description("All the (custom) attributes for this object.")]
        [PropertyOrder(11)]
        [TypeConverter(typeof(AttributeArrayConverter<object>))]
        public AttributeProperties<object>[] Attributes
        {
            get { return data.Attributes.Select(x => new AttributeProperties<object>(data.Attributes, x.Key)).ToArray(); }
        }

        [Description("Total width of all pillars")]
        [PropertyOrder(1)]
        [Category("Pillar")]
        [DynamicReadOnly]
        public double PillarWidth
        {
            get { return data.PillarWidth; }
            set { data.PillarWidth = value; }
        }

        [Description("Shape/Form factor")]
        [PropertyOrder(2)]
        [Category("Pillar")]
        [DynamicReadOnly]
        public double ShapeFactor
        {
            get { return data.ShapeFactor; }
            set { data.ShapeFactor = value; }
        }

        [Description("Channel in which the composite structure is located.")]
        [PropertyOrder(9)]
        [Category("Administration")]
        public string Channel
        {
            get { return data.Branch.ToString(); }
        }

        [Description("Composite structure in which the structure is located.")]
        [PropertyOrder(10)]
        [Category("Administration")]
        public string CompositeStructure
        {
            get { return data.ParentStructure.ToString(); }
        }

        [Description("Chainage of the bridge in the channel on the map.")]
        [PropertyOrder(11)]
        [Category("Administration")]
        [DisplayName("Chainage (Map)")]
        public double Chainage
        {
            get { return NetworkHelper.MapChainage(data.ParentStructure); }
        }

        [Description("Chainage of the bridge in the channel as used in the simulation.")]
        [PropertyOrder(12)]
        [Category("Administration")]
        [DisplayName("Chainage")]
        public double CompuChainage
        {
            get { return data.ParentStructure.Chainage; }
            set { HydroRegionEditorHelper.MoveBranchFeatureTo(data, value); }
        }

        [DynamicReadOnlyValidationMethod]
        public bool DynamicReadOnlyValidationMethod(string propertyName)
        {
            if (propertyName == "Length" || propertyName == "InletLoss" || propertyName == "OutletLoss" || propertyName == "GroundlayerEnabled" || 
                propertyName == "FrictionType" || propertyName == "Friction")
            {
                return data.IsPillar;
            }

            if(propertyName == "GroundlayerThickness" || propertyName == "GroundlayerRoughness")
            {
                return data.IsPillar || !data.GroundLayerEnabled;
            }

            if (propertyName == "PillarWidth" || propertyName == "ShapeFactor")
            {
                return !data.IsPillar;
            }

            if (propertyName == "BottomLevel" || propertyName == "Width" || propertyName == "Height")
            {
                return !data.IsRectangle;
            }

            return false;
        }
    }
}
