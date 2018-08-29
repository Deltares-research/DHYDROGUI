using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.CrossSectionDefinitionGenerators
{
    public class CrossSectionDefinitionIniCategoryGeneratorTestHelper
    {
        protected static CrossSectionDefinitionStandard GetCrossSectionDefinitionStandardWithSections(CrossSectionStandardShapeBase crossSectionStandardShapeCircle)
        {
            return new CrossSectionDefinitionStandard(crossSectionStandardShapeCircle)
            {
                Sections =
                {
                    new CrossSectionSection
                    {
                        SectionType = new CrossSectionSectionType {Name = "Main"}
                    },
                    new CrossSectionSection
                    {
                        SectionType = new CrossSectionSectionType {Name = "Second"}
                    }
                }
            };
        }
    }
}
