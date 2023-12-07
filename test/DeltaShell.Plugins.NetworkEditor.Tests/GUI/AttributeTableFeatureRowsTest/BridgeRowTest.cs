using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Validation.Common;
using DelftTools.Utils.Validation.NameValidation;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.AttributeTableFeatureRowsTest
{
    [TestFixture]
    public class BridgeRowTest
    {
        [Test]
        public void Constructor_WithNullBridge_ThrowsArgumentNullException()
        {
            // Act
            void Call()
            {
                new BridgeRow(null, new NameValidator());
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
                new BridgeRow(new Bridge(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void GetFeature_GetsBridge()
        {
            // Arrange
            var bridge = new Bridge();
            var row = new BridgeRow(bridge, new NameValidator());

            // Act
            IFeature result = row.GetFeature();

            // Assert
            Assert.That(result, Is.SameAs(bridge));
        }

        [Test]
        public void WhenBridgePropertyChanged_RowShouldFirePropertyChangedEvent()
        {
            // Arrange
            var eventRaised = false;
            var bridge = new Bridge();
            var row = new BridgeRow(bridge, new NameValidator());
            row.PropertyChanged += (sender, args) => eventRaised = true;

            // Act
            bridge.Name = "some_name";

            // Assert
            Assert.That(eventRaised);
        }

        [Test]
        public void SetName_SetsBridgeName()
        {
            // Arrange
            var bridge = new Bridge();
            var row = new BridgeRow(bridge, new NameValidator());

            // Act
            row.Name = "some_name";

            // Assert
            Assert.That(bridge.Name, Is.EqualTo("some_name"));
        }

        [Test]
        public void GetName_GetsBridgeName()
        {
            // Arrange
            var bridge = new Bridge { Name = "some_name" };
            var row = new BridgeRow(bridge, new NameValidator());

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

            var bridge = new Bridge { Name = "some_name" };
            var row = new BridgeRow(bridge, nameValidator);

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

            var bridgePillar = new Bridge { Name = "some_name" };
            var row = new BridgeRow(bridgePillar, nameValidator);

            // Act
            row.Name = "some_valid_name";

            // Assert
            Assert.That(row.Name, Is.EqualTo("some_valid_name"));
        }

        [Test]
        public void SetLongName_SetsBridgeLongName()
        {
            // Arrange
            var bridge = new Bridge();
            var row = new BridgeRow(bridge, new NameValidator());

            // Act
            row.LongName = "some_long_name";

            // Assert
            Assert.That(bridge.LongName, Is.EqualTo("some_long_name"));
        }

        [Test]
        public void GetLongName_GetsBridgeLongName()
        {
            // Arrange
            var bridge = new Bridge { LongName = "some_long_name" };
            var row = new BridgeRow(bridge, new NameValidator());

            // Act
            string result = row.LongName;

            // Assert
            Assert.That(result, Is.EqualTo("some_long_name"));
        }

        [Test]
        public void GetBranch_GetsBridgeBranchName()
        {
            // Arrange
            var branch = Substitute.For<IBranch>();
            branch.Name = "some_branch_name";
            var bridge = new Bridge { Branch = branch };
            var row = new BridgeRow(bridge, new NameValidator());

            // Act
            string result = row.Branch;

            // Assert
            Assert.That(result, Is.EqualTo("some_branch_name"));
        }

        [Test]
        public void SetBridgeType_SetsBridgeBridgeType([Values] BridgeType bridgeType)
        {
            // Arrange
            var bridge = new Bridge();
            var row = new BridgeRow(bridge, new NameValidator());

            // Act
            row.BridgeType = bridgeType;

            // Assert
            Assert.That(bridge.BridgeType, Is.EqualTo(bridgeType));
        }

        [Test]
        public void GetBridgeType_GetsBridgeBridgeType([Values] BridgeType bridgeType)
        {
            // Arrange
            var bridge = new Bridge { BridgeType = bridgeType };
            var row = new BridgeRow(bridge, new NameValidator());

            // Act
            BridgeType result = row.BridgeType;

            // Assert
            Assert.That(result, Is.EqualTo(bridgeType));
        }

        [Test]
        public void SetBridgeLength_SetsBridgeBridgeLength()
        {
            // Arrange
            var bridge = new Bridge();
            var row = new BridgeRow(bridge, new NameValidator());

            // Act
            row.BridgeLength = 1.23;

            // Assert
            Assert.That(bridge.BridgeLength, Is.EqualTo(1.23));
        }

        [Test]
        public void GetBridgeLength_GetsBridgeBridgeLength()
        {
            // Arrange
            var bridge = new Bridge { BridgeLength = 1.23 };
            var row = new BridgeRow(bridge, new NameValidator());

            // Act
            double result = row.BridgeLength;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }

        [Test]
        public void SetFlowDirection_SetsBridgeFlowDirection([Values] FlowDirection flowDirection)
        {
            // Arrange
            var bridge = new Bridge();
            var row = new BridgeRow(bridge, new NameValidator());

            // Act
            row.FlowDirection = flowDirection;

            // Assert
            Assert.That(bridge.FlowDirection, Is.EqualTo(flowDirection));
        }

        [Test]
        public void GetFlowDirection_GetsBridgeFlowDirection([Values] FlowDirection flowDirection)
        {
            // Arrange
            var bridge = new Bridge { FlowDirection = flowDirection };
            var row = new BridgeRow(bridge, new NameValidator());

            // Act
            FlowDirection result = row.FlowDirection;

            // Assert
            Assert.That(result, Is.EqualTo(flowDirection));
        }

        [Test]
        public void SetInletLossCoefficient_SetsBridgeInletLossCoefficient()
        {
            // Arrange
            var bridge = new Bridge();
            var row = new BridgeRow(bridge, new NameValidator());

            // Act
            row.InletLossCoefficient = 1.23;

            // Assert
            Assert.That(bridge.InletLossCoefficient, Is.EqualTo(1.23));
        }

        [Test]
        public void GetInletLossCoefficient_GetsBridgeInletLossCoefficient()
        {
            // Arrange
            var bridge = new Bridge { InletLossCoefficient = 1.23 };
            var row = new BridgeRow(bridge, new NameValidator());

            // Act
            double result = row.InletLossCoefficient;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }

        [Test]
        public void SetOutletLossCoefficient_SetsBridgeOutletLossCoefficient()
        {
            // Arrange
            var bridge = new Bridge();
            var row = new BridgeRow(bridge, new NameValidator());

            // Act
            row.OutletLossCoefficient = 1.23;

            // Assert
            Assert.That(bridge.OutletLossCoefficient, Is.EqualTo(1.23));
        }

        [Test]
        public void GetOutletLossCoefficient_GetsBridgeOutletLossCoefficient()
        {
            // Arrange
            var bridge = new Bridge { OutletLossCoefficient = 1.23 };
            var row = new BridgeRow(bridge, new NameValidator());

            // Act
            double result = row.OutletLossCoefficient;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }

        [Test]
        public void SetFrictionType_SetsBridgeFrictionType([Values] BridgeFrictionType frictionType)
        {
            // Arrange
            var bridge = new Bridge();
            var row = new BridgeRow(bridge, new NameValidator());

            // Act
            row.FrictionType = frictionType;

            // Assert
            Assert.That(bridge.FrictionType, Is.EqualTo(frictionType));
        }

        [Test]
        public void GetFrictionType_GetsBridgeFrictionType([Values] BridgeFrictionType frictionType)
        {
            // Arrange
            var bridge = new Bridge { FrictionType = frictionType };
            var row = new BridgeRow(bridge, new NameValidator());

            // Act
            BridgeFrictionType result = row.FrictionType;

            // Assert
            Assert.That(result, Is.EqualTo(frictionType));
        }

        [Test]
        public void SetFriction_SetsBridgeFriction()
        {
            // Arrange
            var bridge = new Bridge();
            var row = new BridgeRow(bridge, new NameValidator());

            // Act
            row.Friction = 1.23;

            // Assert
            Assert.That(bridge.Friction, Is.EqualTo(1.23));
        }

        [Test]
        public void GetFriction_GetsBridgeFriction()
        {
            // Arrange
            var bridge = new Bridge { Friction = 1.23 };
            var row = new BridgeRow(bridge, new NameValidator());

            // Act
            double result = row.Friction;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }

        [Test]
        public void SetShift_SetsBridgeShift()
        {
            // Arrange
            var bridge = new Bridge();
            var row = new BridgeRow(bridge, new NameValidator());

            // Act
            row.Shift = 1.23;

            // Assert
            Assert.That(bridge.Shift, Is.EqualTo(1.23));
        }

        [Test]
        public void GetShift_GetsBridgeShift()
        {
            // Arrange
            var bridge = new Bridge { Shift = 1.23 };
            var row = new BridgeRow(bridge, new NameValidator());

            // Act
            double result = row.Shift;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }

        [Test]
        public void SetWidth_SetsBridgeWidth()
        {
            // Arrange
            var bridge = new Bridge();
            var row = new BridgeRow(bridge, new NameValidator());

            // Act
            row.Width = 1.23;

            // Assert
            Assert.That(bridge.Width, Is.EqualTo(1.23));
        }

        [Test]
        public void GetWidth_GetsBridgeWidth()
        {
            // Arrange
            var bridge = new Bridge { Width = 1.23 };
            var row = new BridgeRow(bridge, new NameValidator());

            // Act
            double result = row.Width;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }

        [Test]
        public void SetHeight_SetsBridgeHeight()
        {
            // Arrange
            var bridge = new Bridge();
            var row = new BridgeRow(bridge, new NameValidator());

            // Act
            row.Height = 1.23;

            // Assert
            Assert.That(bridge.Height, Is.EqualTo(1.23));
        }

        [Test]
        public void GetHeight_GetsBridgeHeight()
        {
            // Arrange
            var bridge = new Bridge { Height = 1.23 };
            var row = new BridgeRow(bridge, new NameValidator());

            // Act
            double result = row.Height;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }
    }
}