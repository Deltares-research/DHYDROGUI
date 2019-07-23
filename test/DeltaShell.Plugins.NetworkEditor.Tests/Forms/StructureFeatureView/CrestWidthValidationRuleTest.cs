using DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.StructureFeatureView
{
    [TestFixture]
    public class CrestWidthValidationRuleTest
    {
        #region SetUp
        private CrestWidthValidationRule rule;

        [TestFixtureSetUp]
        public void SetUpFixture()
        {
            rule = new CrestWidthValidationRule();
        }
        #endregion

        /// <summary>
        /// GIVEN a CrestValiditionRule
        ///   AND a string value
        /// WHEN this value is validated
        /// THEN the corresponding ValidationResult is correct
        /// </summary>
        [TestCase("",     true)]
        [TestCase("1.0",  true)]
        [TestCase("-1.0", false)]
        [TestCase("0.0",  false)]
        [TestCase("bacon", false)]
        public void GivenAnEmptyString_WhenThisValueIsValidated_ThenValidResultIsReturned(string val, bool expectedIsValid)
        {
            // Given | When
            var result = rule.Validate(val, null);

            // Then
            Assert.That(result.IsValid, Is.EqualTo(expectedIsValid), $"The validation of {val} is incorrect:");
        }
    }
}