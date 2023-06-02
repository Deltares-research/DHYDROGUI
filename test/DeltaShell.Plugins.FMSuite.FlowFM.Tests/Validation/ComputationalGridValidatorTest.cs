using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;
using SharpMapTestUtils;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation
{
    public class ComputationalGridValidatorTest
    {
        [Test]
        public void ValidateValidDiscretizationTest()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(1);
            var discretization = new Discretization() { Network = network };
            var channel = network.Channels.First();
            HydroNetworkHelper.GenerateDiscretization(discretization, true, false, 100.0, false, 0.0, false, false,
                true, 10.0, new List<IChannel> { channel });
            var report = ComputationalGridValidator.Validate(discretization, null);
            Assert.That(report.Severity(), Is.EqualTo(ValidationSeverity.None));
            Assert.That(report.Issues.Count(), Is.EqualTo(0));
        }

        /// o------channel1-----o------channel2-----o------channel3-----o------channel4-----o
        /// x-x-x-x-x-x-x-x-x-x-x------------------- -------------------x-------------------x
        [Test]
        public void ValidateDiscretizationMissing1ChannelCalcPointsTest()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(4);
            var discretization = new Discretization
            {
                Network = network,
                SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocationsAndConnectedBranchesWithoutLocationOnThemFullyCovered
            };
            
            Assert.That(discretization.Segments.AllValues.Count, Is.EqualTo(4));

            var channel = network.Channels.First();
            var channel2 = network.Channels.ElementAt(1);
            var channel3 = network.Channels.ElementAt(2);
            var channel4 = network.Channels.ElementAt(3);

            var channel2Segments = discretization.Segments.AllValues.Where(s => s.Branch.Equals(channel2));
            var channel3Segments = discretization.Segments.AllValues.Where(s => s.Branch.Equals(channel3));

            discretization.Segments.RemoveValues(discretization.Segments.CreateValuesFilter(channel2Segments));
            discretization.Segments.RemoveValues(discretization.Segments.CreateValuesFilter(channel3Segments));
            
            HydroNetworkHelper.GenerateDiscretization(discretization, true, false, 100.0, false, 0.0, false, false,
                true, 10.0, new List<IChannel> { channel });

            discretization.Locations.AllValues.Add(new NetworkLocation(channel3, channel3.Length));
            discretization.Locations.AllValues.Add(new NetworkLocation(channel4, channel4.Length));
            
            var report = ComputationalGridValidator.Validate(discretization, null);
            
            Assert.That(report.Severity(), Is.EqualTo(ValidationSeverity.Error));
            
            // and no location on branch2 end (because branch1 end equals branch2 start)
            Assert.That(report.AllErrors.Count(), Is.EqualTo(2));
            Assert.That(report.AllErrors.Select(i => i.Message), Contains.Item($"No computational grid cells defined for branch : {channel2.Name}, not at end of branch; can not start calculation."));
            Assert.That(report.AllErrors.Select(i => i.Message), Contains.Item($"No computational grid cells defined for branch : {channel3.Name}, not at start of branch; can not start calculation."));
        }

        /// o------channel1-----o------channel2-----o------channel3-----o------channel4-----o
        /// x-x-x-x-x-x-x-x-x-x-x------------------- ------------------- -------------------x
        [Test]
        public void ValidateDiscretizationMissing3ChannelCalcPointsTest()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(4);
            var discretization = new Discretization() { Network = network, SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocationsAndConnectedBranchesWithoutLocationOnThemFullyCovered };
            Assert.That(discretization.Segments.AllValues.Count, Is.EqualTo(4));
            var channel = network.Channels.First();
            var channel2 = network.Channels.ElementAt(1);
            var channel2Segments = discretization.Segments.AllValues.Where(s => s.Branch.Equals(channel2));
            discretization.Segments.RemoveValues(discretization.Segments.CreateValuesFilter(channel2Segments));
            var channel3 = network.Channels.ElementAt(2);
            var channel3Segments = discretization.Segments.AllValues.Where(s => s.Branch.Equals(channel3));
            discretization.Segments.RemoveValues(discretization.Segments.CreateValuesFilter(channel3Segments));
            var channel4 = network.Channels.ElementAt(3);
            HydroNetworkHelper.GenerateDiscretization(discretization, true, false, 100.0, false, 0.0, false, false,
                true, 10.0, new List<IChannel> { channel });
            discretization.Locations.AllValues.Add(new NetworkLocation(channel4, channel4.Length));
            var report = ComputationalGridValidator.Validate(discretization, null);
            Assert.That(report.Severity(), Is.EqualTo(ValidationSeverity.Error));

            // error no locations on branch3 at start and end
            // and no location on branch2 end (because branch1 end equals branch2 start)
            Assert.That(report.AllErrors.Count(), Is.EqualTo(4));
            Assert.That(report.AllErrors.Select(i => i.Message), Contains.Item($"No computational grid cells defined for branch : {channel3.Name}, not at start of branch; can not start calculation."));
            Assert.That(report.AllErrors.Select(i => i.Message), Contains.Item($"No computational grid cells defined for branch : {channel2.Name}, not at end of branch; can not start calculation."));
            Assert.That(report.AllErrors.Select(i => i.Message), Contains.Item($"No computational grid cells defined for branch : {channel3.Name}, not at end of branch; can not start calculation."));
            Assert.That(report.AllErrors.Select(i => i.Message), Contains.Item($"No computational grid cells defined for branch : {channel4.Name}, not at start of branch; can not start calculation."));
        }

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
            var reportErrors = ComputationalGridValidator.Validate(model.NetworkDiscretization, model.Grid, model.MinimumSegmentLength);
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

            var reportErrors = ComputationalGridValidator.Validate(model.NetworkDiscretization, model.Grid, model.MinimumSegmentLength);
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

            var report = ComputationalGridValidator.Validate(model.NetworkDiscretization, model.Grid, model.MinimumSegmentLength);

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
            var dxmin1D = Convert.ToDouble(model.ModelDefinition.GetModelProperty(KnownProperties.Dxmin1D).GetValueAsString());
            var networkLocation = new NetworkLocation(firstChannel, dxmin1D);
            model.NetworkDiscretization[networkLocation] = 11.0;
            
            Assert.IsNotNull(model.NetworkDiscretization.Locations.Values.FirstOrDefault(l => l.Branch == firstChannel && Math.Abs(l.Chainage) < double.Epsilon));
            Assert.IsNotNull(model.NetworkDiscretization.Locations.Values.FirstOrDefault(l => l.Branch == firstChannel && Math.Abs(l.Chainage - dxmin1D) < double.Epsilon));
            var report = ComputationalGridValidator.Validate(model.NetworkDiscretization, model.Grid, model.MinimumSegmentLength);

            Assert.That(report.AllErrors.Any(), Is.False);
            Assert.That(report.AllErrors.Count(), Is.EqualTo(0));
            Assert.That(report.WarningCount, Is.EqualTo(0));
            Assert.That(report.Severity(), Is.EqualTo(ValidationSeverity.None));

            var offsetLessThanDxmin1D = dxmin1D - dxmin1D / 10;
            networkLocation = new NetworkLocation(firstChannel, dxmin1D + offsetLessThanDxmin1D);
            model.NetworkDiscretization[networkLocation] = 22.0;
            Assert.IsNotNull(model.NetworkDiscretization.Locations.Values.FirstOrDefault(l => l.Branch == firstChannel && Math.Abs(l.Chainage - (dxmin1D + offsetLessThanDxmin1D)) < double.Epsilon));
            report = ComputationalGridValidator.Validate(model.NetworkDiscretization, model.Grid, model.MinimumSegmentLength);

            Assert.That(report.WarningCount, Is.EqualTo(1));
            Assert.That(report.Severity(), Is.EqualTo(ValidationSeverity.Warning));

        }

        [Test]
        public void ValidateDiscretizationWithTwoPointsDistanceLessThen0point25()
        {
            var model = WaterFlowFMTestHelper.CreateModelWithDemoNetwork();

            var firstChannel = model.Network.Channels.First();  
            model.NetworkDiscretization[new NetworkLocation(firstChannel, 0.2)] = 0.0;

            Assert.IsNotNull(model.NetworkDiscretization.Locations.Values.FirstOrDefault(l => l.Branch == firstChannel && l.Chainage == 0.0));
            Assert.IsNotNull(model.NetworkDiscretization.Locations.Values.FirstOrDefault(l => l.Branch == firstChannel && l.Chainage == 0.2));

            var report = ComputationalGridValidator.Validate(model.NetworkDiscretization, model.Grid, model.MinimumSegmentLength);

            Assert.IsFalse(report.Issues.Any());
            Assert.AreEqual(ValidationSeverity.Warning, report.Severity());
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

            var report = ComputationalGridValidator.Validate(model.NetworkDiscretization, model.Grid, model.MinimumSegmentLength);
            Assert.IsFalse(report.Issues.Any());
        }

        [Test]
        public void TwoGridPointsWithTheSameNameGivesValidationError()
        {
            // Setup
            WaterFlowFMModel model = WaterFlowFMTestHelper.CreateModelWithDemoNetwork();

            const string nonUniqueName = "NotUnique";

            model.NetworkDiscretization.Locations.Values[0].Name = nonUniqueName;
            model.NetworkDiscretization.Locations.Values[1].Name = nonUniqueName;

            // Call
            ValidationReport report = ComputationalGridValidator.Validate(model.NetworkDiscretization, model.Grid);
            
            // Assert
            const string expectedMessage = "Several grid points with the same id exist";
            Assert.That(report.AllErrors.Any(error => error.Message.Equals(expectedMessage)), Is.True);
        }
    }
}