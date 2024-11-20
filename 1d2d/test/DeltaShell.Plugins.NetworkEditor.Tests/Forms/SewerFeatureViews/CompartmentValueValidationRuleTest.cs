using System.Globalization;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.SewerFeatureViews
{
    [TestFixture]
    public class CompartmentValueValidationRuleTest
    {
        [Test]
        public void GivenValidValue_WhenCallingValidate_ThenResultIsValid()
        {
            // Given
            var rule = new CompartmentValueValidationRule();

            // When
            var result = rule.Validate(1.1, CultureInfo.InvariantCulture);

            // Then
            Assert.IsTrue(result.IsValid);
        }

        [Test]
        [TestCase(null, "Null value is invalid")]
        [TestCase("Test", "The value of this parameter must be a double precision number.")]
        [TestCase(-1, "The value of this parameter must be larger than 0.")]
        [TestCase(0, "The value of this parameter must be larger than 0.")]
        public void GivenInvalidValue_WhenCallingValidate_ThenResultIsInvalidWithExpectedMessage(object invalidValue, string expectedMessage)
        {
            // Given
            var rule = new CompartmentValueValidationRule();

            // When
            var result = rule.Validate(invalidValue, CultureInfo.InvariantCulture);

            // Then
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(expectedMessage, result.ErrorContent.ToString());
        }
    }
}
