using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms.Tools;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap.UI.Forms;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.Forms.Tools
{
    [TestFixture]
    public class BoundaryNodeDataMapToolTest
    {
        [Test]
        public void BoundaryNodeDataMapToolExecuteTest()
        {
            var boundaryNodeDataMapTool = new BoundaryNodeDataMapTool();

            // No exceptions should be thrown
            TypeUtils.CallPrivateMethod(boundaryNodeDataMapTool, "TurnSelectedNodesIntoNone", new object[] { null, null });
            TypeUtils.CallPrivateMethod(boundaryNodeDataMapTool, "TurnSelectedNodesIntoHBoundary", new object[] { null, null });
            TypeUtils.CallPrivateMethod(boundaryNodeDataMapTool, "TurnSelectedNodesIntoHTimeSeries", new object[] { null, null });
            TypeUtils.CallPrivateMethod(boundaryNodeDataMapTool, "TurnSelectedNodesIntoQBoundary", new object[] { null, null });
            TypeUtils.CallPrivateMethod(boundaryNodeDataMapTool, "TurnSelectedNodesIntoQTimeSeries", new object[] { null, null });
            TypeUtils.CallPrivateMethod(boundaryNodeDataMapTool, "TurnSelectedNodesIntoQHTable", new object[] { null, null });

            boundaryNodeDataMapTool.BoundaryNodeData = new List<WaterFlowModel1DBoundaryNodeData>();

            // No exceptions should be thrown
            TypeUtils.CallPrivateMethod(boundaryNodeDataMapTool, "TurnSelectedNodesIntoNone", new object[] { null, null });
            TypeUtils.CallPrivateMethod(boundaryNodeDataMapTool, "TurnSelectedNodesIntoHBoundary", new object[] { null, null });
            TypeUtils.CallPrivateMethod(boundaryNodeDataMapTool, "TurnSelectedNodesIntoHTimeSeries", new object[] { null, null });
            TypeUtils.CallPrivateMethod(boundaryNodeDataMapTool, "TurnSelectedNodesIntoQBoundary", new object[] { null, null });
            TypeUtils.CallPrivateMethod(boundaryNodeDataMapTool, "TurnSelectedNodesIntoQTimeSeries", new object[] { null, null });
            TypeUtils.CallPrivateMethod(boundaryNodeDataMapTool, "TurnSelectedNodesIntoQHTable", new object[] { null, null });

            boundaryNodeDataMapTool.BoundaryNodeData = new List<WaterFlowModel1DBoundaryNodeData>
                                                       {
                                                           new WaterFlowModel1DBoundaryNodeData { DataType = WaterFlowModel1DBoundaryNodeDataType.FlowWaterLevelTable },
                                                           new WaterFlowModel1DBoundaryNodeData { DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries }
                                                       };

            TypeUtils.CallPrivateMethod(boundaryNodeDataMapTool, "TurnSelectedNodesIntoNone", new object[] { null, null });
            Assert.AreEqual(WaterFlowModel1DBoundaryNodeDataType.None, boundaryNodeDataMapTool.BoundaryNodeData.ElementAt(0).DataType);
            Assert.AreEqual(WaterFlowModel1DBoundaryNodeDataType.None, boundaryNodeDataMapTool.BoundaryNodeData.ElementAt(1).DataType);

            TypeUtils.CallPrivateMethod(boundaryNodeDataMapTool, "TurnSelectedNodesIntoHBoundary", new object[] { null, null });
            Assert.AreEqual(WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant, boundaryNodeDataMapTool.BoundaryNodeData.ElementAt(0).DataType);
            Assert.AreEqual(WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant, boundaryNodeDataMapTool.BoundaryNodeData.ElementAt(1).DataType);

            TypeUtils.CallPrivateMethod(boundaryNodeDataMapTool, "TurnSelectedNodesIntoHTimeSeries", new object[] { null, null });
            Assert.AreEqual(WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries, boundaryNodeDataMapTool.BoundaryNodeData.ElementAt(0).DataType);
            Assert.AreEqual(WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries, boundaryNodeDataMapTool.BoundaryNodeData.ElementAt(1).DataType);

            TypeUtils.CallPrivateMethod(boundaryNodeDataMapTool, "TurnSelectedNodesIntoQBoundary", new object[] { null, null });
            Assert.AreEqual(WaterFlowModel1DBoundaryNodeDataType.FlowConstant, boundaryNodeDataMapTool.BoundaryNodeData.ElementAt(0).DataType);
            Assert.AreEqual(WaterFlowModel1DBoundaryNodeDataType.FlowConstant, boundaryNodeDataMapTool.BoundaryNodeData.ElementAt(1).DataType);

            TypeUtils.CallPrivateMethod(boundaryNodeDataMapTool, "TurnSelectedNodesIntoQTimeSeries", new object[] { null, null });
            Assert.AreEqual(WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries, boundaryNodeDataMapTool.BoundaryNodeData.ElementAt(0).DataType);
            Assert.AreEqual(WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries, boundaryNodeDataMapTool.BoundaryNodeData.ElementAt(1).DataType);

            TypeUtils.CallPrivateMethod(boundaryNodeDataMapTool, "TurnSelectedNodesIntoQHTable", new object[] { null, null });
            Assert.AreEqual(WaterFlowModel1DBoundaryNodeDataType.FlowWaterLevelTable, boundaryNodeDataMapTool.BoundaryNodeData.ElementAt(0).DataType);
            Assert.AreEqual(WaterFlowModel1DBoundaryNodeDataType.FlowWaterLevelTable, boundaryNodeDataMapTool.BoundaryNodeData.ElementAt(1).DataType);
        }

        [Test]
        public void BoundaryNodeDataMaptToolCreateContextMenuWithoutBoundaryData()
        {
            MockRepository mocks;
            WaterFlowModel1DBoundaryNodeData boundaryNodeData1;
            WaterFlowModel1DBoundaryNodeData boundaryNodeData2;
            WaterFlowModel1DBoundaryNodeData boundaryNodeData3;
            var boundaryNodeDataMapTool = CreateBoundaryNodeDataMapTool(out mocks, out boundaryNodeData1, out boundaryNodeData2, out boundaryNodeData3);

            // Without boundary data
            var items = boundaryNodeDataMapTool.GetContextMenuItems(null);

            Assert.AreEqual(0, items.Count());

            mocks.VerifyAll();
        }

        [Test]
        public void BoundaryNodeDataMapToolCreateContextMenuForEmptyBoundaryData()
        {
            MockRepository mocks;
            WaterFlowModel1DBoundaryNodeData boundaryNodeData1;
            WaterFlowModel1DBoundaryNodeData boundaryNodeData2;
            WaterFlowModel1DBoundaryNodeData boundaryNodeData3;
            var boundaryNodeDataMapTool = CreateBoundaryNodeDataMapTool(out mocks, out boundaryNodeData1, out boundaryNodeData2, out boundaryNodeData3);

            // With empty boundary data
            boundaryNodeDataMapTool.MapControl.SelectedFeatures = new List<WaterFlowModel1DBoundaryNodeData>();

            var items = boundaryNodeDataMapTool.GetContextMenuItems(null);

            Assert.AreEqual(0, items.Count());

            mocks.VerifyAll();
        }

        [Test]
        public void BoundaryNodeDataMapToolCreateContextMenuForHqBoundary()
        {
            MockRepository mocks;
            WaterFlowModel1DBoundaryNodeData boundaryNodeData1;
            WaterFlowModel1DBoundaryNodeData boundaryNodeData2;
            WaterFlowModel1DBoundaryNodeData boundaryNodeData3;
            var boundaryNodeDataMapTool = CreateBoundaryNodeDataMapTool(out mocks, out boundaryNodeData1, out boundaryNodeData2, out boundaryNodeData3);

            // With Q/H boundary data
            boundaryNodeDataMapTool.MapControl.SelectedFeatures = new List<WaterFlowModel1DBoundaryNodeData> { boundaryNodeData1, boundaryNodeData3 };

            var items = boundaryNodeDataMapTool.GetContextMenuItems(null);
            Assert.AreEqual(1, items.Count());

            var subItems = items.First().MenuItem.DropDownItems;

            Assert.AreEqual(6, subItems.Count);
            Assert.IsTrue(subItems[0].Enabled);
            Assert.IsTrue(subItems[1].Enabled);
            Assert.IsTrue(subItems[2].Enabled);
            Assert.IsTrue(subItems[3].Enabled);
            Assert.IsTrue(subItems[4].Enabled);
            Assert.IsTrue(subItems[5].Enabled);

            mocks.VerifyAll();
        }

        [Test]
        public void BoundaryNodeDataMapToolCreateContextMenuForHBoundaryOnly()
        {
            MockRepository mocks;
            WaterFlowModel1DBoundaryNodeData boundaryNodeData1;
            WaterFlowModel1DBoundaryNodeData boundaryNodeData2;
            WaterFlowModel1DBoundaryNodeData boundaryNodeData3;
            var boundaryNodeDataMapTool = CreateBoundaryNodeDataMapTool(out mocks, out boundaryNodeData1, out boundaryNodeData2, out boundaryNodeData3);

            // With H only boundary data
            boundaryNodeDataMapTool.MapControl.SelectedFeatures = new List<WaterFlowModel1DBoundaryNodeData> { boundaryNodeData1, boundaryNodeData2, boundaryNodeData3 };

            var items = boundaryNodeDataMapTool.GetContextMenuItems(null);
            Assert.AreEqual(1, items.Count());

            var subItems = items.First().MenuItem.DropDownItems;

            Assert.AreEqual(6, subItems.Count);
            Assert.IsTrue(subItems[0].Enabled);
            Assert.IsTrue(subItems[1].Enabled);
            Assert.IsTrue(subItems[2].Enabled);
            Assert.IsFalse(subItems[3].Enabled);
            Assert.IsFalse(subItems[4].Enabled);
            Assert.IsFalse(subItems[5].Enabled);

            mocks.VerifyAll();
        }

        private static BoundaryNodeDataMapTool CreateBoundaryNodeDataMapTool(out MockRepository mocks,
                                                                             out WaterFlowModel1DBoundaryNodeData boundaryNodeData1,
                                                                             out WaterFlowModel1DBoundaryNodeData boundaryNodeData2,
                                                                             out WaterFlowModel1DBoundaryNodeData boundaryNodeData3)
        {
            mocks = new MockRepository();

            var mapcontrol = mocks.Stub<IMapControl>();
            
            var node1 = mocks.Stub<HydroNode>();
            var node2 = mocks.Stub<HydroNode>();
            var node3 = mocks.Stub<HydroNode>();
            boundaryNodeData1 = mocks.Stub<WaterFlowModel1DBoundaryNodeData>();
            boundaryNodeData2 = mocks.Stub<WaterFlowModel1DBoundaryNodeData>();
            boundaryNodeData3 = mocks.Stub<WaterFlowModel1DBoundaryNodeData>();
            var boundaryNodeDataMapTool = new BoundaryNodeDataMapTool{MapControl = mapcontrol};

            node1.Stub(n => n.IsConnectedToMultipleBranches).Return(false);
            node2.Stub(n => n.IsConnectedToMultipleBranches).Return(true);
            node3.Stub(n => n.IsConnectedToMultipleBranches).Return(false);
            boundaryNodeData1.Feature = node1;
            boundaryNodeData2.Feature = node2;
            boundaryNodeData3.Feature = node3;

            mocks.ReplayAll();
            return boundaryNodeDataMapTool;
        }
    }
}