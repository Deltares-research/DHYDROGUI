using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.ModelExchange.Queries;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using Deltares.IO.FewsPI;
using DeltaShell.Core;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.TestUtils;
using DeltaShell.Plugins.Fews.Tests.Queries;
using DeltaShell.Plugins.ImportExport.Sobek;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.Scripting;
using DeltaShell.Plugins.SharpMapGis;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using log4net.Core;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;
using SharpMap.Converters.WellKnownText;
using XmlUnit;
using TimeSeries = DelftTools.Functions.TimeSeries;
using Point = NetTopologySuite.Geometries.Point;

namespace DeltaShell.Plugins.Fews.Tests
{
    public class FewsAdapterTestBase
    {
        static FewsAdapterTestBase()
        {
            LogHelper.ConfigureLogging();
            LogHelper.SetLoggingLevel(Level.Fatal);

            TestHelper.SetDeltaresLicenseToEnvironmentVariable();
        }

        internal Discretization NetworkDiscretization;

        internal static DeltaShellApplication GetRunningDSApplication()
        {
            // make sure log4net is initialized
            var app = new DeltaShellApplication
                          {
                              IsProjectCreatedInTemporaryDirectory = false,
                              IsDataAccessSynchronizationDisabled = true,
                              ScriptRunner = {SkipDefaultLibraries = true}
                          };

            app.Plugins.Add(new ScriptingApplicationPlugin());
            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
            app.Plugins.Add(new NetCdfApplicationPlugin());
            app.Plugins.Add(new HydroModelApplicationPlugin());
            app.Plugins.Add(new RealTimeControlApplicationPlugin());
            app.Plugins.Add(new SobekImportApplicationPlugin());

            app.Run();
            //app.ProjectRepositoryFactory.SpeedUpConfigurationCreationUsingCaching = true;
            //app.ProjectRepositoryFactory.ConfigurationCacheDirectory = app.GetUserSettingsDirectoryPath();

            return app;
        }

        internal static void CheckWorkingDirectoryForTestRuns(object testClass)
        {
            
            string testRunDirectory = Path.GetDirectoryName(testClass.GetType().Assembly.Location);
            if (!String.IsNullOrEmpty(testRunDirectory) && !testRunDirectory.Contains(Path.GetTempPath()))
            {
                // DLL is not built in a temp dir created by resharper, so the test is running
                // on the build server. Check if this is in the right directory
                string currentWorkingDir = Environment.CurrentDirectory;
                Assert.AreEqual(testRunDirectory, currentWorkingDir, "Current directory is not equal to the test assembly directory");
            }
        }

        internal static void MakePathsInFewsAdapterConfigFilesAbsolute(string dir, string testRunDirPath)
        {
            foreach (string subDir in Directory.GetDirectories(dir))
            {
                MakePathsInFewsAdapterConfigFilesAbsolute(subDir, testRunDirPath);
            }
            foreach (string xmlFile in Directory.GetFiles(dir, "*.xml"))
            {
                string[] linesInXmlFile = File.ReadAllLines(xmlFile);
                string adjustedContent = linesInXmlFile.Aggregate("", (current, t) => current + t.Replace(TestDirResolver.TestDirPlaceHolder, testRunDirPath) + "\n");
                File.WriteAllText(xmlFile, adjustedContent);
            }
        }

        /// <summary>
        /// to avoid changes in testdata
        /// </summary>
        /// <param name="sourceDirName"></param>
        /// <param name="testRunDir"></param>
        /// <returns></returns>
        internal static string CopySourceTestDataIntoTestFolder(string sourceDirName, string testRunDir)
        {
            FileUtils.DeleteIfExists(testRunDir);
            Directory.CreateDirectory(testRunDir);
            string sourcePath = Path.Combine(TestHelper.GetTestDataDirectory(), sourceDirName);
            string targetPath = Path.Combine(testRunDir, sourceDirName);
            FileUtils.CopyDirectory(sourcePath, targetPath, ".svn");
            return targetPath;
        }

