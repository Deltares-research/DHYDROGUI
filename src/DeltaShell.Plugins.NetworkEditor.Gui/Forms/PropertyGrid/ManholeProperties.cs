using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;
using DelftTools.Utils;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    public class ManholeProperties : ObjectProperties<Manhole>
    {
        [Category("Manhole properties")]
        [PropertyOrder(0)]
        public string Name
        {
            get { return data.Name; }
            set { data.Name = value; }
        }

        [Category("Manhole properties")]
        [PropertyOrder(1)]
        public double X
        {
            get { return data.Geometry.Coordinate.X; }
            set { data.Geometry.Coordinate.X = value; }
        }

        [Category("Manhole properties")]
        [PropertyOrder(2)]
        public double Y
        {
            get { return data.Geometry.Coordinate.Y; }
            set { data.Geometry.Coordinate.Y = value; }
        }

        [Category("Manhole properties")]
        [Description("All the (custom) attributes for this object.")]
        [PropertyOrder(3)]
        [TypeConverter(typeof(ExpandableArrayConverter))]
        public string[] Compartments
        {
            get { return data.Compartments.Select(c => c.Name).ToArray(); }
        }



    }
}
