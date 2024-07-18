using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap.Data.Providers;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class WaterFlowFMNHibernateIntegrationTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void Save_FlowFM_Model_With_BridgePillars_Pillars_Are_Exported()
        {
            using (var app = CreateApplication())
            {
                app.Run();
                IProjectService projectService = app.ProjectService;
                Project project = projectService.CreateProject();

                var bridgepillarsDsproj = "bridgePillars.dsproj";

                var model = new WaterFlowFMModel();
                project.RootFolder.Add(model);

                var pillar = new BridgePillar()
                {
                    Name = "BridgePillar2Test",
                    Geometry =
                        new LineString(new[]
                        {
                            new Coordinate(20.0, 60.0, 0),
                            new Coordinate(140.0, 8.0, 1.0),
                            new Coordinate(180.0, 4.0, 2.0),
                            new Coordinate(260.0, 0.0, 3.0)
                        }),
                };

                model.Area.BridgePillars.Add(pillar);

                /* Set data model */
                var modelFeatureCoordinateDatas = model.BridgePillarsDataModel;
                modelFeatureCoordinateDatas.Clear();
                var modelFeatureCoordinateData = new ModelFeatureCoordinateData<BridgePillar>() { Feature = pillar };
                modelFeatureCoordinateData.UpdateDataColumns();
                //Diameters
                modelFeatureCoordinateData.DataColumns[0].ValueList = new List<double> { 1.0, 2.5, 5.0, 10.0 };
                //DragCoefficient
                modelFeatureCoordinateData.DataColumns[1].ValueList = new List<double> { 10.0, 5.0, 2.5, 1.0 };

                modelFeatureCoordinateDatas.Add(modelFeatureCoordinateData);
                MduFile.SetBridgePillarAttributes(model.Area.BridgePillars, modelFeatureCoordinateDatas);
                /* Done only for testing purposes. 
                 * Please do not attempt to do this without the supervision of another adult. */
                TypeUtils.SetPrivatePropertyValue(model, "BridgePillarsDataModel", modelFeatureCoordinateDatas);

                projectService.SaveProjectAs(bridgepillarsDsproj);
                Assert.IsNotNull(model.MduFile.Path);

                var pillarFile = model.MduFile.Path.Replace(".mdu", ".pliz");
                Assert.IsTrue(File.Exists(pillarFile));
                 /*Check file contents*/
                var readLines = File.ReadAllLines(pillarFile);
                var expectedLines = new List<string>
                {
                    $"{pillar.Name}",
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
        }

        [Test]
        public void Load_FlowFM_Model_With_BridgePillars_Pillar_Is_Imported()
        {
            using (var app = CreateApplication())
            {
                app.Run();
                IProjectService projectService = app.ProjectService;

                var path = TestHelper.GetTestFilePath(@"ImportProjectWithBridgePillars\ProjectWithBridgePillars.dsproj");
                path = TestHelper.CreateLocalCopy(path);
                Assert.IsTrue(File.Exists(path));
                Project project = projectService.OpenProject(path);

                var loadedModel = project.RootFolder.Models.OfType<WaterFlowFMModel>().FirstOrDefault();

                Assert.IsNotNull(loadedModel);
                Assert.IsTrue(loadedModel.Area.BridgePillars.Any());
                Assert.IsNotNull(loadedModel.BridgePillarsDataModel);
                Assert.IsTrue(loadedModel.BridgePillarsDataModel.Any());
                Assert.IsTrue(loadedModel.BridgePillarsDataModel[0].DataColumns[0].ValueList[0].Equals(-599.0));
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void SaveAndLoad_FlowFM_Model_With_BridgePillars_Pillars()
        {
            using (var app = CreateApplication())
            {
                app.Run();
                IProjectService projectService = app.ProjectService;
                Project project = projectService.CreateProject();

                var bridgepillarsDsproj = "bridgePillars.dsproj";
                //create model and set the pillar
                var model = new WaterFlowFMModel();
                project.RootFolder.Add(model);

                var pillar = new BridgePillar()
                {
                    Name = "BridgePillar2Test",
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
                var modelFeatureCoordinateDatas = model.BridgePillarsDataModel;
                modelFeatureCoordinateDatas.Clear();
                var modelFeatureCoordinateData = new ModelFeatureCoordinateData<BridgePillar>() { Feature = pillar };
                modelFeatureCoordinateData.UpdateDataColumns();
                //Diameters
                var diameterValues = new List<double> { 1.0, 2.5, 5.0, 10.0 };
                modelFeatureCoordinateData.DataColumns[0].ValueList = diameterValues;
                //DragCoefficient
                var dragCoeffValues = new List<double> { 10.0, 5.0, 2.5, 1.0 };
                modelFeatureCoordinateData.DataColumns[1].ValueList = dragCoeffValues;

                modelFeatureCoordinateDatas.Add(modelFeatureCoordinateData);
                MduFile.SetBridgePillarAttributes(model.Area.BridgePillars, modelFeatureCoordinateDatas);

                model.Area.BridgePillars.Add(pillar);

                /* Done only for testing purposes. 
                 * Please do not attempt to do this without the supervision of another adult. */
                TypeUtils.SetPrivatePropertyValue(model, "BridgePillarsDataModel", modelFeatureCoordinateDatas);

                //Check the Save & Loads works as expected.
                projectService.SaveProjectAs(bridgepillarsDsproj);
                projectService.CloseProject();
                project = projectService.OpenProject(bridgepillarsDsproj);

                var loadedModel = project.RootFolder.GetAllModelsRecursive().OfType<WaterFlowFMModel>().FirstOrDefault();
                Assert.IsNotNull(loadedModel);
                Assert.IsNotNull(loadedModel.Area);
                Assert.IsNotNull(loadedModel.Area.BridgePillars);
                Assert.IsTrue(loadedModel.Area.BridgePillars.Any(bp => bp.Name.Equals(pillar.Name)));
                Assert.IsTrue(loadedModel.BridgePillarsDataModel.Any());
                /* Check contents of the Bridge Pillar */
                var loadedBpDataModel = loadedModel.BridgePillarsDataModel[0];
                Assert.AreEqual(new List<double> { 1.0, 2.5, 5.0, 10.0 }, loadedBpDataModel.DataColumns[0].ValueList);
                Assert.AreEqual(new List<double> { 10.0, 5.0, 2.5, 1.0 }, loadedBpDataModel.DataColumns[1].ValueList);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ReadWriteModelWithSpatialOperationsTest()
        {
            using (var app = CreateApplication())
            {
                app.Run();
                IProjectService projectService = app.ProjectService;
                Project project = projectService.CreateProject();
                
                var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                var mduFilePath = TestHelper.CreateLocalCopy(mduPath);

                var model = new WaterFlowFMModel(mduFilePath);

                var di = model.GetDataItemByValue(model.Bathymetry);
                var coverageValueConverter = SpatialOperationValueConverterFactory.Create(di.Value, di.Value.GetType());
                di.ValueConverter = coverageValueConverter;

                var operationSet = coverageValueConverter.SpatialOperationSet;
                operationSet.Inputs[0].FeatureType = typeof(UnstructuredGridVertexCoverage);
                operationSet.Inputs[0].Provider = new CoverageFeatureProvider {Coverage = model.Bathymetry};
                
                var maskFeatureColl = new FeatureCollection(
                    new[]
                    {
                        new Feature()
                        {
                            Geometry = new Polygon(
                                new LinearRing(new []
                                {
                                    new Coordinate(0, 0), new Coordinate(10, 10),
                                    new Coordinate(20, -20), new Coordinate(0, 0)
                                }))
                        }
                    }, typeof(Feature));

                var setValueOperation = new SetValueOperation
                {
                    Name = "Set value",
                    Value = 0.0,
                    OperationType = PointwiseOperationType.Overwrite
                };
                setValueOperation.Mask.Provider = maskFeatureColl;
                Assert.IsNotNull(operationSet.AddOperation(setValueOperation));

                var cropOperation = new CropOperation {Name = "Crop"};
                cropOperation.Mask.Provider = maskFeatureColl;
                Assert.IsNotNull(operationSet.AddOperation(cropOperation));

                var smoothOperation = new SmoothingOperation
                {
                    Name = "Smoothing",
                    InverseDistanceWeightExponent = 2.0,
                    IterationCount = 3
                };
                smoothOperation.Mask.Provider = maskFeatureColl;
                Assert.IsNotNull(operationSet.AddOperation(smoothOperation));

                project.RootFolder.Add(model);

                projectService.SaveProjectAs("spatial_hibernate.dsproj");
                projectService.CloseProject();
                project = projectService.OpenProject("spatial_hibernate.dsproj");

                var loadedModel = (WaterFlowFMModel)project.RootFolder.Items[0];
                var loadedDi = loadedModel.GetDataItemByValue(loadedModel.Bathymetry);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void GivenWaterFlowFmModel_WhenEnablingMorphologyAndSpatialOperations_ThenModelShouldLoadAndRun()
        {
            using (var app = CreateApplication())
            {
                app.Run();

                var testDataDirectory = TestHelper.GetTestFilePath(@"MorphologySpatialVarying_Project\FM_model_Zandmotor_MOR1.dsproj_data\zm_dfm");
                var zipFileName = "zm_dfm.zip";
                var zipFilePath = Path.Combine(testDataDirectory, zipFileName);

                TestHelper.PerformActionInTemporaryDirectory(tempDir =>
                {
                    FileUtils.CopyDirectory(testDataDirectory, tempDir);
                    ZipFileUtils.Extract(zipFilePath, tempDir);

                    var mduFileName = "zm_dfm.mdu";
                    var loadedModel = new WaterFlowFMModel(Path.Combine(tempDir, mduFileName));
                    
                    loadedModel.ClearOutput();
                    Assert.NotNull(loadedModel);
                    Assert.IsTrue(loadedModel.OutputIsEmpty);

                    app.RunActivity(loadedModel);
                    Assert.That(loadedModel.Status, Is.Not.EqualTo(ActivityStatus.Failed));
                    Assert.IsFalse(loadedModel.OutputIsEmpty);
                });
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ImportSaveLoadSpatialOperationsTest()
        {
            var dir = Path.GetDirectoryName(Assembly.GetAssembly(typeof(WaterFlowFMNHibernateIntegrationTest)).Location);
            if (dir == null) return;
            string dsprojName = Path.Combine(dir, "FM_Only_Save_Load_Spatial_Operation.dsproj");
            using (var app = CreateApplication())
            {
                app.Run();
                IProjectService projectService = app.ProjectService;
                Project project = projectService.CreateProject();

                var model = new WaterFlowFMModel(TestHelper.GetTestFilePath(@"chezy_samples\chezy.mdu"));               
                project.RootFolder.Add(model);
                projectService.SaveProjectAs(dsprojName);

                projectService.CloseProject();

                project = projectService.OpenProject(dsprojName);

                model = project.RootFolder.Models.OfType<WaterFlowFMModel>().First();

                var valueConverter = model.GetDataItemByValue(model.Roughness).ValueConverter;
                var spatialOperationValueConverter = valueConverter as SpatialOperationSetValueConverter;

                Assert.IsNotNull(spatialOperationValueConverter);

                Assert.AreEqual(2, spatialOperationValueConverter.SpatialOperationSet.Operations.Count);
                Assert.IsTrue(spatialOperationValueConverter.SpatialOperationSet.Operations[1] is InterpolateOperation);

                var values = model.Roughness.GetValues<double>();
                Assert.IsFalse(values.All(v => Math.Abs(v - (double) model.Roughness.Components[0].NoDataValue) < 1e-15), "Roughness spatial data is loaded but only contains no data values, it should contain real values!!");
            }
        }

        
        
        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void SaveModelAndCheckNewModelDirectory()
        {
            var dir = Path.GetDirectoryName(Assembly.GetAssembly(typeof(WaterFlowFMNHibernateIntegrationTest)).Location);
            if (dir == null) return;
            using (var gui = CreateGui())
            {
                var app = gui.Application;
                gui.Run();
                IProjectService projectService = app.ProjectService;
                Project project = projectService.CreateProject();

                var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                var mduFilePath = TestHelper.CreateLocalCopy(mduPath);

                var model = new WaterFlowFMModel(mduFilePath);
                var modelDir = model.ModelDefinition.ModelDirectory;
                project.RootFolder.Add(model);

                //Change location and save again.
                string newLocationProjName = Path.Combine(Path.Combine(dir, "newLocation"), "FM_Only.dsproj");
                projectService.SaveProjectAs(newLocationProjName);

                //Check if the model directory has changed
                Assert.That(modelDir, Is.Not.Null);
                var newModelDir = model.ModelDefinition.ModelDirectory;
                Assert.That(newModelDir, Is.Not.Null);
                Assert.That(modelDir.Equals(newModelDir), Is.False);
            }
        }
        /// <summary>
        /// Test if an FM Model can be saved in an FM only environment.
        /// Then read it in an environment that contains extra plugins with backwards compatibility mappings.
        /// This breaks currently, because the mapping is upgraded while it shouldn't.
        /// </summary>
        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void ReadFlowFMModelWithDifferentPluginConfiguration()
        {
            string dsprojName = "FM_Only.dsproj";
            // the temporary project is required in order to set the path on the model. Else, it saves null in the Path property of the fm model.
            using (var app = CreateApplication())
            {
                app.Run();
                IProjectService projectService = app.ProjectService;
                Project project = projectService.CreateProject();

                var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                var mduFilePath = TestHelper.CreateLocalCopy(mduPath);

                var model = new WaterFlowFMModel(mduFilePath);
                project.RootFolder.Add(model);

                projectService.SaveProjectAs(dsprojName);
            }

            var pluginsToAdd = new List<IPlugin>()
            {
                new NHibernateDaoApplicationPlugin(),
                new CommonToolsApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new FlowFMApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
                new RealTimeControlApplicationPlugin(),
            };
            using (var app = new DeltaShellApplicationBuilder().WithPlugins(pluginsToAdd).Build())
            {
                app.Run();

                app.ProjectService.OpenProject(dsprojName);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void ReadFlowFMModelWithDifferentPluginConfigurationGui()
        {
            var dir = Path.GetDirectoryName(Assembly.GetAssembly(typeof (WaterFlowFMNHibernateIntegrationTest)).Location);
            if (dir == null) return;
            string dsprojName = Path.Combine(dir, "FM_Only.dsproj");
            using (var gui = CreateGui())
            {
                var app = gui.Application;
                gui.Run();
                IProjectService projectService = app.ProjectService;
                Project project = projectService.CreateProject();

                var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                var mduFilePath = TestHelper.CreateLocalCopy(mduPath);

                var model = new WaterFlowFMModel(mduFilePath);
                project.RootFolder.Add(model);

                projectService.SaveProjectAs(dsprojName);
            }

            using (var gui = CreateGuiWithRTC())
            {
                gui.Run();

                gui.Application.ProjectService.OpenProject(dsprojName);
            }
        }

        /// <summary>
        /// Test if an FM model can be saved in an environment with FM and RTC plugins.
        /// Then read it in an environment that only contains FM.
        /// </summary>
        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.VerySlow)]
        public void ReadFlowFMModelWithLessPluginConfigurations()
        {
            string dsprojName = "FM_Only.dsproj";
            
            
            using (var gui = CreateGuiWithRTC())
            {
                var app = gui.Application;
                
                gui.Run();
                IProjectService projectService = app.ProjectService;
                Project project = projectService.CreateProject();

                var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                var mduFilePath = TestHelper.CreateLocalCopy(mduPath);

                var model = new WaterFlowFMModel(mduFilePath);
                project.RootFolder.Add(model);

                projectService.SaveProjectAs(dsprojName);
            }
            
            using (var gui = CreateGui())
            {
                gui.Run();

                gui.Application.ProjectService.OpenProject(dsprojName);
            }
        }
        
        private static IApplication CreateApplication()
        {
            var pluginsToAdd = new List<IPlugin>()
            {
                new NHibernateDaoApplicationPlugin(),
                new CommonToolsApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new FlowFMApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
            };

            return new DeltaShellApplicationBuilder().WithPlugins(pluginsToAdd).Build();
        }
        
        private static IGui CreateGuiWithRTC()
        {
            var pluginsToAdd = new List<IPlugin>()
            {
                //apps : FM+Wave
                new NHibernateDaoApplicationPlugin(),
                new CommonToolsApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),                
                new NetworkEditorApplicationPlugin(),
                new FlowFMApplicationPlugin(),

                //guis : FM+Wave
                new CommonToolsGuiPlugin(),
                new SharpMapGisGuiPlugin(),                
                new NetworkEditorGuiPlugin(),
                new FlowFMGuiPlugin(),
                
                // RTC
                new RealTimeControlApplicationPlugin(),
                new RealTimeControlGuiPlugin(),
            };
            return new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build();
        }
        
        private static IGui CreateGui()
        {
            var pluginsToAdd = new List<IPlugin>()
            {
                //apps : FM+Wave
                new NHibernateDaoApplicationPlugin(),
                new CommonToolsApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new FlowFMApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
                
                //guis : FM+Wave
                new CommonToolsGuiPlugin(),
                new SharpMapGisGuiPlugin(),
                new NetworkEditorGuiPlugin(),
                new FlowFMGuiPlugin(),
            };
            return new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build();
        }
    }
}
