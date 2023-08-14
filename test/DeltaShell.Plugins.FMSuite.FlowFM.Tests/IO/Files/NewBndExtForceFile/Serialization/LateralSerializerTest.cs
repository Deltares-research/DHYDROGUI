using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.Laterals;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.NewBndExtForceFile.Serialization;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files.NewBndExtForceFile.Serialization
{
    [TestFixture]
    public class LateralSerializerTest
    {
        [Test]
        public void Serialize_ArgNull_ThrowsArgumentNullException()
        {
            // Setup
            var lateralSerializer = new LateralSerializer();

            // Call
            void Call() => lateralSerializer.Serialize(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Serialize_SerializesLateral()
        {
            // Setup
            var lateralSerializer = new LateralSerializer();

            var feature = new Feature2D
            {
                Name = "some_name",
                Geometry = new Point(1.23, 2.34)
            };
            var lateral = new Lateral { Feature = feature };

            // Call
            DelftIniCategory delftIniCategory = lateralSerializer.Serialize(lateral);

            // Assert
            Assert.That(delftIniCategory.Name, Is.EqualTo("lateral"));
            Assert.That(delftIniCategory.Properties, Has.Count.EqualTo(8));
            Assert.That(delftIniCategory.GetPropertyValue("id"), Is.EqualTo("some_name"));
            Assert.That(delftIniCategory.GetPropertyValue("name"), Is.EqualTo("some_name"));
            Assert.That(delftIniCategory.GetPropertyValue("type"), Is.EqualTo("discharge"));
            Assert.That(delftIniCategory.GetPropertyValue("locationType"), Is.EqualTo("2d"));
            Assert.That(delftIniCategory.GetPropertyValue("numCoordinates"), Is.EqualTo("1"));
            Assert.That(delftIniCategory.GetPropertyValue("xCoordinates"), Is.EqualTo("1.2300000e+000"));
            Assert.That(delftIniCategory.GetPropertyValue("yCoordinates"), Is.EqualTo("2.3400000e+000"));
            Assert.That(delftIniCategory.GetPropertyValue("discharge"), Is.EqualTo("0.0000000e+000"));
        }

        [Test]
        public void Serialize_LateralDTOWithConstantDischarge()
        {
            // Setup
            var lateralSerializer = new LateralSerializer();

            var xCoordinates = new[] { 1.23, 2.34, 3.45 };
            var yCoordinates = new[] { 4.56, 5.67, 6.78 };

            var feature = new Feature2D
            {
                Name = "some_id",
                Geometry = GetPolygonGeometry(xCoordinates, yCoordinates)
            };
            var lateral = new Lateral { Feature = feature };

            lateral.Data.Discharge.Type = LateralDischargeType.Constant;
            lateral.Data.Discharge.Constant = 1.23;

            // Call
            DelftIniCategory delftIniCategory = lateralSerializer.Serialize(lateral);

            // Assert
            Assert.That(delftIniCategory.Name, Is.EqualTo("lateral"));
            Assert.That(delftIniCategory.Properties, Has.Count.EqualTo(8));
            Assert.That(delftIniCategory.GetPropertyValue("id"), Is.EqualTo("some_id"));
            Assert.That(delftIniCategory.GetPropertyValue("name"), Is.EqualTo("some_id"));
            Assert.That(delftIniCategory.GetPropertyValue("type"), Is.EqualTo("discharge"));
            Assert.That(delftIniCategory.GetPropertyValue("locationType"), Is.EqualTo("2d"));
            Assert.That(delftIniCategory.GetPropertyValue("numCoordinates"), Is.EqualTo("3"));
            Assert.That(delftIniCategory.GetPropertyValue("xCoordinates"), Is.EqualTo("1.2300000e+000 2.3400000e+000 3.4500000e+000"));
            Assert.That(delftIniCategory.GetPropertyValue("yCoordinates"), Is.EqualTo("4.5600000e+000 5.6700000e+000 6.7800000e+000"));
            Assert.That(delftIniCategory.GetPropertyValue("discharge"), Is.EqualTo("1.2300000e+000"));
        }

        [Test]
        public void Serialize_LateralDTOWithTimeSeriesDischarge()
        {
            // Setup
            var lateralSerializer = new LateralSerializer();

            var xCoordinates = new[] { 1.23, 2.34, 3.45 };
            var yCoordinates = new[] { 4.56, 5.67, 6.78 };

            var feature = new Feature2D
            {
                Name = "some_id",
                Geometry = GetPolygonGeometry(xCoordinates, yCoordinates)
            };
            var lateral = new Lateral { Feature = feature };
            lateral.Data.Discharge.Type = LateralDischargeType.TimeSeries;

            // Call
            DelftIniCategory delftIniCategory = lateralSerializer.Serialize(lateral);

            // Assert
            Assert.That(delftIniCategory.Name, Is.EqualTo("lateral"));
            Assert.That(delftIniCategory.Properties, Has.Count.EqualTo(8));
            Assert.That(delftIniCategory.GetPropertyValue("id"), Is.EqualTo("some_id"));
            Assert.That(delftIniCategory.GetPropertyValue("name"), Is.EqualTo("some_id"));
            Assert.That(delftIniCategory.GetPropertyValue("type"), Is.EqualTo("discharge"));
            Assert.That(delftIniCategory.GetPropertyValue("locationType"), Is.EqualTo("2d"));
            Assert.That(delftIniCategory.GetPropertyValue("numCoordinates"), Is.EqualTo("3"));
            Assert.That(delftIniCategory.GetPropertyValue("xCoordinates"), Is.EqualTo("1.2300000e+000 2.3400000e+000 3.4500000e+000"));
            Assert.That(delftIniCategory.GetPropertyValue("yCoordinates"), Is.EqualTo("4.5600000e+000 5.6700000e+000 6.7800000e+000"));
            Assert.That(delftIniCategory.GetPropertyValue("discharge"), Is.EqualTo("lateral_discharge.bc"));
        }

        [Test]
        public void Serialize_LateralDTOWithRealTimeDischarge()
        {
            // Setup
            var lateralSerializer = new LateralSerializer();

            var xCoordinates = new[] { 1.23, 2.34, 3.45 };
            var yCoordinates = new[] { 4.56, 5.67, 6.78 };

            var feature = new Feature2D
            {
                Name = "some_id",
                Geometry = GetPolygonGeometry(xCoordinates, yCoordinates)
            };
            var lateral = new Lateral { Feature = feature };

            lateral.Data.Discharge.Type = LateralDischargeType.RealTime;

            // Call
            DelftIniCategory delftIniCategory = lateralSerializer.Serialize(lateral);

            // Assert
            Assert.That(delftIniCategory.Name, Is.EqualTo("lateral"));
            Assert.That(delftIniCategory.Properties, Has.Count.EqualTo(8));
            Assert.That(delftIniCategory.GetPropertyValue("id"), Is.EqualTo("some_id"));
            Assert.That(delftIniCategory.GetPropertyValue("name"), Is.EqualTo("some_id"));
            Assert.That(delftIniCategory.GetPropertyValue("type"), Is.EqualTo("discharge"));
            Assert.That(delftIniCategory.GetPropertyValue("locationType"), Is.EqualTo("2d"));
            Assert.That(delftIniCategory.GetPropertyValue("numCoordinates"), Is.EqualTo("3"));
            Assert.That(delftIniCategory.GetPropertyValue("xCoordinates"), Is.EqualTo("1.2300000e+000 2.3400000e+000 3.4500000e+000"));
            Assert.That(delftIniCategory.GetPropertyValue("yCoordinates"), Is.EqualTo("4.5600000e+000 5.6700000e+000 6.7800000e+000"));
            Assert.That(delftIniCategory.GetPropertyValue("discharge"), Is.EqualTo("realtime"));
        }

        private static Polygon GetPolygonGeometry(double[] xCoordinates, double[] yCoordinates)
        {
            Coordinate CreateCoordinate(double x, double y) => new Coordinate(x, y);
            List<Coordinate> coordinates = xCoordinates.Zip(yCoordinates, CreateCoordinate).ToList();

            Coordinate firstCoordinate = coordinates[0];
            coordinates.Add(firstCoordinate);

            return new Polygon(new LinearRing(coordinates.ToArray()));
        }
    }
}