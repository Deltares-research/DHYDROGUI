using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.CrossSectionDefinitionGenerators;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.CrossSectionDefinitionGenerators
{
    [TestFixture]
    public class CrossSectionDefinitionIniCategoryGeneratorEllipticalTest : CrossSectionDefinitionIniCategoryGeneratorTestHelper
    {
        private const string CrossSectionName = "myCsDefinition";
        private const double CrossSectionWidth = 2.3;
        private const double CrossSectionHeight = 5.5;

        [Test]
        public void GiveEllipticalCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasTheCorrectName()
        {
            var iniCategory = GenerateEllipticalDelftIniCategory();
            Assert.That(iniCategory.Name, Is.EqualTo(DefinitionPropertySettings.Header));
        }

        [Test]
        public void GivenEllipticalCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasCorrectShapeTypeProperty()
        {
            var iniCategory = GenerateEllipticalDelftIniCategory();

            var shapeTypeValue = iniCategory.GetPropertyValue(DefinitionPropertySettings.DefinitionType.Key);
            Assert.That(shapeTypeValue, Is.EqualTo("Elliptical"));
        }

        [Test]
        public void GivenEllipticalCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasCorrectIdProperty()
        {
            var iniCategory = GenerateEllipticalDelftIniCategory();

            var idValue = iniCategory.GetPropertyValue(DefinitionPropertySettings.Id.Key);
            Assert.That(idValue, Is.EqualTo(CrossSectionName));
        }

        [Test]
        public void GivenEllipticalCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasCorrectMeasurementProperties()
        {
            var iniCategory = GenerateEllipticalDelftIniCategory();
            CheckIfValueWithGivenKeyHasExpectedValue(iniCategory, DefinitionPropertySettings.EllipseWidth.Key, $"{CrossSectionWidth:0.00}");
            CheckIfValueWithGivenKeyHasExpectedValue(iniCategory, DefinitionPropertySettings.EllipseHeight.Key, $"{CrossSectionHeight:0.00}");
        }

        [Test]
        public void GivenEllipticalCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasCorrectStandardProperties()
        {
            var iniCategory = GenerateEllipticalDelftIniCategory();
            CheckIfValueWithGivenKeyHasExpectedValue(iniCategory, DefinitionPropertySettings.Closed.Key, "1");
            CheckIfValueWithGivenKeyHasExpectedValue(iniCategory, DefinitionPropertySettings.GroundlayerUsed.Key, "0");
        }

        [Test]
        public void GivenEllipticalCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasCorrectRoughnessNamesProperty()
        {
            var iniCategory = GenerateEllipticalDelftIniCategory();

            var closedValue = iniCategory.GetPropertyValue(DefinitionPropertySettings.RoughnessNames.Key);
            Assert.That(closedValue, Is.EqualTo("Main Second"));
        }

        private static DelftIniCategory GenerateEllipticalDelftIniCategory()
        {
            var csDefinition = GetEllipticalCrossSectionDefinition();
            var generator = new CrossSectionDefinitionIniCategoryGeneratorElliptical();
            var iniCategory = generator.GenerateIniCategory(csDefinition);
            return iniCategory;
        }

        private static CrossSectionDefinitionStandard GetEllipticalCrossSectionDefinition()
        {
            var crossSectionStandardShapeElliptical = new CrossSectionStandardShapeElliptical
            {
                Name = CrossSectionName,
                Width = CrossSectionWidth,
                Height = CrossSectionHeight
            };
            return GetCrossSectionDefinitionStandardWithSections(crossSectionStandardShapeElliptical);
        }
    }
}