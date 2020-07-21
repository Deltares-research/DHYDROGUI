using System;
using DelftTools.Hydro.Helpers;
using DelftTools.Utils.Aop;

namespace DelftTools.Hydro.CrossSections.StandardShapes
{
    [Obsolete("D3DFMIQ-1923 remove cross section")]
    [Entity(FireOnCollectionChange = false)]
    public class CrossSectionStandardShapeRectangle : CrossSectionStandardShapeWidthHeightBase
    {
        public override CrossSectionStandardShapeType Type => CrossSectionStandardShapeType.Rectangle;

        public static CrossSectionStandardShapeRectangle CreateDefault()
        {
            return new CrossSectionStandardShapeRectangle
            {
                Height = 1,
                Width = 1
            };
        }

        public override CrossSectionDefinitionZW GetTabulatedDefinition()
        {
            return StandardCrossSectionsFactory.GetTabulatedCrossSectionFromRectangle(Width, Height, true);
        }
    }
}