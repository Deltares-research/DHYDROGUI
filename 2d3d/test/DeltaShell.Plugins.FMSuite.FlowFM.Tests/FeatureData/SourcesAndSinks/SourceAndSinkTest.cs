using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.SourcesAndSinks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Features.Generic;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.FeatureData.SourcesAndSinks
{
    [TestFixture]
    internal class SourceAndSinkTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var sourceAndSink = new SourceAndSink();

            // Assert
            Assert.That(sourceAndSink, Is.InstanceOf<FeatureData<SourceAndSinkFunction, Feature2D>>());
            Assert.That(sourceAndSink.Data, Is.TypeOf<SourceAndSinkFunction>());
            Assert.That(sourceAndSink.Feature, Is.Null);
            Assert.That(sourceAndSink.Function, Is.SameAs(sourceAndSink.Data));
            Assert.That(sourceAndSink.TracerNames, Is.Empty);
            Assert.That(sourceAndSink.SedimentFractionNames, Is.Empty);
            Assert.That(sourceAndSink.Area, Is.Zero);
            Assert.That(sourceAndSink.MomentumSource, Is.False);
        }

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
    }
}