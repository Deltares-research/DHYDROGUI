using DelftTools.Hydro.Helpers;
using DelftTools.Utils.Aop;

namespace DelftTools.Hydro.CrossSections.StandardShapes
{
    [Entity(FireOnCollectionChange=false)]
    public class CrossSectionStandardShapeEgg : CrossSectionStandardShapeWidthHeightBase
    {
        public static CrossSectionStandardShapeEgg CreateDefault()
        {
            return new CrossSectionStandardShapeEgg
            {
                Height = 3
            };
        }

        public override CrossSectionStandardShapeType Type
        {
            get 
            {
                 return CrossSectionStandardShapeType.Egg;
            }
        }

        
        public override CrossSectionDefinitionZW GetTabulatedDefinition()
        {
            return StandardCrossSectionsFactory.GetTabulatedCrossSectionFromEgg(Width);
        }

        private double height;

        /// <summary>
        /// Sets the height and auto-scales the width so width/height ratio remains 2/3
        /// </summary>
        public override double Height
        {
            get { return height; }
            set
            {
                height = value;
                width = height*2/3;
            }
        }

        private double width;
        
        /// <summary>
        /// Sets the width and auto-scales the height so width/height ratio remains 2/3
        /// </summary>
        public override double Width
        {
            get { return width; }
            set
            {
                width = value;
                height = 1.5*width;
            }
        }
    }
}