        internal void TryCreateDirectory(string name)
        {
            try
            {
                if (Directory.Exists(name))
                    Directory.Delete(name);

                Directory.CreateDirectory(name);
            }
            catch (Exception)
            {
                // do nothing                
            }
        }

        internal void AssertThatFileExists(string file)
        {
            Assert.IsNotEmpty(file);
            Assert.IsTrue(File.Exists(file), "The expected file " + file + " does not exists");
        }

        internal void AssertXmlStringsAreEqual(string actual, string expected)
        {
            AssertExpectedResult(actual, expected, true);
        }

        private static void AssertExpectedResult(string actual, string expected, bool areSame)
        {
            TextReader reader1 = new StringReader(actual);
            TextReader reader2 = new StringReader(expected);
            DiffResult result = PerformDiff(reader1, reader2);
            string msg = string.Format("comparing {0} to {1}: {2}", actual, expected, result.Difference);
            Assert.AreEqual(areSame, result.Equal, msg);
        }

        private static DiffResult PerformDiff(TextReader reader1, TextReader reader2)
        {
            var xmlDiff = new XmlDiff(reader1, reader2);
            DiffResult result = xmlDiff.Compare();
            return result;
        }


        internal Project CreateProjectWithModel(IModel model)
        {
            var project = new Project("test project");
            project.RootFolder.Add(model);
            return project;
        }

        internal static void PrintResults(IEnumerable<AggregationResult> queryResults)
        {
            foreach (var queryResult in queryResults)
            {
                Console.WriteLine(queryResult);
            }
        }

        internal Network CreateNetwork()
        {
            var node1 = new Node {Geometry = new Point(0, 0)};
            var node2 = new Node {Geometry = new Point(100, 0)};
            var node3 = new Node {Geometry = new Point(200, 0)};

            var branch1 = new Branch
            {
                Name = "Branch1",
                Geometry = GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)"),
                Source = node1,
                Target = node2
            };
            
            var branch2 = new Branch
            {
                Name = "Branch2",
                Geometry = GeometryFromWKT.Parse("LINESTRING (0 0, 200 0)"),
                Source = node2,
                Target = node3
            };

            var network = new Network();
            network.Branches.Add(branch1);
            network.Branches.Add(branch2);
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            network.Nodes.Add(node3);
            return network;
        }

        internal WaterFlowModel1D CreateDemoModel()
        {
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            return model;
        }

        internal WaterFlowModel1D CreateDemoNetworkWithLateralSources()
        {
            var model = WaterFlowModel1DDemoModelTestHelper.CreateModelWithDemoNetwork();
            model.UseSalt = true;
            model.UseSaltInCalculation = true;
            model.OutputSettings.GetEngineParameter(QuantityType.Salinity, ElementSet.GridpointsOnBranches).
            AggregationOptions = AggregationOptions.Current;

            const string locationIdLateral = "lateral test";
            AddLateralAndGetModelData(model.Network, locationIdLateral);

            return model;
        }

        internal NetworkCoverage CreateNetworkCoverage(string locationId, string parameterId)
        {
            var network = new HydroNetwork();
            var node1 = new Node("node1");
            var node2 = new Node("node2");

            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var branch1 = new Branch("branch1", node1, node2, 100.0)
            {
                Geometry = GeometryFromWKT.Parse("LINESTRING (0 0, 100 0)")
            };
            network.Branches.Add(branch1);

            var networkCoverage = new NetworkCoverage { Network = network, Name = parameterId, IsTimeDependent = true };

            // set values
            var time0 = new DateTime(2000, 1, 1);
            networkCoverage[time0, new NetworkLocation(branch1, 0.0) { Name = locationId }] = 0.1;
            networkCoverage[time0, new NetworkLocation(branch1, 100.0) { Name = "target2" }] = 0.2;

            NetworkDiscretization = new Discretization
            {
                Name = "RekenGrid",
                Network = network,
                SegmentGenerationMethod =
                    SegmentGenerationMethod.SegmentBetweenLocationsFullyCovered
            };

            return networkCoverage;
        }

