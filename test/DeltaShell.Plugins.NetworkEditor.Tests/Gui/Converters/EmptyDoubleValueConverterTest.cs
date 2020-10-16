using DeltaShell.Plugins.NetworkEditor.Gui.Converters;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Gui.Converters
{
    [TestFixture]
    public class EmptyDoubleValueConverterTest
    {
        /// <summary>
        /// GIVEN an EmptyDoubleValueConverter
        /// AND an empty string
        /// WHEN ConvertBack is called with this string
        /// THEN NaN is returned
        /// </summary>
        [Test]
        public void GivenAnEmptyString_WhenConvertBackIsCalledWithThisString_ThenNaNIsReturned()
        {
            // Given | When 
            object result = converter.ConvertBack(string.Empty, typeof(double), null, null);

            // Then
            Assert.That(result, Is.TypeOf<double>(), "The type of the result is not of the expected type.");
            var doubleResult = (double) result;
            Assert.That(doubleResult, Is.NaN);
        }

        /// <summary>
        /// GIVEN an EmptyDoubleValueConverter
        /// AND a non-empty string
        /// WHEN ConvertBack is called with this string
        /// THEN This value is returned
        /// </summary>
        [Test]
        public void GivenANonEmptyString_WhenConvertBackIsCalledWithThisString_ThenThisValueIsReturned()
        {
            // Given
            const string originalValue = "0.0";

            // When
            object result = converter.ConvertBack(originalValue, typeof(double), null, null);

            // Then
            Assert.That(result, Is.TypeOf<string>(), "The type of the result is not of the expected type.");
            Assert.That(result, Is.EqualTo(originalValue), "Expected value does not match result value.");
        }

        /// <summary>
        /// GIVEN an EmptyDoubleValueConverter
        /// AND a NaN value
        /// WHEN Convert is called with this value
        /// THEN an empty string is returned
        /// </summary>
        [Test]
        public void GivenANaNValue_WhenConvertIsCalledWithThisValue_ThenAnEmptyStringIsReturned()
        {
            // Given | When
            object result = converter.Convert(double.NaN, typeof(string), null, null);

            // Then
            Assert.That(result, Is.TypeOf<string>(), "The type of the result is not of the expected type.");
            var resultStr = (string) result;
            Assert.That(resultStr, Is.Empty, "Expected value does not match result value.");
        }

        /// <summary>
        /// GIVEN an EmptyDoubleValueConverter
        /// AND not a NaN value
        /// WHEN Convert is called with this value
        /// THEN the value is returned
        /// </summary>
        [Test]
        public void GivANaNValue_WhenConvertIsCalledWithThisValue_ThenTheValueIsReturned()
        {
            const double originalValue = 1.0;
            // Given | When
            object result = converter.Convert(originalValue, typeof(string), null, null);

            // Then
            Assert.That(result, Is.TypeOf<double>(), "The type of the result is not of the expected type.");
            Assert.That(result, Is.EqualTo(originalValue), "Expected value does not match result value.");
        }

        #region SetUp

        private EmptyDoubleValueConverter converter;

        [OneTimeSetUp]
        public void SetUpFixture()
        {
            converter = new EmptyDoubleValueConverter();
        }

        #endregion
    }
}