using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using log4net;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    public class SewerConnectionProperties : ObjectProperties<SewerConnection>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SewerConnectionProperties));

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Name")]
        [PropertyOrder(1)]
        public string Name
        {
            get { return data.Name; }
            set { data.Name = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Length")]
        [PropertyOrder(2)]
        public double Length { 
            get { return data.Length;}
            set { data.Length = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Geometry length")]
        [PropertyOrder(3)]
        public string GeometryLength
        {
            get { return string.Format("{0:0.##}", data.Geometry.Length); }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Invert level begin")]
        [PropertyOrder(5)]
        public double LevelSource
        {
            get => data.LevelSource;
            set => data.LevelSource = value;
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Invert level end")]
        [PropertyOrder(6)]
        public double LevelTarget
        {
            get => data.LevelTarget;
            set => data.LevelTarget = value;
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [PropertyOrder(7)]
        [DisplayName("Sewer type")]
        [TypeConverter(typeof(EnumDescriptionAttributeTypeConverter))]
        public SewerConnectionWaterType WaterType
        {
            get { return data.WaterType; }
            set { data.WaterType = value; }
        }

        [Category(PropertyWindowCategoryHelper.RelationsCategory)]
        [DisplayName("From compartment")]
        [PropertyOrder(3)]
        public string SourceCompartmentName { get { return data.SourceCompartmentName;}  }

        [Category(PropertyWindowCategoryHelper.RelationsCategory)]
        [DisplayName("To compartment")]
        [PropertyOrder(4)]
        public string TargetCompartmentName { get { return data.TargetCompartmentName;}  }

        [Category(PropertyWindowCategoryHelper.RelationsCategory)]
        [DisplayName("From manhole")]
        [PropertyOrder(1)]
        public string SourceManholeName { get { return data.Source.Name; } }

        [Category(PropertyWindowCategoryHelper.RelationsCategory)]
        [DisplayName("To manhole")]
        [PropertyOrder(2)]
        public string TargetManholeName { get { return data.Target.Name; } }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Sewer special connection type")]
        [PropertyOrder(4)]
        [TypeConverter(typeof(EnumDescriptionAttributeTypeConverter))]
        public SewerConnectionSpecialConnectionType SewerConnectionSpecialConnectionType { get { return data.SpecialConnectionType; } }
    }
}