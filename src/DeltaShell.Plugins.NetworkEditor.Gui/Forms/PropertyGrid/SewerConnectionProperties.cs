using System.ComponentModel;
using System.Drawing.Design;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections.Extensions;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using log4net;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    public class SewerConnectionProperties : ObjectProperties<SewerConnection>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SewerConnectionProperties));

        #region Connection properties

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Name")]
        [PropertyOrder(0)]
        public string Name
        {
            get { return data?.Name ?? string.Empty; }
            set { data.Name = value; }
        }

        [Category(PropertyWindowCategoryHelper.RelationsCategory)]
        [PropertyOrder(1)]
        [DisplayName("From manhole")]
        public string FromManhole
        {
            get { return data?.Source?.ToString() ?? string.Empty; }
        }

        [Category(PropertyWindowCategoryHelper.RelationsCategory)]
        [PropertyOrder(2)]
        [DisplayName("To manhole")]
        public string ToManhole
        {
            get { return data?.Target?.ToString() ?? string.Empty; }
        }

        [Category(PropertyWindowCategoryHelper.RelationsCategory)]
        [PropertyOrder(3)]
        [DisplayName("From compartment")]
        public string FromCompartment
        {
            get { return data?.SourceCompartment?.ToString() ?? string.Empty; }
        }

        [Category(PropertyWindowCategoryHelper.RelationsCategory)]
        [PropertyOrder(4)]
        [DisplayName("To compartment")]
        public string ToCompartment
        {
            get { return data?.TargetCompartment?.ToString() ?? string.Empty; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [PropertyOrder(5)]
        [DisplayName("Invert level begin (m)")]
        public double LevelStart
        {
            get { return data?.LevelSource ?? double.NaN; }
            set { data.LevelSource = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [PropertyOrder(6)]
        [DisplayName("Invert level end (m)")]
        public double LevelTarget
        {
            get { return data?.LevelTarget ?? double.NaN; }
            set { data.LevelTarget = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [PropertyOrder(7)]
        [DisplayName("Water type")]
        [TypeConverter(typeof(EnumDescriptionAttributeTypeConverter))]
        public SewerConnectionWaterType WaterType
        {
            get { return data.WaterType; }
            set { data.WaterType = value; }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [PropertyOrder(3)]
        [DisplayName("Length")]
        [Description("Length used for simulation when IsLengthCustom is true.")]
        public string Length
        {
            get { return string.Format("{0:0.##}", data.Length); }
            set
            {
                double result;
                if (double.TryParse(value, out result))
                {
                    data.Length = result;
                }
            }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Geometry length")]
        [Description("Length of the pipe on the map.")]
        [PropertyOrder(4)]
        public string GeometryLength
        {
            get { return string.Format("{0:0.##}", data.Geometry.Length); }
        }

        [Category(PropertyWindowCategoryHelper.GeneralCategory)]
        [DisplayName("Order number")]
        [PropertyOrder(20)]
        public int OrderNumber
        {
            get { return data.OrderNumber; }
            set { data.OrderNumber = value; }
        }

        #endregion

        #region Cross section

        [Category(PropertyWindowCategoryHelper.CrossSectionCategory)]
        [PropertyOrder(0)]
        [DisplayName("Name")]
        [Editor(typeof(SharedCrossSectionsTypeEditor), typeof(UITypeEditor))]
        public string CrossSectionName
        {
            get { return data?.DefinitionName?? string.Empty; }
            set { data.DefinitionName = value; }
        }

        [Category(PropertyWindowCategoryHelper.CrossSectionCategory)]
        [PropertyOrder(1)]
        [DisplayName("Shape")]
        public string CrossSectionShape
        {
            get { return data?.Profile?.Shape?.Type.ToString() ?? string.Empty; }
        }

        [Category(PropertyWindowCategoryHelper.CrossSectionCategory)]
        [PropertyOrder(2)]
        [DisplayName("Diameter (m)")]
        [DynamicVisible]
        public double CrossSectionDiameter
        {
            get { return data?.Profile?.GetProfileDiameter() ?? double.NaN; }
        }

        [Category(PropertyWindowCategoryHelper.CrossSectionCategory)]
        [PropertyOrder(3)]
        [DisplayName("Width (m)")]
        [DynamicVisible]
        public double CrossSectionWidth
        {
            get { return data?.Profile?.GetProfileWidth() ?? double.NaN; }
        }

        [Category(PropertyWindowCategoryHelper.CrossSectionCategory)]
        [PropertyOrder(4)]
        [DisplayName("Height (m)")]
        [DynamicVisible]
        public double CrossSectionHeight
        {
            get { return data?.Profile?.GetProfileHeight() ?? double.NaN; }
        }

        [Category(PropertyWindowCategoryHelper.CrossSectionCategory)]
        [PropertyOrder(5)]
        [DisplayName("Arch height (m)")]
        [DynamicVisible]
        public double ArcHeight
        {
            get { return data?.Profile?.GetProfileArchHeight() ?? double.NaN; }
        }

        [Category(PropertyWindowCategoryHelper.CrossSectionCategory)]
        [PropertyOrder(6)]
        [DynamicVisible]
        public double Slope
        {
            get { return data?.Profile?.GetProfileSlope() ?? double.NaN; }
        }

        [Category(PropertyWindowCategoryHelper.CrossSectionCategory)]
        [PropertyOrder(7)]
        [DisplayName("Bottom width (m)")]
        [DynamicVisible]
        public double BottomWidthB
        {
            get { return data?.Profile?.GetProfileBottomWidthB() ?? double.NaN; }
        }

        [Category(PropertyWindowCategoryHelper.CrossSectionCategory)]
        [PropertyOrder(8)]
        [DisplayName("Maximum flow width (m)")]
        [DynamicVisible]
        public double MaximumFlowWidth
        {
            get { return data?.Profile?.GetProfileMaximumFlowWidth() ?? double.NaN; }
        }

        [Category(PropertyWindowCategoryHelper.CrossSectionCategory)]
        [PropertyOrder(9)]
        [DisplayName("Open or Closed profile")]
        [DynamicVisible]
        public bool Closed
        {
            get { return (data?.CrossSection as ICrossSectionStandardShapeOpenClosed)?.Closed ?? false; }
        }

        [DynamicVisibleValidationMethod]
        public bool IsVisible(string propertyName)
        {
            var shape = data?.Profile?.Shape;
            switch (propertyName)
            {
                case "CrossSectionDiameter":
                    return shape is CrossSectionStandardShapeCircle;
                case "CrossSectionWidth":
                case "CrossSectionHeight":
                    return shape is CrossSectionStandardShapeWidthHeightBase || shape is CrossSectionStandardShapeArch;
                case "ArcHeight":
                    return shape is CrossSectionStandardShapeArch;
                case "Closed":
                    return shape is ICrossSectionStandardShapeOpenClosed;
                case "Slope":
                case "BottomWidthB":
                case "MaximumFlowWidth":
                    return shape is CrossSectionStandardShapeTrapezium;
                default:
                    log.DebugFormat("The visibility of an unknown property '" + propertyName + "' has been requested.");
                    return true;
            }
        }

        #endregion
    }
}
