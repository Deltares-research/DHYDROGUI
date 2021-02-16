using System;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
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
    public class HydroLinksFeatureCoverageValueConverterTest
    {
        private static readonly WKTReader wktReader = new WKTReader();

        private IHydroRegion region;
        private IFeatureCoverage featureCoverageTarget;
        private IFeatureCoverage featureCoverageSource;
        private readonly DateTime time0 = new DateTime(2000, 1, 1);
        private readonly DateTime time1 = new DateTime(2000, 1, 2);

        private IHydroNode node1;
        private IHydroNode node2;
        private ILateralSource lateralSource1;
        private ILateralSource lateralSource2;
        private IDrainageBasin basin;
        private IHydroNetwork network;
        private IChannel branch1;

        [SetUp]
        public void SetUp()
        {
            // setup region
            node1 = new HydroNode { Name = "node1", Geometry = wktReader.Read("POINT(10 0)") };
            node2 = new HydroNode { Name = "node2", Geometry = wktReader.Read("POINT(0 0)") };
            branch1 = new Channel { Name = "branch1", Source = node1, Target = node2, Geometry = wktReader.Read("LINESTRING(0 0, 10 0)") };
            lateralSource1 = new LateralSource() { Name = "lateral1", Branch = branch1, Chainage = 5.0, Geometry = wktReader.Read("POINT(5 0)") }; ;
            lateralSource2 = new LateralSource() { Name = "lateral2", Branch = branch1, Chainage = 7.0, Geometry = wktReader.Read("POINT(7 0)") }; ;
            network = new HydroNetwork { Branches = { branch1 }, Nodes = { node1, node2 } };

            var c1 = new Catchment();
            var c2 = new Catchment();
            basin = new DrainageBasin { Catchments = { c1, c2 } };

            region = new HydroRegion { SubRegions = { network, basin } };

            c1.LinkTo(lateralSource1);
            c2.LinkTo(lateralSource2);

            // setup coverages
            featureCoverageTarget = new FeatureCoverage("q to boundaries");
            featureCoverageTarget.Arguments.Add(new Variable<DateTime>());
            featureCoverageTarget.Arguments.Add(new Variable<IFeature>());
            featureCoverageTarget.Components.Add(new Variable<double>("h"));
            featureCoverageTarget.Features.AddRange(new[] { lateralSource1, lateralSource2 });
            featureCoverageTarget.FeatureVariable.Values.AddRange(new[] { lateralSource1, lateralSource2 });

            featureCoverageSource = new FeatureCoverage("q from catchments");
            featureCoverageSource.Arguments.Add(new Variable<DateTime>());
            featureCoverageSource.Arguments.Add(new Variable<IFeature>());
            featureCoverageSource.Components.Add(new Variable<double>("h"));
            featureCoverageSource.Features.AddRange(new[] { c1, c2 });
            featureCoverageSource.FeatureVariable.Values.AddRange(new[] { c1, c2 });
            featureCoverageSource[time0, c1] = 1.0;
            featureCoverageSource[time1, c1] = 2.0;
            featureCoverageSource[time0, c2] = 3.0;
            featureCoverageSource[time1, c2] = 4.0;
        }

        [Test]
        public void TargetCoverageIsInitializedCorrectly()
        {
            var convertor = new HydroLinksFeatureCoverageValueConverter
                {
                    OriginalValue = featureCoverageTarget,
                    ConvertedValue = featureCoverageSource,
                    HydroRegion = region
                };

            // assert
            convertor.Update(time0);
            featureCoverageTarget[time0, lateralSource1].Should().Be.EqualTo(1.0);
            featureCoverageTarget[time0, lateralSource2].Should().Be.EqualTo(3.0);
            
            convertor.Update(time1);
            featureCoverageTarget[time1, lateralSource1].Should().Be.EqualTo(2.0);
            featureCoverageTarget[time1, lateralSource2].Should().Be.EqualTo(4.0);
        }

        [Test]
        public void TargetCoverageIsUpdatedCorrectly()
        {
            var convertor = new HydroLinksFeatureCoverageValueConverter
            {
                OriginalValue = featureCoverageTarget,
                ConvertedValue = featureCoverageSource,
                HydroRegion = region
            };
            
            // modify network coverage
            var time2 = new DateTime(2000, 1, 3);
            featureCoverageSource[time2] = new[] { 5.0, 6.0 };
            convertor.Update(time2);
            // assert
            featureCoverageTarget[time2, lateralSource1].Should().Be.EqualTo(5.0);
            featureCoverageTarget[time2, lateralSource2].Should().Be.EqualTo(6.0);
        }
        
        [Test]
        public void TargetCoverageIsUpdatedCorrectlyOnClearOfSourceCoverage()
        {
            new HydroLinksFeatureCoverageValueConverter
            {
                OriginalValue = featureCoverageTarget,
                ConvertedValue = featureCoverageSource,
                HydroRegion = region
            };

            // clear feature coverage
            featureCoverageSource.Clear();

            // assert
            Assert.AreEqual(0, featureCoverageTarget.Time.Values.Count);
        }

        [Test]
        public void TargetCoverageIsRebuildWhenItsFeaturesAreChanged()
        {
            var convertor = new HydroLinksFeatureCoverageValueConverter
            {
                OriginalValue = featureCoverageTarget,
                ConvertedValue = featureCoverageSource,
                HydroRegion = region
            };
            
            // clear target feature coverage
            featureCoverageTarget.Clear(); //clear: no more features

            // set features again: we expect it to be rebuild
            featureCoverageTarget.Features = new EventedList<IFeature>(new[] {lateralSource1, lateralSource2});
            featureCoverageTarget.FeatureVariable.Values.AddRange(new[] { lateralSource1, lateralSource2 });
            Assert.AreEqual(0, featureCoverageTarget.Time.Values.Count); 
            convertor.Update(time0);
            Assert.AreEqual(1, featureCoverageTarget.Time.Values.Count);
        }

        [Test]
        [Category(TestCategory.Performance)]
        public void RebuildIsQuick()
        {
            var convertor = new HydroLinksFeatureCoverageValueConverter
            {
                OriginalValue = featureCoverageTarget,
                ConvertedValue = featureCoverageSource,
                HydroRegion = region
            };
            
            // clear target feature coverage
            featureCoverageSource.Clear();
            featureCoverageTarget.Clear(); //clear: no more features

            // add additional features
            var numValues = 200;

            var catchments = Enumerable.Range(0, numValues).Select(i => new Catchment()).ToList();
            var lateralSources = Enumerable.Range(0, numValues).Select(i => new LateralSource()).ToList();
            for (int i = 0; i < numValues; i++)
            {
                basin.Catchments.Add(catchments[i]);
                branch1.BranchFeatures.Add(lateralSources[i]);

                catchments[i].LinkTo(lateralSources[i]);
            }
            featureCoverageSource.Features = new EventedList<IFeature>(catchments.OfType<IFeature>());
            featureCoverageSource.FeatureVariable.Values.AddRange(catchments);

            // add additional time values to coverage
            var times = Enumerable.Range(0, numValues).Select(i => new DateTime(2000, 1, 1).AddDays(i)).ToList();
            featureCoverageSource.Time.SetValues(times);
            
            // set features again: we expect it to be rebuild
            featureCoverageTarget.Features = new EventedList<IFeature>(lateralSources.OfType<IFeature>());

            TestHelper.AssertIsFasterThan(500, () => //this action triggers a Convert
            {
                featureCoverageTarget.FeatureVariable.Values.AddRange(lateralSources);
                times.ForEach(t => convertor.Update(t));
            });
            ;
            Assert.AreEqual(1, featureCoverageTarget.Time.Values.Count);
            Assert.AreEqual(times.LastOrDefault(), featureCoverageTarget.Time.Values[0]);
        }

        [Test]
        public void NumberOfConvertOperationsShouldBeMinimal()
        {
            var convertor = new HydroLinksFeatureCoverageValueConverter
            {
                OriginalValue = featureCoverageTarget,
                ConvertedValue = featureCoverageSource,
                HydroRegion = region
            };
    
            var lastTime = new DateTime(2000, 1, 7);

            featureCoverageSource[new DateTime(2000, 1, 3)] = new[] { 1.0, 2.0 };
            featureCoverageSource[new DateTime(2000, 1, 4)] = new[] { 1.0, 2.0 };
            featureCoverageSource[new DateTime(2000, 1, 5)] = new[] { 1.0, 2.0 };
            featureCoverageSource[new DateTime(2000, 1, 6)] = new[] { 1.0, 2.0 };
            featureCoverageSource[lastTime] = new[] { 1.0, 2.0 };
            convertor.Update(lastTime);
            featureCoverageTarget[lastTime, lateralSource1].Should().Be.EqualTo(1.0);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void LinkWithDataItems()
        {
            // setup dataitems
            var targetDataItem = new DataItem(featureCoverageTarget);
            var sourceDataItem = new DataItem(featureCoverageSource);

            var convertor = new HydroLinksFeatureCoverageValueConverter
            {
                OriginalValue = featureCoverageTarget, //do we have to set this manually?!?!
                HydroRegion = region
            };
            var childDataItem = new DataItem
                {
                    ValueType = typeof (IFeatureCoverage),
                    ValueConverter = convertor
                };
            targetDataItem.Children.Add(childDataItem);

            // link data items
            childDataItem.LinkTo(sourceDataItem);

            // asserts
            convertor.Update(time0);
            featureCoverageTarget[time0, lateralSource1].Should().Be.EqualTo(1.0);
            convertor.Update(time1); 
            featureCoverageTarget[time1, lateralSource2].Should().Be.EqualTo(4.0);
        }
    }
}