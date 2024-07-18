using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DelftTools.Utils.NetCdf;
using DeltaShell.IntegrationTestUtils.Builders;
using DeltaShell.IntegrationTestUtils.NHibernate;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.SharpMapGis;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;
using SharpMap.Converters.WellKnownText;

namespace DeltaShell.Plugins.NetworkEditor.IntegrationTests.NHibernate
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    [Category(TestCategory.Slow)]
    public class NHibernateNetCdfFunctionStoreTest : NHibernateIntegrationTestBase
    {
        [Test]
        public void SaveAndRetrieveFunctionWithNetCdfFunctionValueStore()
        {
            var store = new NetCdfFunctionStore();
            store.CreateNew(TestHelper.GetCurrentMethodName() + ".nc");

            IFunction function = FunctionTestHelper.CreateSimpleFunction(store);

            // make sure we have some values in our function
            IVariable f1 = function.Components[0];
            IList<double> values = function.GetValues<double>(new ComponentFilter(f1));
            Assert.AreEqual(6, values.Count);

            // setup repository
            string path = TestHelper.GetCurrentMethodName() + ".dsproj";

            ProjectRepository.Create(path);

            // save project with a function
            Project project = ProjectRepository.GetProject();
            project.RootFolder.Items.Add(new DataItem(function, "function"));

            ProjectRepository.SaveOrUpdate(project);

            // retrieve 
            Project retrievedProject = ProjectRepository.Open(path);
            var retrievedFunction = (Function) retrievedProject.RootFolder.DataItems.FirstOrDefault().Value;

            // asserts
            IMultiDimensionalArray<double> retrievedValues = retrievedFunction.GetValues<double>(new ComponentFilter(f1));
            Assert.AreEqual(values.Count, retrievedValues.Count);
            Assert.AreEqual(4, retrievedFunction.Store.Functions.Count);
            Assert.AreEqual(102.0, retrievedFunction.Components[0][1.0, 0.0]);

            store.Dispose();
        }

        [Test]
        public void SaveAndRetrieveNetworkCoverageWithNetCdf()
        {
            string path = TestHelper.GetCurrentMethodName() + ".dsproj";

            // create network
            var network = new Network();

            var node1 = new Node("node1");
            var node2 = new Node("node2");
            var node3 = new Node("node3");

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            network.Nodes.Add(node3);

            var branch1 = new Branch("branch1", node1, node2, 100.0) {Geometry = GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)")};
            var branch2 = new Branch("branch2", node1, node2, 200.0) {Geometry = GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)")};
            network.Branches.Add(branch1);
            network.Branches.Add(branch2);

            ProjectRepository.Create(path);

            var project = new Project();
            project.RootFolder.Add(new DataItem(network));
            ProjectRepository.SaveOrUpdate(project);

            var store = new NetCdfFunctionStore();
            store.CreateNew(TestHelper.GetCurrentMethodName() + ".nc");
            INetworkCoverage networkCoverage = new NetworkCoverage {Network = network};
            store.TypeConverters.Add(new NetworkLocationTypeConverter(network));
            store.Functions.Add(networkCoverage);

            // set values
            networkCoverage[new NetworkLocation(branch1, 0.0)] = 0.1;
            networkCoverage[new NetworkLocation(branch1, 100.0)] = 0.2;
            networkCoverage[new NetworkLocation(branch2, 0.0)] = 0.3;
            networkCoverage[new NetworkLocation(branch2, 50.0)] = 0.4;
            networkCoverage[new NetworkLocation(branch2, 200.0)] = 0.5;

            project.RootFolder.Add(new DataItem(networkCoverage));
            ProjectRepository.SaveOrUpdate(project);

            //reload
            Project retrievedProject = ProjectRepository.Open(path);
            IDataItem[] retrievedDataItems = retrievedProject.RootFolder.DataItems.ToArray();
            var retrievedNetwork = (INetwork) retrievedDataItems[0].Value;
            var retrievedNetworkCoverage = (INetworkCoverage) retrievedDataItems[1].Value;

            //compare
            Assert.AreEqual(new NetworkLocation(retrievedNetwork.Branches[0], 0.0), retrievedNetworkCoverage.Arguments[0].Values[0]);
            Assert.AreEqual(networkCoverage.Components[0].Values.Count, retrievedNetworkCoverage.Components[0].Values.Count);
            Assert.AreEqual(network.Branches.Count, retrievedNetwork.Branches.Count);
            Assert.AreEqual(retrievedNetworkCoverage.Network, retrievedNetwork);

            store.Dispose();
        }

        [Test]
        public void SaveNetworkCoverageInNetCdfAndCheckCFConventionsLikeUnitAndStandardName()
        {
            // create network
            var network = new Network();

            var node1 = new Node("node1");
            var node2 = new Node("node2");
            var node3 = new Node("node3");

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            network.Nodes.Add(node3);

            var branch1 = new Branch("branch1", node1, node2, 100.0) {Geometry = GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)")};
            var branch2 = new Branch("branch2", node1, node2, 200.0) {Geometry = GeometryFromWKT.Parse("LINESTRING (100 0, 200 0)")};
            network.Branches.Add(branch1);
            network.Branches.Add(branch2);

            string path = TestHelper.GetCurrentMethodName() + ".nc";

            using (var store = new NetCdfFunctionStore())
            {
                store.CreateNew(path);
                INetworkCoverage networkCoverage = new NetworkCoverage {Network = network};
                store.TypeConverters.Add(new NetworkLocationTypeConverter(network));
                store.Functions.Add(networkCoverage);

                // set values
                networkCoverage[new NetworkLocation(branch1, 0.0)] = 0.1;
                networkCoverage[new NetworkLocation(branch1, 100.0)] = 0.2;
                networkCoverage[new NetworkLocation(branch2, 0.0)] = 0.3;
                networkCoverage[new NetworkLocation(branch2, 50.0)] = 0.4;
                networkCoverage[new NetworkLocation(branch2, 200.0)] = 0.5;

                store.Flush();
                store.Close();
            }

            // open netcdf manually
            NetCdfFile file = NetCdfFile.OpenExisting(path);
            try
            {
                NetCdfVariable varX = file.GetVariableByName("x");
                string unitValue = file.GetAttributeValue(varX, FunctionAttributes.Units);
                Assert.AreEqual("meters", unitValue);

                string standardName = file.GetAttributeValue(varX, FunctionAttributes.StandardName);
                Assert.AreEqual("projection_x_coordinate", standardName);
            }
            finally
            {
                file.Close();
            }
        }

        [Test]
        public void SaveAndRetrieveFeatureCoverageWithNetCdfWithoutFeatures()
        {
            var featureCoverage = new FeatureCoverage("test") {IsTimeDependent = true};

            featureCoverage.Arguments.Add(new Variable<IFeature>("feature"));
            featureCoverage.Components.Add(new Variable<double>("value"));
            featureCoverage.FeatureVariable.FixedSize = 0;

            var store = new NetCdfFunctionStore();
            store.CreateNew(TestHelper.GetCurrentMethodName() + ".nc");
            store.Functions.Add(featureCoverage);

            string path = TestHelper.GetCurrentMethodName() + ".dsproj";
            ProjectRepository.Create(path);

            //save
            var project = new Project();
            project.RootFolder.Add(new DataItem(featureCoverage, DataItemRole.Output));
            ProjectRepository.SaveOrUpdate(project);

            //reload
            Project retrievedProject = ProjectRepository.Open(path);
            IDataItem[] retrievedDataItems = retrievedProject.RootFolder.DataItems.ToArray();
            var retrievedFeatureCoverage = (IFeatureCoverage) retrievedDataItems[0].Value;

            Assert.AreEqual(0, retrievedFeatureCoverage.Features.Count);
            Assert.AreEqual(0, retrievedFeatureCoverage.FeatureVariable.Values.Count);

            store.Dispose();
        }

        [Test]
        public void SaveAndRetrieveRegularGridCoverageWithNetCdf()
        {
            string path = TestHelper.GetCurrentMethodName() + ".dsproj";

            ProjectRepository.Create(path);

            var project = new Project();

            //setup a store with a coverage inside
            var store = new NetCdfFunctionStore();
            store.CreateNew(TestHelper.GetCurrentMethodName() + ".nc");

            //grid
            IRegularGridCoverage regularGridCoverage = new RegularGridCoverage();
            store.Functions.Add(regularGridCoverage);
            regularGridCoverage.Resize(100, 100, 1, 1);

            // set values
            project.RootFolder.Add(new DataItem(regularGridCoverage));
            ProjectRepository.SaveOrUpdate(project);

            //reload
            Project retrievedProject = ProjectRepository.Open(path);
            var retrievedCoverage = (IRegularGridCoverage) retrievedProject.RootFolder.DataItems.FirstOrDefault().Value;

            //compare
            Assert.IsTrue(retrievedCoverage.Store is NetCdfFunctionStore);
            Assert.AreEqual(regularGridCoverage.Components[0].Values.Count, retrievedCoverage.Components[0].Values.Count);
            Assert.AreEqual(regularGridCoverage.X.Values.Count, retrievedCoverage.X.Values.Count);
            Assert.AreEqual(regularGridCoverage.Y.Values.Count, retrievedCoverage.Y.Values.Count);

            store.Dispose();
        }

        [Test]
        public void SaveAndRetrieveRegularGridCoverageWithNetCdfAndClearedAddedComponent()
        {
            string path = TestHelper.GetCurrentMethodName() + ".dsproj";

            ProjectRepository.Create(path);

            var project = new Project();

            //setup a store with a coverage inside
            var store = new NetCdfFunctionStore();
            store.CreateNew(TestHelper.GetCurrentMethodName() + ".nc");

            //grid
            IRegularGridCoverage regularGridCoverage = new RegularGridCoverage();
            store.Functions.Add(regularGridCoverage);
            regularGridCoverage.Resize(100, 100, 1, 1);

            project.RootFolder.Add(regularGridCoverage);
            ProjectRepository.SaveOrUpdate(project);

            regularGridCoverage.Clear();
            regularGridCoverage.Components.Clear();
            regularGridCoverage.Components.Add(new Variable<double>("new_component"));
            ProjectRepository.SaveOrUpdate(project); // bang! exception, cascade

            //reload
            Project retrievedProject = ProjectRepository.Open(path);
            var retrievedCoverage = (IRegularGridCoverage) retrievedProject.RootFolder.DataItems.FirstOrDefault().Value;

            //compare
            Assert.IsTrue(retrievedCoverage.Store is NetCdfFunctionStore);
            Assert.AreEqual(regularGridCoverage.Components[0].Values.Count, retrievedCoverage.Components[0].Values.Count);
            Assert.AreEqual(regularGridCoverage.X.Values.Count, retrievedCoverage.X.Values.Count);
            Assert.AreEqual(regularGridCoverage.Y.Values.Count, retrievedCoverage.Y.Values.Count);

            store.Dispose();
        }

        private static IGui CreateRunningGui()
        {
            var pluginsToAdd = new List<IPlugin>()
            {
                new NHibernateDaoApplicationPlugin(),
                new NetCdfApplicationPlugin(),
                new CommonToolsApplicationPlugin(),
                new SharpMapGisApplicationPlugin(),
                new NetworkEditorApplicationPlugin(),
            };
            IGui gui = new DeltaShellGuiBuilder().WithPlugins(pluginsToAdd).Build();

            gui.Run();

            return gui;
        }
        [Test]
        public void SaveProjectNCToNetCdfNoValues()
        {
            using (IGui gui = CreateRunningGui())
            {
                IProjectService projectService = gui.Application.ProjectService;
                Project project = projectService.CreateProject();

                // create test network
                INetwork network = NHibernateTestsHelper.CreateDummyNetwork();
                // add network coverage
                var networkCoverage = new NetworkCoverage {Network = network};
                var dataItem = new DataItem(networkCoverage, DataItemRole.Output);
                project.RootFolder.Add(dataItem);

                // save project in test directory
                //don't use CurrentMethodName it is too long for the build server
                var projectPath = "1.dsproj";
                projectService.SaveProjectAs(projectPath);

                Project retrievedProject = projectService.OpenProject(projectPath);
                var networkCoverageReOpend = (INetworkCoverage) retrievedProject.RootFolder.DataItems.First(di => di.ValueType == typeof(NetworkCoverage)).Value;

                // set values
                IBranch branch1 = network.Branches[0];
                networkCoverageReOpend[new NetworkLocation(branch1, 0.0)] = 0.1;
                networkCoverageReOpend[new NetworkLocation(branch1, 100.0)] = 0.2;
                networkCoverageReOpend[new NetworkLocation(branch1, 200.0)] = 0.3;
            }
        }

        [Test]
        public void CloseProjectUnlocksNetCdfFiles()
        {
            using (IGui gui = CreateRunningGui())
            {
                IProjectService projectService = gui.Application.ProjectService;
                Project project = projectService.CreateProject();

                // add a coverage to the project.
                INetwork network = NHibernateTestsHelper.CreateDummyNetwork();
                var networkCoverage = new NetworkCoverage {Network = network};
                var dataItem = new DataItem(networkCoverage, DataItemRole.Output);
                project.RootFolder.Add(dataItem);

                //save the project to a local dir
                string projectPath = TestHelper.GetCurrentMethodName() + ".dsproj";
                string dataPath = projectPath + "_data";
                if (Directory.Exists(dataPath))
                {
                    Directory.Delete(dataPath, true);
                }

                projectService.SaveProjectAs(projectPath);

                //define some values in the coverage forcing a write
                IBranch branch1 = network.Branches[0];
                networkCoverage[new NetworkLocation(branch1, 0.0)] = 0.1;

                //assert a file has been written
                Assert.AreEqual(1, Directory.GetFiles(dataPath).Length);

                //close the project 
                projectService.CloseProject();

                //delete the files
                Directory.Delete(dataPath, true);
            }
        }

        [Test]
        public void CloseProjectDeletesUnsavedNetCdfFiles()
        {
            using (IGui gui = CreateRunningGui())
            {
                IProjectService projectService = gui.Application.ProjectService;
                Project project = projectService.CreateProject();

                // save the project
                var projectPath = "ci1.dsproj";
                string dataPath = projectPath + "_data";
                projectService.SaveProjectAs(projectPath);

                // add a coverage to the project.
                INetwork network = NHibernateTestsHelper.CreateDummyNetwork();
                var networkCoverage = new NetworkCoverage {Network = network};
                var dataItem = new DataItem(networkCoverage, DataItemRole.Output);
                project.RootFolder.Add(dataItem);

                // define some values in the coverage forcing a write
                IBranch branch1 = network.Branches[0];
                networkCoverage[new NetworkLocation(branch1, 0.0)] = 0.1;

                // assert a file has been written
                Assert.AreEqual(1, Directory.GetFiles(dataPath).Length);

                // close the project 
                projectService.CloseProject();

                // assert file does not exist anymore
                Assert.AreEqual(0, Directory.GetFiles(dataPath).Length, "file still exists");
            }
        }

        [Test]
        public void SaveAsProjectDeletesUnsavedNetCdfFilesInSourceFolder()
        {
            using (IGui gui = CreateRunningGui())
            {
                IProjectService projectService = gui.Application.ProjectService;
                Project project = projectService.CreateProject();

                // save the project
                var projectPath = "si2.dsproj";
                string dataPath = projectPath + "_data";
                projectService.SaveProjectAs(projectPath);

                // add a coverage to the project.
                INetwork network = NHibernateTestsHelper.CreateDummyNetwork();
                var networkCoverage = new NetworkCoverage {Network = network};
                var dataItem = new DataItem(networkCoverage, DataItemRole.Output);
                project.RootFolder.Add(dataItem);

                // define some values in the coverage forcing a write
                IBranch branch1 = network.Branches[0];
                networkCoverage[new NetworkLocation(branch1, 0.0)] = 0.1;

                // assert a file has been written
                Assert.AreEqual(1, Directory.GetFiles(dataPath).Length);

                // save the project under new name
                projectService.SaveProjectAs("newpath.dsproj");

                // assert file does not exist anymore in old directory
                Assert.AreEqual(0, Directory.GetFiles(dataPath).Length, "file still exists");
            }
        }

        [Test]
        public void SaveProjectDeletesDeletedNetCdfFilesInSourceFolder()
        {
            using (IGui gui = CreateRunningGui())
            {
                IProjectService projectService = gui.Application.ProjectService;
                Project project = projectService.CreateProject();

                // save the project
                var projectPath = "si1.dsproj";
                string dataPath = projectPath + "_data";

                // add a coverage to the project.
                INetwork network = NHibernateTestsHelper.CreateDummyNetwork();
                var networkCoverage = new NetworkCoverage {Network = network};
                var dataItem = new DataItem(networkCoverage, DataItemRole.Output);
                project.RootFolder.Add(dataItem);

                // define some values in the coverage forcing a write
                IBranch branch1 = network.Branches[0];
                networkCoverage[new NetworkLocation(branch1, 0.0)] = 0.1;

                // save project
                projectService.SaveProjectAs(projectPath);

                // assert a file has been written
                Assert.AreEqual(1, Directory.GetFiles(dataPath).Length);

                // delete item
                project.RootFolder.Items.Remove(dataItem);

                // save the project under same name
                projectService.SaveProject();

                // assert file does not exist anymore
                Assert.AreEqual(0, Directory.GetFiles(dataPath).Length, "file still exists");
            }
        }

        [Test]
        public void CloseProjectKeepsDeletedNetCdfFilesInSourceFolder()
        {
            using (IGui gui = CreateRunningGui())
            {
                IProjectService projectService = gui.Application.ProjectService;
                Project project = projectService.CreateProject();

                // save the project
                var projectPath = "ci2.dsproj";
                string dataPath = projectPath + "_data";

                // add a coverage to the project.
                INetwork network = NHibernateTestsHelper.CreateDummyNetwork();
                var networkCoverage = new NetworkCoverage {Network = network};
                var dataItem = new DataItem(networkCoverage, DataItemRole.Output);
                project.RootFolder.Add(dataItem);

                // define some values in the coverage forcing a write
                IBranch branch1 = network.Branches[0];
                networkCoverage[new NetworkLocation(branch1, 0.0)] = 0.1;

                // save project
                projectService.SaveProjectAs(projectPath);

                // assert a file has been written
                Assert.AreEqual(1, Directory.GetFiles(dataPath).Length);

                // delete item
                project.RootFolder.Items.Remove(dataItem);

                // close the project
                projectService.CloseProject();

                // assert file still exists
                Assert.AreEqual(1, Directory.GetFiles(dataPath).Length, "file still exists");
            }
        }

        [Test]
        public void CloseProjectRollbacksDeletedNetCdfStoreInTransaction()
        {
            using (IGui gui = CreateRunningGui())
            {
                IProjectService projectService = gui.Application.ProjectService;
                Project project = projectService.CreateProject();

                // save the project
                var projectPath = "ci3.dsproj";
                string dataPath = projectPath + "_data";

                // add a coverage to the project.
                INetwork network = NHibernateTestsHelper.CreateDummyNetwork();
                var networkCoverage = new NetworkCoverage {Network = network};
                var dataItem = new DataItem(networkCoverage, DataItemRole.Output);
                project.RootFolder.Add(dataItem);

                // define some values in the coverage forcing a write
                IBranch branch1 = network.Branches[0];
                networkCoverage[new NetworkLocation(branch1, 0.0)] = 0.1;

                // save project
                projectService.SaveProjectAs(projectPath);

                // force changes file by modifying
                networkCoverage.Clear();
                networkCoverage[new NetworkLocation(branch1, 0.0)] = 0.2;

                // assert a file + changes file has been written
                Assert.AreEqual(2, Directory.GetFiles(dataPath).Length);

                // delete item
                project.RootFolder.Items.Remove(dataItem);

                // close the project
                projectService.CloseProject();

                // assert file still exists
                int numFiles = Directory.GetFiles(dataPath).Length;
                if (numFiles == 0)
                {
                    Assert.Fail("All files lost!");
                }
                else if (numFiles == 2)
                {
                    Assert.Fail("Changes file not cleaned up");
                }
            }
        }

        [Test]
        public void CloseProjectDoesNotLockDeletedNetCdfStore()
        {
            using (IGui gui = CreateRunningGui())
            {
                IProjectService projectService = gui.Application.ProjectService;
                Project project = projectService.CreateProject();

                // save the project
                var projectPath = "ci3.dsproj";
                string dataPath = projectPath + "_data";

                // add a coverage to the project.
                INetwork network = NHibernateTestsHelper.CreateDummyNetwork();
                var networkCoverage = new NetworkCoverage {Network = network};
                var dataItem = new DataItem(networkCoverage, DataItemRole.Output);
                project.RootFolder.Add(dataItem);

                // define some values in the coverage forcing a write
                IBranch branch1 = network.Branches[0];
                networkCoverage[new NetworkLocation(branch1, 0.0)] = 0.1;

                // save project
                projectService.SaveProjectAs(projectPath);

                // force changes file by modifying
                networkCoverage.Clear();
                networkCoverage[new NetworkLocation(branch1, 0.0)] = 0.2;

                // assert a file + changes file has been written
                Assert.AreEqual(2, Directory.GetFiles(dataPath).Length);

                // delete item
                project.RootFolder.Items.Remove(dataItem);

                // close the project
                projectService.CloseProject();

                // assert file still exists
                string[] fileNames = Directory.GetFiles(dataPath);
                Assert.AreEqual(1, fileNames.Length);
                try
                {
                    File.Delete(fileNames[0]);
                }
                catch (IOException)
                {
                    Assert.Fail("File locked!");
                }
            }
        }

        [Test]
        public void ModifyingNetCdfFunctionStoreAfterSaveShouldCreateChangesFile()
        {
            using (IGui gui = CreateRunningGui())
            {
                IProjectService projectService = gui.Application.ProjectService;
                Project project = projectService.CreateProject();

                // add a coverage to the project.
                INetwork network = NHibernateTestsHelper.CreateDummyNetwork();
                var networkCoverage = new NetworkCoverage {Network = network};
                var dataItem = new DataItem(networkCoverage, DataItemRole.Output);
                project.RootFolder.Add(dataItem);

                //save the project to a local dir
                var projectPath = "changesfiles.dsproj";
                string dataPath = projectPath + "_data";
                if (Directory.Exists(dataPath))
                {
                    Directory.Delete(dataPath, true);
                }

                projectService.SaveProjectAs(projectPath);

                //assert file has been written
                Assert.AreEqual(0, Directory.GetFiles(dataPath, "*.nc.changes").Length);
                Assert.AreEqual(1, Directory.GetFiles(dataPath, "*.nc").Length);

                //define some values in the coverage forcing a write
                IBranch branch1 = network.Branches[0];
                networkCoverage.Clear();
                networkCoverage[new NetworkLocation(branch1, 0.0)] = 0.1;

                //assert a changes file has been written
                Assert.AreEqual(1, Directory.GetFiles(dataPath, "*.nc.changes").Length);
                Assert.AreEqual(1, Directory.GetFiles(dataPath, "*.nc").Length);

                //close the project 
                projectService.CloseProject();

                //assert changes file has been removed
                Assert.AreEqual(0, Directory.GetFiles(dataPath, "*.nc.changes").Length);
                Assert.AreEqual(1, Directory.GetFiles(dataPath, "*.nc").Length);
            }
        }

        [Test]
        public void SaveAsProjectThreeTimesWithChangesInNetCdfCopiesFile()
        {
            using (IGui gui = CreateRunningGui())
            {
                IProjectService projectService = gui.Application.ProjectService;
                Project project = projectService.CreateProject();

                // add a coverage to the project.
                INetwork network = NHibernateTestsHelper.CreateDummyNetwork();
                var networkCoverage = new NetworkCoverage {Network = network};
                var dataItem = new DataItem(networkCoverage, DataItemRole.Output);
                project.RootFolder.Add(dataItem);

                //save the project to a local dir
                var folder = "saveAsThriceWithChanges";
                FileUtils.DeleteIfExists(folder);

                string projectPath1 = folder + "\\changesfiles1.dsproj";
                string projectPath2 = folder + "\\changesfiles2.dsproj";
                string projectPath3 = folder + "\\changesfiles3.dsproj";

                projectService.SaveProjectAs(projectPath1);

                networkCoverage.Clear();
                IBranch branch1 = network.Branches[0];
                networkCoverage[new NetworkLocation(branch1, 0.0)] = 0.1;

                projectService.SaveProjectAs(projectPath2);
                projectService.SaveProjectAs(projectPath3);

                //assert files has been written
                Assert.AreEqual(1, Directory.GetFiles(projectPath1 + "_data", "*.nc").Length, "No NC file in path1");
                Assert.AreEqual(1, Directory.GetFiles(projectPath2 + "_data", "*.nc").Length, "No NC file in path2");
                Assert.AreEqual(1, Directory.GetFiles(projectPath3 + "_data", "*.nc").Length, "No NC file in path3");
            }
        }

        [Test]
        public void SaveAsProjectTwiceWithNetCdfCopiesFile()
        {
            using (IGui gui = CreateRunningGui())
            {
                IProjectService projectService = gui.Application.ProjectService;
                Project project = projectService.CreateProject();

                // add a coverage to the project.
                INetwork network = NHibernateTestsHelper.CreateDummyNetwork();
                var networkCoverage = new NetworkCoverage {Network = network};
                var dataItem = new DataItem(networkCoverage, DataItemRole.Output);
                project.RootFolder.Add(dataItem);

                //save the project to a local dir
                var folder = "saveAsTwice";
                FileUtils.DeleteIfExists(folder);

                string projectPath1 = folder + "\\changesfiles1.dsproj";
                string projectPath2 = folder + "\\changesfiles2.dsproj";

                projectService.SaveProjectAs(projectPath1);
                projectService.SaveProjectAs(projectPath2);

                //assert files has been written
                Assert.AreEqual(1, Directory.GetFiles(projectPath1 + "_data", "*.nc").Length);
                Assert.AreEqual(1, Directory.GetFiles(projectPath2 + "_data", "*.nc").Length);
            }
        }
    }
}
