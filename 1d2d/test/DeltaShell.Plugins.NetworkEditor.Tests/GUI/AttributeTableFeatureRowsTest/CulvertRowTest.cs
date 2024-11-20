using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Validation.NameValidation;
using Deltares.Infrastructure.API.Validation;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.AttributeTableFeatureRowsTest
{
    [TestFixture]
    public class CulvertRowTest
    {
        [Test]
        public void Constructor_WithNullCulvert_ThrowsArgumentNullException()
        {
            // Act
            void Call()
            {
                new CulvertRow(null, new NameValidator());
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
                new CulvertRow(Substitute.For<ICulvert, INotifyPropertyChanged>(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void GetFeature_GetsCulvert()
        {
            // Arrange
            var culvert = new Culvert();
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            IFeature result = row.GetFeature();

            // Assert
            Assert.That(result, Is.SameAs(culvert));
        }

        [Test]
        public void WhenCulvertPropertyChanged_RowShouldFirePropertyChangedEvent()
        {
            // Arrange
            var eventRaised = false;
            var culvert = new Culvert();
            var row = new CulvertRow(culvert, new NameValidator());
            row.PropertyChanged += (sender, args) => eventRaised = true;

            // Act
            culvert.Name = "some_name";

            // Assert
            Assert.That(eventRaised);
        }

        [Test]
        public void SetName_SetsCulvertName()
        {
            // Arrange
            var culvert = new Culvert();
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            row.Name = "some_name";

            // Assert
            Assert.That(culvert.Name, Is.EqualTo("some_name"));
        }

        [Test]
        public void GetName_GetsCulvertName()
        {
            // Arrange
            var culvert = new Culvert { Name = "some_name" };
            var row = new CulvertRow(culvert, new NameValidator());

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

            var culvert = new Culvert { Name = "some_name" };
            var row = new CulvertRow(culvert, nameValidator);

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

            var culvert = new Culvert { Name = "some_name" };
            var row = new CulvertRow(culvert, nameValidator);

            // Act
            row.Name = "some_valid_name";

            // Assert
            Assert.That(row.Name, Is.EqualTo("some_valid_name"));
        }

        [Test]
        public void SetLongName_SetsCulvertLongName()
        {
            // Arrange
            var culvert = new Culvert();
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            row.LongName = "some_long_name";

            // Assert
            Assert.That(culvert.LongName, Is.EqualTo("some_long_name"));
        }

        [Test]
        public void GetLongName_GetsCulvertLongName()
        {
            // Arrange
            var culvert = new Culvert { LongName = "some_long_name" };
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            string result = row.LongName;

            // Assert
            Assert.AreEqual(result, "some_long_name");
        }

        [Test]
        public void GetBranch_GetsCulvertBranch()
        {
            // Arrange
            var branch = Substitute.For<IBranch>();
            branch.Name = "some_branch_name";
            var culvert = new Culvert { Branch = branch };
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            string result = row.Branch;

            // Assert
            Assert.That(result, Is.EqualTo("some_branch_name"));
        }

        [Test]
        public void SetLength_SetsCulvertLength()
        {
            // Arrange
            var culvert = new Culvert();
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            row.Length = 1.23;

            // Assert
            Assert.That(culvert.Length, Is.EqualTo(1.23));
        }

        [Test]
        public void GetLength_GetsCulvertLength()
        {
            // Arrange
            var culvert = new Culvert { Length = 1.23 };
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            double result = row.Length;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }

        [Test]
        public void SetRoughnessType_SetsCulvertRoughnessType([Values] CulvertFrictionType culvertFrictionType)
        {
            // Arrange
            var culvert = new Culvert();
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            row.RoughnessType = culvertFrictionType;

            // Assert
            Assert.That(culvert.FrictionType, Is.EqualTo(culvertFrictionType));
        }

        [Test]
        public void GetRoughnessType_GetsCulvertRoughnessType([Values] CulvertFrictionType culvertFrictionType)
        {
            // Arrange
            var culvert = new Culvert { FrictionType = culvertFrictionType };
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            CulvertFrictionType result = row.RoughnessType;

            // Assert
            Assert.That(result, Is.EqualTo(culvertFrictionType));
        }

        [Test]
        public void SetRoughness_SetsCulvertRoughness()
        {
            // Arrange
            var culvert = new Culvert();
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            row.Roughness = 0.05;

            // Assert
            Assert.That(culvert.Friction, Is.EqualTo(0.05));
        }

        [Test]
        public void GetRoughness_GetsCulvertRoughness()
        {
            // Arrange
            var culvert = new Culvert { Friction = 0.1 };
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            double result = row.Roughness;

            // Assert
            Assert.That(result, Is.EqualTo(0.1));
        }

        [Test]
        public void SetInletLevel_SetsCulvertInletLevel()
        {
            // Arrange
            var culvert = new Culvert();
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            row.InletLevel = 5.0;

            // Assert
            Assert.That(culvert.InletLevel, Is.EqualTo(5.0));
        }

        [Test]
        public void GetInletLevel_GetsCulvertInletLevel()
        {
            // Arrange
            var culvert = new Culvert { InletLevel = 2.0 };
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            double result = row.InletLevel;

            // Assert
            Assert.That(result, Is.EqualTo(2.0));
        }

        [Test]
        public void SetOutletLevel_SetsCulvertOutletLevel()
        {
            // Arrange
            var culvert = new Culvert();
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            row.OutletLevel = 3.0;

            // Assert
            Assert.That(culvert.OutletLevel, Is.EqualTo(3.0));
        }

        [Test]
        public void GetOutletLevel_GetsCulvertOutletLevel()
        {
            // Arrange
            var culvert = new Culvert { OutletLevel = 4.0 };
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            double result = row.OutletLevel;

            // Assert
            Assert.That(result, Is.EqualTo(4.0));
        }

        [Test]
        public void SetInletLossCoefficient_SetsCulvertInletLossCoefficient()
        {
            // Arrange
            var culvert = new Culvert();
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            row.InletLossCoefficient = 0.2;

            // Assert
            Assert.That(culvert.InletLossCoefficient, Is.EqualTo(0.2));
        }

        [Test]
        public void GetInletLossCoefficient_GetsCulvertInletLossCoefficient()
        {
            // Arrange
            var culvert = new Culvert { InletLossCoefficient = 0.3 };
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            double result = row.InletLossCoefficient;

            // Assert
            Assert.That(result, Is.EqualTo(0.3));
        }

        [Test]
        public void SetFlowDirection_SetsCulvertFlowDirection([Values] FlowDirection flowDirection)
        {
            // Arrange
            var culvert = new Culvert();
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            row.FlowDirection = flowDirection;

            // Assert
            Assert.That(culvert.FlowDirection, Is.EqualTo(flowDirection));
        }

        [Test]
        public void GetFlowDirection_GetsCulvertFlowDirection([Values] FlowDirection flowDirection)
        {
            // Arrange
            var culvert = new Culvert { FlowDirection = flowDirection };
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            FlowDirection result = row.FlowDirection;

            // Assert
            Assert.That(result, Is.EqualTo(flowDirection));
        }

        [Test]
        public void SetOutletLossCoefficient_SetsCulvertOutletLossCoefficient()
        {
            // Arrange
            var culvert = new Culvert();
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            row.OutletLossCoefficient = 0.25;

            // Assert
            Assert.That(culvert.OutletLossCoefficient, Is.EqualTo(0.25));
        }

        [Test]
        public void GetOutletLossCoefficient_GetsCulvertOutletLossCoefficient()
        {
            // Arrange
            var culvert = new Culvert { OutletLossCoefficient = 0.35 };
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            double result = row.OutletLossCoefficient;

            // Assert
            Assert.That(result, Is.EqualTo(0.35));
        }

        [Test]
        public void SetBendLossCoefficient_SetsCulvertBendLossCoefficient()
        {
            // Arrange
            var culvert = new Culvert();
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            row.BendLossCoefficient = 0.15;

            // Assert
            Assert.That(culvert.BendLossCoefficient, Is.EqualTo(0.15));
        }

        [Test]
        public void GetBendLossCoefficient_GetsCulvertBendLossCoefficient()
        {
            // Arrange
            var culvert = new Culvert { BendLossCoefficient = 0.25 };
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            double result = row.BendLossCoefficient;

            // Assert
            Assert.That(result, Is.EqualTo(0.25));
        }

        [Test]
        public void SetGated_SetsCulvertIsGated()
        {
            // Arrange
            var culvert = new Culvert();
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            row.Gated = true;

            // Assert
            Assert.That(culvert.IsGated, Is.True);
        }

        [Test]
        public void GetGated_GetsCulvertIsGated()
        {
            // Arrange
            var culvert = new Culvert { IsGated = false };
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            bool result = row.Gated;

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void GetGateLowerEdge_GetsCulvertGateLowerEdge()
        {
            // Arrange
            ICulvert culvert = Substitute.For<ICulvert, INotifyPropertyChanged>();
            culvert.GateLowerEdgeLevel.Returns(3.0);
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            double result = row.GateLowerEdge;

            // Assert
            Assert.That(result, Is.EqualTo(3.0));
        }

        [Test]
        public void SetGateOpening_SetsCulvertGateOpening()
        {
            // Arrange
            var culvert = new Culvert();
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            row.GateOpening = 0.75;

            // Assert
            Assert.That(culvert.GateInitialOpening, Is.EqualTo(0.75));
        }

        [Test]
        public void GetGateOpening_GetsCulvertGateOpening()
        {
            // Arrange
            var culvert = new Culvert { GateInitialOpening = 0.6 };
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            double result = row.GateOpening;

            // Assert
            Assert.That(result, Is.EqualTo(0.6));
        }

        [Test]
        public void SetSubType_SetsCulvertSubType([Values] CulvertType culvertType)
        {
            // Arrange
            var culvert = new Culvert();
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            row.SubType = culvertType;

            // Assert
            Assert.That(culvert.CulvertType, Is.EqualTo(culvertType));
        }

        [Test]
        public void GetSubType_GetsCulvertSubType([Values] CulvertType culvertType)
        {
            // Arrange
            var culvert = new Culvert { CulvertType = culvertType };
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            CulvertType result = row.SubType;

            // Assert
            Assert.That(result, Is.EqualTo(culvertType));
        }

        [Test]
        public void SetShape_SetsCulvertShape()
        {
            // Arrange
            var culvert = new Culvert();
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            row.Shape = CulvertGeometryType.Round;

            // Assert
            Assert.That(culvert.GeometryType, Is.EqualTo(CulvertGeometryType.Round));
        }

        [Test]
        public void GetShape_GetsCulvertShape([Values] CulvertGeometryType culvertGeometryType)
        {
            // Arrange
            var culvert = new Culvert { GeometryType = culvertGeometryType };
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            CulvertGeometryType result = row.Shape;

            // Assert
            Assert.That(result, Is.EqualTo(culvertGeometryType));
        }

        [Test]
        public void SetWidth_SetsCulvertWidth()
        {
            // Arrange
            var culvert = new Culvert();
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            row.Width = 1.5;

            // Assert
            Assert.That(culvert.Width, Is.EqualTo(1.5));
        }

        [Test]
        public void GetWidth_GetsCulvertWidth()
        {
            // Arrange
            var culvert = new Culvert { Width = 2.0 };
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            double result = row.Width;

            // Assert
            Assert.That(result, Is.EqualTo(2.0));
        }

        [Test]
        public void SetHeight_SetsCulvertHeight()
        {
            // Arrange
            var culvert = new Culvert();
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            row.Height = 2.5;

            // Assert
            Assert.That(culvert.Height, Is.EqualTo(2.5));
        }

        [Test]
        public void GetHeight_GetsCulvertHeight()
        {
            // Arrange
            var culvert = new Culvert { Height = 3.0 };
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            double result = row.Height;

            // Assert
            Assert.That(result, Is.EqualTo(3.0));
        }

        [Test]
        public void SetArcHeight_SetsCulvertArcHeight()
        {
            // Arrange
            var culvert = new Culvert();
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            row.ArcHeight = 1.2;

            // Assert
            Assert.That(culvert.ArcHeight, Is.EqualTo(1.2));
        }

        [Test]
        public void GetArcHeight_GetsCulvertArcHeight()
        {
            // Arrange
            var culvert = new Culvert { ArcHeight = 1.5 };
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            double result = row.ArcHeight;

            // Assert
            Assert.That(result, Is.EqualTo(1.5));
        }

        [Test]
        public void SetDiameter_SetsCulvertDiameter()
        {
            // Arrange
            var culvert = new Culvert();
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            row.Diameter = 1.8;

            // Assert
            Assert.That(culvert.Diameter, Is.EqualTo(1.8));
        }

        [Test]
        public void GetDiameter_GetsCulvertDiameter()
        {
            // Arrange
            var culvert = new Culvert { Diameter = 2.2 };
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            double result = row.Diameter;

            // Assert
            Assert.That(result, Is.EqualTo(2.2));
        }

        [Test]
        public void SetRadius_SetsCulvertRadius()
        {
            // Arrange
            var culvert = new Culvert();
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            row.Radius = 1.0;

            // Assert
            Assert.That(culvert.Radius, Is.EqualTo(1.0));
        }

        [Test]
        public void GetRadius_GetsCulvertRadius()
        {
            // Arrange
            var culvert = new Culvert { Radius = 1.5 };
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            double result = row.Radius;

            // Assert
            Assert.That(result, Is.EqualTo(1.5));
        }

        [Test]
        public void SetRadius1_SetsCulvertRadius1()
        {
            // Arrange
            var culvert = new Culvert();
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            row.Radius1 = 2.0;

            // Assert
            Assert.That(culvert.Radius1, Is.EqualTo(2.0));
        }

        [Test]
        public void GetRadius1_GetsCulvertRadius1()
        {
            // Arrange
            var culvert = new Culvert { Radius1 = 2.5 };
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            double result = row.Radius1;

            // Assert
            Assert.That(result, Is.EqualTo(2.5));
        }

        [Test]
        public void SetRadius2_SetsCulvertRadius2()
        {
            // Arrange
            var culvert = new Culvert();
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            row.Radius2 = 3.0;

            // Assert
            Assert.That(culvert.Radius2, Is.EqualTo(3.0));
        }

        [Test]
        public void GetRadius2_GetsCulvertRadius2()
        {
            // Arrange
            var culvert = new Culvert { Radius2 = 3.5 };
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            double result = row.Radius2;

            // Assert
            Assert.That(result, Is.EqualTo(3.5));
        }

        [Test]
        public void SetRadius3_SetsCulvertRadius3()
        {
            // Arrange
            var culvert = new Culvert();
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            row.Radius3 = 4.0;

            // Assert
            Assert.That(culvert.Radius3, Is.EqualTo(4.0));
        }

        [Test]
        public void GetRadius3_GetsCulvertRadius3()
        {
            // Arrange
            var culvert = new Culvert { Radius3 = 4.5 };
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            double result = row.Radius3;

            // Assert
            Assert.That(result, Is.EqualTo(4.5));
        }

        [Test]
        public void SetAngle_SetsCulvertAngle()
        {
            // Arrange
            var culvert = new Culvert();
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            row.Angle = 45.0;

            // Assert
            Assert.That(culvert.Angle, Is.EqualTo(45.0));
        }

        [Test]
        public void GetAngle_GetsCulvertAngle()
        {
            // Arrange
            var culvert = new Culvert { Angle = 60.0 };
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            double result = row.Angle;

            // Assert
            Assert.That(result, Is.EqualTo(60.0));
        }

        [Test]
        public void SetAngle1_SetsCulvertAngle1()
        {
            // Arrange
            var culvert = new Culvert();
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            row.Angle1 = 30.0;

            // Assert
            Assert.That(culvert.Angle1, Is.EqualTo(30.0));
        }

        [Test]
        public void GetAngle1_GetsCulvertAngle1()
        {
            // Arrange
            var culvert = new Culvert { Angle1 = 35.0 };
            var row = new CulvertRow(culvert, new NameValidator());

            // Act
            double result = row.Angle1;

            // Assert
            Assert.That(result, Is.EqualTo(35.0));
        }
    }
}