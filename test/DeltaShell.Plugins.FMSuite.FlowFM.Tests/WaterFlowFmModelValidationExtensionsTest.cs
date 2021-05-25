using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils.Validation;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    public class WaterFlowFmModelValidationExtensionsTest
    {
        [TestCaseSource(nameof(ValidateCoverageCases))]
        public void Validate_CoverageContainsNoDataValue_ReturnsValidationReportWithCorrectIssue(Func<WaterFlowFMModel, IFunction> getCoverage)
        {
            // Setup
            using (var model = new WaterFlowFMModel())
            {
                ConfigureValidModel(model);

                IFunction coverage = getCoverage(model);
                SetOneValueWith(coverage, -999d);

                // Call
                ValidationReport report = model.Validate();

                // Assert
                Assert.That(report.Category, Is.EqualTo("FlowFM (Water Flow FM Model)"));
                
                ValidationReport physicalProcessesReport = report.SubReports.Single(r => r.Category == "Physical Processes");
                Assert.That(physicalProcessesReport.InfoCount, Is.EqualTo(1));
                
                ValidationIssue issue = physicalProcessesReport.Issues.Single();
                Assert.That(issue.ViewData, Is.SameAs(model));
                Assert.That(issue.Subject, Is.SameAs(model));
                Assert.That(issue.Severity, Is.EqualTo(ValidationSeverity.Info));
                Assert.That(issue.Message, Is.EqualTo($"{coverage.Name} contains unspecified points, the calculation kernel will replace these with default values."));
            }
        }

        private static IEnumerable<TestCaseData> ValidateCoverageCases()
        {
            yield return new TestCaseData((Func<WaterFlowFMModel, IFunction>) (data => data.Roughness));
            yield return new TestCaseData((Func<WaterFlowFMModel, IFunction>) (data => data.Viscosity));
            yield return new TestCaseData((Func<WaterFlowFMModel, IFunction>) (data => data.Diffusivity));
            yield return new TestCaseData((Func<WaterFlowFMModel, IFunction>) (data => data.Infiltration));
        }
        
        private static void SetOneValueWith(IFunction function, double value)
        {
            function.Components[0].Values[3] = value;
        }
        
        private static void ConfigureValidModel(WaterFlowFMModel model)
        {
            SetUniformValues(model.Roughness);
            SetUniformValues(model.Viscosity);
            SetUniformValues(model.Diffusivity);
            SetUniformValues(model.Infiltration);
            
        }
        
        private static void SetUniformValues(IFunction function, double value = 7d)
        {
            FunctionHelper.SetValuesRaw(function.Components[0], Enumerable.Repeat(value, 10));
        }
    }
}