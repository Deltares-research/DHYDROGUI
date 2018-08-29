using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.CrossSectionDefinitionGenerators;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.CrossSectionDefinitionGenerators
{
    [TestFixture]
    public class CrossSectionDefinitionIniCategoryGeneratorRectangleTest : CrossSectionDefinitionIniCategoryGeneratorTestHelper
    {
        private const string CrossSectionName = "myCsDefinition";
        private const double CrossSectionWidth = 2.3;
        private const double CrossSectionHeight = 4.4;

        [Test]
        public void GiveRectangleCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasTheCorrectName()
        {
            var iniCategory = GenerateRectangleDelftIniCategory();
            Assert.That(iniCategory.Name, Is.EqualTo(DefinitionRegion.Header));
        }

        [Test]
        public void GivenRectangleCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasCorrectShapeTypeProperty()
        {
            var iniCategory = GenerateRectangleDelftIniCategory();

            var shapeTypeValue = iniCategory.GetPropertyValue(DefinitionRegion.DefinitionType.Key);
            Assert.That(shapeTypeValue, Is.EqualTo("Rectangle"));
        }

        [Test]
        public void GivenRectangleCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasCorrectIdProperty()
        {
            var iniCategory = GenerateRectangleDelftIniCategory();

            var idValue = iniCategory.GetPropertyValue(DefinitionRegion.Id.Key);
            Assert.That(idValue, Is.EqualTo(CrossSectionName));
        }

        [Test]
        public void GivenRectangleCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasCorrectMeasurementProperties()
        {
            var expectedWidthValue = $"{CrossSectionWidth:0.00}";
            var expectedHeightValue = $"{CrossSectionHeight:0.00}";
            var iniCategory = GenerateRectangleDelftIniCategory();

            var widthValue = iniCategory.GetPropertyValue(DefinitionRegion.RectangleWidth.Key);
            var heightValue = iniCategory.GetPropertyValue(DefinitionRegion.RectangleHeight.Key);
            Assert.That(widthValue, Is.EqualTo(expectedWidthValue));
            Assert.That(heightValue, Is.EqualTo(expectedHeightValue));
        }

        [Test]
        public void GivenRectangleCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasCorrectStandardProperties()
        {
            var iniCategory = GenerateRectangleDelftIniCategory();

            var closedValue = iniCategory.GetPropertyValue(DefinitionRegion.Closed.Key);
            var groundLayerUsedValue = iniCategory.GetPropertyValue(DefinitionRegion.GroundlayerUsed.Key);
            Assert.That(closedValue, Is.EqualTo("1"));
            Assert.That(groundLayerUsedValue, Is.EqualTo("0"));
        }

        [Test]
        public void GivenRectangleCrossSectionDefinition_WhenGeneratingIniCategory_ThenIniCategoryHasCorrectRoughnessNamesProperty()
        {
            var iniCategory = GenerateRectangleDelftIniCategory();

            var closedValue = iniCategory.GetPropertyValue(DefinitionRegion.RoughnessNames.Key);
            Assert.That(closedValue, Is.EqualTo("Main Second"));
        }

        private static DelftIniCategory GenerateRectangleDelftIniCategory()
        {
            var csDefinition = GetRectangleCrossSectionDefinition();
            var generator = new CrossSectionDefinitionIniCategoryGeneratorRectangle();
            var iniCategory = generator.GenerateIniCategory(csDefinition);
            return iniCategory;
        }

        private static CrossSectionDefinitionStandard GetRectangleCrossSectionDefinition()
        {
            var crossSectionStandardShapeRectangle = new CrossSectionStandardShapeRectangle
            {
                Name = CrossSectionName,
                Width = CrossSectionWidth,
                Height = CrossSectionHeight
            };
            return GetCrossSectionDefinitionStandardWithSections(crossSectionStandardShapeRectangle);
        }
    }
}