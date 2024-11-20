using DelftTools.Hydro.Helpers;
using DelftTools.Utils.Aop;

namespace DelftTools.Hydro.CrossSections.StandardShapes
{
    [Entity(FireOnCollectionChange=false)]
    public class CrossSectionStandardShapeElliptical : CrossSectionStandardShapeWidthHeightBase
    {
        public static CrossSectionStandardShapeElliptical CreateDefault()
        {
            return new CrossSectionStandardShapeElliptical
            {
                Height = 1,
                Width = 1
            };
        }

        public override CrossSectionStandardShapeType Type
        {
            get { return CrossSectionStandardShapeType.Elliptical; }
        }

        
        public override CrossSectionDefinitionZW GetTabulatedDefinition()
        {
            return StandardCrossSectionsFactory.GetTabulatedCrossSectionFromEllipse(Width,Height);
        }
    }
}