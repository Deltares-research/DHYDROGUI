using System.Globalization;
using System.Windows;
using DeltaShell.NGHS.Common.Gui.Converters;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Tests.Gui.Converters
{
    [TestFixture]
    public class UnitStringToUnitDisplayStringConverterTest
    {
        [Test]
        [TestCase("m", "[m]")]
        [TestCase("Kg", "[Kg]")]
        [TestCase("-", "[-]")]
        [TestCase("", "[]")]
        public void GivenAUnitStringToUnitDisplayStringConverter_WhenConvertIsCalledWithAString_ThenTheCorrectValueIsReturned(string inputString, string expectedOutput)
        {
            // Given
            var converter = new UnitStringToUnitDisplayStringConverter();

            // When
            object result = converter.Convert(inputString, typeof(string), null, CultureInfo.InvariantCulture);

            // Then
            Assert.That(result, Is.EqualTo(expectedOutput));
        }

        [Test]
        public void GivenAUnitStringToUnitDisplayStringConverter_WhenConvertIsCalledWithAnObjectOtherThanAString_ThenDependencyPropertyUnsetIsReturned()
        {
            // Given
            var converter = new UnitStringToUnitDisplayStringConverter();
            var someObject = new object();

            // When
            object result = converter.Convert(someObject, typeof(string), null, CultureInfo.InvariantCulture);

            // Then
            Assert.That(result, Is.EqualTo(DependencyProperty.UnsetValue));
        }

        [Test]
        public void GivenAUnitStringToUnitDisplayStringConverter_WhenConvertIsCalledWithAnotherTargetTypeThanAString_ThenDependencyPropertyUnsetIsReturned()
        {
            // Given
            var converter = new UnitStringToUnitDisplayStringConverter();

            // When
            object result = converter.Convert("m", typeof(object), null, CultureInfo.InvariantCulture);

            // Then
            Assert.That(result, Is.EqualTo(DependencyProperty.UnsetValue));
        }

        [Test]
        [TestCase("[m]", "m")]
        [TestCase("[Kg]", "Kg")]
        [TestCase("[-]", "-")]
        [TestCase("[]", "")]
        public void GivenAUnitStringToUnitDisplayStringConverter_WhenConvertBackIsCalledWithAString_ThenTheCorrectValueIsReturned(string inputString, string expectedOutput)
        {
            // Given
            var converter = new UnitStringToUnitDisplayStringConverter();

            // When
            object result = converter.ConvertBack(inputString, typeof(string), null, CultureInfo.InvariantCulture);

            // Then
            Assert.That(result, Is.EqualTo(expectedOutput));
        }

        [Test]
        public void GivenAUnitStringToUnitDisplayStringConverter_WhenConvertBackIsCalledWithAnObjectOtherThanAString_ThenDependencyPropertyUnsetIsReturned()
        {
            // Given
            var converter = new UnitStringToUnitDisplayStringConverter();
            var someObject = new object();

            // When
            object result = converter.ConvertBack(someObject, typeof(string), null, CultureInfo.InvariantCulture);

            // Then
            Assert.That(result, Is.EqualTo(DependencyProperty.UnsetValue));
        }

        [Test]
        public void GivenAUnitStringToUnitDisplayStringConverter_WhenConvertBackIsCalledWithAnotherTargetTypeThanAString_ThenDependencyPropertyUnsetIsReturned()
        {
            // Given
            var converter = new UnitStringToUnitDisplayStringConverter();

            // When
            object result = converter.ConvertBack("[m]", typeof(object), null, CultureInfo.InvariantCulture);

            // Then
            Assert.That(result, Is.EqualTo(DependencyProperty.UnsetValue));
        }
    }
}