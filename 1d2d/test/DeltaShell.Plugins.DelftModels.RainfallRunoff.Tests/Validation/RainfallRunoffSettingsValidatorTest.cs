using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Validation;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Validation
{
    [TestFixture]
    public class RainfallRunoffSettingsValidatorTest
    {
        [Test]
        public void EvaporationPeriodSettings()
        {
            var rrm = new RainfallRunoffModel();

            var validator = new RainfallRunoffSettingsValidator();
            rrm.EvaporationStartActivePeriod = 26;
            rrm.EvaporationEndActivePeriod = -1;

            var report = validator.Validate(rrm, rrm);

            Assert.AreEqual(ValidationSeverity.Error, report.Severity());
            var issues = report.GetAllIssuesRecursive();

            Assert.AreEqual(3, issues.Count); //start > 24, end < 1, end <= start
        }
    }
}
