using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.FeatureData
{
    [TestFixture]
    class SourceAndSinkTest
    {
        [Test]
        public void IsPointSourceTest()
        {
            var sourceAndSink = new SourceAndSink()
            {
                Feature = new Feature2D() {Name = "test", Geometry = new Point(new Coordinate(0, 0))}
            };

            Assert.NotNull(sourceAndSink);
            Assert.IsTrue(sourceAndSink.IsPointSource);

            sourceAndSink.Feature.Geometry = new LineString(new[] {new Coordinate(0, 0), new Coordinate(1, 1)});
            Assert.IsFalse(sourceAndSink.IsPointSource);
        }

        [Test]
        public void MomentumSourceTest()
        {
            var sourceAndSink = new SourceAndSink();

            sourceAndSink.Area = 0.0;
            Assert.IsFalse(sourceAndSink.MomentumSource);

            sourceAndSink.Area = 0.1;
            Assert.IsTrue(sourceAndSink.MomentumSource);
        }

        [Test]
        public void CanIncludeMomentumSourceTest()
        {
            var sourceAndSink = new SourceAndSink()
            {
                Feature = new Feature2D() {Name = "test", Geometry = new Point(new Coordinate(0, 0))}
            };
            Assert.NotNull(sourceAndSink);

            /* It should always be the opposit of isPointSource */
            Assert.AreEqual(!sourceAndSink.IsPointSource, sourceAndSink.CanIncludeMomentum);

            sourceAndSink.Feature.Geometry = new LineString(new[] {new Coordinate(0, 0), new Coordinate(1, 1)});
            Assert.AreEqual(!sourceAndSink.IsPointSource, sourceAndSink.CanIncludeMomentum);
        }

        [Test]
        public void FeaturePropertyChangedSourceAndSinkTest()
        {
            /* Mainly test the name is correctly replaced. */
            var sourceAndSink = new SourceAndSink();
            sourceAndSink.Feature = new Feature2D() {Name = "test"};
            int changedProps = 0;
            ((INotifyPropertyChange) sourceAndSink.Feature).PropertyChanged += (s, e) => { changedProps++; };

            var newName = "New name";
            sourceAndSink.Feature.Name = newName;

            Assert.Greater(changedProps, 0);
            Assert.AreEqual(newName, sourceAndSink.Feature.Name);
        }

        [Test]
        public void
            GivenAModelWithSourcesAndSinks_WhenAddingTracersAndSedimentsFractionsToModel_ThenTheyShouldBeAddedToComponents()
        {
            var fractionList = CreateSedimentFractionList();
            var tracerList = CreateBoundaryConditionList();

            var sourceSink = new SourceAndSink();
            var boundarySet = new BoundaryConditionSet();

            var model = new WaterFlowFMModel
            {
                SourcesAndSinks = {sourceSink},
                BoundaryConditionSets = {boundarySet}
            };

            var initialComponentsCount = sourceSink.Function.Components.Count;

            model.SedimentFractions.AddRange(fractionList);
            boundarySet.BoundaryConditions.AddRange(tracerList);

            var finalComponents = sourceSink.Function.Components;
            var finalComponentsCount = sourceSink.Function.Components.Count;

            Assert.AreEqual(fractionList.Count + tracerList.Count, finalComponentsCount - initialComponentsCount);
            fractionList.ForEach(sf => Assert.That(finalComponents.Select(c => c.Name).Contains(sf.Name)));
            tracerList.ForEach(t => Assert.That(finalComponents.Select(c => c.Name).Contains(t.TracerName)));
        }

        [Test]
        public void
            GivenAModelWithSourcesAndSinks_WhenRemovingTracersAndSedimentsFractionsToModel_ThenTheyShouldBeRemovedFromComponents()
        {
            var fractionList = CreateSedimentFractionList();
            var tracerList = CreateBoundaryConditionList();

            var sourceSink = new SourceAndSink();
            var boundarySet = new BoundaryConditionSet();

            var model = new WaterFlowFMModel
            {
                SourcesAndSinks = { sourceSink },
                BoundaryConditionSets = { boundarySet }
            };

            model.SedimentFractions.AddRange(fractionList);
            boundarySet.BoundaryConditions.AddRange(tracerList);

            model.SedimentFractions.Clear();
            Assert.IsFalse(sourceSink.Function.Components.Select(c => c.Name).Contains("Fraction_1"));
            Assert.IsFalse(sourceSink.Function.Components.Select(c => c.Name).Contains("Fraction_2"));
            Assert.IsFalse(sourceSink.Function.Components.Select(c => c.Name).Contains("Fraction_3"));

            model.BoundaryConditionSets.ForEach(bcs => bcs.BoundaryConditions.Clear());
            Assert.IsFalse(sourceSink.Function.Components.Select(c => c.Name).Contains("Tracer_1"));
            Assert.IsFalse(sourceSink.Function.Components.Select(c => c.Name).Contains("Tracer_2"));
            Assert.IsFalse(sourceSink.Function.Components.Select(c => c.Name).Contains("Tracer_3"));
        }

        [Test]
        public void
            GivenAModelWithSourcesAndSinks_WhenAddingANewSourceAndSink_ThenTheirFunctionDataShouldBeSynced()
        {
            var fractionList = CreateSedimentFractionList();
            var tracerList = CreateBoundaryConditionList();

            var firstSourceSink = new SourceAndSink();
            var boundarySet = new BoundaryConditionSet();

            var model = new WaterFlowFMModel
            {
                SourcesAndSinks = { firstSourceSink },
                BoundaryConditionSets = { boundarySet }
            };

            model.SedimentFractions.AddRange(fractionList);
            boundarySet.BoundaryConditions.AddRange(tracerList);

            var componentsFirst = firstSourceSink.Function.Components;
            var componentsfFirstCount = firstSourceSink.Function.Components.Count;

            var newSourceSink = new SourceAndSink();
            model.SourcesAndSinks.Add(newSourceSink);

            var componentsNew = newSourceSink.Function.Components;
            var componentsNewCount = newSourceSink.Function.Components.Count;

            Assert.AreEqual(
                componentsfFirstCount, componentsNewCount);
            Assert.AreEqual(
                componentsFirst.Select(c => c.Name), 
                componentsNew.Select(c => c.Name));
        }

        private List<FlowBoundaryCondition> CreateBoundaryConditionList()
        {
            return new List<FlowBoundaryCondition>
            {
                new FlowBoundaryCondition(FlowBoundaryQuantityType.Tracer, BoundaryConditionDataType.Empty)
                {
                    Name = "Tracer_1",
                    TracerName = "Tracer_1",
                },
                new FlowBoundaryCondition(FlowBoundaryQuantityType.Tracer, BoundaryConditionDataType.Empty)
                {
                    Name = "Tracer_2",
                    TracerName = "Tracer_2",
                },
                new FlowBoundaryCondition(FlowBoundaryQuantityType.Tracer, BoundaryConditionDataType.Empty)
                {
                    Name = "Tracer_3",
                    TracerName = "Tracer_3",
                }
            };
        }

        private List<SedimentFraction> CreateSedimentFractionList()
        {
            return new List<SedimentFraction>
            {
                new SedimentFraction {Name = "Fraction_1"},
                new SedimentFraction {Name = "Fraction_2"},
                new SedimentFraction {Name = "Fraction_3"}
            };
        }

    }
}
