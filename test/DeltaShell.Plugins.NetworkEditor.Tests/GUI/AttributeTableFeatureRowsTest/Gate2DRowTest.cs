using DelftTools.Hydro.Structures;
using DelftTools.Utils.Validation.Common;
using DelftTools.Utils.Validation.NameValidation;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using GeoAPI.Extensions.Feature;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.AttributeTableFeatureRowsTest
{
    [TestFixture]
    public class Gate2DRowTest
    {
        [Test]
        public void Constructor_WithNullGate2D_ThrowsArgumentNullException()
        {
            // Act
            void Call()
            {
                new Gate2DRow(null, new NameValidator());
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_WithNullNameValidator_ThrowsArgumentNullException()
        {
            // Act
            void Call()
            {
                new Gate2DRow(new Gate2D(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void GetFeature_GetsGate2D()
        {
            // Arrange
            var gate2D = new Gate2D();
            var row = new Gate2DRow(gate2D, new NameValidator());

            // Act
            IFeature result = row.GetFeature();

            // Assert
            Assert.That(result, Is.SameAs(gate2D));
        }

        [Test]
        public void WhenGate2DPropertyChanged_RowShouldFirePropertyChangedEvent()
        {
            // Arrange
            var eventRaised = false;
            var gate2D = new Gate2D();
            var row = new Gate2DRow(gate2D, new NameValidator());
            row.PropertyChanged += (sender, args) => eventRaised = true;

            // Act
            gate2D.Name = "some_name";

            // Assert
            Assert.That(eventRaised);
        }

        [Test]
        public void SetName_SetsGate2DName()
        {
            // Arrange
            var gate2D = new Gate2D();
            var row = new Gate2DRow(gate2D, new NameValidator());

            // Act
            row.Name = "some_name";

            // Assert
            Assert.That(gate2D.Name, Is.EqualTo("some_name"));
        }

        [Test]
        public void GetName_GetsGate2DName()
        {
            // Arrange
            var gate2D = new Gate2D { Name = "some_name" };
            var row = new Gate2DRow(gate2D, new NameValidator());

            // Act
            string result = row.Name;

            // Assert
            Assert.AreEqual(result, "some_name");
        }

        [Test]
        public void SetName_InvalidName_OriginalNameIsPreserved()
        {
            // Arrange
            var validator = Substitute.For<IValidator<string>>();
            validator.Validate("some_invalid_name").Returns(ValidationResult.Fail("message"));
            var nameValidator = new NameValidator();
            nameValidator.AddValidator(validator);

            var gate2D = new Gate2D { Name = "some_name" };
            var row = new Gate2DRow(gate2D, nameValidator);

            // Act
            row.Name = "some_invalid_name";

            // Assert
            Assert.That(row.Name, Is.EqualTo("some_name"));
        }

        [Test]
        public void SetName_ValidName_NameIsUpdated()
        {
            // Arrange
            var validator = Substitute.For<IValidator<string>>();
            validator.Validate("some_valid_name").Returns(ValidationResult.Success);
            var nameValidator = new NameValidator();
            nameValidator.AddValidator(validator);

            var gate2D = new Gate2D { Name = "some_name" };
            var row = new Gate2DRow(gate2D, nameValidator);

            // Act
            row.Name = "some_valid_name";

            // Assert
            Assert.That(row.Name, Is.EqualTo("some_valid_name"));
        }

        [Test]
        public void SetLongName_SetsGate2DLongName()
        {
            // Arrange
            var gate2D = new Gate2D();
            var row = new Gate2DRow(gate2D, new NameValidator());

            // Act
            row.LongName = "some_long_name";

            // Assert
            Assert.That(gate2D.LongName, Is.EqualTo("some_long_name"));
        }

        [Test]
        public void GetLongName_GetsGate2DLongName()
        {
            // Arrange
            var gate2D = new Gate2D { LongName = "some_long_name" };
            var row = new Gate2DRow(gate2D, new NameValidator());

            // Act
            string result = row.LongName;

            // Assert
            Assert.AreEqual(result, "some_long_name");
        }

        [Test]
        public void GetBranch_ReturnsEmptyString()
        {
            // Arrange
            var gate2D = new Gate2D();
            var row = new Gate2DRow(gate2D, new NameValidator());

            // Act
            string result = row.Branch;

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void SetGroupName_SetsGate2DGroupName()
        {
            // Arrange
            var gate2D = new Gate2D();
            var row = new Gate2DRow(gate2D, new NameValidator());

            // Act
            row.GroupName = "some_group_name";

            // Assert
            Assert.That(gate2D.GroupName, Is.EqualTo("some_group_name"));
        }

        [Test]
        public void GetGroupName_GetsGate2DGroupName()
        {
            // Arrange
            var gate2D = new Gate2D { GroupName = "some_group_name" };
            var row = new Gate2DRow(gate2D, new NameValidator());

            // Act
            string result = row.GroupName;

            // Assert
            Assert.AreEqual(result, "some_group_name");
        }

        [Test]
        public void SetSillLevel_SetsGate2DSillLevel()
        {
            // Arrange
            var gate2D = new Gate2D();
            var row = new Gate2DRow(gate2D, new NameValidator());

            // Act
            row.SillLevel = 1.23;

            // Assert
            Assert.That(gate2D.SillLevel, Is.EqualTo(1.23));
        }

        [Test]
        public void GetSillLevel_GetsGate2DSillLevel()
        {
            // Arrange
            var gate2D = new Gate2D { SillLevel = 1.23 };
            var row = new Gate2DRow(gate2D, new NameValidator());

            // Act
            double result = row.SillLevel;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }

        [Test]
        public void SetLowerEdgeLevel_SetsGate2DLowerEdgeLevel()
        {
            // Arrange
            var gate2D = new Gate2D();
            var row = new Gate2DRow(gate2D, new NameValidator());

            // Act
            row.LowerEdgeLevel = 1.23;

            // Assert
            Assert.That(gate2D.LowerEdgeLevel, Is.EqualTo(1.23));
        }

        [Test]
        public void GetLowerEdgeLevel_GetsGate2DLowerEdgeLevel()
        {
            // Arrange
            var gate2D = new Gate2D { LowerEdgeLevel = 1.23 };
            var row = new Gate2DRow(gate2D, new NameValidator());

            // Act
            double result = row.LowerEdgeLevel;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }

        [Test]
        public void SetOpeningWidth_SetsGate2DOpeningWidth()
        {
            // Arrange
            var gate2D = new Gate2D();
            var row = new Gate2DRow(gate2D, new NameValidator());

            // Act
            row.OpeningWidth = 1.23;

            // Assert
            Assert.That(gate2D.OpeningWidth, Is.EqualTo(1.23));
        }

        [Test]
        public void GetOpeningWidth_GetsGate2DOpeningWidth()
        {
            // Arrange
            var gate2D = new Gate2D { OpeningWidth = 1.23 };
            var row = new Gate2DRow(gate2D, new NameValidator());

            // Act
            double result = row.OpeningWidth;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }

        [Test]
        public void SetDoorHeight_SetsGate2DDoorHeight()
        {
            // Arrange
            var gate2D = new Gate2D();
            var row = new Gate2DRow(gate2D, new NameValidator());

            // Act
            row.DoorHeight = 1.23;

            // Assert
            Assert.That(gate2D.DoorHeight, Is.EqualTo(1.23));
        }

        [Test]
        public void GetDoorHeight_GetsGate2DDoorHeight()
        {
            // Arrange
            var gate2D = new Gate2D { DoorHeight = 1.23 };
            var row = new Gate2DRow(gate2D, new NameValidator());

            // Act
            double result = row.DoorHeight;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }

        [Test]
        public void SetHorizontalOpeningDirection_SetsGate2DHorizontalOpeningDirection([Values] GateOpeningDirection gateOpeningDirection)
        {
            // Arrange
            var gate2D = new Gate2D();
            var row = new Gate2DRow(gate2D, new NameValidator());

            // Act
            row.HorizontalOpeningDirection = gateOpeningDirection;

            // Assert
            Assert.That(gate2D.HorizontalOpeningDirection, Is.EqualTo(gateOpeningDirection));
        }

        [Test]
        public void GetHorizontalOpeningDirection_GetsGate2DHorizontalOpeningDirection([Values] GateOpeningDirection gateOpeningDirection)
        {
            // Arrange
            var gate2D = new Gate2D { HorizontalOpeningDirection = gateOpeningDirection };
            var row = new Gate2DRow(gate2D, new NameValidator());

            // Act
            GateOpeningDirection result = row.HorizontalOpeningDirection;

            // Assert
            Assert.That(result, Is.EqualTo(gateOpeningDirection));
        }

        [Test]
        public void SetSillWidth_SetsGate2DSillWidth()
        {
            // Arrange
            var gate2D = new Gate2D();
            var row = new Gate2DRow(gate2D, new NameValidator());

            // Act
            row.SillWidth = 1.23;

            // Assert
            Assert.That(gate2D.SillWidth, Is.EqualTo(1.23));
        }

        [Test]
        public void GetSillWidth_GetsGate2DSillWidth()
        {
            // Arrange
            var gate2D = new Gate2D { SillWidth = 1.23 };
            var row = new Gate2DRow(gate2D, new NameValidator());

            // Act
            double result = row.SillWidth;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }
    }
}