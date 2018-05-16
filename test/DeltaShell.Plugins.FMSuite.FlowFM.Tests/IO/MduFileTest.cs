using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class MduFileTest
    {
        private string mduFilePath;
        private string mduDir;
        private string modelName;
        private string saveDirectory;
        private string savePath;
        private string newMduDir;
        private string newMduName;

        [TestCase(@"TestModelWithNcInSubFolder\trynet.mdu", "Sub\\gridtry.nc")]
        [TestCase(@"TestModelWithoutNcInSubFolder\trynet.mdu", "gridtry.nc")]
        public void GivenModelForImporting_WhenNcFileIsInSubFolder_ThenNcFileShouldBeAgainInSubFolder(string relativeMduFilePath, string relativeNcFilePath)
        {
            mduFilePath = TestHelper.GetTestFilePath(relativeMduFilePath);
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);
            mduDir = Path.GetDirectoryName(mduFilePath);
            Assert.NotNull(mduDir);
            modelName = Path.GetFileName(mduFilePath);

            saveDirectory = Path.Combine(mduDir, "MduFileReadsAndWritesTest");
            Directory.CreateDirectory(saveDirectory);
            savePath = Path.Combine(saveDirectory, "trynet.mdu");
            newMduDir = Path.GetDirectoryName(savePath);
            Assert.NotNull(newMduDir);
            newMduName = Path.GetFileName(savePath);
            try
            {
                var mduFile = new MduFile();

                var originalArea = new HydroArea();
                var originalMD = new WaterFlowFMModelDefinition(mduDir, modelName);
                mduFile.Read(mduFilePath, originalMD, originalArea);
                mduFile.Write(savePath, originalMD, originalArea, false);
                
                var netFileLocationShouldBe = Path.Combine(newMduDir, relativeNcFilePath);

                Assert.IsTrue(File.Exists(netFileLocationShouldBe));
            }
            finally
            {
                FileUtils.DeleteIfExists(mduDir);
            }
        }
    }
}