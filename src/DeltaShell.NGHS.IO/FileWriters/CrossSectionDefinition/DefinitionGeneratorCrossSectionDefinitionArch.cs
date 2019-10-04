using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.Location;

namespace DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition
{
    public class DefinitionGeneratorCrossSectionDefinitionArch : DefinitionGeneratorCrossSectionDefinitionStandardTemplate
    {
        public DefinitionGeneratorCrossSectionDefinitionArch() : base(CrossSectionRegion.CrossSectionDefinitionType.Arch)
        {
        }

        protected DefinitionGeneratorCrossSectionDefinitionArch(string subArchType) : base(subArchType)
        {
        }

        protected override bool HasCorrectCrossSectionShape(CrossSectionDefinitionStandard standardDefinition)
        {
            var shapeCircle = standardDefinition.Shape as CrossSectionStandardShapeArch;
            return shapeCircle != null;
        }

        protected override void AddShapeMeasurementProperties(ICrossSectionStandardShape shape)
        {
            var archShape = shape as CrossSectionStandardShapeArch;
            if (archShape == null) return;

            IniCategory.AddProperty(DefinitionPropertySettings.ArchCrossSectionWidth, archShape.Width);
            IniCategory.AddProperty(DefinitionPropertySettings.ArchCrossSectionHeight, archShape.Height);
            IniCategory.AddProperty(DefinitionPropertySettings.ArchHeight, archShape.ArcHeight);
        }
    }
}