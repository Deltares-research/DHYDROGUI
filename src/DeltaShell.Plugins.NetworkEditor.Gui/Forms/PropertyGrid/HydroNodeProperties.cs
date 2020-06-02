using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DeltaShell.Plugins.CommonTools.Gui.Property;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "HydroNodeProperties_DisplayName")]
    public class HydroNodeProperties : ObjectProperties<HydroNode>
    {
        [Category("General")]
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

        [Category("Relations")]
        [Description("Number of branches that end in this node.")]
        public int IncomingBranches
        {
            get
            {
                return data.IncomingBranches.Count;
            }
        }

        [Category("Relations")]
        [Description("Number of branches that start in this node.")]
        public int OutgoingBranches
        {
            get
            {
                return data.OutgoingBranches.Count;
            }
        }

        [Category("General")]
        public double X
        {
            get
            {
                return data.Geometry.Centroid.X;
            }
            set
            {
                //unwanted relation..also causes a crash when setting with no mapcontrol open.
                HydroRegionEditorHelper.MoveNodeTo(data, value, Y);
            }
        }

        [Category("General")]
        public double Y
        {
            get
            {
                return data.Geometry.Centroid.Y;
            }
            set
            {
                HydroRegionEditorHelper.MoveNodeTo(data, X, value);
            }
        }

        [Category("General")]
        [Description("All the (custom) attributes for this object.")]
        [TypeConverter(typeof(AttributeArrayConverter<object>))]
        public AttributeProperties<object>[] Attributes
        {
            get
            {
                return data.Attributes.Select(x => new AttributeProperties<object>(data.Attributes, x.Key)).ToArray();
            }
        }
    }
}