using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using Deltares.Infrastructure.API.Validation;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.GridProperties
{
    [TestFixture]
    public class LeveeBreachPropertiesTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowProperties()
        {
            WindowsFormsTestHelper.ShowPropertyGridForObject(new LeveeBreachProperties { Data = new LeveeBreach() { Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(100, 200), }) }});
        }

        [Test]
        public void CheckVisibilityOfDynamicVisibleProperties()
        {
            var leveeBreach = new LeveeBreach(){WaterLevelFlowLocationsActive = true};
            var props = new LeveeBreachProperties { Data = leveeBreach };
            Assert.That(props.IsVisible(nameof(LeveeBreachProperties.WaterLevelUpstreamLocationX)), Is.True);
            Assert.That(props.IsVisible(nameof(LeveeBreachProperties.WaterLevelUpstreamLocationY)), Is.True);
            Assert.That(props.IsVisible(nameof(LeveeBreachProperties.WaterLevelDownstreamLocationX)), Is.True);
            Assert.That(props.IsVisible(nameof(LeveeBreachProperties.WaterLevelDownstreamLocationY)), Is.True);
            leveeBreach.WaterLevelFlowLocationsActive = false;
            Assert.That(props.IsVisible(nameof(LeveeBreachProperties.WaterLevelUpstreamLocationX)), Is.False);
            Assert.That(props.IsVisible(nameof(LeveeBreachProperties.WaterLevelUpstreamLocationY)), Is.False);
            Assert.That(props.IsVisible(nameof(LeveeBreachProperties.WaterLevelDownstreamLocationX)), Is.False);
            Assert.That(props.IsVisible(nameof(LeveeBreachProperties.WaterLevelDownstreamLocationY)), Is.False);

            Assert.That(props.IsVisible(nameof(LeveeBreachProperties.LeveeBreachFormula)), Is.True);
            Assert.That(props.IsVisible(nameof(LeveeBreachProperties.StartTimeBreachGrowth)), Is.True);
            leveeBreach.GetActiveLeveeBreachSettings().BreachGrowthActive = false;
            Assert.That(props.IsVisible(nameof(LeveeBreachProperties.LeveeBreachFormula)), Is.False);
            Assert.That(props.IsVisible(nameof(LeveeBreachProperties.StartTimeBreachGrowth)), Is.False);
            props.BreachGrowthActive = true;
            Assert.That(props.IsVisible(nameof(LeveeBreachProperties.LeveeBreachFormula)), Is.True);
            Assert.That(props.IsVisible(nameof(LeveeBreachProperties.StartTimeBreachGrowth)), Is.True);
        }
        [Test]
        public void CheckSnappingOfBranchLocationProperties()
        {
            var leveeBreach = new LeveeBreach(){ Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(100, 200), }) } ;
            var props = new LeveeBreachProperties { Data = leveeBreach };
            Assert.That(props.UseBreachLocationSnapping, Is.True);
            Assert.That(props.BreachLocationX, Is.EqualTo(50));
            Assert.That(props.BreachLocationY, Is.EqualTo(100));

            props.BreachLocationX = 25;
            Assert.That(props.BreachLocationX, Is.EqualTo(25));
            Assert.That(props.BreachLocationY, Is.EqualTo(50));

            props.BreachLocationY = 100;
            Assert.That(props.BreachLocationX, Is.EqualTo(50));
            Assert.That(props.BreachLocationY, Is.EqualTo(100));

            props.UseBreachLocationSnapping = false;

            props.BreachLocationX = 100;
            Assert.That(props.BreachLocationX, Is.EqualTo(100));
            Assert.That(props.BreachLocationY, Is.EqualTo(100));

            props.BreachLocationY = 75;
            Assert.That(props.BreachLocationX, Is.EqualTo(100));
            Assert.That(props.BreachLocationY, Is.EqualTo(75));

            props.UseBreachLocationSnapping = true;
            Assert.That(props.BreachLocationX, Is.EqualTo(100));
            Assert.That(props.BreachLocationY, Is.EqualTo(200));
        }

        [Test]
        public void SetName_InvalidName_OriginalNameIsPreserved()
        {
            // Arrange
            var validator = Substitute.For<IValidator<string>>();
            validator.Validate("some_invalid_name").Returns(ValidationResult.Fail("message"));

            var data = new LeveeBreach { Name = "some_name" };
            var properties = new LeveeBreachProperties { Data = data };
            properties.NameValidator.AddValidator(validator);

            // Act
            properties.Name = "some_invalid_name";

            // Assert
            Assert.That(properties.Name, Is.EqualTo("some_name"));
        }

        [Test]
        public void SetName_ValidName_NameIsUpdated()
        {
            // Arrange
            var validator = Substitute.For<IValidator<string>>();
            validator.Validate("some_valid_name").Returns(ValidationResult.Success);

            var data = new LeveeBreach { Name = "some_name" };
            var properties = new LeveeBreachProperties { Data = data };
            properties.NameValidator.AddValidator(validator);

            // Act
            properties.Name = "some_valid_name";

            // Assert
            Assert.That(properties.Name, Is.EqualTo("some_valid_name"));
        }
    }
}