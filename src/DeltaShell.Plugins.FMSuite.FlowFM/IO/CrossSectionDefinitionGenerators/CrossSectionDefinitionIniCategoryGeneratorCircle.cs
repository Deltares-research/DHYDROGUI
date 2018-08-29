using System;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.CrossSectionDefinitionGenerators
{
    public class CrossSectionDefinitionIniCategoryGeneratorCircle : ICrossSectionDefinitionIniCategoryGenerator
    {
        public static DelftIniCategory GenerateIniCategory(CrossSectionDefinitionStandard crossSectionDefinition)
        {
            var crossSectionStandardShape = crossSectionDefinition.Shape as CrossSectionStandardShapeCircle;
            if(crossSectionStandardShape == null) throw new Exception();

            var iniCategory = new DelftIniCategory(DefinitionRegion.Header);
            iniCategory.AddProperty(DefinitionRegion.Id.Key, crossSectionStandardShape.Name);
            iniCategory.AddProperty(DefinitionRegion.DefinitionType.Key, crossSectionDefinition.ShapeType.ToString());
            iniCategory.AddProperty(DefinitionRegion.Diameter.Key, $"{crossSectionStandardShape.Diameter:0.00}");
            iniCategory.AddProperty(DefinitionRegion.Closed.Key, "1");
            iniCategory.AddProperty(DefinitionRegion.GroundlayerUsed.Key, "0");
            iniCategory.AddProperty(DefinitionRegion.RoughnessNames.Key, crossSectionDefinition.Sections.Select(s => s.SectionType.Name));
            return iniCategory;
        }
    }
}
