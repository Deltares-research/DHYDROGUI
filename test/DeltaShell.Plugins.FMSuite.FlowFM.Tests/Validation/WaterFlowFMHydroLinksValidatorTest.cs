using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
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
            HydroModel hydroModel = new HydroModelBuilder().BuildModel(ModelGroup.RHUModels);
            WaterFlowFMModel fmModel = hydroModel.GetActivitiesOfType<WaterFlowFMModel>().First();

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
            HydroModel hydroModel = new HydroModelBuilder().BuildModel(ModelGroup.RHUModels);
            WaterFlowFMModel fmModel = hydroModel.GetActivitiesOfType<WaterFlowFMModel>().First();
            RainfallRunoffModel rrModel = hydroModel.GetActivitiesOfType<RainfallRunoffModel>().First();
            
            IHydroNetwork network = fmModel.Network;

            var node1 = new HydroNode("node1");
            var node2 = new HydroNode("node2");
            var branch = new Branch(node1, node2);

            network.Nodes.Add(node1);
            network.Nodes.Add(node1);
            network.Branches.Add(branch);

            const string nonUniqueName = "nonUniqueName";
            var lateralSource1 = new LateralSource { Name = nonUniqueName, Branch = branch, Network = network }; // linked to catchment
            var lateralSource2 = new LateralSource { Name = nonUniqueName, Branch = branch, Network = network }; // not linked

            var lateralSourceData1 = new Model1DLateralSourceData() { Feature = lateralSource1 };
            var lateralSourceData2 = new Model1DLateralSourceData() { Feature = lateralSource2 };
            
            fmModel.LateralSourcesData.Add(lateralSourceData1);
            fmModel.LateralSourcesData.Add(lateralSourceData2);
            
            var catchment = new Catchment();
            rrModel.Basin.Catchments.Add(catchment);
            
            catchment.LinkTo(lateralSource1);
            lateralSourceData1.DataType = lateralDataType;

            // Call
            ValidationReport report = WaterFlowFMHydroLinksValidator.Validate(fmModel);
            
            // Assert
            const string expectedMessage = "A hydrolink between a catchment and a lateral must be of type realtime.";
            Assert.That(report.AllErrors.Any(error => error.Message.Equals(expectedMessage)), Is.True);
        }
    }
}