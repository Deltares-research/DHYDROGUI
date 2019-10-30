using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.FMSuite.Common.Gui.Forms;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.Forms
{
    [TestFixture]
    public class WaterFlowModel1DBoundaryNodeDataViewWpfTest
    {
        private Model1DBoundaryNodeData boundaryNodeData;

        [SetUp]
        public void Setup()
        {
            boundaryNodeData = WaterFlowModel1DBoundaryAndLateralWpfViewHelper.GetStandardBoundaryNodeData();
        }

        [Category(TestCategory.WindowsForms)]
        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(true, true)]
        public void ShowUserControl(bool saltEnabled, bool temperatureEnabled)
        {
            boundaryNodeData.UseSalt = saltEnabled;
            boundaryNodeData.UseTemperature = temperatureEnabled;
            var view = new Model1DBoundaryNodeDataViewWpf() { Data = boundaryNodeData };
            WpfTestHelper.ShowModal(view);
        }

    }
}