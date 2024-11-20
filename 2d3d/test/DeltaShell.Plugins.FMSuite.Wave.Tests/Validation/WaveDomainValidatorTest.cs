using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Common.Wind;
using DeltaShell.Plugins.FMSuite.Wave.Validation;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Validation
{
    [TestFixture]
    public class WaveDomainValidatorTest
    {
        [Test]
        public void Validate_WavesModelDomainReportIsGenerated()
        {
            // Setup
            using (var model = new WaveModel())
            {
                // Call
                ValidationReport report = WaveDomainValidator.Validate(model);

                // Assert
                Assert.That(report.Category, Is.EqualTo("Waves Model Domain"));
            }
        }

        [Test]
        public void Validate_UseGlobalMeteoDataFalse_InvalidMeteoData_AddsExpectedValidationIssues()
        {
            // Setup
            using (var model = new WaveModel())
            {
                model.OuterDomain.UseGlobalMeteoData = false;
                model.OuterDomain.MeteoData.FileType = WindDefinitionType.WindXWindY;
                model.OuterDomain.MeteoData.HasSpiderWeb = true;

                // Call
                ValidationReport report = WaveDomainValidator.Validate(model);

                // Assert
                ValidationReport subReport = report.SubReports.SingleOrDefault(r => r.Category.Equals("Domain: Outer"));
                Assert.That(subReport, Is.Not.Null);

                IEnumerable<ValidationIssue> issues = subReport.Issues;
                Assert.DoesNotThrow(() => issues.Single(i =>
                                                            i.Severity == ValidationSeverity.Warning
                                                            && i.Message.Equals("Use custom wind file option is switched on but no x-component file has been selected.")));

                Assert.DoesNotThrow(() => issues.Single(i =>
                                                            i.Severity == ValidationSeverity.Warning
                                                            && i.Message.Equals("Use custom wind file option is switched on but no y-component file has been selected.")));

                Assert.DoesNotThrow(() => issues.Single(i =>
                                                            i.Severity == ValidationSeverity.Warning
                                                            && i.Message.Equals("Use spider web file option is switched on but no file has been selected.")));
            }
        }
    }
}