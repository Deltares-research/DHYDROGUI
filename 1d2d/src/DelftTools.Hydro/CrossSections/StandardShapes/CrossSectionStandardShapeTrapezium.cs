using System;
using DelftTools.Hydro.Helpers;
using DelftTools.Utils.Aop;

namespace DelftTools.Hydro.CrossSections.StandardShapes
{
    [Entity(FireOnCollectionChange=false)]
    public class CrossSectionStandardShapeTrapezium : CrossSectionStandardShapeBase
    {
        public override CrossSectionStandardShapeType Type
        {
            get { return CrossSectionStandardShapeType.Trapezium; }
        }


        public override CrossSectionDefinitionZW GetTabulatedDefinition()
        {
            return StandardCrossSectionsFactory.GetTabulatedCrossSectionFromTrapezium(Slope, BottomWidthB, MaximumFlowWidth);
        }

        private double slope;
        public virtual double Slope
        {
            get { return slope; }
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Slope should be greater than 0");
                slope = value;
            }
        }

        public virtual double BottomWidthB { get; set; }
        public virtual double MaximumFlowWidth { get; set; }
       
        public static CrossSectionStandardShapeTrapezium CreateDefault()
        {
            return new CrossSectionStandardShapeTrapezium {Slope = 2, BottomWidthB = 10, MaximumFlowWidth = 20};
        }
    }
}