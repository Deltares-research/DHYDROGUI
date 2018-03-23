using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.IO;
using DeltaShell.Core;
using DeltaShell.Dimr;
using DeltaShell.IntegrationTestUtils;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel.ValueConverters;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterQualityModel;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.Wave;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;
using SharpTestsEx;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    [Category(TestCategory.Slow)]
    public class NHibernateHydroModelIntegrationTest : NHibernateIntegrationTestBase
    {
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();
            factory.AddPlugin(new NetworkEditorApplicationPlugin());
            factory.AddPlugin(new HydroModelApplicationPlugin());
            factory.AddPlugin(new WaterFlowModel1DApplicationPlugin());
            factory.AddPlugin(new RainfallRunoffApplicationPlugin());
            factory.AddPlugin(new RealTimeControlApplicationPlugin());
            factory.AddPlugin(new WaterQualityModelApplicationPlugin());
            factory.AddPlugin(new FlowFMApplicationPlugin());
            factory.AddPlugin(new WaveApplicationPlugin());
        }

        [Test]
        public void SaveLoadHydroModelWithSeveralSubActivities()
        {
            var hydroModelBuilder = new HydroModelBuilder();
            var hydroModel = hydroModelBuilder.BuildModel(ModelGroup.All);

            var retrievedHydroModel = SaveAndRetrieveObject(hydroModel);

            Assert.AreEqual(5, retrievedHydroModel.Activities.Count);
            Assert.AreEqual(4, retrievedHydroModel.Region.AllRegions.Count());
        }

        [Test]
        public void SaveLoadHydroModelWithCurrentWorkflowData()
        {
            var hydroModelBuilder = new HydroModelBuilder();
            var hydroModel = hydroModelBuilder.BuildModel(ModelGroup.OverLandFlow1D2D);

            var workFlow = hydroModel.CurrentWorkflow as Iterative1D2DCoupler;
            Assert.NotNull(workFlow);

            var couplerData = ((Iterative1D2DCouplerData)workFlow.Data);
            couplerData.MaxIteration = 5;
            couplerData.MaxError = 2;
            couplerData.Debug = true;

            var retrievedHydroModel = SaveAndRetrieveObject(hydroModel);
            
            var retrievedCouplerData = (Iterative1D2DCouplerData) ((Iterative1D2DCoupler) retrievedHydroModel.CurrentWorkflow).Data;

            Assert.AreEqual(5,retrievedCouplerData.MaxIteration);
            Assert.AreEqual(2, retrievedCouplerData.MaxError);
            Assert.AreEqual(true, retrievedCouplerData.Debug);

        }

        [Test]
        public void SaveLoadAndReSaveCompositeHydroModelWorkFlowData()
        {
            var compositeHydroModelWorkFlowData = new CompositeHydroModelWorkFlowData
                {
                    HydroModelWorkFlowDataLookUp = new Dictionary<IHydroModelWorkFlowData, IList<int>>
                        {
                            {new Iterative1D2DCouplerData(), new List<int>(new[]{1,4,7,2})}
                        }
                };

            var retrievedcompositeHydroModelWorkFlowData = SaveAndRetrieveObject(compositeHydroModelWorkFlowData);
            var loadedIterative1D2DCouplerData = (Iterative1D2DCouplerData)retrievedcompositeHydroModelWorkFlowData.WorkFlowDatas.First();

            loadedIterative1D2DCouplerData.MaxIteration = 2;

            // resave
            ProjectRepository.SaveOrUpdate(ProjectRepository.GetProject());

            Assert.AreEqual(new List<int>(new[] { 1, 4, 7, 2 }), retrievedcompositeHydroModelWorkFlowData.HydroModelWorkFlowDataLookUp[loadedIterative1D2DCouplerData]);
        }

        [Test]
        public void SaveAndLoadIterative1D2DCouplerData()
        {
            var iterative1D2DCouplerData = new Iterative1D2DCouplerData
                {
                    MaxError = 5,
                    MaxIteration = 6,
                    Debug = true
                };

            var retrievedIterative1D2DCouplerData = SaveAndRetrieveObject(iterative1D2DCouplerData);

            Assert.AreEqual(5, retrievedIterative1D2DCouplerData.MaxError);
            Assert.AreEqual(6, retrievedIterative1D2DCouplerData.MaxIteration);
            Assert.AreEqual(true, retrievedIterative1D2DCouplerData.Debug);
        }

        [Test]
        public void SaveLoadHydroModelWithCurrentWorkflow()
        {
            var hydroModelBuilder = new HydroModelBuilder();
            var hydroModel = hydroModelBuilder.BuildModel(ModelGroup.All);

            hydroModel.CurrentWorkflow = hydroModel.Workflows.ElementAt(2);

            var retrievedHydroModel = SaveAndRetrieveObject(hydroModel);
            Assert.AreEqual(2, retrievedHydroModel.Workflows.IndexOf(retrievedHydroModel.CurrentWorkflow));
        }

        [Test]
        public void SaveHydroModelDoesNotCausePropertyChangeOnWorkflowTools9667()
        {
            var hydroModelBuilder = new HydroModelBuilder();
            var hydroModel = hydroModelBuilder.BuildModel(ModelGroup.All);

            hydroModel.CurrentWorkflow = hydroModel.Workflows.ElementAt(2);

            var path = TestHelper.GetCurrentMethodName() + "1.dsproj";
            var project = new Project();
            project.RootFolder.Add(hydroModel);

            ProjectRepository.Create(path);

            try
            {
                var workFlowBefore = hydroModel.CurrentWorkflow;
                ProjectRepository.SaveOrUpdate(project);
                var workFlowAfter = hydroModel.CurrentWorkflow;

                Assert.AreSame(workFlowBefore, workFlowAfter);
            }
            finally
            {
                ProjectRepository.Close();
            }
        }

        [Test]
        public void SaveLoadDataItemWithConverter()
        {
            var converter = new HydroRegionFeatureCoverageFromNetworkCoverageValueConverter
            {
                OriginalValue =
                    new FeatureCoverage { Arguments = { new Variable<DateTime>(), new Variable<IFeature>() } },
                ConvertedValue = new NetworkCoverage("b", true),
                HydroRegion = new HydroRegion()
            };

            var dataItem = new DataItem
                {
                    ValueType = typeof (INetworkCoverage),
                    ValueConverter = converter
                };
            var folder = new Folder();
            folder.Items.Add(dataItem);

            var folderAfterLoad = SaveAndRetrieveObject(folder); //cannot use test method with dataitem directly
            var dataItemAfterLoad = folderAfterLoad.DataItems.First();
            var converterAfterLoad = dataItemAfterLoad.ValueConverter as HydroRegionFeatureCoverageFromNetworkCoverageValueConverter;

            converterAfterLoad.OriginalValue.Should().Not.Be.Null();
            converterAfterLoad.ConvertedValue.Should().Not.Be.Null();
            converterAfterLoad.HydroRegion.Should().Not.Be.Null();
        }

        [Test]
        public void SaveLoadHydroRegionFeatureCoverageFromNetworkCoverageValueConverter()
        {
            var converter = new HydroRegionFeatureCoverageFromNetworkCoverageValueConverter
                {
                    OriginalValue =
                        new FeatureCoverage {Arguments = {new Variable<DateTime>(), new Variable<IFeature>()}},
                    ConvertedValue = new NetworkCoverage("b", true),
                    HydroRegion = new HydroRegion()
                };

            var converterAfterLoad = SaveAndRetrieveObject(converter);

            converterAfterLoad.OriginalValue.Should().Not.Be.Null();
            converterAfterLoad.ConvertedValue.Should().Not.Be.Null();
            converterAfterLoad.HydroRegion.Should().Not.Be.Null();
        }

        [Test]
        public void SaveLoadHydroLinksFeatureCoverageValueConverter()
        {
            var converter = new HydroLinksFeatureCoverageValueConverter
            {
                OriginalValue = new FeatureCoverage { Arguments = { new Variable<DateTime>(), new Variable<IFeature>() } },
                ConvertedValue = new FeatureCoverage { Arguments = { new Variable<DateTime>(), new Variable<IFeature>() } },
                HydroRegion = new HydroRegion()
            };

            var converterAfterLoad = SaveAndRetrieveObject(converter);

            converterAfterLoad.OriginalValue.Should().Not.Be.Null();
            converterAfterLoad.ConvertedValue.Should().Not.Be.Null();
            converterAfterLoad.HydroRegion.Should().Not.Be.Null();
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void RunFailingFlowFMFromIntegratedModelConnectsOutputFromDimrCorrectly()
        {
            /* Issue DELFT3DFM-838, when this model is executed directly from the FlowFM it fails (and it's expected to fail)
                 the problem, however, is that FlowFM is not generating any log file because it crashes while initializing. */

            var projectPath = TestHelper.GetTestFilePath(@"integratedModel\modelRunFails.dsproj");
            projectPath = TestHelper.CreateLocalCopy(projectPath);
            Assert.IsTrue(File.Exists(projectPath));

            using (var app = new DeltaShellApplication())
            {
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new HydroModelApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new RealTimeControlApplicationPlugin());

                app.Run();
                app.OpenProject(projectPath);
                var hydroModel = app.Project.RootFolder.Models.OfType<HydroModel>().FirstOrDefault();
                Assert.IsNotNull(hydroModel);
                Assert.IsTrue(hydroModel.Activities.Any());
                var flowFM = hydroModel.Activities.OfType<WaterFlowFMModel>().FirstOrDefault();
                Assert.IsNotNull(flowFM);

                ActivityRunner.RunActivity(flowFM);

                //Check if the dia file has been generated.
                CheckLogFileGeneratedForHydroModel(hydroModel, DimrRunner.DimrRunLogfileDataItemTag);
            }
            var directoryPath = Path.GetDirectoryName(projectPath);
            FileUtils.DeleteIfExists(directoryPath);
        }

        private static void CheckLogFileGeneratedForHydroModel(IHydroModel model, string logTag)
        {
            var logFileDataItem = model.DataItems.FirstOrDefault(di => di.Tag == logTag);
            Assert.IsNotNull(logFileDataItem, "No log item was created.");
            Assert.IsNotNull(logFileDataItem.Value);
            var textLogFile = ((TextDocument) logFileDataItem.Value).Content;
            Assert.IsNotNull(textLogFile);
        }
    }
}