using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;
using SharpMapTestUtils;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation
{
    public class WaterFlowFMModelComputationalGridValidatorTest
    {
        [Test]
        public void ValidateComputationalGridFromWaterFlowFMModel1D2DValidator()
        {
            var model = new WaterFlowFMModel();
            var reportErrors = WaterFlowFMModelComputationalGridValidator.Validate(model.NetworkDiscretization, model);

            var computationalGridErrors = reportErrors.GetAllIssuesRecursive().Where(i => i.Subject.Equals(model.NetworkDiscretization)).ToList();
            Assert.AreEqual(1, computationalGridErrors.Count);

            string expectedErrorMessage = Resources.WaterFlowFMModelComputationalGridValidator_Validate_No_computational_grid_defined_;
            var errorFound = computationalGridErrors.First();

            Assert.AreEqual(ValidationSeverity.Error, errorFound.Severity);
            Assert.AreEqual(expectedErrorMessage, errorFound.Message);

            //if grid is defined then it does not fail
            model.Grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2);
            reportErrors = WaterFlowFMModelComputationalGridValidator.Validate(model.NetworkDiscretization, model);
            computationalGridErrors = reportErrors.GetAllIssuesRecursive().Where(i => i.Subject.Equals(model.NetworkDiscretization)).ToList();
            Assert.AreEqual(0, computationalGridErrors.Count);
            model.Grid = null;

            reportErrors = WaterFlowFMModelComputationalGridValidator.Validate(model.NetworkDiscretization, model);
            computationalGridErrors = reportErrors.GetAllIssuesRecursive().Where(i => i.Subject.Equals(model.NetworkDiscretization)).ToList();
            Assert.AreEqual(1, computationalGridErrors.Count);

            WaterFlowFMTestHelper.ConfigureDemoNetwork(model.Network);
            var firstChannel = model.Network.Channels.First();
            model.NetworkDiscretization[new NetworkLocation(firstChannel, 0.2)] = 0.0;
            reportErrors = WaterFlowFMModelComputationalGridValidator.Validate(model.NetworkDiscretization, model);
            computationalGridErrors = reportErrors.GetAllIssuesRecursive().Where(i => i.Subject.Equals(model.NetworkDiscretization)).ToList();
            Assert.AreEqual(0, computationalGridErrors.Count);
        }

        [Test]
        public void WaterFlowFMModel1DAllBranchesRequireDiscretizationToValidate()
        {
            var model = WaterFlowFMTestHelper.CreateModelWithDemoNetwork();
            Assert.AreEqual(2, model.Network.Branches.Count);

            //Generate discretization only for one branch
            model.NetworkDiscretization.Clear();
            var offSets = new double[] { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };
            HydroNetworkHelper.GenerateDiscretization(model.NetworkDiscretization, (IChannel)model.Network.Branches[0], offSets);

            var reportErrors = WaterFlowFMModelComputationalGridValidator.Validate(model.NetworkDiscretization, model);

            var computationalGridErrors = reportErrors.GetAllIssuesRecursive().Where(i => i.Subject.Equals(model.Network.Branches[1])).ToList();
            Assert.AreEqual(1, computationalGridErrors.Count);

            string expectedErrorMessage = string.Format(
                Resources.WaterFlowFMModelComputationalGridValidator_Validate_No_computational_grid_cells_defined_for_branch__0___can_not_start_calculation_,
                model.Network.Branches[1].Name);
            var errorFound = computationalGridErrors.First();

            Assert.AreEqual(ValidationSeverity.Error, errorFound.Severity);
            Assert.AreEqual(expectedErrorMessage, errorFound.Message);

            //Generate discretization for second branch too.
            var offsets2 = new double[] { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150 };
            HydroNetworkHelper.GenerateDiscretization(model.NetworkDiscretization, (IChannel)model.Network.Branches[1], offsets2);

            reportErrors = WaterFlowFMModelComputationalGridValidator.Validate(model.NetworkDiscretization, model);
            Assert.IsFalse(reportErrors.GetAllIssuesRecursive().Any(i => i.Subject.Equals(model.Network.Branches[1])));
        }

        [Test]
        public void ValidateComputationalGridFailsWhenStructurOnDiscretizationPoint()
        {
            //branchStructures.FirstOrDefault(bs => branchLocations.Any(bl => Math.Abs(bl.Chainage - bs.Chainage) < BranchFeature.Epsilon));
            var model = WaterFlowFMTestHelper.CreateModelWithDemoNetwork();
            Assert.IsTrue(model.Network.Branches.Any());

            var networkBranch = model.Network.Branches[0];
            var branchLocation = model.NetworkDiscretization.GetLocationsForBranch(networkBranch).First();

            // Add a structure
            var structureName = "testPump";
            var branchFeature = new Pump(structureName) {Chainage = branchLocation.Chainage};
            NetworkHelper.AddBranchFeatureToBranch(branchFeature,networkBranch);

            var reportErrors = WaterFlowFMModelComputationalGridValidator.Validate(model.NetworkDiscretization, model);
            var computationalGridErrors = reportErrors.GetAllIssuesRecursive().Where(i => i.Subject.Equals(branchFeature)).ToList();
            Assert.AreEqual(1, computationalGridErrors.Count);

            var errorFound = computationalGridErrors.First();
            var expectedErrorMessage = string.Format(Resources.WaterFlowFMModelComputationalGridValidator_FiniteVolumeCheckStructuresNotOnGridPoints_Original_discretization_is_invalid__structure__0__is_on_a_grid_point, structureName);
            Assert.AreEqual(ValidationSeverity.Error, errorFound.Severity);
            Assert.AreEqual(expectedErrorMessage, errorFound.Message);
        }

        [Test]
        public void ValidateDiscretizationWith2StructuresBetweenGridPoints()
        {
            var model = WaterFlowFMTestHelper.CreateModelWithDemoNetwork();
            var firstChannel = model.Network.Channels.First();

            HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(new Weir { Chainage = 15.0 }, firstChannel);
            HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(new Weir { Chainage = 16.0 }, firstChannel);

            var report = WaterFlowFMModelComputationalGridValidator.Validate(model.NetworkDiscretization);

            Assert.AreEqual(ValidationSeverity.Error, report.Severity());
        }

        [Test]
        public void ValidateDiscretizationWithTwoPointsDistanceLessThen0point25()
        {
            var model = WaterFlowFMTestHelper.CreateModelWithDemoNetwork();

            var firstChannel = model.Network.Channels.First();
            model.NetworkDiscretization[new NetworkLocation(firstChannel, 0.2)] = 0.0;

            Assert.IsNotNull(model.NetworkDiscretization.Locations.Values.FirstOrDefault(l => l.Branch == firstChannel && l.Chainage == 0.0));
            Assert.IsNotNull(model.NetworkDiscretization.Locations.Values.FirstOrDefault(l => l.Branch == firstChannel && l.Chainage == 0.2));

            var report = WaterFlowFMModelComputationalGridValidator.Validate(model.NetworkDiscretization);

            Assert.AreEqual(ValidationSeverity.None, report.Severity());
        }

        [Test]
        public void ValidateDiscretizationWith2StructuresInACompositeStructureBetweenGridPoints()
        {
            var model = WaterFlowFMTestHelper.CreateModelWithDemoNetwork();
            var firstChannel = model.Network.Channels.First();
            var weir1 = new Weir { Chainage = 15.0, Branch = firstChannel };
            var weir2 = new Weir { Chainage = 15.0, Branch = firstChannel };
            var compositeBranchStructure = new CompositeBranchStructure
            {
                Branch = firstChannel,
                Chainage = 15.0
            };

            firstChannel.BranchFeatures.Add(compositeBranchStructure);
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, weir1);
            HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, weir2);

            var report = WaterFlowFMModelComputationalGridValidator.Validate(model.NetworkDiscretization);
            Assert.AreNotEqual(ValidationSeverity.Error, report.Severity()); //should give no error?
        }
    }
}