using System;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.CrossSectionDefinitionGenerators
{
    public class CrossSectionDefinitionIniCategoryGeneratorSteelCunette : ACrossSectionDefinitionIniCategoryGenerator
    {
        public override DelftIniCategory GenerateIniCategory(CrossSectionDefinitionStandard crossSectionDefinition)
        {
            var crossSectionShape = crossSectionDefinition.Shape as CrossSectionStandardShapeSteelCunette;
            if (crossSectionShape == null) throw new Exception();

            return base.GenerateIniCategory(crossSectionDefinition);
        }

        protected override void AddMeasurementsProperties(ICrossSectionStandardShape crossSectionShape)
        {
            var steelCunetteShape = crossSectionShape as CrossSectionStandardShapeSteelCunette;
            iniCategory.AddProperty(DefinitionPropertySettings.SteelCunetteHeight.Key, $"{steelCunetteShape.Height:0.00}");
            iniCategory.AddProperty(DefinitionPropertySettings.SteelCunetteA.Key, $"{steelCunetteShape.AngleA:0.00}");
            iniCategory.AddProperty(DefinitionPropertySettings.SteelCunetteA1.Key, $"{steelCunetteShape.AngleA1:0.00}");
            iniCategory.AddProperty(DefinitionPropertySettings.SteelCunetteR.Key, $"{steelCunetteShape.RadiusR:0.00}");
            iniCategory.AddProperty(DefinitionPropertySettings.SteelCunetteR1.Key, $"{steelCunetteShape.RadiusR1:0.00}");
            iniCategory.AddProperty(DefinitionPropertySettings.SteelCunetteR2.Key, $"{steelCunetteShape.RadiusR2:0.00}");
            iniCategory.AddProperty(DefinitionPropertySettings.SteelCunetteR3.Key, $"{steelCunetteShape.RadiusR3:0.00}");
        }
    }
}
