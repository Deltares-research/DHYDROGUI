using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DeltaShell.Plugins.CommonTools.Gui.Property;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "LateralSourceProperties_DisplayName")]
    public class LateralSourceProperties : ObjectProperties<LateralSource>
    {
        [Category("General")]
        [PropertyOrder(1)]
        public string LongName
        {
            get { return data.LongName; }
            set { data.LongName = value; }
        }

        [Category("General")]
        [PropertyOrder(2)]
        public string Name
        {
            get { return data.Name; }
            set { data.Name = value; }
        }

        [Category("General")]
        [Description("All the (custom) attributes for this object.")]
        [TypeConverter(typeof(AttributeArrayConverter<object>))]
        public AttributeProperties<object>[] Attributes
        {
            get { return data.Attributes.Select(x => new AttributeProperties<object>(data.Attributes, x.Key)).ToArray(); }
        }

        [Category("Administration")]
        [Description("Channel in which the source is located.")]
        [PropertyOrder(3)]
        public string Channel
        {
            get { return data.Branch.ToString(); }
        }

        [Category("Administration")]
        [Description("Chainage of the source in the channel.")]
        [DisplayName("Chainage (Map)")]
        [PropertyOrder(3)]
        public double Chainage
        {
            get { return NetworkHelper.MapChainage(data); }
        }

        [Category("Administration")]
        [Description("Chainage of the source in the channel as used in the simulation.")]
        [PropertyOrder(3)]
        [DisplayName("Chainage")]
        public double CompuChainage
        {
            get { return data.Chainage; }
            set { HydroRegionEditorHelper.MoveBranchFeatureTo(data, value); }
        }

        [Description("Length of the reach segment of diffuse lateral into with it is discharging")]
        [DisplayName("Length Diffuse Lateral")]
        [Category("Lateral Diffusion")]
        [PropertyOrder(7)]
        public double LengthLateralSource
        {
            get { return data.Length; }
            set { HydroRegionEditorHelper.UpdateBranchFeatureGeometry(data, value); }
        }
    }
}