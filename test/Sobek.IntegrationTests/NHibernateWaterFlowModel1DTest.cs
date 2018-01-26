using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Editing;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.Core;
using DeltaShell.Core.Services;
using DeltaShell.Gui;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.Data.NHibernate.DelftTools.Shell.Core.Dao;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Roughness;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel;
using DeltaShell.Plugins.ImportExport.Sobek;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.Scripting;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms.CoverageViews;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using log4net.Core;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;
using SharpMap;
using SharpMap.Layers;
using SharpMap.UI.Forms;
using SharpTestsEx;
using AggregationOptions = DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi.AggregationOptions;
using Application = System.Windows.Forms.Application;
using ElementSet = DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi.ElementSet;
using GeometryFactory = SharpMap.Converters.Geometries.GeometryFactory;
using Point = NetTopologySuite.Geometries.Point;
using QuantityType = DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi.QuantityType;

namespace Sobek.IntegrationTests
{
    [TestFixture]
    [NUnit.Framework.Category(TestCategory.DataAccess)]
    [NUnit.Framework.Category(TestCategory.Slow)]
    public class NHibernateWaterFlowModel1DTest
    {
        private NHibernateProjectRepository projectRepository;
        private NHibernateProjectRepositoryFactory factory;
        private long totalMemoryBeforeTest;
        private long maximumExpectedMemoryLeak;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            LogHelper.SetLoggingLevel(Level.Error);

            factory = new NHibernateProjectRepositoryFactory();
            factory.AddPlugin(new SharpMapGisApplicationPlugin());
            factory.AddPlugin(new CommonToolsApplicationPlugin());
            factory.AddPlugin(new NetCdfApplicationPlugin());
            factory.AddPlugin(new ScriptingApplicationPlugin());

            factory.AddPlugin(new NetworkEditorApplicationPlugin());

            factory.AddPlugin(new HydroModelApplicationPlugin());
            factory.AddPlugin(new WaterFlowModel1DApplicationPlugin());
            factory.AddPlugin(new RealTimeControlApplicationPlugin());
            factory.AddPlugin(new RainfallRunoffApplicationPlugin());
            factory.AddPlugin(new WaterQualityModelApplicationPlugin());
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            LogHelper.ResetLogging();
        }
        
        [SetUp]
        public void SetUp()
        {
            maximumExpectedMemoryLeak = 0;
            totalMemoryBeforeTest = GC.GetTotalMemory(true);

#if PROFILE_MEMORY
            MessageBox.Show("Start memory profiler");
#endif

            projectRepository = factory.CreateNew();
        }

        /// <summary>
        /// Used for memory profiling (run nunit, run empty test, take snapshot, run test, take second snapshot).
        /// </summary>
        [Test]
        public void Empty()
        {
            
        }

        [TearDown]
        public void TearDown()
        {
            projectRepository.Dispose();
            projectRepository = null;

            // aggresive collect..needed because GC is so lazy it goes out of mem (?!?)
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
#if PROFILE_MEMORY
            MessageBox.Show("Stop memory profiler");
#endif

            LogHelper.SetLoggingLevel(Level.Debug);

            var totalMemoryAfterTest = GC.GetTotalMemory(true);
            Console.WriteLine(@"Total memory leak after test: " + (totalMemoryAfterTest - totalMemoryBeforeTest)/(1024.0 * 1000.0) + @" Mb");
            (totalMemoryAfterTest - totalMemoryBeforeTest)
                .Should("memory leak should be within expectations").Be.LessThan(maximumExpectedMemoryLeak == 0 ? 40000000 : maximumExpectedMemoryLeak); // TODO: analyze it! 

            LogHelper.ResetLogging();
        }

        private static void RunModel(WaterFlowModel1D flowModel1D)
        {
            ActivityRunner.RunActivity(flowModel1D);

            if (flowModel1D.Status == ActivityStatus.Failed)
            {
                Assert.Fail("Model run has failed");
            }
        }

        private void SaveBypassModel(string path, bool clearDiscretization)
        {
            //import by pass
            var modelPath =
                TestHelper.GetTestDataPath(
                    typeof(SobekWaterFlowModel1DImporterTest).Assembly, @"BYPASS.lit\3\Network.TP");

            var importedModel = (HydroModel)new SobekHydroModelImporter().ImportItem(modelPath);
            var flowModel = importedModel.Activities.OfType<WaterFlowModel1D>().FirstOrDefault();

            if (clearDiscretization)
            {
                flowModel.NetworkDiscretization.Clear();
            }
            using (var projectRepository = factory.CreateNew())
            {
                projectRepository.Create(path);
                //create a project with the model and save it
                var project = projectRepository.GetProject();
                project.RootFolder.Items.Add(importedModel);
                projectRepository.SaveOrUpdate(project);
                projectRepository.Close();
            }
        }

        public static Project GetProjectFor(IBranchFeature branchFeature)
        {
            var network = new HydroNetwork();
            var branch = new Channel();
            network.Branches.Add(branch);
            var node1 = new HydroNode { Network = network };
            var node2 = new HydroNode { Network = network };
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            branch.Source = node1;
            branch.Target = node2;

            branchFeature.Branch = branch;

            branch.BranchFeatures.Add(branchFeature);

            var project = new Project();

            project.RootFolder.Add(network);
            return project;
        }

        protected T SaveAndRetrieveObject<T>(T objectToSave) where T : class
        {
            var path = TestHelper.GetCurrentMethodName() + "1.dsproj";
            var project = new Project();
            project.RootFolder.Add(objectToSave);

            projectRepository.Create(path);
            projectRepository.SaveOrUpdate(project);
            projectRepository.Close();

            var np = projectRepository.Open(path);
            Assert.IsNotNull(np.RootFolder.Items[0]);

            var retrievedEntry = np.RootFolder.Items[0];

            if (retrievedEntry is IDataItem)
            {
                return ((IDataItem)np.RootFolder.Items[0]).Value as T;
            }

            if (retrievedEntry is IModel || retrievedEntry is Folder)
            {
                return retrievedEntry as T;
            }
            return null;
        }

        /// <summary>
        /// same as DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.WaterFlowModel1DTestHelper
        /// adding reference to DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests breaks tests on buildserver
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="name"></param>
        /// <param name="chainage"></param>
        private static void AddDefaultCrossSection(IChannel channel, string name, double chainage)
        {
            var yzCoordinates = new List<Coordinate>
                                    {
                                        new Coordinate(0.0, 0.0),
                                        new Coordinate(100.0, 0.0),
                                        new Coordinate(150.0, -10.0),
                                        new Coordinate(300.0, -10.0),
                                        new Coordinate(350.0, 0.0),
                                        new Coordinate(500.0, 0.0)
                                    };

            var cs1 = CrossSectionHelper.AddXYZCrossSectionFromYZCoordinates(channel, chainage, yzCoordinates);
            cs1.Definition.Name = name;
        }

        private static HydroNetwork CreateSimplerNetwork()
        {
            INode inNode, outNode;
            return CreateSimplerNetwork(out inNode, out outNode);
        }

        /// <summary>
        /// same as DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.WaterFlowModel1DTestHelper
        /// adding reference to DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests breaks tests on buildserver
        /// </summary>
        /// <param name="inflowNode"></param>
        /// <param name="outflowNode"></param>
        /// <returns></returns>
        private static HydroNetwork CreateSimplerNetwork(out INode inflowNode, out INode outflowNode)
        {
            var network = new HydroNetwork();

            // add nodes and branches
            var node1 = new HydroNode { Name = "node1", Network = network };
            var node2 = new HydroNode { Name = "node2", Network = network };

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var branch1 = new Channel("branch1", node1, node2, 100.0);
            var vertices = new List<Coordinate>
                               {
                                   new Coordinate(0, 0),
                                   new Coordinate(100, 0)
                               };
            branch1.Geometry = GeometryFactory.CreateLineString(vertices.ToArray());


            network.Branches.Add(branch1);

            // add boundary sources

            inflowNode = node1;

            outflowNode = node2;

            // add cross-sections
            AddDefaultCrossSection(branch1, "crs1", 40);
            return network;
        }

        private static DeltaShellGui GetRunningGuiWithFlowPlugins()
        {
            var gui = new DeltaShellGui();

            gui.Application.Plugins.Add(new HydroModelApplicationPlugin());
            gui.Application.Plugins.Add(new NetCdfApplicationPlugin());
            gui.Application.Plugins.Add(new NHibernateDaoApplicationPlugin());
            gui.Application.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
            gui.Application.Plugins.Add(new NetworkEditorApplicationPlugin());
            gui.Application.Plugins.Add(new SharpMapGisApplicationPlugin());
            gui.Application.Plugins.Add(new CommonToolsApplicationPlugin());

            gui.Plugins.Add(new WaterFlowModel1DGuiPlugin());
            gui.Plugins.Add(new NetworkEditorGuiPlugin());
            gui.Plugins.Add(new ProjectExplorerGuiPlugin());
            gui.Plugins.Add(new SharpMapGisGuiPlugin());
            gui.Plugins.Add(new CommonToolsGuiPlugin());

            gui.Run();
            return gui;
        }

        private static DeltaShellApplication GetRunningAppWithFlowPlugins()
        {
            var app = new DeltaShellApplication();

            app.Plugins.Add(new HydroModelApplicationPlugin());
            app.Plugins.Add(new NetCdfApplicationPlugin());
            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new CommonToolsApplicationPlugin());

