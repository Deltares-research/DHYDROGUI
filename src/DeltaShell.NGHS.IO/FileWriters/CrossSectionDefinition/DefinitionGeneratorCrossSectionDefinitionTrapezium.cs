using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition
{
    public class
        DefinitionGeneratorCrossSectionDefinitionTrapezium : DefinitionGeneratorCrossSectionDefinitionStandard
    {
        public DefinitionGeneratorCrossSectionDefinitionTrapezium()
            : base(CrossSectionRegion.CrossSectionDefinitionType.Trapezium)
        {
        }

        public override DelftIniCategory CreateDefinitionRegion(ICrossSectionDefinition crossSectionDefinition)
        {
            var standardDefinition = crossSectionDefinition as CrossSectionDefinitionStandard;
            if (standardDefinition == null) return IniCategory;
            if (standardDefinition.ShapeType != CrossSectionStandardShapeType.Trapezium) return IniCategory;
            AddCommonRegionElements(crossSectionDefinition);
            var shapeTrapezium = standardDefinition.Shape as CrossSectionStandardShapeTrapezium;
            if (shapeTrapezium == null) return IniCategory;
            IniCategory.AddProperty(DefinitionRegion.Slope.Key, shapeTrapezium.Slope, DefinitionRegion.Slope.Description, DefinitionRegion.Slope.Format);
            IniCategory.AddProperty(DefinitionRegion.MaximumFlowWidth.Key, shapeTrapezium.MaximumFlowWidth, DefinitionRegion.MaximumFlowWidth.Description, DefinitionRegion.MaximumFlowWidth.Format);
            IniCategory.AddProperty(DefinitionRegion.BottomWidth.Key, shapeTrapezium.BottomWidthB, DefinitionRegion.BottomWidth.Description, DefinitionRegion.BottomWidth.Format);
            return IniCategory;
        }
    }
}