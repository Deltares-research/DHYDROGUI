using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.Utils.Validation;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
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
            
            var lateralSource = new LateralSource();
            var branch = new Channel();
            fmModel.Network.Branches.Add(branch);
            branch.BranchFeatures.Add(lateralSource);
            
            var catchment = new Catchment();
            rrModel.Basin.Catchments.Add(catchment);
            
            Model1DLateralSourceData lateralSourceData = fmModel.LateralSourcesData.First();
            catchment.LinkTo(lateralSourceData.Feature);
            
            lateralSourceData.DataType = lateralDataType;
            
            // Call
            ValidationReport report = fmModel.Validate();
            
            // Assert
            ValidationReport hydroLinksReport = report.SubReports.FirstOrDefault(sr => sr.Category.Equals("HydroLinks"));
            Assert.That(hydroLinksReport, Is.Not.Null);
            
            IEnumerable<ValidationIssue> issues = hydroLinksReport.Issues;
            Assert.That(issues, Has.Count.EqualTo(1));
            
            ValidationIssue issue = issues.First();
            const string expectedErrorMessage = "A hydrolink between a catchment and a lateral must be of type realtime.";
            Assert.That(issue.Message, Is.EqualTo(expectedErrorMessage));
            Assert.That(issue.Severity, Is.EqualTo(ValidationSeverity.Error));
        }
    }
}