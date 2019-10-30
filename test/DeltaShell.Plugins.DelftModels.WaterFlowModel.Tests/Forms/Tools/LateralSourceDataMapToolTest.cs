using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.DataObjects;
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

            lateralSourceDataMapTool.LateralSourceData = new List<Model1DLateralSourceData>();

            // No exceptions should be thrown
            TypeUtils.CallPrivateMethod(lateralSourceDataMapTool, "TurnSelectedLateralsIntoQBoundary", new object[] { null, null });
            TypeUtils.CallPrivateMethod(lateralSourceDataMapTool, "TurnSelectedLateralsIntoQTimeSeries", new object[] { null, null });
            TypeUtils.CallPrivateMethod(lateralSourceDataMapTool, "TurnSelectedLateralsIntoQHTable", new object[] { null, null });

            lateralSourceDataMapTool.LateralSourceData = new List<Model1DLateralSourceData>
                                                       {
                                                           new Model1DLateralSourceData { DataType = Model1DLateralDataType.FlowTimeSeries },
                                                           new Model1DLateralSourceData { DataType = Model1DLateralDataType.FlowWaterLevelTable }
                                                       };

            TypeUtils.CallPrivateMethod(lateralSourceDataMapTool, "TurnSelectedLateralsIntoQBoundary", new object[] { null, null });
            Assert.AreEqual(Model1DLateralDataType.FlowConstant, lateralSourceDataMapTool.LateralSourceData.ElementAt(0).DataType);
            Assert.AreEqual(Model1DLateralDataType.FlowConstant, lateralSourceDataMapTool.LateralSourceData.ElementAt(1).DataType);

            TypeUtils.CallPrivateMethod(lateralSourceDataMapTool, "TurnSelectedLateralsIntoQTimeSeries", new object[] { null, null });
            Assert.AreEqual(Model1DLateralDataType.FlowTimeSeries, lateralSourceDataMapTool.LateralSourceData.ElementAt(0).DataType);
            Assert.AreEqual(Model1DLateralDataType.FlowTimeSeries, lateralSourceDataMapTool.LateralSourceData.ElementAt(1).DataType);

            TypeUtils.CallPrivateMethod(lateralSourceDataMapTool, "TurnSelectedLateralsIntoQHTable", new object[] { null, null });
            Assert.AreEqual(Model1DLateralDataType.FlowWaterLevelTable, lateralSourceDataMapTool.LateralSourceData.ElementAt(0).DataType);
            Assert.AreEqual(Model1DLateralDataType.FlowWaterLevelTable, lateralSourceDataMapTool.LateralSourceData.ElementAt(1).DataType);
        }

        [Test]
        public void LateralSourceDataMapToolCreateContextMenuTest()
        {
            var mocks = new MockRepository();
            var mapcontrol = mocks.Stub<IMapControl>();
            var lateralSource1 = mocks.Stub<LateralSource>();
            var lateralSource2 = mocks.Stub<LateralSource>();
            var lateralSourceData1 = mocks.Stub<Model1DLateralSourceData>();
            var lateralSourceData2 = mocks.Stub<Model1DLateralSourceData>();
            var lateralSourceDataMapTool = new LateralSourceDataMapTool { MapControl = mapcontrol };

            lateralSourceData1.Feature = lateralSource1;
            lateralSourceData2.Feature = lateralSource2;
            
            mocks.ReplayAll();

            // Without lateral data
            var items = lateralSourceDataMapTool.GetContextMenuItems(null);
            Assert.AreEqual(0, items.Count());

            // With empty lateral data
            mapcontrol.SelectedFeatures = new List<Model1DLateralSourceData>();
            
            items = lateralSourceDataMapTool.GetContextMenuItems(null);
            Assert.AreEqual(0, items.Count());
            
            // With lateral data
            mapcontrol.SelectedFeatures = new List<Model1DLateralSourceData> { lateralSourceData1, lateralSourceData2 };

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
