using System;
using System.Collections.Generic;
using DelftTools.Utils;
using DelftTools.Utils.Data;
using DelftTools.Utils.Validation.Common;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class HydroLinkTest
    {
        [Test]
        public void DefaultConstructor_InitializesInstanceCorrectly()
        {
            // Call
            var hydroLink = new HydroLink();

            // Assert
            AssertIsInstanceOf(hydroLink);
            Assert.That(hydroLink.Name, Is.Null);
            Assert.That(hydroLink.Source, Is.Null);
            Assert.That(hydroLink.Target, Is.Null);
            Assert.That(hydroLink.Geometry, Is.Null);
            Assert.That(hydroLink.Attributes, Is.Null);
        }

        private static void AssertIsInstanceOf(HydroLink hydroLink)
        {
            Assert.That(hydroLink, Is.InstanceOf<Unique<long>>());
            Assert.That(hydroLink, Is.InstanceOf<IFeature>());
            Assert.That(hydroLink, Is.InstanceOf<INameable>());
        }

        [Test]
        [TestCaseSource(nameof(ConstructorArgumentNullCases))]
        public void Constructor_ArgumentNull_ThrowsArgumentNullException(IHydroObject source, IHydroObject target, string expParamName)
        {
            // Call
            void Call() => new HydroLink(source, target);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo(expParamName));
        }

        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Setup
            IHydroObject source = CreateHydroObject("source_name");
            IHydroObject target = CreateHydroObject("target_name");

            // Call
            var hydroLink = new HydroLink(source, target);

            // Assert
            AssertIsInstanceOf(hydroLink);
            Assert.That(hydroLink.Name, Is.EqualTo($"HL_source_name_target_name"));
            Assert.That(hydroLink.Source, Is.SameAs(source));
            Assert.That(hydroLink.Target, Is.SameAs(target));
            Assert.That(hydroLink.Geometry, Is.Null);
            Assert.That(hydroLink.Attributes, Is.Null);
        }

        [Test]
        public void Clone_ClonesInstanceCorrectly()
        {
            // Setup
            IHydroObject source = CreateHydroObject("source_name");
            IHydroObject target = CreateHydroObject("target_name");

            var attributes = Substitute.For<IFeatureAttributeCollection>();
            var clonedAttributes = Substitute.For<IFeatureAttributeCollection>();
            attributes.Clone().Returns(clonedAttributes);

            var geometry = Substitute.For<IGeometry>();
            var clonedGeometry = Substitute.For<IGeometry>();
            geometry.Clone().Returns(clonedGeometry);

            var hydroLink = new HydroLink(source, target)
            {
                Attributes = attributes,
                Geometry = geometry
            };

            // Call
            var clonedHydroLink = (HydroLink)hydroLink.Clone();

            // Assert
            Assert.That(clonedHydroLink.Name, Is.EqualTo("HL_source_name_target_name"));
            Assert.That(clonedHydroLink.Source, Is.SameAs(hydroLink.Source));
            Assert.That(clonedHydroLink.Target, Is.SameAs(hydroLink.Target));
            Assert.That(clonedHydroLink.Geometry, Is.SameAs(clonedGeometry));
            Assert.That(clonedHydroLink.Attributes, Is.SameAs(clonedAttributes));
        }

        [Test]
        public void SetSource_ValueNull_ThrowsArgumentNullException()
        {
            // Setup
            var hydroLink = new HydroLink();

            // Call
            void Call() => hydroLink.Source = null;

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("Source"));
        }

        [Test]
        public void SetSource_SetsSource()
        {
            // Setup
            var hydroLink = new HydroLink();
            var source = Substitute.For<IHydroObject>();

            // Call
            hydroLink.Source = source;

            // Assert
            Assert.That(hydroLink.Source, Is.SameAs(source));
        }

        [Test]
        public void SetTarget_SetsTarget()
        {
            // Setup
            var hydroLink = new HydroLink();
            var target = Substitute.For<IHydroObject>();

            // Call
            hydroLink.Target = target;

            // Assert
            Assert.That(hydroLink.Target, Is.SameAs(target));
        }

        [Test]
        public void SetTarget_ValueNull_ThrowsArgumentNullException()
        {
            // Setup
            var hydroLink = new HydroLink();

            // Call
            void Call() => hydroLink.Target = null;

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("Target"));
        }

        [Test]
        public void SetNameIfValid_ValidName_NameIsUpdated()
        {
            // Arrange
            var validator = Substitute.For<IValidator<string>>();
            validator.Validate("some_valid_name").Returns(ValidationResult.Success);

            var data = new HydroLink { Name = "some_name" };
            data.AttachNameValidator(validator);

            // Act
            data.SetNameIfValid("some_valid_name");

            // Assert
            Assert.That(data.Name, Is.EqualTo("some_valid_name"));
        }

        [Test]
        public void AttachNameValidator_SubValidatorNull_ThrowsArgumentNullException()
        {
            // Arrange
            var data = new HydroLink { Name = "some_name" };

            // Act
            void Call() => data.AttachNameValidator(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void DetachNameValidator_SubValidatorNull_ThrowsArgumentNullException()
        {
            // Arrange
            var data = new HydroLink { Name = "some_name" };

            // Act
            void Call() => data.DetachNameValidator(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void DetachNameValidator_RemovesValidator()
        {
            // Arrange
            var validator = Substitute.For<IValidator<string>>();
            validator.Validate("some_invalid_name").Returns(ValidationResult.Fail("message"));

            var data = new HydroLink { Name = "some_name" };
            data.AttachNameValidator(validator);

            // Pre-conditions
            data.SetNameIfValid("some_invalid_name");
            Assert.That(data.Name, Is.EqualTo("some_name"));

            // Act
            data.DetachNameValidator(validator);
            data.SetNameIfValid("some_invalid_name");

            // Assert
            Assert.That(data.Name, Is.EqualTo("some_invalid_name"));
        }

        private static IHydroObject CreateHydroObject(string name)
        {
            var hydroObject = Substitute.For<IHydroObject>();
            hydroObject.Name = name;

            return hydroObject;
        }

        private static IEnumerable<TestCaseData> ConstructorArgumentNullCases()
        {
            yield return new TestCaseData(null, Substitute.For<IHydroObject>(), "source");
            yield return new TestCaseData(Substitute.For<IHydroObject>(), null, "target");
        }
    }
}