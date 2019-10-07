using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition
{
    public class 
        DefinitionGeneratorCrossSectionDefinitionArch : DefinitionGeneratorCrossSectionDefinitionStandard
    {
        public DefinitionGeneratorCrossSectionDefinitionArch()
            : base(CrossSectionRegion.CrossSectionDefinitionType.Arch)
        {
        }

        public override DelftIniCategory CreateDefinitionRegion(ICrossSectionDefinition crossSectionDefinition)
        {
            var standardDefinition = crossSectionDefinition as CrossSectionDefinitionStandard;
            if (standardDefinition == null) return IniCategory;
            if (standardDefinition.ShapeType != CrossSectionStandardShapeType.Arch) return IniCategory;
            AddCommonRegionElements(crossSectionDefinition);
            var shapeArch = standardDefinition.Shape as CrossSectionStandardShapeArch;
            if (shapeArch == null) return IniCategory;
            IniCategory.AddProperty(DefinitionRegion.ArchCrossSectionHeight.Key, shapeArch.Height, DefinitionRegion.ArchCrossSectionHeight.Description, DefinitionRegion.ArchCrossSectionHeight.Format);
            IniCategory.AddProperty(DefinitionRegion.ArchCrossSectionWidth.Key, shapeArch.Width, DefinitionRegion.ArchCrossSectionWidth.Description, DefinitionRegion.ArchCrossSectionWidth.Format);
            IniCategory.AddProperty(DefinitionRegion.ArchHeight.Key, shapeArch.ArcHeight, DefinitionRegion.ArchHeight.Description, DefinitionRegion.ArchHeight.Format);
            GenerateTabulatedProfile(ConverStandardToZw(standardDefinition));
            return IniCategory;
        }
    }
}