using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.CrossSectionDefinitionGenerators;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.CrossSectionDefinitionGenerators
{
    [TestFixture]
    public class CrossSectionDefinitionIniCategoryGeneratorRoundTest
    {
        private const string CrossSectionName = "myCsDefinition";
        private const double CrossSectionDiameter = 33.34;
        
        [Test]
        public void GivenRoundCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasTheCorrectName()
        {
            var iniCategory = GenerateCircleDelftIniCategory();
            Assert.That(iniCategory.Name, Is.EqualTo(DefinitionRegion.Header));
        }

        [Test]
        public void GivenRoundCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasCorrectShapeTypeProperty()
        {
            var iniCategory = GenerateCircleDelftIniCategory();

            var shapeTypeValue = iniCategory.GetPropertyValue(DefinitionRegion.DefinitionType.Key);
            Assert.That(shapeTypeValue, Is.EqualTo("Circle"));
        }

        [Test]
        public void GivenRoundCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasCorrectIdProperty()
        {
            var iniCategory = GenerateCircleDelftIniCategory();

            var idValue = iniCategory.GetPropertyValue(DefinitionRegion.Id.Key);
            Assert.That(idValue, Is.EqualTo(CrossSectionName));
        }

        [Test]
        public void GivenRoundCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasCorrectDiameterProperty()
        {
            var iniCategory = GenerateCircleDelftIniCategory();

            var expectedDiameterValue = $"{CrossSectionDiameter:0.00}";
            var diameterValue = iniCategory.GetPropertyValue(DefinitionRegion.Diameter.Key);
            Assert.That(diameterValue, Is.EqualTo(expectedDiameterValue));
        }

        [Test]
        public void GivenRoundCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasCorrectStandardProperties()
        {
            var iniCategory = GenerateCircleDelftIniCategory();

            var closedValue = iniCategory.GetPropertyValue(DefinitionRegion.Closed.Key);
            var groundLayerUsedValue = iniCategory.GetPropertyValue(DefinitionRegion.GroundlayerUsed.Key);
            Assert.That(closedValue, Is.EqualTo("1"));
            Assert.That(groundLayerUsedValue, Is.EqualTo("0"));
        }

        [Test]
        public void GivenRoundCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasCorrectRoughnessNamesProperty()
        {
            var iniCategory = GenerateCircleDelftIniCategory();

            var closedValue = iniCategory.GetPropertyValue(DefinitionRegion.RoughnessNames.Key);
            Assert.That(closedValue, Is.EqualTo("Main Second"));
        }

        private static DelftIniCategory GenerateCircleDelftIniCategory()
        {
            var csDefinition = GetCircleCrossSectionDefinition();
            var iniCategory = CrossSectionDefinitionIniCategoryGeneratorCircle.GenerateIniCategory(csDefinition);
            return iniCategory;
        }

        private static CrossSectionDefinitionStandard GetCircleCrossSectionDefinition()
        {
            var crossSectionStandardShapeCircle = new CrossSectionStandardShapeCircle
            {
                Name = CrossSectionName,
                Diameter = CrossSectionDiameter
            };
            return new CrossSectionDefinitionStandard(crossSectionStandardShapeCircle)
            {
                Sections = {
                    new CrossSectionSection
                    {
                        SectionType = new CrossSectionSectionType { Name = "Main" }
                    },
                    new CrossSectionSection
                    {
                        SectionType = new CrossSectionSectionType { Name = "Second" }
                    }
                }
            };
        }
    }
}