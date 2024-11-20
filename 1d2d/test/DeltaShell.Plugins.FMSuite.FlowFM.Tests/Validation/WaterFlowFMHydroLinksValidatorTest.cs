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
            ValidationReport report = WaterFlowFMHydroLinksValidator.Validate(fmModel);

            // Assert
            Assert.That(report, Is.Not.Null);
            Assert.That(report.Issues, Is.Empty);
        }

        [Test]
        public void Validate_RealtimeLateralSourceWithoutLinks_IsInvalid()
        {
            // Setup
            HydroModel hydroModel = CreateHydroModelWithCatchmentAndWasteWaterTreatmentPlant();
            var fmModel = GetHydroModelChildOfType<WaterFlowFMModel>(hydroModel);

            // Call
            ValidationReport report = WaterFlowFMHydroLinksValidator.Validate(fmModel);

            // Assert
            Assert.That(report, Is.Not.Null);
            Assert.That(report.Issues, Has.Count.EqualTo(1));

            const string expectedMessage = "A lateral of type realtime must have a hydrolink between a catchment or a waste water treatment plant and the lateral.";
            Assert.That(report.AllErrors, Has.One.Matches<ValidationIssue>(issue => issue.Message == expectedMessage));
        }

        [Test]
        [TestCase(Model1DLateralDataType.FlowConstant)]
        [TestCase(Model1DLateralDataType.FlowTimeSeries)]
        [TestCase(Model1DLateralDataType.FlowWaterLevelTable)]
        public void Validate_CatchmentLinkedToNonRealtimeLateralSource_IsInvalid(Model1DLateralDataType dataType)
        {
            // Setup
            HydroModel hydroModel = CreateHydroModelWithCatchmentAndWasteWaterTreatmentPlant();
            var fmModel = GetHydroModelChildOfType<WaterFlowFMModel>(hydroModel);
            var rrModel = GetHydroModelChildOfType<RainfallRunoffModel>(hydroModel);

            LinkCatchmentToLateralSource(fmModel, rrModel);
            SetLateralSourceDataType(fmModel, dataType);

            // Call
            ValidationReport report = WaterFlowFMHydroLinksValidator.Validate(fmModel);

            // Assert
            Assert.That(report, Is.Not.Null);
            Assert.That(report.Issues, Has.Count.EqualTo(1));

            const string expectedMessage = "A hydrolink between a catchment and a lateral must be of type realtime.";
            Assert.That(report.AllErrors, Has.One.Matches<ValidationIssue>(issue => issue.Message == expectedMessage));
        }

        [Test]
        public void Validate_WasteWaterTreatmentPlantAndCatchmentLinkedToLateralSource_IsValid()
        {
            // Setup
            HydroModel hydroModel = CreateHydroModelWithCatchmentAndWasteWaterTreatmentPlant();

            var fmModel = GetHydroModelChildOfType<WaterFlowFMModel>(hydroModel);
            var rrModel = GetHydroModelChildOfType<RainfallRunoffModel>(hydroModel);

            LinkWasteWaterTreatmentPlantToLateralSource(fmModel, rrModel);
            LinkCatchmentToLateralSource(fmModel, rrModel);

            // Call
            ValidationReport report = WaterFlowFMHydroLinksValidator.Validate(fmModel);

            // Assert
            Assert.That(report, Is.Not.Null);
            Assert.That(report.Issues, Is.Empty);
        }

        [Test]
        public void Validate_CatchmentAndWasteWaterTreatmentPlantLinkedToLateralSource_IsValid()
        {
            // Setup
            HydroModel hydroModel = CreateHydroModelWithCatchmentAndWasteWaterTreatmentPlant();
            var fmModel = GetHydroModelChildOfType<WaterFlowFMModel>(hydroModel);
            var rrModel = GetHydroModelChildOfType<RainfallRunoffModel>(hydroModel);

            LinkCatchmentToLateralSource(fmModel, rrModel);
            LinkWasteWaterTreatmentPlantToLateralSource(fmModel, rrModel);

            // Call
            ValidationReport report = WaterFlowFMHydroLinksValidator.Validate(fmModel);

            // Assert
            Assert.That(report, Is.Not.Null);
            Assert.That(report.Issues, Is.Empty);
        }

        [Test]
        public void Validate_CatchmentLinkedToLateralSource_IsValid()
        {
            // Setup
            HydroModel hydroModel = CreateHydroModelWithCatchmentAndWasteWaterTreatmentPlant();

            var fmModel = GetHydroModelChildOfType<WaterFlowFMModel>(hydroModel);
            var rrModel = GetHydroModelChildOfType<RainfallRunoffModel>(hydroModel);

            LinkCatchmentToLateralSource(fmModel, rrModel);

            // Call
            ValidationReport report = WaterFlowFMHydroLinksValidator.Validate(fmModel);

            // Assert
            Assert.That(report, Is.Not.Null);
            Assert.That(report.Issues, Is.Empty);
        }

        [Test]
        public void Validate_WasteWaterTreatmentPlantLinkedToLateralSource_IsValid()
        {
            // Setup
            HydroModel hydroModel = CreateHydroModelWithCatchmentAndWasteWaterTreatmentPlant();
            var fmModel = GetHydroModelChildOfType<WaterFlowFMModel>(hydroModel);
            var rrModel = GetHydroModelChildOfType<RainfallRunoffModel>(hydroModel);

            LinkWasteWaterTreatmentPlantToLateralSource(fmModel, rrModel);

            // Call
            ValidationReport report = WaterFlowFMHydroLinksValidator.Validate(fmModel);

            // Assert
            Assert.That(report, Is.Not.Null);
            Assert.That(report.Issues, Is.Empty);
        }

        private static HydroModel CreateHydroModelWithCatchmentAndWasteWaterTreatmentPlant()
        {
            var fmModel = new WaterFlowFMModel();
            var rrModel = new RainfallRunoffModel();
            var hydroModel = new HydroModel();

            hydroModel.Activities.Add(fmModel);
            hydroModel.Activities.Add(rrModel);

            Branch branch = CreateBranchInNetwork(fmModel);
            CreateLateralSourceOnBranch(branch, fmModel);

            var catchment = new Catchment();
            rrModel.Basin.Catchments.Add(catchment);

            var wasteWaterTreatmentPlant = new WasteWaterTreatmentPlant();
            rrModel.Basin.WasteWaterTreatmentPlants.Add(wasteWaterTreatmentPlant);

            return hydroModel;
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

        private static void CreateLateralSourceOnBranch(Branch branch, WaterFlowFMModel fmModel)
        {
            var lateralSource = new LateralSource
            {
                Name = "nonUniqueName",
                Branch = branch,
                Network = fmModel.Network
            };

            var lateralSourceData = new Model1DLateralSourceData
            {
                Feature = lateralSource,
                DataType = Model1DLateralDataType.FlowRealTime
            };

            fmModel.LateralSourcesData.Add(lateralSourceData);
        }

        private static void LinkCatchmentToLateralSource(WaterFlowFMModel fmModel, RainfallRunoffModel rrModel)
        {
            LinkHydroObjectToLateralSource(fmModel, rrModel.Basin.Catchments.First());
        }

        private static void LinkWasteWaterTreatmentPlantToLateralSource(WaterFlowFMModel fmModel, RainfallRunoffModel rrModel)
        {
            LinkHydroObjectToLateralSource(fmModel, rrModel.Basin.WasteWaterTreatmentPlants.First());
        }

        private static void LinkHydroObjectToLateralSource(WaterFlowFMModel fmModel, IHydroObject hydroObject)
        {
            LateralSource lateralSource = fmModel.LateralSourcesData.First().Feature;
            hydroObject.LinkTo(lateralSource);
        }

        private static void SetLateralSourceDataType(WaterFlowFMModel fmModel, Model1DLateralDataType dataType)
        {
            Model1DLateralSourceData lateralSourcesData = fmModel.LateralSourcesData.First();
            lateralSourcesData.DataType = dataType;
        }

        private static T GetHydroModelChildOfType<T>(HydroModel hydroModel)
        {
            return hydroModel.Activities.OfType<T>().First();
        }
    }
}