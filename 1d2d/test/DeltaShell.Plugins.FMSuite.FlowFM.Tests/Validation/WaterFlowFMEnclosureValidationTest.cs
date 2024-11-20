using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Validators;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using NUnit.Framework;
using SharpMapTestUtils;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation
{
    [TestFixture]
    public class WaterFlowFMEnclosureValidationTest
    {
        private WaterFlowFMModel flowFmModel;
        private GroupableFeature2DPolygon validEnclosureFeature;
        
        [SetUp]
        public void Setup()
        {
            flowFmModel = new WaterFlowFMModel();
            flowFmModel.Grid = UnstructuredGridTestHelper.GenerateRegularGrid(10, 10, 10, 10);
            validEnclosureFeature = FlowFMTestHelper.CreateFeature2DPolygonFromGeometry(
                                    "Enclosure01", 
                                    FlowFMTestHelper.GetValidGeometryForEnclosureExample());
            flowFmModel.Area.Enclosures.Add(validEnclosureFeature);
        }

        [Test]
        public void EnclosureValidationIsIncludedInWaterFlowFmValidationTest()
        {
            var report = flowFmModel.Validate();
            var enclosureSubReport = report.SubReports.FirstOrDefault(sr => sr.Category.Equals("Enclosure"));
            Assert.NotNull(enclosureSubReport);
            Assert.AreEqual(0, enclosureSubReport.Issues.Count());
        }

        [Test]
        public void OnlyOneEnclosurePerModelValidationTest()
        {
            var validationReport = WaterFlowFMEnclosureValidator.Validate(flowFmModel);
            Assert.AreEqual(0, validationReport.ErrorCount);

            var enclosures = flowFmModel.Area.Enclosures;
            enclosures.Add(FlowFMTestHelper.CreateFeature2DPolygonFromGeometry(
                            "Enclosure02", 
                            FlowFMTestHelper.GetValidGeometryForEnclosureExample()));
            validationReport = WaterFlowFMEnclosureValidator.Validate(flowFmModel);
            Assert.AreEqual(1, validationReport.ErrorCount);

            var errorFound = validationReport.AllErrors.FirstOrDefault( e => e.Subject.Equals(flowFmModel.Area.Enclosures));
            Assert.NotNull(errorFound);

            var enclosuresNames = String.Join(", ", enclosures.Select(e => e.Name));
            var expectedErrorMessage = string.Format(
                Resources
                    .WaterFlowFMEnclosureValidator_Validate_Only_one_enclosure_per_model_is_allowed__Enclosures_in_model___0_,
                enclosuresNames);

            Assert.AreEqual(expectedErrorMessage, errorFound.Message);
            Assert.AreEqual(enclosures, errorFound.ViewData);
        }

        [Test]
        public void ValidEnclosurePolygonPassesValidationTest()
        {
            var validationReport = WaterFlowFMEnclosureValidator.Validate(flowFmModel);
            Assert.AreEqual(0, validationReport.ErrorCount);
            Assert.AreEqual(1, flowFmModel.Area.Enclosures.Count);

            validationReport = WaterFlowFMEnclosureValidator.Validate(flowFmModel);
            Assert.AreEqual(1, flowFmModel.Area.Enclosures.Count); //Ensure it has been added to the model.

            Assert.AreEqual(validEnclosureFeature.Geometry, flowFmModel.Area.Enclosures[0].Geometry); //Ensure it has been added to the model.
            Assert.AreEqual(0, validationReport.ErrorCount);
        }

        [Test]
        public void InvalidEnclosureGeometryTypeFailsValidationTest()
        {
            var enclosures = flowFmModel.Area.Enclosures;
            //Remove valid enclosures from setup
            enclosures.Clear();
            Assert.AreEqual(0, flowFmModel.Area.Enclosures.Count);
            Assert.IsFalse(flowFmModel.Grid.IsEmpty);

            var featureName = "Enclosure02";
            var invalidEnclosureFeature = FlowFMTestHelper.CreateFeature2DPolygonFromGeometry(
                            featureName, FlowFMTestHelper.GetJustLinearRing());
            enclosures.Add(invalidEnclosureFeature);

            var validationReport = WaterFlowFMEnclosureValidator.Validate(flowFmModel);
            Assert.AreEqual(1, validationReport.ErrorCount);

            var errorFound = validationReport.AllErrors.FirstOrDefault(e => e.Subject.Equals(invalidEnclosureFeature));
            Assert.NotNull(errorFound);

            var expectedErrorMessage = string.Format(
                Resources.WaterFlowFMEnclosureValidator_Validate_GeometryNotValid,
                featureName);

            Assert.AreEqual(expectedErrorMessage, errorFound.Message);
            var validatedFeatures = errorFound.ViewData as ValidatedFeatures;
            Assert.That(validatedFeatures, Is.Not.Null);
            Assert.That(validatedFeatures.Features, Does.Contain(invalidEnclosureFeature));
        }

        [Test]
        public void InvalidEnclosurePolygonFailsValidationTest()
        {
            var enclosures = flowFmModel.Area.Enclosures;
            //Remove valid enclosures from setup
            enclosures.Clear();
            Assert.AreEqual(0, flowFmModel.Area.Enclosures.Count);
            Assert.IsFalse(flowFmModel.Grid.IsEmpty);

            var featureName = "Enclosure02";
            var invalidEnclosureFeature = FlowFMTestHelper.CreateFeature2DPolygonFromGeometry(
                            featureName,
                            FlowFMTestHelper.GetInvalidGeometryForEnclosureExample());
            enclosures.Add(invalidEnclosureFeature);

            var validationReport = WaterFlowFMEnclosureValidator.Validate(flowFmModel);
            Assert.AreEqual(1, validationReport.ErrorCount);

            var errorFound = validationReport.AllErrors.FirstOrDefault(e => e.Subject.Equals(invalidEnclosureFeature));
            Assert.NotNull(errorFound);

            var expectedErrorMessage = string.Format(
                Resources.WaterFlowFMEnclosureValidator_Validate_Drawn_polygon_not__0__not_valid,
                featureName);

            Assert.AreEqual(expectedErrorMessage, errorFound.Message);
            var validatedFeatures = errorFound.ViewData as ValidatedFeatures;
            Assert.That(validatedFeatures, Is.Not.Null);
            Assert.That(validatedFeatures.Features, Does.Contain(invalidEnclosureFeature));
        }
    }
}