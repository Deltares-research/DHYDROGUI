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
            iniCategory = new DelftIniCategory(DefinitionPropertySettings.Header);
        }

        private void AddStandardProperties(CrossSectionDefinitionStandard crossSectionDefinition)
        {
            iniCategory.AddProperty(DefinitionPropertySettings.Id.Key, crossSectionDefinition.Shape.Name);
            iniCategory.AddProperty(DefinitionPropertySettings.DefinitionType.Key, crossSectionDefinition.ShapeType.ToString());
        }

        protected abstract void AddMeasurementsProperties(ICrossSectionStandardShape crossSectionShape);

        private void AddStandardProperties()
        {
            iniCategory.AddProperty(DefinitionPropertySettings.Closed.Key, "1");
            iniCategory.AddProperty(DefinitionPropertySettings.GroundlayerUsed.Key, "0");
        }

        private void AddRoughnessNamesProperty(CrossSectionDefinitionStandard crossSectionDefinition)
        {
            iniCategory.AddProperty(DefinitionPropertySettings.RoughnessNames.Key, crossSectionDefinition.Sections.Select(s => s.SectionType.Name));
        }
    }
}