using System;
using System.ComponentModel;
using DeltaShell.NGHS.IO.Helpers;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.Helpers
{
    [TestFixture]
    public class DelftIniCategoryTest
    {
        [Test]
        public void GetProperty_StringComparisonNotDefined_ThrowsInvalidEnumArgumentException()
        {
            // Setup
            var category = new DelftIniCategory("some_category");

            // Call
            void Call() => category.GetProperty("some_property", (StringComparison) 99);

            // Assert
            var e = Assert.Throws<InvalidEnumArgumentException>(Call);
            Assert.That(e.Message, Is.EqualTo("stringComparison"));
        }

        [TestCase("some_property")]
        [TestCase("Some_property")]
        [TestCase("Some_Property")]
        [TestCase("SOME_PROPERTY")]
        public void GetProperty_Default_GetsCorrectProperty(string propertyName)
        {
            // Setup
            var property = new DelftIniProperty("some_property", "some_value", "some_comment");
            var category = new DelftIniCategory("some_category");
            category.Properties.Add(property);

            // Call
            IDelftIniProperty result = category.GetProperty(propertyName);

            // Assert
            Assert.That(result, Is.SameAs(property));
        }

        [TestCase("Some_property", StringComparison.Ordinal)]
        [TestCase("Some_property", StringComparison.CurrentCulture)]
        [TestCase("Some_property", StringComparison.InvariantCulture)]
        [TestCase("some_other_property", StringComparison.OrdinalIgnoreCase)]
        [TestCase("some_other_property", StringComparison.CurrentCultureIgnoreCase)]
        [TestCase("some_other_property", StringComparison.InvariantCultureIgnoreCase)]
        public void GetProperty_PropertyNotFound_ReturnsNull(string propertyName, StringComparison stringComparison)
        {
            // Setup
            var property = new DelftIniProperty("some_property", "some_value", "some_comment");
            var category = new DelftIniCategory("some_category");
            category.AddProperty(property);

            // Call
            IDelftIniProperty result = category.GetProperty(propertyName, stringComparison);

            // Assert
            Assert.That(result, Is.Null);
        }
    }
}