using System;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
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
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel;
using DeltaShell.Plugins.ImportExport.Sobek;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.Scripting;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using GeoAPI.Extensions.Networks;
using log4net.Core;
using NUnit.Framework;
using SharpTestsEx;
using Control = System.Windows.Controls.Control;

namespace Sobek.IntegrationTests
{
    [TestFixture]
    [Category(TestCategory.Performance)]
    public class NHibernateWaterFlowModel1DPerformanceTest
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

        private static HydroNetwork GetBypassNetwork()
        {
            var modelPath =
                TestHelper.GetTestDataPath(
                    typeof(SobekWaterFlowModel1DImporterTest).Assembly, @"BYPASS.lit\3\Network.TP");
            var importer = new SobekNetworkImporter();
            var network = (HydroNetwork)importer.ImportItem(modelPath);
            return network;
        }
        private static WaterFlowModel1D GetMaasModel()
        {
            var modelPath =
                TestHelper.GetTestDataPath(
                    typeof(SobekWaterFlowModel1DImporterTest).Assembly, @"Maas.lit\5\Network.TP");
            var importer = new SobekWaterFlowModel1DImporter();
            importer.TargetItem = new WaterFlowModel1D();
            var model = (WaterFlowModel1D)importer.ImportItem(modelPath);
            return model;
        }
        private double SaveNxNModel(WaterFlowModel1D modelNXN)
        {
            var startTime = DateTime.Now;
            var compositeModel = new CompositeModel();
            compositeModel.Activities.Add(modelNXN);

            Project project = projectRepository.GetProject();
            project.RootFolder.Items.Add(compositeModel);
            project.RootFolder.Items.Add(new DataItem(modelNXN.Network));

            projectRepository.SaveOrUpdate(project);
            projectRepository.Close();
            return (DateTime.Now - startTime).TotalMilliseconds;
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

        private void SaveProjectItem(IProjectItem model1, string path)
        {
            using (var repository = factory.CreateNew())
            {
                repository.Create(path);
                var project = repository.GetProject();
                project.RootFolder.Add(model1);
                repository.SaveOrUpdate(project);
            }
        }

        [Test]
        public void SaveImportedZwolleModelShouldBeFast()
        {
            var modelPath = 
                TestHelper.GetTestDataPath(typeof(SobekWaterFlowModel1DImporterTest).Assembly, @"SW_max_1.lit\3\Network.TP");

            var importedModel = (HydroModel)new SobekHydroModelImporter().ImportItem(modelPath);
            projectRepository.Create(TestHelper.GetCurrentMethodName()+".dsproj");
            var project = projectRepository.GetProject();
            project.RootFolder.Add(importedModel);

            TestHelper.AssertIsFasterThan(2500, () => projectRepository.SaveOrUpdate(project), true);
        }

        [Test]
        public void OpenSavedMaasModelInApplicationIsFast()
        {
            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new RealTimeControlApplicationPlugin());
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());

                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                
                gui.Run();

                //first save it
                var maasModel = GetMaasModel();
                var tempFileName = TestHelper.GetCurrentMethodName();
                gui.Application.Project.RootFolder.Add(maasModel);
                gui.Application.SaveProjectAs(tempFileName);

