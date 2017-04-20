using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms.PropertyGrid;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.Forms.PropertyGrid
{
    [TestFixture]
    public class WaterFlowModel1DBoundaryDataPropertiesTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowProperties()
        {
            WindowsFormsTestHelper.ShowPropertyGridForObject(new WaterFlowModel1DBoundaryNodeDataProperties { Data = new WaterFlowModel1DBoundaryNodeData() });
        }
    }
}