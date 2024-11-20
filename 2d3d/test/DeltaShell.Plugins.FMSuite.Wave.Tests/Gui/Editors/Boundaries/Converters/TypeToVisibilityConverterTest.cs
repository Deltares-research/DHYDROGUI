using System;
using System.Globalization;
using System.Windows;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Converters;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.Shapes;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.Converters
{
    [TestFixture]
    public class TypeToVisibilityConverterTest
    {
        [Test]
        [TestCase(typeof(GaussViewShape), typeof(GaussViewShape), Visibility.Visible)]
        [TestCase(typeof(JonswapViewShape), typeof(JonswapViewShape), Visibility.Visible)]
        [TestCase(typeof(GaussViewShape), typeof(JonswapViewShape), Visibility.Collapsed)]
        [TestCase(typeof(JonswapViewShape), typeof(GaussViewShape), Visibility.Collapsed)]
        [TestCase(typeof(GaussViewShape), typeof(PiersonMoskowitzViewShape), Visibility.Collapsed)]
        [TestCase(typeof(JonswapViewShape), typeof(PiersonMoskowitzViewShape), Visibility.Collapsed)]
        public void GivenATypeToVisibilityConverter_WhenConvertIsCalled_ThenTheCorrectResultsAreExpected(Type inputType,
                                                                                                         Type maskType,
                                                                                                         object expectedResult)
        {
            // Given
            var converter = new TypeToVisibilityConverter();

            // When
            object result = converter.Convert(inputType, typeof(Visibility), maskType, CultureInfo.InvariantCulture);

            // Then
            Assert.That(result, Is.EqualTo(expectedResult),
                        "Expected a different converted value:");
        }

        [Test]
        public void GivenATypeToVisibilityConverterAndSomeValueNotAType_WhenConvertIsCalled_ThenDependencyPropertyUnsetValueIsReturned()
        {
            // Given
            var converter = new TypeToVisibilityConverter();
            var otherInput = new object();

            // When
            object result = converter.Convert(otherInput, typeof(Visibility), null, CultureInfo.InvariantCulture);

            // Then
            Assert.That(result, Is.EqualTo(DependencyProperty.UnsetValue));
        }

        [Test]
        public void GivenATypeToVisibilityConverterAndATargetTypeNotAVisibility_WhenConvertIsCalled_ThenDependencyPropertyUnsetValueIsReturned()
        {
            // Given
            var converter = new TypeToVisibilityConverter();

            // When
            object result = converter.Convert(typeof(GaussViewShape), typeof(IViewShape), null, CultureInfo.InvariantCulture);

            // Then
            Assert.That(result, Is.EqualTo(DependencyProperty.UnsetValue));
        }

        [Test]
        public void ConvertBack_ThrowsNotSupportedException()
        {
            // Call | Assert
            void Call() => new TypeToVisibilityConverter().ConvertBack(typeof(GaussViewShape), typeof(Visibility), null, CultureInfo.InvariantCulture);
            Assert.Throws<NotSupportedException>(Call);
        }
    }
}