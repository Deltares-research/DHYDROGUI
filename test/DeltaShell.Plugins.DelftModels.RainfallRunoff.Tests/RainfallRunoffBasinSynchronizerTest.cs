using System.Linq;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow.DataItems;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests
{
    [TestFixture]
    public class RainfallRunoffBasinSynchronizerTest
    {
        [Test]
        public void AddCatchmentToBasinAddsModelData()
        {
            var rrModel = new RainfallRunoffModel { Name = "Test" };

            Assert.AreEqual(0, rrModel.GetAllModelData().Count());

            rrModel.Basin.Catchments.Add(new Catchment { CatchmentType = CatchmentType.Paved });

            Assert.AreEqual(1, rrModel.GetAllModelData().Count());
        }

        [Test]
        public void ChangeCatchmentTypeRemovesModelData()
        {
            var rrModel = new RainfallRunoffModel { Name = "Test" };
            var catchment = new Catchment { CatchmentType = CatchmentType.GreenHouse };
            rrModel.Basin.Catchments.Add(catchment);

            Assert.AreEqual(1, rrModel.GetAllModelData().Count());

            var pavedCatchment = new Catchment { CatchmentType = CatchmentType.Paved };
            rrModel.Basin.Catchments.Add(pavedCatchment);

            Assert.AreEqual(2, rrModel.GetAllModelData().Count());

            pavedCatchment.CatchmentType = CatchmentType.None;

            Assert.AreEqual(1, rrModel.GetAllModelData().Count());
        }

        [Test]
        public void ChangeCatchmentTypeRemovesModelDataForLinkedBasin()
        {
            var rrModel = new RainfallRunoffModel { Name = "Test" };
            var catchment = new Catchment { CatchmentType = CatchmentType.GreenHouse };
            var basin = new DrainageBasin {Catchments = {catchment}};
            var dataItem = new DataItem(basin, DataItemRole.Input);

            rrModel.GetDataItemByValue(rrModel.Basin).LinkTo(dataItem);

            Assert.AreEqual(1, rrModel.GetAllModelData().Count());

            var pavedCatchment = new Catchment { CatchmentType = CatchmentType.Paved };
            rrModel.Basin.Catchments.Add(pavedCatchment);

            Assert.AreEqual(2, rrModel.GetAllModelData().Count());

            pavedCatchment.CatchmentType = CatchmentType.None;

            Assert.AreEqual(1, rrModel.GetAllModelData().Count());
        }

        [Test]
        public void SetToKnownCatchmentTypeAddsModelData()
        {
            var rrModel = new RainfallRunoffModel { Name = "Test" };
            var greenHouseCatchment = new Catchment { CatchmentType = CatchmentType.GreenHouse };
            var catchment = new Catchment { CatchmentType = CatchmentType.None };
            rrModel.Basin.Catchments.Add(greenHouseCatchment);
            rrModel.Basin.Catchments.Add(catchment);

            Assert.AreEqual(1, rrModel.GetAllModelData().Count());

            catchment.CatchmentType = CatchmentType.Paved;

            Assert.AreEqual(2, rrModel.GetAllModelData().Count());
        }

        [Test]
        public void RemoveCatchmentRemovesModelData()
        {
            var rrModel = new RainfallRunoffModel { Name = "Test" };
            rrModel.Basin.Catchments.Add(new Catchment { CatchmentType = CatchmentType.Paved });

            Assert.AreEqual(1, rrModel.GetAllModelData().Count());

            rrModel.Basin.Catchments.Remove(rrModel.Basin.Catchments.First());

            Assert.AreEqual(0, rrModel.GetAllModelData().Count());
        }

        [Test]
        public void SetBasinByDataItemShouldRefreshAreas()
        {
            var rrModel = new RainfallRunoffModel { Name = "Test" };
            rrModel.Basin.Catchments.Add(new Catchment { CatchmentType = CatchmentType.Paved });

            Assert.AreEqual(1, rrModel.GetAllModelData().Count());

            var dataItemWithBasin = rrModel.DataItems.FirstOrDefault(di => di.Value is IDrainageBasin);

            Assert.IsNotNull(dataItemWithBasin);

            var basin = new DrainageBasin();

            //test clear old data
            dataItemWithBasin.Value = basin;
            Assert.AreEqual(0, rrModel.GetAllModelData().Count());

            //test subscribe
            basin.Catchments.Add(new Catchment { CatchmentType = CatchmentType.Paved });

            Assert.AreEqual(1, rrModel.GetAllModelData().Count());
        }

        [Test]
        public void SetBasinByRelinkingDataItemShouldKeepData()
        {
            var basin = new DrainageBasin();
            var rrModel = new RainfallRunoffModel { Name = "Test" };
            var externalDataItem = new DataItem(basin, DataItemRole.Input);
            var rrBasinDataItem = rrModel.DataItems.First(di => di.Value is IDrainageBasin);

            // link to external
            rrBasinDataItem.LinkTo(externalDataItem);

            // add catchment
            rrModel.Basin.Catchments.Add(new Catchment { CatchmentType = CatchmentType.Paved });

            Assert.AreEqual(1, rrModel.GetAllModelData().Count());
            var pavedData = (PavedData)rrModel.ModelData[0];
            pavedData.NumberOfInhabitants = 123456;
            
            // create clone of external basin + dataitem
            var clonedBasin = (IDrainageBasin) rrModel.Basin.Clone();
            var clonedDataItem = new DataItem(clonedBasin, DataItemRole.Input);
            
            // relink basin data item
            rrBasinDataItem.LinkTo(clonedDataItem, true);
            
            // assert data still there
            Assert.AreEqual(1, rrModel.GetAllModelData().Count());
            Assert.AreEqual(123456, ((PavedData) rrModel.ModelData[0]).NumberOfInhabitants);

            // test subscribe
            rrModel.Basin.Catchments.Add(new Catchment { CatchmentType = CatchmentType.Paved });

            Assert.AreEqual(2, rrModel.GetAllModelData().Count());
        }
        
        [Test]
        public void SetBasinByPropertyShouldRefreshAreas()
        {
            var rrModel = new RainfallRunoffModel { Name = "Test" };

            rrModel.Basin.Catchments.Add(new Catchment { CatchmentType = CatchmentType.Paved });

            Assert.AreEqual(1, rrModel.GetAllModelData().Count());
            Assert.IsNotNull(rrModel.Basin);

            //test clear old data
            rrModel.Basin = new DrainageBasin();
            Assert.AreEqual(0, rrModel.GetAllModelData().Count());

            //test subscribe
            rrModel.Basin.Catchments.Add(new Catchment { CatchmentType = CatchmentType.Paved });

            Assert.AreEqual(1, rrModel.GetAllModelData().Count());
        } 
    }
}