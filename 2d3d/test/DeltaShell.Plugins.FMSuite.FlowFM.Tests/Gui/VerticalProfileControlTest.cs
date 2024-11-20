using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.DepthLayers;
using DeltaShell.Plugins.FMSuite.Common.Gui.Editors;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    internal class VerticalProfileControlTest
    {
        [Test]
        public void Show()
        {
            WindowsFormsTestHelper.ShowModal(new VerticalProfileControl());
        }

        [Test]
        public void ShowWithSigmaDepthLayers()
        {
            var verticalProfileControl = new VerticalProfileControl
            {
                ModelDepthLayerDefinition = new DepthLayerDefinition(DepthLayerType.Sigma, 0.6, 0.15,
                                                                     0.25)
            };
            WindowsFormsTestHelper.ShowModal(verticalProfileControl);
        }

        [Test]
        public void ShowWithZDepthLayers()
        {
            var verticalProfileControl = new VerticalProfileControl
            {
                ModelDepthLayerDefinition = new DepthLayerDefinition(DepthLayerType.Z, 2, 3,
                                                                     1)
            };
            WindowsFormsTestHelper.ShowModal(verticalProfileControl);
        }
    }
}