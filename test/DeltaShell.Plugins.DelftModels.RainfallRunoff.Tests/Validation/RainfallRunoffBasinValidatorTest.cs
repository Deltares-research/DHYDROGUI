using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Validation;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Validation
{
    [TestFixture]
    public class RainfallRunoffBasinValidatorTest
    {
        [Test]
        public void ValidateBasin()
        {
            var rrm = new RainfallRunoffModel();
            var basin = rrm.Basin;

            var catchment = new Catchment();
            basin.Catchments.Add(catchment);
            var wwtp = new WasteWaterTreatmentPlant();
            basin.WasteWaterTreatmentPlants.Add(wwtp);

            var report = new RainfallRunoffBasinValidator().Validate(rrm, basin);

            var errors = report.Issues.Where(i => i.Severity == ValidationSeverity.Error);
            var catchmentErrors = errors.Where(i => ReferenceEquals(i.Subject, catchment));
            var wwtpErrors = errors.Where(i => ReferenceEquals(i.Subject, wwtp));

            var warnings = report.Issues.Where(i => i.Severity == ValidationSeverity.Warning);
            var catchmentWarnings = warnings.Where(i => ReferenceEquals(i.Subject, catchment));
            var wwtpWarnings = warnings.Where(i => ReferenceEquals(i.Subject, wwtp));

            Assert.AreEqual(ValidationSeverity.Error, report.Severity());
            Assert.GreaterOrEqual(report.ErrorCount, 1);
            Assert.GreaterOrEqual(report.WarningCount, 0);
            Assert.GreaterOrEqual(catchmentErrors.Count(), 0);
            Assert.GreaterOrEqual(catchmentWarnings.Count(), 0);
            Assert.GreaterOrEqual(wwtpErrors.Count(),1);
            Assert.GreaterOrEqual(wwtpWarnings.Count(), 0);
        }

        [Test]
        public void ValidateBasinWithDuplicateIds()
        {
            var rrm = new RainfallRunoffModel();
            var basin = rrm.Basin;

            basin.Catchments.Add(new Catchment {Name = "c1"});

            var catchment = new Catchment {Name = "c2"};
            basin.Catchments.Add(catchment);

            catchment.Name = "c1";

            var report = new RainfallRunoffBasinValidator().Validate(rrm, basin);

            Assert.AreEqual(ValidationSeverity.Error, report.Severity());
        }

        [Test]
        public void ValidateRegionWithBasinWithDuplicateLinkIds()
        {
            var rrm = new RainfallRunoffModel();
            var basin = rrm.Basin;
            
            var node1 = new HydroNode();
            var node2 = new HydroNode();
            var branch = new Channel {Source = node1, Target = node2};
            var lateral = new LateralSource() {Branch = branch};
            var network = new HydroNetwork {Branches = {branch}, Nodes = {node1, node2}};

            var region = new HydroRegion() {SubRegions = {basin, network}};

            var c1 = new Catchment {Name = "c1"};
            var c2 = new Catchment {Name = "c2"};
            basin.Catchments.Add(c1);
            basin.Catchments.Add(c2);

            var link1 = c1.LinkTo(lateral);
            var link2 = c2.LinkTo(c1);

            link1.Name = "link1";
            link2.Name = "link1";

            var report = new RainfallRunoffBasinValidator().Validate(rrm, basin);

            var errorIssues = report.GetAllIssuesRecursive().Where(i => i.Severity == ValidationSeverity.Error);

            Assert.AreEqual(1, errorIssues.Count());
            Assert.AreEqual("[Error] link1 (c2 -> c1): Several links with the same id exist", errorIssues.First().ToString());
        }
    }
}