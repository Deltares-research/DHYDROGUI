using System;
using DelftTools.Hydro.Helpers;
using DelftTools.Utils.Aop;

namespace DelftTools.Hydro.CrossSections.StandardShapes
{
    [Obsolete("D3DFMIQ-1923 remove cross section")]
    [Entity(FireOnCollectionChange = false)]
    public class CrossSectionStandardShapeSteelCunette : CrossSectionStandardShapeBase
    {
        public override CrossSectionStandardShapeType Type => CrossSectionStandardShapeType.SteelCunette;

        public virtual double Height { get; set; }
        public virtual double RadiusR { get; set; }
        public virtual double RadiusR1 { get; set; }
        public virtual double RadiusR2 { get; set; }
        public virtual double RadiusR3 { get; set; }
        public virtual double AngleA { get; set; }
        public virtual double AngleA1 { get; set; }

        public static CrossSectionStandardShapeSteelCunette CreateDefault()
        {
            return new CrossSectionStandardShapeSteelCunette
            {
                AngleA = 28,
                AngleA1 = 0,
                Height = 0.78,
                RadiusR = 0.5,
                RadiusR1 = 0.8,
                RadiusR2 = 0.2,
                RadiusR3 = 0
            };
        }

        public override CrossSectionDefinitionZW GetTabulatedDefinition()
        {
            return StandardCrossSectionsFactory.GetTabulatedCrossSectionFromSteelCunette(Height, RadiusR, RadiusR1,
                                                                                         RadiusR2, RadiusR3, AngleA,
                                                                                         AngleA1);
        }
    }
}