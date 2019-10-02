using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.Location;

namespace DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition
{
    public class DefinitionGeneratorCrossSectionDefinitionCunette : DefinitionGeneratorCrossSectionDefinitionStandardTemplate
    {
        public DefinitionGeneratorCrossSectionDefinitionCunette() : base(CrossSectionRegion.CrossSectionDefinitionType.Mouth)
        {
        }

        protected override bool HasCorrectCrossSectionShape(CrossSectionDefinitionStandard standardDefinition)
        {
            var cunetteShape = standardDefinition.Shape as CrossSectionStandardShapeCunette;
            return cunetteShape != null;
        }

        protected override void AddShapeMeasurementProperties(ICrossSectionStandardShape shape)
        {
            var cunetteShape = shape as CrossSectionStandardShapeCunette;
            if (cunetteShape == null) return;

            IniCategory.AddProperty(DefinitionPropertySettings.CunetteWidth, cunetteShape.Width);
            IniCategory.AddProperty(DefinitionPropertySettings.CunetteHeight, cunetteShape.Height);
        }
    }
}