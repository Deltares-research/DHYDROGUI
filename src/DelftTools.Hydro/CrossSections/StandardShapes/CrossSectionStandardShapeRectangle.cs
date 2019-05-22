using DelftTools.Hydro.Helpers;
using DelftTools.Utils.Aop;

namespace DelftTools.Hydro.CrossSections.StandardShapes
{
    [Entity(FireOnCollectionChange=false)]
    public class CrossSectionStandardShapeRectangle : CrossSectionStandardShapeWidthHeightBase
    {
        public static CrossSectionStandardShapeRectangle CreateDefault()
        {
            return new CrossSectionStandardShapeRectangle
                       {
                           Height = 1, Width = 1
                       };
        }

        public override CrossSectionStandardShapeType Type
        {
            get { return CrossSectionStandardShapeType.Rectangle; }
        }

        public override CrossSectionDefinitionZW GetTabulatedDefinition()
        {
            return StandardCrossSectionsFactory.GetTabulatedCrossSectionFromRectangle(Width,Height,true);
        }
    }
}
