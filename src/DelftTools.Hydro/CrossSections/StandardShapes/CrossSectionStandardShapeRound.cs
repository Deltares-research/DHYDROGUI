using DelftTools.Hydro.Helpers;
using DelftTools.Utils.Aop;

namespace DelftTools.Hydro.CrossSections.StandardShapes
{
    [Entity(FireOnCollectionChange = false)]
    public class CrossSectionStandardShapeRound : CrossSectionStandardShapeBase
    {
        public override CrossSectionStandardShapeType Type => CrossSectionStandardShapeType.Round;

        public virtual double Diameter { get; set; }

        public static CrossSectionStandardShapeRound CreateDefault()
        {
            return new CrossSectionStandardShapeRound {Diameter = 0.160d};
        }

        public override CrossSectionDefinitionZW GetTabulatedDefinition()
        {
            return StandardCrossSectionsFactory.GetTabulatedCrossSectionFromCircle(Diameter);
        }
    }
}