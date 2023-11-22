using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.Core;
using DeltaShell.IntegrationTestUtils;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
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
            using (var app = DeltaShellCoreFactory.CreateApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Run();

                app.CreateNewProject();

                var bridgepillarsDsproj = "bridgePillars.dsproj";
                app.SaveProjectAs(bridgepillarsDsproj); // save to initialize file repository..

                var model = new WaterFlowFMModel();
                app.Project.RootFolder.Add(model);

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

                app.SaveProjectAs(bridgepillarsDsproj);
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
            using (var app = DeltaShellCoreFactory.CreateApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Run();

                app.CreateNewProject();

                var path = TestHelper.GetTestFilePath(@"ImportProjectWithBridgePillars\ProjectWithBridgePillars.dsproj");
                path = TestHelper.CreateLocalCopy(path);
                Assert.IsTrue(File.Exists(path));
                app.OpenProject(path);

                var loadedModel = app.Project.RootFolder.Models.OfType<WaterFlowFMModel>().FirstOrDefault();

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
            using (var app = DeltaShellCoreFactory.CreateApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Run();

                app.CreateNewProject();

                var bridgepillarsDsproj = "bridgePillars.dsproj";
                app.SaveProjectAs(bridgepillarsDsproj); // save to initialize file repository..
                //create model and set the pillar
                var model = new WaterFlowFMModel();
                app.Project.RootFolder.Add(model);

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
                app.SaveProjectAs(bridgepillarsDsproj);
                app.CloseProject();
                app.OpenProject(bridgepillarsDsproj);

                var loadedModel = app.GetAllModelsInProject().OfType<WaterFlowFMModel>().FirstOrDefault();
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
            using (var app = DeltaShellCoreFactory.CreateApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Run();

                app.CreateNewProject();
                app.SaveProjectAs("spatial_hibernate.dsproj"); // save to initialize file repository..
                
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

                app.Project.RootFolder.Add(model);

                app.SaveProjectAs("spatial_hibernate.dsproj");
                app.CloseProject();
                app.OpenProject("spatial_hibernate.dsproj");

                var loadedModel = (WaterFlowFMModel)app.Project.RootFolder.Items[0];
                var loadedDi = loadedModel.GetDataItemByValue(loadedModel.Bathymetry);
                var loadedOperations = ((SpatialOperationSetValueConverter)loadedDi.ValueConverter).SpatialOperationSet.Operations;

                //Assert.AreEqual(2, loadedOperations.Count);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void GivenWaterFlowFmModel_WhenEnablingMorphologyAndSpatialOperations_ThenModelShouldLoadAndRun()
        {
            using (var app = DeltaShellCoreFactory.CreateApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Run();
                
                app.CreateNewProject();
                app.SaveProjectAs("spatial_hibernate.dsproj");

                var testDataDirectory = TestHelper.GetTestFilePath(@"MorphologySpatialVarying_Project\FM_model_Zandmotor_MOR1.dsproj_data\zm_dfm");
                var zipFileName = "zm_dfm.zip";
                var zipFilePath = Path.Combine(testDataDirectory, zipFileName);

                TestHelper.PerformActionInTemporaryDirectory(tempDir =>
                {
                    FileUtils.CopyDirectory(testDataDirectory, tempDir);
                    ZipFileUtils.Extract(zipFilePath, tempDir);

                    var mduFileName = "zm_dfm.mdu";
                    var model = new WaterFlowFMModel(Path.Combine(tempDir, mduFileName));

                    app.Project.RootFolder.Add(model);

                    var loadedModel = (WaterFlowFMModel)app.Project.RootFolder.Items[0];
                    loadedModel.ClearOutput();
                    Assert.NotNull(loadedModel);
                    Assert.IsTrue(loadedModel.OutputIsEmpty);

                    app.SaveProjectAs("spatial_hibernate.dsproj"); // save to initialize file repository..
                    app.RunActivity(loadedModel);
                    Assert.That(loadedModel.Status, Is.Not.EqualTo(ActivityStatus.Failed));
                    Assert.IsFalse(loadedModel.OutputIsEmpty);

                    app.CloseProject();
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
            using (var app = DeltaShellCoreFactory.CreateApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Run();

                app.CreateNewProject();

                var model = new WaterFlowFMModel(TestHelper.GetTestFilePath(@"chezy_samples\chezy.mdu"));               
                app.Project.RootFolder.Add(model);
                app.SaveProjectAs(dsprojName); // save to initialize file repository..

                app.CloseProject();

                app.OpenProject(dsprojName);

                model = app.Project.RootFolder.Models.OfType<WaterFlowFMModel>().First();

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
            string dsprojName = Path.Combine(dir, "FM_Only.dsproj");
            using (var gui = DeltaShellCoreFactory.CreateGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());

                gui.Plugins.Add(new CommonToolsGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());

                gui.Run();

                app.CreateNewProject();

                var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                var mduFilePath = TestHelper.CreateLocalCopy(mduPath);

                var model = new WaterFlowFMModel(mduFilePath);
                var modelDir = model.ModelDefinition.ModelDirectory;
                gui.Application.Project.RootFolder.Add(model);

                app.SaveProjectAs(dsprojName); // save to initialize file repository..

                //Change location and save again.
                string newLocationProjName = Path.Combine(Path.Combine(dir, "newLocation"), "FM_Only.dsproj");
                app.SaveProjectAs(newLocationProjName);

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
            using (var app = DeltaShellCoreFactory.CreateApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());

                app.Run();
                app.CreateNewProject();

                var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                var mduFilePath = TestHelper.CreateLocalCopy(mduPath);

                var model = new WaterFlowFMModel(mduFilePath);
                app.Project.RootFolder.Add(model);

                app.SaveProjectAs(dsprojName); // save to initialize file repository..
            }

            using (var app = DeltaShellCoreFactory.CreateApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());

                app.Plugins.Add(new RealTimeControlApplicationPlugin());

                app.Run();

                app.OpenProject(dsprojName);
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
            using (var gui = DeltaShellCoreFactory.CreateGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());

                gui.Plugins.Add(new CommonToolsGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());

                gui.Run();
                app.CreateNewProject();

                var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                var mduFilePath = TestHelper.CreateLocalCopy(mduPath);

                var model = new WaterFlowFMModel(mduFilePath);
                gui.Application.Project.RootFolder.Add(model);

                app.SaveProjectAs(dsprojName); // save to initialize file repository..
            }

            using (var gui = DeltaShellCoreFactory.CreateGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());

                gui.Plugins.Add(new CommonToolsGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());

                app.Plugins.Add(new RealTimeControlApplicationPlugin());
                gui.Plugins.Add(new RealTimeControlGuiPlugin());

                gui.Run();

                app.OpenProject(dsprojName);
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
            using (var gui = DeltaShellCoreFactory.CreateGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());

                gui.Plugins.Add(new CommonToolsGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());

                app.Plugins.Add(new RealTimeControlApplicationPlugin());
                gui.Plugins.Add(new RealTimeControlGuiPlugin());

                gui.Run();
                app.CreateNewProject();

                var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                var mduFilePath = TestHelper.CreateLocalCopy(mduPath);

                var model = new WaterFlowFMModel(mduFilePath);
                gui.Application.Project.RootFolder.Add(model);

                app.SaveProjectAs(dsprojName); // save to initialize file repository..
            }

            using (var gui = DeltaShellCoreFactory.CreateGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());

                gui.Plugins.Add(new CommonToolsGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());

                
                gui.Run();

                app.OpenProject(dsprojName);
            }
        }
        
        // <summary>
        /// Test if an FM model can be saved in an environment with FM and Wave plugins.
        /// Then read it in an environment that contains FM, Wave and RTC.
        /// TOOLS-22951 - Work in progress & postponed
        /// </summary>
        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        [Category(TestCategory.WorkInProgress)]
        public void ReadFlowFMModelandWaveWithDifferentPluginConfigurationsGui()
        {
            string dsprojName = "FM_Wave.dsproj";
            using (var gui = DeltaShellCoreFactory.CreateGui())
            {
                var app = gui.Application;
                //apps : FM+Wave
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());

                //guis : FM+Wave
                gui.Plugins.Add(new CommonToolsGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());
                
                gui.Run();
                app.CreateNewProject();

                var model = WaterFlowFMModelDefinitionValidatorTest.CreateValidModel();
                gui.Application.Project.RootFolder.Add(model);
                NetFile.Write(model.NetFilePath, model.Grid);
               
                app.SaveProjectAs(dsprojName); // save to initialize file repository..
                
            }

            using (var gui = DeltaShellCoreFactory.CreateGui())
            {
                var app = gui.Application;
                //apps : FM+Wave+RTC
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
               
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new RealTimeControlApplicationPlugin());

                //guis : FM+Wave+RTC
                gui.Plugins.Add(new CommonToolsGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());
                gui.Plugins.Add(new RealTimeControlGuiPlugin());

                gui.Run();

                app.OpenProject(dsprojName);
                app.CloseProject();
            }
        }
    }
}
