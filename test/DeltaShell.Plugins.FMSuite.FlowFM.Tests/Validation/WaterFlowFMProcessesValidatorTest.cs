using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils.Validation;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using NUnit.Framework;
using SharpMapTestUtils;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation
{
    [TestFixture]
    public class WaterFlowFMProcessesValidatorTest
    {
        private static readonly Random random = new Random();

        [Test]
        public void Validate_ModelNull_ThrowsArgumentNUllException()
        {
            // Call
            void Call() => WaterFlowFMProcessesValidator.Validate(null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("model"));
        }

        [Test]
        public void Validate_MeteoDataIsEmpty_ReturnsValidationReportWithCorrectIssue()
        {
            // Setup
            using (var model = new WaterFlowFMModel())
            {
                model.Grid = UnstructuredGridTestHelper.GenerateRegularGrid(3, 3, 1, 1);

                model.ModelDefinition.HeatFluxModel.Type = HeatFluxModelType.Composite;
                SetValues(model.SpatialData.Roughness);
                SetValues(model.SpatialData.Viscosity);
                SetValues(model.SpatialData.Diffusivity);

                // Precondition
                Assert.That(model.ModelDefinition.HeatFluxModel.MeteoData.GetValues<double>(), Is.Empty);

                // Call
                ValidationReport report = WaterFlowFMProcessesValidator.Validate(model);

                // Assert
                Assert.That(report.Category, Is.EqualTo("Physical Processes"));
                Assert.That(report.Issues, Has.Count.EqualTo(1));
                Assert.That(report.ErrorCount, Is.EqualTo(1));

                ValidationIssue issue = report.Issues.Single();
                Assert.That(issue.ViewData, Is.SameAs(model.ModelDefinition.HeatFluxModel));
                Assert.That(issue.Subject, Is.SameAs(model));
                Assert.That(issue.Severity, Is.EqualTo(ValidationSeverity.Error));
                Assert.That(issue.Message, Is.EqualTo("Composite Model option is selected for Temperature, however no Meteo Data was specified."));
            }
        }

        [TestCase(HeatFluxModelType.None)]
        [TestCase(HeatFluxModelType.TransportOnly)]
        [TestCase(HeatFluxModelType.ExcessTemperature)]
        [TestCase(HeatFluxModelType.Composite)]
        public void Validate_ValidModel_ReturnsEmptyValidationReport(HeatFluxModelType heatFluxModelType)
        {
            // Setup
            using (var model = new WaterFlowFMModel())
            {
                model.Grid = UnstructuredGridTestHelper.GenerateRegularGrid(3, 3, 1, 1);

                model.ModelDefinition.HeatFluxModel.Type = heatFluxModelType;
                SetValues(model.ModelDefinition.HeatFluxModel.MeteoData, heatFluxModelType);
                SetValues(model.SpatialData.Roughness);
                SetValues(model.SpatialData.Viscosity);
                SetValues(model.SpatialData.Diffusivity);

                // Call
                ValidationReport report = WaterFlowFMProcessesValidator.Validate(model);

                // Assert
                Assert.That(report.Category, Is.EqualTo("Physical Processes"));
                Assert.That(report.Issues, Is.Empty);
            }
        }

        [TestCaseSource(nameof(ValidateCoverageCases))]
        public void Validate_CoverageContainsNoDataValue_ReturnsValidationReportWithCorrectIssue(Func<ISpatialData, IFunction> getCoverage)
        {
            // Setup
            using (var model = new WaterFlowFMModel())
            {
                model.Grid = UnstructuredGridTestHelper.GenerateRegularGrid(3, 3, 1, 1);

                var heatFluxModelType = random.NextEnumValue<HeatFluxModelType>();
                model.ModelDefinition.HeatFluxModel.Type = heatFluxModelType;
                SetValues(model.ModelDefinition.HeatFluxModel.MeteoData, heatFluxModelType);
                SetValues(model.SpatialData.Roughness);
                SetValues(model.SpatialData.Viscosity);
                SetValues(model.SpatialData.Diffusivity);

                IFunction coverage = getCoverage(model.SpatialData);

                coverage.Components[0].Values[6] = -999d;

                // Call
                ValidationReport report = WaterFlowFMProcessesValidator.Validate(model);

                // Assert
                Assert.That(report.Category, Is.EqualTo("Physical Processes"));
                Assert.That(report.Issues, Has.Count.EqualTo(1));
                Assert.That(report.InfoCount, Is.EqualTo(1));

                ValidationIssue issue = report.Issues.Single();
                Assert.That(issue.ViewData, Is.SameAs(model));
                Assert.That(issue.Subject, Is.SameAs(model));
                Assert.That(issue.Severity, Is.EqualTo(ValidationSeverity.Info));
                Assert.That(issue.Message, Is.EqualTo($"{coverage.Name} contains unspecified points, the calculation kernel will replace these with default values"));
            }
        }

        private static IEnumerable<TestCaseData> ValidateCoverageCases()
        {
            yield return new TestCaseData((Func<ISpatialData, IFunction>) (data => data.Roughness));
            yield return new TestCaseData((Func<ISpatialData, IFunction>) (data => data.Viscosity));
            yield return new TestCaseData((Func<ISpatialData, IFunction>) (data => data.Diffusivity));
        }

        private static void SetValues(IFunction function)
        {
            function.SetValues(Enumerable.Repeat(7d, function.GetValues().Count));
        }

        private static void SetValues(IFunction function, HeatFluxModelType heatFluxModelType)
        {
            if (heatFluxModelType != HeatFluxModelType.Composite)
            {
                return;
            }

            FunctionHelper.SetValuesRaw(function.Components[0], Enumerable.Repeat(7d, 3));
        }
    }
}