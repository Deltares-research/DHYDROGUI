using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Validation;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Validation
{
    [TestFixture]
    public class PavedDataValidatorTest
    {
        [Test]
        public void Validate_NoPavedData_ReturnsEmptyValidationReport()
        {
            // Setup
            var validator = new PavedDataValidator();
            var rrModel = Substitute.For<IRainfallRunoffModel>();

            // Call
            ValidationReport report = validator.Validate(rrModel, Enumerable.Empty<PavedData>());

            // Assert
            Assert.That(report.IsEmpty);
        }

        [Test]
        public void GivenPavedDataWithoutMixedLink_WhenValidating_AddsErrorToValidationReport()
        {
            // Setup
            var rrModel = Substitute.For<IRainfallRunoffModel>();
            var basin = Substitute.For<IDrainageBasin>();

            var catchment = new Catchment() { Basin = basin };

            var pavedData = new PavedData(catchment);
            var pavedDatas = new List<PavedData> { pavedData };

            // Precondition
            Assert.That(pavedData.MixedSewerTarget, Is.Null);
            
            // Call
            var validator = new PavedDataValidator();
            ValidationReport report = validator.Validate(rrModel, pavedDatas);

            // Assert
            const string expectedMessage = "No runoff target has been defined for the paved rainfall/mixed flow, or the " +
                                           "selected runoff type does not match any of the linked features.";
            IEnumerable<ValidationIssue> errors = report.Issues.Where(i => i.Severity == ValidationSeverity.Error);
            Assert.That(errors.Count, Is.EqualTo(1));
            Assert.That(errors.First().Message, Is.EqualTo(expectedMessage));
        }
        
        [Test]
        public void GivenPavedDataWithSeparateSewerSystemAndWithoutValidDwfLink_WhenValidating_AddsErrorsToValidationReport()
        {
            // Setup
            var rrModel = Substitute.For<IRainfallRunoffModel>();

            PavedData pavedData = CreatePavedDataWithSeparateSystemAndWithoutValidDwfSewerTarget();
            var pavedDatas = new List<PavedData> { pavedData };

            // Precondition
            Assert.That(pavedData.MixedSewerTarget, Is.Not.Null);
            Assert.That(pavedData.DwfSewerTarget, Is.Null);
            
            // Call
            var validator = new PavedDataValidator();
            ValidationReport report = validator.Validate(rrModel, pavedDatas);

            // Assert
            IEnumerable<ValidationIssue> errors = report.Issues.Where(i => i.Severity == ValidationSeverity.Error);

            const string expectedMessage1 = "No runoff target has been defined for the paved dry water flow.";
            bool containsExpectedError1 = errors.Any(e => e.Message.Equals(expectedMessage1));
            Assert.That(containsExpectedError1, Is.True);

            const string expectedMessage2 = "A paved node dry water flow sewer link can only be connected (downstream) to a " +
                                            "boundary, lateral or waste water treatment plant.";
            bool containsExpectedError2 = errors.Any(e => e.Message.Equals(expectedMessage2));
            Assert.That(containsExpectedError2, Is.True);
            
        }
        
        [Test]
        public void GivenPavedDataWithMultipleLinksToDifferentWasteWaterTreatmentPlants_WhenValidating_AddsErrorToValidationReport()
        {
            // Setup
            var rrModel = Substitute.For<IRainfallRunoffModel>();

            PavedData pavedData = CreatePavedDataWithMultipleLinksToSameTargetWasteWaterTreatmentPlant();
            var pavedDatas = new List<PavedData> { pavedData };
            
            // Call
            var validator = new PavedDataValidator();
            ValidationReport report = validator.Validate(rrModel, pavedDatas);

            // Assert
            IEnumerable<ValidationIssue> errors = report.Issues.Where(i => i.Severity == ValidationSeverity.Error);

            const string expectedMessage = "A paved node may not be connected downstream to more then 1 Waste Water Treatment Plant.";
            bool containsExpectedError = errors.Any(e => e.Message.Equals(expectedMessage));
            Assert.That(containsExpectedError, Is.True);
        }
        
        [Test]
        [TestCaseSource(nameof(GetDifferentRRBoundariesTestCases))]
        public void GivenPavedDataWithMultipleLinksToDifferentRRBoundaries_WhenValidating_AddsErrorToValidationReport(
            IHydroObject firstLinkTarget,
            IHydroObject secondLinkTarget)
        {
            // Setup
            var rrModel = Substitute.For<IRainfallRunoffModel>();

            PavedData pavedData = CreatePavedDataWithMultipleLinksToLateralSources(firstLinkTarget, secondLinkTarget);
            var pavedDatas = new List<PavedData> { pavedData };
            
            // Call
            var validator = new PavedDataValidator();
            ValidationReport report = validator.Validate(rrModel, pavedDatas);

            // Assert
            IEnumerable<ValidationIssue> errors = report.Issues.Where(i => i.Severity == ValidationSeverity.Error);

            const string expectedMessage = "A paved node may not be connected downstream to more then 1 boundary.";
            bool containsExpectedError = errors.Any(e => e.Message.Equals(expectedMessage));
            Assert.That(containsExpectedError, Is.True);
        }

        private static IEnumerable<TestCaseData> GetDifferentRRBoundariesTestCases()
        {
            yield return new TestCaseData(new LateralSource(), new LateralSource())
                .SetName("Paved node with a link to two different Lateral Sources.");
            yield return new TestCaseData(new RunoffBoundary(), new RunoffBoundary())
                .SetName("Paved node with a link to two different Runoff Boundaries.");
            yield return new TestCaseData(new RunoffBoundary(), new LateralSource())
                .SetName("Paved node with a link to a Lateral Source and a Runoff Boundary.");
        }

        private static Catchment CreateCatchmentWithSubstituteBasin()
        {
            var basin = Substitute.For<IDrainageBasin>();

            var catchment = new Catchment() { Basin = basin };

            basin.When(b => b.AddNewLink(Arg.Any<IHydroObject>(), Arg.Any<IHydroObject>()))
                 .Do(callInfo =>
                 {
                     var source = callInfo.ArgAt<IHydroObject>(0);
                     var target = callInfo.ArgAt<IHydroObject>(1);

                     var sourceLink = new HydroLink(source, target);
                     catchment.Links.Add(sourceLink);
                 });

            return catchment;
        }

        private static PavedData CreatePavedDataWithMultipleLinksToSameTargetWasteWaterTreatmentPlant()
        {
            Catchment catchment = CreateCatchmentWithSubstituteBasin();
            
            var pavedData = new PavedData(catchment)
            {
                SewerType = PavedEnums.SewerType.SeparateSystem
            };

            // Add a valid link for the MixedSewerTarget
            pavedData.MixedAndOrRainfallSewerPumpDischarge = PavedEnums.SewerPumpDischargeTarget.WWTP;
            catchment.LinkTo(new WasteWaterTreatmentPlant());
            
            // Set DwfSewerTarget to
            pavedData.DryWeatherFlowSewerPumpDischarge = PavedEnums.SewerPumpDischargeTarget.WWTP;
            catchment.LinkTo(new WasteWaterTreatmentPlant()); // link to different WWTP

            return pavedData;
        }

        private static PavedData CreatePavedDataWithMultipleLinksToLateralSources(IHydroObject firstLinkTarget, 
                                                                                  IHydroObject secondLinkTarget)
        {
            Catchment catchment = CreateCatchmentWithSubstituteBasin();

            var pavedData = new PavedData(catchment)
            {
                SewerType = PavedEnums.SewerType.SeparateSystem
            };
            
            pavedData.MixedAndOrRainfallSewerPumpDischarge = PavedEnums.SewerPumpDischargeTarget.BoundaryNode;
            catchment.LinkTo(firstLinkTarget);
            
            pavedData.DryWeatherFlowSewerPumpDischarge = PavedEnums.SewerPumpDischargeTarget.BoundaryNode;
            catchment.LinkTo(secondLinkTarget); 

            return pavedData;
        }

        private static PavedData CreatePavedDataWithSeparateSystemAndWithoutValidDwfSewerTarget()
        {
            var basin = Substitute.For<IDrainageBasin>();

            var catchment = new Catchment() { Basin = basin };

            basin.When(b => b.AddNewLink(Arg.Any<IHydroObject>(), Arg.Any<IHydroObject>()))
                 .Do(callInfo =>
                 {
                     var source = callInfo.ArgAt<IHydroObject>(0);
                     var target = callInfo.ArgAt<IHydroObject>(1);

                     var sourceLink = new HydroLink(source, target);
                     catchment.Links.Add(sourceLink);
                 });
            
            var pavedData = new PavedData(catchment)
            {
                SewerType = PavedEnums.SewerType.SeparateSystem
            };

            // Add a valid link for the MixedSewerTarget
            pavedData.MixedAndOrRainfallSewerPumpDischarge = PavedEnums.SewerPumpDischargeTarget.WWTP;
            catchment.LinkTo(new WasteWaterTreatmentPlant());
            
            // Set DwfSewerTarget to Boundary, but without setting a valid target
            pavedData.DryWeatherFlowSewerPumpDischarge = PavedEnums.SewerPumpDischargeTarget.BoundaryNode;

            return pavedData;
        }
    }
}