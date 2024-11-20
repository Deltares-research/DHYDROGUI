using System;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.DataObjects.BoundaryData
{
    [TestFixture]
    public class WaterQualityLoadTest
    {
        [Test]
        public void DefaultConstructorTestExpectedValues()
        {
            // setup

            // call
            var load = new WaterQualityLoad();

            // assert
            Assert.IsInstanceOf<INameable>(load);
            Assert.AreEqual(string.Empty, load.Name);
            Assert.AreEqual(0, load.X);
            Assert.AreEqual(0, load.Y);
            Assert.IsNaN(load.Z);
            Assert.AreEqual(string.Empty, load.LoadType);
        }

        [Test]
        public void NewlyCreatedWaterQualityLoadIsValidFeature()
        {
            // setup

            // call
            var load = new WaterQualityLoad();

            // assert
            Assert.IsInstanceOf<Feature>(load);
            Assert.IsInstanceOf<IPoint>(load.Geometry);
            var point = (IPoint) load.Geometry;
            Assert.AreEqual(0.0, point.X);
            Assert.AreEqual(0.0, point.Y);
            Assert.IsNaN(point.Z);
            Assert.AreEqual(1, point.CoordinateSequence.Count);
            Assert.AreEqual(0.0, point.CoordinateSequence.GetX(0));
            Assert.AreEqual(0.0, point.CoordinateSequence.GetY(0));

            Assert.IsNull(load.Attributes);
        }

        [Test]
        public void XYZPropertiesLinkedWithGeometry()
        {
            // setup
            var load = new WaterQualityLoad();

            // call
            load.X = 1.2;
            load.Y = 3.4;
            load.Z = 5.6;

            // assert
            Assert.AreEqual(1.2, ((IPoint) load.Geometry).X);
            Assert.AreEqual(3.4, ((IPoint) load.Geometry).Y);
            Assert.AreEqual(5.6, ((IPoint) load.Geometry).Z);
        }

        [Test]
        public void GeometryPointXYZPropertiesLinkedWithLoadXYZ()
        {
            // setup
            var load = new WaterQualityLoad();

            // call
            var point = (IPoint) load.Geometry;
            point.X = 1.2;
            point.Y = 3.4;
            point.Z = 5.6;

            // assert
            Assert.AreEqual(1.2, load.X);
            Assert.AreEqual(3.4, load.Y);
            Assert.AreEqual(5.6, load.Z);
        }

        [Test]
        public void SetGeometryToDifferentTypeThenIPointThrowsArgumentException()
        {
            // setup
            var mocks = new MockRepository();
            var nonIPointStub = mocks.Stub<IGeometry>();
            mocks.ReplayAll();

            var load = new WaterQualityLoad();

            // call
            TestDelegate call = () => load.Geometry = nonIPointStub;

            // assert
            var exception = Assert.Throws<NotSupportedException>(call);
            Assert.AreEqual("Only point geometries are supported", exception.Message);
        }

        [Test]
        public void SetGeometryToDifferentIPointUpdatesXYZ()
        {
            var load = new WaterQualityLoad {Geometry = new Point(1.1, 7.9, double.NaN)};

            Assert.AreEqual(1.1, load.X);
            Assert.AreEqual(7.9, load.Y);
            Assert.IsNaN(load.Z);
        }
    }
}