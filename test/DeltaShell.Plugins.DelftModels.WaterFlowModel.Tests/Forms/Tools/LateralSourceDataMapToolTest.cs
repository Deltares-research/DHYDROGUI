using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
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
    public class  LateralSourceDataMapToolTest
    {
        [Test]
        public void LateralSourceDataMapToolExecuteTest()
        {
            var lateralSourceDataMapTool = new LateralSourceDataMapTool();

            // No exceptions should be thrown
            TypeUtils.CallPrivateMethod(lateralSourceDataMapTool, "TurnSelectedLateralsIntoQBoundary", new object[] { null, null });
            TypeUtils.CallPrivateMethod(lateralSourceDataMapTool, "TurnSelectedLateralsIntoQTimeSeries", new object[] { null, null });
            TypeUtils.CallPrivateMethod(lateralSourceDataMapTool, "TurnSelectedLateralsIntoQHTable", new object[] { null, null });

            lateralSourceDataMapTool.LateralSourceData = new List<WaterFlowModel1DLateralSourceData>();

            // No exceptions should be thrown
            TypeUtils.CallPrivateMethod(lateralSourceDataMapTool, "TurnSelectedLateralsIntoQBoundary", new object[] { null, null });
            TypeUtils.CallPrivateMethod(lateralSourceDataMapTool, "TurnSelectedLateralsIntoQTimeSeries", new object[] { null, null });
            TypeUtils.CallPrivateMethod(lateralSourceDataMapTool, "TurnSelectedLateralsIntoQHTable", new object[] { null, null });

            lateralSourceDataMapTool.LateralSourceData = new List<WaterFlowModel1DLateralSourceData>
                                                       {
                                                           new WaterFlowModel1DLateralSourceData { DataType = WaterFlowModel1DLateralDataType.FlowTimeSeries },
                                                           new WaterFlowModel1DLateralSourceData { DataType = WaterFlowModel1DLateralDataType.FlowWaterLevelTable }
                                                       };

            TypeUtils.CallPrivateMethod(lateralSourceDataMapTool, "TurnSelectedLateralsIntoQBoundary", new object[] { null, null });
            Assert.AreEqual(WaterFlowModel1DLateralDataType.FlowConstant, lateralSourceDataMapTool.LateralSourceData.ElementAt(0).DataType);
            Assert.AreEqual(WaterFlowModel1DLateralDataType.FlowConstant, lateralSourceDataMapTool.LateralSourceData.ElementAt(1).DataType);

            TypeUtils.CallPrivateMethod(lateralSourceDataMapTool, "TurnSelectedLateralsIntoQTimeSeries", new object[] { null, null });
            Assert.AreEqual(WaterFlowModel1DLateralDataType.FlowTimeSeries, lateralSourceDataMapTool.LateralSourceData.ElementAt(0).DataType);
            Assert.AreEqual(WaterFlowModel1DLateralDataType.FlowTimeSeries, lateralSourceDataMapTool.LateralSourceData.ElementAt(1).DataType);

            TypeUtils.CallPrivateMethod(lateralSourceDataMapTool, "TurnSelectedLateralsIntoQHTable", new object[] { null, null });
            Assert.AreEqual(WaterFlowModel1DLateralDataType.FlowWaterLevelTable, lateralSourceDataMapTool.LateralSourceData.ElementAt(0).DataType);
            Assert.AreEqual(WaterFlowModel1DLateralDataType.FlowWaterLevelTable, lateralSourceDataMapTool.LateralSourceData.ElementAt(1).DataType);
        }

        [Test]
        public void LateralSourceDataMapToolCreateContextMenuTest()
        {
            var mocks = new MockRepository();
            var mapcontrol = mocks.Stub<IMapControl>();
            var lateralSource1 = mocks.Stub<LateralSource>();
            var lateralSource2 = mocks.Stub<LateralSource>();
            var lateralSourceData1 = mocks.Stub<WaterFlowModel1DLateralSourceData>();
            var lateralSourceData2 = mocks.Stub<WaterFlowModel1DLateralSourceData>();
            var lateralSourceDataMapTool = new LateralSourceDataMapTool { MapControl = mapcontrol };

            lateralSourceData1.Feature = lateralSource1;
            lateralSourceData2.Feature = lateralSource2;
            
            mocks.ReplayAll();

            // Without lateral data
            var items = lateralSourceDataMapTool.GetContextMenuItems(null);
            Assert.AreEqual(0, items.Count());

            // With empty lateral data
            mapcontrol.SelectedFeatures = new List<WaterFlowModel1DLateralSourceData>();
            
            items = lateralSourceDataMapTool.GetContextMenuItems(null);
            Assert.AreEqual(0, items.Count());
            
            // With lateral data
            mapcontrol.SelectedFeatures = new List<WaterFlowModel1DLateralSourceData> { lateralSourceData1, lateralSourceData2 };

            items = lateralSourceDataMapTool.GetContextMenuItems(null);
            Assert.AreEqual(1, items.Count());

            var subItems = items.First().MenuItem.DropDownItems;
            Assert.AreEqual(3, subItems.Count);

            Assert.IsTrue(subItems[0].Enabled);
            Assert.IsTrue(subItems[1].Enabled);
            Assert.IsTrue(subItems[2].Enabled);
        }
    }
}
