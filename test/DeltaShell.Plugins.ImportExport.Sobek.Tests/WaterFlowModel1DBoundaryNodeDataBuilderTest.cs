using System.Data;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests
{
    [TestFixture]
    public class WaterFlowModel1DBoundaryNodeDataBuilderTest
    {
        private WaterFlowModel1DBoundaryNodeDataBuilder builder;

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            builder = new WaterFlowModel1DBoundaryNodeDataBuilder();
        }

        [Test]
        public void TestQh()
        {
            var hqTable = new DataTable();
                hqTable.Columns.Add("H", typeof(double));
                hqTable.Columns.Add("Q", typeof(double));

            var newRow = hqTable.NewRow();
            newRow[0] = 5;
            newRow[1] = 1;
            hqTable.Rows.Add(newRow);

            var condition = new SobekFlowBoundaryCondition
                                {
                                    StorageType = SobekFlowBoundaryStorageType.Qh,
                                    BoundaryType = SobekFlowBoundaryConditionType.Flow,
                                    FlowHqTable = hqTable
                                };

            var flowBoundaryNodeData = WaterFlowModel1DBoundaryNodeDataBuilder.ToFlowBoundaryNodeData(condition);

            Assert.AreEqual(WaterFlowModel1DBoundaryNodeDataType.FlowWaterLevelTable,flowBoundaryNodeData.DataType);
            Assert.AreEqual(1.0, flowBoundaryNodeData.Data[5.0]);
        }

        [Test]
        public void TestHq()
        {
            var qhTable = new DataTable();
            qhTable.Columns.Add("Q", typeof(double));
            qhTable.Columns.Add("H", typeof(double));

            var newRow = qhTable.NewRow();
            newRow[0] = 1;
            newRow[1] = 5;
            qhTable.Rows.Add(newRow);

            var condition = new SobekFlowBoundaryCondition
            {
                StorageType = SobekFlowBoundaryStorageType.Qh,
                BoundaryType = SobekFlowBoundaryConditionType.Level, 
                LevelQhTable = qhTable
            };

            // builder will convert to Q function of h
            var flowBoundaryNodeData = WaterFlowModel1DBoundaryNodeDataBuilder.ToFlowBoundaryNodeData(condition);

            Assert.AreEqual(WaterFlowModel1DBoundaryNodeDataType.FlowWaterLevelTable, flowBoundaryNodeData.DataType);
            Assert.AreEqual(1.0, flowBoundaryNodeData.Data[5.0]);

        }


    }
}