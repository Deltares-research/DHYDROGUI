using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition
{
    internal class DefinitionGeneratorCrossSectionDefinitionSteelCunette : DefinitionGeneratorCrossSectionDefinitionStandard
    {
        public DefinitionGeneratorCrossSectionDefinitionSteelCunette()
            : base(CrossSectionRegion.CrossSectionDefinitionType.SteelCunette)
        {
        }

        public override DelftIniCategory CreateDefinitionRegion(ICrossSectionDefinition crossSectionDefinition)
        {
            var standardDefinition = crossSectionDefinition as CrossSectionDefinitionStandard;
            if (standardDefinition == null) return IniCategory;
            if (standardDefinition.ShapeType != CrossSectionStandardShapeType.SteelCunette) return IniCategory;
            AddCommonRegionElements(crossSectionDefinition);
            var shapeSteelCunette = standardDefinition.Shape as CrossSectionStandardShapeSteelCunette;
            if (shapeSteelCunette == null) return IniCategory;
            IniCategory.AddProperty(DefinitionRegion.SteelCunetteHeight.Key, shapeSteelCunette.Height, DefinitionRegion.SteelCunetteHeight.Description, DefinitionRegion.SteelCunetteHeight.Format);
            IniCategory.AddProperty(DefinitionRegion.SteelCunetteR.Key, shapeSteelCunette.RadiusR, DefinitionRegion.SteelCunetteR.Description, DefinitionRegion.SteelCunetteR.Format);
            IniCategory.AddProperty(DefinitionRegion.SteelCunetteR1.Key, shapeSteelCunette.RadiusR1, DefinitionRegion.SteelCunetteR1.Description, DefinitionRegion.SteelCunetteR1.Format);
            IniCategory.AddProperty(DefinitionRegion.SteelCunetteR2.Key, shapeSteelCunette.RadiusR2, DefinitionRegion.SteelCunetteR2.Description, DefinitionRegion.SteelCunetteR2.Format);
            IniCategory.AddProperty(DefinitionRegion.SteelCunetteR3.Key, shapeSteelCunette.RadiusR3, DefinitionRegion.SteelCunetteR3.Description, DefinitionRegion.SteelCunetteR3.Format);
            IniCategory.AddProperty(DefinitionRegion.SteelCunetteA.Key, shapeSteelCunette.AngleA, DefinitionRegion.SteelCunetteA.Description, DefinitionRegion.SteelCunetteA.Format);
            IniCategory.AddProperty(DefinitionRegion.SteelCunetteA1.Key, shapeSteelCunette.AngleA1, DefinitionRegion.SteelCunetteA1.Description, DefinitionRegion.SteelCunetteA1.Format);
            GenerateTabulatedProfile(ConverStandardToZw(standardDefinition)); 
            return IniCategory;
        }
    }
}