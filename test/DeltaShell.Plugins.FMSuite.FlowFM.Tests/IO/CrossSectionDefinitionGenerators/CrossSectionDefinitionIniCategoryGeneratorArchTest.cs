using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.CrossSectionDefinitionGenerators;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.CrossSectionDefinitionGenerators
{
    [TestFixture]
    public class CrossSectionDefinitionIniCategoryGeneratorArchTest : CrossSectionDefinitionIniCategoryGeneratorTestHelper
    {
        private const string CrossSectionName = "myCsDefinition";
        private const double CrossSectionWidth = 2.3;
        private const double CrossSectionHeight = 1.1;
        private const double CrossSectionArcHeight = 0.8;

        [Test]
        public void GiveArchCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasTheCorrectName()
        {
            var iniCategory = GenerateArchDelftIniCategory();
            Assert.That(iniCategory.Name, Is.EqualTo(DefinitionRegion.Header));
        }

        [Test]
        public void GivenArchCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasCorrectShapeTypeProperty()
        {
            var iniCategory = GenerateArchDelftIniCategory();

            var shapeTypeValue = iniCategory.GetPropertyValue(DefinitionRegion.DefinitionType.Key);
            Assert.That(shapeTypeValue, Is.EqualTo("Arch"));
        }

        [Test]
        public void GivenArchCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasCorrectIdProperty()
        {
            var iniCategory = GenerateArchDelftIniCategory();

            var idValue = iniCategory.GetPropertyValue(DefinitionRegion.Id.Key);
            Assert.That(idValue, Is.EqualTo(CrossSectionName));
        }

        [Test]
        public void GivenArchCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasCorrectMeasurementProperties()
        {
            var iniCategory = GenerateArchDelftIniCategory();

            CheckIfValueWithGivenKeyHasExpectedValue(iniCategory, DefinitionRegion.ArchCrossSectionWidth.Key, $"{CrossSectionWidth:0.00}");
            CheckIfValueWithGivenKeyHasExpectedValue(iniCategory, DefinitionRegion.ArchCrossSectionHeight.Key, $"{CrossSectionHeight:0.00}");
            CheckIfValueWithGivenKeyHasExpectedValue(iniCategory, DefinitionRegion.ArchHeight.Key, $"{CrossSectionArcHeight:0.00}");
        }

        [Test]
        public void GivenArchCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasCorrectStandardProperties()
        {
            var iniCategory = GenerateArchDelftIniCategory();
            CheckIfValueWithGivenKeyHasExpectedValue(iniCategory, DefinitionRegion.Closed.Key, "1");
            CheckIfValueWithGivenKeyHasExpectedValue(iniCategory, DefinitionRegion.GroundlayerUsed.Key, "0");
        }

        [Test]
        public void GivenArchCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasCorrectRoughnessNamesProperty()
        {
            var iniCategory = GenerateArchDelftIniCategory();

            var closedValue = iniCategory.GetPropertyValue(DefinitionRegion.RoughnessNames.Key);
            Assert.That(closedValue, Is.EqualTo("Main Second"));
        }

        private static DelftIniCategory GenerateArchDelftIniCategory()
        {
            var csDefinition = GetArchCrossSectionDefinition();
            var generator = new CrossSectionDefinitionIniCategoryGeneratorArch();
            var iniCategory = generator.GenerateIniCategory(csDefinition);
            return iniCategory;
        }

        private static CrossSectionDefinitionStandard GetArchCrossSectionDefinition()
        {
            var crossSectionStandardShapeArch = new CrossSectionStandardShapeArch
            {
                Name = CrossSectionName,
                Width = CrossSectionWidth,
                Height = CrossSectionHeight,
                ArcHeight = CrossSectionArcHeight
            };
            return GetCrossSectionDefinitionStandardWithSections(crossSectionStandardShapeArch);
        }
    }
}