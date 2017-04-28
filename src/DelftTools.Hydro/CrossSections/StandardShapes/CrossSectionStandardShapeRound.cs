using DelftTools.Hydro.Helpers;
using DelftTools.Utils.Aop;

namespace DelftTools.Hydro.CrossSections.StandardShapes
{
    [Entity(FireOnCollectionChange=false)]
    public class CrossSectionStandardShapeRound : CrossSectionStandardShapeBase
    {
        public static CrossSectionStandardShapeRound CreateDefault()
        {
            return new CrossSectionStandardShapeRound
                       {
                           Diameter = 0.160d
                       };
        }

        public override CrossSectionStandardShapeType Type
        {
            get
            {
                return CrossSectionStandardShapeType.Round;
                //throw new NotImplementedException("Not implemented yet. Wait for closed cross-sections");
            }
        }

        public override CrossSectionDefinitionZW GetTabulatedDefinition()
        {
            return StandardCrossSectionsFactory.GetTabulatedCrossSectionFromCircle(Diameter);
        }

        public virtual double Diameter { get; set; }

    }
}
