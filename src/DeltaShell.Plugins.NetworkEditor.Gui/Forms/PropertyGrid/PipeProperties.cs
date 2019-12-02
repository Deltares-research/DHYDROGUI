using System.ComponentModel;
using DelftTools.Hydro.CrossSections.Extensions;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;
using log4net;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    public class PipeProperties : ObjectProperties<Pipe>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PipeProperties));

        #region Connection properties

        [Category("Connection properties")]
        [PropertyOrder(0)]
        public string Name
        {
            get { return data?.Name ?? string.Empty; }
            set { data.Name = value; }
        }

        [Category("Connection properties")]
        [PropertyOrder(1)]
        [DisplayName("Begin manhole")]
        public string FromManhole
        {
            get { return data?.Source?.ToString() ?? string.Empty; }
        }

        [Category("Connection properties")]
        [PropertyOrder(2)]
        [DisplayName("End manhole")]
        public string ToManhole
        {
            get { return data?.Target?.ToString() ?? string.Empty; }
        }

        [Category("Connection properties")]
        [PropertyOrder(3)]
        [DisplayName("Begin compartment")]
        public string FromCompartment
        {
            get { return data?.SourceCompartment?.ToString() ?? string.Empty; }
        }

        [Category("Connection properties")]
        [PropertyOrder(4)]
        [DisplayName("End compartment")]
        public string ToCompartment
        {
            get { return data?.TargetCompartment?.ToString() ?? string.Empty; }
        }

        [Category("Connection properties")]
        [PropertyOrder(5)]
        [DisplayName("Invert level begin (m)")]
        public double LevelStart
        {
            get { return data?.LevelSource ?? double.NaN; }
        }

        [Category("Connection properties")]
        [PropertyOrder(6)]
        [DisplayName("Invert level end (m)")]
        public double LevelTarget
        {
            get { return data?.LevelTarget ?? double.NaN; }
        }

        [Category("Connection properties")]
        [PropertyOrder(7)]
        [DisplayName("Water type")]
        public string WaterType
        {
            get { return data?.WaterType.ToString() ?? string.Empty; }
        }

        [Category("Connection properties")]
        [PropertyOrder(8)]
        [DisplayName("Length (m)")]
        [Description("Length used for simulation when IsLengthCustom is true")]
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

        #endregion

        #region Cross section

        [Category("Cross section properties")]
        [PropertyOrder(0)]
        [DisplayName("Name")]
        public string CrossSectionName
        {
            get { return data?.Profile?.ToString() ?? string.Empty; }
        }

        [Category("Cross section properties")]
        [PropertyOrder(1)]
        [DisplayName("Shape")]
        public string CrossSectionShape
        {
            get { return data?.Profile?.Shape?.Type.ToString() ?? string.Empty; }
        }

        [Category("Cross section properties")]
        [PropertyOrder(2)]
        [DisplayName("Diameter (m)")]
        [DynamicVisible]
        public double CrossSectionDiameter
        {
            get { return data?.Profile?.GetProfileDiameter() ?? double.NaN; }
        }

        [Category("Cross section properties")]
        [PropertyOrder(3)]
        [DisplayName("Width (m)")]
        [DynamicVisible]
        public double CrossSectionWidth
        {
            get { return data?.Profile?.GetProfileWidth() ?? double.NaN; }
        }

        [Category("Cross section properties")]
        [PropertyOrder(4)]
        [DisplayName("Height (m)")]
        [DynamicVisible]
        public double CrossSectionHeight
        {
            get { return data?.Profile?.GetProfileHeight() ?? double.NaN; }
        }

        [Category("Cross section properties")]
        [PropertyOrder(5)]
        [DisplayName("Arch height (m)")]
        [DynamicVisible]
        public double ArcHeight
        {
            get { return data?.Profile?.GetProfileArchHeight() ?? double.NaN; }
        }

        [Category("Cross section properties")]
        [PropertyOrder(6)]
        [DynamicVisible]
        public double Slope
        {
            get { return data?.Profile?.GetProfileSlope() ?? double.NaN; }
        }

        [Category("Cross section properties")]
        [PropertyOrder(7)]
        [DisplayName("Bottom width (m)")]
        [DynamicVisible]
        public double BottomWidthB
        {
            get { return data?.Profile?.GetProfileBottomWidthB() ?? double.NaN; }
        }

        [Category("Cross section properties")]
        [PropertyOrder(8)]
        [DisplayName("Maximum flow width (m)")]
        [DynamicVisible]
        public double MaximumFlowWidth
        {
            get { return data?.Profile?.GetProfileMaximumFlowWidth() ?? double.NaN; }
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
