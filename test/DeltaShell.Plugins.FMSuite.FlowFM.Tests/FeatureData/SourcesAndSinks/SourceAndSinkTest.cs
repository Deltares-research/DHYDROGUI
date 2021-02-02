using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.TestUtils.AutoFixtureCustomizations;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.SourcesAndSinks;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.Sediment;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.FeatureData.SourcesAndSinks
{
    [TestFixture]
    internal class SourceAndSinkTest
    {
        [Test]
        public void IsPointSourceTest()
        {
            var sourceAndSink = new SourceAndSink()
            {
                Feature = new Feature2D()
                {
                    Name = "test",
                    Geometry = new Point(new Coordinate(0, 0))
                }
            };

            Assert.NotNull(sourceAndSink);
            Assert.IsTrue(sourceAndSink.IsPointSource);

            sourceAndSink.Feature.Geometry = new LineString(new[]
            {
                new Coordinate(0, 0),
                new Coordinate(1, 1)
            });
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
                Feature = new Feature2D()
                {
                    Name = "test",
                    Geometry = new Point(new Coordinate(0, 0))
                }
            };
            Assert.NotNull(sourceAndSink);

            /* It should always be the opposit of isPointSource */
            Assert.AreEqual(!sourceAndSink.IsPointSource, sourceAndSink.CanIncludeMomentum);

            sourceAndSink.Feature.Geometry = new LineString(new[]
            {
                new Coordinate(0, 0),
                new Coordinate(1, 1)
            });
            Assert.AreEqual(!sourceAndSink.IsPointSource, sourceAndSink.CanIncludeMomentum);
        }

        [Test]
        public void FeaturePropertyChangedSourceAndSinkTest()
        {
            /* Mainly test the name is correctly replaced. */
            var sourceAndSink = new SourceAndSink();
            sourceAndSink.Feature = new Feature2D() {Name = "test"};
            var changedProps = 0;
            ((INotifyPropertyChange) sourceAndSink.Feature).PropertyChanged += (s, e) => { changedProps++; };

            var newName = "New name";
            sourceAndSink.Feature.Name = newName;

            Assert.Greater(changedProps, 0);
            Assert.AreEqual(newName, sourceAndSink.Feature.Name);
        }

        [Test]
        public void Given_DefaultSourceAndSink_When_Created_Then_DefaultVariableTime_Is_Now()
        {
            // 1. Set up test model
            var sourceAndSink = new SourceAndSink();
            var acceptedDifference = new TimeSpan(0, 0, 5);
            DateTime defaultDateTime = DateTime.Today;

            // 2. Verify expectations
            Assert.That(sourceAndSink.Function, Is.Not.Null);
            Assert.That(sourceAndSink.Function.Arguments, Is.Not.Null);
            IVariable<DateTime> defaultArgument = sourceAndSink.Function.Arguments.OfType<IVariable<DateTime>>().FirstOrDefault();
            Assert.That(defaultArgument, Is.Not.Null);
            Assert.That(defaultArgument.DefaultValue, Is.Not.Null);

            TimeSpan defValue = defaultArgument.DefaultValue.ToUniversalTime() - defaultDateTime.ToUniversalTime();
            Assert.That(
                defValue,
                Is.LessThan(acceptedDifference),
                $"The default time for source and sink ({defaultArgument.DefaultValue}) does not match the expectations ({defaultDateTime}).");
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenAWaterFlowFMModelWithTracers_WhenAddingASourceAndSink_TheTracersAreAddedToTheSourceAndSink()
        {
            // Given
            using (var model = new WaterFlowFMModel())
            {
                var tracers = Create.For<IList<string>>();
                model.TracerDefinitions.AddRange(tracers);

                var sourceAndSink = new SourceAndSink();

                // When
                model.SourcesAndSinks.Add(sourceAndSink);

                // Then
                IEventedList<IVariable> variables = sourceAndSink.Data.Components;
                Assert.That(variables, Has.Count.EqualTo(7));
                Assert.That(variables[0].Name, Is.EqualTo("Discharge"));
                Assert.That(variables[1].Name, Is.EqualTo("Salinity"));
                Assert.That(variables[2].Name, Is.EqualTo("Temperature"));
                Assert.That(variables[3].Name, Is.EqualTo("Secondary Flow"));
                Assert.That(variables[4].Name, Is.EqualTo(tracers[0]));
                Assert.That(variables[5].Name, Is.EqualTo(tracers[1]));
                Assert.That(variables[6].Name, Is.EqualTo(tracers[2]));
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenAWaterFlowFMModelWithSourcesAndSinks_WhenAddingATracer_TheTracerIsAddedToTheSourcesAndSinks()
        {
            // Given
            using (var model = new WaterFlowFMModel())
            {
                List<SourceAndSink> sourcesAndSinks = CreateSourcesAndSinks().ToList();
                model.SourcesAndSinks.AddRange(sourcesAndSinks);

                // When
                model.TracerDefinitions.Add("Some Tracer");

                // Then
                foreach (SourceAndSink sourceAndSink in sourcesAndSinks)
                {
                    IEventedList<IVariable> variables = sourceAndSink.Data.Components;
                    Assert.That(variables, Has.Count.EqualTo(5));
                    Assert.That(variables[0].Name, Is.EqualTo("Discharge"));
                    Assert.That(variables[1].Name, Is.EqualTo("Salinity"));
                    Assert.That(variables[2].Name, Is.EqualTo("Temperature"));
                    Assert.That(variables[3].Name, Is.EqualTo("Secondary Flow"));
                    Assert.That(variables[4].Name, Is.EqualTo("Some Tracer"));
                }
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenAWaterFlowFMModelWithSourcesAndSinks_WhenRemovingATracer_TheTracerIsRemovedFromTheSourcesAndSinks()
        {
            // Given
            using (var model = new WaterFlowFMModel())
            {
                List<SourceAndSink> sourcesAndSinks = CreateSourcesAndSinks().ToList();
                model.SourcesAndSinks.AddRange(sourcesAndSinks);

                model.TracerDefinitions.Add("Some Tracer 1");
                model.TracerDefinitions.Add("Some Tracer 2");

                // When
                model.TracerDefinitions.Remove("Some Tracer 1");

                // Then
                foreach (SourceAndSink sourceAndSink in sourcesAndSinks)
                {
                    IEventedList<IVariable> variables = sourceAndSink.Data.Components;
                    Assert.That(variables, Has.Count.EqualTo(5));
                    Assert.That(variables[0].Name, Is.EqualTo("Discharge"));
                    Assert.That(variables[1].Name, Is.EqualTo("Salinity"));
                    Assert.That(variables[2].Name, Is.EqualTo("Temperature"));
                    Assert.That(variables[3].Name, Is.EqualTo("Secondary Flow"));
                    Assert.That(variables[4].Name, Is.EqualTo("Some Tracer 2"));
                }
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenAWaterFlowFMModelWithSedimentFractions_WhenAddingASourceAndSink_TheSedimentFractionsAreAddedToTheSourceAndSink()
        {
            // Given
            using (var model = new WaterFlowFMModel())
            {
                List<SedimentFraction> sedimentFractions = CreateSedimentFractions().ToList();
                model.SedimentFractions.AddRange(sedimentFractions);

                var sourceAndSink = new SourceAndSink();

                // When
                model.SourcesAndSinks.Add(sourceAndSink);

                // Then
                IEventedList<IVariable> variables = sourceAndSink.Data.Components;
                Assert.That(variables, Has.Count.EqualTo(7));
                Assert.That(variables[0].Name, Is.EqualTo("Discharge"));
                Assert.That(variables[1].Name, Is.EqualTo("Salinity"));
                Assert.That(variables[2].Name, Is.EqualTo("Temperature"));
                Assert.That(variables[3].Name, Is.EqualTo(sedimentFractions[0].Name));
                Assert.That(variables[4].Name, Is.EqualTo(sedimentFractions[1].Name));
                Assert.That(variables[5].Name, Is.EqualTo(sedimentFractions[2].Name));
                Assert.That(variables[6].Name, Is.EqualTo("Secondary Flow"));
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenAWaterFlowFMModelWithSourcesAndSinks_WhenAddingASedimentFraction_TheSedimentFractionIsAddedToTheSourcesAndSinks()
        {
            // Given
            using (var model = new WaterFlowFMModel())
            {
                List<SourceAndSink> sourcesAndSinks = CreateSourcesAndSinks().ToList();
                model.SourcesAndSinks.AddRange(sourcesAndSinks);

                // When
                model.SedimentFractions.Add(new SedimentFraction {Name = "Some Sediment Fraction"});

                // Then
                foreach (SourceAndSink sourceAndSink in sourcesAndSinks)
                {
                    IEventedList<IVariable> variables = sourceAndSink.Data.Components;
                    Assert.That(variables, Has.Count.EqualTo(5));
                    Assert.That(variables[0].Name, Is.EqualTo("Discharge"));
                    Assert.That(variables[1].Name, Is.EqualTo("Salinity"));
                    Assert.That(variables[2].Name, Is.EqualTo("Temperature"));
                    Assert.That(variables[3].Name, Is.EqualTo("Some Sediment Fraction"));
                    Assert.That(variables[4].Name, Is.EqualTo("Secondary Flow"));
                }
            }
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void GivenAWaterFlowFMModelWithSourcesAndSinks_WhenRemovingASedimentFraction_TheSedimentFractionIsRemovedFromTheSourcesAndSinks()
        {
            // Given
            using (var model = new WaterFlowFMModel())
            {
                List<SourceAndSink> sourcesAndSinks = CreateSourcesAndSinks().ToList();
                model.SourcesAndSinks.AddRange(sourcesAndSinks);

                var sedimentFraction1 = new SedimentFraction {Name = "Some Sediment Fraction 1"};
                var sedimentFraction2 = new SedimentFraction {Name = "Some Sediment Fraction 2"};
                model.SedimentFractions.Add(sedimentFraction1);
                model.SedimentFractions.Add(sedimentFraction2);

                // When
                model.SedimentFractions.Remove(sedimentFraction1);

                // Then
                foreach (SourceAndSink sourceAndSink in sourcesAndSinks)
                {
                    IEventedList<IVariable> variables = sourceAndSink.Data.Components;
                    Assert.That(variables, Has.Count.EqualTo(5));
                    Assert.That(variables[0].Name, Is.EqualTo("Discharge"));
                    Assert.That(variables[1].Name, Is.EqualTo("Salinity"));
                    Assert.That(variables[2].Name, Is.EqualTo("Temperature"));
                    Assert.That(variables[3].Name, Is.EqualTo("Some Sediment Fraction 2"));
                    Assert.That(variables[4].Name, Is.EqualTo("Secondary Flow"));
                }
            }
        }

        [Test]
        public void GetTracerNames_ReturnsCorrectNames()
        {
            // Setup
            var sourceAndSink = new SourceAndSink();
            sourceAndSink.Function.AddTracer("Some Tracer 1");
            sourceAndSink.Function.AddTracer("Some Tracer 2");
            sourceAndSink.Function.AddTracer("Some Tracer 3");

            // Call
            List<string> names = sourceAndSink.TracerNames.ToList();

            // Assert
            Assert.That(names, Has.Count.EqualTo(3));
            Assert.That(names[0], Is.EqualTo("Some Tracer 1"));
            Assert.That(names[1], Is.EqualTo("Some Tracer 2"));
            Assert.That(names[2], Is.EqualTo("Some Tracer 3"));
        }

        [Test]
        public void GetSedimentFractionNames_ReturnsCorrectNames()
        {
            // Setup
            var sourceAndSink = new SourceAndSink();
            sourceAndSink.Function.AddSedimentFraction("Some Sediment Fraction 1");
            sourceAndSink.Function.AddSedimentFraction("Some Sediment Fraction 2");
            sourceAndSink.Function.AddSedimentFraction("Some Sediment Fraction 3");

            // Call
            List<string> names = sourceAndSink.SedimentFractionNames.ToList();

            // Assert
            Assert.That(names, Has.Count.EqualTo(3));
            Assert.That(names[0], Is.EqualTo("Some Sediment Fraction 1"));
            Assert.That(names[1], Is.EqualTo("Some Sediment Fraction 2"));
            Assert.That(names[2], Is.EqualTo("Some Sediment Fraction 3"));
        }

        private static IEnumerable<SourceAndSink> CreateSourcesAndSinks()
        {
            yield return new SourceAndSink();
            yield return new SourceAndSink();
            yield return new SourceAndSink();
        }

        private static IEnumerable<SedimentFraction> CreateSedimentFractions()
        {
            yield return new SedimentFraction {Name = "Fraction_1"};
            yield return new SedimentFraction {Name = "Fraction_2"};
            yield return new SedimentFraction {Name = "Fraction_3"};
        }
    }
}