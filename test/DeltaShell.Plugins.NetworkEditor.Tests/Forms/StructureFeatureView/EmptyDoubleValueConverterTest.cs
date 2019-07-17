using System;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.StructureFeatureView
{
    [TestFixture]
    public class EmptyDoubleValueConverterTest
    {
        #region SetUp
        private EmptyDoubleValueConverter converter;

        [TestFixtureSetUp]
        public void SetUpFixture()
        {
            converter = new EmptyDoubleValueConverter();
        }
        #endregion

        /// <summary>
        /// GIVEN an EmptyDoubleValueConverter
        ///   AND an empty string
        /// WHEN ConvertBack is called with this string
        /// THEN NaN is returned
        /// </summary>
        [Test]
        public void GivenAnEmptyString_WhenConvertBackIsCalledWithThisString_ThenNaNIsReturned()
        {
            // Given | When 
            var result = converter.ConvertBack(string.Empty, typeof(Double),  null, null);

            // Then
            Assert.That(result, Is.TypeOf<Double>(), "The type of the result is not of the expected type.");
            var doubleResult = (double) result;
            Assert.That(doubleResult, Is.NaN);
        }

        /// <summary>
        /// GIVEN an EmptyDoubleValueConverter
        ///   AND a non-empty string
        /// WHEN ConvertBack is called with this string
        /// THEN This value is returned
        /// </summary>
        [Test]
        public void GivenANonEmptyString_WhenConvertBackIsCalledWithThisString_ThenThisValueIsReturned()
        {
            // Given
            const string originalValue = "0.0";

            // When
            var result = converter.ConvertBack(originalValue, typeof(Double), null, null);

            // Then
            Assert.That(result, Is.TypeOf<string>(), "The type of the result is not of the expected type.");
            Assert.That(result, Is.EqualTo(originalValue), "Expected value does not match result value.");
        }

        /// <summary>
        /// GIVEN an EmptyDoubleValueConverter
        ///   AND a NaN value
        /// WHEN Convert is called with this value
        /// THEN an empty string is returned
        /// </summary>
        [Test]
        public void GivenANaNValue_WhenConvertIsCalledWithThisValue_ThenAnEmptyStringIsReturned()
        {
            // Given | When
            var result = converter.Convert(double.NaN, typeof(String), null, null);

            // Then
            Assert.That(result, Is.TypeOf<string>(), "The type of the result is not of the expected type.");
            var resultStr = (string) result;
            Assert.That(resultStr, Is.Empty, "Expected value does not match result value.");
        }

        /// <summary>
        /// GIVEN an EmptyDoubleValueConverter
        ///   AND not a NaN value
        /// WHEN Convert is called with this value
        /// THEN the value is returned
        /// </summary>
        [Test]
        public void GivANaNValue_WhenConvertIsCalledWithThisValue_ThenTheValueIsReturned()
        {
            const double originalValue = 1.0;
            // Given | When
            var result = converter.Convert(originalValue, typeof(String), null, null);

            // Then
            Assert.That(result, Is.TypeOf<double>(), "The type of the result is not of the expected type.");
            Assert.That(result, Is.EqualTo(originalValue), "Expected value does not match result value.");
        }
    }
}