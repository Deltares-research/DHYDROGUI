using DelftTools.Hydro.Helpers;
using DelftTools.Utils.Aop;

namespace DelftTools.Hydro.CrossSections.StandardShapes
{
    [Entity(FireOnCollectionChange = false)]
    public class CrossSectionStandardShapeInvertedEgg : CrossSectionStandardShapeEgg
    {
        public static CrossSectionStandardShapeInvertedEgg CreateDefault()
        {
            return new CrossSectionStandardShapeInvertedEgg
            {
                Height = 3
            };
        }
        public override CrossSectionStandardShapeType Type
        {
            get
            {
                return CrossSectionStandardShapeType.InvertedEgg;
                //throw new NotImplementedException("Not implemented yet. Wait for closed cross-sections");
            }
        }

        public override CrossSectionDefinitionZW GetTabulatedDefinition()
        {
            return StandardCrossSectionsFactory.GetTabulatedCrossSectionFromInvertedEgg(Width);
        }
    }
}