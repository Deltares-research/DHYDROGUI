using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.Location;

namespace DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition
{
    public class DefinitionGeneratorCrossSectionDefinitionStandardTemplate : DefinitionGeneratorCrossSectionDefinitionStandard
    {
        private string templateType;
        public DefinitionGeneratorCrossSectionDefinitionStandardTemplate(string templateType) : base(CrossSectionRegion.CrossSectionDefinitionType.Zw_Template)
        {
            this.templateType = templateType;
        }

        protected override void AddCommonProperties(ICrossSectionDefinition crossSectionDefinition)
        {
            base.AddCommonProperties(crossSectionDefinition);
            IniCategory.AddProperty(DefinitionPropertySettings.Template, templateType);
        }

        protected override bool HasCorrectCrossSectionShape(CrossSectionDefinitionStandard standardDefinition)
        {
            return true;
        }

        protected override void AddShapeMeasurementProperties(ICrossSectionStandardShape shape)
        {
            //negeer ik met opzet
        }
    }
}