using System;
using DelftTools.Hydro.Helpers;
using DelftTools.Utils.Aop;

namespace DelftTools.Hydro.CrossSections.StandardShapes
{
    [Entity(FireOnCollectionChange = false)]
    [Obsolete("D3DFMIQ-1923 remove cross section")]
    public class CrossSectionStandardShapeArch : CrossSectionStandardShapeBase
    {
        private double arcHeight;
        public override CrossSectionStandardShapeType Type => CrossSectionStandardShapeType.Arch;

        public virtual double Width { get; set; }
        public virtual double Height { get; set; }

        public virtual double ArcHeight
        {
            get => arcHeight;
            set
            {
                arcHeight = value;
                AfterArcHeightSet();
            }
        }

        public static CrossSectionStandardShapeArch CreateDefault()
        {
            return new CrossSectionStandardShapeArch
            {
                ArcHeight = 1,
                Height = 2,
                Width = 1
            };
        }

        public override CrossSectionDefinitionZW GetTabulatedDefinition()
        {
            return StandardCrossSectionsFactory.GetTabulatedCrossSectionFromArch(Width, Height, ArcHeight);
        }

        [EditAction]
        private void AfterArcHeightSet()
        {
            if (ArcHeight > Height)
            {
                Height = ArcHeight;
            }
        }
    }
}