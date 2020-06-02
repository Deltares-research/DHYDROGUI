using System;
using DelftTools.Hydro.Helpers;
using DelftTools.Utils.Aop;

namespace DelftTools.Hydro.CrossSections.StandardShapes
{
    [Entity(FireOnCollectionChange = false)]
    public class CrossSectionStandardShapeTrapezium : CrossSectionStandardShapeBase
    {
        private double slope;
        public override CrossSectionStandardShapeType Type => CrossSectionStandardShapeType.Trapezium;

        public virtual double Slope
        {
            get => slope;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("Slope should be greater than 0");
                }

                slope = value;
            }
        }

        public virtual double BottomWidthB { get; set; }
        public virtual double MaximumFlowWidth { get; set; }

        public static CrossSectionStandardShapeTrapezium CreateDefault()
        {
            return new CrossSectionStandardShapeTrapezium
            {
                Slope = 2,
                BottomWidthB = 10,
                MaximumFlowWidth = 20
            };
        }

        public override CrossSectionDefinitionZW GetTabulatedDefinition()
        {
            return StandardCrossSectionsFactory.GetTabulatedCrossSectionFromTrapezium(
                Slope, BottomWidthB, MaximumFlowWidth);
        }
    }
}