using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation
{
    [TestFixture]
    public class WaterFlowFMProcessesValidatorTest
    {
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
                ConfigureValidModel(model);

                model.ModelDefinition.HeatFluxModel.Type = HeatFluxModelType.Composite;

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

        [Test]
        public void Validate_DefaultTracerCoverageAndBoundaryConditionsDoNotContainTracer_ReturnsValidationReportWithCorrectIssue()
        {
            // Setup
            using (var model = new WaterFlowFMModel())
            {
                ConfigureValidModel(model);

                model.TracerDefinitions.Add("Some tracer");

                // Call
                ValidationReport report = WaterFlowFMProcessesValidator.Validate(model);

                // Assert
                Assert.That(report.Category, Is.EqualTo("Physical Processes"));
                Assert.That(report.Issues, Has.Count.EqualTo(1));
                Assert.That(report.WarningCount, Is.EqualTo(1));

                ValidationIssue issue = report.Issues.Single();
                Assert.That(issue.ViewData, Is.SameAs(model));
                Assert.That(issue.Subject, Is.SameAs(model));
                Assert.That(issue.Severity, Is.EqualTo(ValidationSeverity.Warning));
                Assert.That(issue.Message, Is.EqualTo("Tracer 'Some tracer' concentration has not been set in any boundary condition nor initial field. It is now set to default value 0."));
            }
        }

        [Test]
        public void Validate_OneValidTracer_OneInvalidTracer_ReturnsValidationReportWithCorrectIssue()
        {
            // Setup
            using (var model = new WaterFlowFMModel())
            {
                ConfigureValidModel(model);

                model.TracerDefinitions.Add("Some tracer 1");
                model.TracerDefinitions.Add("Some tracer 2");
                SetValues(model.SpatialData.InitialTracers.First(), 7d);

                // Call
                ValidationReport report = WaterFlowFMProcessesValidator.Validate(model);

                // Assert
                Assert.That(report.Category, Is.EqualTo("Physical Processes"));
                Assert.That(report.Issues, Has.Count.EqualTo(1));
                Assert.That(report.WarningCount, Is.EqualTo(1));

                ValidationIssue issue = report.Issues.Single();
                Assert.That(issue.ViewData, Is.SameAs(model));
                Assert.That(issue.Subject, Is.SameAs(model));
                Assert.That(issue.Severity, Is.EqualTo(ValidationSeverity.Warning));
                Assert.That(issue.Message, Is.EqualTo("Tracer 'Some tracer 2' concentration has not been set in any boundary condition nor initial field. It is now set to default value 0."));
            }
        }

        [TestCaseSource(nameof(ValidateCoverageCases))]
        public void Validate_CoverageContainsNoDataValue_ReturnsValidationReportWithCorrectIssue(Func<ISpatialData, IFunction> getCoverage)
        {
            // Setup
            using (var model = new WaterFlowFMModel())
            {
                ConfigureValidModel(model);

                IFunction coverage = getCoverage(model.SpatialData);
                SetOneValueWith(coverage, -999d);

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

        [TestCaseSource(nameof(ConfigureValidModels))]
        public void Validate_ValidModel_ReturnsEmptyValidationReport(Action<WaterFlowFMModel> configureValidModel)
        {
            // Setup
            using (var model = new WaterFlowFMModel())
            {
                configureValidModel(model);

                // Call
                ValidationReport report = WaterFlowFMProcessesValidator.Validate(model);

                // Assert
                Assert.That(report.Category, Is.EqualTo("Physical Processes"));
                Assert.That(report.Issues, Is.Empty);
            }
        }

        private static IEnumerable<Action<WaterFlowFMModel>> ConfigureValidModels()
        {
            foreach (HeatFluxModelType heatFluxModelType in (HeatFluxModelType[]) Enum.GetValues(typeof(HeatFluxModelType)))
            {
                yield return model =>
                {
                    ConfigureValidModel(model, heatFluxModelType);

                    model.TracerDefinitions.Add("Some tracer");
                    SetValues(model.SpatialData.InitialTracers.Single(), 7d);
                };

                yield return model =>
                {
                    ConfigureValidModel(model, heatFluxModelType);

                    model.TracerDefinitions.Add("Some tracer");
                    BoundaryConditionSet boundaryConditionSet = CreateBoundaryConditionSetWithTracer("Some tracer");
                    model.BoundaryConditionSets.Add(boundaryConditionSet);
                };
            }
        }

        private static BoundaryConditionSet CreateBoundaryConditionSetWithTracer(string tracer)
        {
            var boundaryConditionSet = new BoundaryConditionSet();
            var boundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.Tracer, BoundaryConditionDataType.Empty) {TracerName = tracer};
            boundaryConditionSet.BoundaryConditions.Add(boundaryCondition);

            return boundaryConditionSet;
        }

        private static void ConfigureValidModel(IWaterFlowFMModel model, HeatFluxModelType heatFluxModelType = HeatFluxModelType.None)
        {
            SetValues(model.SpatialData.Roughness, 7d);
            SetValues(model.SpatialData.Viscosity, 7d);
            SetValues(model.SpatialData.Diffusivity, 7d);

            model.ModelDefinition.HeatFluxModel.Type = heatFluxModelType;
            SetValues(model.ModelDefinition.HeatFluxModel.MeteoData, heatFluxModelType);
        }

        private static void SetOneValueWith(IFunction function, double value)
        {
            function.Components[0].Values[3] = value;
        }

        private static void SetValues(IFunction function, double value)
        {
            FunctionHelper.SetValuesRaw(function.Components[0], Enumerable.Repeat(value, 10));
        }

        private static void SetValues(IFunction function, HeatFluxModelType heatFluxModelType)
        {
            if (heatFluxModelType != HeatFluxModelType.Composite)
            {
                return;
            }

            FunctionHelper.SetValuesRaw(function.Components[0], Enumerable.Repeat(7d, 10));
        }
    }
}