using System.ComponentModel;
using DelftTools.Hydro.CrossSections.Extensions;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    public class PipeProperties : ObjectProperties<Pipe>
    {
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
        [DisplayName("Source manhole")]
        public string FromManhole
        {
            get { return data?.Source?.ToString() ?? string.Empty; }
        }

        [Category("Connection properties")]
        [PropertyOrder(2)]
        [DisplayName("Target manhole")]
        public string ToManhole
        {
            get { return data?.Target?.ToString() ?? string.Empty; }
        }

        [Category("Connection properties")]
        [PropertyOrder(3)]
        [DisplayName("Source compartment")]
        public string FromCompartment
        {
            get { return data?.SourceCompartment?.ToString() ?? string.Empty; }
        }

        [Category("Connection properties")]
        [PropertyOrder(4)]
        [DisplayName("Target compartment")]
        public string ToCompartment
        {
            get { return data?.TargetCompartment?.ToString() ?? string.Empty; }
        }

        [Category("Connection properties")]
        [PropertyOrder(5)]
        [DisplayName("Level source")]
        public double LevelStart
        {
            get { return data?.LevelSource ?? double.NaN; }
        }

        [Category("Connection properties")]
        [PropertyOrder(6)]
        [DisplayName("Level target")]
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

        [Category("Connection properties")]
        [PropertyOrder(9)]
        [Description("Length of the channel on the map.")]
        [DisplayName("Geometry length")]
        public string GeometryLength
        {
            get { return string.Format("{0:0.##}", data.Geometry.Length); }
        }

        [Category("Connection properties")]
        [PropertyOrder(10)]
        [DisplayName("Order number")]
        [Description("Order number will be used for interpolation over branches. A chain of branches with the same order number will be treated as one.")]
        public int OrderNumber
        {
            get { return data?.OrderNumber ?? -1; }
            set { data.OrderNumber = value; }
        }

        #endregion

        #region Cross section

        [Category("Cross section properties")]
        [PropertyOrder(0)]
        [DisplayName("Name")]
        public string CrossSectionName
        {
            get { return data?.SewerProfileDefinition?.ToString() ?? string.Empty; }
        }

        [Category("Cross section properties")]
        [PropertyOrder(1)]
        [DisplayName("Shape")]
        public string CrossSectionShape
        {
            get { return data?.SewerProfileDefinition?.Shape?.Type.ToString() ?? string.Empty; }
        }

        [Category("Cross section properties")]
        [PropertyOrder(2)]
        [DisplayName("Diameter")]
        [DynamicVisible]
        public double CrossSectionDiameter
        {
            get { return data?.SewerProfileDefinition?.GetProfileDiameter() ?? double.NaN; }
        }

        [Category("Cross section properties")]
        [PropertyOrder(3)]
        [DisplayName("Width")]
        [DynamicVisible]
        public double CrossSectionWidth
        {
            get { return data?.SewerProfileDefinition?.GetProfileWidth() ?? double.NaN; }
        }

        [Category("Cross section properties")]
        [PropertyOrder(4)]
        [DisplayName("Height")]
        [DynamicVisible]
        public double CrossSectionHeight
        {
            get { return data?.SewerProfileDefinition?.GetProfileHeight() ?? double.NaN; }
        }

        [Category("Cross section properties")]
        [PropertyOrder(5)]
        [DisplayName("Arch height")]
        [DynamicVisible]
        public double ArcHeight
        {
            get { return data?.SewerProfileDefinition?.GetProfileArchHeight() ?? double.NaN; }
        }

        [Category("Cross section properties")]
        [PropertyOrder(6)]
        [DynamicVisible]
        public double Slope
        {
            get { return data?.SewerProfileDefinition?.GetProfileSlope() ?? double.NaN; }
        }

        [Category("Cross section properties")]
        [PropertyOrder(7)]
        [DynamicVisible]
        public double BottomWidthB
        {
            get { return data?.SewerProfileDefinition?.GetProfileBottomWidthB() ?? double.NaN; }
        }

        [Category("Cross section properties")]
        [PropertyOrder(8)]
        [DynamicVisible]
        public double MaximumFlowWidth
        {
            get { return data?.SewerProfileDefinition?.GetProfileMaximumFlowWidth() ?? double.NaN; }
        }

        [DynamicVisibleValidationMethod]
        public bool IsVisible(string propertyName)
        {
            switch (propertyName)
            {
                case "CrossSectionDiameter":
                    return data.SewerProfileDefinition.Shape is CrossSectionStandardShapeRound;
                case "CrossSectionWidth":
                case "CrossSectionHeight":
                    var shape = data.SewerProfileDefinition.Shape;
                    return shape is CrossSectionStandardShapeWidthHeightBase || shape is CrossSectionStandardShapeArch;
                case "ArcHeight":
                    return data.SewerProfileDefinition.Shape is CrossSectionStandardShapeArch;
                case "Slope":
                case "BottomWidthB":
                case "MaximumFlowWidth":
                    return data.SewerProfileDefinition.Shape is CrossSectionStandardShapeTrapezium;
            }
            return true;
        }

        #endregion
    }
}
