using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.CrossSectionDefinitionGenerators;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.CrossSectionDefinitionGenerators
{
    [TestFixture]
    public class CrossSectionDefinitionIniCategoryGeneratorSteelSteelCunetteTest : CrossSectionDefinitionIniCategoryGeneratorTestHelper
    {
        private const string CrossSectionName = "myCsDefinition";
        private const double CrossSectionHeight = 2.3;
        private const double AngleA = 28;
        private const double AngleA1 = 0;
        private const double RadiusR = 0.5;
        private const double RadiusR1 = 0.8;
        private const double RadiusR2 = 0.2;
        private const double RadiusR3 = 0;

        [Test]
        public void GiveSteelCunetteCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasTheCorrectName()
        {
            var iniCategory = GenerateSteelCunetteDelftIniCategory();
            Assert.That(iniCategory.Name, Is.EqualTo(DefinitionRegion.Header));
        }

        [Test]
        public void GivenSteelCunetteCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasCorrectShapeTypeProperty()
        {
            var iniCategory = GenerateSteelCunetteDelftIniCategory();

            var shapeTypeValue = iniCategory.GetPropertyValue(DefinitionRegion.DefinitionType.Key);
            Assert.That(shapeTypeValue, Is.EqualTo("SteelCunette"));
        }

        [Test]
        public void GivenSteelCunetteCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasCorrectIdProperty()
        {
            var iniCategory = GenerateSteelCunetteDelftIniCategory();

            var idValue = iniCategory.GetPropertyValue(DefinitionRegion.Id.Key);
            Assert.That(idValue, Is.EqualTo(CrossSectionName));
        }

        [Test]
        public void GivenSteelCunetteCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasCorrectMeasurementProperties()
        {
            var iniCategory = GenerateSteelCunetteDelftIniCategory();

            CheckIfValueWithGivenKeyHasExpectedValue(iniCategory, DefinitionRegion.SteelCunetteHeight.Key, $"{CrossSectionHeight:0.00}");
            CheckIfValueWithGivenKeyHasExpectedValue(iniCategory, DefinitionRegion.SteelCunetteA.Key, $"{AngleA:0.00}");
            CheckIfValueWithGivenKeyHasExpectedValue(iniCategory, DefinitionRegion.SteelCunetteA1.Key, $"{AngleA1:0.00}");
            CheckIfValueWithGivenKeyHasExpectedValue(iniCategory, DefinitionRegion.SteelCunetteR.Key, $"{RadiusR:0.00}");
            CheckIfValueWithGivenKeyHasExpectedValue(iniCategory, DefinitionRegion.SteelCunetteR1.Key, $"{RadiusR1:0.00}");
            CheckIfValueWithGivenKeyHasExpectedValue(iniCategory, DefinitionRegion.SteelCunetteR2.Key, $"{RadiusR2:0.00}");
            CheckIfValueWithGivenKeyHasExpectedValue(iniCategory, DefinitionRegion.SteelCunetteR3.Key, $"{RadiusR3:0.00}");
        }

        [Test]
        public void GivenSteelCunetteCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasCorrectStandardProperties()
        {
            var iniCategory = GenerateSteelCunetteDelftIniCategory();
            CheckIfValueWithGivenKeyHasExpectedValue(iniCategory, DefinitionRegion.Closed.Key, "1");
            CheckIfValueWithGivenKeyHasExpectedValue(iniCategory, DefinitionRegion.GroundlayerUsed.Key, "0");
        }

        [Test]
        public void GivenSteelCunetteCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasCorrectRoughnessNamesProperty()
        {
            var iniCategory = GenerateSteelCunetteDelftIniCategory();

            var closedValue = iniCategory.GetPropertyValue(DefinitionRegion.RoughnessNames.Key);
            Assert.That(closedValue, Is.EqualTo("Main Second"));
        }

        private static DelftIniCategory GenerateSteelCunetteDelftIniCategory()
        {
            var csDefinition = GetSteelCunetteCrossSectionDefinition();
            var generator = new CrossSectionDefinitionIniCategoryGeneratorSteelCunette();
            var iniCategory = generator.GenerateIniCategory(csDefinition);
            return iniCategory;
        }

        private static CrossSectionDefinitionStandard GetSteelCunetteCrossSectionDefinition()
        {
            var crossSectionStandardShapeSteelCunette = new CrossSectionStandardShapeSteelCunette
            {
                Name = CrossSectionName,
                Height = CrossSectionHeight,
                AngleA = AngleA,
                AngleA1 = AngleA1,
                RadiusR = RadiusR,
                RadiusR1 = RadiusR1,
                RadiusR2 = RadiusR2,
                RadiusR3 = RadiusR3
            };
            return GetCrossSectionDefinitionStandardWithSections(crossSectionStandardShapeSteelCunette);
        }
    }
}