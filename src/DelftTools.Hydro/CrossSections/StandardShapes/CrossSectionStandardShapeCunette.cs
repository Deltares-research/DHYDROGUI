using System;
using DelftTools.Hydro.Helpers;
using DelftTools.Utils.Aop;

namespace DelftTools.Hydro.CrossSections.StandardShapes
{
    [Obsolete("D3DFMIQ-1923 remove cross section")]
    [Entity(FireOnCollectionChange = false)]
    public class CrossSectionStandardShapeCunette : CrossSectionStandardShapeWidthHeightBase
    {
        private double height;

        private double width;

        public override CrossSectionStandardShapeType Type => CrossSectionStandardShapeType.Cunette;

        /// <summary>
        /// Sets the height and auto-scales the width so width/height ratio remains 1/1.577
        /// </summary>
        public override double Height
        {
            get => height;
            set
            {
                height = value;
                width = (height * 1) / 0.634;
            }
        }

        /// <summary>
        /// Sets the width and auto-scales the height so width/height ratio remains 1/1.577
        /// </summary>
        public override double Width
        {
            get => width;
            set
            {
                width = value;
                height = 0.634 * width;
            }
        }

        public static CrossSectionStandardShapeCunette CreateDefault()
        {
            return new CrossSectionStandardShapeCunette {Width = 1};
        }

        public override CrossSectionDefinitionZW GetTabulatedDefinition()
        {
            return StandardCrossSectionsFactory.GetTabulatedCrossSectionFromCunette(Width, Height);
        }
    }
}