using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core.Workflow;
using DelftTools.TestUtils;
using DelftTools.TestUtils.TestReferenceHelper;
using DelftTools.Utils.IO;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMap;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class FMHisFileFunctionStoreTest
    {
        [OneTimeSetUp]
        public void SetMapCoordinateSystemFactory()
        {
            if (Map.CoordinateSystemFactory == null)
                Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
        }

        [Test]
        public void OpenHisFileCheckFunctions()
        {
            var store = new FMHisFileFunctionStore(TestHelper.GetTestFilePath("output_hisfiles\\sfbay_his.nc"));
            Assert.AreEqual(10, store.Functions.Count);
        }

        [Test]
        public void OpenStationsWaterLevelTimeSeries()
        {
            var store = new FMHisFileFunctionStore(TestHelper.GetTestFilePath("output_hisfiles\\sfbay_his.nc"));

            var waterLevelFunction = (FeatureCoverage)store.Functions.FirstOrDefault(f => f.Components[0].Name == "waterlevel");

            Assert.IsNotNull(waterLevelFunction);
            Assert.AreEqual(37248, waterLevelFunction.GetValues().Count);
            Assert.AreEqual(388, waterLevelFunction.Time.Values.Count);
            Assert.AreEqual(96, waterLevelFunction.Arguments[1].Values.Count);
            Assert.AreEqual(new DateTime(1999, 12, 16), waterLevelFunction.Time.Values.First());
            Assert.AreEqual("(POR)", waterLevelFunction.Arguments[1].Values.OfType<Feature2D>().First().Name);
            Assert.AreEqual("(POR)", waterLevelFunction.Features.OfType<Feature2D>().First().Name);
            Assert.AreEqual(1.5, (double)waterLevelFunction.Components[0].Values[0], 0.001);
            Assert.AreEqual("m", waterLevelFunction.Components[0].Unit.Symbol);

        }

        [Test]
        public void OpenStationsWaterLevelTimeSeriesCheckWithTimeFilter()
        {
            var store = new FMHisFileFunctionStore(TestHelper.GetTestFilePath("output_hisfiles\\sfbay_his.nc"));
            var waterLevelFunction = (FeatureCoverage) store.Functions.FirstOrDefault(f => f.Components[0].Name == "waterlevel");

            var timeFiltered = (IFeatureCoverage) waterLevelFunction.FilterTime(waterLevelFunction.Time.Values.First());
            Assert.AreEqual(96, timeFiltered.FeatureVariable.Values.Cast<IFeature>().ToArray().Length);
        }

        [Test]
        public void ShowWaterBalanceTimeSeries()
        {
            var store = new FMHisFileFunctionStore(TestHelper.GetTestFilePath("output_hisfiles\\har_1d2d_his.nc"));
            IFunction waterbalancetimeseries =
                store.Functions.First(f => f.Components[0].Name == "WaterBalance_total_volume");

            double[] expectedSeries = new[]
            {
                0.0, 216117380.39221892, 213569886.88264033, 211512224.48981249, 209755740.84053218, 208179872.92569879,
                206707387.47398412, 205320172.47705263, 204001444.64842579, 202735815.96192247, 201511154.33547282
            };

            CollectionAssert.AreEquivalent(expectedSeries, waterbalancetimeseries.GetValues<double>().ToArray());
        }

        [Test]
        public void OpenStationsWaterLevelTimeSeriesCheckWithStationFilter()
        {
            var store = new FMHisFileFunctionStore(TestHelper.GetTestFilePath("output_hisfiles\\sfbay_his.nc"));
            var waterLevelFunction = (FeatureCoverage)store.Functions.FirstOrDefault(f => f.Components[0].Name == "waterlevel");

            var timeSeriesForPoint = waterLevelFunction.GetTimeSeries(waterLevelFunction.Features.Skip(1).First());
            Assert.AreEqual(388, timeSeriesForPoint.GetValues().Count);
            Assert.AreEqual(0.1957, (double) timeSeriesForPoint.GetValues()[50], 0.001);
        }
        
        [Test]
        public void OpenCrossSectionDischargeTimeSeries()
        {
            var store = new FMHisFileFunctionStore(TestHelper.GetTestFilePath("output_hisfiles\\sfbay_his.nc"));

            var dischargeFunction = (FeatureCoverage)store.Functions.FirstOrDefault(f => f.Components[0].Name == "cross_section_discharge");

            Assert.IsNotNull(dischargeFunction);
            Assert.AreEqual(16296, dischargeFunction.GetValues().Count);
            Assert.AreEqual(388, dischargeFunction.Time.Values.Count);
            Assert.AreEqual(new DateTime(1999, 12, 16), dischargeFunction.Time.Values.First());
            Assert.AreEqual("L1", dischargeFunction.Arguments[1].Values.OfType<Feature2D>().First().Name);
            Assert.AreEqual("L1", dischargeFunction.Features.OfType<Feature2D>().First().Name);
            Assert.AreEqual(0.0d, (double) dischargeFunction.Components[0].Values[0], 0.001);
            Assert.AreEqual("m^3/s", dischargeFunction.Components[0].Unit.Symbol);
            Assert.AreEqual(
                new LineString(new []
                    {
                        new Coordinate(544991.375, 4186662.5),
                        new Coordinate(546229.875, 4184738.25)
                    }),
                dischargeFunction.Features.OfType<Feature2D>().First().Geometry);
        }


        [Test]
        public void OpenGeneralStructureTimeSeries()
        {
            var store = new FMHisFileFunctionStore(TestHelper.GetTestFilePath("output_hisfiles\\generalStructure_his.nc"));

            /* We use any of the components of general structure, just to check it has been created. */
            var generalStructureFunction = (FeatureCoverage)store.Functions.FirstOrDefault(f => f.Components[0].Name == "general_structure_discharge");

            Assert.IsNotNull(generalStructureFunction);
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void OpenHisFileInModelContextAndExpectFeaturesToBeSameInstance()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            var localMduFilePath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel(localMduFilePath);

            ActivityRunner.RunActivity(model);

            var waterLevelFunction = (FeatureCoverage)model.OutputHisFileStore.Functions.FirstOrDefault(f => f.Components[0].Name == "waterlevel");
            Assert.IsNotNull(waterLevelFunction);
            Assert.AreSame(model.Area.ObservationPoints.First(),
                           waterLevelFunction.Arguments[1].Values.OfType<IFeature>().First(), "feature waterlevel1");
            Assert.AreSame(model.Area.ObservationPoints.First(),
                           waterLevelFunction.Features.First(), "feature waterlevel2");

            var dischargeFunction = (FeatureCoverage)model.OutputHisFileStore.Functions.FirstOrDefault(f => f.Components[0].Name == "cross_section_discharge");
            Assert.IsNotNull(dischargeFunction);
            Assert.AreSame(model.Area.ObservationCrossSections.First(),
                           dischargeFunction.Arguments[1].Values.OfType<IFeature>().First(),"feat discharge1");
            Assert.AreSame(model.Area.ObservationCrossSections.First(),
                           dischargeFunction.Features.First(), "feat discharge2");
        }


        [Test]
        public void OpenLeveeBreachTimeSeries()
        {
            var store = new FMHisFileFunctionStore(TestHelper.GetTestFilePath("output_hisfiles\\leveeBreach_his.nc"));

            /*
             waar staat dit!!! file moet corrupt zijn, coordinates attributes is niet gezet
            var result = (FeatureCoverage)store.Functions.FirstOrDefault(f => f.Components[0].Name == "dambreak_discharge");
            Assert.IsNotNull(result, "dambreak_discharge");

            result = (FeatureCoverage)store.Functions.FirstOrDefault(f => f.Components[0].Name == "dambreak_cumulative_discharge");
            Assert.IsNotNull(result, "dambreak_cumulative_discharge");
            */
            var result = (FeatureCoverage)store.Functions.FirstOrDefault(f => f.Components[0].Name == "dambreak_s1up");
            Assert.IsNotNull(result, "dambreak_s1up");

            result = (FeatureCoverage)store.Functions.FirstOrDefault(f => f.Components[0].Name == "dambreak_s1dn");
            Assert.IsNotNull(result, "dambreak_s1dn");

            result = (FeatureCoverage)store.Functions.FirstOrDefault(f => f.Components[0].Name == "dambreak_breach_depth");
            Assert.IsNotNull(result, "dambreak_breach_depth");

            result = (FeatureCoverage)store.Functions.FirstOrDefault(f => f.Components[0].Name == "dambreak_breach_width");
            Assert.IsNotNull(result, "dambreak_breach_width");
        }

        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void RunModelDeleteObservationPointsRunAgain()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            var localMduFilePath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel(localMduFilePath);

            ActivityRunner.RunActivity(model);

            var waterLevelFunction = (FeatureCoverage)model.OutputHisFileStore.Functions.FirstOrDefault(f => f.Components[0].Name == "waterlevel");
            Assert.IsNotNull(waterLevelFunction);
            Assert.AreSame(model.Area.ObservationPoints.First(),
                           waterLevelFunction.Arguments[1].Values.OfType<IFeature>().First(), "feature waterlevel1");
            Assert.AreSame(model.Area.ObservationPoints.First(),
                           waterLevelFunction.Features.First(), "feature waterlevel2");

            for (int i = 0; i < 100; ++i)
            {
                model.Area.ObservationPoints.RemoveAt(0);
            }

            ActivityRunner.RunActivity(model);

            waterLevelFunction =
                (FeatureCoverage)
                    model.OutputHisFileStore.Functions.FirstOrDefault(f => f.Components[0].Name == "waterlevel");
            Assert.IsNotNull(waterLevelFunction);
            Assert.AreSame(model.Area.ObservationPoints.First(),
                           waterLevelFunction.FeatureVariable.Values.OfType<IFeature>().First(), "feature waterlevel1");
            Assert.AreSame(model.Area.ObservationPoints.First(),waterLevelFunction.Features.First());
        }


        [Test]
        [Category(TestCategory.Slow)]
        public void OpenHisFile()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen\har.mdu");
            var localMduFilePath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel(localMduFilePath);

            ActivityRunner.RunActivity(model);

            var observationPoint = model.Area.ObservationPoints[0];

            var numEventsBefore = TestReferenceHelper.FindEventSubscriptions(observationPoint, true);

            var waterLevelFunction = model.OutputHisFileStore.Functions.FirstOrDefault(f => f.Components[0].Name == "waterlevel") as FeatureCoverage;
            Assert.IsNotNull(waterLevelFunction);

            for (var i = 0; i < 5; ++i)
            {
                var timeSeries = waterLevelFunction.Arguments[1].GetValues<IFeature>().ToList();
            }

            var numEventsAfter = TestReferenceHelper.FindEventSubscriptions(observationPoint, true);

            Assert.IsTrue(numEventsAfter <= numEventsBefore + 2);
        }
        
        [Test]
        public void OpenHisFileCheckFunctions_Grouped() // Issue #: FM1D2D-1825
        {
            string testDataFilePath = TestHelper.GetTestFilePath(@"output_hisfiles");
            const string hisFileName = "FM_model_his.nc";

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                string hisFilePath = Path.Combine(tempDir, hisFileName); 
                
                FileUtils.CopyFile(Path.Combine(testDataFilePath, hisFileName), hisFilePath);

                var store = new FMHisFileFunctionStore(hisFilePath);
                // currently 119 output funct are created, but the time based function should be grouped in 10 groups (9 defined + 1 other/default) and 0 single groupings
                // netCDF groups
                // cross_section, weirgens, orifice, culvert, pumps, compoundStructures, bridge, source_sink, lateral, stations
                // friendly named :
                // Observation cross sections, Weirs + general structures, Orifices, Culverts, Pumps, Compound structures, Bridges, Source and sinks, Laterals, Observation points
                
                Assert.AreEqual(119, store.Functions.Count);

                List<IGrouping<string, IFunction>> groupings = store.GetFunctionGrouping().ToList();
                Assert.AreEqual(10, groupings.Count);
                List<string> groupingsNames = groupings.Select(g => g.Key).OrderBy(n => n).ToList();
                
                Assert.That(groupingsNames, Does.Contain(UserFriendlyNames.Pumps));
                Assert.That(groupingsNames, Does.Contain(UserFriendlyNames.ObservationCrossSections));
                Assert.That(groupingsNames, Does.Contain(UserFriendlyNames.WeirsAndGeneralStructures));
                Assert.That(groupingsNames, Does.Contain(UserFriendlyNames.Orifices));
                Assert.That(groupingsNames, Does.Contain(UserFriendlyNames.Culverts));
                Assert.That(groupingsNames, Does.Contain(UserFriendlyNames.CompoundStructures));
                Assert.That(groupingsNames, Does.Contain(UserFriendlyNames.Bridges));
                Assert.That(groupingsNames, Does.Contain(UserFriendlyNames.SourcesAndSinks));
                Assert.That(groupingsNames, Does.Contain(UserFriendlyNames.Laterals));
                Assert.That(groupingsNames, Does.Contain(UserFriendlyNames.Default));

                int numberOfSingleGroupings = groupings.Count(g => g.Count() == 1);
                Assert.AreEqual(0, numberOfSingleGroupings);

                int numberOfMultipleGroupings = groupings.Count(g => g.Count() > 1);
                Assert.AreEqual(10, numberOfMultipleGroupings);
            });
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCase("dimr_2_26_25_his.nc")]
        [TestCase("dimr_2_27_13_his.nc")]
        public void OpeningHisFilesOfDifferentDimrVersionsKeepTheSameHisOutputGroupsForBackwardsCompatability(string hisFileName) // Issue #: FM1D2D-2950
        {
            string testDataFilePath = TestHelper.GetTestFilePath(@"output_hisfiles");
            const int expectedAmountOfHisOutputGroups = 5;

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                // Arrange
                string hisFilePath = Path.Combine(tempDir, hisFileName); 
                FileUtils.CopyFile(Path.Combine(testDataFilePath, hisFileName), hisFilePath);

                // Act
                var store = new FMHisFileFunctionStore(hisFilePath);
                List<string> groupingsNames = GetStructureOutputGroupingsNames(store);
                
                // Assert
                Assert.That(groupingsNames.Count, Is.EqualTo(expectedAmountOfHisOutputGroups));
                Assert.That(groupingsNames.Contains(UserFriendlyNames.Pumps));
                Assert.That(groupingsNames.Contains(UserFriendlyNames.CompoundStructures));
                Assert.That(groupingsNames.Contains(UserFriendlyNames.Laterals));
                Assert.That(groupingsNames.Contains(UserFriendlyNames.Orifices));
                Assert.That(groupingsNames.Contains(UserFriendlyNames.WeirsAndGeneralStructures));
            });
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenBasicFmModelWithStructures_AfterRunning_ExpectedThatStructuresAreAvailableInOutputStore()
        {
            const int expectedAmountOfHisOutputGroups = 5;
            
            using (var fmModel = new WaterFlowFMModel())
            {
                // Arrange
                Channel channel = CreateBranchInNetwork(fmModel.Network);
                AddCrossSectionToBranch(channel);
                GenerateGridPoints(fmModel, channel);
                AddStructuresToBranch(channel);

                ValidationReport validationReport = fmModel.Validate();
                Assert.That(validationReport.AllErrors.Count(), Is.EqualTo(0), "Validation report contains errors, none were expected.");
                
                // Act
                ActivityRunner.RunActivity(fmModel);
                List<string> groupingsNames = GetStructureOutputGroupingsNames(fmModel.OutputHisFileStore);

                // Assert
                Assert.That(groupingsNames.Count, Is.EqualTo(expectedAmountOfHisOutputGroups));
                Assert.That(groupingsNames.Contains(UserFriendlyNames.Pumps));
                Assert.That(groupingsNames.Contains(UserFriendlyNames.CompoundStructures));
                Assert.That(groupingsNames.Contains(UserFriendlyNames.Laterals));
                Assert.That(groupingsNames.Contains(UserFriendlyNames.Orifices));
                Assert.That(groupingsNames.Contains(UserFriendlyNames.WeirsAndGeneralStructures));
            }
        }

        private static List<string> GetStructureOutputGroupingsNames(FMHisFileFunctionStore store)
        {
            return store.GetFunctionGrouping()
                        .Select(g => g.Key)
                        .OrderBy(name => name).ToList();
        }

        /// <summary>
        /// The structures created will be:
        /// <list type="bullet">
        /// <item><description>2 pumps in a composite structure</description></item>
        /// <item><description>1 Weir</description></item>
        /// <item><description>1 Orifice</description></item>
        /// <item><description>1 Lateral</description></item>
        /// </list>
        /// </summary>
        /// <param name="channel">Channel the structures will be added to.</param>
        private static void AddStructuresToBranch(Channel channel)
        {
            CompositeBranchStructure compoundStructure = new CompositeBranchStructure() { Chainage = 10 };
            var pump = new Pump()
            {
                Name = "pump1",
                ParentStructure = compoundStructure
            };
            var pump2 = new Pump()
            {
                Name = "pump2",
                ParentStructure = compoundStructure
            };
            compoundStructure.Structures.Add(pump);
            compoundStructure.Structures.Add(pump2);
                
            var weir = new Weir()
            {
                Chainage = 40
            };
            var orifice = new Orifice()
            {
                Chainage = 70
            };
            var lateral = new LateralSource();
                
            channel.BranchFeatures.Add(pump);
            channel.BranchFeatures.Add(pump2);
            channel.BranchFeatures.Add(compoundStructure);
            channel.BranchFeatures.Add(weir);
            channel.BranchFeatures.Add(orifice);
            channel.BranchFeatures.Add(lateral);
        }

        private static void GenerateGridPoints(WaterFlowFMModel fmModel, Channel channel)
        {
            var offsets = new double[] { 0, 30, 60, 100 };
            HydroNetworkHelper.GenerateDiscretization(fmModel.NetworkDiscretization, channel, offsets);
        }

        private static void AddCrossSectionToBranch(Channel channel)
        {
            var crossSection = CrossSection.CreateDefault();
            crossSection.Name = "Marlon";
            channel.BranchFeatures.Add(crossSection);
        }

        private static Channel CreateBranchInNetwork(IHydroNetwork network)
        {
            var node1 = new HydroNode
            {
                Name = "Node1",
                Network = network,
                Geometry = new Point(0.0, 0.0)
            };
            var node2 = new HydroNode
            {
                Name = "Node2",
                Network = network,
                Geometry = new Point(100.0, 0.0)
            };
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var channel = new Channel("branch1", node1, node2)
            {
                Geometry = new LineString(new[] { node1.Geometry.Coordinate, node2.Geometry.Coordinate }),
            };
            network.Branches.Add(channel);
            return channel;
        }
    }
}