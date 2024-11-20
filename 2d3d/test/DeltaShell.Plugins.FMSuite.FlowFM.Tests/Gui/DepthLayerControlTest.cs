using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.DepthLayers;
using DeltaShell.Plugins.FMSuite.Common.Gui.Editors;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    public class DepthLayerControlTest
    {
        [Test]
        public void Show()
        {
            var control = new DepthLayerControl {DepthLayerDefinition = new DepthLayerDefinition(DepthLayerType.Sigma, 0.5, 0.5)};

            WindowsFormsTestHelper.ShowModal(control);
        }
    }
}