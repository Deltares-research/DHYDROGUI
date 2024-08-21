using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Validation;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation
{
    [TestFixture]
    public class WaterFlowFMHydroLinksValidatorTest
    {
        [Test]
        public void Validate_HydroRegionNull_ReturnsEmptyValidationReport()
        {
            // Setup
            var fmModel = new WaterFlowFMModel();
            
            // Precondition
            Assert.That(fmModel.Network.Parent, Is.Null);

            // Call
            ValidationReport report = fmModel.Validate();
            
            // Assert
            ValidationReport hydroLinksReport = report.SubReports.FirstOrDefault(sr => sr.Category.Equals("HydroLinks"));
            Assert.That(hydroLinksReport, Is.Not.Null);
            Assert.That(hydroLinksReport.Issues, Is.Empty);
        }

        [Test]
        public void Validate_RealtimeLateralWithoutLinkBetweenCatchmentAndLateral_AddsValidationIssueToReport()
        {
            // Setup
            var fmModel = new WaterFlowFMModel();
            LinkWithHydroModelActivities(fmModel);

            var lateralSourceData = new Model1DLateralSourceData()
            {
                Feature = new LateralSource(),
                DataType = Model1DLateralDataType.FlowRealTime
            };
            
            fmModel.LateralSourcesData.Add(lateralSourceData);
            
            // Call
            ValidationReport report = fmModel.Validate();
            
            // Assert
            ValidationReport hydroLinksReport = report.SubReports.FirstOrDefault(sr => sr.Category.Equals("HydroLinks"));
            Assert.That(hydroLinksReport, Is.Not.Null);

            IEnumerable<ValidationIssue> issues = hydroLinksReport.Issues;
            Assert.That(issues, Has.Count.EqualTo(1));

            ValidationIssue issue = issues.First();
            const string expectedErrorMessage = "A lateral of type realtime must have a hydrolink between a catchment and the lateral.";
            Assert.That(issue.Message, Is.EqualTo(expectedErrorMessage));
            Assert.That(issue.Severity, Is.EqualTo(ValidationSeverity.Error));
        }

        [Test]
        [TestCase(Model1DLateralDataType.FlowConstant)]
        [TestCase(Model1DLateralDataType.FlowTimeSeries)]
        [TestCase(Model1DLateralDataType.FlowWaterLevelTable)]
        public void Validate_HydroLinkBetweenCatchmentAndLateral_WhenNotRealTime_AddsValidationIssueToReport(Model1DLateralDataType lateralDataType)
        {
            // Setup
            var fmModel = new WaterFlowFMModel();
            var rrModel = new RainfallRunoffModel();
            LinkWithHydroModelActivities(fmModel, rrModel);
            
            Branch branch = CreateBranchInNetwork(fmModel);
            LateralSource lateralSource1 = CreateLateralSourcesOnBranch(branch, fmModel);
            
            var catchment = new Catchment();
            rrModel.Basin.Catchments.Add(catchment);
            
            catchment.LinkTo(lateralSource1);
            
            foreach (Model1DLateralSourceData lateralSourcesData in fmModel.LateralSourcesData)
            {
                lateralSourcesData.DataType = lateralDataType;
            }

            // Call
            ValidationReport report = WaterFlowFMHydroLinksValidator.Validate(fmModel);
            
            // Assert
            const string expectedMessage = "A hydrolink between a catchment and a lateral must be of type realtime.";
            Assert.That(report.AllErrors.Any(error => error.Message.Equals(expectedMessage)), Is.True);
        }
        
        [Test]
        public void RealTimeLateralSourceWithIncomingLinkFromCatchmentAndWasteWaterTreatmentPlant_IsValidForLinkWasteWaterTreatmentPlantFirst()
        {
            // Setup
            var fmModel = new WaterFlowFMModel();
            var rrModel = new RainfallRunoffModel();
            LinkWithHydroModelActivities(fmModel, rrModel);
            
            Branch branch = CreateBranchInNetwork(fmModel);
            LateralSource lateralSource1 = CreateLateralSourcesOnBranch(branch, fmModel);

            var catchment = new Catchment();
            rrModel.Basin.Catchments.Add(catchment);

            var wasteWaterTreatmentPlant = new WasteWaterTreatmentPlant();
            rrModel.Basin.WasteWaterTreatmentPlants.Add(wasteWaterTreatmentPlant);
            
            // Call
            LinkWasteWaterTreatmentPlantFirst(wasteWaterTreatmentPlant, lateralSource1, catchment);
            ValidationReport report = WaterFlowFMHydroLinksValidator.Validate(fmModel);
            
            // Assert
            Assert.That(report.AllErrors.Any(), Is.False);
        }
        
        [Test]
        public void RealTimeLateralSourceWithIncomingLinkFromCatchmentAndWasteWaterTreatmentPlant_IsValidForLinkCatchmentFirst()
        {
            // Setup
            var fmModel = new WaterFlowFMModel();
            var rrModel = new RainfallRunoffModel();
            LinkWithHydroModelActivities(fmModel, rrModel);
            
            Branch branch = CreateBranchInNetwork(fmModel);
            LateralSource lateralSource1 = CreateLateralSourcesOnBranch(branch, fmModel);

            var catchment = new Catchment();
            rrModel.Basin.Catchments.Add(catchment);

            var wasteWaterTreatmentPlant = new WasteWaterTreatmentPlant();
            rrModel.Basin.WasteWaterTreatmentPlants.Add(wasteWaterTreatmentPlant);
            
            // Call
            LinkCatchmentFirst(wasteWaterTreatmentPlant, lateralSource1, catchment);
            ValidationReport report = WaterFlowFMHydroLinksValidator.Validate(fmModel);
            
            // Assert
            Assert.That(report.AllErrors.Any(), Is.False);
        }

        [Test]
        public void RealTimeLateralSourceWithIncomingLinkFromOnlyWasteWaterTreatmentPlant_IsInvalidAndGivesError()
        {
            // Setup
            var fmModel = new WaterFlowFMModel();
            var rrModel = new RainfallRunoffModel();
            LinkWithHydroModelActivities(fmModel, rrModel);

            Branch branch = CreateBranchInNetwork(fmModel);
            LateralSource lateralSource1 = CreateLateralSourcesOnBranch(branch, fmModel);
            
            var wasteWaterTreatmentPlant = new WasteWaterTreatmentPlant();
            rrModel.Basin.WasteWaterTreatmentPlants.Add(wasteWaterTreatmentPlant);
            wasteWaterTreatmentPlant.LinkTo(lateralSource1);
            
            // Call
            ValidationReport report = WaterFlowFMHydroLinksValidator.Validate(fmModel);
            
            // Assert
            const string expectedMessage = "A lateral of type realtime must have a hydrolink between a catchment and the lateral.";
            Assert.That(report.AllErrors.Any(error => error.Message.Equals(expectedMessage)), Is.True);
        }

        private static void LinkWithHydroModelActivities(WaterFlowFMModel fmModel, RainfallRunoffModel rrModel = null)
        {
            var hydroModel = new HydroModel();
            hydroModel.Activities.Add(fmModel);
            if (rrModel != null)
            {
                hydroModel.Activities.Add(rrModel);
            }
        }

        private static LateralSource CreateLateralSourcesOnBranch(Branch branch, WaterFlowFMModel fmModel)
        {
            const string nonUniqueName = "nonUniqueName";
            var lateralSource1 = new LateralSource { Name = nonUniqueName, Branch = branch, Network = fmModel.Network }; // linked to catchment
            var lateralSource2 = new LateralSource { Name = nonUniqueName, Branch = branch, Network = fmModel.Network }; // not linked

            var lateralSourceData1 = new Model1DLateralSourceData() { Feature = lateralSource1, DataType = Model1DLateralDataType.FlowRealTime};
            var lateralSourceData2 = new Model1DLateralSourceData() { Feature = lateralSource2, DataType = Model1DLateralDataType.FlowRealTime};
            
            fmModel.LateralSourcesData.Add(lateralSourceData1);
            fmModel.LateralSourcesData.Add(lateralSourceData2);
            return lateralSource1;
        }

        private static Branch CreateBranchInNetwork(WaterFlowFMModel fmModel)
        {
            IHydroNetwork network = fmModel.Network;

            var node1 = new HydroNode("node1");
            var node2 = new HydroNode("node2");
            var branch = new Branch(node1, node2);

            network.Nodes.Add(node1);
            network.Nodes.Add(node1);
            network.Branches.Add(branch);
            return branch;
        }
        
        private static void LinkCatchmentFirst(IHydroObject wasteWaterTreatmentPlant, IHydroObject lateralSource, IHydroObject catchment)
        {
            catchment.LinkTo(lateralSource);
            wasteWaterTreatmentPlant.LinkTo(lateralSource);
        }

        private static void LinkWasteWaterTreatmentPlantFirst(IHydroObject wasteWaterTreatmentPlant, IHydroObject lateralSource, IHydroObject catchment)
        {
            wasteWaterTreatmentPlant.LinkTo(lateralSource);
            catchment.LinkTo(lateralSource);
        }
    }
}