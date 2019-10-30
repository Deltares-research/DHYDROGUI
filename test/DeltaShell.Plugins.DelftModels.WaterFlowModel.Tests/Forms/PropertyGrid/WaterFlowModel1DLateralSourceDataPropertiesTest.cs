using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms.PropertyGrid;
using DeltaShell.Plugins.FMSuite.Common.Gui.Forms;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.Forms.PropertyGrid
{
    [TestFixture]
    public class WaterFlowModel1DLateralSourceDataPropertiesTest
    {
        [Test, Category(TestCategory.WindowsForms)]
        public void ShowProperties()
        {
            WindowsFormsTestHelper.ShowPropertyGridForObject(new Model1DLateralDataProperties { Data = new Model1DLateralSourceData() });
        }
    }
}