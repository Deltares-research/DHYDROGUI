using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition
{
    public class 
        DefinitionGeneratorCrossSectionDefinitionCircle : DefinitionGeneratorCrossSectionDefinitionStandard
    {
        public DefinitionGeneratorCrossSectionDefinitionCircle()
            : base(CrossSectionRegion.CrossSectionDefinitionType.Circle)
        {
        }

        public override DelftIniCategory CreateDefinitionRegion(ICrossSectionDefinition crossSectionDefinition)
        {
            var standardDefinition = crossSectionDefinition as CrossSectionDefinitionStandard;
            if (standardDefinition == null) return IniCategory;
            if (standardDefinition.ShapeType != CrossSectionStandardShapeType.Circle) return IniCategory;
            AddCommonRegionElements(crossSectionDefinition);
            var shapeCircle = standardDefinition.Shape as CrossSectionStandardShapeCircle;
            if (shapeCircle == null) return IniCategory;
            IniCategory.AddProperty(DefinitionRegion.Diameter.Key, shapeCircle.Diameter, DefinitionRegion.Diameter.Description, DefinitionRegion.Diameter.Format);
            GenerateTabulatedProfile(ConverStandardToZw(standardDefinition));
            return IniCategory;
        }
    }
}