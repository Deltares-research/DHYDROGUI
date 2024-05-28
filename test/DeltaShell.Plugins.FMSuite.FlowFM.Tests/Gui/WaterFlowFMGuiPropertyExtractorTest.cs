using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class WaterFlowFMGuiPropertyExtractorTest
    {
        [Test]
        public void CheckTooltips()
        {
            var model = new WaterFlowFMModel();

            var extractor = new WaterFlowFMGuiPropertyExtractor(model);
            var objectDescription = extractor.ExtractObjectDescription(new string[0]);

            Assert.Greater(objectDescription.FieldDescriptions.Count, 65);
            string bedLevelTooltip = objectDescription.FieldDescriptions.First(f => f.Name == "Bedlevuni").ToolTip;
            Assert.AreEqual("Mdu name: Bedlevuni\r\nDescription:\r\nUniform bed level used at missing z values if BedlevType > 2\r\n", bedLevelTooltip);
        }
    }
}