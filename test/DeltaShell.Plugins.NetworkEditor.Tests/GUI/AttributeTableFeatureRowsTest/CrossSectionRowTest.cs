using System.ComponentModel;
using DelftTools.Hydro.CrossSections;
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
    public class CrossSectionRowTest
    {
        [Test]
        public void Constructor_WithNullCrossSection_ThrowsArgumentNullException()
        {
            // Act
            void Call()
            {
                new CrossSectionRow(null, new NameValidator());
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
                new CrossSectionRow(Substitute.For<ICrossSection, INotifyPropertyChanged>(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void GetFeature_GetsCrossSection()
        {
            // Arrange
            var crossSectionDefinition = Substitute.For<ICrossSectionDefinition>();
            var crossSection = new CrossSection(crossSectionDefinition);
            var row = new CrossSectionRow(crossSection, new NameValidator());

            // Act
            IFeature result = row.GetFeature();

            // Assert
            Assert.That(result, Is.SameAs(crossSection));
        }

        [Test]
        public void WhenCrossSectionPropertyChanged_RowShouldFirePropertyChangedEvent()
        {
            // Arrange
            var eventRaised = false;
            var crossSectionDefinition = Substitute.For<ICrossSectionDefinition>();
            var crossSection = new CrossSection(crossSectionDefinition);
            var row = new CrossSectionRow(crossSection, new NameValidator());
            row.PropertyChanged += (sender, args) => eventRaised = true;

            // Act
            crossSection.Name = "some_name";

            // Assert
            Assert.That(eventRaised);
        }

        [Test]
        public void SetName_SetsCrossSectionName()
        {
            // Arrange
            var crossSectionDefinition = Substitute.For<ICrossSectionDefinition>();
            var crossSection = new CrossSection(crossSectionDefinition);
            var row = new CrossSectionRow(crossSection, new NameValidator());

            // Act
            row.Name = "some_name";

            // Assert
            Assert.That(crossSection.Name, Is.EqualTo("some_name"));
        }

        [Test]
        public void GetName_GetsCrossSectionName()
        {
            // Arrange
            var crossSectionDefinition = Substitute.For<ICrossSectionDefinition>();
            var crossSection = new CrossSection(crossSectionDefinition) { Name = "some_name" };
            var row = new CrossSectionRow(crossSection, new NameValidator());

            // Act
            string result = row.Name;

            // Assert
            Assert.That(result, Is.EqualTo("some_name"));
        }

        [Test]
        public void SetName_InvalidName_OriginalNameIsPreserved()
        {
            // Arrange
            var validator = Substitute.For<IValidator<string>>();
            validator.Validate("some_invalid_name").Returns(ValidationResult.Fail("message"));
            var nameValidator = new NameValidator();
            nameValidator.AddValidator(validator);

            var crossSectionDefinition = Substitute.For<ICrossSectionDefinition>();
            var crossSection = new CrossSection(crossSectionDefinition) { Name = "some_name" };
            var row = new CrossSectionRow(crossSection, nameValidator);

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

            var crossSectionDefinition = Substitute.For<ICrossSectionDefinition>();
            var crossSection = new CrossSection(crossSectionDefinition) { Name = "some_name" };
            var row = new CrossSectionRow(crossSection, nameValidator);

            // Act
            row.Name = "some_valid_name";

            // Assert
            Assert.That(row.Name, Is.EqualTo("some_valid_name"));
        }

        [Test]
        public void SetLongName_SetsCrossSectionNLongName()
        {
            // Arrange
            var crossSectionDefinition = Substitute.For<ICrossSectionDefinition>();
            var crossSection = new CrossSection(crossSectionDefinition);
            var row = new CrossSectionRow(crossSection, new NameValidator());

            // Act
            row.LongName = "some_name";

            // Assert
            Assert.That(crossSection.LongName, Is.EqualTo("some_name"));
        }

        [Test]
        public void GetLongName_GetsCrossSectionLongName()
        {
            // Arrange
            var crossSectionDefinition = Substitute.For<ICrossSectionDefinition>();
            var crossSection = new CrossSection(crossSectionDefinition) { LongName = "some_name" };
            var row = new CrossSectionRow(crossSection, new NameValidator());

            // Act
            string result = row.LongName;

            // Assert
            Assert.That(result, Is.EqualTo("some_name"));
        }

        [Test]
        public void GetBranch_GetsCompositeBranchStructureBranchName()
        {
            // Arrange
            var branch = Substitute.For<IBranch>();
            branch.Name = "some_branch_name";
            var crossSectionDefinition = Substitute.For<ICrossSectionDefinition>();
            var crossSection = new CrossSection(crossSectionDefinition) { Branch = branch };
            var row = new CrossSectionRow(crossSection, new NameValidator());

            // Act
            string result = row.Branch;

            // Assert
            Assert.That(result, Is.EqualTo("some_branch_name"));
        }

        [Test]
        public void SetChainage_SetsCrossSectionChainage()
        {
            // Arrange
            var crossSectionDefinition = Substitute.For<ICrossSectionDefinition>();
            var crossSection = new CrossSection(crossSectionDefinition);
            var row = new CrossSectionRow(crossSection, new NameValidator());

            // Act
            row.Chainage = 1.23;

            // Assert
            Assert.That(crossSection.Chainage, Is.EqualTo(1.23));
        }

        [Test]
        public void GetChainage_GetsCrossSectionChainage()
        {
            // Arrange
            var crossSectionDefinition = Substitute.For<ICrossSectionDefinition>();
            var crossSection = new CrossSection(crossSectionDefinition) { Chainage = 1.23 };
            var row = new CrossSectionRow(crossSection, new NameValidator());

            // Act
            double result = row.Chainage;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }

        [Test]
        public void GetLowestPoint_GetsCrossSectionDefinitionLowestPoint()
        {
            // Arrange
            var crossSectionDefinition = Substitute.For<ICrossSectionDefinition>();
            crossSectionDefinition.LowestPoint.Returns(1.23);
            var crossSection = new CrossSection(crossSectionDefinition);
            var row = new CrossSectionRow(crossSection, new NameValidator());

            // Act
            double result = row.LowestPoint;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }

        [Test]
        public void GetHighestPoint_GetsCrossSectionDefinitionHighestPoint()
        {
            // Arrange
            var crossSectionDefinition = Substitute.For<ICrossSectionDefinition>();
            crossSectionDefinition.HighestPoint.Returns(1.23);
            var crossSection = new CrossSection(crossSectionDefinition);
            var row = new CrossSectionRow(crossSection, new NameValidator());

            // Act
            double result = row.HighestPoint;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }

        [Test]
        public void GetCrossSectionType_GetsCrossSectionDefinitionCrossSectionType([Values] CrossSectionType crossSectionType)
        {
            // Arrange
            var crossSectionDefinition = Substitute.For<ICrossSectionDefinition>();
            crossSectionDefinition.CrossSectionType.Returns(crossSectionType);
            var crossSection = new CrossSection(crossSectionDefinition);
            var row = new CrossSectionRow(crossSection, new NameValidator());

            // Act
            CrossSectionType result = row.CrossSectionType;

            // Assert
            Assert.That(result, Is.EqualTo(crossSectionType));
        }

        [Test]
        public void GetWidth_GetsCrossSectionDefinitionWidth()
        {
            // Arrange
            var crossSectionDefinition = Substitute.For<ICrossSectionDefinition>();
            crossSectionDefinition.Width.Returns(1.23);
            var crossSection = new CrossSection(crossSectionDefinition);
            var row = new CrossSectionRow(crossSection, new NameValidator());

            // Act
            double result = row.Width;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }

        [Test]
        public void GetThalweg_GetsRoundedCrossSectionDefinitionWidth()
        {
            // Arrange
            var crossSectionDefinition = Substitute.For<ICrossSectionDefinition>();
            crossSectionDefinition.Thalweg.Returns(1.23456);
            var crossSection = new CrossSection(crossSectionDefinition);
            var row = new CrossSectionRow(crossSection, new NameValidator());

            // Act
            double result = row.Thalweg;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }

        [Test]
        public void GetDefinitionName_GetsCrossSectionDefinitionName()
        {
            // Arrange
            var crossSectionDefinition = Substitute.For<ICrossSectionDefinition>();
            crossSectionDefinition.Name = "some_cross_section_definition_name";
            var crossSection = new CrossSection(crossSectionDefinition);
            var row = new CrossSectionRow(crossSection, new NameValidator());

            // Act
            string result = row.DefinitionName;

            // Assert
            Assert.That(result, Is.EqualTo("some_cross_section_definition_name"));
        }
    }
}