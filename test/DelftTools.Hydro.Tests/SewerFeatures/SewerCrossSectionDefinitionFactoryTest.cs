using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.SewerFeatures;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.SewerFeatures
{
    [TestFixture]
    public class SewerCrossSectionDefinitionFactoryTest
    {
        [Test]
        [TestCase(SewerCrossSectionDefinitionFactory.DefaultPipeProfileName, "Default Pipe Profile")]
        [TestCase(SewerCrossSectionDefinitionFactory.DefaultPumpSewerStructureProfileName, "Default Pump sewer structure profile")]
        [TestCase(SewerCrossSectionDefinitionFactory.DefaultWeirSewerStructureProfileName, "Default Weir/Orifice sewer structure profile")]
        public void DefaultProfileName_ReturnCorrectResult(string result, string expResult)
        {
            Assert.That(result, Is.EqualTo(expResult));
        }

        [Test]
        public void CreateDefaultPipeProfile_ReturnsCorrectCrossSectionDefinition()
        {
            // Call
            CrossSectionDefinition result = SewerCrossSectionDefinitionFactory.CreateDefaultPipeProfile();

            // Result
            Assert.That(result.Name, Is.EqualTo("Default Pipe Profile"));
            var standard = result as CrossSectionDefinitionStandard;
            Assert.That(standard, Is.Not.Null, $"The cross section definition is not a {typeof(CrossSectionDefinitionStandard)}");
            var circle = standard.Shape as CrossSectionStandardShapeCircle;
            Assert.That(circle, Is.Not.Null, $"The shape is not a {typeof(CrossSectionStandardShapeCircle)}");
            Assert.That(circle.Diameter, Is.EqualTo(0.4));
        }

        [Test]
        public void CreateDefaultPumpSewerStructureProfile_ReturnsCorrectCrossSectionDefinition()
        {
            // Call
            CrossSectionDefinition result = SewerCrossSectionDefinitionFactory.CreateDefaultPumpSewerStructureProfile();

            // Result
            Assert.That(result.Name, Is.EqualTo("Default Pump sewer structure profile"));
            var standard = result as CrossSectionDefinitionStandard;
            Assert.That(standard, Is.Not.Null, $"The cross section definition is not a {typeof(CrossSectionDefinitionStandard)}");
            var circle = standard.Shape as CrossSectionStandardShapeCircle;
            Assert.That(circle, Is.Not.Null, $"The shape is not a {typeof(CrossSectionStandardShapeCircle)}");
            Assert.That(circle.Diameter, Is.EqualTo(0.1));
        }

        [Test]
        public void CreateDefaultWeirSewerStructureProfile_ReturnsCorrectCrossSectionDefinition()
        {
            // Call
            CrossSectionDefinition result = SewerCrossSectionDefinitionFactory.CreateDefaultWeirSewerStructureProfile();

            // Result
            Assert.That(result.Name, Is.EqualTo("Default Weir/Orifice sewer structure profile"));
            var standard = result as CrossSectionDefinitionStandard;
            Assert.That(standard, Is.Not.Null, $"The cross section definition is not a {typeof(CrossSectionDefinitionStandard)}");
            var rectangle = standard.Shape as CrossSectionStandardShapeRectangle;
            Assert.That(rectangle, Is.Not.Null, $"The shape is not a {typeof(CrossSectionStandardShapeRectangle)}");
            Assert.That(rectangle.Height, Is.EqualTo(10.0));
            Assert.That(rectangle.Width, Is.EqualTo(10.0));
        }
    }
}