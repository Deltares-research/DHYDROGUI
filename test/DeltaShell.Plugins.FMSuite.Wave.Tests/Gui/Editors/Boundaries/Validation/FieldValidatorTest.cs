using System;
using System.Collections.Generic;
using System.Globalization;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Validation;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.Validation
{
    [TestFixture]
    public class FieldValidatorTest
    {
        private readonly Random random = new Random();

        [TestCaseSource(nameof(CultureInfos))]
        public void Validate_WithPositiveDoubleValue_ReturnsValidValidationResult(CultureInfo cultureInfo)
        {
            // Setup
            double value = random.NextDouble();
            string stringValue = value.ToString(cultureInfo);

            // Call
            bool result = FieldValidator.IsPositiveDouble(stringValue, cultureInfo);

            // Assert
            Assert.That(result, Is.True);
        }

        [TestCaseSource(nameof(CultureInfos))]
        public void Validate_WithNegativeDoubleValue_ReturnsInvalidValidationResult(CultureInfo cultureInfo)
        {
            // Setup
            double value = -1 * random.NextDouble();
            string stringValue = value.ToString(cultureInfo);

            // Call
            bool result = FieldValidator.IsPositiveDouble(stringValue, cultureInfo);

            // Assert
            Assert.That(result, Is.False);
        }

        [TestCase("NaN")]
        [TestCase("One")]
        [TestCase("string")]
        [TestCase("null")]
        public void Validate_WithNaNValue_ReturnsInvalidValidationResult(string stringValue)
        {
            // Call
            bool result = FieldValidator.IsPositiveDouble(stringValue, CultureInfo.CurrentCulture);

            // Assert
            Assert.That(result, Is.False);
        }

        private static IEnumerable<CultureInfo> CultureInfos()
        {
            yield return CultureInfo.CurrentCulture;
            yield return CultureInfo.CurrentUICulture;
            yield return CultureInfo.DefaultThreadCurrentCulture;
            yield return CultureInfo.DefaultThreadCurrentUICulture;
            yield return CultureInfo.InstalledUICulture;
            yield return CultureInfo.InvariantCulture;
        }
    }
}