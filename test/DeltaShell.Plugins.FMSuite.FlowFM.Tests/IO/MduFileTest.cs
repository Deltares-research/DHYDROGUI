using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
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
                var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
                mduFile.Read(mduFilePath, originalMD, originalArea, allFixedWeirsAndCorrespondingProperties);
                mduFile.Write(savePath, originalMD, originalArea, allFixedWeirsAndCorrespondingProperties, false);
                
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

        [Test]
        public void GivenAnMduToReadWithFixedWeirs_WhenTheSchemeNumbersRequiresMoreColumnsThanGivenInPlizFile_ThenAllMissingPropertiesShouldBeCreatedUsingTheDefaultValues()
        {
            mduFilePath = TestHelper.GetTestFilePath(@"HydroAreaCollection\FlowFMFixedWeirs\FlowFM2.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);
            mduDir = Path.GetDirectoryName(mduFilePath);
            Assert.NotNull(mduDir);
            modelName = Path.GetFileName(mduFilePath);

            saveDirectory = Path.Combine(mduDir, "MduFileReadsAndWritesTest");
            Directory.CreateDirectory(saveDirectory);
            savePath = Path.Combine(saveDirectory, "FlowFM2.mdu");
            newMduDir = Path.GetDirectoryName(savePath);
            Assert.NotNull(newMduDir);
            newMduName = Path.GetFileName(savePath);
            try
            {
                var mduFile = new MduFile();

                var originalArea = new HydroArea();
                var originalMD = new WaterFlowFMModelDefinition(mduDir, modelName);
                var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();
                mduFile.Read(mduFilePath, originalMD, originalArea, allFixedWeirsAndCorrespondingProperties);

                Assert.AreEqual(7, allFixedWeirsAndCorrespondingProperties[0].DataColumns.Count);

                //CrestLevel
                Assert.AreEqual(0, allFixedWeirsAndCorrespondingProperties[0].DataColumns[0].ValueList[0]);
                Assert.AreEqual(0, allFixedWeirsAndCorrespondingProperties[0].DataColumns[0].ValueList[1]);

                //Ground Height Left
                Assert.AreEqual(0, allFixedWeirsAndCorrespondingProperties[0].DataColumns[1].ValueList[0]);
                Assert.AreEqual(0, allFixedWeirsAndCorrespondingProperties[0].DataColumns[1].ValueList[1]);

                //Ground Height Right
                Assert.AreEqual(0, allFixedWeirsAndCorrespondingProperties[0].DataColumns[2].ValueList[0]);
                Assert.AreEqual(0, allFixedWeirsAndCorrespondingProperties[0].DataColumns[2].ValueList[1]);

                //Crest Width
                Assert.AreEqual(3, allFixedWeirsAndCorrespondingProperties[0].DataColumns[3].ValueList[0]);
                Assert.AreEqual(3, allFixedWeirsAndCorrespondingProperties[0].DataColumns[3].ValueList[1]);

                //Slope Left
                Assert.AreEqual(4, allFixedWeirsAndCorrespondingProperties[0].DataColumns[4].ValueList[0]);
                Assert.AreEqual(4, allFixedWeirsAndCorrespondingProperties[0].DataColumns[4].ValueList[1]);

                //Slope Right
                Assert.AreEqual(4, allFixedWeirsAndCorrespondingProperties[0].DataColumns[5].ValueList[0]);
                Assert.AreEqual(4, allFixedWeirsAndCorrespondingProperties[0].DataColumns[5].ValueList[1]);

                //Roughness Code
                Assert.AreEqual(0, allFixedWeirsAndCorrespondingProperties[0].DataColumns[6].ValueList[0]);
                Assert.AreEqual(0, allFixedWeirsAndCorrespondingProperties[0].DataColumns[6].ValueList[1]);

                mduFile.Write(savePath, originalMD, originalArea, allFixedWeirsAndCorrespondingProperties,false);

                var generatedResultsContent = File.ReadAllLines(@"FlowFMFixedWeirs\MduFileReadsAndWritesTest\TwoFixedWeirs_fxw2_fxw.pliz");

                var expectedResultsContent = 
                    new string[]{
                        "Weir01",
                        "    2    9",
                        "5.400000000000000E+000  4.600000000000000E+000  0.000000000000000E+000  0.000000000000000E+000  0.000000000000000E+000  3.000000000000000E+000  4.000000000000000E+000  4.000000000000000E+000  0.000000000000000E+000",
                        "1.200000000000000E+000  1.000000000000000E+001  0.000000000000000E+000  0.000000000000000E+000  0.000000000000000E+000  3.000000000000000E+000  4.000000000000000E+000  4.000000000000000E+000  0.000000000000000E+000",
                        "Weir02",
                        "    2    9",
                        "2.000000000000000E+000  7.000000000000000E-001  0.000000000000000E+000  0.000000000000000E+000  0.000000000000000E+000  3.000000000000000E+000  4.000000000000000E+000  4.000000000000000E+000  0.000000000000000E+000",
                        "3.900000000000000E+000  3.900000000000000E+000  0.000000000000000E+000  0.000000000000000E+000  0.000000000000000E+000  3.000000000000000E+000  4.000000000000000E+000  4.000000000000000E+000  0.000000000000000E+000"
                        };

                for (int i =0; i < 8; i++)
                {
                    Assert.AreEqual(expectedResultsContent[i], generatedResultsContent[i],
                        "Line " + (i + 1) + " of generated file " + savePath +
                        " differs from expected result");
                }

            }
            finally
            {
                FileUtils.DeleteIfExists(mduDir);
            }
        }

        [Test]
        public void GivenAnMduToReadWithFixedWeirs_WhenTheSchemeNumbersRequiresLessColumnsThanGivenInPlizFile_ThenOnlyTheNeededPropertiesShouldBeCreatedAfterReadingThePlizFile()
            {
                mduFilePath = TestHelper.GetTestFilePath(@"HydroAreaCollection\FlowFMFixedWeirs\FlowFM3.mdu");
                mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);
                mduDir = Path.GetDirectoryName(mduFilePath);
                Assert.NotNull(mduDir);
                modelName = Path.GetFileName(mduFilePath);

            saveDirectory = Path.Combine(mduDir, "MduFileReadsAndWritesTest");
            Directory.CreateDirectory(saveDirectory);
            savePath = Path.Combine(saveDirectory, "FlowFM3.mdu");
            newMduDir = Path.GetDirectoryName(savePath);
            Assert.NotNull(newMduDir);
            newMduName = Path.GetFileName(savePath);
            try
                {
                    var mduFile = new MduFile();

                    var originalArea = new HydroArea();
                    var originalMD = new WaterFlowFMModelDefinition(mduDir, modelName);
                    var allFixedWeirsAndCorrespondingProperties = new List<ModelFeatureCoordinateData<FixedWeir>>();

                    

                    mduFile.Read(mduFilePath, originalMD, originalArea, allFixedWeirsAndCorrespondingProperties);

                    Assert.AreEqual(3, allFixedWeirsAndCorrespondingProperties[0].DataColumns.Count);

                    //CrestLevel
                    Assert.AreEqual(1.2, allFixedWeirsAndCorrespondingProperties[0].DataColumns[0].ValueList[0]);
                    Assert.AreEqual(6.4, allFixedWeirsAndCorrespondingProperties[0].DataColumns[0].ValueList[1]);

                    //Ground Height Left
                    Assert.AreEqual(3.5, allFixedWeirsAndCorrespondingProperties[0].DataColumns[1].ValueList[0]);
                    Assert.AreEqual(3.0, allFixedWeirsAndCorrespondingProperties[0].DataColumns[1].ValueList[1]);

                    //Ground Height Right
                    Assert.AreEqual(3.2, allFixedWeirsAndCorrespondingProperties[0].DataColumns[2].ValueList[0]);
                    Assert.AreEqual(3.3, allFixedWeirsAndCorrespondingProperties[0].DataColumns[2].ValueList[1]);

                    mduFile.Write(savePath, originalMD, originalArea, allFixedWeirsAndCorrespondingProperties, false);

                    var generatedResultsContent = File.ReadAllLines(@"FlowFMFixedWeirs\MduFileReadsAndWritesTest\TwoFixedWeirs_fxw.pliz");

                    var expectedResultsContent =
                        new string[]{
                            "Weir01",
                            "    2    5",
                            "5.400000000000000E+000  4.600000000000000E+000  1.200000000000000E+000  3.500000000000000E+000  3.200000000000000E+000",
                            "1.200000000000000E+000  1.000000000000000E+001  6.400000000000000E+000  3.000000000000000E+000  3.300000000000000E+000",
                            "Weir02",
                            "    2    5",
                            "2.000000000000000E+000  7.000000000000000E-001  1.700000000000000E+000  4.500000000000000E+000  4.200000000000000E+000",
                            "3.900000000000000E+000  3.900000000000000E+000  6.100000000000000E+000  4.000000000000000E+000  4.300000000000000E+000"
                        };

                    for (int i = 0; i < 8; i++)
                    {
                        Assert.AreEqual(expectedResultsContent[i], generatedResultsContent[i],
                            "Line " + (i + 1) + " of generated file " + savePath +
                            " differs from expected result");
                    }
            }
                finally
                {
                    FileUtils.DeleteIfExists(mduDir);
                }
            }
    }
}