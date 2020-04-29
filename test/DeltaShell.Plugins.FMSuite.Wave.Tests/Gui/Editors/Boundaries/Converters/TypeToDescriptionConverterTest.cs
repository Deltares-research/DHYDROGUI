using System;
using System.Globalization;
using System.Windows;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Converters;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.Shapes;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.Converters
{
    [TestFixture]
    public class TypeToDescriptionConverterTest
    {
        [Test]
        [TestCase(typeof(GaussViewShape), "Gauss")]
        [TestCase(typeof(JonswapViewShape), "Jonswap")]
        [TestCase(typeof(PiersonMoskowitzViewShape), "Pierson-Moskowitz")]
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
            object result = converter.Convert(typeof(IViewShape), typeof(string), null, CultureInfo.InvariantCulture);

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
            object result = converter.Convert(typeof(GaussViewShape), typeof(IViewShape), null, CultureInfo.InvariantCulture);

            // Then
            Assert.That(result, Is.EqualTo(DependencyProperty.UnsetValue));
        }

        [Test]
        public void ConvertBack_ThrowsNotSupportedException()
        {
            // Call | Assert
            void Call() => (new TypeToDescriptionConverter()).ConvertBack(typeof(GaussViewShape), typeof(string), null, CultureInfo.InvariantCulture);
            Assert.Throws<NotSupportedException>(Call);
        }
    }
}