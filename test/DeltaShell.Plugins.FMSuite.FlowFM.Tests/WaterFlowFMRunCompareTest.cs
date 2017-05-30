using System.Diagnostics;
using System.IO;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.NetCdf;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class WaterFlowFMRunCompareTest
    {
        [Test]
        [Category("DIMR_Introduction")]
        [Category(TestCategory.WorkInProgress)]
        public void ImportHarlingenRunAndCompareTest()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            var mduFilePath = TestHelper.CreateLocalCopy(mduPath);
            var localMduDir = Path.GetDirectoryName(mduFilePath);
            var exportMduPath = Path.Combine(localMduDir, "export");
            
            try
            {
                if (Directory.Exists(exportMduPath))
                    Directory.Delete(exportMduPath, true);
            }
            catch (IOException)
            {
                // failed to delete.. sometimes happens on build server, let's retry once:
                exportMduPath += "2";
                if (Directory.Exists(exportMduPath))
                    Directory.Delete(exportMduPath, true);
            }

            Directory.CreateDirectory(exportMduPath);
            var exportedMduFile = Path.Combine(exportMduPath, Path.GetFileName(mduFilePath));

            var model = new WaterFlowFMModel(mduFilePath);
            model.ExportTo(exportedMduFile, false);
            
            // run both
            RunUnstruc(exportedMduFile);
            ActivityRunner.RunActivity(model);
            Assert.AreEqual(ActivityStatus.Cleaned, model.Status);

            // get waterlevels from file
            var mapFile = Path.Combine(exportMduPath, @"DFM_OUTPUT_har\001_map.nc");
            NetCdfFile ncFile = null;
            int[] shape = null;
            try
            {
                ncFile = NetCdfFile.OpenExisting(mapFile);
                var ncVar = ncFile.GetVariableByName("s1");
                shape = ncFile.GetShape(ncVar);
            }
            finally
            {
                if (ncFile != null) ncFile.Close();
            }

            // todo: we want to compare numerical results here, but first need to upgrade fm dll
            Assert.AreEqual(model.OutputWaterLevel.Arguments[0].Values.Count, shape[0], "nr. of time steps");
            Assert.AreEqual(model.OutputWaterLevel.Arguments[1].Values.Count, shape[1], "nr. of water levels");
        }

        private static void RunUnstruc(string localMduFile)
        {
            var unstrucBatchScript = TestHelper.GetTestFilePath(@"unstruc\dflowfm.bat");
            var process = new Process();
            process.StartInfo.FileName = unstrucBatchScript;
            process.StartInfo.Arguments = Path.GetDirectoryName(unstrucBatchScript) + " "
                                          + Path.GetDirectoryName(localMduFile) + " "
                                          + Path.GetFileName(localMduFile);
            process.Start();
            process.WaitForExit(120000); // 2 min. tops
        }
    }
}