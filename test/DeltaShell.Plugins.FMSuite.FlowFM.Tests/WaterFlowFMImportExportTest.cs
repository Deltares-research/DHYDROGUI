using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.Core;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap;
using SharpMap.Extensions.CoordinateSystems;
using SharpMapTestUtils;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class WaterFlowFMImportExportTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ImportModelWithSedimentSpatiallyVaryingOperations()
        {
            if (Map.CoordinateSystemFactory == null)
                Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
            /* This test is relevant because when we are importing a model we do not load the state from the DB
              so it could happen the Spatially Varying operations are not loaded. */
            var mduPath = TestHelper.GetTestFilePath(@"spatially_varying_sediment_properties_in_model\FlowFM.mdu");
            var localMduFilePath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(localMduFilePath);

            var fraction = model.SedimentFractions.FirstOrDefault(sf => sf.Name == "gouwe");
            Assert.IsNotNull(fraction);
            var spatvaryingProp =
                fraction.CurrentSedimentType.Properties.FirstOrDefault(p => p.Name == "IniSedThick") as
                    ISpatiallyVaryingSedimentProperty;
            Assert.IsNotNull(spatvaryingProp);
            Assert.IsTrue(spatvaryingProp.IsSpatiallyVarying);
            var dataItem = model.DataItems.FirstOrDefault(di => di.Name == "gouwe_IniSedThick");
            Assert.IsNotNull(dataItem);
            var coverage = dataItem.Value as UnstructuredGridCellCoverage;
            Assert.IsNotNull(coverage);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        //[Ignore("outofmemory")]
        [Category("Quarantine")]
        public void ModelImportTestDcsm()
        {
            var mduPath = TestHelper.GetTestFilePath(@"dcsm\par16.mdu");
            var localMduFilePath = TestHelper.CreateLocalCopy(mduPath);
            
            var model = new WaterFlowFMModel(localMduFilePath);

            Assert.IsNotNull(model);
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void ExportOutputCoverage()
        {
            if(Map.CoordinateSystemFactory == null)
                Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            var localMduFilePath = TestHelper.CreateLocalCopy(mduPath);
            var localMduDir = Path.GetDirectoryName(localMduFilePath);

            var model = new WaterFlowFMModel(localMduFilePath);

            ActivityRunner.RunActivity(model);

            var exporter = new CoverageFileExporter();

            var exportDir = Path.Combine(localMduDir,"export");
            FileUtils.CreateDirectoryIfNotExists(exportDir, true);

            Assert.IsTrue(exporter.Export(model.OutputWaterLevel, Path.Combine(exportDir,"test.nc")));
        }

        [Test]
        public void ExportImportAssertUseTemperatureIsSetCorrectly()
        {
            var waterFlowFMModel = new WaterFlowFMModel();
            waterFlowFMModel.ModelDefinition.GetModelProperty(KnownProperties.Temperature).SetValueAsString("3");
            const string dir = "temptest";
            Directory.CreateDirectory(dir);
            const string mduFileName = "excesstemp.mdu";
            var mduPath = Path.Combine(Path.GetFullPath(dir), mduFileName);
            waterFlowFMModel.ExportTo(mduPath);
            var importedModel = new WaterFlowFMModel(mduPath);
            Assert.IsTrue(importedModel.UseTemperature);
        }

        [TestCase(false)]
        [TestCase(true)]
        [Category(TestCategory.DataAccess)]
        public void GivenTimFile_WhenImportingMeteorologicalDataFromFile_ThenImportResultIsAsExpected(bool useSolarRadiation)
        {
            var originalTimFilePath = TestHelper.GetTestFilePath(Path.Combine("timFiles", "FlowFM_MeteoData.tim"));
            var timFilePath = TestHelper.CreateLocalCopySingleFile(originalTimFilePath);

            try
            {
                var newFmModel = new WaterFlowFMModel();
                newFmModel.ModelDefinition.HeatFluxModel.Type = HeatFluxModelType.Composite;
                // set model date to previously used default (01-01-2001)
                var modelDefinition = newFmModel.ModelDefinition;
                modelDefinition.SetModelProperty(KnownProperties.RefDate, "2001-01-01");

                if (useSolarRadiation) newFmModel.ModelDefinition.HeatFluxModel.ContainsSolarRadiation = true;

                // Import exported meteo data
                new TimFile().Read(timFilePath, newFmModel.ModelDefinition.HeatFluxModel.MeteoData,
                    newFmModel.ReferenceTime);
                var meteoData = newFmModel.ModelDefinition.HeatFluxModel.MeteoData;

                var expectedValuesList = new[]
                {
                    new[] {1.0, 5.0, 9.0},
                    new[] {2.0, 6.0, 10.0},
                    new[] {3.0, 7.0, 11.0},
                    new[] {4.0, 8.0, 12.0}
                };

                // Check argument names
                var arguments = meteoData.Arguments;
                Assert.That(arguments.Count, Is.EqualTo(1));
                Assert.That(arguments[0].Name, Is.EqualTo("Time"));

                // Check argument values
                var timeValues = meteoData.Arguments[0].GetValues<DateTime>().ToArray();
                Assert.That(timeValues,
                    Is.EqualTo(new[]
                        {new DateTime(2001, 1, 1), new DateTime(2001, 1, 1, 12, 0, 0), new DateTime(2001, 1, 2)}));

                // Check component names
                var componentNames = meteoData.Components.Select(comp => comp.Name).ToArray();
                var list = new List<string> { "Humidity", "Air temperature", "Cloud coverage" };
                if (useSolarRadiation) list.Add("Solar radiation");
                Assert.That(componentNames, Is.EqualTo(list.ToArray()));

                // Check component values
                var numOfComponents = useSolarRadiation ? 4 : 3;
                for (var i = 0; i < numOfComponents; i++)
                {
                    Assert.That(meteoData.Components[i].GetValues<double>().ToArray(),
                        Is.EqualTo(expectedValuesList[i]));
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(timFilePath);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        [Category(TestCategory.DataAccess)]
        public void GivenWaterFlowFmModel_WhenWritingModelMeteoData_ThenTimeSeriesFileIsWrittenInTheRightOrder(bool useSolarRadiation)
        {
            var tempDir = FileUtils.CreateTempDirectory();
            var timFilePath = Path.Combine(tempDir, "meteoData.tim");
            try
            {
                // Create fm model with meteo data and export to tim file
                var fmModel = GetWaterFlowFmModelWithMeteoData(useSolarRadiation);
                new TimFile().Write(timFilePath, fmModel.ModelDefinition.HeatFluxModel.MeteoData, fmModel.ReferenceTime);

                // Read tim file content and check if the result is as expected
                var expectedLines = new[]
                {
                    "0.0000000e+00 1.0000000e+00 2.0000000e+00 3.0000000e+00" + (useSolarRadiation ? " 4.0000000e+00": string.Empty),
                    "1.2000000e+01 2.0000000e+00 3.0000000e+00 4.0000000e+00" + (useSolarRadiation ? " 5.0000000e+00": string.Empty),
                    "2.4000000e+01 5.0000000e+00 6.0000000e+00 7.0000000e+00" + (useSolarRadiation ? " 8.0000000e+00": string.Empty)
                };
                var writtenLinesInFile = File.ReadAllLines(timFilePath);
                Assert.That(writtenLinesInFile.Length, Is.EqualTo(expectedLines.Length));
                for (var lineNumber = 0; lineNumber < writtenLinesInFile.Length; lineNumber++)
                {
                    Assert.That(writtenLinesInFile[lineNumber], Is.EqualTo(expectedLines[lineNumber]),
                        $"Written time series file is unequal at line {lineNumber}");
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(tempDir);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        [Category(TestCategory.DataAccess)]
        public void GivenWaterFlowFmModel_WhenExportingImportingMeteoData_ThenMeteoDataIsCorrect(bool useSolarRadiation)
        {
            var tempDir = FileUtils.CreateTempDirectory();
            var timFilePath = Path.Combine(tempDir, "meteoData.tim");
            try
            {
                var fmModel = GetWaterFlowFmModelWithMeteoData(useSolarRadiation);
                var meteoData = fmModel.ModelDefinition.HeatFluxModel.MeteoData;

                // Export meteo data
                var timFileImporterExporter = new TimFile();
                timFileImporterExporter.Write(timFilePath, meteoData, fmModel.ReferenceTime);

                // Create new fm model
                var newFmModel = new WaterFlowFMModel();
                newFmModel.ModelDefinition.HeatFluxModel.Type = HeatFluxModelType.Composite;
                if (useSolarRadiation) newFmModel.ModelDefinition.HeatFluxModel.ContainsSolarRadiation = true;

                // Import exported meteo data
                timFileImporterExporter.Read(timFilePath, newFmModel.ModelDefinition.HeatFluxModel.MeteoData, newFmModel.ReferenceTime);
                var importedMeteoData = newFmModel.ModelDefinition.HeatFluxModel.MeteoData;
                
                // Check that meteo data before and after write/read are equal
                Assert.That(importedMeteoData.Arguments.Count, Is.EqualTo(meteoData.Arguments.Count));
                Assert.That(importedMeteoData.Components.Count, Is.EqualTo(meteoData.Components.Count));
                Assert.That(importedMeteoData.Arguments[0].Values, Is.EqualTo(meteoData.Arguments[0].Values));
                var numOfComponents = useSolarRadiation ? 4 : 3;
                for (var i = 0; i < numOfComponents; i++)
                {
                    Assert.That(importedMeteoData.Components[i].Name, Is.EqualTo(meteoData.Components[i].Name));
                    ListTestUtils.AssertAreEqual(importedMeteoData.Components[i].GetValues<double>(), meteoData.Components[i].GetValues<double>(), 1e-10);
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(tempDir);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Integration)]
        public void Test_Export_WaterFlowFmModel_WithPillarBridges()
        {
            var tempDir = FileUtils.CreateTempDirectory();
            var exportPath = Path.Combine(tempDir, "testBridgePillars.mdu");
            exportPath = TestHelper.CreateLocalCopy(exportPath);
            FileUtils.DeleteIfExists(exportPath);
            try
            {
                var fmModel = new WaterFlowFMModel();

                #region Set Pillar and DataModel

                var pillar = new BridgePillar()
                {
                    Name = "BridgePillarTest",
                    Geometry =
                        new LineString(new[]
                        {
                            new Coordinate(20.0, 60.0, 0),
                            new Coordinate(140.0, 8.0, 1.0),
                            new Coordinate(180.0, 4.0, 2.0),
                            new Coordinate(260.0, 0.0, 3.0)
                        }),
                };

                /* Set data model */
                var modelFeatureCoordinateDatas = fmModel.BridgePillarsDataModel;
                modelFeatureCoordinateDatas.Clear();
                var modelFeatureCoordinateData = new ModelFeatureCoordinateData<BridgePillar>() { Feature = pillar };
                modelFeatureCoordinateData.UpdateDataColumns();
                //Diameters
                modelFeatureCoordinateData.DataColumns[0].ValueList = new List<double> { 1.0, 2.5, 5.0, 10.0 };
                //DragCoefficient
                modelFeatureCoordinateData.DataColumns[1].ValueList = new List<double> { 10.0, 5.0, 2.5, 1.0 };

                modelFeatureCoordinateDatas.Add(modelFeatureCoordinateData);
                MduFile.SetBridgePillarAttributes(fmModel.Area.BridgePillars, modelFeatureCoordinateDatas);
                /* Done only for testing purposes. 
                 * Please do not attempt to do this without the supervision of another adult. */
                TypeUtils.SetPrivatePropertyValue(fmModel, "BridgePillarsDataModel", modelFeatureCoordinateDatas);

                #endregion
                fmModel.Area.BridgePillars.Add(pillar);
                Assert.IsTrue(fmModel.ExportTo(exportPath));
                Assert.IsTrue(File.Exists(exportPath));

                var bridgeFile = exportPath.Replace(".mdu",".pliz");
                Assert.IsTrue(File.Exists(bridgeFile));

                //Check contents of the file
                var readLines = File.ReadAllLines(bridgeFile);
                var expectedLines = new List<string>
                {
                    "BridgePillarTest",
                    "    4    4",
                    "2.000000000000000E+001  6.000000000000000E+001  1.000000000000000E+000  1.000000000000000E+001",
                    "1.400000000000000E+002  8.000000000000000E+000  2.500000000000000E+000  5.000000000000000E+000",
                    "1.800000000000000E+002  4.000000000000000E+000  5.000000000000000E+000  2.500000000000000E+000",
                    "2.600000000000000E+002  0.000000000000000E+000  1.000000000000000E+001  1.000000000000000E+000"
                };

                var idx = 0;
                foreach (var textLine in readLines)
                {
                    var expectedLine = expectedLines[idx];
                    Assert.AreEqual(expectedLine, textLine);
                    idx++;
                }

            }
            finally
            {
                FileUtils.DeleteIfExists(exportPath);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Integration)]
        public void GivenAnFMModelWithMorpholy_WhenExporting_ThenOnlyOneSetOfMorphologyFilesIsExported()
        {
            var tempDirPath = FileUtils.CreateTempDirectory();
            var tempProjectFilePath = Path.Combine(tempDirPath, "Project.dsproj");
            var tempMduFilePath = Path.Combine(tempDirPath, "FlowFM.mdu");

            var exportDirPath = FileUtils.CreateTempDirectory();
            var exportMduFilePath = Path.Combine(exportDirPath, "exported.mdu");

            try
            {
                using (var app = GetConfiguredApplication(tempProjectFilePath))
                {
                    using (var model = new WaterFlowFMModel() {MduFilePath = tempDirPath})
                    {
                        model.Grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2);
                        var cellsValue = ((int)UGridFileHelper.BedLevelLocation.Faces).ToString();
                        model.ModelDefinition.GetModelProperty(KnownProperties.BedlevType).SetValueAsString(cellsValue);

                        model.ModelDefinition.GetModelProperty(GuiProperties.UseMorSed).Value = true;
                        model.SedimentFractions.Add(new SedimentFraction { Name = "Fraction" });
                        
                        model.ExportTo(tempMduFilePath);
                        model.ReloadGrid(true, true);
                    }

                    using (var model = new WaterFlowFMModel(tempMduFilePath))
                    {
                        TypeUtils.CallPrivateMethod(model, "UpdateBathymetryCoverage", UGridFileHelper.BedLevelLocation.Faces);

                        var project = app.Project;
                        project.RootFolder.Add(model);

                        app.SaveProject();

                        model.ValidateBeforeRun = true;
                        var report = model.Validate();
                        Assert.AreEqual(0, report.AllErrors.Count(), "Model has errors in the validation report.");
                        app.RunActivity(model);
                        Assert.AreEqual(ActivityStatus.Cleaned, model.Status);

                        app.SaveProject();

                        model.ExportTo(exportMduFilePath);

                        var exportDirInfo = new DirectoryInfo(exportDirPath);
                        var morFiles = exportDirInfo.GetFiles("*.mor");
                        var sedFiles = exportDirInfo.GetFiles("*.sed");

                        var morFileName = "exported.mor";
                        var sedFileName = "exported.sed";
                        

                        Assert.NotNull(morFiles.FirstOrDefault(f => f.Name == morFileName));
                        Assert.NotNull(sedFiles.FirstOrDefault(f => f.Name == sedFileName));

                        Assert.AreEqual(morFiles.Length, 1, "More then one morphology file after export");
                        Assert.AreEqual(sedFiles.Length, 1, "More then one sediment file after export");

                        var properties = model.ModelDefinition.Properties;

                        // files referenced in MDU file
                        var morPropValue = properties.FirstOrDefault(p =>
                            p.PropertyDefinition.MduPropertyName.Equals(KnownProperties.MorFile,
                                StringComparison.InvariantCultureIgnoreCase)).GetValueAsString();

                        var sedPropValue = properties.FirstOrDefault(p =>
                            p.PropertyDefinition.MduPropertyName.Equals(KnownProperties.SedFile,
                                StringComparison.InvariantCultureIgnoreCase)).GetValueAsString();

                        Assert.AreEqual(morFileName, morPropValue);
                        Assert.AreEqual(sedFileName, sedPropValue);

                        app.CloseProject();
                    }
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(tempDirPath);
                FileUtils.DeleteIfExists(exportDirPath);
            }
        }

        private DeltaShellApplication GetConfiguredApplication(string savePath)
        {
            var app = new DeltaShellApplication();
            app.IsProjectCreatedInTemporaryDirectory = true;
            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new FlowFMApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());
            app.Run();
            app.SaveProjectAs(Path.Combine(savePath));
            return app;
        }

        private static WaterFlowFMModel GetWaterFlowFmModelWithMeteoData(bool useSolarRadiation)
        {
            var fmModel = new WaterFlowFMModel();
            Assert.IsNull(fmModel.ModelDefinition.HeatFluxModel.MeteoData);
            fmModel.ModelDefinition.HeatFluxModel.Type = HeatFluxModelType.Composite;

            var meteoData = fmModel.ModelDefinition.HeatFluxModel.MeteoData;
            Assert.IsNotNull(meteoData);

            // Setup lists of values
            var timesList = new List<DateTime>();
            var humidityValues = new List<double>();
            var airTemperatureValues = new List<double>();
            var cloudCoverageValues = new List<double>();
            var solarRadiationValues = new List<double>();

            var timeStep = new TimeSpan(0, 12, 0);
            var startTime = fmModel.StartTime;
            for (var i = 0; i < 3; ++i)
            {
                timesList.Add(startTime);
                startTime += timeStep;
                humidityValues.Add(i * i + 1);
                airTemperatureValues.Add(i * i + 2);
                cloudCoverageValues.Add(i * i + 3);
                solarRadiationValues.Add(i * i + 4);
            }

            // Set meteo data values and write to file
            meteoData.Arguments.FirstOrDefault(arg => arg.Name == "Time")?.SetValues(timesList);
            meteoData.Components.FirstOrDefault(arg => arg.Name == "Humidity")?.SetValues(humidityValues);
            meteoData.Components.FirstOrDefault(arg => arg.Name == "Air temperature")?.SetValues(airTemperatureValues);
            meteoData.Components.FirstOrDefault(arg => arg.Name == "Cloud coverage")?.SetValues(cloudCoverageValues);
            if (useSolarRadiation)
            {
                fmModel.ModelDefinition.HeatFluxModel.ContainsSolarRadiation = true;
                meteoData.Components.FirstOrDefault(arg => arg.Name == "Solar radiation")?.SetValues(solarRadiationValues);
            }

            return fmModel;
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Integration)]
        public void GivenAnFMModelWithAMorphologyBoundaryCondition_WhenSavedAndLoaded_ThenOnlyOneBoundaryIsCreated()
        {
            var tempDirPath = FileUtils.CreateTempDirectory();
            var tempProjectFilePath = Path.Combine(tempDirPath, "Project.dsproj");

            var boundaryName = "boundary1";
            var flowBoundaryConditionName = "boundary_condition_flow";
            var morphBoundaryConditionName = "boundary_condition_morph";

            try
            {
                using (var app = GetConfiguredApplication(tempProjectFilePath))
                {
                    using (var model = new WaterFlowFMModel())
                    {
                        model.ModelDefinition.GetModelProperty(GuiProperties.UseMorSed).Value = true;

                        var boundary = new Feature2D
                        {
                            Geometry =
                                new LineString(new[]
                                    {new Coordinate(0, 0), new Coordinate(1, 0)}),
                            Name = boundaryName
                        };

                        var boundaryConditionSet = new BoundaryConditionSet() {Feature = boundary};
                        model.BoundaryConditionSets.Add(boundaryConditionSet);

                        var flowBoundaryCondition =
                            new FlowBoundaryCondition(FlowBoundaryQuantityType.MorphologyBedLevelPrescribed,
                                BoundaryConditionDataType.TimeSeries)
                            {
                                Feature = boundary,
                                Name = flowBoundaryConditionName,
                            };

                        var morphologyBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                            BoundaryConditionDataType.TimeSeries)
                        {
                            Feature = boundary,
                            Name = morphBoundaryConditionName
                        };

                        flowBoundaryCondition.DataPointIndices.Add(1);
                        flowBoundaryCondition.PointData[0].Arguments[0]
                            .SetValues(new[] {model.StartTime, model.StopTime});
                        morphologyBoundaryCondition.DataPointIndices.Add(1);
                        morphologyBoundaryCondition.PointData[0].Arguments[0]
                            .SetValues(new[] {model.StartTime, model.StopTime});

                        boundaryConditionSet.BoundaryConditions.Add(flowBoundaryCondition);
                        boundaryConditionSet.BoundaryConditions.Add(morphologyBoundaryCondition);

                        var project = app.Project;
                        project.RootFolder.Add(model);

                        app.SaveProject();

                        app.OpenProject(tempProjectFilePath);

                        Assert.That(model.Boundaries.Count, Is.EqualTo(1));
                        Assert.That(model.Boundaries.FirstOrDefault().Name, Is.EqualTo(boundaryName));
                        Assert.That(model.BoundaryConditionSets.Count, Is.EqualTo(1));
                        Assert.That(model.BoundaryConditionSets.FirstOrDefault().Feature.Name, Is.EqualTo(boundaryName));
                        Assert.That(model.BoundaryConditionSets.FirstOrDefault().BoundaryConditions.Count, Is.EqualTo(2));

                        var boundaryConditionNames = model.BoundaryConditionSets.FirstOrDefault().BoundaryConditions
                            .Select(bc => bc.Name).ToList();
                        Assert.That(boundaryConditionNames.Contains(flowBoundaryConditionName));
                        Assert.That(boundaryConditionNames.Contains(morphBoundaryConditionName));

                        app.CloseProject();
                    }
                }
            }
            finally
            {
                FileUtils.DeleteIfExists(tempDirPath);
            }
        }
    }
}