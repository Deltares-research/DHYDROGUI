using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
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
            
            sourceAndSink.Feature.Geometry = new LineString( new []{ new Coordinate(0, 0), new Coordinate(1,1) });
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
                Feature = new Feature2D() { Name = "test", Geometry = new Point(new Coordinate(0, 0)) }
            };
            Assert.NotNull(sourceAndSink);

            /* It should always be the opposit of isPointSource */
            Assert.AreEqual( !sourceAndSink.IsPointSource, sourceAndSink.CanIncludeMomentum);
            
            sourceAndSink.Feature.Geometry = new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 1) });
            Assert.AreEqual(!sourceAndSink.IsPointSource, sourceAndSink.CanIncludeMomentum);
        }

        [Test]
        public void FeaturePropertyChangedSourceAndSinkTest()
        {
            /* Mainly test the name is correctly replaced. */
            var sourceAndSink = new SourceAndSink();
            sourceAndSink.Feature = new Feature2D() { Name = "test"};
            int changedProps = 0;
            ((INotifyPropertyChange)sourceAndSink.Feature).PropertyChanged += (s, e) =>
            {
                changedProps++;
            };

            var newName = "New name";
            sourceAndSink.Feature.Name = newName;

            Assert.Greater(changedProps, 0);
            Assert.AreEqual(newName, sourceAndSink.Feature.Name);
        }
    }
}
