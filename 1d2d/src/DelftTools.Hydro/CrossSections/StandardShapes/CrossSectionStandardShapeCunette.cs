using DelftTools.Hydro.Helpers;
using DelftTools.Utils.Aop;

namespace DelftTools.Hydro.CrossSections.StandardShapes
{
    [Entity(FireOnCollectionChange=false)]
    public class CrossSectionStandardShapeCunette : CrossSectionStandardShapeWidthHeightBase
    {
        public static CrossSectionStandardShapeCunette CreateDefault()
        {
            return new CrossSectionStandardShapeCunette
            {
                Width = 1
            };
        }

        public override CrossSectionStandardShapeType Type
        {
            get { return CrossSectionStandardShapeType.Cunette; }
        }

        public override CrossSectionDefinitionZW GetTabulatedDefinition()
        {
            return StandardCrossSectionsFactory.GetTabulatedCrossSectionFromCunette(Width,Height);
        }

        private double height;

        /// <summary>
        /// Sets the height and auto-scales the width so width/height ratio remains 1/1.577
        /// </summary>
        public override double Height
        {
            get { return height; }
            set
            {
                height = value;
                width = height * 1 / 0.634;
            }
        }

        private double width;

        /// <summary>
        /// Sets the width and auto-scales the height so width/height ratio remains 1/1.577
        /// </summary>
        public override double Width
        {
            get { return width; }
            set
            {
                width = value;
                height = 0.634 * width;
            }
        }

    }
}