                Action formShownAction =
                    delegate
                        {
                            TestHelper.AssertIsFasterThan(7000, () =>
                                                          gui.Application.OpenProject(tempFileName));

                        };
                WpfTestHelper.ShowModal((Control) gui.MainWindow, formShownAction);
            }
        }

        [Test]
        public void OpenByPassNetworkInApplicationIsFast()
        {
            using (var gui = new DeltaShellGui())
            {
                var app = gui.Application;
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new RealTimeControlApplicationPlugin());
                app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
                app.Plugins.Add(new NetCdfApplicationPlugin());

                gui.Plugins.Add(new ProjectExplorerGuiPlugin());
                
                gui.Run();

                //first save it
                var network = GetBypassNetwork();
                var tempFileName = TestHelper.GetCurrentMethodName() + ".dsproj";

                FileUtils.DeleteIfExists(tempFileName);
                SaveProjectItem(new DataItem(network), tempFileName);

                //var tempFileName =
                //@"D:\src\DeltaShell\delta-shell\test\Plugins\DelftModels\DeltaShell.Plugins.DelftModels.WaterFlowModel.IntegrationTests\bin\Debug\NHibernateWaterFlowModel1DTest.LoadBypassModelShouldBeFast.dsproj";
                //reload in app..in app because without app we get a fast load due to lazy loading etc..
                //Action<Form> formShownAction = delegate
                //{
                LogHelper.ConfigureLogging(Level.Debug);
                    TestHelper.AssertIsFasterThan(6700, () =>
                                                            {
                                                                gui.Application.OpenProject(
                                                                    tempFileName);
                                                                var loadedNetwork =
                                                                    (IHydroNetwork)
                                                                    gui.Application.Project.RootFolder.DataItems.First()
                                                                        .Value;

                                                            });

                //};
                //WpfTestHelper.ShowModal((Control)gui.MainWindow, formShownAction);
            }
        }

        [Test]
        public void SaveImportedMaasModelIsFast()
        {
            WaterFlowModel1D model1 = GetMaasModel();
            var path = TestHelper.GetCurrentMethodName() + ".dsproj";
            using (var repository = factory.CreateNew())
            {
                repository.Create(path);
                var project = repository.GetProject();
                project.RootFolder.Add(model1);

                TestHelper.AssertIsFasterThan(4500, () =>
                                                        {

                                                            repository.SaveOrUpdate(project);
                                                        }, true);

            }
        }

        [Test]
        public void SaveImportedModelInApplicationShouldBeFast()
        {
            //LogHelper.ResetLogging();
            using (var gui = GetRunningGuiWithFlowPlugins())
            {
                IApplication app = gui.Application;
                Action formShownAction = delegate
                                             {

                                                 var modelPath =
                                                     TestHelper.GetTestDataPath(
                                                         typeof(SobekWaterFlowModel1DImporterTest).Assembly,
                                                         @"SW_max_1.lit\3\Network.TP");
                                                 //what a cumbersome syntax here..
                                                 var importedModel = (HydroModel)new SobekHydroModelImporter(false, false).ImportItem(modelPath);

                                                 app.Project.RootFolder.Add(importedModel);
                                                 //open a view for the network
                                                 gui.CommandHandler.OpenView(importedModel.Region.SubRegions.OfType<HydroNetwork>().FirstOrDefault());
                                                 //cannot use test name ...too long for me
                                                 const string projectPath = "test.dsproj";
                                                 //10 seconds is still very slow but much better than > 3 minutes
                                                 TestHelper.AssertIsFasterThan(4000,
                                                     () =>
                                                         {
                                                             app.SaveProjectAs(projectPath);
                                                             app.CloseProject();
                                                         }, true);
                                             };

                WpfTestHelper.ShowModal((Control) gui.MainWindow, formShownAction);
            }
        }

        [Test]
        public void SaveImportedModelWithDiscretizationsShouldBeFast()
        {
            using (var gui = GetRunningGuiWithFlowPlugins())
            {
                IApplication app = gui.Application;
                
                var modelPath =
                    TestHelper.GetTestDataPath(
                        typeof(SobekWaterFlowModel1DImporterTest).Assembly,
                        @"3testjz\Network.TP");
                //what a cumbersome syntax here..
                var importedModel = (HydroModel)new SobekHydroModelImporter(false, false).ImportItem(modelPath);
                app.Project.RootFolder.Add(importedModel);

                const string projectPath = "test.dsproj";
                //8 seconds for normal
                //7 seconds no function-values at all.
                //3.5 seconds no branchfeatures (structures) at all. 
                TestHelper.AssertIsFasterThan(11000,
                    () =>
                        {
                            app.SaveProjectAs(projectPath);
                            app.CloseProject();
                        }, true);
            }
        }

        [Test]
        public void LoadBypassModelShouldBeFast()
        {
            var path = TestHelper.GetCurrentMethodName() + ".dsproj";

            //save the bypass with discretization
            SaveBypassModel(path,false);


            TestHelper.AssertIsFasterThan(8500, () =>
                                                    {
                                                        using (var repository = factory.CreateNew())
                                                        {
                                                            repository.Open(path);
                                                            //create a project with the model and save it
                                                            var project = repository.GetProject();
                                                            //assert the discretization is ok
                                                            var model = 
                                                                project.RootFolder.Items.OfType<HydroModel>().First()
                                                                .Activities.OfType<WaterFlowModel1D>().First();

                                                            Assert.AreEqual(1953, model.NetworkDiscretization.Locations.Values.Count);
                                                        }
                                                    }, true);
        }

        [Test]
        public void SaveModelWithTimeSerieAsBoundaryCondition()
        {
            var path = TestHelper.GetCurrentMethodName() + ".dsproj";

            //import by pass
            var modelPath =
                TestHelper.GetTestDataPath(
                    typeof(SobekWaterFlowModel1DImporterTest).Assembly, @"DS_Salt.lit\2\Network.TP");

            var importedModel = (HydroModel)new SobekHydroModelImporter().ImportItem(modelPath);

            projectRepository.Create(path);
            //create a project with the model and save it
            var project = projectRepository.GetProject();
            project.RootFolder.Items.Add(importedModel);

            TestHelper.AssertIsFasterThan(850, () => projectRepository.SaveOrUpdate(project));

            projectRepository.Close();
        }

        [Test, Category(TestCategory.WindowsForms)]
        public void SaveModelWithTimeSerieAsBoundaryConditionWithView()
        {
            var path = TestHelper.GetCurrentMethodName() + ".dsproj";

            //import by pass
            var modelPath =
                TestHelper.GetTestDataPath(
                    typeof(SobekWaterFlowModel1DImporterTest).Assembly, @"DS_Salt.lit\2\Network.TP");

            var modelImporter = new SobekWaterFlowModel1DImporter();
            modelImporter.TargetItem = new WaterFlowModel1D();
            var importedModel = (WaterFlowModel1D)modelImporter.ImportItem(modelPath);

            var view = new WaterFlowModel1DBoundaryNodeDataViewWpf();
            var boundaryData = new WaterFlowModel1DBoundaryNodeData
                                   {
                                       Name = "test",
                                       DataType = WaterFlowModel1DBoundaryNodeDataType.FlowTimeSeries,
                                       Data = importedModel.BoundaryConditions[0].Data
                                   };
            view.Data = boundaryData;

            Action onShown = delegate
            {
                using (var projectRepository = factory.CreateNew())
                {
                    projectRepository.Create(path);
                    //create a project with the model and save it
                    var project = projectRepository.GetProject();
                    project.RootFolder.Items.Add(importedModel);

                    //projectRepository.SaveOrUpdate(project);
                    TestHelper.AssertIsFasterThan(920, () => projectRepository.SaveOrUpdate(project));

                    projectRepository.Close();
                }
            };

            WpfTestHelper.ShowModal(view, onShown);
        }

        [Test]
        public void SaveOfModelShouldScale()
        {
            // note enabling logging; hangs the test
            var path5X5 = TestHelper.GetCurrentMethodName() + "5x5" + ".dsproj";
            projectRepository.Create(path5X5);

            var model5X5 = WaterFlowModel1DDemoModelTestHelper.CreateModelWithLargeNetwork(5);
            var totalMilliseconds5X5 = SaveNxNModel(model5X5);
            Assert.Less(totalMilliseconds5X5, 2000.0);

            var path25X25 = TestHelper.GetCurrentMethodName() + "25x25" + ".dsproj";
            projectRepository.Create(path25X25);

            var model25X25 = WaterFlowModel1DDemoModelTestHelper.CreateModelWithLargeNetwork(25);

            double totalMilliseconds25X25 = SaveNxNModel(model25X25);
            Assert.Less(totalMilliseconds25X25, 25 * totalMilliseconds5X5);
        }
    }
}