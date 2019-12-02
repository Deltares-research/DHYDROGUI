using System;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    [Entity]
    public class PipeShape : DrawingShape
    {
        public IPipe Pipe { get; set; }

        public CompartmentShape ConnectedCompartmentShape { get; set; }

        public override object Source
        {
            get { return Pipe; }
            set { Pipe = value as IPipe; }
        }

        public override double TopLevel
        {
            get { return BottomLevel + Height; }
            set { }
        }
        public override double BottomLevel
        {
            get { return CalculateBottomLevel(); }
            set { }
        }

        public override double Width
        {
            get { return GetPipeWidth(); }
            set { }
        }

        public override double Height
        {
            get { return GetPipeHeight(); }
            set { }
        }

        private double CalculateBottomLevel()
        {
            var connectedCompartment = ConnectedCompartmentShape?.Compartment;

            if (Pipe == null || connectedCompartment == null) return double.NaN;

            if (connectedCompartment == Pipe.SourceCompartment)
            {
                return Pipe.LevelSource;
            }

            return connectedCompartment == Pipe.TargetCompartment ? Pipe.LevelTarget : double.NaN;
        }

        private double GetPipeWidth()
        {
            var shape = Pipe?.Profile?.Shape;
            if (shape == null) return 0;
            var rectangleShape = shape as CrossSectionStandardShapeWidthHeightBase;
            if (rectangleShape != null)
            {
                return rectangleShape.Width;
            }

            var roundShape = shape as CrossSectionStandardShapeCircle;
            if (roundShape != null)
            {
                return roundShape.Diameter;
            }

            throw new ArgumentException($"Pipe shape {shape?.Type} is not yet supported");
        }

        private double GetPipeHeight()
        {
            var shape = Pipe?.Profile?.Shape;
            if (shape == null) return 0;
            var rectangleShape = shape as CrossSectionStandardShapeWidthHeightBase;
            if (rectangleShape != null)
            {
                return rectangleShape.Height;
            }

            var roundShape = shape as CrossSectionStandardShapeCircle;
            if (roundShape != null)
            {
                return roundShape.Diameter;
            }

            throw new ArgumentException($"Sewer pipe shape {shape?.Type} is not yet supported");
        }
    }
}