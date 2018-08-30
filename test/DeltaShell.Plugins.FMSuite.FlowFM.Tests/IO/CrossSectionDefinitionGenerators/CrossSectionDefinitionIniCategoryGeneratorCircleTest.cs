using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.CrossSectionDefinitionGenerators;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.CrossSectionDefinitionGenerators
{
    [TestFixture]
    public class CrossSectionDefinitionIniCategoryGeneratorCircleTest : CrossSectionDefinitionIniCategoryGeneratorTestHelper
    {
        private const string CrossSectionName = "myCsDefinition";
        private const double CrossSectionDiameter = 33.34;
        
        [Test]
        public void GivenCircleCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasTheCorrectName()
        {
            var iniCategory = GenerateCircleDelftIniCategory();
            Assert.That(iniCategory.Name, Is.EqualTo(DefinitionPropertySettings.Header));
        }

        [Test]
        public void GivenCircleCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasCorrectShapeTypeProperty()
        {
            var iniCategory = GenerateCircleDelftIniCategory();

            var shapeTypeValue = iniCategory.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.That(shapeTypeValue, Is.EqualTo("Circle"));
        }

        [Test]
        public void GivenCircleCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasCorrectIdProperty()
        {
            var iniCategory = GenerateCircleDelftIniCategory();

            var idValue = iniCategory.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.That(idValue, Is.EqualTo(CrossSectionName));
        }

        [Test]
        public void GivenCircleCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasCorrectDiameterProperty()
        {
            var iniCategory = GenerateCircleDelftIniCategory();
            CheckIfValueWithGivenKeyHasExpectedValue(iniCategory, DefinitionPropertySettings.Diameter.Key, $"{CrossSectionDiameter:0.00}");
        }

        [Test]
        public void GivenCircleCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasCorrectStandardProperties()
        {
            var iniCategory = GenerateCircleDelftIniCategory();
            CheckIfValueWithGivenKeyHasExpectedValue(iniCategory, DefinitionPropertySettings.Closed.Key, "1");
            CheckIfValueWithGivenKeyHasExpectedValue(iniCategory, DefinitionPropertySettings.GroundlayerUsed.Key, "0");
        }

        [Test]
        public void GivenCircleCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasCorrectRoughnessNamesProperty()
        {
            var iniCategory = GenerateCircleDelftIniCategory();

            var closedValue = iniCategory.GetPropertyValue(DefinitionPropertySettings.RoughnessNames.Key);
            Assert.That(closedValue, Is.EqualTo("Main Second"));
        }

        private static DelftIniCategory GenerateCircleDelftIniCategory()
        {
            var csDefinition = GetCircleCrossSectionDefinition();
            var generator = new CrossSectionDefinitionIniCategoryGeneratorCircle();
            var iniCategory = generator.GenerateIniCategory(csDefinition);
            return iniCategory;
        }

        private static CrossSectionDefinitionStandard GetCircleCrossSectionDefinition()
        {
            var crossSectionStandardShapeCircle = new CrossSectionStandardShapeCircle
            {
                Name = CrossSectionName,
                Diameter = CrossSectionDiameter
            };
            return GetCrossSectionDefinitionStandardWithSections(crossSectionStandardShapeCircle);
        }
    }
}