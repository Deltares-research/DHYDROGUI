using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Controls;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.Validation;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.Validation
{
    [TestFixture]
    public class PositiveDoubleValidationRuleTest
    {
        private readonly Random random = new Random();

        [TestCaseSource(nameof(CultureInfos))]
        public void Validate_WithPositiveDoubleValue_ReturnsValidValidationResult(CultureInfo cultureInfo)
        {
            // Setup
            var validationRule = new PositiveDoubleValidationRule();
            double value = random.NextDouble();
            var stringValue = value.ToString(cultureInfo);

            // Call
            ValidationResult result = validationRule.Validate(stringValue, cultureInfo);

            // Assert
            Assert.That(result.IsValid, Is.True);
            Assert.That(result.ErrorContent, Is.EqualTo(null));
        }

        [TestCaseSource(nameof(CultureInfos))]
        public void Validate_WithNegativeDoubleValue_ReturnsInvalidValidationResult(CultureInfo cultureInfo)
        {
            // Setup
            var validationRule = new PositiveDoubleValidationRule();
            double value = -1 * random.NextDouble();
            var stringValue = value.ToString(cultureInfo);

            // Call
            ValidationResult result = validationRule.Validate(stringValue, cultureInfo);

            // Assert
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ErrorContent, Is.EqualTo(null));
        }

        [TestCase("NaN")]
        [TestCase("∞")]
        [TestCase("-∞")]
        [TestCase("One")]
        [TestCase("string")]
        [TestCase("null")]
        public void Validate_WithNaNValue_ReturnsInvalidValidationResult(string stringValue)
        {
            // Setup
            var validationRule = new PositiveDoubleValidationRule();

            // Call
            ValidationResult result = validationRule.Validate(stringValue, CultureInfo.CurrentCulture);

            // Assert
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.ErrorContent, Is.EqualTo(null));
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