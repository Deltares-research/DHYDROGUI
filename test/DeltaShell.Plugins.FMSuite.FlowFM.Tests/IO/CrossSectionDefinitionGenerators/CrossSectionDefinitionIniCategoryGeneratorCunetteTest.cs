using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.CrossSectionDefinitionGenerators;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.CrossSectionDefinitionGenerators
{
    [TestFixture]
    public class CrossSectionDefinitionIniCategoryGeneratorCunetteTest : CrossSectionDefinitionIniCategoryGeneratorTestHelper
    {
        private const string CrossSectionName = "myCsDefinition";
        private const double CrossSectionWidth = 2.3;

        [Test]
        public void GiveCunetteCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasTheCorrectName()
        {
            var iniCategory = GenerateCunetteDelftIniCategory();
            Assert.That(iniCategory.Name, Is.EqualTo(DefinitionPropertySettings.Header));
        }

        [Test]
        public void GivenCunetteCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasCorrectShapeTypeProperty()
        {
            var iniCategory = GenerateCunetteDelftIniCategory();

            var shapeTypeValue = iniCategory.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.That(shapeTypeValue, Is.EqualTo("Cunette"));
        }

        [Test]
        public void GivenCunetteCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasCorrectIdProperty()
        {
            var iniCategory = GenerateCunetteDelftIniCategory();

            var idValue = iniCategory.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.That(idValue, Is.EqualTo(CrossSectionName));
        }

        [Test]
        public void GivenCunetteCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasCorrectMeasurementProperties()
        {
            var iniCategory = GenerateCunetteDelftIniCategory();
            CheckIfValueWithGivenKeyHasExpectedValue(iniCategory, DefinitionPropertySettings.CunetteWidth.Key, $"{CrossSectionWidth:0.00}");
        }

        [Test]
        public void GivenCunetteCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasCorrectStandardProperties()
        {
            var iniCategory = GenerateCunetteDelftIniCategory();
            CheckIfValueWithGivenKeyHasExpectedValue(iniCategory, DefinitionPropertySettings.Closed.Key, "1");
            CheckIfValueWithGivenKeyHasExpectedValue(iniCategory, DefinitionPropertySettings.GroundlayerUsed.Key, "0");
        }

        [Test]
        public void GivenCunetteCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasCorrectRoughnessNamesProperty()
        {
            var iniCategory = GenerateCunetteDelftIniCategory();

            var closedValue = iniCategory.GetPropertyValue(DefinitionPropertySettings.RoughnessNames.Key);
            Assert.That(closedValue, Is.EqualTo("Main Second"));
        }

        private static DelftIniCategory GenerateCunetteDelftIniCategory()
        {
            var csDefinition = GetCunetteCrossSectionDefinition();
            var generator = new CrossSectionDefinitionIniCategoryGeneratorCunette();
            var iniCategory = generator.GenerateIniCategory(csDefinition);
            return iniCategory;
        }

        private static CrossSectionDefinitionStandard GetCunetteCrossSectionDefinition()
        {
            var crossSectionStandardShapeCunette = new CrossSectionStandardShapeCunette
            {
                Name = CrossSectionName,
                Width = CrossSectionWidth
            };
            return GetCrossSectionDefinitionStandardWithSections(crossSectionStandardShapeCunette);
        }
    }
}