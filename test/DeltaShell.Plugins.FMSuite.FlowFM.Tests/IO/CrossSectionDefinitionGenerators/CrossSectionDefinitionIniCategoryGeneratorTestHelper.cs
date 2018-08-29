using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.Helpers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.CrossSectionDefinitionGenerators
{
    public class CrossSectionDefinitionIniCategoryGeneratorTestHelper
    {
        protected static void CheckIfValueWithGivenKeyHasExpectedValue(DelftIniCategory iniCategory, string key, string expectedValue)
        {
            var atualValue = iniCategory.GetPropertyValue(key);
            Assert.That(atualValue, Is.EqualTo(expectedValue));
        }

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
