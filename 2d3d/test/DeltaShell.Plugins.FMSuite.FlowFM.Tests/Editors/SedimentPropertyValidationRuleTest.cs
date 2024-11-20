using System.Globalization;
using System.Windows.Controls;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Editors
{
    [TestFixture]
    public class SedimentPropertyValidationRuleTest
    {
        [Test]
        public void TestSedimentPropertyValidationRuleDoesNotAcceptStrings()
        {
            var validationRule = new SedimentPropertyValidationRule()
            {
                MinValue = new ComparisonValue() {Value = 0.36},
                MaxValue = new ComparisonValue() {Value = 0.87}
            };

            Assert.AreNotEqual(ValidationResult.ValidResult, validationRule.Validate("asdf", CultureInfo.InvariantCulture));
        }

        [Test]
        public void TestSedimentPropertyValidationRuleValidation()
        {
            var validationRule = new SedimentPropertyValidationRule()
            {
                MinValue = new ComparisonValue() {Value = 0.36},
                MaxValue = new ComparisonValue() {Value = 0.87}
            };

            Assert.AreNotEqual(ValidationResult.ValidResult, validationRule.Validate(0.35, CultureInfo.InvariantCulture));
            Assert.AreEqual(ValidationResult.ValidResult, validationRule.Validate(0.36, CultureInfo.InvariantCulture));
            Assert.AreEqual(ValidationResult.ValidResult, validationRule.Validate(0.37, CultureInfo.InvariantCulture));

            Assert.AreEqual(ValidationResult.ValidResult, validationRule.Validate(0.86, CultureInfo.InvariantCulture));
            Assert.AreEqual(ValidationResult.ValidResult, validationRule.Validate(0.87, CultureInfo.InvariantCulture));
            Assert.AreNotEqual(ValidationResult.ValidResult, validationRule.Validate(0.88, CultureInfo.InvariantCulture));
        }

        [Test]
        public void TestSedimentPropertyValidationRuleWithOpenLimitsValidation()
        {
            var validationRule = new SedimentPropertyValidationRule()
            {
                MinValue = new ComparisonValue() {Value = 0.36},
                MaxValue = new ComparisonValue() {Value = 0.87},
                MinIsOpened = new ComparisonBoolValue() {Value = true},
                MaxIsOpened = new ComparisonBoolValue() {Value = true}
            };

            Assert.AreNotEqual(ValidationResult.ValidResult, validationRule.Validate(0.35, CultureInfo.InvariantCulture));
            Assert.AreNotEqual(ValidationResult.ValidResult, validationRule.Validate(0.36, CultureInfo.InvariantCulture));
            Assert.AreEqual(ValidationResult.ValidResult, validationRule.Validate(0.37, CultureInfo.InvariantCulture));

            Assert.AreEqual(ValidationResult.ValidResult, validationRule.Validate(0.86, CultureInfo.InvariantCulture));
            Assert.AreNotEqual(ValidationResult.ValidResult, validationRule.Validate(0.87, CultureInfo.InvariantCulture));
            Assert.AreNotEqual(ValidationResult.ValidResult, validationRule.Validate(0.88, CultureInfo.InvariantCulture));

            validationRule.MaxIsOpened = new ComparisonBoolValue() {Value = false};

            Assert.AreNotEqual(ValidationResult.ValidResult, validationRule.Validate(0.35, CultureInfo.InvariantCulture));
            Assert.AreNotEqual(ValidationResult.ValidResult, validationRule.Validate(0.36, CultureInfo.InvariantCulture));
            Assert.AreEqual(ValidationResult.ValidResult, validationRule.Validate(0.37, CultureInfo.InvariantCulture));

            Assert.AreEqual(ValidationResult.ValidResult, validationRule.Validate(0.86, CultureInfo.InvariantCulture));
            Assert.AreEqual(ValidationResult.ValidResult, validationRule.Validate(0.87, CultureInfo.InvariantCulture));
            Assert.AreNotEqual(ValidationResult.ValidResult, validationRule.Validate(0.88, CultureInfo.InvariantCulture));

            validationRule.MaxIsOpened = new ComparisonBoolValue() {Value = true};
            validationRule.MinIsOpened = new ComparisonBoolValue() {Value = false};

            Assert.AreNotEqual(ValidationResult.ValidResult, validationRule.Validate(0.35, CultureInfo.InvariantCulture));
            Assert.AreEqual(ValidationResult.ValidResult, validationRule.Validate(0.36, CultureInfo.InvariantCulture));
            Assert.AreEqual(ValidationResult.ValidResult, validationRule.Validate(0.37, CultureInfo.InvariantCulture));

            Assert.AreEqual(ValidationResult.ValidResult, validationRule.Validate(0.86, CultureInfo.InvariantCulture));
            Assert.AreNotEqual(ValidationResult.ValidResult, validationRule.Validate(0.87, CultureInfo.InvariantCulture));
            Assert.AreNotEqual(ValidationResult.ValidResult, validationRule.Validate(0.88, CultureInfo.InvariantCulture));

            validationRule.MaxIsOpened = new ComparisonBoolValue() {Value = false};

            Assert.AreNotEqual(ValidationResult.ValidResult, validationRule.Validate(0.35, CultureInfo.InvariantCulture));
            Assert.AreEqual(ValidationResult.ValidResult, validationRule.Validate(0.36, CultureInfo.InvariantCulture));
            Assert.AreEqual(ValidationResult.ValidResult, validationRule.Validate(0.37, CultureInfo.InvariantCulture));

            Assert.AreEqual(ValidationResult.ValidResult, validationRule.Validate(0.86, CultureInfo.InvariantCulture));
            Assert.AreEqual(ValidationResult.ValidResult, validationRule.Validate(0.87, CultureInfo.InvariantCulture));
            Assert.AreNotEqual(ValidationResult.ValidResult, validationRule.Validate(0.88, CultureInfo.InvariantCulture));
        }
    }
}