            app.Run();
            return app;
        }

        private void SaveModelToProject(WaterFlowModel1D model1, string path)
        {
            using (var projectRepository = factory.CreateNew())
            {
                projectRepository.Create(path);
                var project = new Project();
                project.RootFolder.Items.Add(model1);
                projectRepository.SaveOrUpdate(project);
                projectRepository.Close();
            }
        }

        [Test]
        public void MigrateFlow1DModel64BitPlatformShouldNotFailOnFirstRun() // Jira issue: SOBEK3-453
        {
            using (var app = GetRunningAppWithFlowPlugins())
            {
                var path = TestHelper.GetTestFilePath(@"SOBEK3-453\saltvoorjan.dsproj");
                path = TestHelper.CopyProjectToLocalDirectory(path);

                app.OpenProject(path);

                var model = (ITimeDependentModel)app.Project.RootFolder.Models.First();

                model.StopTime = model.StartTime + model.TimeStep; // 1 time step

                app.RunActivityInBackground(model); // should not throw exception because of lazy loading

                while (app.IsActivityRunningOrWaiting(model))
                {
                    Thread.Sleep(0);
                    Application.DoEvents();
                }

                Assert.AreNotEqual(ActivityStatus.Failed, model.Status, "SOBEK3-453: Migrated flow1D model on 64Bit platform failed on first run");
            }

            maximumExpectedMemoryLeak = 80000000; // NH is not cleaned for the first time.
        }

        [Test]
        public void OpenAndRunModelShouldNotCrashWithThreadingExceptionTools7612()
        {
            using (var app = GetRunningAppWithFlowPlugins())
            {
                var path = TestHelper.GetTestFilePath(@"TOOLS-7612\23.dsproj");
                path = TestHelper.CopyProjectToLocalDirectory(path);

                app.OpenProject(path);

                var model = (ITimeDependentModel) app.Project.RootFolder.Models.First();

                model.StopTime = model.StartTime + model.TimeStep; // 1 time step

                app.RunActivityInBackground(model); // should not throw exception because of lazy loading

                while (app.IsActivityRunningOrWaiting(model))
                {
                    Thread.Sleep(0);
                    Application.DoEvents();
                }
            }

            maximumExpectedMemoryLeak = 80000000; // NH is not cleaned for the first time.
        }

        [Test]
        public void SaveModelFollowedByExport()
        {
            var hybridProjectRepository = new HybridProjectRepository(factory);
            var project = new Project();
            var data = new WaterFlowModel1D();
            var dataItem = new DataItem(data);
            
            project.RootFolder.Items.Add(dataItem);

            string path = TestHelper.GetCurrentMethodName() + ".dsproj";
            string path2 = TestHelper.GetCurrentMethodName() + "_export" + ".dsproj";
            
            hybridProjectRepository.SaveProjectAs(project, path);
            hybridProjectRepository.Export(dataItem, path2);
        }

        [Test]
        public void WaterFlowModel1DOutputSettingsWhenProjectIsSavedOrLoadedCanBeSavedAndLoaded()
        {
            var settings = new WaterFlowModel1DOutputSettingData();
            var properties = settings.GetType().GetProperties();
            var rnd = new Random();            
            foreach (var propertyInfo in properties)
            {
                if (propertyInfo.PropertyType == typeof(AggregationOptions))
                {
                    propertyInfo.SetValue(settings, (AggregationOptions)rnd.Next(0, 4), null);
                }
            }

            var outputTimeStep = new TimeSpan(1,1,1,0);
            settings.GridOutputTimeStep = outputTimeStep;
            settings.StructureOutputTimeStep = outputTimeStep;

            string path = TestHelper.GetCurrentMethodName() + ".dsproj";

            // create and save the project and settings
            projectRepository.Create(path);
            Project project = projectRepository.GetProject();
            project.RootFolder.Add(settings);
            projectRepository.SaveOrUpdate(project);
            
            // load the project and settings
            projectRepository = factory.CreateNew();
            projectRepository.Open(path);
            Project retrievedProject = projectRepository.GetProject();

            var retrievedSettings = (WaterFlowModel1DOutputSettingData)retrievedProject.RootFolder.Items.OfType<IDataItem>().Select(item => item.Value).First();            
            
            for (int i = 0; i < properties.Length; i++)
            {
                if (properties[i].PropertyType.IsValueType)
                {
                    var retrievedValue = properties[i].GetValue(retrievedSettings, null);
                    var value = properties[i].GetValue(settings, null);

                    Assert.AreEqual(value, retrievedValue, properties[i].Name);
                }
            }
        }

        [Test]
        public void WaterFlowModel1DOutputSettingsWhenProjectIsSavedOrLoadedCanBeSavedAndLoadedIntoModel()
        {
            var outputTimeStep = new TimeSpan(1, 1, 1, 1);
            var model = new WaterFlowModel1D();
            model.OutputSettings.GridOutputTimeStep = outputTimeStep;

            const string path = "outputsettingsdata.dsproj";

            // create and save the project and settings
            projectRepository.Create(path);
            Project project = projectRepository.GetProject();
            project.RootFolder.Add(model);
            projectRepository.SaveOrUpdate(project);

            // load the project and settings
            projectRepository = factory.CreateNew();
            projectRepository.Open(path);
            Project retrievedProject = projectRepository.GetProject();

            var retrievedModel = (WaterFlowModel1D)retrievedProject.RootFolder.Models.First();

            //add all assert abouts thing that should not have changed.
            Assert.IsNotNull(retrievedModel.OutputSettings);
            Assert.AreEqual(outputTimeStep, retrievedModel.OutputSettings.GridOutputTimeStep);
        }

        [Test]
        public void WaterFlowModel1DHasNoDoubleSaltConcentrationOutputCoverageAfterReload_Tools7556()
        {
            string path = TestHelper.GetCurrentMethodName() + ".dsproj";

            var flowModel1D = new WaterFlowModel1D
                                    {
                                        Network = new HydroNetwork()
                                    };

            projectRepository.Create(path);
            Project project = projectRepository.GetProject();
            project.RootFolder.Add(flowModel1D);

            //set salt output
            var saltParameter = flowModel1D.OutputSettings.GetEngineParameter(QuantityType.Salinity, ElementSet.GridpointsOnBranches);
            saltParameter.AggregationOptions = AggregationOptions.Current;

            //test start situation
            var saltConcentrationCoverageName = String.Format("{0}", WaterFlowModelParameterNames.LocationSaltConcentration);
            Assert.AreEqual(1, flowModel1D.OutputFunctions.Count(c => c.Name == saltConcentrationCoverageName));
            Assert.AreEqual(1, project.RootFolder.GetAllItemsRecursive().OfType<INetworkCoverage>().Count(nc => nc.Name == saltConcentrationCoverageName));

            //save
            projectRepository.SaveOrUpdate(project);
            projectRepository.Close();

            // load the project and settings
            projectRepository = factory.CreateNew();
            projectRepository.Open(path);
            Project retrievedProject = projectRepository.GetProject();

            var retrievedFlowModel1D = (WaterFlowModel1D)retrievedProject.RootFolder.Models.First();

            Assert.AreEqual(1, retrievedFlowModel1D.OutputFunctions.Count(c => c.Name == saltConcentrationCoverageName));
            Assert.AreEqual(1, retrievedProject.RootFolder.GetAllItemsRecursive().OfType<INetworkCoverage>().Count(nc => nc.Name == saltConcentrationCoverageName));

            projectRepository.Close();

        }

        [Test]
        public void Issue1311RegenerateDiscretizationAndSaveSimpleModel()
        {
            string path = TestHelper.GetCurrentMethodName() + ".dsproj";
            projectRepository.Create(path);
            var project = projectRepository.GetProject();
            
            // construct a simple network
            var network = CreateSimplerNetwork();
            
            // add discretization
            var networkDiscretization = new Discretization
            {
                Network = network,
                SegmentGenerationMethod =
                    SegmentGenerationMethod.SegmentBetweenLocationsFullyCovered
            };

            IDataItem dataItemDiscretz = new DataItem(networkDiscretization);
            
            foreach (IChannel channel in network.Channels)
            {              
                HydroNetworkHelper.GenerateDiscretization(networkDiscretization, channel, 0, false, 0.5, false, false, true,
                                                            channel.Length / 2.0);
            }

            var flowModel1D = new WaterFlowModel1D
                                  {
                                      Network = network
            };

            project.RootFolder.Add(dataItemDiscretz);
            project.RootFolder.Items.Add(flowModel1D);
            projectRepository.SaveOrUpdate(project);


            IDataItem modelDiscretization = null;
            //link model network coverage to network discretization
            foreach (var dataItem in flowModel1D.DataItems)
            {
                if (dataItem.Value.GetType() == typeof(Discretization))
                {
                    modelDiscretization = dataItem;
                    modelDiscretization.LinkTo(dataItemDiscretz);
                }
            }
            
            projectRepository.SaveOrUpdate(project);

            //unlink discretization from network coverage
            modelDiscretization.Unlink();

            flowModel1D.NetworkDiscretization.Clear();
            projectRepository.SaveOrUpdate(project);

            //create new networkdiscretization (regenerate)
            var newDiscretization = new Discretization
            {
                Network = network,
                SegmentGenerationMethod =
                    SegmentGenerationMethod.SegmentBetweenLocationsFullyCovered
            };

            foreach (IChannel channel in network.Channels)
            {
                HydroNetworkHelper.GenerateDiscretization(newDiscretization, channel, 0, false, 0.5, false, false, true,
                                                            channel.Length / 4.0);
            }

            IDataItem newDataItemDiscr = new DataItem(newDiscretization);
            project.RootFolder.Add(newDataItemDiscr);
            IDataItem networkCoverage2 = null;
            //link model network coverage to network discretization
            foreach (var dataItem in flowModel1D.DataItems)
            {
                if (dataItem.Value.GetType() == typeof(Discretization))
                {
                    networkCoverage2 = dataItem;
                    networkCoverage2.LinkTo(newDataItemDiscr);
                }
            }

            projectRepository.SaveOrUpdate(project);
            // save again sometimes leads to error, see issue 1311
            projectRepository.SaveOrUpdate(project);

            projectRepository.Close();
            projectRepository.Dispose();
            
        }

        [Test]
        public void DataItemLinkTo()
        {
            string path = TestHelper.GetCurrentMethodName() + ".dsproj";
            projectRepository.Create(path);
            var project = projectRepository.GetProject();

            // construct a simple network
            var network = CreateSimplerNetwork();

            var newDiscretization = new Discretization
            {
                Network = network,
                SegmentGenerationMethod =
                    SegmentGenerationMethod.SegmentBetweenLocationsFullyCovered
            };

            using (var flowModel1D = new WaterFlowModel1D { NetworkDiscretization = newDiscretization })
            {
                project.RootFolder.Items.Add(flowModel1D);

                var dataItemSource = new DataItem(newDiscretization);
                var dataItemTarget = new DataItem(newDiscretization);

                project.RootFolder.Items.Add(dataItemSource);
                project.RootFolder.Items.Add(dataItemTarget);

                projectRepository.SaveOrUpdate(project);

                dataItemSource.LinkTo(dataItemTarget);
                bool linked = dataItemSource.IsLinked;
                projectRepository.SaveOrUpdate(project);
                projectRepository.Dispose();
                Assert.IsTrue(linked);
            }
        }

        [Test]
        public void SplitChannelForgetCrossSectionAndSave()
        {
            //reproduces issue 5277
            var path = TestHelper.GetCurrentMethodName() + ".dsproj";
            projectRepository.Create(path);
            var project = projectRepository.GetProject();

            var csst = new CrossSectionSectionType {Name = "MyCrossSectionSectionType"};
            var hydroNetwork = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(100, 0));
            hydroNetwork.CrossSectionSectionTypes.Add(csst);

            var crossSection1 = CrossSectionDefinitionZW.CreateDefault();
            crossSection1.AddSection(csst, crossSection1.FlowWidth());
            HydroNetworkHelper.AddCrossSectionDefinitionToBranch(hydroNetwork.Branches[0], crossSection1, 1.0);
            
            var networkDiscretization = new Discretization
            {
                Network = hydroNetwork,
                SegmentGenerationMethod =
                    SegmentGenerationMethod.SegmentBetweenLocationsFullyCovered
            };
            HydroNetworkHelper.GenerateDiscretization(networkDiscretization, true, false, 200, false,
                                                      0.5, false, false, true, 200);
            var startTime = DateTime.Now;
            var flowModel1D = new WaterFlowModel1D
            {
                NetworkDiscretization = networkDiscretization,
                StartTime = startTime,
                StopTime = startTime.AddMinutes(5),
                TimeStep = new TimeSpan(0, 1, 0),
                OutputTimeStep = new TimeSpan(0, 1, 0),
                Network = hydroNetwork
            };

            project.RootFolder.Add(hydroNetwork);
            project.RootFolder.Add(flowModel1D);

            projectRepository.SaveOrUpdate(project);

            RunModel(flowModel1D);

            projectRepository.SaveOrUpdate(project);
            HydroNetworkHelper.SplitChannelAtNode(hydroNetwork.Channels.First(), 50.0d);

            projectRepository.SaveOrUpdate(project);

            try
            {
                flowModel1D.Initialize();
            }
            catch (Exception)
            {
            }

            var crossSection2 = CrossSectionDefinitionZW.CreateDefault();
            HydroNetworkHelper.AddCrossSectionDefinitionToBranch(hydroNetwork.Branches[1], crossSection2, 1.0);
            
            projectRepository.SaveOrUpdate(project);
        }

        [Test]
        public void GenerateDiscretizationLinkToNetworkAndSaveSimpleModelAndCheckIfRetreivedModelIsLinked()
        {
            string path = "GenerateDiscretization~1.dsproj";
            projectRepository.Create(path);
            var project = new Project();

            // construct a simple network
            var network = CreateSimplerNetwork();

            // add discretization
            var networkDiscretization = new Discretization
            {
                Network = network,
                SegmentGenerationMethod =
                    SegmentGenerationMethod.SegmentBetweenLocationsFullyCovered
            };

            foreach (IChannel channel in network.Channels)
            {
                HydroNetworkHelper.GenerateDiscretization(networkDiscretization, channel, 0, false, 0.5, false, false, true,
                                                            channel.Length / 2.0);
            }

            using (var flowModel1D = new WaterFlowModel1D
                                            {
                                                Network = network
                                                //NetworkDiscretization = networkDiscretization
                                            })
            {

                project.RootFolder.Items.Add(flowModel1D);
                projectRepository.SaveOrUpdate(project);

                IDataItem dataItemDiscretz = new DataItem(networkDiscretization);
                IDataItem networkCoverage = null;
                //link model network coverage to network discretization
                foreach (var dataItem in flowModel1D.DataItems)
                {
                    if (dataItem.Value.GetType() == typeof (Discretization))
                    {
                        networkCoverage = dataItem;
                        networkCoverage.LinkTo(dataItemDiscretz);
                    }
                }
                project.RootFolder.Items.Add(dataItemDiscretz);
                projectRepository.SaveOrUpdate(project);

                // retrieve the networkcoverage from model and check if still unlinked
                projectRepository = factory.CreateNew();
                projectRepository.Open(path);
                Project project2 = projectRepository.GetProject();
                WaterFlowModel1D model2 = null;
                var itemsInProject2 = project2.RootFolder.GetAllItemsRecursive();
                foreach (var item in itemsInProject2)
                {
                    if (item.GetType() == typeof (WaterFlowModel1D))
                    {
                        model2 = (WaterFlowModel1D) item;
                    }
                }
                IDataItem networkCoverage2 = null;
                foreach (var dataItem in model2.DataItems)
                {
                    if (dataItem.Value.GetType() == typeof (Discretization))
                    {
                        networkCoverage2 = dataItem;
                    }
                }

                bool linked;
                linked = networkCoverage2.IsLinked;
                projectRepository.Dispose();
                Assert.IsTrue(linked);
            }
        }

        [Test]
        public void Issue1120SaveNetworkAfterBranchIsDeletedShouldNotFail()
        {
            using (var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork())
            {
                var project = new Project();
                project.RootFolder.Add(model);

                const string path = "SaveNetworkAfterBranchIsDeletedShouldNotFail.dsproj";

                var mapControl = new MapControl();
                var networkMapLayer = MapLayerProviderHelper.CreateLayersRecursive(model.Network, null, new List<IMapLayerProvider> {new NetworkEditorMapLayerProvider()});
                mapControl.Map.Layers.Add(networkMapLayer);
                HydroRegionEditorHelper.AddHydroRegionEditorMapTool(mapControl);

                model.Network.Branches.RemoveAt(1);

                projectRepository.Create(path);
                projectRepository.SaveOrUpdate(project);
            }
        }
  
        [Test]
        [NUnit.Framework.Category(TestCategory.WindowsForms)]
        public void DragOutputCoverageToExternalMapSaveRemoveMapAndSaveAgainShouldNotCrash()
        {
            using (var gui = GetRunningGuiWithFlowPlugins())
            {
                var path1 = "drag1.dsproj";
                var path2 = "drag2.dsproj";

                var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();

                gui.Application.Project.RootFolder.Add(model);

                RunModel(model);

                //add map to project
                var map = new Map();
                gui.Application.Project.RootFolder.Add(map);

                //add coverage to map
                var coverageLayer = SharpMapLayerFactory.CreateMapLayerForCoverage(model.OutputFlow, map);
                map.Layers.Add(coverageLayer);

                var mainWindow = (Window)gui.MainWindow;

                mainWindow.IsVisibleChanged += delegate
                                        {
                                            if (!mainWindow.IsVisible)
                                            {
                                                return;
                                            }

                                            gui.Application.SaveProjectAs(path1);

                                            var mapItem = gui.Application.Project.RootFolder.DataItems.First(di => di.Value == map);
                                            gui.Application.Project.RootFolder.Items.Remove(mapItem);

                                            gui.Application.SaveProjectAs(path2);
                                        };

                WpfTestHelper.ShowModal(mainWindow);
            }
        }

        [Test]
        public void SaveModelAfterDeletingSavedSharedDefinition()
        {
            using (var app = GetRunningAppWithFlowPlugins())
            {
                var path = "qq324.dsproj";
                app.SaveProjectAs(path); // to initialize file storing

                var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();

                app.Project.RootFolder.Add(model);

                var cs = CrossSection.CreateDefault();
                NetworkHelper.AddBranchFeatureToBranch(cs, model.Network.Branches[0], 15);
                cs.ShareDefinitionAndChangeToProxy();

                app.SaveProjectAs(path);

                cs.MakeDefinitionLocal();
                model.Network.SharedCrossSectionDefinitions.Clear();
                
                app.SaveProject();
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.WindowsForms)]
        public void SaveAsModelTwiceWithOpenCoverageViewShouldNotGiveException()
        {
            using (var gui = GetRunningGuiWithFlowPlugins())
            {
                var path1 = "1.dsproj";
                var path2 = "2.dsproj";

                var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();

                gui.Application.Project.RootFolder.Add(model);

                RunModel(model);

                model.OutputFunctions.Count()
                    .Should("Model ran: should have output coverages").Be.GreaterThan(0);

                var mainWindow = (Window)gui.MainWindow;

                mainWindow.Loaded += delegate
                                        {
                                            gui.DocumentViewsResolver.OpenViewForData(model.OutputFlow, typeof(CoverageTableView));
                                            
                                            gui.Application.SaveProjectAs(path1);
                                            gui.Application.SaveProjectAs(path2); //exception
                                            Assert.Greater(model.OutputFunctions.Count(),0);
                                        };

                WpfTestHelper.ShowModal(mainWindow);
            }
        }

        [Test]
        public void Issue1252LoadModelWithNetworkAndWaterLevelShouldNotFail()
        {
            const string path = @"Issue1252LoadModel.dsproj";
            int expectedOutputWaterLevelCount;

            using (var app = GetRunningAppWithFlowPlugins())
            {
                app.SaveProjectAs(path); // to initialize file storing

                var project = app.Project;
                Map map = new Map();
                project.RootFolder.Add(map);

                var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
                project.RootFolder.Add(model);

                //when save/update is not carried out before the mudel run, the branch ids in the network are not unique
                app.SaveProject();
                LogHelper.ResetLogging();

                ActivityRunner.RunActivity(model);

                //adding a networkcoverage layer first puts it first in the DB
                var networkCoverageLayer = new NetworkCoverageGroupLayer {NetworkCoverage = model.OutputWaterLevel};
                map.Layers.Add(networkCoverageLayer);

                var networkMapLayer = MapLayerProviderHelper.CreateLayersRecursive(model.Network, null, new List<IMapLayerProvider> { new NetworkEditorMapLayerProvider() });
                map.Layers.Add(networkMapLayer);

                //read it before closing the repository
                expectedOutputWaterLevelCount = model.OutputWaterLevel.Components[0].Values.Count;

                app.SaveProjectAs(path);
                app.CloseProject();
            }

            // Reloading the project produced exception like in 1252
            using (var app = GetRunningAppWithFlowPlugins())
            {
                app.OpenProject(path);

                var retrievedModel = (WaterFlowModel1D) app.Project.RootFolder.Models.FirstOrDefault();
                var retrievedWaterLevel = retrievedModel.OutputWaterLevel;
                Assert.IsNotNull(retrievedWaterLevel.Store);

                Assert.AreEqual(expectedOutputWaterLevelCount, retrievedWaterLevel.Components[0].Values.Count);
            }

        }

        [Test]
        public void WriteDelftFlowModel1D()
        {
            string path = TestHelper.GetCurrentMethodName() + ".dsproj";
            projectRepository.Create(path);

            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();

            Project project = projectRepository.GetProject();
            project.RootFolder.Items.Add(model);
            project.RootFolder.Items.Add(new DataItem(model.Network));
            projectRepository.SaveOrUpdate(project);
            projectRepository.Close();

            //retrieve the model again and verify we have a boundary condition
            Project loadedProject = projectRepository.Open(path);
            var retrievedModel = (WaterFlowModel1D)loadedProject.RootFolder.Models.FirstOrDefault();
            
            //verify the retrieved object is OK
            Assert.AreEqual(model.BoundaryConditions.Count(), retrievedModel.BoundaryConditions.Count());
            Assert.AreEqual(model.DataItems.Count, retrievedModel.DataItems.Count);

            //assert the event wiring from dataitem is ok
            var propChanged = "";
            var network = retrievedModel.DataItems.FirstOrDefault(di => di.Value is INetwork);
            var notifyPropertyChanged = network as INotifyPropertyChanged;
            if (notifyPropertyChanged != null)
            {
                notifyPropertyChanged.PropertyChanged +=
                    delegate(object sender, PropertyChangedEventArgs e)
                        {
                            propChanged = e.PropertyName;
                        };
            }
            retrievedModel.Network.Name = "new name";
            Assert.AreEqual(propChanged, "Name");
            var fbc2 = retrievedModel.BoundaryConditions.First();
            var fbc1 = model.BoundaryConditions.First();
            Assert.AreEqual(fbc1.Data.Name, fbc2.Data.Name);
            Assert.AreEqual(fbc1.Feature.Name, fbc2.Feature.Name);
        }

        [Test]
        public void SaveAndRetrieveWaterFlowModel1DWithOutputOutOfSync()
        {
            var waterFlowModel1D = new WaterFlowModel1D();
            TypeUtils.SetPrivatePropertyValue(waterFlowModel1D, "OutputOutOfSync", true);

            // Save model
            projectRepository.Create(TestHelper.GetCurrentMethodName() + ".dsproj");
            var project = projectRepository.GetProject();
            project.RootFolder.Items.Add(waterFlowModel1D);
            projectRepository.SaveOrUpdate(project);
            projectRepository.Close();

            // Retrieve model
            var loadedProject = projectRepository.Open(TestHelper.GetCurrentMethodName() + ".dsproj");
            var retrievedModel = loadedProject.RootFolder.Models.FirstOrDefault();

            Assert.IsNotNull(retrievedModel);
            Assert.IsTrue(retrievedModel.OutputOutOfSync);
        }

        [Test]
        public void SavingAModelShouldNotClearData()
        {
            using (var app = GetRunningAppWithFlowPlugins())
            {
                // cleanup
                File.Delete("1.dsproj");
                FileUtils.DeleteIfExists("1.dsproj_data");

                //save the project before the model is run
                string projectPath = "1.dsproj"; //don't use current method name..filename then is too long for some build agents 
                app.SaveProjectAs(projectPath);
                
                var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
                model.OutputSettings.LocationWaterDepth = AggregationOptions.Current;

                app.Project.RootFolder.Items.Add(model);
                app.SaveProjectAs(projectPath);
                
                //run the model
                LogHelper.ResetLogging();

                ActivityRunner.RunActivity(model);
                
                //save the project before the model is run
                app.SaveProjectAs("newPath.dsproj");
                
                Assert.AreEqual(11, model.OutputDepth.Arguments[0].Values.Count);
            }
        }

        [Test]
        public void ExportModelDoesNotRequireSavingFirst()
        {
            var path = TestHelper.GetCurrentMethodName() + ".dsproj";

            using (var gui =GetRunningGuiWithFlowPlugins())
            {
                //create model
                var model1 = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
                
                gui.Application.Project.RootFolder.Items.Add(model1);
                
                //export model
                var projectExporter = new ProjectItemExporter();
                projectExporter.HybridProjectRepository = ((DeltaShellApplication) gui.Application).HybridProjectRepository;
                projectExporter.ProjectPath = path;
                projectExporter.Export(model1, path); //export (clone) doesn'type clone outputcoverages...

                //should get here without exceptions
            }
        }

        [Test]
        public void ImportAndExportShouldPreserveOutputCoverages()
        {
            var path = "qq.dsproj"; //using short filename, because path based on testname became too long.

            using (var gui = GetRunningGuiWithFlowPlugins())
            {
                //create model
                var model1 = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
                
                gui.Application.Project.RootFolder.Items.Add(model1);

                //run model
                RunModel(model1);
                
                //export model
                var projectExporter = new ProjectItemExporter();
                projectExporter.HybridProjectRepository = ((DeltaShellApplication)gui.Application).HybridProjectRepository;
                projectExporter.ProjectPath = path;
                projectExporter.Export(model1, path); //export (clone) doesn'type clone outputcoverages...

                int countBeforeImport = model1.OutputFunctions.Count();
                countBeforeImport.Should("Expected at least some output coverages after model run!").Be.GreaterThan(4); //we expect at least output coverages (the actual amount may change over time)

                //import model
                var projectImporter = new ProjectImporter
                                          {
                                              HybridProjectRepository = ((DeltaShellApplication) gui.Application).HybridProjectRepository,
                                              TargetDataDirectory = "."
                                          };

                var importedItems = projectImporter.ImportItem(path) as List<IProjectItem>;

                var importedModel = importedItems.First(item => item.Name.Contains("flow")) as WaterFlowModel1D;

                //check output is available
                importedModel.OutputFunctions.Count().Should("Expected output coverages after import").Be.EqualTo(countBeforeImport);
            }
        }

        [Test]
        public void ImportModelShouldNotTriggerClearingOfOutput()
        {
            var path = "ImportClearsOutput.dsproj";

            using (var gui = GetRunningGuiWithFlowPlugins())
            {
                //create model
                var model1 = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
                
                gui.Application.Project.RootFolder.Items.Add(model1);

                //run model
                RunModel(model1);

                //save model
                gui.Application.SaveProjectAs(path);
                
                int countBeforeImport = model1.OutputFunctions.Count();
                countBeforeImport.Should("Expected at least some output coverages after model run!").Be.GreaterThan(4); //we expect at least output coverages (the actual amount may change over time)

                Assert.Greater(model1.OutputFunctions.First().Components[0].Values.Count, 0);

                //import model
                var projectImporter = new ProjectImporter
                                          {
                                              HybridProjectRepository = ((DeltaShellApplication) gui.Application).HybridProjectRepository,
                                              TargetDataDirectory = "."
                                          };

                var importedItems = projectImporter.ImportItem(path) as List<IProjectItem>;

                var importedModel = importedItems.First(item => item.Name.Contains("flow")) as WaterFlowModel1D;

                Assert.Greater(importedModel.OutputFunctions.First().Components[0].Values.Count, 0);
            }
        }

        [Test]
        public void SavingAgainAfterSplittingABranchAfterSavingShouldWork()
        {
            var path = "zz.dsproj";
            
            projectRepository.Create(path);

            var project = new Project();
            var model = new WaterFlowModel1D();
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(1);
            model.Network = network;

            project.RootFolder.Items.Add(model);

            projectRepository.SaveOrUpdate(project);

            var branch1 = model.Network.Branches.First();

            NetworkHelper.SplitBranchAtNode(branch1, branch1.Length/2);

            projectRepository.SaveOrUpdate(project);

            projectRepository.Close();
            projectRepository.Dispose();

            projectRepository = factory.CreateNew();
            var retrievedProject = projectRepository.Open(path);
        }

        [Test]
        public void RetrievingComputationalGridAfterLoadDoesNotClearOutput()
        {
            //relates to issue 4966
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            RunModel(model);
            
            var retrievedModel = SaveAndRetrieveObject(model);
            //all is well
            Assert.IsNotNull(retrievedModel.OutputWaterLevel);

            //action! get the geometry of an input coverage like..comp grid..this will cause initialization of the coverarage and changes in the model!
            var geometry = retrievedModel.NetworkDiscretization.Geometry;
                
            //error?
            Assert.IsNotNull(retrievedModel.OutputWaterLevel);
        }

        [Test]
        public void LoadModelAfterRun()
        {
            using (var app = GetRunningAppWithFlowPlugins())
            {
                //save the project to a new dir
                const string tempDirName = @"tempdir";
                if (Directory.Exists(tempDirName))
                    Directory.Delete(tempDirName, true);
                Directory.CreateDirectory(tempDirName);

                const string newPath = tempDirName + @"\new.proj";
                
                app.SaveProjectAs(newPath); // to initialize file storing
                var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
                
                app.Project.RootFolder.Items.Add(model);
                app.SaveProject();

                //run the model
                LogHelper.ResetLogging();

                ActivityRunner.RunActivity(model);

                //keep expectations for future reference
                int expectedOutputDepthCount = model.OutputDepth.Components[0].Values.Count;
                object expected10ThValue = model.OutputDepth.Components[0].Values[10];
                
                app.SaveProjectAs(newPath);
                app.CloseProject();

                //reload the project
                app.OpenProject(newPath);
                var retrievedProject = app.Project;
                var retrievedModel = (WaterFlowModel1D)retrievedProject.RootFolder.Models.FirstOrDefault();

                //add all assert abouts thing that should not have changed.
                Assert.AreEqual(model.BoundaryConditions.Count(), retrievedModel.BoundaryConditions.Count());
                Assert.AreEqual(model.BoundaryConditions.First().Data.Arguments[0].Values[0], retrievedModel.BoundaryConditions.First().Data.Arguments[0].Values[0]);
                Assert.AreEqual(model.StopTime, retrievedModel.StopTime);
                Assert.AreEqual(model.StartTime, retrievedModel.StartTime);
                Assert.AreEqual(model.TimeStep, retrievedModel.TimeStep);
                Assert.AreEqual(model.OutputTimeStep, retrievedModel.OutputTimeStep);

                Assert.AreEqual(expectedOutputDepthCount, retrievedModel.OutputDepth.Components[0].Values.Count);

                Assert.AreEqual(expected10ThValue, retrievedModel.OutputDepth.Components[0].Values[10]);
                Assert.NotNull(retrievedModel.OutputDepth.Store);
            }
        }
        
        [Test]
        public void RunTwoModels()
        {
            using (var app = GetRunningAppWithFlowPlugins())
            {
                string projectPath = TestHelper.GetCurrentMethodName() + ".dsproj";
                app.SaveProjectAs(projectPath); // to initialize file storing                
                var model1 = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
                model1.Name = "model1";
                model1.OutputSettings.LocationWaterDepth = AggregationOptions.Current;

                var model2 = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
                model2.Name = "model2";
                model2.OutputSettings.LocationWaterDepth = AggregationOptions.Current;

                app.Project.RootFolder.Items.Add(model1);
                app.Project.RootFolder.Items.Add(model2);

                //save the project before the model is run
                app.SaveProjectAs(projectPath);

                //run the models
                LogHelper.ResetLogging();

                ActivityRunner.RunActivity(model1);

                LogHelper.ResetLogging();

                ActivityRunner.RunActivity(model2);

                //save expectations for later
                int expectedOutputDepthCount = model1.OutputDepth.Components[0].Values.Count;
                object expected10TheValue = model1.OutputDepth.Components[0].Values[10];

                //save and close the project.
                app.SaveProjectAs(projectPath);
                app.CloseProject();

                //reload the project
                app.OpenProject(projectPath);
                var retrievedProject = app.Project;
                
                var retrievedModel = (WaterFlowModel1D) retrievedProject.RootFolder.Models.FirstOrDefault();

                //add all assert abouts thing that should not have changed.
                Assert.AreEqual(model1.StopTime, retrievedModel.StopTime);
                Assert.AreEqual(model1.StartTime, retrievedModel.StartTime);
                Assert.AreEqual(model1.TimeStep, retrievedModel.TimeStep);
                Assert.AreEqual(model1.OutputTimeStep, retrievedModel.OutputTimeStep);
                Assert.AreEqual(expectedOutputDepthCount, retrievedModel.OutputDepth.Components[0].Values.Count);

                Assert.AreEqual(expected10TheValue, retrievedModel.OutputDepth.Components[0].Values[10]);
                Assert.NotNull(retrievedModel.OutputDepth.Store);
            }
        }

        [Test]
        public void RunModelAfterLoad()
        {
            using (var app = GetRunningAppWithFlowPlugins())
            {
                string projectPath = TestHelper.GetCurrentMethodName() + ".dsproj";
                app.SaveProjectAs(projectPath); // to initialize file storing                
            
                var model1 = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
                
                app.Project.RootFolder.Items.Add(model1);

                //save the project before the model is run
                app.SaveProjectAs(projectPath);
                app.CloseProject();

                //reload the project
                app.OpenProject(projectPath);
                var retrievedProject = app.Project;
                var retrievedModel = (WaterFlowModel1D) retrievedProject.RootFolder.Models.FirstOrDefault();
                Assert.IsNotNull(retrievedModel.NetworkDiscretization);

                LogHelper.ResetLogging();

                ActivityRunner.RunActivity(retrievedModel);
                //TODO: compare results to expected xml.
            }
        }

        [Test]
        public void SaveLoadFlowTimeSeriesBoundaryCondition()
        {
            //create a dummy model
            var model1 = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            string path = TestHelper.GetCurrentMethodName() + ".dsproj";

            //create a timeseries bc
            var firstBoundaryCondition = model1.BoundaryConditions.First();
            firstBoundaryCondition.DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries;
            firstBoundaryCondition.Data[new DateTime(2000, 1, 1)] = 4.0;
            
            //save 
            using (var projectRepository = factory.CreateNew())
            {
                projectRepository.Create(path);
                var project = new Project();
                project.RootFolder.Items.Add(model1);
                projectRepository.SaveOrUpdate(project);
                projectRepository.Close();
            }

            //reload
            using (var projectRepository = factory.CreateNew())
            {
                Project retrievedProject = projectRepository.Open(path);
                var retrievedModel = (WaterFlowModel1D)retrievedProject.RootFolder.Models.FirstOrDefault();
                var retrievedBoundaryCondition = retrievedModel.BoundaryConditions.First();

                Assert.AreEqual(firstBoundaryCondition.Name, retrievedBoundaryCondition.Name);
                Assert.AreEqual(firstBoundaryCondition.DataType, retrievedBoundaryCondition.DataType);
                Assert.AreEqual(4.0, retrievedBoundaryCondition.Data[new DateTime(2000,1,1)]);
            }
        }

        [Test]
        public void DeepCloneSavedFlowModel()
        {
            string path = TestHelper.GetCurrentMethodName() + ".dsproj";

            using (var app = GetRunningAppWithFlowPlugins())
            {
                app.SaveProjectAs(path); // to initialize file storing                
            
                var model = new WaterFlowModel1D();

                app.Project.RootFolder.Items.Add(model);
                app.SaveProject();

                app.SaveProjectAs(path);
                app.CloseProject();

                //reload the project
                app.OpenProject(path);
                var retrievedProject = app.Project;
                var retrievedModel = (WaterFlowModel1D)retrievedProject.RootFolder.Models.FirstOrDefault();

                Assert.NotNull(retrievedModel);

                var clonedModel = retrievedModel.DeepClone();
                Assert.NotNull(clonedModel);
            }
        }

        [Test]
        public void SaveLoadConstantBoundaryCondition()
        {
            //create a dummy model
            var model1 = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            string path = TestHelper.GetCurrentMethodName() + ".dsproj";

            //create a constant bc
            var firstBoundaryCondition = model1.BoundaryConditions.First();
            firstBoundaryCondition.DataType = WaterFlowModel1DBoundaryNodeDataType.FlowConstant;
            firstBoundaryCondition.Flow = 1.1;
            
            //save 
            using (var projectRepository = factory.CreateNew())
            {
                projectRepository.Create(path);
                var project = new Project();
                project.RootFolder.Items.Add(model1);
                projectRepository.SaveOrUpdate(project);
                projectRepository.Close();    
            }

            //reload
            using (var projectRepository = factory.CreateNew())
            {
                Project retrievedProject = projectRepository.Open(path);
                var retrievedModel = (WaterFlowModel1D) retrievedProject.RootFolder.Models.FirstOrDefault();
                var retrievedBoundaryCondition = retrievedModel.BoundaryConditions.First();
                
                Assert.AreEqual(firstBoundaryCondition.DataType, retrievedBoundaryCondition.DataType);
                Assert.AreEqual(firstBoundaryCondition.Flow, retrievedBoundaryCondition.Flow);
            }
        }

        [Test]
        public void SaveLoadSaltBoundaryCondition()
        {
            //create a dummy model
            var model1 = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            string path = TestHelper.GetCurrentMethodName() + ".dsproj";

            //create a salty bc
            var firstBoundaryCondition = model1.BoundaryConditions[0];
            firstBoundaryCondition.UseSalt = true;
            firstBoundaryCondition.SaltConditionType = SaltBoundaryConditionType.TimeDependent;
            firstBoundaryCondition.SaltConcentrationTimeSeries[new DateTime(2000, 1, 1)] = 4.0;
            
            
            //save 
            using (var projectRepository = factory.CreateNew())
            {
                projectRepository.Create(path);
                var project = new Project();
                project.RootFolder.Items.Add(model1);
                projectRepository.SaveOrUpdate(project);
                projectRepository.Close();
            }

            //reload
            using (var projectRepository = factory.CreateNew())
            {
                Project retrievedProject = projectRepository.Open(path);
                var retrievedModel = (WaterFlowModel1D)retrievedProject.RootFolder.Models.FirstOrDefault();
                var retrievedBoundaryCondition = retrievedModel.BoundaryConditions.First();

                Assert.IsTrue(retrievedBoundaryCondition.UseSalt);
                Assert.AreEqual(SaltBoundaryConditionType.TimeDependent, retrievedBoundaryCondition.SaltConditionType);
                Assert.AreEqual(4.0, retrievedBoundaryCondition.SaltConcentrationTimeSeries[new DateTime(2000, 1, 1)]);
            }
        }

        [Test]
        public void SaveLoadSaltBoundaryConditionConstant()
        {
            var conc = 18.3333;
            var thatcher = 3600;


            //create a dummy model
            var model1 = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            string path = TestHelper.GetCurrentMethodName() + ".dsproj";

            //create a salty bc
            var firstBoundaryCondition = model1.BoundaryConditions[0];
            firstBoundaryCondition.UseSalt = true;
            firstBoundaryCondition.SaltConditionType = SaltBoundaryConditionType.Constant;
            firstBoundaryCondition.SaltConcentrationConstant = conc;
            firstBoundaryCondition.ThatcherHarlemannCoefficient = thatcher;


            //save 
            using (var projectRepository = factory.CreateNew())
            {
                projectRepository.Create(path);
                var project = new Project();
                project.RootFolder.Items.Add(model1);
                projectRepository.SaveOrUpdate(project);
                projectRepository.Close();
            }

            //reload
            using (var projectRepository = factory.CreateNew())
            {
                Project retrievedProject = projectRepository.Open(path);
                var retrievedModel = (WaterFlowModel1D)retrievedProject.RootFolder.Models.FirstOrDefault();
                var retrievedBoundaryCondition = retrievedModel.BoundaryConditions.First();

                Assert.IsTrue(retrievedBoundaryCondition.UseSalt);
                Assert.AreEqual(SaltBoundaryConditionType.Constant, retrievedBoundaryCondition.SaltConditionType);
                Assert.AreEqual(conc, retrievedBoundaryCondition.SaltConcentrationConstant);
                Assert.AreEqual(thatcher, retrievedBoundaryCondition.ThatcherHarlemannCoefficient);
            }
        }
        
        [Test]
        public void SaveLoadLateralSourceData()
        {
            //create a dummy model
            var model1 = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            string path = TestHelper.GetCurrentMethodName() + ".dsproj";

            model1.UseSalt = true;

            var waterFlowModel1DLateralSourceData = new WaterFlowModel1DLateralSourceData();

            waterFlowModel1DLateralSourceData.DataType = WaterFlowModel1DLateralDataType.FlowConstant;
            waterFlowModel1DLateralSourceData.Flow = 1.5;
            waterFlowModel1DLateralSourceData.UseSalt = true;
            waterFlowModel1DLateralSourceData.SaltLateralDischargeType = SaltLateralDischargeType.MassConstant;
            waterFlowModel1DLateralSourceData.SaltMassTimeSeries[new DateTime(2000, 1, 1)] = 5.0;
            waterFlowModel1DLateralSourceData.SaltMassDischargeConstant = 2;
            waterFlowModel1DLateralSourceData.SaltConcentrationTimeSeries[new DateTime(2000, 1, 1)] = 3.0;
            waterFlowModel1DLateralSourceData.SaltConcentrationDischargeConstant = 4;

            model1.LateralSourceData.Add(waterFlowModel1DLateralSourceData);
            
            //save 
            SaveModelToProject(model1, path);

            //reload
            using (var projectRepository = factory.CreateNew())
            {
                Project retrievedProject = projectRepository.Open(path);
                var retrievedModel = (WaterFlowModel1D)retrievedProject.RootFolder.Models.FirstOrDefault();
                var retrievedLateralSourceData= retrievedModel.LateralSourceData.First();

                Assert.AreEqual(WaterFlowModel1DLateralDataType.FlowConstant,retrievedLateralSourceData.DataType);
                Assert.AreEqual(1.5,retrievedLateralSourceData.Flow);
                Assert.IsTrue(retrievedLateralSourceData.UseSalt);
                Assert.AreEqual(SaltLateralDischargeType.MassConstant,retrievedLateralSourceData.SaltLateralDischargeType);
                Assert.AreEqual(5,retrievedLateralSourceData.SaltMassTimeSeries[new DateTime(2000,1,1)]);
                Assert.AreEqual(2,retrievedLateralSourceData.SaltMassDischargeConstant);
                Assert.AreEqual(3, retrievedLateralSourceData.SaltConcentrationTimeSeries[new DateTime(2000, 1, 1)]);
                Assert.AreEqual(4, retrievedLateralSourceData.SaltConcentrationDischargeConstant);
                
            }
        }

        [Test]
        public void SaveLinkedBoundaryCondition()
        {
            //create a dummy model
            var model1 = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            string path = TestHelper.GetCurrentMethodName() + ".dsproj";

            //create a constant bc
            var firstBoundaryCondition = model1.BoundaryConditions.First();
            firstBoundaryCondition.DataType = WaterFlowModel1DBoundaryNodeDataType.FlowConstant;
            

            //save 
            using (var projectRepository = factory.CreateNew())
            {
                projectRepository.Create(path);
                var project = new Project();
                project.RootFolder.Items.Add(model1);

                //link the first boundarycondition to a dataitem
                var constantDataItem = new DataItem(
                    new FlowParameter() {Value = 5.0});
                project.RootFolder.Items.Add(constantDataItem);
                firstBoundaryCondition.FlowConstantDataItem.LinkTo(constantDataItem);

                projectRepository.SaveOrUpdate(project);
                projectRepository.Close();
            }

            //reload
            using (var projectRepository = factory.CreateNew())
            {
                Project retrievedProject = projectRepository.Open(path);
                var retrievedModel = (WaterFlowModel1D)retrievedProject.RootFolder.Models.FirstOrDefault();
                var retrievedBoundaryCondition = retrievedModel.BoundaryConditions.First();

                Assert.AreEqual(firstBoundaryCondition.DataType, retrievedBoundaryCondition.DataType);
                Assert.AreEqual(firstBoundaryCondition.Flow, retrievedBoundaryCondition.Flow);
            }
        }

        [Test]
        public void TestFlowBoundaryConditionsCreationAfterLoad()
        {
            //setup repo.
            string path = TestHelper.GetCurrentMethodName() + ".dsproj";
            projectRepository.Create(path);

            var network = new HydroNetwork();
            var delftFlowModel1D = new WaterFlowModel1D { Network = network };

            var project = new Project();
            project.RootFolder.Add(delftFlowModel1D);
            project.RootFolder.Add(network);
            projectRepository.SaveOrUpdate(project);
            projectRepository.Close();

            var retievedProject = projectRepository.Open(path);

            var retrievedNetwork = (HydroNetwork)retievedProject.RootFolder.DataItems.FirstOrDefault().Value;
            var retrievedModel = (WaterFlowModel1D)retievedProject.RootFolder.Models.FirstOrDefault();

            //add some stuff to the persisted network
            var branch = new Channel();
            var node1 = new HydroNode { Network = network };
            var node2 = new HydroNode { Network = network };

            branch.Source = node1;
            branch.Target = node2;

            retrievedNetwork.Branches.Add(branch);
            retrievedNetwork.Nodes.Add(node1);
            retrievedNetwork.Nodes.Add(node2);

            Assert.AreEqual(2, retrievedModel.BoundaryConditions.Count());
        }

        [Test]
        public void TestRoughnessDataAfterLoad()
        {
            //setup repo.
            string path = TestHelper.GetCurrentMethodName() + ".dsproj";
            projectRepository.Create(path);

            var network = CreateSimplerNetwork();

            var crossSectionType = new CrossSectionSectionType {Name = "test"};
            network.CrossSectionSectionTypes.Add(crossSectionType);
            var delftFlowModel1D = new WaterFlowModel1D { Network = network };
            var crossSection = network.CrossSections.FirstOrDefault();
            crossSection.Definition.Sections.Add(new CrossSectionSection
                                          {
                                              MinY = 0.0, MaxY = crossSection.Definition.Width, SectionType = crossSectionType
                                          });

            var roughnessSection = delftFlowModel1D.RoughnessSections.Where(rs => rs.Name == crossSectionType.Name).FirstOrDefault();
            roughnessSection.RoughnessNetworkCoverage[new NetworkLocation(crossSection.Branch, crossSection.Chainage)] =
                new object[] {23.0, RoughnessType.Manning};

            var project = new Project();
            project.RootFolder.Add(delftFlowModel1D);
            project.RootFolder.Add(network);

            projectRepository.SaveOrUpdate(project);
            projectRepository.Close();

            var retrievedProject = projectRepository.Open(path);

            var retrievedNetwork = (HydroNetwork)retrievedProject.RootFolder.DataItems.FirstOrDefault().Value;

            Assert.AreEqual(2, retrievedNetwork.CrossSectionSectionTypes.Count); // default + test
            Assert.AreEqual("test", retrievedNetwork.CrossSectionSectionTypes.Last().Name);

            var retrievedCrossSection = retrievedNetwork.CrossSections.FirstOrDefault();
            Assert.AreEqual(1, retrievedCrossSection.Definition.Sections.Count);

            var retrievedModel = (WaterFlowModel1D)retrievedProject.RootFolder.Models.FirstOrDefault();
            Assert.AreEqual(2, retrievedModel.RoughnessSections.Count);
            var retrievedRoughnessSection = retrievedModel.RoughnessSections.Where(rs => rs.Name == crossSectionType.Name).FirstOrDefault();
            Assert.AreEqual("test", retrievedRoughnessSection.Name);
            var retrievedRoughnessValue = retrievedRoughnessSection.RoughnessNetworkCoverage.EvaluateRoughnessValue(
                new NetworkLocation(retrievedCrossSection.Branch, retrievedCrossSection.Chainage));
            // ==> retrievedRoughnessSection.RoughnessNetworkCoverage.Locations.Values[0].Branch == crossSection.Branch

            Assert.AreEqual(23.0, retrievedRoughnessValue, 1.0e-6);
            var retrievedRoughnessType = retrievedRoughnessSection.RoughnessNetworkCoverage.EvaluateRoughnessType(
                new NetworkLocation(retrievedCrossSection.Branch, retrievedCrossSection.Chainage));
            Assert.AreEqual(RoughnessType.Manning, retrievedRoughnessType);
        }

        [Test]
        public void SaveAndRoughnessCoverageWithQAndH()
        {
            var network = CreateSimplerNetwork();
            network.Branches.Add(new Channel("Piet", null, null,0.0));
            var defaultType = network.CrossSectionSectionTypes.First();
            var crossSectionSection = new RoughnessSection(defaultType,network);
            crossSectionSection.AddHRoughnessFunctionToBranch(network.Branches.First(),new Function{Name = "Kees"});
            crossSectionSection.AddQRoughnessFunctionToBranch(network.Branches.Last(), new Function { Name = "PietR" });

            var project = new Project();    
            project.RootFolder.Add(crossSectionSection);

            //setup repo.
            string path = TestHelper.GetCurrentMethodName() + ".dsproj";
            projectRepository.Create(path);
            projectRepository.SaveOrUpdate(project);

            var retrievedProject = projectRepository.Open(path);
            var retrievedCrossSectionSection = (RoughnessSection)retrievedProject.RootFolder.DataItems.First().Value;
            var retrievedBranch1 = retrievedCrossSectionSection.Network.Branches.First();
            var retrievedBranch2 = retrievedCrossSectionSection.Network.Branches.Last();

            Assert.AreEqual(RoughnessFunction.FunctionOfH, retrievedCrossSectionSection.GetRoughnessFunctionType(retrievedBranch1));
            Assert.AreEqual(RoughnessFunction.FunctionOfQ, retrievedCrossSectionSection.GetRoughnessFunctionType(retrievedBranch2));
        }

        [Test]
        public void SaveAndRoughnessCoverageWithQAndConstant()
        {
            var path = TestHelper.GetCurrentMethodName() + ".dsproj";
            projectRepository.Create(path);

            var network = CreateSimplerNetwork();

            // add extra branch to test storage for multiple branches in network
            var node3 = new HydroNode { Name = "node3", Network = network };

            network.Nodes.Add(node3);

            var branch2 = new Channel("branch2", network.Nodes[1], node3, 100.0);
            var vertices = new List<Coordinate>
                               {
                                   new Coordinate(100, 0),
                                   new Coordinate(200, 0)
                               };
            branch2.Geometry = GeometryFactory.CreateLineString(vertices.ToArray());
            network.Branches.Add(branch2);
            AddDefaultCrossSection(branch2, "crs2", 40);

            //
            var delftFlowModel1D = new WaterFlowModel1D { Network = network };
            var roughnessSection = delftFlowModel1D.RoughnessSections[0];
            roughnessSection.AddQRoughnessFunctionToBranch(network.Branches[1]);
            Assert.AreEqual(RoughnessFunction.Constant, roughnessSection.GetRoughnessFunctionType(network.Branches[0]));
            Assert.AreEqual(RoughnessFunction.FunctionOfQ, roughnessSection.GetRoughnessFunctionType(network.Branches[1]));

            var project = new Project();
            project.RootFolder.Add(delftFlowModel1D);
            //project.RootFolder.Add(network);

            projectRepository.SaveOrUpdate(project);
            projectRepository.Close();

            var retrievedProject = projectRepository.Open(path);

            var retrievedModel = (WaterFlowModel1D)retrievedProject.RootFolder.Models.FirstOrDefault();
            var retrievedNetwork = retrievedModel.Network;
            var retrievedRoughnessSection = retrievedModel.RoughnessSections[0];
            Assert.AreEqual(retrievedNetwork, retrievedRoughnessSection.Network);
            Assert.AreEqual(RoughnessFunction.Constant, retrievedRoughnessSection.GetRoughnessFunctionType(retrievedNetwork.Branches[0]));
            Assert.AreEqual(RoughnessFunction.FunctionOfQ, retrievedRoughnessSection.GetRoughnessFunctionType(retrievedNetwork.Branches[1]));
        }
        
        [Test]
        public void TOOLS4766ASimpleReModelRunsAfterImportButNotAfterSavingAndLoading()
        {
            // problem caused by invalid save load of roughness
            var path = TestHelper.GetCurrentMethodName() + ".dsproj";
            projectRepository.Create(path);
            var project = projectRepository.GetProject();

            // construct a simple network
            var network = CreateSimplerNetwork();
            var branch = network.Branches[0];
            var delftFlowModel1D = new WaterFlowModel1D { Network = network };
            project.RootFolder.Add(delftFlowModel1D);

            Assert.AreEqual("Main", delftFlowModel1D.RoughnessSections[0].Name);
            //delftFlowModel1D.RoughnessSections[0].RoughnessNetworkCoverage[new NetworkLocation(branch, 0.0)] =
            //    new object[] {23.0, RoughnessType.Manning};
            var function = delftFlowModel1D.RoughnessSections[0].AddQRoughnessFunctionToBranch(branch);
            Assert.AreEqual(RoughnessFunction.FunctionOfQ, delftFlowModel1D.RoughnessSections[0].GetRoughnessFunctionType(branch));
            //Assert.AreEqual(1, delftFlowModel1D.RoughnessSections[0].RoughnessFunctionOfH.Count);
            //Assert.AreEqual(1, delftFlowModel1D.RoughnessSections[0].RoughnessFunctionOfQ.Count);
            //Assert.AreEqual("defaultH", delftFlowModel1D.RoughnessSections[0].RoughnessFunctionOfH[0].Name);
            //Assert.AreEqual(function, delftFlowModel1D.RoughnessSections[0].RoughnessFunctionOfQ[0]);
            Assert.AreEqual(function, delftFlowModel1D.RoughnessSections[0].FunctionOfQ(branch));

            projectRepository.SaveOrUpdate(project);
            projectRepository.Close();

            var retrievedProject = projectRepository.Open(path);
            var retrievedModel = (WaterFlowModel1D)retrievedProject.RootFolder.Models.FirstOrDefault();
            var retrievedNetwork = retrievedModel.Network;
            var retrievedBranch = retrievedNetwork.Branches[0];
            Assert.AreEqual("Main", retrievedModel.RoughnessSections[0].Name);
            //retrievedModel.RoughnessSections[0].RoughnessNetworkCoverage[new NetworkLocation(retrievedBranch, 0.0)] =
            //    new object[] { 23.0, RoughnessType.Manning };
            Assert.AreEqual(RoughnessFunction.FunctionOfQ, retrievedModel.RoughnessSections[0].GetRoughnessFunctionType(retrievedBranch));
            //Assert.AreEqual(1, retrievedModel.RoughnessSections[0].RoughnessFunctionOfH.Count);
            //Assert.AreEqual(1, retrievedModel.RoughnessSections[0].RoughnessFunctionOfQ.Count);
            //Assert.IsNotNull(retrievedModel.RoughnessSections[0].RoughnessFunctionOfH[0]);
            //Assert.IsNotNull(retrievedModel.RoughnessSections[0].RoughnessFunctionOfQ[0]);
        }

        [Test]
        public void Issue1275SaveModelAddBranchAndCountBoundaryConditions()
        {
            var network = new HydroNetwork();
            var delftFlowModel1D = new WaterFlowModel1D { Network = network };
            var node1 = new HydroNode { Network = network };
            var node2 = new HydroNode { Network = network };
            var branch = new Channel { Source = node1, Target = node2 };

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            network.Branches.Add(branch);

            var project = new Project();
            project.RootFolder.Add(delftFlowModel1D);
            
            Assert.AreEqual(2, delftFlowModel1D.BoundaryConditions.Count);

            // Setup repository
            var path = TestHelper.GetCurrentMethodName() + ".dsproj";
            projectRepository.Create(path);

            // Save
            projectRepository.SaveOrUpdate(project);

            // Extend branch
            var node3 = new HydroNode { Network = network };
            var branch2 = new Channel {Source = node2, Target = node3};

            network.Branches.Add(branch2);
            network.Nodes.Add(node3);

            Assert.IsFalse(node1.IsConnectedToMultipleBranches);
            Assert.IsTrue(node2.IsConnectedToMultipleBranches);
            Assert.IsFalse(node3.IsConnectedToMultipleBranches);
            Assert.AreEqual(3, delftFlowModel1D.BoundaryConditions.Count);
        }
    
        [Test]
        public void SaveLoadReMaasModel()
        {
            //relates to issue 4693 but stays as a good reference test

            var modelPath = TestHelper.GetTestDataPath(typeof(SobekWaterFlowModel1DImporterTest).Assembly, @"ReModels\JAMM2010.sbk\40\DEFTOP.1");
            var modelImporter = new SobekWaterFlowModel1DImporter();
            modelImporter.TargetItem = new WaterFlowModel1D(); //makes sure we don't get some composite model (with rtc) back
            var model = (IModel)modelImporter.ImportItem(modelPath);
            string path = TestHelper.GetCurrentMethodName() + ".dsproj";
            
            using (var repository = factory.CreateNew())
            {
                repository.Create(path);
                var project = repository.GetProject();
                project.RootFolder.Add(model);
                repository.SaveOrUpdate(project);    
            }

            //reload
            using (var repository = factory.CreateNew())
            {
                repository.Open(path);
            }
        }

        [Test]
        public void ImportTwenteModelSaveDeleteBranchAndSaveShouldNotTriggerReSave()
        {
            var modelPath = 
                TestHelper.GetTestDataPath(typeof(SobekWaterFlowModel1DImporterTest).Assembly, @"TwenteKanaal.lit\3\network.tp");
            var importer = new SobekHydroModelImporter(false);
            var importedModel = (HydroModel)importer.ImportItem(modelPath);

            var flowModel = importedModel.Activities.OfType<WaterFlowModel1D>().First();

            string path = TestHelper.GetCurrentMethodName() + ".dsproj";
            using (var repository = factory.CreateNew())
            {
                repository.Create(path);
                var project = repository.GetProject();
                project.RootFolder.Add(importedModel);
                repository.SaveOrUpdate(project);
                
                var network = flowModel.Network;
                var northernChannel = network.Branches.OfType<IChannel>().First(ch => ch.LongName.Contains("Aadorp"));
                
                flowModel.Network.Branches.Remove(northernChannel);
                
                repository.SaveOrUpdate(project);
            }
        }

        [Test]
        public void SaveByPassModelTwice()
        {
            var path = TestHelper.GetCurrentMethodName() + ".dsproj";
            //LogHelper.ConfigureLogging();
            //test reproduces issue 3507

            var modelPath =
                TestHelper.GetTestDataPath(
                    typeof(SobekWaterFlowModel1DImporterTest).Assembly,
                    @"BYPASS.Lit\3\Network.TP");
            //what a cumbersome syntax here..
            var importedModel = (HydroModel)new SobekHydroModelImporter().ImportItem(modelPath);

            using (var rep = factory.CreateNew())
            {
                rep.Create(path);
            
                var project = rep.GetProject();
                var model = (HydroModel)importedModel.DeepClone();

                project.RootFolder.Add(model);
                rep.SaveOrUpdate(project);

            }

            using (var rep = factory.CreateNew())
            {
                rep.Create(path);
            
                var project = rep.GetProject();
                var model = (HydroModel)importedModel.DeepClone();

                project.RootFolder.Add(model);
                               
                rep.SaveOrUpdate(project);
            }
        }

        [Test]
        public void SaveBypassModelWithDiscretization()
        {
            var path = TestHelper.GetCurrentMethodName() + ".dsproj";
            SaveBypassModel(path, false);
        }

        [Test]
        public void SaveLoadInitialConditionsType()
        {
            //test 'local' parameters

            var path = TestHelper.GetCurrentMethodName() + ".dsproj";
            var waterFlowModel1D = new WaterFlowModel1D();
            //change it!
            const double defaultInitialDepth = 23.0;
            const double defaultInitialWaterLevel = 11.5;
            waterFlowModel1D.DefaultInitialDepth = defaultInitialDepth;
            
            waterFlowModel1D.DefaultInitialWaterLevel = defaultInitialWaterLevel;
            waterFlowModel1D.InitialConditionsType = InitialConditionsType.WaterLevel;

            SaveModelToProject(waterFlowModel1D,path);
            
            using (var projectRepository = factory.CreateNew())
            {
                var retrievedProject = projectRepository.Open(path);
                var retrievedModel = retrievedProject.GetAllItemsRecursive().OfType<WaterFlowModel1D>().FirstOrDefault(); 
                Assert.AreEqual(InitialConditionsType.WaterLevel, retrievedModel.InitialConditionsType);
                Assert.AreEqual(defaultInitialDepth,retrievedModel.DefaultInitialDepth);
                Assert.AreEqual(defaultInitialWaterLevel, retrievedModel.DefaultInitialWaterLevel);
            }
        }

        [Test]
        public void LoadZwolleModelWithRoughnessCoverages()
        {
            var modelPath = TestHelper.GetTestDataPath(typeof(SobekWaterFlowModel1DImporterTest).Assembly, @"SW_max_1.lit\3\Network.TP");
            var importer = new SobekWaterFlowModel1DImporter();
            var model = (WaterFlowModel1D) importer.ImportItem(modelPath);

            var sectionMain = model.RoughnessSections.First(r => r.RoughnessNetworkCoverage.Name == "Main");
            Assert.AreEqual(368, sectionMain.RoughnessNetworkCoverage.Locations.AllValues.Count);
        }

        [Test]
        public void SerializeRoughnessNetworkCoverageAndNetwork()
        {
            string path = TestHelper.GetCurrentMethodName() + ".dsproj";
            var network = CreateSimplerNetwork();
            var branch = network.Branches[0];
            network.CrossSectionSectionTypes.Add(new CrossSectionSectionType { Name = "main" });

            var roughnessCoverage = new RoughnessNetworkCoverage("main", false, null) { Network = network, DefaultRoughnessType = RoughnessType.Chezy };
            var networkLocation1 = new NetworkLocation(branch, 0);
            roughnessCoverage[networkLocation1] = new object[] { 100.0, RoughnessType.Chezy };
            roughnessCoverage[new NetworkLocation(branch, 1.0)] = new object[] { 200.0, RoughnessType.Chezy };

            using (var projectRepository = factory.CreateNew())
            {
                projectRepository.Create(path);

                var project = new Project();
                project.RootFolder.Add(new DataItem(network));
                project.RootFolder.Add(new DataItem(roughnessCoverage));

                projectRepository.SaveOrUpdate(project);

                //reload
                var retrieverProject = projectRepository.Open(path);
                IDataItem[] retrievedDataItems = retrieverProject.RootFolder.DataItems.ToArray();
                var retrievedNetwork = (INetwork)retrievedDataItems[0].Value;
                var retrievedRoughnessCoverage = (RoughnessNetworkCoverage)retrievedDataItems[1].Value;
                
                //compare
                Assert.AreEqual(new NetworkLocation(retrievedNetwork.Branches[0], 0.0), retrievedRoughnessCoverage.Arguments[0].Values[0]);
                Assert.AreEqual(roughnessCoverage.Components[0].Values.Count, retrievedRoughnessCoverage.Components[0].Values.Count);
                Assert.AreEqual(RoughnessType.Chezy, retrievedRoughnessCoverage.EvaluateRoughnessType(networkLocation1));
                Assert.AreEqual(network.Branches.Count, retrievedNetwork.Branches.Count);
                Assert.AreEqual(retrievedRoughnessCoverage.Network, retrievedNetwork);

                Assert.AreEqual(roughnessCoverage.Name, retrievedRoughnessCoverage.Name);
                Assert.AreEqual(roughnessCoverage.Network.Name, retrievedRoughnessCoverage.Network.Name);
                Assert.AreEqual(roughnessCoverage.Network.Nodes.FirstOrDefault().Name, retrievedRoughnessCoverage.Network.Nodes.FirstOrDefault().Name);
            }
        }

        [Test]
        public void SaveAndRetrieveRoughnessCoverage()
        {
            var network = new HydroNetwork();
            network.CrossSectionSectionTypes.Add(new CrossSectionSectionType { Name = "main" });
            var roughnessCoverage = new RoughnessNetworkCoverage("main", false,null) { Network = network , DefaultRoughnessType = RoughnessType.Manning };
            var retrievedEntity = SaveAndRetrieveObject(roughnessCoverage);
            Assert.IsNotNull(retrievedEntity);
            Assert.AreEqual(RoughnessType.Manning,retrievedEntity.DefaultRoughnessType);
        }

        [Test]
        public void SaveAndRetrieveRoughnessSection()
        {
            var network = new HydroNetwork();
            network.CrossSectionSectionTypes.Add(new CrossSectionSectionType { Name = "Main" });
            var model = new WaterFlowModel1D {Network = network};
            model.UseReverseRoughness = true;
            
            var main = model.RoughnessSections.MainChannel();
            var reverseMain = (ReverseRoughnessSection)model.RoughnessSections.GetApplicableReverseRoughnessSection(main);
            reverseMain.UseNormalRoughness = false;
            
            var retrievedEntity = SaveAndRetrieveObject(reverseMain);
            Assert.IsNotNull(retrievedEntity);
            Assert.AreEqual(reverseMain.UseNormalRoughness, retrievedEntity.UseNormalRoughness);
            Assert.AreEqual(main.Name, retrievedEntity.NormalSection.Name);
        }

        [Test]
        public void SaveLoadWFM1DWithParameterSettings()
        {
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            model.UseSaveStateTimeRange = true;
            model.Initialize();
            //Assert.IsNotNull(model.ModelEngine);
            var origParamSettings = model.ParameterSettings;
            var path = TestHelper.GetCurrentMethodName() + ".dsproj";
            SaveModelToProject(model, path);

            using (var projectRepository = factory.CreateNew())
            {
                var retrievedProject = projectRepository.Open(path);
                var retrievedModel = (WaterFlowModel1D)retrievedProject.RootFolder.Models.FirstOrDefault();
                IList<ModelApiParameter> retrievedParameterSettings = retrievedModel.ParameterSettings;
                for (int i = 0; i < retrievedParameterSettings.Count; i++)
                {
                    Assert.AreEqual(origParamSettings[i].Name,retrievedParameterSettings[i].Name);
                    Assert.AreEqual(origParamSettings[i].Value, retrievedParameterSettings[i].Value);
                }

                Assert.AreEqual(model.UseSaveStateTimeRange, retrievedModel.UseSaveStateTimeRange);
                Assert.AreEqual(model.SaveStateStartTime, retrievedModel.SaveStateStartTime);
                Assert.AreEqual(model.SaveStateStopTime, retrievedModel.SaveStateStopTime);
                Assert.AreEqual(model.SaveStateTimeStep, retrievedModel.SaveStateTimeStep);
            }
        }

        [Test]
        public void SaveLoadWaterFlowModel1D()
        {
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            var path = TestHelper.GetCurrentMethodName() + ".dsproj";
            
            SaveModelToProject(model, path);

            using (var projectRepository = factory.CreateNew())
            {
                var retrievedProject = projectRepository.Open(path);
                var retrievedModel = (WaterFlowModel1D)retrievedProject.RootFolder.Models.FirstOrDefault();

                //Assert.IsTrue(retrievedModel.RunInSeparateProcess, "RunInSeparateProcess flag should be saved");
            }
        }

        [Test]
        public void SaveQRoughnessFunctionAndRemoveBranch()
        {
            //issue 5185
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(new Point(0, 0), new Point(100, 0));
            
            var defaultType = network.CrossSectionSectionTypes.First();
            var roughnessSection = new RoughnessSection(defaultType, network);
            IBranch branch = network.Branches[0];
            roughnessSection.AddQRoughnessFunctionToBranch(branch, new Function { Name = "PietR" });

            var project = new Project();
            project.RootFolder.Add(roughnessSection);

            //setup repo.
            string path = TestHelper.GetCurrentMethodName() + ".dsproj";
            projectRepository.Create(path);
            projectRepository.SaveOrUpdate(project);

            //remove the branch
            network.BeginEdit("Removing branch");
            network.Branches.RemoveAt(0);
            network.EndEdit();

            //save
            projectRepository.SaveOrUpdate(project);
            
            //check the function cleaned up
            try
            {
                roughnessSection.FunctionOfQ(branch);
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is KeyNotFoundException);
            }

            
        }

        [Test]
        public void SaveLoadFiniteVolumeGridType()
        {
            const AggregationOptions finiteVolumeDiscretizationType = (AggregationOptions) (int) FiniteVolumeDiscretizationType.OnGridPoints;

            // Create a dummy model
            var waterFlowModel1D = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            var path = TestHelper.GetCurrentMethodName() + ".dsproj";
            var gridTypeParameter = waterFlowModel1D.OutputSettings.EngineParameters.First(p => p.Name == WaterFlowModelParameterNames.FiniteVolumeGridType);
            gridTypeParameter.AggregationOptions = finiteVolumeDiscretizationType;

            // Save the model 
            SaveModelToProject(waterFlowModel1D, path);

            // Reload the model
            using (var projectRepository = factory.CreateNew())
            {
                var retrievedProject = projectRepository.Open(path);
                var retrievedModel = (WaterFlowModel1D) retrievedProject.RootFolder.Models.FirstOrDefault();
                gridTypeParameter = retrievedModel.OutputSettings.EngineParameters.First(p => p.Name == WaterFlowModelParameterNames.FiniteVolumeGridType);
                
                Assert.AreEqual(finiteVolumeDiscretizationType, gridTypeParameter.AggregationOptions);
            }
        }

        [Test]
        public void SaveLoadTemperatureBoundaryConditionData()
        {
            //create a dummy model
            var model1 = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            string path = TestHelper.GetCurrentMethodName() + ".dsproj";

            //create temperature data
            var firstBoundaryCondition = model1.BoundaryConditions[0];
            firstBoundaryCondition.UseTemperature = true;
            firstBoundaryCondition.TemperatureConditionType = TemperatureBoundaryConditionType.TimeDependent;
            firstBoundaryCondition.TemperatureTimeSeries[new DateTime(2000, 1, 1)] = 4.0;

            //save 
            using (var projectRepository = factory.CreateNew())
            {
                projectRepository.Create(path);
                var project = new Project();
                project.RootFolder.Items.Add(model1);
                projectRepository.SaveOrUpdate(project);
                projectRepository.Close();
            }

            //reload
            using (var projectRepository = factory.CreateNew())
            {
                Project retrievedProject = projectRepository.Open(path);
                var retrievedModel = (WaterFlowModel1D)retrievedProject.RootFolder.Models.FirstOrDefault();
                var retrievedBoundaryCondition = retrievedModel.BoundaryConditions.First();

                Assert.IsTrue(retrievedBoundaryCondition.UseTemperature);
                Assert.AreEqual(TemperatureBoundaryConditionType.TimeDependent, retrievedBoundaryCondition.TemperatureConditionType);
                Assert.AreEqual(4.0, retrievedBoundaryCondition.TemperatureTimeSeries[new DateTime(2000, 1, 1)]);
            }
        }

        [Test]
        public void SaveLoadTemperatureLateralSourceData()
        {
            //create a dummy model
            var model1 = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            string path = TestHelper.GetCurrentMethodName() + ".dsproj";

            model1.UseTemperature = true;

            var waterFlowModel1DLateralSourceData = new WaterFlowModel1DLateralSourceData();

            waterFlowModel1DLateralSourceData.DataType = WaterFlowModel1DLateralDataType.FlowConstant;
            waterFlowModel1DLateralSourceData.Flow = 1.5;
            waterFlowModel1DLateralSourceData.UseTemperature = true;
            waterFlowModel1DLateralSourceData.TemperatureLateralDischargeType = TemperatureLateralDischargeType.TimeDependent;
            waterFlowModel1DLateralSourceData.TemperatureTimeSeries[new DateTime(2000, 1, 1)] = 5.0;
            waterFlowModel1DLateralSourceData.TemperatureConstant = 2;

            model1.LateralSourceData.Add(waterFlowModel1DLateralSourceData);

            //save 
            SaveModelToProject(model1, path);

            //reload
            using (var projectRepository = factory.CreateNew())
            {
                Project retrievedProject = projectRepository.Open(path);
                var retrievedModel = (WaterFlowModel1D)retrievedProject.RootFolder.Models.FirstOrDefault();
                var retrievedLateralSourceData = retrievedModel.LateralSourceData.First();

                Assert.AreEqual(WaterFlowModel1DLateralDataType.FlowConstant, retrievedLateralSourceData.DataType);
                Assert.AreEqual(1.5, retrievedLateralSourceData.Flow);
                Assert.IsTrue(retrievedLateralSourceData.UseTemperature);
                Assert.AreEqual(TemperatureLateralDischargeType.TimeDependent, retrievedLateralSourceData.TemperatureLateralDischargeType);
                Assert.AreEqual(5, retrievedLateralSourceData.TemperatureTimeSeries[new DateTime(2000, 1, 1)]);
                Assert.AreEqual(2, retrievedLateralSourceData.TemperatureConstant);
            }
        }

        [Test]
        public void SaveLoadTemperatureModelData()
        {
            //create a dummy model
            var model1 = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            string path = TestHelper.GetCurrentMethodName() + ".dsproj";

            model1.UseTemperature = true;

            model1.BackgroundTemperature = 0.123;
            model1.SurfaceArea = 10000;
            model1.AtmosphericPressure = 1000;
            model1.StantonNumber = 0.345;
            model1.DaltonNumber = 0.567;
            model1.HeatCapacityWater = 3930;
            model1.Latitude = 20000;
            model1.Longitude = -20000;
            //save 
            SaveModelToProject(model1, path);

            //reload
            using (var projectRepository = factory.CreateNew())
            {
                Project retrievedProject = projectRepository.Open(path);
                var retrievedModel = (WaterFlowModel1D)retrievedProject.RootFolder.Models.FirstOrDefault();

                Assert.NotNull(retrievedModel);
                Assert.AreEqual(0.123, retrievedModel.BackgroundTemperature, 0.001);
                Assert.AreEqual(10000, retrievedModel.SurfaceArea, 0.001);
                Assert.AreEqual(1000, retrievedModel.AtmosphericPressure, 0.001);
                Assert.AreEqual(0.345, retrievedModel.StantonNumber, 0.001);
                Assert.AreEqual(0.567, retrievedModel.DaltonNumber, 0.001);
                Assert.AreEqual(3930, retrievedModel.HeatCapacityWater, 0.001);
                Assert.AreEqual(20000, retrievedModel.Latitude, 0.001);
                Assert.AreEqual(-20000, retrievedModel.Longitude, 0.001);
            }
        }

        [Test]
        public void SaveLoadInitialTemperature()
        {
            //create a dummy model
            var model1 = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            string path = TestHelper.GetCurrentMethodName() + ".dsproj";

            model1.UseTemperature = true;

            var branch = model1.Network.Branches.First();
            model1.InitialTemperature.Arguments[0].SetValues(new List<INetworkLocation>()
            {
                new NetworkLocation(branch, 0),
                new NetworkLocation(branch, branch.Length /2),
                new NetworkLocation(branch, branch.Length)
            });
            model1.InitialTemperature.Components[0].SetValues(new List<double>{0.123, 0.456, 0.789});
            
            //save 
            SaveModelToProject(model1, path);

            //reload
            using (var projectRepository = factory.CreateNew())
            {
                Project retrievedProject = projectRepository.Open(path);
                var retrievedModel = (WaterFlowModel1D)retrievedProject.RootFolder.Models.FirstOrDefault();
                Assert.NotNull(retrievedModel);

                var retrievedBranch = retrievedModel.Network.Branches.First();
                var retrievedLoc0 = new NetworkLocation(retrievedBranch, 0);
                var retrievedLoc1 = new NetworkLocation(retrievedBranch, branch.Length / 2);
                var retrievedLoc2 = new NetworkLocation(retrievedBranch, branch.Length);

                var retrievedInitialTemperature = retrievedModel.InitialTemperature;
                Assert.AreEqual(0.123, (double)retrievedInitialTemperature[retrievedLoc0], 0.001);
                Assert.AreEqual(0.456, (double)retrievedInitialTemperature[retrievedLoc1], 0.001);
                Assert.AreEqual(0.789, (double)retrievedInitialTemperature[retrievedLoc2], 0.001);
            }
        }

        [Test]
        public void SaveLoadTemperatureMeteoData()
        {
            //create a dummy model
            var model1 = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            string path = TestHelper.GetCurrentMethodName() + ".dsproj";

            model1.UseTemperature = true;
            var t0 = DateTime.Now;
            var t1 = t0.AddMinutes(30);
            var t2 = t1.AddMinutes(30);

            model1.MeteoData.Arguments[0].SetValues(new List<DateTime>{ t0, t1, t2 });
            model1.MeteoData.Components[0].SetValues(new List<double> { 0.123, 0.456, 0.789 }); // AirTemperature
            model1.MeteoData.Components[1].SetValues(new List<double> { 0.147, 0.258, 0.369 }); // RelativeHumidity
            model1.MeteoData.Components[2].SetValues(new List<double> { 0.326, 0.159, 0.487 }); // Cloudiness
            
            //save 
            SaveModelToProject(model1, path);

            //reload
            using (var projectRepository = factory.CreateNew())
            {
                Project retrievedProject = projectRepository.Open(path);
                var retrievedModel = (WaterFlowModel1D)retrievedProject.RootFolder.Models.FirstOrDefault();
                Assert.NotNull(retrievedModel);

                var retrievedMeteoData = retrievedModel.MeteoData;
                var retrievedAirTemperatureValues = retrievedMeteoData.AirTemperature.Values;
                var retrievedRelativeHumidityValues = retrievedMeteoData.RelativeHumidity.Values;
                var retrievedCloudinessValues = retrievedMeteoData.Cloudiness.Values;

                Assert.AreEqual(0.123, (double)retrievedAirTemperatureValues[0], 0.001);
                Assert.AreEqual(0.456, (double)retrievedAirTemperatureValues[1], 0.001);
                Assert.AreEqual(0.789, (double)retrievedAirTemperatureValues[2], 0.001);

                Assert.AreEqual(0.147, (double)retrievedRelativeHumidityValues[0], 0.001);
                Assert.AreEqual(0.258, (double)retrievedRelativeHumidityValues[1], 0.001);
                Assert.AreEqual(0.369, (double)retrievedRelativeHumidityValues[2], 0.001);

                Assert.AreEqual(0.326, (double)retrievedCloudinessValues[0], 0.001);
                Assert.AreEqual(0.159, (double)retrievedCloudinessValues[1], 0.001);
                Assert.AreEqual(0.487, (double)retrievedCloudinessValues[2], 0.001);
            }
        }

    }
}