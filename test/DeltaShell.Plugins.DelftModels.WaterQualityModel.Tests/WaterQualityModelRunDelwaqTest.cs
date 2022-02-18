using System.IO;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests
{
    [TestFixture]
    public class WaterQualityModelRunDelwaqTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)] // TOOLS-22280
        public void RunDelwaqModelWithDoubleAliasEntries()
        {
            string dataDir = TestHelper.GetTestDataDirectory();
            string hydFile = Path.Combine(dataDir, "ValidWaqModels", "FM", "FlowFM.hyd");
            
            using (var model = new WaterQualityModel())
            {
                new HydFileImporter().ImportItem(hydFile, model);
                model.Boundaries[0].LocationAliases = "load 1, load 1"; // add double alias

                string subFilePath = TestHelper.GetTestFilePath(@"ValidWaqModels\coli_04.sub");
                new SubFileImporter().Import(model.SubstanceProcessLibrary, subFilePath);

                // Send the model to delwaq
                ActivityRunner.RunActivity(model);

                // Assert that a model can run fine in delwaq when it writes a double statement of
                // USEDATA_ITEM 'load 1' FORITEM 'sea_002.pli' 'sea_002.pli'
                // I would like to assert some more, but the activity runner doesn't let me.
                Assert.AreEqual(ActivityStatus.Cleaned, model.Status);
            }
        }
    }
}