using System.IO;
using System.Text;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
    public class FlowFMNetFileImporterTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ImportItem_ForANewGrid_ShouldMarkOutputOutOfSync()
        {
            using (var tempDirectory = new TemporaryDirectory())
            using (var model = new WaterFlowFMModel())
            {
                // Arrange
                string restartFilePath = Path.Combine(tempDirectory.Path, "test_rst.nc");
                const string text = "This is some text in the file.";

                using (FileStream fs = File.Create(restartFilePath))
                {
                    byte[] info = new UTF8Encoding(true).GetBytes(text);
                    fs.Write(info, 0, info.Length);
                }

                
                model.ImportFromMdu(TestHelper.GetTestFilePath(@"chezy_samples\chezy.mdu"));
                model.ConnectOutput(tempDirectory.Path);

                // Act
                new FlowFMNetFileImporter().ImportItem(TestHelper.GetTestFilePath(@"harlingen\fm_003_net.nc"), model);

                // Assert
                Assert.IsTrue(model.OutputOutOfSync);
            }
        }

        
    }
}