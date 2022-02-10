using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.SewerFeatures;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.SewerFeatures
{
    [TestFixture]
    public class SewerCrossSectionDefinitionInstanceCreatorTest
    {
        [Test]
        public void CreateDefaultSewerProfile_ReturnsCorrectCrossSectionDefinition()
        {
            // Setup
            var creator = new SewerCrossSectionDefinitionInstanceCreator();

            // Call
            CrossSectionDefinition result = creator.CreateDefaultSewerProfile();

            // Result
            Assert.That(result.Name, Is.EqualTo("Default Sewer Profile"));
            var standard = result as CrossSectionDefinitionStandard;
            Assert.That(standard, Is.Not.Null, $"The cross section definition is not a {typeof(CrossSectionDefinitionStandard)}");
            var circle = standard.Shape as CrossSectionStandardShapeCircle;
            Assert.That(circle, Is.Not.Null, $"The shape is not a {typeof(CrossSectionStandardShapeCircle)}");
            Assert.That(circle.Diameter, Is.EqualTo(0.4));
        }

        [Test]
        public void CreateDefaultPressurizedPipeSewerConnectionProfile_ReturnsCorrectCrossSectionDefinition()
        {
            // Setup
            var creator = new SewerCrossSectionDefinitionInstanceCreator();

            // Call
            CrossSectionDefinition result = creator.CreateDefaultPressurizedPipeSewerConnectionProfile();

            // Result
            Assert.That(result.Name, Is.EqualTo("Default Pressurized Pipe Sewer Connection Profile"));
            var standard = result as CrossSectionDefinitionStandard;
            Assert.That(standard, Is.Not.Null, $"The cross section definition is not a {typeof(CrossSectionDefinitionStandard)}");
            var circle = standard.Shape as CrossSectionStandardShapeCircle;
            Assert.That(circle, Is.Not.Null, $"The shape is not a {typeof(CrossSectionStandardShapeCircle)}");
            Assert.That(circle.Diameter, Is.EqualTo(0.1));
        }

        [Test]
        public void CreateDefaultWeirSewerConnectionProfile_ReturnsCorrectCrossSectionDefinition()
        {
            // Setup
            var creator = new SewerCrossSectionDefinitionInstanceCreator();

            // Call
            CrossSectionDefinition result = creator.CreateDefaultWeirSewerConnectionProfile();

            // Result
            Assert.That(result.Name, Is.EqualTo("Default Weir/Orifice Sewer Connection Profile"));
            var standard = result as CrossSectionDefinitionStandard;
            Assert.That(standard, Is.Not.Null, $"The cross section definition is not a {typeof(CrossSectionDefinitionStandard)}");
            var rectangle = standard.Shape as CrossSectionStandardShapeRectangle;
            Assert.That(rectangle, Is.Not.Null, $"The shape is not a {typeof(CrossSectionStandardShapeRectangle)}");
            Assert.That(rectangle.Height, Is.EqualTo(10.0));
            Assert.That(rectangle.Width, Is.EqualTo(10.0));
        }
    }
}