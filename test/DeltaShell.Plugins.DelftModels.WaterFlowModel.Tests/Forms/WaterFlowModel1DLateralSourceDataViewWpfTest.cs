using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.Forms
{
    [TestFixture]
    public class WaterFlowModel1DLateralSourceDataViewWpfTest
    {
        private WaterFlowModel1DLateralSourceData lateralSourceData;

        [SetUp]
        public void Setup()
        {
            lateralSourceData = WaterFlowModel1DBoundaryAndLateralWpfViewHelper.GetStandardLateralSourceData();
        }

        [Category(TestCategory.WindowsForms)]
        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(true, true)]
        public void ShowUserControl(bool saltEnabled, bool temperatureEnabled)
        {
            lateralSourceData.UseSalt = saltEnabled;
            lateralSourceData.UseTemperature = temperatureEnabled;
            var view = new WaterFlowModel1DLateralSourceDataViewWpf { Data = lateralSourceData };
            WpfTestHelper.ShowModal(view);
        }

    }
}