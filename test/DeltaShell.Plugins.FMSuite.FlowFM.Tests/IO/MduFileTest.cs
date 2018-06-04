using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using log4net.Util;
using NUnit.Framework;
using SharpMap.Extensions.CoordinateSystems;

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
        [TestCase(@"TestModelWithNcInSubFolderAndDefaultNames\trynet.mdu", "Sub\\trynet_net.nc")]
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

        [TestCase(@"cs_after_save\before_save_AmersfoortRDNew_net.nc", 28992, "Amersfoort / RD New")]
        [TestCase(@"cs_after_save\before_save_AmersfoortRDOld_net.nc", 28991, "Amersfoort / RD Old")]
        [TestCase(@"cs_after_save\before_save_UTMzone30N_net.nc", 32630, "WGS 84 / UTM zone 30N")]
        public void SetCoordinateSystemNameNetfileWithModelCoordinateSystemNameTest(string netFile, int espgModel, string expectedCoordinateSystemName)
        {
            var workingDirectory = FileUtils.CreateTempDirectory();
            var coordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(espgModel);

            var netFilePath = TestHelper.GetTestFilePath(netFile);
            var netFileInfo = new FileInfo(netFilePath);
            Assert.IsTrue(netFileInfo.Exists);

            var workingNetFilePath = Path.Combine(workingDirectory, netFileInfo.Name);
            var workingNetFileInfo = new FileInfo(workingNetFilePath);
            FileUtils.CopyFile(netFileInfo.FullName, workingNetFilePath);
            Assert.IsTrue(workingNetFileInfo.Exists);

            var mduFile = new MduFile();
            var fileProjectedName = TypeUtils.CallPrivateMethod<string>(mduFile, "GetProjectedCoordinateSystemNameFromNetFile", workingNetFilePath);
            Assert.That(fileProjectedName, Is.EqualTo("Unknown projected"));

            UnstructuredGridFileHelper.SetCoordinateSystem(workingNetFilePath, coordinateSystem);

            var editedFileProjectedName = TypeUtils.CallPrivateMethod<string>(mduFile, "GetProjectedCoordinateSystemNameFromNetFile", workingNetFilePath);
            Assert.IsTrue(editedFileProjectedName.Equals(expectedCoordinateSystemName));

            FileUtils.DeleteIfExists(workingDirectory);
        }

        [TestCase(true, @"update_CS_netfile\amersfoortRDNew_net.nc", 28992, true)]
        [TestCase(true, @"update_CS_netfile\unknown_projected_net.nc", 28992, false)]
        [TestCase(true, @"update_CS_netfile\wgs84_net.nc", 4326 , true)]

        [TestCase(false, @"update_CS_netfile\amersfoortRDNew_net.nc", 28992, true)]
        [TestCase(false, @"update_CS_netfile\unknown_projected_net.nc", 28992, true)]
        [TestCase(false, @"update_CS_netfile\wgs84_net.nc", 4326, true)]

        [TestCase(true, @"update_CS_netfile\amersfoortRDNew_net.nc", 28991, false)]
        [TestCase(true, @"update_CS_netfile\unknown_projected_net.nc", 28991, false)]
        [TestCase(true, @"update_CS_netfile\wgs84_net.nc", 4326, true)]

        [TestCase(false, @"update_CS_netfile\amersfoortRDNew_net.nc", 28991, true)]
        [TestCase(false, @"update_CS_netfile\unknown_projected_net.nc", 28991, true)]

        [TestCase(true, @"update_CS_netfile\wgs84_net.nc", 28992, false)]
        [TestCase(true, @"update_CS_netfile\amersfoortRDNew_net.nc", 4326, false)]

        public void ShouldUpdateNetfileCoordinateSystemTest(bool hasCoordinateSystem, string targetFile, int epsgModelDefinition, bool expected)
        {
            var netFilePath = TestHelper.GetTestFilePath(targetFile);
            var netFileInfo = new FileInfo(netFilePath);
            Assert.IsTrue(netFileInfo.Exists);

            var modelDefinition = new WaterFlowFMModelDefinition();
            var mduFile = new MduFile();
            if (hasCoordinateSystem == true)
                modelDefinition.CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(epsgModelDefinition);
      
            var result = TypeUtils.CallPrivateMethod<bool>(mduFile, "IsNetfileCoordinateSystemUpToDate", modelDefinition, netFilePath);

            Assert.That(result, Is.EqualTo(expected));
        }
    }
}