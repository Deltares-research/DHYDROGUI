using System;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Exporters;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Exporters
{
    [TestFixture]
    public class BasinGeometryShapeFileSerializerTest
    {
        [Test]
        public void GivenBasinGeometryShapeFileSerializer_WritingAndReadingCatchmentGeometries_ShouldWorkCorrectly()
        {
            //Arrange
            var path = TestHelper.GetTestWorkingDirectoryGeneratedTestFilePath("shp");
            var basin = new DrainageBasin();
            basin.Catchments.Add(new Catchment
            {
                Name = "Catchment 1",
                Geometry = CreateSquarePolygon()
            });

            basin.Catchments.Add(new Catchment
            {
                Name = "Catchment 2",
                Geometry = CreateSquarePolygon(100, 100, 200)
            });

            var serializer = new BasinGeometryShapeFileSerializer();

            // Act
            Assert.True(serializer.WriteCatchmentGeometry(basin, path));

            var basin2 = new DrainageBasin();
            basin2.Catchments.Add(new Catchment { Name = "Catchment 1" });
            basin2.Catchments.Add(new Catchment { Name = "Catchment 2" });

            // Assert
            Assert.IsTrue(serializer.ReadCatchmentGeometry(basin2, path));

            Assert.AreEqual(basin.Catchments[0].Geometry, basin2.Catchments[0].Geometry);
            Assert.AreEqual(basin.Catchments[1].Geometry, basin2.Catchments[1].Geometry);
        }

        [Test]
        public void GivenBasinGeometryShapeFileSerializer_ReadingCatchmentGeometries_ShouldNotWorkWithDuplicateIds()
        {
            //Arrange
            var path = TestHelper.GetTestWorkingDirectoryGeneratedTestFilePath("shp");
            var basin = new DrainageBasin();
            basin.Catchments.Add(new Catchment
            {
                Name = "Catchment 1",
                Geometry = CreateSquarePolygon()
            });

            basin.Catchments.Add(new Catchment
            {
                Name = "Catchment 1",
                Geometry = CreateSquarePolygon(100, 100, 200)
            });

            var serializer = new BasinGeometryShapeFileSerializer();

            // Act
            Assert.True(serializer.WriteCatchmentGeometry(basin, path));

            var basin2 = new DrainageBasin();
            basin2.Catchments.Add(new Catchment { Name = "Catchment 1" });
            basin2.Catchments.Add(new Catchment { Name = "Catchment 1" });

            // Assert
            TestHelper.AssertAtLeastOneLogMessagesContains(() =>
            {
                Assert.IsFalse(serializer.ReadCatchmentGeometry(basin2, path));
            }, "Either the basin catchments do not have unique id's or the features from shape file");
        }

        [Test]
        public void BasinGeometryShapeFileSerializer_WritingCatchmentGeometries_ToInvalidPathGivesErrorMessage()
        {
            //Arrange
            var basin = new DrainageBasin();
            var serializer = new BasinGeometryShapeFileSerializer();

            // Assert
            TestHelper.AssertAtLeastOneLogMessagesContains(() =>
            {
                Assert.IsFalse(serializer.WriteCatchmentGeometry(basin, null));
            }, "Could not write catchment geometries of basin");
        }

        [Test]
        public void BasinGeometryShapeFileSerializer_ReadingCatchmentGeometries_FromInvalidPathGivesErrorMessage()
        {
            //Arrange
            var basin = new DrainageBasin();
            var serializer = new BasinGeometryShapeFileSerializer();

            // Assert
            TestHelper.AssertAtLeastOneLogMessagesContains(() =>
            {
                Assert.IsFalse(serializer.ReadCatchmentGeometry(basin, "test"));
            }, "Could not read catchment geometries for basin");
        }

        [Test]
        public void BasinGeometryShapeFileSerializer_ReadingCatchmentGeometries_WithNullPathThrowsArgumentError()
        {
            //Arrange
            var basin = new DrainageBasin();
            var serializer = new BasinGeometryShapeFileSerializer();

            // Assert
            var error = Assert.Throws<ArgumentException>(() => serializer.ReadCatchmentGeometry(basin, null));
            Assert.NotNull(error);
            Assert.AreEqual(error.ParamName, "path");
            Assert.AreEqual(error.Message, "Exception of type 'System.ArgumentException' was thrown.\r\nParameter name: path");
        }

        private static Polygon CreateSquarePolygon(double offsetX = 0, double offsetY = 0, double size = 10)
        {
            var coordinates = new[]
            {
                new Coordinate(offsetX, offsetY),
                new Coordinate(offsetX, offsetY + size),
                new Coordinate(offsetX + size, offsetY + size),
                new Coordinate(offsetX + size, offsetY),
                new Coordinate(offsetX, offsetY),
            };

            return new Polygon(new LinearRing(coordinates));
        }
    }
}