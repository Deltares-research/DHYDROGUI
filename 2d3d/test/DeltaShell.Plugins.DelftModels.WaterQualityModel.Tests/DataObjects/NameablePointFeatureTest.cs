using System;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.DataObjects
{
    [TestFixture]
    public class NameablePointFeatureTest
    {
        [Test]
        public void TestDefaultConstructorExpectedValues()
        {
            // setup

            // call
            var pointFeature = new SimpleNameablePointFeature();

            // assert
            Assert.IsInstanceOf<INameable>(pointFeature);
            Assert.IsInstanceOf<Feature>(pointFeature);
            Assert.IsInstanceOf<INotifyPropertyChange>(pointFeature);

            Assert.IsInstanceOf<IPoint>(pointFeature.Geometry);
            var pointGeometry = (IPoint) pointFeature.Geometry;
            Assert.AreEqual(0, pointFeature.X);
            Assert.AreEqual(pointFeature.X, pointGeometry.X);
            Assert.AreEqual(0, pointFeature.Y);
            Assert.AreEqual(pointFeature.Y, pointGeometry.Y);
            Assert.IsNaN(pointFeature.Z);
            Assert.AreEqual(pointFeature.Z, pointGeometry.Z);

            Assert.AreEqual(string.Empty, pointFeature.Name);
        }

        [Test]
        public void TestSimpleNameablePointFeatureOnlySupportsIPointInstances()
        {
            // setup
            var mocks = new MockRepository();
            var nonIPointStub = mocks.Stub<IGeometry>();
            mocks.ReplayAll();

            var pointFeature = new SimpleNameablePointFeature();

            // call
            TestDelegate call = () => pointFeature.Geometry = nonIPointStub;

            // assert
            var exception = Assert.Throws<NotSupportedException>(call);
            Assert.AreEqual("Only point geometries are supported", exception.Message);
        }

        [Test]
        public void SetXYUpdatesFeatureEnvelope()
        {
            // setup
            var pointFeature = new SimpleNameablePointFeature();

            // call
            pointFeature.X = 1.1;

            // assert
            Envelope envelope = pointFeature.Geometry.EnvelopeInternal;
            Assert.AreEqual(1.1, envelope.MinX);
            Assert.AreEqual(1.1, envelope.MaxX);
            Assert.AreEqual(0.0, envelope.MinY);
            Assert.AreEqual(0.0, envelope.MaxY);

            // call
            pointFeature.Y = 3.4;

            // assert
            envelope = pointFeature.Geometry.EnvelopeInternal;
            Assert.AreEqual(1.1, envelope.MinX);
            Assert.AreEqual(1.1, envelope.MaxX);
            Assert.AreEqual(3.4, envelope.MinY);
            Assert.AreEqual(3.4, envelope.MaxY);
        }

        [Test]
        public void SetXYSendsPropertyChangeEventForGeometry()
        {
            // setup
            var pointFeature = new SimpleNameablePointFeature();
            var changingCount = 0;
            var changedCount = 0;
            string geometryPropertyName = nameof(NameablePointFeature.Geometry);
            ((INotifyPropertyChange) pointFeature).PropertyChanging += (sender, args) =>
            {
                if (args.PropertyName == geometryPropertyName)
                {
                    changingCount++;
                }
            };
            ((INotifyPropertyChange) pointFeature).PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == geometryPropertyName)
                {
                    changedCount++;
                }
            };

            // call & assert
            pointFeature.X = 1.1;
            Assert.AreEqual(1, changingCount);
            Assert.AreEqual(1, changedCount);

            pointFeature.Y = 2.2;
            Assert.AreEqual(2, changingCount);
            Assert.AreEqual(2, changedCount);

            pointFeature.Z = 3.3;
            Assert.AreEqual(3, changingCount);
            Assert.AreEqual(3, changedCount);
        }

        private class SimpleNameablePointFeature : NameablePointFeature {}
    }
}