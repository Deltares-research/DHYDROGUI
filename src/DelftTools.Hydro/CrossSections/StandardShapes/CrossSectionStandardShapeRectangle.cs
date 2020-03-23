using DelftTools.Hydro.Helpers;
using DelftTools.Utils.Aop;

namespace DelftTools.Hydro.CrossSections.StandardShapes
{
    [Entity(FireOnCollectionChange=false)]
    public class CrossSectionStandardShapeRectangle : CrossSectionStandardShapeWidthHeightBase, ICrossSectionStandardShapeOpenClosed
    {
        public static CrossSectionStandardShapeRectangle CreateDefault()
        {
            return new CrossSectionStandardShapeRectangle
                       {
                           Height = 1, Width = 1, Closed = true
                       };
        }

        public override CrossSectionStandardShapeType Type
        {
            get { return CrossSectionStandardShapeType.Rectangle; }
        }

        public override CrossSectionDefinitionZW GetTabulatedDefinition()
        {
            return StandardCrossSectionsFactory.GetTabulatedCrossSectionFromRectangle(Width, Height, Closed);
        }

        public override bool Closed { get; set; }
    }
}
