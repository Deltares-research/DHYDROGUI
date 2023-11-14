using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DeltaShell.Plugins.CommonTools.Gui.Property;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "WasteWaterTreatmentPlantProperties_DisplayName")]
    public class WasteWaterTreatmentPlantProperties : ObjectProperties<WasteWaterTreatmentPlant>
    {
        [Category("General")]
        [PropertyOrder(1)]
        public string Name
        {
            get { return data.Name; }
            set { data.SetNameIfValid(value); }
        }

        [Category("General")]
        [PropertyOrder(2)]
        public string LongName
        {
            get { return data.LongName; }
            set { data.LongName= value; }
        }

        [Category("General")]
        [Description("All the (custom) attributes for this object.")]
        [PropertyOrder(2)]
        [TypeConverter(typeof(AttributeArrayConverter<object>))]
        public AttributeProperties<object>[] Attributes
        {
            get { return data.Attributes.Select(x => new AttributeProperties<object>(data.Attributes, x.Key)).ToArray(); }
        }

        [Category("General")]
        public double X
        {
            get { return data.Geometry.Centroid.X; }
        }

        [Category("General")]
        public double Y
        {
            get { return data.Geometry.Centroid.Y; }
        }
    }
}
