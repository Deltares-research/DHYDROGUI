using System;
using DelftTools.Hydro;
using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    [Entity]
    public class ConnectionShape : DrawingShape
    {
        //private Compartment sourceCompartment;
        //private Compartment targetCompartment;
        //private CompartmentShape sourceCompartmentShape;
        //private CompartmentShape targetCompartmentShape;

        public virtual ISewerConnection SewerConnection { get; set; }

        /*public CompartmentShape SourceCompartmentShape
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
*/     
        public override double Width { get; set; }
        
        public override double Height
        {
            get { return TopLevel - BottomLevel; }
            set { }
        }

        /*protected double GetWidthBasedOnCompartments()
        {
            if (sourceCompartmentShape == null || targetCompartmentShape == null) return 0;
            var sourceIsLeft = sourceCompartmentShape.LeftOffset < targetCompartmentShape.LeftOffset;

            var leftShape = sourceIsLeft ? sourceCompartmentShape : targetCompartmentShape;
            var rightShape = sourceIsLeft ? targetCompartmentShape : sourceCompartmentShape;

            return rightShape.LeftOffset - (leftShape.LeftOffset + leftShape.Width);
        }*/

        protected double GetTopLevelBasedOnCompartments()
        {
            var sc = SewerConnection?.SourceCompartment;
            var tc = SewerConnection?.TargetCompartment;

            if (sc == null || tc == null)
            {
                return topLevel;
            }

            topLevel = Math.Min(sc.SurfaceLevel, tc.SurfaceLevel);
            return topLevel;
        }

        private double topLevel;
        private double bottomLevel;

        protected double GetBottomLevelBasedOnCompartments()
        {
            var sc = SewerConnection?.SourceCompartment;
            var tc = SewerConnection?.TargetCompartment;

            if (sc == null || tc == null)
            {
                return bottomLevel;
            }

            bottomLevel = Math.Max(sc.BottomLevel, tc.BottomLevel);
            return bottomLevel;
        }
    }
}