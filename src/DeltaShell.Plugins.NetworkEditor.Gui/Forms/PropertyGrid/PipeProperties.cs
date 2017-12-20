using System;
using System.ComponentModel;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.ComponentModel;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    //[DisplayName("Pipe information")]
    public class PipeProperties : ObjectProperties<Pipe>
    {
        #region Connection properties

        [Category("Connection properties")]
        [PropertyOrder(0)]
        public string Name
        {
            get { return data.Name; }
            set { data.Name = value; }
        }

        [Category("Connection properties")]
        [PropertyOrder(1)]
        [DisplayName("Source manhole")]
        public string FromManhole
        {
            get { return data.Source.ToString(); }
        }

        [Category("Connection properties")]
        [PropertyOrder(2)]
        [DisplayName("Target manhole")]
        public string ToManhole
        {
            get { return data.Target.ToString(); }
        }

        [Category("Connection properties")]
        [PropertyOrder(3)]
        [DisplayName("Source compartment")]
        public string FromCompartment
        {
            get { return data.SourceCompartment.ToString(); }
        }

        [Category("Connection properties")]
        [PropertyOrder(4)]
        [DisplayName("Target compartment")]
        public string ToCompartment
        {
            get { return data.TargetCompartment.ToString(); }
        }

        [Category("Connection properties")]
        [PropertyOrder(5)]
        [DisplayName("Level source")]
        public double LevelStart
        {
            get { return data.LevelSource; }
        }

        [Category("Connection properties")]
        [PropertyOrder(6)]
        [DisplayName("Level target")]
        public double LevelTarget
        {
            get { return data.LevelTarget; }
        }

        [Category("Connection properties")]
        [PropertyOrder(7)]
        [DisplayName("Water type")]
        public string WaterType
        {
            get { return data.WaterType.ToString(); }
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
            get { return data.OrderNumber; }
            set { data.OrderNumber = value; }
        }

        #endregion

        #region Cross section

        [Category("Cross section properties")]
        [PropertyOrder(0)]
        [DisplayName("Name")]
        public string CrossSectionName
        {
            get { return data.SewerProfileDefinition.ToString(); }
        }

        [Category("Cross section properties")]
        [PropertyOrder(1)]
        [DisplayName("Shape")]
        public string CrossSectionShape
        {
            get { return data.SewerProfileDefinition.Shape.Type.ToString(); }
        }

        [Category("Cross section properties")]
        [PropertyOrder(2)]
        [DisplayName("Diameter")]
        [DynamicVisible]
        public double CrossSectionDiameter
        {
            get
            {
                var roundShape = data.SewerProfileDefinition.Shape as CrossSectionStandardShapeRound;
                return roundShape?.Diameter ?? double.NaN;
            }
        }

        [Category("Cross section properties")]
        [PropertyOrder(3)]
        [DisplayName("Width")]
        [DynamicVisible]
        public double CrossSectionWidth
        {
            get
            {
                var shape = data.SewerProfileDefinition.Shape;
                var widthBasedShape = shape as CrossSectionStandardShapeWidthHeightBase;
                if(widthBasedShape != null) return Math.Round(widthBasedShape.Width, 2, MidpointRounding.AwayFromZero);

                var archShape = shape as CrossSectionStandardShapeArch;
                if (archShape != null) return Math.Round(archShape.Width, 2, MidpointRounding.AwayFromZero);

                return double.NaN;
            }
        }

        [Category("Cross section properties")]
        [PropertyOrder(4)]
        [DisplayName("Height")]
        [DynamicVisible]
        public double CrossSectionHeight
        {
            get
            {
                var shape = data.SewerProfileDefinition.Shape;
                var widthBasedShape = shape as CrossSectionStandardShapeWidthHeightBase;
                if (widthBasedShape != null) return Math.Round(widthBasedShape.Height, 2, MidpointRounding.AwayFromZero);

                var archShape = shape as CrossSectionStandardShapeArch;
                return archShape != null ? Math.Round(archShape.Height, 2, MidpointRounding.AwayFromZero) : double.NaN;
            }
        }

        [Category("Cross section properties")]
        [PropertyOrder(5)]
        [DisplayName("Arch height")]
        [DynamicVisible]
        public double ArcHeight
        {
            get
            {
                var archShape = data.SewerProfileDefinition.Shape as CrossSectionStandardShapeArch;
                return archShape != null ? Math.Round(archShape.Height, 2, MidpointRounding.AwayFromZero) : double.NaN;
            }
        }

        [Category("Cross section properties")]
        [PropertyOrder(6)]
        [DynamicVisible]
        public double Slope
        {
            get
            {
                var trapezoidShape = data.SewerProfileDefinition.Shape as CrossSectionStandardShapeTrapezium;
                return trapezoidShape != null ? Math.Round(trapezoidShape.Slope, 2, MidpointRounding.AwayFromZero) : double.NaN;
            }
        }

        [Category("Cross section properties")]
        [PropertyOrder(7)]
        [DynamicVisible]
        public double BottomWidthB
        {
            get
            {
                var trapezoidShape = data.SewerProfileDefinition.Shape as CrossSectionStandardShapeTrapezium;
                return trapezoidShape != null ? Math.Round(trapezoidShape.BottomWidthB, 2, MidpointRounding.AwayFromZero) : double.NaN;
            }
        }

        [Category("Cross section properties")]
        [PropertyOrder(8)]
        [DynamicVisible]
        public double MaximumFlowWidth
        {
            get
            {
                var trapezoidShape = data.SewerProfileDefinition.Shape as CrossSectionStandardShapeTrapezium;
                return trapezoidShape != null ? Math.Round(trapezoidShape.MaximumFlowWidth, 2, MidpointRounding.AwayFromZero) : double.NaN;
            }
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
