using DelftTools.Hydro.Helpers;
using DelftTools.Utils.Aop;

namespace DelftTools.Hydro.CrossSections.StandardShapes
{
    [Entity(FireOnCollectionChange = false)]
    public class CrossSectionStandardShapeUShape : CrossSectionStandardShapeArch
    {
        public override CrossSectionStandardShapeType Type
        {
            get { return CrossSectionStandardShapeType.UShape; }
        }


        public override CrossSectionDefinitionZW GetTabulatedDefinition()
        {
            return StandardCrossSectionsFactory.GetTabulatedCrossSectionFromUShape(Width, Height, ArcHeight);
        }


        public static CrossSectionStandardShapeUShape CreateDefault()
        {
            return new CrossSectionStandardShapeUShape {ArcHeight = 1, Height = 2, Width = 1};
        }
    }
}