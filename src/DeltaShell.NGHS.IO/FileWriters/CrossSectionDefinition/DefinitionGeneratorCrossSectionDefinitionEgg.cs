using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.Location;

namespace DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition
{
    public class DefinitionGeneratorCrossSectionDefinitionEgg : DefinitionGeneratorCrossSectionDefinitionStandard
    {
        public DefinitionGeneratorCrossSectionDefinitionEgg() : base(CrossSectionRegion.CrossSectionDefinitionType.Egg)
        {
        }

        protected override bool HasCorrectCrossSectionShape(CrossSectionDefinitionStandard standardDefinition)
        {
            var eggShape = standardDefinition.Shape as CrossSectionStandardShapeEgg;
            return eggShape != null;
        }

        protected override void AddShapeMeasurementProperties(ICrossSectionStandardShape shape)
        {
            var eggShape = shape as CrossSectionStandardShapeEgg;
            if (eggShape == null) return;

            IniCategory.AddProperty(DefinitionPropertySettings.Diameter, eggShape.Width);
        }
    }
}