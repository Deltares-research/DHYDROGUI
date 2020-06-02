using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DeltaShell.Plugins.CommonTools.Gui.Property;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "ExtraResistanceProperties_DisplayName")]
    public class ExtraResistanceProperties : ObjectProperties<IExtraResistance>
    {
        [Category("General")]
        [PropertyOrder(1)]
        public string Name
        {
            get
            {
                return data.Name;
            }
            set
            {
                data.Name = value;
            }
        }

        [Category("General")]
        [PropertyOrder(2)]
        public string LongName
        {
            get
            {
                return data.LongName;
            }
            set
            {
                data.LongName = value;
            }
        }

        [Category("General")]
        [Description("All the (custom) attributes for this object.")]
        [PropertyOrder(4)]
        [TypeConverter(typeof(AttributeArrayConverter<object>))]
        public AttributeProperties<object>[] Attributes
        {
            get
            {
                return data.Attributes.Select(x => new AttributeProperties<object>(data.Attributes, x.Key)).ToArray();
            }
        }

        [Description("Channel in which the Extra Resistance is located.")]
        [PropertyOrder(4)]
        [Category("Administration")]
        public string Channel
        {
            get
            {
                return data.Channel.ToString();
            }
        }

        [Description("Composite structure in which the Extra Resistance is located.")]
        [PropertyOrder(6)]
        [Category("Administration")]
        public string CompositeStructure
        {
            get
            {
                return data.ParentStructure.ToString();
            }
        }

        [Description("Chainage of the extra resistance in the channel on the map.")]
        [PropertyOrder(5)]
        [Category("Administration")]
        [DisplayName("Chainage (Map)")]
        public double Chainage
        {
            get
            {
                return NetworkHelper.MapChainage(data.ParentStructure);
            }
        }

        [Description("Chainage of the extra resistance in the channel as used in the simulation.")]
        [PropertyOrder(6)]
        [Category("Administration")]
        [DisplayName("Chainage")]
        public double CompuChainage
        {
            get
            {
                return data.ParentStructure.Chainage;
            }
            set
            {
                HydroRegionEditorHelper.MoveBranchFeatureTo(data, value);
            }
        }
    }
}