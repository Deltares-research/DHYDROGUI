using System.Collections.Generic;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation
{
    [TestFixture]
    public class WaterFlowFMMeteoValidationTest
    {
        [Test]
        public void
            Given_WaterFlowModelWithFmMeteoField_When_AddingTwoGlobalPrecipitationSeries_Then_AValidationIssueIsThrown()
        {
            var model = new WaterFlowFMModel();
            var meteoData = FmMeteoField.CreateMeteoPrecipitationSeries(FmMeteoLocationType.Global);
            model.ModelDefinition.FmMeteoFields.Add(meteoData);
            model.ModelDefinition.FmMeteoFields.Add(meteoData);
            var issues = new List<ValidationIssue>();
            WaterFlowFMMeteoValidation.ValidateFmMeteoQuantitiesCanHaveOnlyOneGlobalLocationType(
                model.ModelDefinition.FmMeteoFields, issues);

            Assert.That(issues.Count, Is.EqualTo(1));
        }

        [Test]
        public void
            Given_WaterFlowModelWithFmMeteoField_When_AddingOneGlobalPrecipitationSeries_Then_NoValidationIssueAreThrown()
        {
            var model = new WaterFlowFMModel();
            var meteoData = FmMeteoField.CreateMeteoPrecipitationSeries(FmMeteoLocationType.Global);
            model.ModelDefinition.FmMeteoFields.Add(meteoData);
            var issues = new List<ValidationIssue>();
            WaterFlowFMMeteoValidation.ValidateFmMeteoQuantitiesCanHaveOnlyOneGlobalLocationType(
                model.ModelDefinition.FmMeteoFields, issues);

            Assert.That(issues.Count, Is.EqualTo(0));
        }

        [TestCase(FmMeteoLocationType.Feature)]
        [TestCase(FmMeteoLocationType.Grid)]
        [TestCase(FmMeteoLocationType.Polygon)]
        [Test]
        public void
            Given_WaterFlowModelWithFmMeteoField_When_AddingOneNonGlobalPrecipitationSeries_Then_AValidationIssueIsThrown(FmMeteoLocationType fmMeteoLocationType)
        {
            var model = new WaterFlowFMModel();

            var meteoData = FmMeteoField.CreateMeteoPrecipitationSeries(fmMeteoLocationType);
            model.ModelDefinition.FmMeteoFields.Add(meteoData);
            var issues = new List<ValidationIssue>();
            WaterFlowFMMeteoValidation.ValidateFmMeteoLocationTypes(model, issues);

            Assert.That(issues.Count, Is.EqualTo(1));
        }

    }
}