        internal FeatureCoverage CreateFeaturCoverage(string locationId, string parameterId, bool timeDependent)
        {
            var features = new EventedList<IFeature>
                           {
                               new MockFeature {Name = locationId, Geometry = new Point(0, 0)},
                               new MockFeature {Name = "feature2", Geometry = new Point(1, 1)},
                               new MockFeature {Name = "feature3", Geometry = new Point(2, 2)},
                           };

            var featureCoverage = new FeatureCoverage { Name = parameterId, Features = features, IsTimeDependent = timeDependent };
            if (timeDependent)
                featureCoverage.Arguments.Add(new Variable<DateTime>("time"));

            featureCoverage.Arguments.Add(new Variable<MockFeature>("feature"));
            featureCoverage.Components.Add(new Variable<double>("value"));
            featureCoverage.Components[0].Attributes[FunctionAttributes.StandardName] = parameterId;

            return featureCoverage;
        }

        private static void AddLateralAndGetModelData(IHydroNetwork network, string id)
        {
            DateTime now = DateTime.Now;
            var t = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
            var lateralSource = new LateralSource();
            var waterFlowModel1DLateralSourceData = new WaterFlowModel1DLateralSourceData
            {
                Feature = lateralSource,
                DataType = WaterFlowModel1DLateralDataType.FlowTimeSeries
            };
            lateralSource.Name = id;

            waterFlowModel1DLateralSourceData.Data[t] = 1.0;
            waterFlowModel1DLateralSourceData.Data[t.AddSeconds(30)] = 1.0;
            waterFlowModel1DLateralSourceData.Data[t.AddSeconds(60)] = 1.5;
            waterFlowModel1DLateralSourceData.Data[t.AddSeconds(120)] = 1.0;
            waterFlowModel1DLateralSourceData.Data[t.AddSeconds(180)] = 0.5;
            waterFlowModel1DLateralSourceData.Data.Arguments[0].ExtrapolationType = ExtrapolationType.Constant;
            waterFlowModel1DLateralSourceData.UseSalt = true;
            waterFlowModel1DLateralSourceData.SaltLateralDischargeType = SaltLateralDischargeType.MassTimeSeries;
            waterFlowModel1DLateralSourceData.SaltMassTimeSeries[t.AddSeconds(30)] = 5.0;
            waterFlowModel1DLateralSourceData.SaltMassTimeSeries[t.AddSeconds(60)] = 1.5;
            waterFlowModel1DLateralSourceData.SaltMassTimeSeries[t.AddSeconds(120)] = 7.0;
            waterFlowModel1DLateralSourceData.SaltMassTimeSeries[t.AddSeconds(180)] = 4.5;
            waterFlowModel1DLateralSourceData.SaltMassDischargeConstant = 2;
            waterFlowModel1DLateralSourceData.SaltConcentrationTimeSeries[new DateTime(2000, 1, 1)] = 3.0;
            waterFlowModel1DLateralSourceData.SaltConcentrationDischargeConstant = 4;
            NetworkHelper.AddBranchFeatureToBranch(lateralSource, network.Branches[0], 15);
        }

        internal static TimeSeries CreateTimeSeries(bool isDouble)
        {
            DateTime now = DateTime.Now;
            var t = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
            var timeSeries = new TimeSeries();
            if (isDouble)
            {
                timeSeries.Components.Add(new Variable<double> { DefaultValue = -9999 });
                timeSeries[t] = 22.2;
                timeSeries[t.AddSeconds(30)] = 1.2;
                timeSeries[t.AddSeconds(60)] = 3.3;
                timeSeries[t.AddSeconds(120)] = 42.0;
                timeSeries[t.AddSeconds(180)] = 13.1;
                return timeSeries;
            }
            timeSeries.Components.Add(new Variable<bool> { DefaultValue = false });
            timeSeries[t] = true;
            timeSeries[t.AddSeconds(30)] = false;
            timeSeries[t.AddSeconds(60)] = true;
            timeSeries[t.AddSeconds(120)] = false;
            timeSeries[t.AddSeconds(180)] = true;
            return timeSeries;
        }
    }
}