using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.Location;

namespace DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition
{
    internal class DefinitionGeneratorCrossSectionDefinitionSteelCunette : DefinitionGeneratorCrossSectionDefinitionStandard
    {
        public DefinitionGeneratorCrossSectionDefinitionSteelCunette() : base(CrossSectionRegion.CrossSectionDefinitionType.SteelCunette)
        {
            GenerateProfileProperties = true;
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

            IniCategory.AddProperty(DefinitionPropertySettings.SteelCunetteHeight, steelCunetteShape.Height);
            IniCategory.AddProperty(DefinitionPropertySettings.SteelCunetteR, steelCunetteShape.RadiusR);
            IniCategory.AddProperty(DefinitionPropertySettings.SteelCunetteR1, steelCunetteShape.RadiusR1);
            IniCategory.AddProperty(DefinitionPropertySettings.SteelCunetteR2, steelCunetteShape.RadiusR2);
            IniCategory.AddProperty(DefinitionPropertySettings.SteelCunetteR3, steelCunetteShape.RadiusR3);
            IniCategory.AddProperty(DefinitionPropertySettings.SteelCunetteA, steelCunetteShape.AngleA);
            IniCategory.AddProperty(DefinitionPropertySettings.SteelCunetteA1, steelCunetteShape.AngleA1);
        }
    }
}