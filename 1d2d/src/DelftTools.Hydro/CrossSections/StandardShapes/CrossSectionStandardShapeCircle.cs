using DelftTools.Hydro.Helpers;
using DelftTools.Utils.Aop;

namespace DelftTools.Hydro.CrossSections.StandardShapes
{
    [Entity(FireOnCollectionChange=false)]
    public class CrossSectionStandardShapeCircle : CrossSectionStandardShapeBase
    {
        public static CrossSectionStandardShapeCircle CreateDefault()
        {
            return new CrossSectionStandardShapeCircle
            {
                Diameter = 0.160d
            };
        }

        public override CrossSectionStandardShapeType Type
        {
            get
            {
                return CrossSectionStandardShapeType.Circle;
            }
        }

        public override CrossSectionDefinitionZW GetTabulatedDefinition()
        {
            return StandardCrossSectionsFactory.GetTabulatedCrossSectionFromCircle(Diameter);
        }

        public virtual double Diameter { get; set; }

    }
}
