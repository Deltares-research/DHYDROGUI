using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Dao;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DelftTools.Utils.NetCdf;
using DeltaShell.IntegrationTestUtils.NHibernate;
using DeltaShell.NGHS.TestUtils.Builders;
using DeltaShell.Plugins.Data.NHibernate.DelftTools.Shell.Core.Dao;
using DeltaShell.Plugins.NetCDF;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap.Converters.WellKnownText;
using SharpTestsEx;

namespace DeltaShell.Plugins.NetworkEditor.IntegrationTests.NHibernate
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    [Category(TestCategory.Slow), Apartment(ApartmentState.STA)]
    public class NHibernateNetCdfFunctionStoreTest
    {
        private IProjectRepository projectRepository;

        [SetUp]
        public void SetUp()
        {
            projectRepository = CreateProjectRepository();

        }

        [TearDown]
        public void TearDown()
        {
            projectRepository.Dispose();
        }

        [Test]
        public void SaveAndRetrieveFunctionWithNetCdfFunctionValueStore()
        {
            var store = new NetCdfFunctionStore();
            store.CreateNew(TestHelper.GetCurrentMethodName() + ".nc");

            var function = FunctionTestHelper.CreateSimpleFunction(store);

            // make sure we have some values in our function
            var f1 = function.Components[0];
            IList<double> values = function.GetValues<double>(new ComponentFilter(f1));
            Assert.AreEqual(6, values.Count);


            // setup repository
            string path = TestHelper.GetCurrentMethodName() + ".dsproj";
            

            projectRepository.Create(path);

            // save project with a function
            var project = projectRepository.GetProject();
            project.RootFolder.Items.Add(new DataItem(function, "function"));

            //TODO: nhibernate inserts path into the store resulting in a bad DB with too many functions.
            projectRepository.SaveOrUpdate(project);

            // retrieve 
            var retrievedProject = projectRepository.Open(path);
            var retrievedFunction = (Function)retrievedProject.RootFolder.DataItems.FirstOrDefault().Value;

            // asserts
            var retrievedValues = retrievedFunction.GetValues<double>(new ComponentFilter(f1));
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

            var branch1 = new Branch("branch1", node1, node2, 100.0)
                              {
                                  Geometry = GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)")
                              };
            var branch2 = new Branch("branch2", node1, node2, 200.0)
                              {
                                  Geometry = GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)")
                              };
            network.Branches.Add(branch1);
            network.Branches.Add(branch2);
            
            projectRepository.Create(path);
            
            var project = new Project();
            project.RootFolder.Add(new DataItem(network));
            projectRepository.SaveOrUpdate(project);

            var store = new NetCdfFunctionStore();
            store.CreateNew(TestHelper.GetCurrentMethodName() + ".nc");
            INetworkCoverage networkCoverage = new NetworkCoverage { Network = network };
            store.TypeConverters.Add(new NetworkLocationTypeConverter(network));
            store.Functions.Add(networkCoverage);

            // set values
            networkCoverage[new NetworkLocation(branch1, 0.0)] = 0.1;
            networkCoverage[new NetworkLocation(branch1, 100.0)] = 0.2;
            networkCoverage[new NetworkLocation(branch2, 0.0)] = 0.3;
            networkCoverage[new NetworkLocation(branch2, 50.0)] = 0.4;
            networkCoverage[new NetworkLocation(branch2, 200.0)] = 0.5;

            project.RootFolder.Add(new DataItem(networkCoverage));
            projectRepository.SaveOrUpdate(project);

            //reload
            var retrievedProject = projectRepository.Open(path);
            IDataItem[] retrievedDataItems = retrievedProject.RootFolder.DataItems.ToArray();
            var retrievedNetwork = (INetwork)retrievedDataItems[0].Value;
            var retrievedNetworkCoverage = (INetworkCoverage)retrievedDataItems[1].Value;

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

            var branch1 = new Branch("branch1", node1, node2, 100.0)
                {
                    Geometry = GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)")
                };
            var branch2 = new Branch("branch2", node1, node2, 200.0)
                {
                    Geometry = GeometryFromWKT.Parse("LINESTRING (100 0, 200 0)")
                };
            network.Branches.Add(branch1);
            network.Branches.Add(branch2);
            
            var path = TestHelper.GetCurrentMethodName() + ".nc";

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
            var file = NetCdfFile.OpenExisting(path);
            try
            {
                var varX = file.GetVariableByName("x");
                var unitValue = file.GetAttributeValue(varX, FunctionAttributes.Units);
                Assert.AreEqual("meters", unitValue);

                var standardName = file.GetAttributeValue(varX, FunctionAttributes.StandardName);
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

            var path = TestHelper.GetCurrentMethodName() + ".dsproj";
            projectRepository.Create(path);

            //save
            var project = new Project();
            project.RootFolder.Add(new DataItem(featureCoverage, DataItemRole.Output));
            projectRepository.SaveOrUpdate(project);

            //reload
            var retrievedProject = projectRepository.Open(path);
            var retrievedDataItems = retrievedProject.RootFolder.DataItems.ToArray();
            var retrievedFeatureCoverage = (IFeatureCoverage)retrievedDataItems[0].Value;

            Assert.AreEqual(0, retrievedFeatureCoverage.Features.Count);
            Assert.AreEqual(0, retrievedFeatureCoverage.FeatureVariable.Values.Count);

            store.Dispose();
        }

        [Test]
        public void SaveAndRetrieveFeatureCoverageWithNetCdf()
        {
            string path = TestHelper.GetCurrentMethodName() + ".dsproj";

            //feature source
            var features = new IFeature[]
                               {
                                   new Bridge {Name = "feature1", Geometry = new Point(100, 100)},
                                   new Bridge {Name = "feature2", Geometry = new Point(200, 200)}
                               };
            
            //feature coverage
            var featureCoverage = new FeatureCoverage("test");

            var store = new NetCdfFunctionStore();
            store.CreateNew(TestHelper.GetCurrentMethodName() + ".nc");
            store.Functions.Add(featureCoverage);
            featureCoverage.Features = new EventedList<IFeature>(features);
            featureCoverage.Components.Add(new Variable<double>("value"));
            featureCoverage.Arguments.Add(new Variable<IFeature>("feature")); // Pump BranchStructure Weir IStructure

            for(int i = 0; i < featureCoverage.Features.Count; i++)
            {
                featureCoverage[featureCoverage.Features[i]] = Convert.ToDouble(i);
            }

            projectRepository.Create(path);
            

            //save
            var project = new Project();
            project.RootFolder.Add(new DataItem(featureCoverage, DataItemRole.Output));
            projectRepository.SaveOrUpdate(project);

            //reload
            var retrievedProject = projectRepository.Open(path);
            IDataItem[] retrievedDataItems = retrievedProject.RootFolder.DataItems.ToArray();
            var retrievedFeatureCoverage = (IFeatureCoverage)retrievedDataItems[0].Value;

            //test
            Assert.IsAssignableFrom(typeof(NetCdfFunctionStore), retrievedFeatureCoverage.Store);
            Assert.AreEqual(features.Length, retrievedFeatureCoverage.Features.Count);
            Assert.AreEqual(features.Length, retrievedFeatureCoverage.FeatureVariable.Values.Count);
            Assert.AreEqual(retrievedFeatureCoverage.Features[0], retrievedFeatureCoverage.Arguments[0].Values[0]);

            for (int i = 0; i < featureCoverage.Features.Count; i++)
            {
                Assert.AreEqual(retrievedFeatureCoverage[retrievedFeatureCoverage.Features[i]], Convert.ToDouble(i));
            }

            store.Dispose();
        }

        [Test]
        public void SaveAndRetrieveFeatureCoverageWithNetCdfAndThenClone()
        {
            var repo = new MockRepository();
            IFeature featureMock1 = repo.DynamicMultiMock<IFeature>(typeof(INameable));
            ((INameable)featureMock1).Name = "feature1-clone";
            IFeature featureMock2 = repo.DynamicMultiMock<IFeature>(typeof(INameable));
            ((INameable)featureMock2).Name = "feature2-clone";

            repo.ReplayAll();

            string path = TestHelper.GetCurrentMethodName() + ".dsproj";

            //feature source
            var features = new IFeature[]
                               {
                                   new Bridge {Name = "feature1", Geometry = new Point(100, 100)},
                                   new Bridge {Name = "feature2", Geometry = new Point(200, 200)}
                               };

            //feature coverage
            var featureCoverage = new FeatureCoverage("test");

            using (var store = new NetCdfFunctionStore())
            {
                store.CreateNew(TestHelper.GetCurrentMethodName() + ".nc");
                store.Functions.Add(featureCoverage);
                featureCoverage.Features = new EventedList<IFeature>(features);
                featureCoverage.Components.Add(new Variable<double>("value"));
                featureCoverage.Arguments.Add(new Variable<IFeature>("feature"));
                    // Pump BranchStructure Weir IStructure

                for (int i = 0; i < featureCoverage.Features.Count; i++)
                {
                    featureCoverage[featureCoverage.Features[i]] = Convert.ToDouble(i);
                }

                projectRepository.Create(path);

                //save
                var project = new Project();
                project.RootFolder.Add(new DataItem(featureCoverage, DataItemRole.Output));
                projectRepository.SaveOrUpdate(project);

                //reload
                var retrievedProject = projectRepository.Open(path);
                IDataItem[] retrievedDataItems = retrievedProject.RootFolder.DataItems.ToArray();
                var retrievedFeatureCoverage = (IFeatureCoverage) retrievedDataItems[0].Value;

                //clone
                var clonedFeatureCoverage = (IFeatureCoverage) retrievedFeatureCoverage.Clone();
                retrievedProject.RootFolder.Add(clonedFeatureCoverage);

                FeatureCoverage.RefreshAfterClone(
                    clonedFeatureCoverage,
                    retrievedFeatureCoverage.Features,
                    new[]
                        {
                            featureMock1,
                            featureMock2
                        });
                Assert.AreEqual(2, clonedFeatureCoverage.FeatureVariable.Values.Count);

                for (int i = 0; i < featureCoverage.Features.Count; i++)
                {
                    Assert.AreEqual(clonedFeatureCoverage[clonedFeatureCoverage.Features[i]], Convert.ToDouble(i));
                }
            }
        }

        [Test]
        public void SettingFeaturesAgainAfterSaveShouldThrowException()
        {
            IList<IBranchFeature> features = new List<IBranchFeature>
                                                 {
                                                     new Bridge {Name = "feature1", Geometry = new Point(100, 100)},
                                                     new Bridge {Name = "feature2", Geometry = new Point(200, 200)},
                                                 };

            var coverage = new FeatureCoverage();

            var store = new NetCdfFunctionStore();
            store.CreateNew(TestHelper.GetCurrentMethodName() + ".nc");
            store.Functions.Add(coverage);

            coverage.Components.Add(new Variable<double>("value"));
            coverage.Arguments.Add(new Variable<IFeature>("feature"));
            coverage.Features = new EventedList<IFeature>(features.Cast<IFeature>());
            coverage.FeatureVariable.SetValues(features);

            var feature = features[1];
            IList<double> values = coverage.GetValues<double>(new VariableValueFilter<IFeature>(coverage.FeatureVariable, feature));
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual(coverage.Components[0].DefaultValue, values[0]);

            IList<double> allValues = coverage.GetValues<double>();
            Assert.AreEqual(2, allValues.Count);

            double[] valuesArray = new [] { 1.0, 2.0};
            coverage.SetValues(valuesArray);

            int exceptions = 0;
            string expectedMessage = "Changing the feature list after setting and persisting spatial data values is not allowed!";

            //testing two paths:
            try
            {
                coverage.Features.Add(features[0]);
            }
            catch (NotSupportedException nse)
            {
                if (nse.Message.Equals(expectedMessage))
                {
                    exceptions++;
                }
            }

            try
            {
                coverage.Features = new EventedList<IFeature>(features.Cast<IFeature>());
            }
            catch (NotSupportedException nse)
            {
                if (nse.Message.Equals(expectedMessage))
                {
                    exceptions++;
                }
            }

            exceptions.Should("Expected two exceptions to be thrown").Be.EqualTo(2);
        }

        [Test]
        public void ModifyingInternalCollectionInFeaturesAfterSaveShouldNotThrowException()
        {
            var hydroNode = new HydroNode {Name = "feature1", Geometry = new Point(100, 100)};
            IList<IFeature> features = new List<IFeature> { hydroNode };

            var coverage = new FeatureCoverage();

            var store = new NetCdfFunctionStore();
            store.CreateNew(TestHelper.GetCurrentMethodName() + ".nc");
            store.Functions.Add(coverage);

            coverage.Components.Add(new Variable<double>("value"));
            coverage.Arguments.Add(new Variable<IFeature>("feature"));
            coverage.Features = new EventedList<IFeature>(features);
            coverage.FeatureVariable.SetValues(features);

            var feature = features[0];
            IList<double> values = coverage.GetValues<double>(new VariableValueFilter<IFeature>(coverage.FeatureVariable, feature));
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual(coverage.Components[0].DefaultValue, values[0]);

            IList<double> allValues = coverage.GetValues<double>();
            Assert.AreEqual(1, allValues.Count);

            double[] valuesArray = new[] { 1.0 };
            coverage.SetValues(valuesArray);

            hydroNode.Links.Add(new HydroLink(Substitute.For<IHydroObject>(), Substitute.For<IHydroObject>())); //triggers a bubbling collection changed
            
            //we should get here without exception
        }

        [Test]
        public void SettingFeaturesAgainAfterSaveAndClearShouldNotThrowException()
        {
            IList<IBranchFeature> features = new List<IBranchFeature>
                                                 {
                                                     new Bridge {Name = "feature1", Geometry = new Point(100, 100)},
                                                     new Bridge {Name = "feature2", Geometry = new Point(200, 200)},
                                                 };

            var coverage = new FeatureCoverage();

            var store = new NetCdfFunctionStore();
            store.CreateNew(TestHelper.GetCurrentMethodName() + ".nc");
            store.Functions.Add(coverage);

            coverage.Components.Add(new Variable<double>("value"));
            coverage.Arguments.Add(new Variable<IFeature>("feature"));
            coverage.Features = new EventedList<IFeature>(features.Cast<IFeature>());
            coverage.FeatureVariable.SetValues(features);

            var feature = features[1];
            IList<double> values = coverage.GetValues<double>(new VariableValueFilter<IFeature>(coverage.FeatureVariable, feature));
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual(coverage.Components[0].DefaultValue, values[0]);

            IList<double> allValues = coverage.GetValues<double>();
            Assert.AreEqual(2, allValues.Count);

            double[] valuesArray = new[] { 1.0, 2.0 };
            coverage.SetValues(valuesArray);

            coverage.Clear();
            var oneFeatureList = new []{feature};
            coverage.Features = new EventedList<IFeature>(oneFeatureList);
            coverage.FeatureVariable.SetValues(oneFeatureList);
            var expected = 3.0;
            coverage.SetValues(new[] {expected});

            Assert.AreEqual(oneFeatureList.Count(), coverage.Features.Count);
            Assert.AreEqual(expected, coverage.Components[0].Values[0]);
        }

        [Test]
        public void CloningSaveLoadedFeatureCoverageWithCustomFeatureTypeShouldWork()
        {
            var path = "clonefc.dsproj";

            IList<IBranchFeature> features = new List<IBranchFeature>
                                                 {
                                                     new Bridge {Name = "feature1", Geometry = new Point(100, 100)},
                                                     new Bridge {Name = "feature2", Geometry = new Point(200, 200)},
                                                 };

            var coverage = new FeatureCoverage();

            var store = new NetCdfFunctionStore();
            store.CreateNew(TestHelper.GetCurrentMethodName() + ".nc");
            store.Functions.Add(coverage);

            coverage.Components.Add(new Variable<double>("value"));
            coverage.Arguments.Add(new Variable<IBranchFeature>("feature"));
            coverage.Features = new EventedList<IFeature>(features.Cast<IFeature>());
            coverage.FeatureVariable.SetValues(features);

            projectRepository.Create(path);

            //save
            var project = new Project();
            project.RootFolder.Add(new DataItem(coverage, DataItemRole.Output));
            projectRepository.SaveOrUpdate(project);

            //reload
            var retrievedProject = projectRepository.Open(path);
            IDataItem[] retrievedDataItems = retrievedProject.RootFolder.DataItems.ToArray();
            var retrievedFeatureCoverage = (IFeatureCoverage)retrievedDataItems[0].Value;

            var clonedCoverage = (IFeatureCoverage)retrievedFeatureCoverage.Clone();
            Assert.AreEqual(coverage.FeatureVariable.ValueType, clonedCoverage.FeatureVariable.ValueType);
        }

        [Test]
        public void SaveAndRetrieveRegularGridCoverageWithNetCdf()
        {
            string path = TestHelper.GetCurrentMethodName() + ".dsproj";

            projectRepository.Create(path);
            
            var project = new Project();

            //setup a store with a coverage inside
            var store = new NetCdfFunctionStore();
            store.CreateNew(TestHelper.GetCurrentMethodName() + ".nc");
            
            //grid
            var component = new Variable<double>("pressure");
            IRegularGridCoverage regularGridCoverage = new RegularGridCoverage();
            store.Functions.Add(regularGridCoverage);
            regularGridCoverage.Resize(100,100,1,1);

            // set values
            project.RootFolder.Add(new DataItem(regularGridCoverage));
            projectRepository.SaveOrUpdate(project);

            //reload
            var retrievedProject = projectRepository.Open(path);
            IRegularGridCoverage retrievedCoverage = (IRegularGridCoverage)retrievedProject.RootFolder.DataItems.FirstOrDefault().Value;
            
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

            projectRepository.Create(path);
            
            var project = new Project();

            //setup a store with a coverage inside
            var store = new NetCdfFunctionStore();
            store.CreateNew(TestHelper.GetCurrentMethodName() + ".nc");

            //grid
            IRegularGridCoverage regularGridCoverage = new RegularGridCoverage();
            store.Functions.Add(regularGridCoverage);
            regularGridCoverage.Resize(100, 100, 1, 1);

            project.RootFolder.Add(regularGridCoverage);
            projectRepository.SaveOrUpdate(project);

            regularGridCoverage.Clear();
            regularGridCoverage.Components.Clear();
            regularGridCoverage.Components.Add(new Variable<double>("new_component"));
            projectRepository.SaveOrUpdate(project); // bang! exception, cascade

            //reload
            var retrievedProject = projectRepository.Open(path);
            IRegularGridCoverage retrievedCoverage = (IRegularGridCoverage)retrievedProject.RootFolder.DataItems.FirstOrDefault().Value;

            //compare
            Assert.IsTrue(retrievedCoverage.Store is NetCdfFunctionStore);
            Assert.AreEqual(regularGridCoverage.Components[0].Values.Count, retrievedCoverage.Components[0].Values.Count);
            Assert.AreEqual(regularGridCoverage.X.Values.Count, retrievedCoverage.X.Values.Count);
            Assert.AreEqual(regularGridCoverage.Y.Values.Count, retrievedCoverage.Y.Values.Count);

            store.Dispose();
        }

        [Test]
        public void SaveProjectNCToNetCdfNoValues()
        {
            using (IGui gui = CreateRunningGui())
            {
                IProjectService projectService = GetProjectServiceWithProject(gui);

                Project project = projectService.Project;

                // create test network
                NetworkCoverage networkCoverage = CreateNetworkCoverage(projectService);
                var dataItem = new DataItem(networkCoverage, DataItemRole.Output);
                project.RootFolder.Add(dataItem);

                // save project in test directory
                //don't use CurrentMethodName it is too long for the build server
                var projectPath = "1.dsproj";
                projectService.SaveProjectAs(projectPath);

                Project retrievedProject = projectService.OpenProject(projectPath);
                INetworkCoverage networkCoverageReOpend = (INetworkCoverage)retrievedProject.RootFolder.DataItems.First(di => di.ValueType == typeof(NetworkCoverage)).Value;

                // set values
                var branch1 = networkCoverage.Network.Branches[0];
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
                IProjectService projectService = GetProjectServiceWithProject(gui);

                Project project = projectService.Project;

                // add a coverage to the project.
                NetworkCoverage networkCoverage = CreateNetworkCoverage(projectService);
                AddProjectDataItem(project, networkCoverage);

                //save the project to a local dir
                var projectPath = TestHelper.GetCurrentMethodName() + ".dsproj";
                string dataPath = projectPath + "_data";
                if (Directory.Exists(dataPath))
                {
                    Directory.Delete(dataPath, true);
                }

                projectService.SaveProjectAs(projectPath);

                //define some values in the coverage forcing a write
                var branch1 = networkCoverage.Network.Branches[0];
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
                IProjectService projectService = GetProjectServiceWithProject(gui);

                Project project = projectService.Project;

                // save the project
                var projectPath = "ci1.dsproj";
                string dataPath = projectPath + "_data";
                projectService.SaveProjectAs(projectPath);
                
                // add a coverage to the project.
                NetworkCoverage networkCoverage = CreateNetworkCoverage(projectService);
                AddProjectDataItem(project, networkCoverage);

                // define some values in the coverage forcing a write
                var branch1 = networkCoverage.Network.Branches[0];
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
                IProjectService projectService = GetProjectServiceWithProject(gui);

                Project project = projectService.Project;

                // save the project
                var projectPath = "si2.dsproj";
                string dataPath = projectPath + "_data";
                projectService.SaveProjectAs(projectPath);

                // add a coverage to the project.
                NetworkCoverage networkCoverage = CreateNetworkCoverage(projectService);
                AddProjectDataItem(project, networkCoverage);

                // define some values in the coverage forcing a write
                var branch1 = networkCoverage.Network.Branches[0];
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
                IProjectService projectService = GetProjectServiceWithProject(gui);

                Project project = projectService.Project;

                // save the project
                var projectPath = "si1.dsproj";
                string dataPath = projectPath + "_data";
                
                // add a coverage to the project.
                NetworkCoverage networkCoverage = CreateNetworkCoverage(projectService);
                DataItem dataItem = AddProjectDataItem(project, networkCoverage);

                // define some values in the coverage forcing a write
                var branch1 = networkCoverage.Network.Branches[0];
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
                IProjectService projectService = GetProjectServiceWithProject(gui);

                Project project = projectService.Project;

                // save the project
                var projectPath = "ci2.dsproj";
                string dataPath = projectPath + "_data";

                // add a coverage to the project.
                NetworkCoverage networkCoverage = CreateNetworkCoverage(projectService);
                DataItem dataItem = AddProjectDataItem(project, networkCoverage);

                // define some values in the coverage forcing a write
                var branch1 = networkCoverage.Network.Branches[0];
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
                IProjectService projectService = GetProjectServiceWithProject(gui);

                Project project = projectService.Project;

                // save the project
                var projectPath = "ci3.dsproj";
                string dataPath = projectPath + "_data";

                // add a coverage to the project.
                NetworkCoverage networkCoverage = CreateNetworkCoverage(projectService);
                DataItem dataItem = AddProjectDataItem(project, networkCoverage);

                // define some values in the coverage forcing a write
                var branch1 = networkCoverage.Network.Branches[0];
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
                var numFiles = Directory.GetFiles(dataPath).Length;
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
                IProjectService projectService = GetProjectServiceWithProject(gui);

                Project project = projectService.Project;

                // save the project
                var projectPath = "ci3.dsproj";
                string dataPath = projectPath + "_data";

                // add a coverage to the project.
                NetworkCoverage networkCoverage = CreateNetworkCoverage(projectService);
                DataItem dataItem = AddProjectDataItem(project, networkCoverage);

                // define some values in the coverage forcing a write
                var branch1 = networkCoverage.Network.Branches[0];
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
                var fileNames = Directory.GetFiles(dataPath);
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
                IProjectService projectService = GetProjectServiceWithProject(gui);

                Project project = projectService.Project;

                // add a coverage to the project.
                NetworkCoverage networkCoverage = CreateNetworkCoverage(projectService);
                AddProjectDataItem(project, networkCoverage);

                //save the project to a local dir
                var projectPath = "changesfiles.dsproj";
                string dataPath = projectPath + "_data";
                if (Directory.Exists(dataPath))
                {
                    Directory.Delete(dataPath, true);
                }

                projectService.SaveProjectAs(projectPath);

                //assert file has been written
                Assert.AreEqual(0, Directory.GetFiles(dataPath,"*.nc.changes").Length);
                Assert.AreEqual(1, Directory.GetFiles(dataPath, "*.nc").Length);

                //define some values in the coverage forcing a write
                var branch1 = networkCoverage.Network.Branches[0];
                networkCoverage.Clear();
                networkCoverage[new NetworkLocation(branch1, 0.0)] = 0.1;

                //assert a changes file has been written
                Assert.AreEqual(1, Directory.GetFiles(dataPath,"*.nc.changes").Length);
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
                IProjectService projectService = GetProjectServiceWithProject(gui);

                Project project = projectService.Project;

                // add a coverage to the project.
                NetworkCoverage networkCoverage = CreateNetworkCoverage(projectService);
                AddProjectDataItem(project, networkCoverage);

                //save the project to a local dir
                var folder = "saveAsThriceWithChanges";
                FileUtils.DeleteIfExists(folder);

                var projectPath1 = folder + "\\changesfiles1.dsproj";
                var projectPath2 = folder + "\\changesfiles2.dsproj";
                var projectPath3 = folder + "\\changesfiles3.dsproj";

                projectService.SaveProjectAs(projectPath1);

                networkCoverage.Clear();
                var branch1 = networkCoverage.Network.Branches[0];
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
                IProjectService projectService = GetProjectServiceWithProject(gui);

                Project project = projectService.Project;
                // add a coverage to the project.
                NetworkCoverage networkCoverage = CreateNetworkCoverage(projectService);
                AddProjectDataItem(project, networkCoverage);

                //save the project to a local dir
                var folder = "saveAsTwice";
                FileUtils.DeleteIfExists(folder);

                var projectPath1 = folder + "\\changesfiles1.dsproj";
                var projectPath2 = folder + "\\changesfiles2.dsproj";

                projectService.SaveProjectAs(projectPath1);
                projectService.SaveProjectAs(projectPath2);

                //assert files has been written
                Assert.AreEqual(1, Directory.GetFiles(projectPath1 + "_data", "*.nc").Length);
                Assert.AreEqual(1, Directory.GetFiles(projectPath2 + "_data", "*.nc").Length);
            }
        }

        private static IGui CreateRunningGui()
        {
            IGui gui = new DHYDROGuiBuilder().Build();

            gui.Run();

            return gui;
        }
        
        private static NHibernateProjectRepository CreateProjectRepository()
        {
            return new DHYDRONHibernateProjectRepositoryBuilder().Build();
        }

        private static IProjectService GetProjectServiceWithProject(IGui gui)
        {
            IProjectService projectService = gui.Application.ProjectService;
            projectService.CreateProject();

            return projectService;
        }

        private static DataItem AddProjectDataItem(Project project, IFunction function)
        {
            var dataItem = new DataItem(function, DataItemRole.Output);
            project.RootFolder.Add(dataItem);

            return dataItem;
        }

        private static void AssignFunctionStore(IProjectService projectService, IFunction function)
        {
            string netCdfFilePath = Path.Combine(projectService.ProjectFilePath + "_data", "functions.nc");
            var store = NetCdfFunctionStore.CreateNewNetCdfFunctionStore(netCdfFilePath);
            
            function.Store = store;
            store.Functions.Add(function);
        }

        private static NetworkCoverage CreateNetworkCoverage(IProjectService projectService)
        {
            INetwork network = NHibernateTestsHelper.CreateDummyNetwork();
            var coverage = new NetworkCoverage { Network = network };
            
            AssignFunctionStore(projectService, coverage);

            return coverage;
        }
    }
}