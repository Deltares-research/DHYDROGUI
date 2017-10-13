using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using GeoAPI.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport
{
    [TestFixture]
    public class WaterFlowModelFileWriterTest
    {
        [Test]
        public void TestFileWriter_CopiesMorphologyFilesToWorkingDirectory()
        {
            var testDir = Path.Combine(Path.GetTempPath(), TestHelper.GetCurrentMethodName());

            using (var waterFlowModel1D = new WaterFlowModel1D()
            {
                // use a valid network for the calculation
                Network = HydroNetworkHelper.GetSnakeHydroNetwork(1),
                ExplicitWorkingDirectory = Path.Combine(testDir, "workDir"),
                UseMorphology = true,
                AdditionalMorphologyOutput = true,
            })
            {
                try
                {
                    // setup
                    FileUtils.DeleteIfExists(waterFlowModel1D.ExplicitWorkingDirectory);
                    FileUtils.CreateDirectoryIfNotExists(waterFlowModel1D.ExplicitWorkingDirectory);

                    var morFileInfo = new FileInfo(Path.Combine(testDir, @"SomeDir\Default.mor"));
                    FileUtils.CreateDirectoryIfNotExists(morFileInfo.DirectoryName);
                    using (File.Create(morFileInfo.FullName)) ;
                    waterFlowModel1D.MorphologyPath = morFileInfo.FullName;

                    Assert.NotNull(morFileInfo.DirectoryName);
                    var bcmFileInfo = new FileInfo(Path.Combine(morFileInfo.DirectoryName, @"SubDir\Default.bcm"));
                    FileUtils.CreateDirectoryIfNotExists(bcmFileInfo.DirectoryName);
                    using (File.Create(bcmFileInfo.FullName)) ;
                    waterFlowModel1D.BcmPath = bcmFileInfo.FullName;
                
                    var sedFileInfo = new FileInfo(Path.Combine(testDir, @"SomeDir\Default.sed"));
                    FileUtils.CreateDirectoryIfNotExists(sedFileInfo.DirectoryName);
                    using (File.Create(sedFileInfo.FullName)) ;
                    waterFlowModel1D.SedimentPath = sedFileInfo.FullName;
                
                    Assert.NotNull(sedFileInfo.DirectoryName);
                    var traFileInfo = new FileInfo(Path.Combine(sedFileInfo.DirectoryName, @"..\Default.tra"));
                    FileUtils.CreateDirectoryIfNotExists(traFileInfo.DirectoryName);
                    using (File.Create(traFileInfo.FullName)) ;
                    waterFlowModel1D.TraPath = traFileInfo.FullName;

                    // export
                    var targetPath = Path.Combine(waterFlowModel1D.WorkingDirectory, "FileWriters");
                    WaterFlowModel1DFileWriter.Write(Path.Combine(targetPath, ModelFileNames.ModelDefinitionFilename), waterFlowModel1D);

                    // check results 
                    // (MOR and SED files should be copied to target directory)
                    // (BCM and TRA files should be copied relative to MOR and SED files respectively)
                    Assert.IsTrue(File.Exists(Path.Combine(targetPath, "Default.mor")));
                    Assert.IsTrue(File.Exists(Path.Combine(targetPath, @"SubDir\Default.bcm")));
                    Assert.IsTrue(File.Exists(Path.Combine(targetPath, "Default.sed")));
                    Assert.IsTrue(File.Exists(Path.Combine(targetPath, @"..\Default.tra")));
                }
                finally
                {
                    FileUtils.DeleteIfExists(waterFlowModel1D.ExplicitWorkingDirectory);
                }
            }
        }

        [Test, Category(TestCategory.Integration)]
        public void TestFileWriterCrossSectionDefinitions()
        {
            using (var waterFlowModel1D = new WaterFlowModel1D()
            {
                // use a valid network for the calculation
                Network = HydroNetworkHelper.GetSnakeHydroNetwork(1),
                ExplicitWorkingDirectory = Path.Combine(Path.GetTempPath(),Path.GetRandomFileName())

            })
            {
                waterFlowModel1D.Network.CrossSectionSectionTypes.Add(new CrossSectionSectionType
                {
                    Name = CrossSectionDefinitionZW.Floodplain1SectionTypeName
                });
                waterFlowModel1D.Network.CrossSectionSectionTypes.Add(new CrossSectionSectionType
                {
                    Name = CrossSectionDefinitionZW.Floodplain2SectionTypeName
                });
                var branch = waterFlowModel1D.Network.Channels.FirstOrDefault();
                Assert.NotNull(branch, "No branched added to the network");
                
                var offsets = new double[] {0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100};
                
                HydroNetworkHelper.GenerateDiscretization(waterFlowModel1D.NetworkDiscretization, branch, offsets);
                try
                {
                    waterFlowModel1D.Initialize();
                }
                catch
                {
                    // ignored, because we are only interested in the generated files, not the working of the wfm1d
                }

                var relativePathCrossSectionDefinitionsExpectedFile =
                    TestHelper.GetTestFilePath(
                        @"FileWriters/WaterFlowModelFileWriterTest_Integration_CrossSectionDefinition_expected.txt");
                var relativePathCrossSectionLocationsExpectedFile =
                    TestHelper.GetTestFilePath(
                        @"FileWriters/WaterFlowModelFileWriterTest_Integration_CrossSectionLocation_expected.txt");
                var yzCoordinates1 = new List<Coordinate>
                                    {
                                        new Coordinate(0.0, 0.0),
                                        new Coordinate(1.0, 0.11)
                                    };
                CrossSectionHelper.AddYZCrossSectionFromYZCoordinates(branch, 0.0, yzCoordinates1);

                // add cross-sections
                var width1 = Math.Sqrt(800.0)*2.0;
                
                var heightWidthFlowData1 = new List<HeightFlowStorageWidth>
                {
                    new HeightFlowStorageWidth(0.0, width1 - 5.0, width1 - 5.0),
                    new HeightFlowStorageWidth(10.0, width1, width1)
                };

                var csDef = new CrossSectionDefinitionZW(){};
                csDef.Sections.Add(new CrossSectionSection() { SectionType = new CrossSectionSectionType() { Name = "Main" } });
                csDef.ZWDataTable.Set(heightWidthFlowData1);
                
                var cs = HydroNetworkHelper.AddCrossSectionDefinitionToBranch(branch, csDef, 2.0);
                cs.Name = "CrossSection2";
                
                var targetPath = Path.Combine(waterFlowModel1D.WorkingDirectory, "FileWriters");
                WaterFlowModel1DFileWriter.Write(Path.Combine(targetPath, ModelFileNames.ModelDefinitionFilename), waterFlowModel1D);

                string errorMessage;
                var relativePathActualCrossSectionDefinitionFile = Path.Combine(targetPath, new ModelFileNames().CrossSectionDefinitions);
                Assert.IsTrue(
                    FileComparer.Compare(relativePathCrossSectionDefinitionsExpectedFile, relativePathActualCrossSectionDefinitionFile,
                        out errorMessage, true),
                    string.Format("Generated CrossSectionDefinitions file does not match template!{0}{1}",
                        Environment.NewLine, errorMessage));

                var relativePathActualCrossSectionLocationFile = Path.Combine(targetPath,
                    new ModelFileNames().CrossSectionLocations);
                Assert.IsTrue(
                    FileComparer.Compare(relativePathCrossSectionLocationsExpectedFile, relativePathActualCrossSectionLocationFile,
                        out errorMessage, true),
                    string.Format("Generated Cross Section Locations file does not match template!{0}{1}",
                        Environment.NewLine, errorMessage));
               
            }

        } 
    }
}