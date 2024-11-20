using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.FunctionListView;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.DataObjects;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.Forms.FunctionListViewTest
{
    [TestFixture]
    public class BloomFunctionsTableViewTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void OpenTableViewTest()
        {
            var functionListView = new BloomFunctionsTableView
            {
                Data = BloomInfoTest.CreateFunctionList(),
                BloomInfo = BloomInfoTest.CreateBloomInfo()
            };

            WindowsFormsTestHelper.ShowModal(functionListView);
        }
    }
}