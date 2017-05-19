using System.Linq;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation
{
    [TestFixture()]
    public class WaterFlowFMSedimentMorphologyValidatorTests
    {
        [Test]
        public void ValidateSedimentNameTest()
        {
            var validName = "Sediment_001";
            var issue = WaterFlowFMSedimentMorphologyValidator.ValidateSedimentName(validName);
            Assert.IsNull(issue);
            var invalidName = "Sediment#001";
            issue = WaterFlowFMSedimentMorphologyValidator.ValidateSedimentName(invalidName);
            Assert.IsNotNull(issue);
        }

        [Test]
        public void ValidateMorphologyBetaWarningTest()
        {
            var model = new WaterFlowFMModel();
            var report = model.Validate();
            var morReport = report.SubReports.FirstOrDefault(r => r.Category.Contains("Morphology / Sediment Beta warning"));
            Assert.IsNull(morReport);

            model.ModelDefinition.GetModelProperty(GuiProperties.UseMorSed).Value = true;
            report = model.Validate();
            morReport = report.SubReports.FirstOrDefault(r => r.Category.Contains("Morphology / Sediment Beta warning"));
            Assert.AreEqual(0, morReport.AllErrors.Count());
            Assert.IsNotNull(morReport);
            var betaWarningIssue = morReport.GetAllIssuesRecursive().FirstOrDefault(i => i.Severity == ValidationSeverity.Warning);
            Assert.IsNotNull(betaWarningIssue);

            Assert.That(betaWarningIssue.Message, Is.StringContaining("Morphology is beta version"));
        }
    }
}