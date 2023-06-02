using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation
{
    [TestFixture]
    public class WaterFlowFMArea2DValidatorTest
    {
        [Test]
        [TestCaseSource(nameof(CreateModelWithInvalidFeatureNames))]
        public void Validate_ModelWithInvalidFeatureNames_ReturnsValidationError(WaterFlowFMModel model, string expectedMessage)
        {
            ValidationReport report = WaterFlowFMArea2DValidator.Validate(model);

            Assert.IsTrue(ContainsValidationMessage(report, expectedMessage),
                          $"Validation report missing expected message: '{expectedMessage}'. " +
                          $"Validation errors: '{GetValidationMessages(report)}'.");
        }

        private static IEnumerable<TestCaseData> CreateModelWithInvalidFeatureNames()
        {
            yield return CreateModelWithDuplicateEmbankmentNames();
            yield return CreateModelWithDuplicateBridgePillarsNames();
            yield return CreateModelWithDuplicateDryAreaNames();
            yield return CreateModelWithDuplicateFixedWeirNames();
            yield return CreateModelWithDuplicateGateNames();
            yield return CreateModelWithDuplicateGullyNames();
            yield return CreateModelWithDuplicateLandBoundaryNames();
            yield return CreateModelWithDuplicateLeveeBreachNames();
            yield return CreateModelWithDuplicateObservationPointNames();
            yield return CreateModelWithDuplicateObservationCrossSectionNames();
            yield return CreateModelWithDuplicatePumpNames();
            yield return CreateModelWithDuplicateRoofAreaNames();
            yield return CreateModelWithDuplicateThinDamNames();
            yield return CreateModelWithDuplicateWeirNames();

            yield return CreateModelWithEmptyEmbankmentName();
            yield return CreateModelWithEmptyBridgePillarName();
            yield return CreateModelWithEmptyDryAreaName();
            yield return CreateModelWithEmptyFixedWeirName();
            yield return CreateModelWithEmptyGateName();
            yield return CreateModelWithEmptyGullyName();
            yield return CreateModelWithEmptyLandBoundaryName();
            yield return CreateModelWithEmptyLeveeBreachName();
            yield return CreateModelWithEmptyObservationPointName();
            yield return CreateModelWithEmptyObservationCrossSectionName();
            yield return CreateModelWithEmptyPumpName();
            yield return CreateModelWithEmptyRoofAreaName();
            yield return CreateModelWithEmptyThinDamName();
            yield return CreateModelWithEmptyWeirName();
        }

        private static TestCaseData CreateModelWithDuplicateEmbankmentNames()
        {
            var model = new WaterFlowFMModel();

            model.Area.Embankments.Add(new Embankment { Name = nameof(Embankment) });
            model.Area.Embankments.Add(new Embankment { Name = nameof(Embankment) });

            const string expectedMessage = "Several embankments (2D) with the same id exist";

            return new TestCaseData(model, expectedMessage);
        }

        private static TestCaseData CreateModelWithDuplicateBridgePillarsNames()
        {
            var model = new WaterFlowFMModel();

            model.Area.BridgePillars.Add(new BridgePillar { Name = nameof(BridgePillar) });
            model.Area.BridgePillars.Add(new BridgePillar { Name = nameof(BridgePillar) });

            const string expectedMessage = "Several bridge pillars (2D) with the same id exist";

            return new TestCaseData(model, expectedMessage);
        }

        private static TestCaseData CreateModelWithDuplicateDryAreaNames()
        {
            var model = new WaterFlowFMModel();

            model.Area.DryAreas.Add(new GroupableFeature2DPolygon { Name = nameof(GroupableFeature2DPolygon) });
            model.Area.DryAreas.Add(new GroupableFeature2DPolygon { Name = nameof(GroupableFeature2DPolygon) });

            const string expectedMessage = "Several dry areas (2D) with the same id exist";

            return new TestCaseData(model, expectedMessage);
        }

        private static TestCaseData CreateModelWithDuplicateFixedWeirNames()
        {
            var model = new WaterFlowFMModel();

            model.Area.FixedWeirs.Add(new FixedWeir { Name = nameof(FixedWeir) });
            model.Area.FixedWeirs.Add(new FixedWeir { Name = nameof(FixedWeir) });

            const string expectedMessage = "Several fixed weirs (2D) with the same id exist";

            return new TestCaseData(model, expectedMessage);
        }

        private static TestCaseData CreateModelWithDuplicateGateNames()
        {
            var model = new WaterFlowFMModel();

            model.Area.Gates.Add(new Gate2D { Name = nameof(Gate2D) });
            model.Area.Gates.Add(new Gate2D { Name = nameof(Gate2D) });

            const string expectedMessage = "Several gates (2D) with the same id exist";

            return new TestCaseData(model, expectedMessage);
        }

        private static TestCaseData CreateModelWithDuplicateGullyNames()
        {
            var model = new WaterFlowFMModel();

            model.Area.Gullies.Add(new Gully { Name = nameof(Gully) });
            model.Area.Gullies.Add(new Gully { Name = nameof(Gully) });

            const string expectedMessage = "Several gullies (2D) with the same id exist";

            return new TestCaseData(model, expectedMessage);
        }

        private static TestCaseData CreateModelWithDuplicateLandBoundaryNames()
        {
            var model = new WaterFlowFMModel();

            model.Area.LandBoundaries.Add(new LandBoundary2D { Name = nameof(LandBoundary2D) });
            model.Area.LandBoundaries.Add(new LandBoundary2D { Name = nameof(LandBoundary2D) });

            const string expectedMessage = "Several land boundaries (2D) with the same id exist";

            return new TestCaseData(model, expectedMessage);
        }

        private static TestCaseData CreateModelWithDuplicateLeveeBreachNames()
        {
            var model = new WaterFlowFMModel();

            model.Area.LeveeBreaches.Add(new LeveeBreach { Name = nameof(LeveeBreach) });
            model.Area.LeveeBreaches.Add(new LeveeBreach { Name = nameof(LeveeBreach) });

            const string expectedMessage = "Several levee breaches (2D) with the same id exist";

            return new TestCaseData(model, expectedMessage);
        }

        private static TestCaseData CreateModelWithDuplicateObservationPointNames()
        {
            var model = new WaterFlowFMModel();

            model.Area.ObservationPoints.Add(new GroupableFeature2DPoint { Name = nameof(GroupableFeature2DPoint) });
            model.Area.ObservationPoints.Add(new GroupableFeature2DPoint { Name = nameof(GroupableFeature2DPoint) });

            const string expectedMessage = "Several observation points (2D) with the same id exist";

            return new TestCaseData(model, expectedMessage);
        }

        private static TestCaseData CreateModelWithDuplicateObservationCrossSectionNames()
        {
            var model = new WaterFlowFMModel();

            model.Area.ObservationCrossSections.Add(new ObservationCrossSection2D { Name = nameof(ObservationCrossSection2D) });
            model.Area.ObservationCrossSections.Add(new ObservationCrossSection2D { Name = nameof(ObservationCrossSection2D) });

            const string expectedMessage = "Several observation cross-sections (2D) with the same id exist";

            return new TestCaseData(model, expectedMessage);
        }

        private static TestCaseData CreateModelWithDuplicatePumpNames()
        {
            var model = new WaterFlowFMModel();

            model.Area.Pumps.Add(new Pump2D { Name = nameof(Pump2D) });
            model.Area.Pumps.Add(new Pump2D { Name = nameof(Pump2D) });

            const string expectedMessage = "Several pumps (2D) with the same id exist";

            return new TestCaseData(model, expectedMessage);
        }

        private static TestCaseData CreateModelWithDuplicateRoofAreaNames()
        {
            var model = new WaterFlowFMModel();

            model.Area.RoofAreas.Add(new GroupableFeature2DPolygon { Name = nameof(GroupableFeature2DPolygon) });
            model.Area.RoofAreas.Add(new GroupableFeature2DPolygon { Name = nameof(GroupableFeature2DPolygon) });

            const string expectedMessage = "Several roof areas (2D) with the same id exist";

            return new TestCaseData(model, expectedMessage);
        }

        private static TestCaseData CreateModelWithDuplicateThinDamNames()
        {
            var model = new WaterFlowFMModel();

            const string nonUniqueName = "NotUnique";
            model.Area.ThinDams.Add(new ThinDam2D { Name = nonUniqueName });
            model.Area.ThinDams.Add(new ThinDam2D { Name = nonUniqueName });

            const string expectedMessage = "Several thin dams (2D) with the same id exist";

            return new TestCaseData(model, expectedMessage);
        }

        private static TestCaseData CreateModelWithDuplicateWeirNames()
        {
            var model = new WaterFlowFMModel();

            model.Area.Weirs.Add(new Weir2D { Name = nameof(Weir2D) });
            model.Area.Weirs.Add(new Weir2D { Name = nameof(Weir2D) });

            const string expectedMessage = "Several weirs (2D) with the same id exist";

            return new TestCaseData(model, expectedMessage);
        }

        private static TestCaseData CreateModelWithEmptyEmbankmentName()
        {
            var model = new WaterFlowFMModel();

            model.Area.Embankments.Add(new Embankment());
            model.Area.Embankments[0].Name = string.Empty;

            const string expectedMessage = "Name of a embankment (2D) is not set (it is empty or consists of only white-space characters).";

            return new TestCaseData(model, expectedMessage);
        }

        private static TestCaseData CreateModelWithEmptyBridgePillarName()
        {
            var model = new WaterFlowFMModel();

            model.Area.BridgePillars.Add(new BridgePillar());
            model.Area.BridgePillars[0].Name = string.Empty;

            const string expectedMessage = "Name of a bridge pillar (2D) is not set (it is empty or consists of only white-space characters).";

            return new TestCaseData(model, expectedMessage);
        }

        private static TestCaseData CreateModelWithEmptyDryAreaName()
        {
            var model = new WaterFlowFMModel();

            model.Area.DryAreas.Add(new GroupableFeature2DPolygon());
            model.Area.DryAreas[0].Name = string.Empty;

            const string expectedMessage = "Name of a dry area (2D) is not set (it is empty or consists of only white-space characters).";

            return new TestCaseData(model, expectedMessage);
        }

        private static TestCaseData CreateModelWithEmptyFixedWeirName()
        {
            var model = new WaterFlowFMModel();

            model.Area.FixedWeirs.Add(new FixedWeir());
            model.Area.FixedWeirs[0].Name = string.Empty;

            const string expectedMessage = "Name of a fixed weir (2D) is not set (it is empty or consists of only white-space characters).";

            return new TestCaseData(model, expectedMessage);
        }

        private static TestCaseData CreateModelWithEmptyGateName()
        {
            var model = new WaterFlowFMModel();

            model.Area.Gates.Add(new Gate2D());
            model.Area.Gates[0].Name = string.Empty;

            const string expectedMessage = "Name of gate (2D) ({Gate2D: <no branch>, 0})) is not set (it is empty or consists of only white-space characters).";

            return new TestCaseData(model, expectedMessage);
        }

        private static TestCaseData CreateModelWithEmptyGullyName()
        {
            var model = new WaterFlowFMModel();

            model.Area.Gullies.Add(new Gully());
            model.Area.Gullies[0].Name = string.Empty;

            const string expectedMessage = "Name of a gully (2D) is not set (it is empty or consists of only white-space characters).";

            return new TestCaseData(model, expectedMessage);
        }

        private static TestCaseData CreateModelWithEmptyLandBoundaryName()
        {
            var model = new WaterFlowFMModel();

            model.Area.LandBoundaries.Add(new LandBoundary2D());
            model.Area.LandBoundaries[0].Name = string.Empty;

            const string expectedMessage = "Name of a land boundary (2D) is not set (it is empty or consists of only white-space characters).";

            return new TestCaseData(model, expectedMessage);
        }

        private static TestCaseData CreateModelWithEmptyLeveeBreachName()
        {
            var model = new WaterFlowFMModel();

            model.Area.LeveeBreaches.Add(new LeveeBreach());
            model.Area.LeveeBreaches[0].Name = string.Empty;

            const string expectedMessage = "Name of a levee breach (2D) is not set (it is empty or consists of only white-space characters).";

            return new TestCaseData(model, expectedMessage);
        }

        private static TestCaseData CreateModelWithEmptyObservationPointName()
        {
            var model = new WaterFlowFMModel();

            model.Area.ObservationPoints.Add(new GroupableFeature2DPoint());
            model.Area.ObservationPoints[0].Name = string.Empty;

            const string expectedMessage = "Name of a observation point (2D) is not set (it is empty or consists of only white-space characters).";

            return new TestCaseData(model, expectedMessage);
        }

        private static TestCaseData CreateModelWithEmptyObservationCrossSectionName()
        {
            var model = new WaterFlowFMModel();

            model.Area.ObservationCrossSections.Add(new ObservationCrossSection2D());
            model.Area.ObservationCrossSections[0].Name = string.Empty;

            const string expectedMessage = "Name of a observation cross-section (2D) is not set (it is empty or consists of only white-space characters).";

            return new TestCaseData(model, expectedMessage);
        }

        private static TestCaseData CreateModelWithEmptyPumpName()
        {
            var model = new WaterFlowFMModel();

            model.Area.Pumps.Add(new Pump2D());
            model.Area.Pumps[0].Name = string.Empty;

            const string expectedMessage = "Name of pump (2D) ({Pump2D: <no branch>, 0})) is not set (it is empty or consists of only white-space characters).";

            return new TestCaseData(model, expectedMessage);
        }

        private static TestCaseData CreateModelWithEmptyRoofAreaName()
        {
            var model = new WaterFlowFMModel();

            model.Area.RoofAreas.Add(new GroupableFeature2DPolygon());
            model.Area.RoofAreas[0].Name = string.Empty;

            const string expectedMessage = "Name of a roof area (2D) is not set (it is empty or consists of only white-space characters).";

            return new TestCaseData(model, expectedMessage);
        }

        private static TestCaseData CreateModelWithEmptyThinDamName()
        {
            var model = new WaterFlowFMModel();

            model.Area.ThinDams.Add(new ThinDam2D());
            model.Area.ThinDams[0].Name = string.Empty;

            const string expectedMessage = "Name of a thin dam (2D) is not set (it is empty or consists of only white-space characters).";

            return new TestCaseData(model, expectedMessage);
        }

        private static TestCaseData CreateModelWithEmptyWeirName()
        {
            var model = new WaterFlowFMModel();

            model.Area.Weirs.Add(new Weir2D());
            model.Area.Weirs[0].Name = string.Empty;

            const string expectedMessage = "Name of weir (2D) ({Weir2D: <no branch>, 0})) is not set (it is empty or consists of only white-space characters).";

            return new TestCaseData(model, expectedMessage);
        }

        private static bool ContainsValidationMessage(ValidationReport report, string message)
        {
            return report.AllErrors.Any(error => error.Message.Contains(message));
        }

        private static string GetValidationMessages(ValidationReport report)
        {
            return string.Join(Environment.NewLine, report.AllErrors.Select(er => er.Message));
        }
    }
}