using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Core;
using DeltaShell.NGHS.IO.FileWriters.SpatialData;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport;
using DeltaShell.Plugins.ImportExport.Sobek;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.IntegrationTests
{
    [TestFixture]
    public class WFM1DDsProjTest
    {
        [TestFixture]
        public class CrossSectionsFromDsProjTest
        {
            private DeltaShellApplication app;

            [SetUp]
            public void SetUp()
            {
                LogHelper.ConfigureLogging();
                LogHelper.SetLoggingLevel(Level.Info);

                app = GetRunningDSApplication();
            }

            [TearDown]
            public void TearDown()
            {
                app.Dispose();
                LogHelper.SetLoggingLevel(Level.Error);
            }

            [Test]
            [Category(TestCategory.Integration)]
            public void WriteModelFromDsProjAndReadFromFiles()
            {
                string testDataDirName = "WFM1D";
                string sourcePath =
                    Path.GetFullPath(Path.Combine(TestHelper.GetDataDir(), @"FileWriters\IntegrationTests",
                        testDataDirName));
                FileUtils.CopyDirectory(sourcePath, testDataDirName, ".svn");

                string dsProjPath = Path.Combine(testDataDirName, "TestModel_for_FileReaders.dsproj");
                app.OpenProject(dsProjPath);

                WaterFlowModel1D waterFlowModel1D = app.Project.RootFolder.Models.OfType<WaterFlowModel1D>().ToList()[0];
                //string targetPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                string targetPath = Path.Combine(Path.GetTempPath(), "FileWriters");
                
                var modelFilename = Path.Combine(targetPath, ModelFileNames.ModelDefinitionFilename);
                
                WaterFlowModel1DFileWriter.Write(modelFilename, waterFlowModel1D);
                var readModel = WaterFlowModel1DFileReader.Read(modelFilename, (s,c,t)=>{Console.WriteLine(@"Step : {0} ({1} / {2})", s, c, t);});
                Assert.NotNull(readModel);
                
                ////PLEASE REMOVE THIS WHEN MODEL PARAMETER READER IS DONE:
                readModel.StartTime = waterFlowModel1D.StartTime;
                readModel.UseSalt = true;
                readModel.InitialSaltConcentration.DefaultValue = 0;
                readModel.DispersionCoverage.DefaultValue = 0;

                var sfbModelFilename = Path.Combine(targetPath + "_SFB", ModelFileNames.ModelDefinitionFilename);
                WaterFlowModel1DFileWriter.Write(sfbModelFilename, readModel);

                var readModelAgain = WaterFlowModel1DFileReader.Read(sfbModelFilename, (s, c, t) => { Console.WriteLine(@"Step : {0} ({1} / {2})", s, c, t); });
                Assert.NotNull(readModelAgain);
                
                // now do some file compare!

                var generatedFromDsProjFiles = Directory.GetFiles(targetPath, "*", SearchOption.TopDirectoryOnly);
                var generatedFromSFBFilesFiles = Directory.GetFiles(targetPath+ "_SFB", "*", SearchOption.TopDirectoryOnly);

                //Assert.AreEqual(generatedFromDsProjFiles.Length, generatedFromSFBFilesFiles.Length);
                
                
                for (int i = 0; i < generatedFromSFBFilesFiles.Length; i++)
                {
                    var filename = Path.GetFileName(generatedFromSFBFilesFiles[i]);
                    var directoryName = Path.GetDirectoryName(generatedFromDsProjFiles[0]);

                    if (directoryName == null || filename == null) continue;

                    filename = Path.Combine(directoryName, filename);
                    var generated_DsProj = generatedFromDsProjFiles.FirstOrDefault(sfb => sfb == filename);

                    // TODO: the following exceptions need to be removed once all read/write functionality has been implemented
                    if (generated_DsProj != null && !generated_DsProj.Contains("roughness") &&
                        !generated_DsProj.Contains("Retention") && !generated_DsProj.Contains("Definition") &&
                        !generated_DsProj.Contains("Structure") && !generated_DsProj.Contains("Dispersion") && !generated_DsProj.Contains("F3") &&
                        !generated_DsProj.Contains("Reversed") && !generated_DsProj.Contains("Initial") &&
                        !generated_DsProj.Contains("BoundaryConditions") &&
                        !generated_DsProj.Contains("BoundaryLocations") && !generated_DsProj.Contains("WindShielding"))
                    {
                        string errorMessage;
                        Console.WriteLine(
                            @"Comparing from dsproj model generated file {0} with from SFB model generated file {1}",
                            generated_DsProj, generatedFromSFBFilesFiles[i]);
                        Assert.IsTrue(
                            FileComparer.Compare(generatedFromSFBFilesFiles[i], generated_DsProj,
                                out errorMessage, true),
                            string.Format(
                                "Generated file from dsproj {0} does not match file generated from sfb file import {1}! : {2}{3}",
                                generated_DsProj, generatedFromSFBFilesFiles[i], Environment.NewLine, errorMessage));
                    }
                }
            }
            
            [Test]
            [Category(TestCategory.Integration)]
            public void GivenModelWhenWriteToSFBThenSFBFilesShouldBeCreated()
            {
                string testDataDirName = "WFM1D";
                string sourcePath =
                    Path.GetFullPath(Path.Combine(TestHelper.GetDataDir(), @"FileWriters\IntegrationTests",
                        testDataDirName));
                FileUtils.CopyDirectory(sourcePath, testDataDirName, ".svn");

                string dsProjPath = Path.Combine(testDataDirName, "TestModel_for_FileReaders.dsproj");
                app.OpenProject(dsProjPath);

                WaterFlowModel1D waterFlowModel1D = app.Project.RootFolder.Models.OfType<WaterFlowModel1D>().ToList()[0];
                //string targetPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                string targetPath = Path.Combine(Path.GetTempPath(), "FileWriters");
                
                var modelFilename = Path.Combine(targetPath, ModelFileNames.ModelDefinitionFilename);
                
                WaterFlowModel1DFileWriter.Write(modelFilename, waterFlowModel1D);
                // now do some file compare!

                var generatedFiles = Directory.GetFiles(targetPath, "*", SearchOption.TopDirectoryOnly);
                
                var modelFileNames = new ModelFileNames(modelFilename);

                Assert.IsTrue(generatedFiles.Contains(modelFilename), "Sobek has not created file + " + modelFilename + " needed for the cf dll");
                Assert.IsTrue(generatedFiles.Contains(modelFileNames.Network), "Sobek has not created file + " + modelFileNames.Network + " needed for the cf dll");
                Assert.IsTrue(generatedFiles.Contains(modelFileNames.BoundaryConditions), "Sobek has not created file + " + modelFileNames.BoundaryConditions + " needed for the cf dll");
                Assert.IsTrue(generatedFiles.Contains(modelFileNames.BoundaryLocations), "Sobek has not created file + " + modelFileNames.BoundaryLocations + " needed for the cf dll");
                Assert.IsTrue(generatedFiles.Contains(modelFileNames.CrossSectionDefinitions), "Sobek has not created file + " + modelFileNames.CrossSectionDefinitions + " needed for the cf dll");
                Assert.IsTrue(generatedFiles.Contains(modelFileNames.CrossSectionLocations), "Sobek has not created file + " + modelFileNames.CrossSectionLocations + " needed for the cf dll");
                Assert.IsTrue(generatedFiles.Contains(modelFileNames.LateralDischarge), "Sobek has not created file + " + modelFileNames.LateralDischarge + " needed for the cf dll");
                //Assert.IsTrue(generatedFiles.Contains(modelFileNames.LogFile), "Sobek has not created file + " + modelFileNames.LogFile + " needed for the cf dll");
                Assert.IsTrue(generatedFiles.Contains(modelFileNames.ObservationPoints), "Sobek has not created file + " + modelFileNames.ObservationPoints + " needed for the cf dll");
                Assert.IsTrue(generatedFiles.Contains(modelFileNames.Retention), "Sobek has not created file + " + modelFileNames.Retention + " needed for the cf dll");
                Assert.IsTrue(generatedFiles.Contains(modelFileNames.SobekSim), "Sobek has not created file + " + modelFileNames.SobekSim + " needed for the cf dll");
                Assert.IsTrue(generatedFiles.Contains(modelFileNames.Structures), "Sobek has not created file + " + modelFileNames.Structures + " needed for the cf dll");
                
                // Check if roughness files are generated
                foreach (var roughnessFile in modelFileNames.RoughnessFiles)
                {
                    Assert.IsTrue(generatedFiles.Contains(roughnessFile), "Sobek has not created roughness file + " + roughnessFile + " needed for the cf dll");
                }
                
                // Check if spatial data files are generated
                CheckGeneratedSpatialDataFilesFromSFBModel(waterFlowModel1D, generatedFiles, targetPath);
            }

            private static void CheckGeneratedSpatialDataFilesFromSFBModel(WaterFlowModel1D waterFlowModel1D, string[] generatedFromSFBFiles, string targetPath)
            {
                var spatialDataFilenames = new List<string>();
                
                switch (waterFlowModel1D.InitialConditionsType)
                {
                    case InitialConditionsType.WaterLevel:
                        spatialDataFilenames.Add(Path.Combine(targetPath, SpatialDataFileNames.InitialWaterLevel));
                        break;
                    case InitialConditionsType.Depth:
                        spatialDataFilenames.Add(Path.Combine(targetPath, SpatialDataFileNames.InitialWaterDepth));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (waterFlowModel1D.UseSalt) 
                {
                    spatialDataFilenames.Add(Path.Combine(targetPath, SpatialDataFileNames.InitialSalinity));
                    spatialDataFilenames.Add(Path.Combine(targetPath, SpatialDataFileNames.Dispersion));
                }

                spatialDataFilenames.Add(Path.Combine(targetPath, SpatialDataFileNames.InitialDischarge));
                spatialDataFilenames.Add(Path.Combine(targetPath, SpatialDataFileNames.WindShielding));

                foreach (var spatialDataFilename in spatialDataFilenames)
                {
                    Assert.IsTrue(generatedFromSFBFiles.Contains(spatialDataFilename), "Sobek has not created file + " + spatialDataFilename + " needed for the cf dll");
                }
            }

            private static DeltaShellApplication GetRunningDSApplication()
            {
                // make sure log4net is initialized
                var app = new DeltaShellApplication
                {
                    IsProjectCreatedInTemporaryDirectory = false,
                    IsDataAccessSynchronizationDisabled = true,
                    ScriptRunner = {SkipDefaultLibraries = true}
                };

                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());
                app.Plugins.Add(new SobekImportApplicationPlugin());

                app.Run();
                
                return app;
            }
        }
    }
}