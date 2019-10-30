using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using NUnit.Framework;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
    public class HydroModelExtensionsTest
    {
        [Test]
        [Category(TestCategory.Integration)]
        public void MoveRrModelIntoIntegratedModel()
        {
            var folder = new Folder();
            var rrModel = new RainfallRunoffModel();
            var c = new Catchment() {Name = "test Unpaved Catchment", CatchmentType = CatchmentType.GreenHouse};
            rrModel.Basin.Catchments.Add(c);
            var data = (GreenhouseData) rrModel.GetCatchmentModelData(c);
            data.CalculationArea = 3.45;
            folder.Add(rrModel);

            var builder = new HydroModelBuilder();
            var integratedModel = builder.BuildModel(ModelGroup.All);

            rrModel.MoveModelIntoIntegratedModel(folder, integratedModel);

            // Test whether the existing RR model was replaced. 
            var imRrModel = integratedModel.Activities.OfType<RainfallRunoffModel>().First();
            Assert.That(Equals(rrModel, imRrModel));

            // Test whether the basin has been imported as a SubRegion in the HydroRegion. 
            var imBasin = integratedModel.Region.SubRegions.OfType<DrainageBasin>().First();
            Assert.AreEqual(rrModel.Basin, imBasin);

            // Test whether there is a link from the RR model's basin to the basin that is part of the HydroRegion. 
            var imBasinDataItem = imRrModel.AllDataItems.First(di => di.ValueType == typeof (DrainageBasin));
            Assert.That(imBasinDataItem.LinkedTo != null);
            Assert.That(Equals(imBasinDataItem.LinkedTo.Value, imBasin));

            // Test whether the boundary data is transferred to the RR model in the integrated model.  
            var c2 = imBasin.Catchments.First();
            Assert.That(c2.Name == "test Unpaved Catchment");
            Assert.That(imRrModel.GetCatchmentModelData(c2).CalculationArea == 3.45);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void MoveFlowModelIntoIntegratedModel()
        {
            var folder = new Folder();
            var flowModel = new WaterFlowModel1D();
            var n = new Node("test node");
            flowModel.Network.Nodes.Add(n);
            folder.Add(flowModel);
            var boundary = new Model1DBoundaryNodeData {Name = "test boundary", Feature = n};
            flowModel.BoundaryConditions.Add(boundary);

            var builder = new HydroModelBuilder();
            var integratedModel = builder.BuildModel(ModelGroup.All);

            flowModel.MoveModelIntoIntegratedModel(folder, integratedModel);

            // Test whether the existing flow model was replaced. 
            var imFlowModel = integratedModel.Activities.OfType<WaterFlowModel1D>().First();
            Assert.That(Equals(flowModel, imFlowModel));

            // Test whether the network has been imported as a SubRegion in the HydroRegion. 
            var imNetwork = integratedModel.Region.SubRegions.OfType<IHydroNetwork>().First();
            Assert.AreEqual(flowModel.Network, imNetwork);

            // Test whether there is a link from the flow model's network to the network that is part of the HydroRegion. 
            var imNetworkDataItem = imFlowModel.AllDataItems.First(di => di.Value is IHydroNetwork);
            Assert.That(imNetworkDataItem.LinkedTo != null);
            Assert.That(Equals(imNetworkDataItem.LinkedTo.Value, imNetwork));

            // Test whether the model data is moved correctly to the integrated model. 
            Assert.That(imFlowModel.Network.Nodes.First().Name == "test node");
            Assert.That(Equals(imFlowModel.BoundaryConditions.First().Feature, imFlowModel.Network.Nodes.First()));

        }

        [Test]
        [Category(TestCategory.Integration)]
        public void UpgradeRrModelIntoIntegratedModel()
        {
            var folder = new Folder();
            var rrModel = new RainfallRunoffModel();
            var c = new Catchment() { Name = "test Unpaved Catchment", CatchmentType = CatchmentType.GreenHouse };
            rrModel.Basin.Catchments.Add(c);
            var data = (GreenhouseData)rrModel.GetCatchmentModelData(c);
            data.CalculationArea = 3.45;
            folder.Add(rrModel);

            rrModel.UpgradeModelIntoIntegratedModel(folder);

            var integratedModel = folder.Models.First(m => m is HydroModel) as HydroModel;
            var imRrModel = integratedModel.Activities.OfType<RainfallRunoffModel>().First();
            Assert.That(Equals(rrModel, imRrModel));

            var imBasin = integratedModel.Region.SubRegions.OfType<DrainageBasin>().First();
            Assert.AreEqual(rrModel.Basin, imBasin);

            var imBasinDataItem = imRrModel.AllDataItems.First(di => di.ValueType == typeof(DrainageBasin));
            Assert.That(imBasinDataItem.LinkedTo != null);
            Assert.That(Equals(imBasinDataItem.LinkedTo.Value, imBasin));

            var c2 = imBasin.Catchments.First();
            Assert.That(c2.Name == "test Unpaved Catchment");
            Assert.That(imRrModel.GetCatchmentModelData(c2).CalculationArea == 3.45);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void UpgradeFlowModelIntoIntegratedModel()
        {
            var folder = new Folder();
            var flowModel = new WaterFlowModel1D();
            var n = new Node("test node");
            flowModel.Network.Nodes.Add(n);
            folder.Add(flowModel);
            var boundary = new Model1DBoundaryNodeData { Name = "test boundary", Feature = n };
            flowModel.BoundaryConditions.Add(boundary);

            flowModel.UpgradeModelIntoIntegratedModel(folder);

            var integratedModel = folder.Models.OfType<HydroModel>().First();
            var imFlowModel = integratedModel.Activities.OfType<WaterFlowModel1D>().First();
            Assert.That(Equals(flowModel, imFlowModel));

            var imNetwork = integratedModel.Region.SubRegions.OfType<IHydroNetwork>().First();
            Assert.AreEqual(flowModel.Network, imNetwork);

            var imNetworkDataItem = imFlowModel.AllDataItems.First(di => di.Value is IHydroNetwork);
            Assert.That(imNetworkDataItem.LinkedTo != null);
            Assert.That(Equals(imNetworkDataItem.LinkedTo.Value, imNetwork));

            // Test whether the model data is moved correctly to the integrated model. 
            Assert.That(imFlowModel.Network.Nodes.First().Name == "test node");
            Assert.That(Equals(imFlowModel.BoundaryConditions.First().Feature, imFlowModel.Network.Nodes.First()));
        }
    }
}
