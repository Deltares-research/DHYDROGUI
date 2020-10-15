using System;
using System.Globalization;
using System.Windows;
using DeltaShell.NGHS.Common.Gui.Converters;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Tests.Gui.Converters
{
    [TestFixture]
    public class TypeToDescriptionConverterTest
    {
        [Test]
        [TestCase(typeof(DescriptionTestClass), "Description")]
        [TestCase(typeof(DescriptionWithSpacesTestClass), "Description description")]
        public void GivenATypeToDescriptionConverterAndATypeWithDescription_WhenConvertIsCalled_ThenTheCorrespondingDescriptionValueIsReturned(Type inputType, string expectedResult)
        {
            // Given
            var converter = new TypeToDescriptionConverter();

            // When
            object result = converter.Convert(inputType, typeof(string), null, CultureInfo.InvariantCulture);

            // Then
            Assert.That(result, Is.EqualTo(expectedResult),
                        "Expected a different Description to be returned:");
        }

        [Test]
        public void GivenATypeToDescriptionConverterAndATypeWithoutDescription_WhenConvertIsCalled_ThenDependencyPropertyUnsetValueIsReturned()
        {
            // Given
            var converter = new TypeToDescriptionConverter();

            // When
            object result = converter.Convert(typeof(WithoutDescriptionTestClass), typeof(string), null, CultureInfo.InvariantCulture);

            // Then
            Assert.That(result, Is.EqualTo(DependencyProperty.UnsetValue));
        }

        [Test]
        public void GivenATypeToDescriptionConverterAndSomeValueNotAType_WhenConvertIsCalled_ThenDependencyPropertyUnsetValueIsReturned()
        {
            // Given
            var converter = new TypeToDescriptionConverter();
            var otherInput = new object();

            // When
            object result = converter.Convert(otherInput, typeof(string), null, CultureInfo.InvariantCulture);

            // Then
            Assert.That(result, Is.EqualTo(DependencyProperty.UnsetValue));
        }

        [Test]
        public void GivenATypeToDescriptionConverterAndTargetTypeNotAString_WhenConvertIsCalled_ThenDependencyPropertyUnsetValueIsReturned()
        {
            // Given
            var converter = new TypeToDescriptionConverter();

            // When
            object result = converter.Convert(typeof(DescriptionTestClass), typeof(object), null, CultureInfo.InvariantCulture);

            // Then
            Assert.That(result, Is.EqualTo(DependencyProperty.UnsetValue));
        }

        [Test]
        public void ConvertBack_ThrowsNotSupportedException()
        {
            // Call | Assert
            void Call() => new TypeToDescriptionConverter().ConvertBack(typeof(DescriptionTestClass), typeof(string), null, CultureInfo.InvariantCulture);
            Assert.Throws<NotSupportedException>(Call);
        }

        [System.ComponentModel.Description("Description")]
        private class DescriptionTestClass {}

        [System.ComponentModel.Description("Description description")]
        private class DescriptionWithSpacesTestClass {}

        private class WithoutDescriptionTestClass {}
    }
}