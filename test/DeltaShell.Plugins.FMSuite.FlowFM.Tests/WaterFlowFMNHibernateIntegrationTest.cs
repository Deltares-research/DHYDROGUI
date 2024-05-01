using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DelftTools.Functions.Generic;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using DHYDRO.TestModels.DFlowFM;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap.Api.SpatialOperations;
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
                        })
                };

                model.Area.BridgePillars.Add(pillar);

                /* Set data model */
                IList<ModelFeatureCoordinateData<BridgePillar>> modelFeatureCoordinateDatas = model.BridgePillarsDataModel;
                modelFeatureCoordinateDatas.Clear();
                var modelFeatureCoordinateData = new ModelFeatureCoordinateData<BridgePillar>() {Feature = pillar};
                modelFeatureCoordinateData.UpdateDataColumns();
                //Diameters
                modelFeatureCoordinateData.DataColumns[0].ValueList = new List<double>
                {
                    1.0,
                    2.5,
                    5.0,
                    10.0
                };
                //DragCoefficient
                modelFeatureCoordinateData.DataColumns[1].ValueList = new List<double>
                {
                    10.0,
                    5.0,
                    2.5,
                    1.0
                };

                modelFeatureCoordinateDatas.Add(modelFeatureCoordinateData);
                MduFile.SetBridgePillarAttributes(model.Area.BridgePillars, modelFeatureCoordinateDatas);
                /* Done only for testing purposes. 
                 * Please do not attempt to do this without the supervision of another adult. */
                TypeUtils.SetPrivatePropertyValue(model, "BridgePillarsDataModel", modelFeatureCoordinateDatas);

                app.SaveProjectAs(bridgepillarsDsproj);
                Assert.IsNotNull(model.MduFilePath);

                string pillarFile = model.MduFilePath.Replace(".mdu", ".pliz");
                Assert.IsTrue(File.Exists(pillarFile));
                /*Check file contents*/
                string[] readLines = File.ReadAllLines(pillarFile);
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
                foreach (string textLine in readLines)
                {
                    string expectedLine = expectedLines[idx];
                    Assert.AreEqual(expectedLine, textLine);
                    idx++;
                }
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void Load_FlowFM_Model_With_BridgePillars_Pillar_Is_Imported()
        {
            using (var app = CreateApplication())
            {
                app.Run();

                app.CreateNewProject();

                string path = TestHelper.GetTestFilePath(@"ImportProjectWithBridgePillars\ProjectWithBridgePillars.dsproj");
                path = TestHelper.CreateLocalCopy(path);
                Assert.IsTrue(File.Exists(path));
                app.OpenProject(path);

                WaterFlowFMModel loadedModel = app.Project.RootFolder.Models.OfType<WaterFlowFMModel>().FirstOrDefault();

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
                        })
                };

                /* Set data model */
                IList<ModelFeatureCoordinateData<BridgePillar>> modelFeatureCoordinateDatas = model.BridgePillarsDataModel;
                modelFeatureCoordinateDatas.Clear();
                var modelFeatureCoordinateData = new ModelFeatureCoordinateData<BridgePillar>() {Feature = pillar};
                modelFeatureCoordinateData.UpdateDataColumns();
                //Diameters
                var diameterValues = new List<double>
                {
                    1.0,
                    2.5,
                    5.0,
                    10.0
                };
                modelFeatureCoordinateData.DataColumns[0].ValueList = diameterValues;
                //DragCoefficient
                var dragCoeffValues = new List<double>
                {
                    10.0,
                    5.0,
                    2.5,
                    1.0
                };
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

                WaterFlowFMModel loadedModel = app.GetAllModelsInProject().OfType<WaterFlowFMModel>().FirstOrDefault();
                Assert.IsNotNull(loadedModel);
                Assert.IsNotNull(loadedModel.Area);
                Assert.IsNotNull(loadedModel.Area.BridgePillars);
                Assert.IsTrue(loadedModel.Area.BridgePillars.Any(bp => bp.Name.Equals(pillar.Name)));
                Assert.IsTrue(loadedModel.BridgePillarsDataModel.Any());
                /* Check contents of the Bridge Pillar */
                ModelFeatureCoordinateData<BridgePillar> loadedBpDataModel = loadedModel.BridgePillarsDataModel[0];
                Assert.AreEqual(new List<double>
                {
                    1.0,
                    2.5,
                    5.0,
                    10.0
                }, loadedBpDataModel.DataColumns[0].ValueList);
                Assert.AreEqual(new List<double>
                {
                    10.0,
                    5.0,
                    2.5,
                    1.0
                }, loadedBpDataModel.DataColumns[1].ValueList);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.VerySlow)]
        public void ReadWriteModelWithSpatialOperationsTest()
        {
            using (var app = CreateApplication())
            {
                app.Run();

                app.CreateNewProject();

                app.SaveProjectAs("spatial_hibernate.dsproj"); // save to initialize file repository..

                string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                string mduFilePath = TestHelper.CreateLocalCopy(mduPath);

                var model = new WaterFlowFMModel();
                model.ImportFromMdu(mduFilePath);

                IDataItem di = model.GetDataItemByValue(model.SpatialData.Bathymetry);
                SpatialOperationSetValueConverter coverageValueConverter = SpatialOperationValueConverterFactory.Create(di.Value, di.Value.GetType());
                di.ValueConverter = coverageValueConverter;

                ISpatialOperationSet operationSet = coverageValueConverter.SpatialOperationSet;
                operationSet.Inputs[0].FeatureType = typeof(UnstructuredGridVertexCoverage);
                operationSet.Inputs[0].Provider = new CoverageFeatureProvider {Coverage = model.SpatialData.Bathymetry};

                var maskFeatureColl = new FeatureCollection(
                    new[]
                    {
                        new Feature
                        {
                            Geometry = new Polygon(
                                new LinearRing(new[]
                                {
                                    new Coordinate(0, 0),
                                    new Coordinate(10, 10),
                                    new Coordinate(20, -20),
                                    new Coordinate(0, 0)
                                }))
                        }
                    }, typeof(Feature));

                var setValueOperation = new SetValueOperation
                {
                    Value = 0.0,
                    OperationType = PointwiseOperationType.Overwrite
                };
                setValueOperation.Mask.Provider = maskFeatureColl;
                Assert.IsNotNull(operationSet.AddOperation(setValueOperation));

                var cropOperation = new CropOperation();
                cropOperation.Mask.Provider = maskFeatureColl;
                Assert.IsNotNull(operationSet.AddOperation(cropOperation));

                var smoothOperation = new SmoothingOperation
                {
                    InverseDistanceWeightExponent = 2.0,
                    IterationCount = 3
                };
                smoothOperation.Mask.Provider = maskFeatureColl;
                Assert.IsNotNull(operationSet.AddOperation(smoothOperation));

                app.Project.RootFolder.Add(model);

                app.SaveProjectAs("spatial_hibernate.dsproj");
                app.CloseProject();
                app.OpenProject("spatial_hibernate.dsproj");

                var loadedModel = (WaterFlowFMModel) app.Project.RootFolder.Items[0];
                IDataItem loadedDi = loadedModel.GetDataItemByValue(loadedModel.SpatialData.Bathymetry);
                var loadedOperations = (SpatialOperationSetValueConverter) loadedDi.ValueConverter;

                Assert.IsNull(loadedOperations,
                              "No spatial operations for bed level should be loaded.");
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void GivenWaterFlowFmModel_WhenEnablingMorphologyAndSpatialOperations_ThenModelShoulLoadAndRun()
        {
            using(var tempDir = new TemporaryDirectory())
            using (var app = CreateApplication())
            {
                app.Run();

                app.CreateNewProject();
                app.SaveProjectAs("spatial_hibernate.dsproj");

                DFlowFMModelRepository.f005_boundary_conditions
                                      .c011_waterlevel_tim_varying
                                      .CopyTo(new DirectoryInfo(tempDir.Path));
                const string mduFileName = "tfl.mdu";
                WaterFlowFMModel model = ImportModelFromTemporaryDirectory(tempDir.Path, mduFileName);

                app.Project.RootFolder.Add(model);

                var loadedModel = (WaterFlowFMModel) app.Project.RootFolder.Items[0];
                loadedModel.ClearOutput();
                Assert.NotNull(loadedModel);
                Assert.IsTrue(loadedModel.OutputIsEmpty);

                app.SaveProjectAs("spatial_hibernate.dsproj"); // save to initialize file repository..
                app.RunActivity(loadedModel);
                Assert.IsFalse(loadedModel.OutputIsEmpty);

                app.CloseProject();
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ImportSaveLoadSpatialOperationsTest()
        {
            string dir = Path.GetDirectoryName(Assembly.GetAssembly(typeof(WaterFlowFMNHibernateIntegrationTest)).Location);
            if (dir == null)
            {
                return;
            }

            string dsprojName = Path.Combine(dir, "FM_Only_Save_Load_Spatial_Operation.dsproj");
            using (var app = CreateApplication())
            {
                app.Run();

                app.CreateNewProject();

                var model = new WaterFlowFMModel();
                model.ImportFromMdu(TestHelper.GetTestFilePath(@"chezy_samples\chezy.mdu"));

                app.Project.RootFolder.Add(model);
                app.SaveProjectAs(dsprojName); // save to initialize file repository..

                app.CloseProject();

                app.OpenProject(dsprojName);

                model = app.Project.RootFolder.Models.OfType<WaterFlowFMModel>().First();

                IValueConverter valueConverter = model.GetDataItemByValue(model.SpatialData.Roughness).ValueConverter;
                var spatialOperationValueConverter = valueConverter as SpatialOperationSetValueConverter;

                Assert.IsNotNull(spatialOperationValueConverter);

                Assert.AreEqual(2, spatialOperationValueConverter.SpatialOperationSet.Operations.Count);
                Assert.IsTrue(spatialOperationValueConverter.SpatialOperationSet.Operations[1] is InterpolateOperation);

                IMultiDimensionalArray<double> values = model.SpatialData.Roughness.GetValues<double>();
                Assert.IsFalse(values.All(v => Math.Abs(v - (double) model.SpatialData.Roughness.Components[0].NoDataValue) < 1e-15), "Roughness spatial data is loaded but only contains no data values, it should contain real values!!");
            }
        }

        
        
        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void SaveModelAndCheckNewModelDirectory()
        {
            string dir = Path.GetDirectoryName(Assembly.GetAssembly(typeof(WaterFlowFMNHibernateIntegrationTest)).Location);
            if (dir == null)
            {
                return;
            }

            string dsprojName = Path.Combine(dir, "FM_Only.dsproj");
            using (var gui = CreateGui())
            {
                IApplication app = gui.Application;
                
                gui.Run();

                app.CreateNewProject();

                string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                string mduFilePath = TestHelper.CreateLocalCopy(mduPath);

                var model = new WaterFlowFMModel();
                model.ImportFromMdu(mduFilePath);

                string modelDir = model.GetModelDirectory();
                gui.Application.Project.RootFolder.Add(model);

                app.SaveProjectAs(dsprojName); // save to initialize file repository..

                //Change location and save again.
                string newLocationProjName = Path.Combine(Path.Combine(dir, "newLocation"), "FM_Only.dsproj");
                app.SaveProjectAs(newLocationProjName);

                //Check if the model directory has changed
                Assert.That(modelDir, Is.Not.Null);
                string newModelDir = model.GetModelDirectory();
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
        [Category(TestCategory.Integration)]
        [Category(TestCategory.VerySlow)]
        public void ReadFlowFMModelWithDifferentPluginConfiguration()
        {
            var dsprojName = "FM_Only.dsproj";
            // the temporary project is required in order to set the path on the model. Else, it saves null in the Path property of the fm model.
            using (var app = CreateApplication())
            {
                app.Run();

                app.CreateNewProject();

                string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                string mduFilePath = TestHelper.CreateLocalCopy(mduPath);

                var model = new WaterFlowFMModel();
                model.ImportFromMdu(mduFilePath);

                app.Project.RootFolder.Add(model);

                app.SaveProjectAs(dsprojName); // save to initialize file repository..
            }

            using (var app = CreateApplicationWithRTC())
            {
                app.Run();

                app.OpenProject(dsprojName);
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.VerySlow)]
        public void ReadFlowFMModelWithDifferentPluginConfigurationGui()
        {
            string dir = Path.GetDirectoryName(Assembly.GetAssembly(typeof(WaterFlowFMNHibernateIntegrationTest)).Location);
            if (dir == null)
            {
                return;
            }

            string dsprojName = Path.Combine(dir, "FM_Only.dsproj");
            using (var gui = CreateGui())
            {
                IApplication app = gui.Application;

                gui.Run();

                app.CreateNewProject();

                string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                string mduFilePath = TestHelper.CreateLocalCopy(mduPath);

                var model = new WaterFlowFMModel();
                model.ImportFromMdu(mduFilePath);

                gui.Application.Project.RootFolder.Add(model);

                app.SaveProjectAs(dsprojName); // save to initialize file repository..
            }

            using (var gui = CreateGuiWithRTC())
            {
                IApplication app = gui.Application;

                gui.Run();

                app.OpenProject(dsprojName);
            }
        }

        /// <summary>
        /// Test if an FM model can be saved in an environment with FM and RTC plugins.
        /// Then read it in an environment that only contains FM.
        /// </summary>
        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.VerySlow)]
        public void ReadFlowFMModelWithLessPluginConfigurations()
        {
            var dsprojName = "FM_Only.dsproj";
            using (var gui = CreateGuiWithRTC())
            {
                IApplication app = gui.Application;

                gui.Run();

                app.CreateNewProject();

                string mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                string mduFilePath = TestHelper.CreateLocalCopy(mduPath);

                var model = new WaterFlowFMModel();
                model.ImportFromMdu(mduFilePath);

                gui.Application.Project.RootFolder.Add(model);

                app.SaveProjectAs(dsprojName); // save to initialize file repository..
            }

            using (var gui = CreateGui())
            {
                IApplication app = gui.Application;

                gui.Run();

                app.OpenProject(dsprojName);
            }
        }
        
        private static IApplication CreateApplication()
        {
            var pluginsToAdd = new List<IPlugin>
            {
                new NHibernateDaoApplicationPlugin(),
                new CommonToolsApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new FlowFMApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
            };
            return new DeltaShellApplicationBuilder().WithPlugins(pluginsToAdd).Build();
        }
        
        private static IApplication CreateApplicationWithRTC()
        {
            var pluginsToAdd = new List<IPlugin>
            {
                new NHibernateDaoApplicationPlugin(),
                new CommonToolsApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new FlowFMApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
                new RealTimeControlApplicationPlugin()
            };
            return new DeltaShellApplicationBuilder().WithPlugins(pluginsToAdd).Build();
        }
        
        private static IGui CreateGui()
        {
            var pluginsToAdd = new List<IPlugin>
            {
                new NHibernateDaoApplicationPlugin(),
                new CommonToolsApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new FlowFMApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
                new CommonToolsGuiPlugin(),
                new SharpMapGisGuiPlugin(),
                new NetworkEditorGuiPlugin(),
                new FlowFMGuiPlugin(),
            };
            return new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build();
        }
        private static IGui CreateGuiWithRTC()
        {
            var pluginsToAdd = new List<IPlugin>
            {
                new NHibernateDaoApplicationPlugin(),
                new CommonToolsApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new FlowFMApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
                new CommonToolsGuiPlugin(),
                new SharpMapGisGuiPlugin(),
                new NetworkEditorGuiPlugin(),
                new FlowFMGuiPlugin(),
                new RealTimeControlApplicationPlugin(),
                new RealTimeControlGuiPlugin(),
            };
            return new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build();
        }

        private static WaterFlowFMModel ImportModelFromTemporaryDirectory(string tempDir, string mduFileName)
        {
            string mduFilePath = Path.Combine(tempDir, mduFileName);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduFilePath);

            return model;
        }
    }
}