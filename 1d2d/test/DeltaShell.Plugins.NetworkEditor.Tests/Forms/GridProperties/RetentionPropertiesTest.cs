﻿using DelftTools.Hydro;
using DelftTools.TestUtils;
using Deltares.Infrastructure.API.Validation;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.GridProperties
{
    [TestFixture]
    public class RetentionPropertiesTest
    {
        [Test]
        public void SetName_InvalidName_OriginalNameIsPreserved()
        {
            // Arrange
            var validator = Substitute.For<IValidator<string>>();
            validator.Validate("some_invalid_name").Returns(ValidationResult.Fail("message"));

            var data = new Retention { Name = "some_name" };
            var properties = new RetentionProperties { Data = data };
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

            var data = new Retention { Name = "some_name" };
            var properties = new RetentionProperties { Data = data };
            properties.NameValidator.AddValidator(validator);

            // Act
            properties.Name = "some_valid_name";

            // Assert
            Assert.That(properties.Name, Is.EqualTo("some_valid_name"));
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowProperties()
        {
            WindowsFormsTestHelper.ShowPropertyGridForObject(new RetentionProperties { Data = new Retention() });
        }
    }
}