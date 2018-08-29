using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.CrossSectionDefinitionGenerators
{
    public abstract class ACrossSectionDefinitionIniCategoryGenerator
    {
        protected DelftIniCategory iniCategory;

        public virtual DelftIniCategory GenerateIniCategory(CrossSectionDefinitionStandard crossSectionDefinition)
        {
            InitializeIniCategory();
            AddStandardProperties(crossSectionDefinition);
            AddMeasurementsProperties(crossSectionDefinition.Shape);
            AddStandardProperties();
            AddRoughnessNamesProperty(crossSectionDefinition);

            return iniCategory;
        }

        private void InitializeIniCategory()
        {
            iniCategory = new DelftIniCategory(DefinitionRegion.Header);
        }

        private void AddStandardProperties(CrossSectionDefinitionStandard crossSectionDefinition)
        {
            iniCategory.AddProperty(DefinitionRegion.Id.Key, crossSectionDefinition.Shape.Name);
            iniCategory.AddProperty(DefinitionRegion.DefinitionType.Key, crossSectionDefinition.ShapeType.ToString());
        }

        protected abstract void AddMeasurementsProperties(ICrossSectionStandardShape crossSectionShape);

        private void AddStandardProperties()
        {
            iniCategory.AddProperty(DefinitionRegion.Closed.Key, "1");
            iniCategory.AddProperty(DefinitionRegion.GroundlayerUsed.Key, "0");
        }

        private void AddRoughnessNamesProperty(CrossSectionDefinitionStandard crossSectionDefinition)
        {
            iniCategory.AddProperty(DefinitionRegion.RoughnessNames.Key, crossSectionDefinition.Sections.Select(s => s.SectionType.Name));
        }
    }
}