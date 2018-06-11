using System;
using DelftTools.Hydro;
using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    [Entity]
    public class ConnectionShape : DrawingShape
    {
        private Compartment sourceCompartment;
        private Compartment targetCompartment;
        private CompartmentShape sourceCompartmentShape;
        private CompartmentShape targetCompartmentShape;

        public CompartmentShape SourceCompartmentShape
        {
            get { return sourceCompartmentShape; }
            set
            {
                sourceCompartmentShape = value;
                sourceCompartment = sourceCompartmentShape?.Compartment;
            }
        }

        public CompartmentShape TargetCompartmentShape
        {
            get { return targetCompartmentShape; }
            set
            {
                targetCompartmentShape = value;
                targetCompartment = targetCompartmentShape?.Compartment;
            }
        }
     
        public override double Width
        {
            get { return GetWidthBasedOnCompartments(); }
            set { }
        }
        
        public override double Height
        {
            get { return TopLevel - BottomLevel; }
            set { }
        }

        protected double GetWidthBasedOnCompartments()
        {
            if (sourceCompartmentShape == null || targetCompartmentShape == null) return 0;
            var sourceIsLeft = sourceCompartmentShape.LeftOffset < targetCompartmentShape.LeftOffset;

            var leftShape = sourceIsLeft ? sourceCompartmentShape : targetCompartmentShape;
            var rightShape = sourceIsLeft ? targetCompartmentShape : sourceCompartmentShape;

            return rightShape.LeftOffset - (leftShape.LeftOffset + leftShape.Width);
        }

        protected double GetTopLevelBasedOnCompartments()
        {
            if (sourceCompartment == null || targetCompartment == null)
            {
                return double.NaN;
            }

            return Math.Min(sourceCompartment.SurfaceLevel, targetCompartment.SurfaceLevel);
        }

        protected double GetBottomLevelBasedOnCompartments()
        {
            if (sourceCompartment == null || targetCompartment == null)
            {
                return double.NaN;
            }

            return Math.Max(sourceCompartment.BottomLevel, targetCompartment.BottomLevel);
        }
    }
}