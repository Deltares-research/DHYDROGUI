using System;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    [Entity]
    public abstract class InternalConnectionShape : DrawingShape
    {
        public ISewerConnection SewerConnection { get; set; }

        public override double Height
        {
            get { return TopLevel - BottomLevel; }
            set { }
        }
        
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