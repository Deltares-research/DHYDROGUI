using System.Globalization;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Editors
{
    [TestFixture]
    public class SedimentNamePropertyValidationRuleTests
    {
        [Test]
        public void ValidateTest()
        {
            var validator = new SedimentNamePropertyValidationRule();
            var validName = "Sediment_001";
            Assert.IsTrue(validator.Validate(validName, CultureInfo.InvariantCulture).IsValid);
            var invalidName = "Sediment#001";
            Assert.IsFalse(validator.Validate(invalidName, CultureInfo.InvariantCulture).IsValid);
        }
    }
}