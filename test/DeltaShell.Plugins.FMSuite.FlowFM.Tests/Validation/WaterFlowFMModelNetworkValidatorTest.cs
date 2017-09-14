using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation
{
    [TestFixture]
    public class WaterFlowFMModelNetworkValidatorTest
    {
        [Test]
        public void Validate1DNetworkFromWaterFlowFMModel1D2DValidator()
        {
            var model = new WaterFlowFMModel();
            var report = WaterFlowFMModelNetworkValidator.Validate(model.Network);
            Assert.AreEqual(0, report.ErrorCount);
        }

        [Test]
        public void WaterFlowFMModelNetworkValidatesIfNoCrossSectionIsPresent()
        {
            var model = new WaterFlowFMModel();
            var network = model.Network;
            WaterFlowFMTestHelper.ConfigureDemoNetwork(network);

            var lstCS = network.CrossSections.ToList();
            network.Branches.ForEach( b => b.BranchFeatures.RemoveAllWhere( bf => lstCS.Contains(bf)));
            Assert.IsEmpty(network.CrossSections);
            
            var report = WaterFlowFMModelNetworkValidator.Validate(model.Network);
            Assert.AreEqual(0, report.ErrorCount);
        }

        [Test]
        public void BranchesWithTheSameOrderNumberWithOnlyZWCrossSectionShouldBeValid()
        {
            var network = new HydroNetwork();
            WaterFlowFMTestHelper.ConfigureDemoNetwork(network);

            #region remove all CS and add one ZW cross-section at branch2

            foreach (var cs in network.CrossSections.ToList())
            {
                cs.Branch.BranchFeatures.Remove(cs);
            }

            var zwcs = CrossSection.CreateDefault(CrossSectionType.ZW, null);
            NetworkHelper.AddBranchFeatureToBranch(zwcs, network.Branches.Last(), 50.0);


            #endregion

            var lstBranches = network.Branches.ToList();
            var lstCS = network.CrossSections.ToList();

            Assert.AreEqual(2, lstBranches.Count);
            Assert.AreEqual(1, lstCS.Count);

            lstBranches[0].OrderNumber = 1;
            lstBranches[1].OrderNumber = 1;

            Assert.IsFalse(ContainsError(WaterFlowFMModelNetworkValidator.Validate(network),
                string.Format("No cross sections on channel {0}; can not start calculation.", lstBranches[1].Name)));
        }

        [Test]
        public void ThreeBranchesWithTheSameOrderNumber_ConnectedToOneNode_Should_Not_Be_Valid()
        {
            var network = new HydroNetwork();
            WaterFlowFMTestHelper.ConfigureDemoNetwork(network);

            #region add third branch with CS

            var node4 = new HydroNode { Name = "Node4", Network = network };
            node4.Geometry = new Point(200.0, 0.0);
            network.Nodes.Add(node4);

            var branch3 = new Channel("branch3", network.Nodes[1], node4, 100.0);

            branch3.Geometry = new LineString(new[]
            {
                new Coordinate(100, 0),
                new Coordinate(200, 0)
            });

            network.Branches.Add(branch3);


            var crossSection3 = new CrossSectionDefinitionXYZ("crs3");
            var csFeature3 = HydroNetworkHelper.AddCrossSectionDefinitionToBranch(branch3, crossSection3, 50);
            csFeature3.Name = "cs3";
            csFeature3.Geometry = new LineString(new[] { new Coordinate(150, -5), new Coordinate(150, 5) });

            #endregion

            var lstBranches = network.Branches.ToList();
            const int orderNumber = 1;

            Assert.AreEqual(3, lstBranches.Count);

            Assert.AreEqual(-1, lstBranches[0].OrderNumber);
            Assert.AreEqual(-1, lstBranches[1].OrderNumber);
            Assert.AreEqual(-1, lstBranches[2].OrderNumber);

            Assert.IsFalse(ContainsError(WaterFlowFMModelNetworkValidator.Validate(network),
                string.Format("More than two branches with the same ordernumber '{0}' are connected to node {1}; can not start calculation.",
                    orderNumber, network.Nodes[1].Name)));

            lstBranches[0].OrderNumber = orderNumber;
            lstBranches[1].OrderNumber = orderNumber;
            lstBranches[2].OrderNumber = orderNumber;

            Assert.IsTrue(ContainsError(WaterFlowFMModelNetworkValidator.Validate(network),
                string.Format("More than two branches with the same ordernumber '{0}' are connected to node {1}; can not start calculation.",
                    orderNumber, network.Nodes[1].Name)));
        }

        [Test]
        public void MultiTypeOfCrossSection_OnBranchesWithTheSameOrderNumber_Should_Not_Be_Valid()
        {
            var network = new HydroNetwork();
            WaterFlowFMTestHelper.ConfigureDemoNetwork(network);

            #region Remove first cs and add ZW crossSection

            var cs = network.CrossSections.First();
            cs.Branch.BranchFeatures.Remove(cs);

            var heightFlowStorageWidthData = new List<HeightFlowStorageWidth>
            {
                new HeightFlowStorageWidth(0.0,10.0,10.0),
                new HeightFlowStorageWidth(10.0,30.0,30.0)
            };

            var crossSectionDefinition = new CrossSectionDefinitionZW { Name = "ZW1" };
            var zwCS = new CrossSection(crossSectionDefinition);
            zwCS.Name = "zwCS";
            zwCS.Branch = cs.Branch;
            zwCS.Chainage = 50.0;

            crossSectionDefinition.ZWDataTable.Set(heightFlowStorageWidthData);

            NetworkHelper.AddBranchFeatureToBranch(zwCS, zwCS.Branch, zwCS.Chainage);

            #endregion

            var lstBranches = network.Branches.ToList();
            var lstCS = network.CrossSections.ToList();

            Assert.AreEqual(2, lstBranches.Count);
            Assert.AreEqual(2, lstCS.Count);
            Assert.AreNotEqual(lstCS[0].CrossSectionType, lstCS[1].CrossSectionType);

            lstBranches[0].OrderNumber = 1;
            lstBranches[1].OrderNumber = 1;

            Assert.IsTrue(ContainsError(WaterFlowFMModelNetworkValidator.Validate(network),
                string.Format("Multiple cross-section-types (mix of Standard/ZW and Geometry/YZ) per branch(es) not supported.({0})",
                    lstBranches[1].Name + "," + lstBranches[0].Name)));

        }

        [Test]
        public void NoCrossSectionsInNetworkGiveAValidationWarning()
        {
            var model = WaterFlowFMTestHelper.CreateModelWithDemoNetwork(false);

            var report = WaterFlowFMModelNetworkValidator.Validate(model.Network);
            Assert.AreNotEqual(0, report.WarningCount);

            var expectedMessage = Resources
                .WaterFlowFMModelNetworkValidator_GetCrossSectionValidationIssues_No_CrossSection_defined__all_channels_will_be_using_the_default_values_;
            ContainsValidationIssue(report, expectedMessage, ValidationSeverity.Warning);
        }

        [Test]
        public void ZWCrossSectionShouldHaveAtMostOneEntryWithWidthEqualToZero()
        {
            var network = new HydroNetwork();
            WaterFlowFMTestHelper.ConfigureDemoNetwork(network);

            #region remove all CrossSection and add one ZW cross-section at both branches

            foreach (var cs in network.CrossSections.ToList())
            {
                cs.Branch.BranchFeatures.Remove(cs);
            }

            var zwcs = CrossSection.CreateDefault(CrossSectionType.ZW, null);
            zwcs.Name = "zwcs";
            ((CrossSectionDefinitionZW)zwcs.Definition).ZWDataTable.AddCrossSectionZWRow(-15.0, 0.0, 0.0); // add first row of zero width
            NetworkHelper.AddBranchFeatureToBranch(zwcs, network.Branches.First(), 50.0);
            var zwcs2 = CrossSection.CreateDefault(CrossSectionType.ZW, null);
            zwcs2.Name = "zwcs2";
            NetworkHelper.AddBranchFeatureToBranch(zwcs2, network.Branches.Last(), 50.0);

            #endregion

            Assert.IsFalse(ContainsError(WaterFlowFMModelNetworkValidator.Validate(network),
                string.Format("tabulated cross section {0} cannot have zero width at levels above deepest point of its definition.", zwcs.Name)));

            ((CrossSectionDefinitionZW)zwcs.Definition).ZWDataTable.AddCrossSectionZWRow(-20.0, 0.0, 0.0); // add second row of zero width (no points in between)

            Assert.IsTrue(ContainsError(WaterFlowFMModelNetworkValidator.Validate(network),
                string.Format("tabulated cross section {0} cannot have zero width at levels above deepest point of its definition.", zwcs.Name)));

        }

        private static bool ContainsError(ValidationReport report, string errorMessage)
        {
            return ContainsValidationIssue(report, errorMessage, ValidationSeverity.Error);
        }
        private static bool ContainsValidationIssue(ValidationReport report, string errorMessage, ValidationSeverity severity)
        {
            foreach (var issue in report.Issues.Where(i => i.Severity == severity))
            {
                Console.WriteLine(issue.Message);
                if (issue.Message == errorMessage) return true;
            }

            return report.SubReports.Any(subReport => ContainsValidationIssue(subReport, errorMessage, severity));
        }
    }
}