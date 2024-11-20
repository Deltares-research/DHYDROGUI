using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition
{
    public class DefinitionGeneratorCrossSectionDefinitionSteelCunette : DefinitionGeneratorCrossSectionDefinitionStandardTemplate
    {
        public DefinitionGeneratorCrossSectionDefinitionSteelCunette() : base(CrossSectionRegion.CrossSectionDefinitionType.SteelMouth)
        {
        }

        protected override bool HasCorrectCrossSectionShape(CrossSectionDefinitionStandard standardDefinition)
        {
            var shapeSteelCunette = standardDefinition.Shape as CrossSectionStandardShapeSteelCunette;
            return shapeSteelCunette != null;
        }

        protected override void AddShapeMeasurementProperties(ICrossSectionStandardShape shape)
        {
            var steelCunetteShape = shape as CrossSectionStandardShapeSteelCunette;
            if (steelCunetteShape == null) return;

            IniSection.AddPropertyFromConfiguration(DefinitionPropertySettings.SteelCunetteHeight, steelCunetteShape.Height);
            IniSection.AddPropertyFromConfiguration(DefinitionPropertySettings.SteelCunetteR, steelCunetteShape.RadiusR);
            IniSection.AddPropertyFromConfiguration(DefinitionPropertySettings.SteelCunetteR1, steelCunetteShape.RadiusR1);
            IniSection.AddPropertyFromConfiguration(DefinitionPropertySettings.SteelCunetteR2, steelCunetteShape.RadiusR2);
            IniSection.AddPropertyFromConfiguration(DefinitionPropertySettings.SteelCunetteR3, steelCunetteShape.RadiusR3);
            IniSection.AddPropertyFromConfiguration(DefinitionPropertySettings.SteelCunetteA, steelCunetteShape.AngleA);
            IniSection.AddPropertyFromConfiguration(DefinitionPropertySettings.SteelCunetteA1, steelCunetteShape.AngleA1);
        }
    }
}