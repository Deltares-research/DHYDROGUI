using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.CrossSectionDefinitionGenerators;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.CrossSectionDefinitionGenerators
{
    [TestFixture]
    public class CrossSectionDefinitionIniCategoryGeneratorTrapeziumTest : CrossSectionDefinitionIniCategoryGeneratorTestHelper
    {
        private const string CrossSectionName = "myCsDefinition";
        private const double CrossSectionSlope = 2.3;
        private const double CrossSectionBottomWidthB = 10.0;
        private const double CrossSectionMaximumFlowWidth = 20.2;

        [Test]
        public void GiveTrapeziumCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasTheCorrectName()
        {
            var iniCategory = GenerateTrapeziumDelftIniCategory();
            Assert.That(iniCategory.Name, Is.EqualTo(DefinitionRegion.Header));
        }

        [Test]
        public void GivenTrapeziumCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasCorrectShapeTypeProperty()
        {
            var iniCategory = GenerateTrapeziumDelftIniCategory();

            var shapeTypeValue = iniCategory.GetPropertyValue(DefinitionRegion.DefinitionType.Key);
            Assert.That(shapeTypeValue, Is.EqualTo("Trapezium"));
        }

        [Test]
        public void GivenTrapeziumCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasCorrectIdProperty()
        {
            var iniCategory = GenerateTrapeziumDelftIniCategory();

            var idValue = iniCategory.GetPropertyValue(DefinitionRegion.Id.Key);
            Assert.That(idValue, Is.EqualTo(CrossSectionName));
        }

        [Test]
        public void GivenTrapeziumCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasCorrectMeasurementProperties()
        {
            var iniCategory = GenerateTrapeziumDelftIniCategory();
            CheckIfValueWithGivenKeyHasExpectedValue(iniCategory, DefinitionRegion.Slope.Key, $"{CrossSectionSlope:0.00}");
            CheckIfValueWithGivenKeyHasExpectedValue(iniCategory, DefinitionRegion.BottomWidth.Key, $"{CrossSectionBottomWidthB:0.00}");
            CheckIfValueWithGivenKeyHasExpectedValue(iniCategory, DefinitionRegion.MaximumFlowWidth.Key, $"{CrossSectionMaximumFlowWidth:0.00}");
        }

        [Test]
        public void GivenTrapeziumCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasCorrectStandardProperties()
        {
            var iniCategory = GenerateTrapeziumDelftIniCategory();
            CheckIfValueWithGivenKeyHasExpectedValue(iniCategory, DefinitionRegion.Closed.Key, "1");
            CheckIfValueWithGivenKeyHasExpectedValue(iniCategory, DefinitionRegion.GroundlayerUsed.Key, "0");
        }

        [Test]
        public void GivenTrapeziumCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasCorrectRoughnessNamesProperty()
        {
            var iniCategory = GenerateTrapeziumDelftIniCategory();

            var closedValue = iniCategory.GetPropertyValue(DefinitionRegion.RoughnessNames.Key);
            Assert.That(closedValue, Is.EqualTo("Main Second"));
        }

        private static DelftIniCategory GenerateTrapeziumDelftIniCategory()
        {
            var csDefinition = GetTrapeziumCrossSectionDefinition();
            var generator = new CrossSectionDefinitionIniCategoryGeneratorTrapezium();
            var iniCategory = generator.GenerateIniCategory(csDefinition);
            return iniCategory;
        }

        private static CrossSectionDefinitionStandard GetTrapeziumCrossSectionDefinition()
        {
            var crossSectionStandardShapeTrapezium = new CrossSectionStandardShapeTrapezium
            {
                Name = CrossSectionName,
                Slope = CrossSectionSlope,
                BottomWidthB = CrossSectionBottomWidthB,
                MaximumFlowWidth = CrossSectionMaximumFlowWidth
            };
            return GetCrossSectionDefinitionStandardWithSections(crossSectionStandardShapeTrapezium);
        }
    }
}