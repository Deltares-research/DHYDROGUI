using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.FMSuite.Common.Gui.Forms;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.Forms
{
    [TestFixture]
    public class WaterFlowModel1DLateralSourceDataViewWpfTest
    {
        private Model1DLateralSourceData lateralSourceData;

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
            var view = new Model1DLateralSourceDataViewWpf { Data = lateralSourceData };
            WpfTestHelper.ShowModal(view);
        }

    }
}