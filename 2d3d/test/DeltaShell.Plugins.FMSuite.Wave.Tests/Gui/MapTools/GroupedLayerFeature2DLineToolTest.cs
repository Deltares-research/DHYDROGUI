using System.Drawing;
using DeltaShell.Plugins.FMSuite.Wave.Gui.MapTools;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.MapTools
{
    [TestFixture]
    public class GroupedLayerFeature2DLineToolTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            const string groupLayer = "groupLayer";
            const string childVectoryLayer = "childVectorLayer";
            const string name = "groupedTool";
            var icon = new Bitmap(10, 10);

            // Call
            var lineTool = new GroupedLayerFeature2DLineTool(groupLayer,
                                                             childVectoryLayer,
                                                             name,
                                                             icon);

            // Assert
            Assert.That(lineTool.Name, Is.EqualTo(name),
                        "Expected a different Name.");
            Assert.That(lineTool.LayerName, Is.EqualTo(groupLayer),
                        "Expected a different LayerName.");
        }
    }
}