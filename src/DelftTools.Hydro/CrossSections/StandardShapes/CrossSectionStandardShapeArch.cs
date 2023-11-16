using DelftTools.Hydro.Helpers;
using DelftTools.Utils.Aop;

namespace DelftTools.Hydro.CrossSections.StandardShapes
{
    [Entity(FireOnCollectionChange=false)]
    public class CrossSectionStandardShapeArch : CrossSectionStandardShapeBase
    {
        public override CrossSectionStandardShapeType Type
        {
            get { return CrossSectionStandardShapeType.Arch; }
        }


        public override CrossSectionDefinitionZW GetTabulatedDefinition()
        {
            return StandardCrossSectionsFactory.GetTabulatedCrossSectionFromArch(Width, Height, ArcHeight);
        }

        
        public static CrossSectionStandardShapeArch CreateDefault()
        {
            return new CrossSectionStandardShapeArch {ArcHeight = 1, Height = 2, Width = 1};
        }

        public virtual double Width { get; set; }
        public virtual double Height { get; set; }
        private double arcHeight;
        public virtual double ArcHeight
        {
            get { return arcHeight; }
            set
            {
                arcHeight = value;
                AfterArcHeightSet();
            }
        }

        private void AfterArcHeightSet()
        {
            if (ArcHeight > Height)
            {
                Height = ArcHeight;
            }
        }
    }
}