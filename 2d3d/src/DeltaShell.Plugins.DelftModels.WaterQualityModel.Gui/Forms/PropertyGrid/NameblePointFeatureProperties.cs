using System.ComponentModel;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.PropertyGrid
{
    public abstract class NameblePointFeatureProperties : ObjectProperties<NameablePointFeature>
    {
        [Category("General")]
        [DisplayName("Name")]
        [PropertyOrder(1)]
        public string Name
        {
            get => data.Name;
            set => data.Name = value;
        }

        [Category("Location")]
        [DisplayName("X")]
        [PropertyOrder(1)]
        public double X
        {
            get => data.X;
            set => data.X = value;
        }

        [Category("Location")]
        [DisplayName("Y")]
        [PropertyOrder(2)]
        public double Y
        {
            get => data.Y;
            set => data.Y = value;
        }

        [Category("Location")]
        [DisplayName("Z")]
        [Description("Depth (0(top) - 1 (bottom) for sigma layers / between ZMax and ZMin for Z layers)")]
        [PropertyOrder(3)]
        [DynamicVisible]
        public double Z
        {
            get => data.Z;
            set => data.Z = value;
        }

        [DynamicVisibleValidationMethod]
        public virtual bool IsPropertyVisible(string propertyName)
        {
            return true;
        }
    }
}