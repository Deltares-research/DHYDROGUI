using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Validation.Common;
using DelftTools.Utils.Validation.NameValidation;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using GeoAPI.Extensions.Feature;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.AttributeTableFeatureRowsTest
{
    [TestFixture]
    public class Weir2DRowTest
    {
        [Test]
        public void Constructor_WithNullWeir2D_ThrowsArgumentNullException()
        {
            // Act
            void Call()
            {
                new Weir2DRow(null, new NameValidator());
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
                new Weir2DRow(new Weir2D(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void GetFeature_GetsWeir2D()
        {
            // Arrange
            var weir2D = new Weir2D();
            var row = new Weir2DRow(weir2D, new NameValidator());

            // Act
            IFeature result = row.GetFeature();

            // Assert
            Assert.That(result, Is.SameAs(weir2D));
        }

        [Test]
        public void WhenWeir2DPropertyChanged_RowShouldFirePropertyChangedEvent()
        {
            // Arrange
            var eventRaised = false;
            var weir2D = new Weir2D();
            var row = new Weir2DRow(weir2D, new NameValidator());
            row.PropertyChanged += (sender, args) => eventRaised = true;

            // Act
            weir2D.Name = "some_name";

            // Assert
            Assert.That(eventRaised);
        }

        [Test]
        public void SetName_SetsWeir2DName()
        {
            // Arrange
            var weir2D = new Weir2D();
            var row = new Weir2DRow(weir2D, new NameValidator());

            // Act
            row.Name = "some_name";

            // Assert
            Assert.That(weir2D.Name, Is.EqualTo("some_name"));
        }

        [Test]
        public void GetName_GetsWeir2DName()
        {
            // Arrange
            var weir2D = new Weir2D { Name = "some_name" };
            var row = new Weir2DRow(weir2D, new NameValidator());

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

            var weir2D = new Weir2D { Name = "some_name" };
            var row = new Weir2DRow(weir2D, nameValidator);

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

            var weir2D = new Weir2D { Name = "some_name" };
            var row = new Weir2DRow(weir2D, nameValidator);

            // Act
            row.Name = "some_valid_name";

            // Assert
            Assert.That(row.Name, Is.EqualTo("some_valid_name"));
        }

        [Test]
        public void SetLongName_SetsWeir2DLongName()
        {
            // Arrange
            var weir2D = new Weir2D();
            var row = new Weir2DRow(weir2D, new NameValidator());

            // Act
            row.LongName = "some_name";

            // Assert
            Assert.That(weir2D.LongName, Is.EqualTo("some_name"));
        }

        [Test]
        public void GetLongName_GetsWeir2DLongName()
        {
            // Arrange
            var weir2D = new Weir2D { LongName = "some_name" };
            var row = new Weir2DRow(weir2D, new NameValidator());

            // Act
            string result = row.LongName;

            // Assert
            Assert.AreEqual(result, "some_name");
        }

        [Test]
        public void GetBranch_GetsWeir2DBranchNameEmptyString()
        {
            // Arrange
            var weir2D = new Weir2D();
            var row = new Weir2DRow(weir2D, new NameValidator());

            // Act
            string result = row.Branch;

            // Assert
            Assert.AreEqual(result, string.Empty);
        }

        [Test]
        public void GetWeirFormulaName_GetsWeir2DFormulaName()
        {
            // Arrange
            var weir2D = new Weir2D();
            var weirFormula = new SimpleWeirFormula();
            var row = new Weir2DRow(weir2D, new NameValidator());

            // Act
            string result = row.WeirFormulaName;

            // Assert
            Assert.AreEqual(result, weirFormula.Name);
        }

        [Test]
        public void SetCrestWidth_SetsWeir2DCrestWidth()
        {
            // Arrange
            var weir2D = new Weir2D();
            var row = new Weir2DRow(weir2D, new NameValidator());

            // Act
            row.CrestWidth = 10.0;

            // Assert
            Assert.That(weir2D.CrestWidth, Is.EqualTo(10.0));
        }

        [Test]
        public void GetCrestWidth_GetsWeir2DCrestWidth()
        {
            // Arrange
            var weir2D = new Weir2D { CrestWidth = 10.0 };
            var row = new Weir2DRow(weir2D, new NameValidator());

            // Act
            double result = row.CrestWidth;

            // Assert
            Assert.AreEqual(result, 10.0);
        }

        [Test]
        public void SetCrestLevel_SetsWeir2DCrestLevel()
        {
            // Arrange
            var weir2D = new Weir2D();
            var row = new Weir2DRow(weir2D, new NameValidator());

            // Act
            row.CrestLevel = 10.0;

            // Assert
            Assert.That(weir2D.CrestLevel, Is.EqualTo(10.0));
        }

        [Test]
        public void GetCrestLevel_GetsWeir2DCrestLevel()
        {
            // Arrange
            var weir2D = new Weir2D { CrestLevel = 10.0 };
            var row = new Weir2DRow(weir2D, new NameValidator());

            // Act
            double result = row.CrestLevel;

            // Assert
            Assert.AreEqual(result, 10.0);
        }

        [Test]
        public void SetFlowDirection_SetsWeir2DFlowDirection([Values] FlowDirection flowDirection)
        {
            // Arrange
            var weir2D = new Weir2D();
            var row = new Weir2DRow(weir2D, new NameValidator());

            // Act
            row.FlowDirection = flowDirection;

            // Assert
            Assert.That(weir2D.FlowDirection, Is.EqualTo(flowDirection));
        }

        [Test]
        public void GetFlowDirection_GetsWeir2DFlowDirection([Values] FlowDirection flowDirection)
        {
            // Arrange
            var weir2D = new Weir2D { FlowDirection = flowDirection };
            var row = new Weir2DRow(weir2D, new NameValidator());

            // Act
            FlowDirection result = row.FlowDirection;

            // Assert
            Assert.AreEqual(result, flowDirection);
        }

        [Test]
        public void SetGroupName_SetsWeir2DGroupName()
        {
            // Arrange
            var weir2D = new Weir2D();
            var row = new Weir2DRow(weir2D, new NameValidator());

            // Act
            row.GroupName = "some_group_name";

            // Assert
            Assert.That(weir2D.GroupName, Is.EqualTo("some_group_name"));
        }

        [Test]
        public void GetGroupName_GetsWeir2DGroupName()
        {
            // Arrange
            var weir2D = new Weir2D { GroupName = "some_group_name" };
            var row = new Weir2DRow(weir2D, new NameValidator());

            // Act
            string result = row.GroupName;

            // Assert
            Assert.AreEqual(result, "some_group_name");
        }
    }
}