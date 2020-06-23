using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;
using SharpMapTestUtils;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation
{
    public class WaterFlowFMModelComputationalGridValidatorTest
    {
        [Test]
        public void ValidateAllCasesForComputationalGridFromWaterFlowFMModel1D2DW()
        {
            var model = new WaterFlowFMModel();
            string expectedErrorMessage = Resources.WaterFlowFMModelComputationalGridValidator_Validate_No_computational_grid_defined_;
            /* As a start, the validation should fail*/
            var computationalGridErrors = ValidateComputationalGridAndGetErrorsForSubject(model, model.NetworkDiscretization);
            Assert.AreEqual(1, computationalGridErrors.Count);
            var errorFound = computationalGridErrors.First();

            Assert.AreEqual(ValidationSeverity.Error, errorFound.Severity);
            Assert.AreEqual(expectedErrorMessage, errorFound.Message);

            /* First, we are going to define a Grid (2D), once its created the model should be valid. */
            model.Grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2);
            computationalGridErrors = ValidateComputationalGridAndGetErrorsForSubject(model, model.NetworkDiscretization);
            Assert.AreEqual(0, computationalGridErrors.Count);

            /* Second, remove the Grid, the same previous error should be displayed again.*/
            model.Grid = null;
            computationalGridErrors = ValidateComputationalGridAndGetErrorsForSubject(model, model.NetworkDiscretization);
            Assert.AreEqual(1, computationalGridErrors.Count);
            errorFound = computationalGridErrors.First();
            Assert.AreEqual(ValidationSeverity.Error, errorFound.Severity);
            Assert.AreEqual(expectedErrorMessage, errorFound.Message);

            /* Third, let's validate the model if there is a 1D Network. */
            WaterFlowFMTestHelper.ConfigureDemoNetwork(model.Network);
            var firstChannel = model.Network.Channels.First();
            model.NetworkDiscretization[new NetworkLocation(firstChannel, 0.2)] = 0.0;
            computationalGridErrors = ValidateComputationalGridAndGetErrorsForSubject(model, model.NetworkDiscretization);
            Assert.AreEqual(0, computationalGridErrors.Count);

            /* Fourth, and last, validate with both a 1D network and a 2D Grid. */
            model.Grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2);
            computationalGridErrors = ValidateComputationalGridAndGetErrorsForSubject(model, model.NetworkDiscretization);
            Assert.AreEqual(0, computationalGridErrors.Count);
        }

        private static List<ValidationIssue> ValidateComputationalGridAndGetErrorsForSubject(WaterFlowFMModel model, object issueSubject)
        {
            var reportErrors = WaterFlowFMModelComputationalGridValidator.Validate(model.NetworkDiscretization, model);
            var computationalGridErrors = reportErrors.GetAllIssuesRecursive().Where(i => i.Subject.Equals(issueSubject)).ToList();
            return computationalGridErrors;
        }

        [Test]
        public void ValidateComputationalGridFromWaterFlowFMModel1D2DWithout1DNetworkOr2DGrid()
        {
            var model = new WaterFlowFMModel();
            string expectedErrorMessage = Resources.WaterFlowFMModelComputationalGridValidator_Validate_No_computational_grid_defined_;

            var computationalGridErrors = ValidateComputationalGridAndGetErrorsForSubject(model, model.NetworkDiscretization);
            Assert.AreEqual(1, computationalGridErrors.Count);
            var errorFound = computationalGridErrors.First();

            Assert.AreEqual(ValidationSeverity.Error, errorFound.Severity);
            Assert.AreEqual(expectedErrorMessage, errorFound.Message);
        }

        [Test]
        public void ValidateComputationalGridFromWaterFlowFMModel1D2DWith1DNetworkAndNo2DGrid()
        {
            var model = new WaterFlowFMModel();

            WaterFlowFMTestHelper.ConfigureDemoNetwork(model.Network);
            var firstChannel = model.Network.Channels.First();
            model.NetworkDiscretization[new NetworkLocation(firstChannel, 0.2)] = 0.0;
            var computationalGridErrors = ValidateComputationalGridAndGetErrorsForSubject(model, model.NetworkDiscretization);
            Assert.AreEqual(0, computationalGridErrors.Count);
        }

        [Test]
        public void ValidateComputationalGridFromWaterFlowFMModel1D2DWith2DGridAndNo1DNetwork()
        {
            var model = new WaterFlowFMModel();

            model.Grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2);
            var computationalGridErrors = ValidateComputationalGridAndGetErrorsForSubject(model, model.NetworkDiscretization);
            Assert.AreEqual(0, computationalGridErrors.Count);
        }

        [Test]
        [Category("Quarantine")]
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
        public void ValidateComputationalGridFailsWhenStructureOnDiscretizationPoint()
        {
            /* Checking the following condition is covered: 
             * branchStructures.FirstOrDefault(bs => branchLocations.Any(bl => Math.Abs(bl.Chainage - bs.Chainage) < BranchFeature.Epsilon));
             */
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

            var firstStructure = HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(new Weir { Chainage = 15.0 }, firstChannel);
            var secondStructure = HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(new Weir { Chainage = 16.0 }, firstChannel);
            Assert.NotNull(firstStructure);
            Assert.NotNull(secondStructure);

            var report = WaterFlowFMModelComputationalGridValidator.Validate(model.NetworkDiscretization);

            Assert.AreEqual(ValidationSeverity.Error, report.Severity());
            /* We retreive the composite structures the same way they get added by the hydroNetworkHelper*/
            
            Assert.IsTrue( report.AllErrors.Any( 
                e => e.Message.Equals(
                    string.Format(Resources.WaterFlowFMModelComputationalGridValidator_CheckBranchStructureLocations_No_grid_points_defined_between_structure__0__and__1_,
                        firstStructure.Name, secondStructure.Name))));
        }

        [Test]
        public void ValidateDiscretizationWithAnEdgeLessThanDxmin1D()
        {
            var model = WaterFlowFMTestHelper.CreateModelWithDemoNetwork();
            var firstChannel = model.Network.Channels.First();
            var dxmin1D = Convert.ToDouble(model.ModelDefinition.GetModelProperty("Dxmin1D").GetValueAsString());
            var networkLocation = new NetworkLocation(firstChannel, dxmin1D);
            model.NetworkDiscretization[networkLocation] = 11.0;
            
            Assert.IsNotNull(model.NetworkDiscretization.Locations.Values.FirstOrDefault(l => l.Branch == firstChannel && Math.Abs(l.Chainage) < double.Epsilon));
            Assert.IsNotNull(model.NetworkDiscretization.Locations.Values.FirstOrDefault(l => l.Branch == firstChannel && Math.Abs(l.Chainage - dxmin1D) < double.Epsilon));
            var report = WaterFlowFMModelComputationalGridValidator.Validate(model.NetworkDiscretization, model);

            Assert.That(report.AllErrors.Any(), Is.False);
            Assert.That(report.AllErrors.Count(), Is.EqualTo(0));
            Assert.That(report.Severity(), Is.EqualTo(ValidationSeverity.None));

            var offsetLessThanDxmin1D = dxmin1D - dxmin1D / 10;
            networkLocation = new NetworkLocation(firstChannel, dxmin1D + offsetLessThanDxmin1D);
            model.NetworkDiscretization[networkLocation] = 22.0;
            Assert.IsNotNull(model.NetworkDiscretization.Locations.Values.FirstOrDefault(l => l.Branch == firstChannel && Math.Abs(l.Chainage - (dxmin1D + offsetLessThanDxmin1D)) < double.Epsilon));
            report = WaterFlowFMModelComputationalGridValidator.Validate(model.NetworkDiscretization, model);

            Assert.That(report.AllErrors.Any(), Is.True);
            Assert.That(report.AllErrors.Count(), Is.EqualTo(1));
            Assert.That(report.Severity(), Is.EqualTo(ValidationSeverity.Error));

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

            Assert.IsFalse(report.Issues.Any());
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
            Assert.IsFalse(report.Issues.Any());
        }
    }
}