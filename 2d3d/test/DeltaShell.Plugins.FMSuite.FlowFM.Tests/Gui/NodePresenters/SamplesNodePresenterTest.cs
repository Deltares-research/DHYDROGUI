using System.Collections.Generic;
using System.Drawing;
using DelftTools.Controls;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Drawing;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.NodePresenters
{
    [TestFixture]
    public class SamplesNodePresenterTest
    {
        [Test]
        [TestCaseSource(nameof(GetArgumentNullCases))]
        public void Constructor_ArgumentNull_ThrowsException(GuiPlugin guiPlugin, SamplesImageProvider imageProvider)
        {
            // Call
            void Call() => new SamplesNodePresenter(guiPlugin, imageProvider);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void UpdateNode_SetsTextAndImage()
        {
            // Setup
            using (var guiPlugin = new FlowFMGuiPlugin())
            {
                var provider = new SamplesImageProvider();
                var nodePresenter = new SamplesNodePresenter(guiPlugin, provider);

                var parentNode = Substitute.For<ITreeNode>();
                var node = Substitute.For<ITreeNode>();
                var samples = new Samples(WaterFlowFMModelDefinition.InitialVelocityXName);

                // Call
                nodePresenter.UpdateNode(parentNode, node, samples);

                // Assert
                node.Received(1).Text = WaterFlowFMModelDefinition.InitialVelocityXName;
                node.Received(1).Image = Arg.Is<Image>(i => i.PixelsEqual(Resources.velocity_x));
            }
        }

        private static IEnumerable<TestCaseData> GetArgumentNullCases()
        {
            yield return new TestCaseData(null, new SamplesImageProvider());
            yield return new TestCaseData(new FlowFMGuiPlugin(), null);
        }
    }
}