using System;
using DelftTools.Hydro.Helpers;
using DelftTools.Utils.Aop;

namespace DelftTools.Hydro.CrossSections.StandardShapes
{
    [Obsolete("D3DFMIQ-1923 remove cross section")]
    [Entity(FireOnCollectionChange = false)]
    public class CrossSectionStandardShapeElliptical : CrossSectionStandardShapeWidthHeightBase
    {
        public override CrossSectionStandardShapeType Type => CrossSectionStandardShapeType.Elliptical;

        public static CrossSectionStandardShapeElliptical CreateDefault()
        {
            return new CrossSectionStandardShapeElliptical
            {
                Height = 1,
                Width = 1
            };
        }

        public override CrossSectionDefinitionZW GetTabulatedDefinition()
        {
            return StandardCrossSectionsFactory.GetTabulatedCrossSectionFromEllipse(Width, Height);
        }
    }
}