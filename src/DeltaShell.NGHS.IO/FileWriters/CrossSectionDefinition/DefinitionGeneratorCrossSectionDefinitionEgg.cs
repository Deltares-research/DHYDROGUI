using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.Location;

namespace DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition
{
    public class DefinitionGeneratorCrossSectionDefinitionEgg : DefinitionGeneratorCrossSectionDefinitionStandardTemplate
    {
        public DefinitionGeneratorCrossSectionDefinitionEgg() : base(CrossSectionRegion.CrossSectionDefinitionType.Egg)
        {
        }

        protected DefinitionGeneratorCrossSectionDefinitionEgg(string subEggType) : base(subEggType)
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

            IniCategory.AddProperty(DefinitionPropertySettings.EggWidth, eggShape.Width);
            IniCategory.AddProperty(DefinitionPropertySettings.EggHeight, eggShape.Height);
        }
    }
}