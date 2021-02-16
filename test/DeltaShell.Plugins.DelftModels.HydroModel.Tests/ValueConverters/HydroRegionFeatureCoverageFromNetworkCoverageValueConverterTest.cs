using System;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.DelftModels.HydroModel.ValueConverters;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.IO;
using NUnit.Framework;
using SharpTestsEx;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.ValueConverters
{
    [TestFixture]
    public class HydroRegionFeatureCoverageFromNetworkCoverageValueConverterTest
    {
        private static readonly WKTReader WktReader = new WKTReader();

        private HydroRegion region;
        private NetworkCoverage networkCoverage;
        private FeatureCoverage featureCoverage;
        private readonly DateTime time0 = new DateTime(2000, 1, 1);
        private readonly DateTime time1 = new DateTime(2000, 1, 2);

        [SetUp]
        public void SetUp()
        {
            // setup region
            var node1 = new HydroNode {Name = "node1", Geometry = WktReader.Read("POINT(10 0)")};
            var node2 = new HydroNode {Name = "node2", Geometry = WktReader.Read("POINT(0 0)")};
            var branch1 = new Channel
                {Name = "branch1", Source = node1, Target = node2, Geometry = WktReader.Read("LINESTRING(0 0, 10 0)")};
            var lateralSource1 = new LateralSource() { Name = "lateral1", Branch = branch1, Chainage = 5.0, Geometry = WktReader.Read("POINT(5 0)") }; ;
            var lateralSource2 = new LateralSource() { Name = "lateral2", Branch = branch1, Chainage = 7.0, Geometry = WktReader.Read("POINT(7 0)") }; ;
            var network = new HydroNetwork {Branches = {branch1}, Nodes = {node1, node2}};

            var c1 = new Catchment();
            var c2 = new Catchment();
            var basin = new DrainageBasin {Catchments = {c1, c2}};

            region = new HydroRegion {SubRegions = {network, basin}};

            c1.LinkTo(lateralSource1);
            c2.LinkTo(lateralSource2);

            // setup coverages
            networkCoverage = new NetworkCoverage("h on laterals", true) {Network = network};
            networkCoverage[time0, new NetworkLocation(branch1, 5)] = 1.0;
            networkCoverage[time1, new NetworkLocation(branch1, 5)] = 2.0;
            networkCoverage[time0, new NetworkLocation(branch1, 7)] = 3.0;
            networkCoverage[time1, new NetworkLocation(branch1, 7)] = 4.0;

            featureCoverage = new FeatureCoverage("h on catchments");
            featureCoverage.Arguments.Add(new Variable<DateTime>());
            featureCoverage.Arguments.Add(new Variable<IFeature>());
            featureCoverage.Components.Add(new Variable<double>("h"));
            featureCoverage.Features.AddRange(new[] {c1, c2});
            featureCoverage.FeatureVariable.Values.AddRange(new[] {c1, c2});
        }

        [Test]
        public void FeatureCoverageIsInitializedCorrectly()
        {
            var convertor = new HydroRegionFeatureCoverageFromNetworkCoverageValueConverter
                {
                    OriginalValue = featureCoverage,
                    ConvertedValue = networkCoverage,
                    HydroRegion = region
                };

            var c1 = featureCoverage.Features.ElementAt(0);
            var c2 = featureCoverage.Features.ElementAt(1);

            convertor.Update(time0);
            // assert
            featureCoverage[time0, c1].Should().Be.EqualTo(1.0);
            featureCoverage[time0, c2].Should().Be.EqualTo(3.0);

            convertor.Update(time1);
            featureCoverage[time1, c1].Should().Be.EqualTo(2.0);
            featureCoverage[time1, c2].Should().Be.EqualTo(4.0);
        }

        [Test]
        public void FeatureCoverageIsUpdatedCorrectly()
        {
            var convertor = new HydroRegionFeatureCoverageFromNetworkCoverageValueConverter
                {
                    OriginalValue = featureCoverage,
                    ConvertedValue = networkCoverage,
                    HydroRegion = region
                };

            // modify network coverage
            var time2 = new DateTime(2000, 1, 3);
            networkCoverage.AddValuesForTime(new[] {5.0, 6.0}, time2);

            var c1 = featureCoverage.Features.ElementAt(0);
            var c2 = featureCoverage.Features.ElementAt(1);

            convertor.Update(time2);

            // assert
            featureCoverage[time2, c1].Should().Be.EqualTo(5.0);
            featureCoverage[time2, c2].Should().Be.EqualTo(6.0);
        }

        [Test]
        public void FeatureCoverageIsUpdatedCorrectlyOnClearOfNetworkCoverage()
        {
            new HydroRegionFeatureCoverageFromNetworkCoverageValueConverter
                {
                    OriginalValue = featureCoverage,
                    ConvertedValue = networkCoverage,
                    HydroRegion = region
                };

            // clear network coverage
            networkCoverage.Clear();
            
            // assert
            Assert.AreEqual(0, featureCoverage.Time.Values.Count);
        }

        [Test]
        public void FeatureCoverageUpdateWithoutLinksDoesNotCrash()
        {
            // Remove links:
            region.SubRegions.OfType<IDrainageBasin>()
                  .First()
                  .AllCatchments.ForEach(c => c.UnlinkFrom(c.Links.First().Target));

            var convertor = new HydroRegionFeatureCoverageFromNetworkCoverageValueConverter
            {
                OriginalValue = featureCoverage,
                ConvertedValue = networkCoverage,
                HydroRegion = region
            };

            // modify network coverage
            var time2 = new DateTime(2000, 1, 3);
            networkCoverage.AddValuesForTime(new[] { 5.0, 6.0 }, time2);

            var c1 = featureCoverage.Features.ElementAt(0);
            var c2 = featureCoverage.Features.ElementAt(1);

            convertor.Update(time2);

            // assert
            featureCoverage[time2, c1].Should("No link -> No value propagation should occur.").Not.Be.EqualTo(5.0);
            featureCoverage[time2, c2].Should("No link -> No value propagation should occur.").Not.Be.EqualTo(6.0);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void LinkWithDataItems()
        {
            var c1 = featureCoverage.Features.First();

            // setup dataitems
            var networkCoverageDataItem = new DataItem(networkCoverage);
            var featureCoverageDataItem = new DataItem(featureCoverage);

            var converter = new HydroRegionFeatureCoverageFromNetworkCoverageValueConverter
                {
                    OriginalValue = featureCoverage, HydroRegion = region
                };
            var childDataItem = new DataItem
                {
                    ValueType = typeof(INetworkCoverage),
                    ValueConverter = converter
                };
            featureCoverageDataItem.Children.Add(childDataItem);

            // link data items
            childDataItem.LinkTo(networkCoverageDataItem);

            // asserts
            converter.Update(time0);
            featureCoverage[time0,c1].Should().Be.EqualTo(1.0);

            converter.Update(time1); 
            featureCoverage[time1,c1].Should().Be.EqualTo(2.0);
        }
    }
}