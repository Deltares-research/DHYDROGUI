using System.Linq;
using DelftTools.Controls.Swf.DataEditorGenerator.Metadata;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    public class WaterFlowFMGuiPropertyExtractorTest
    {
        [Test]
        public void CheckExtraction()
        {
            var model = new WaterFlowFMModel();

            var extractor = new WaterFlowFMGuiPropertyExtractor(model);
            ObjectUIDescription objectDescription = extractor.ExtractObjectDescription(new string[0]);

            Assert.Greater(objectDescription.FieldDescriptions.Count, 65);
            Assert.IsNotNull(objectDescription.FieldDescriptions.FirstOrDefault(f => f.Name == "Icgsolver"));
            Assert.IsNotNull(objectDescription.FieldDescriptions.FirstOrDefault(f => f.Label == "Sediment/Morphology"));
        }

        [Test]
        public void CheckTooltips()
        {
            var model = new WaterFlowFMModel();

            var extractor = new WaterFlowFMGuiPropertyExtractor(model);
            ObjectUIDescription objectDescription = extractor.ExtractObjectDescription(new string[0]);

            Assert.Greater(objectDescription.FieldDescriptions.Count, 65);
            string bedLevelTooltip = objectDescription.FieldDescriptions.First(f => f.Name == "Bedlevuni").ToolTip;
            Assert.AreEqual("Uniform bed level used at missing z values if BedlevType > 2", bedLevelTooltip);
        }
    }
}