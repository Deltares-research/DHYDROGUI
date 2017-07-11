using System;
using System.IO;
using System.Linq;
using System.Reflection;
using DelftTools.TestUtils;
using DeltaShell.Core;
using DeltaShell.Gui;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation;
using DeltaShell.Plugins.FMSuite.Wave;
using DeltaShell.Plugins.FMSuite.Wave.Gui;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
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
        public void ReadWriteModelWithSpatialOperationsTest()
        {
            using (var app = new DeltaShellApplication { IsProjectCreatedInTemporaryDirectory = true })
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Run();

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

                var loadedModel = (WaterFlowFMModel)app.Project.RootFolder.Items[0];
                var loadedDi = loadedModel.GetDataItemByValue(loadedModel.Bathymetry);
                var loadedOperations = ((SpatialOperationSetValueConverter)loadedDi.ValueConverter).SpatialOperationSet.Operations;

                //Assert.AreEqual(2, loadedOperations.Count);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void LoadAndRunModelWithMorphologyAndSpatialOperationsTest()
        {
            using (var app = new DeltaShellApplication { IsProjectCreatedInTemporaryDirectory = true })
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Run();

                app.SaveProjectAs("spatial_hibernate.dsproj"); // save to initialize file repository..
                //app.OpenProject(TestHelper.GetTestFilePath(@"MorphologySpatialVarying_Project\FM_model_Zandmotor_MOR1.dsproj"));

                var mduPath = TestHelper.GetTestFilePath(@"MorphologySpatialVarying_Project\FM_model_Zandmotor_MOR1.dsproj_data\zm_dfm\zm_dfm.mdu");
                var mduFilePath = TestHelper.CreateLocalCopy(mduPath);

                var model = new WaterFlowFMModel(mduFilePath);

                app.Project.RootFolder.Add(model);

                var loadedModel = (WaterFlowFMModel)app.Project.RootFolder.Items[0];
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
            var dir = Path.GetDirectoryName(Assembly.GetAssembly(typeof(WaterFlowFMNHibernateIntegrationTest)).Location);
            if (dir == null) return;
            string dsprojName = Path.Combine(dir, "FM_Only_Save_Load_Spatial_Operation.dsproj");
            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.IsProjectCreatedInTemporaryDirectory = true;
                app.Run();

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
            using (var gui = new DeltaShellGui())
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
            using (var app = new DeltaShellApplication() { IsProjectCreatedInTemporaryDirectory = true})
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());

                app.Run();

                var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                var mduFilePath = TestHelper.CreateLocalCopy(mduPath);

                var model = new WaterFlowFMModel(mduFilePath);
                app.Project.RootFolder.Add(model);

                app.SaveProjectAs(dsprojName); // save to initialize file repository..
            }

            using (var app = new DeltaShellApplication())
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
            using (var gui = new DeltaShellGui())
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

                var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                var mduFilePath = TestHelper.CreateLocalCopy(mduPath);

                var model = new WaterFlowFMModel(mduFilePath);
                gui.Application.Project.RootFolder.Add(model);

                app.SaveProjectAs(dsprojName); // save to initialize file repository..
            }

            using (var gui = new DeltaShellGui())
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
            using (var gui = new DeltaShellGui())
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

                var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
                var mduFilePath = TestHelper.CreateLocalCopy(mduPath);

                var model = new WaterFlowFMModel(mduFilePath);
                gui.Application.Project.RootFolder.Add(model);

                app.SaveProjectAs(dsprojName); // save to initialize file repository..
            }

            using (var gui = new DeltaShellGui())
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
            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                //apps : FM+Wave
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new WaveApplicationPlugin());

                //guis : FM+Wave
                gui.Plugins.Add(new CommonToolsGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());
                gui.Plugins.Add(new WaveGuiPlugin());
                
                gui.Run();

                var model = WaterFlowFMModelDefinitionValidatorTest.CreateValidModel();
                gui.Application.Project.RootFolder.Add(model);
                NetFile.Write(model.NetFilePath, model.Grid);
               
                app.SaveProjectAs(dsprojName); // save to initialize file repository..
                
            }

            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                //apps : FM+Wave+RTC
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
               
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new WaveApplicationPlugin());
                app.Plugins.Add(new RealTimeControlApplicationPlugin());

                //guis : FM+Wave+RTC
                gui.Plugins.Add(new CommonToolsGuiPlugin());
                gui.Plugins.Add(new SharpMapGisGuiPlugin());
                
                gui.Plugins.Add(new NetworkEditorGuiPlugin());
                gui.Plugins.Add(new FlowFMGuiPlugin());
                gui.Plugins.Add(new WaveGuiPlugin());
                gui.Plugins.Add(new RealTimeControlGuiPlugin());

                gui.Run();

                app.OpenProject(dsprojName);
                app.CloseProject();
            }
        }
    }
}
