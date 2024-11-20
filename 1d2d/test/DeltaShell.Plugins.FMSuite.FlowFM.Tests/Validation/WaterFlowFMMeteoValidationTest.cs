using System.Linq;
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
            Given_WaterFlowModelWithFmMeteoField_When_AddingTwoGlobalPrecipitationSeries_Then_OneValidationIssueIsThrown()
        {
            var model = new WaterFlowFMModel();
            var meteoData = FmMeteoField.CreateMeteoPrecipitationSeries(FmMeteoLocationType.Global);
            model.ModelDefinition.FmMeteoFields.Add(meteoData);
            model.ModelDefinition.FmMeteoFields.Add(meteoData);
            var validationReport = WaterFlowFMMeteoValidation.Validate(model);
            Assert.IsNotNull(validationReport);

            Assert.IsTrue(validationReport.Issues.FirstOrDefault().ToString().Contains("[Error] Meteo: There is more than one global Precipitation present, only Precipication rainfall (Global) will be used in the calculation"));
            Assert.That(validationReport.ErrorCount, Is.EqualTo(1));
        }

        [Test]
        public void
            Given_WaterFlowModelWithFmMeteoField_When_AddingOneGlobalPrecipitationSeries_Then_NoValidationIssueAreThrown()
        {
            var model = new WaterFlowFMModel();
            var meteoData = FmMeteoField.CreateMeteoPrecipitationSeries(FmMeteoLocationType.Global);
            model.ModelDefinition.FmMeteoFields.Add(meteoData);
            var validationReport = WaterFlowFMMeteoValidation.Validate(model);
            Assert.IsNotNull(validationReport);

            Assert.That(validationReport.ErrorCount, Is.EqualTo(0));
        }

        [TestCase(FmMeteoLocationType.Feature)]
        [TestCase(FmMeteoLocationType.Grid)]
        [TestCase(FmMeteoLocationType.Polygon)]
        [Test]
        public void
            Given_WaterFlowModelWithFmMeteoField_When_AddingOneNonGlobalPrecipitationSeries_Then_OneValidationIssueIsThrown(FmMeteoLocationType fmMeteoLocationType)
        {
            var model = new WaterFlowFMModel();

            var meteoData = FmMeteoField.CreateMeteoPrecipitationSeries(fmMeteoLocationType);
            model.ModelDefinition.FmMeteoFields.Add(meteoData);
            var validationReport = WaterFlowFMMeteoValidation.Validate(model);
            Assert.IsNotNull(validationReport);

            Assert.IsTrue(validationReport.Issues.FirstOrDefault().ToString().Contains("[Error] Meteo: Meteo location types: feature, grid & polygon are not yet supported"));
            Assert.That(validationReport.ErrorCount, Is.EqualTo(1));
        }
    }
}

