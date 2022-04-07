using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    [ResourcesDisplayName(typeof(Resources), "CatchmentProperties_DisplayName")]
    public class CatchmentProperties : ObjectProperties<Catchment>
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

        [Description("Catchment area based on geometry.")]
        [Category("General")]
        [DisplayName("Geometry area (m²)")]
        [PropertyOrder(2)]
        [DynamicReadOnly]
        public double Area
        {
            get { return data.GeometryArea; }
            set
            {
                if (data.IsGeometryDerivedFromAreaSize)
                {
                    data.SetAreaSize(value);
                }
            }
        }

        [Description("Use derived Geometry to display catchment")]
        [Category("General")]
        [DisplayName("Default Geometry")]
        [PropertyOrder(4)]
        public bool IsDefaultGeometry
        {
            get { return data.IsGeometryDerivedFromAreaSize; }
        }

        [DynamicReadOnlyValidationMethod]
        public bool IsReadOnly(string propertyName)
        {
            if (propertyName == "Area")
            {
                return !data.IsGeometryDerivedFromAreaSize;
            }

            return false;
        }
    }
}