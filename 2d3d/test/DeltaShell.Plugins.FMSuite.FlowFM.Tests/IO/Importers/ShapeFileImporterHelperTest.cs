using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap.Data.Providers;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
    public class ShapeFileImporterHelperTest
    {
        /// <summary>
        /// GIVEN a valid path describing a file with a single IFeature
        /// WHEN Read is called
        /// THEN a list describing only this feature is returned
        /// </summary>
        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAValidPathDescribingAFileWithASingleIFeature_WhenReadIsCalled_ThenAListDescribingOnlyThisFeatureIsReturned()
        {
            using (var tempDir = new TemporaryDirectory())
            {
                // Given
                string featurePathSrc =
                    Path.Combine(TestHelper.GetTestDataDirectory(), "ShapeFileImporter", "SingleFeature");

                FileUtils.CopyDirectory(featurePathSrc, tempDir.Path);

                string shapeFilePath = Path.Combine(tempDir.Path, "SingleFeature.shp");

                // When
                IList<IFeature> result = ShapeFileImporterHelper.Read<ILineString>(shapeFilePath, null).ToList();

                // Then
                Assert.That(result, Is.Not.Null, "Expected the Read result not to be null.");
                Assert.That(result.Count, Is.EqualTo(1), "Expected a different number of IFeatures to be read.");
                IFeature feature = result.First();

                Assert.That(feature.Geometry, Is.Not.Null, $"Expected Geometry on {feature} retrieved IFeature.");
                Assert.That(feature.Attributes, Is.Not.Null, $"Expected attributes on {feature} retrieved IFeature.");
            }
        }

        /// <summary>
        /// GIVEN a valid path describing a file with multiple IFeatures
        /// WHEN Read is called
        /// THEN a list containing only these features is returned
        /// </summary>
        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAValidPathDescribingAFileWithMultipleIFeatures_WhenReadIsCalled_ThenAListContainingOnlyTheseFeaturesIsReturned()
        {
            using (var tempDir = new TemporaryDirectory())
            {
                // Given
                string featurePathSrc = Path.Combine(TestHelper.GetTestDataDirectory(), "ShapeFileImporter", "MultipleFeatures");

                FileUtils.CopyDirectory(featurePathSrc, tempDir.Path);

                string shapeFilePath = Path.Combine(tempDir.Path, "SixFeatures.shp");

                // When
                IList<IFeature> result = ShapeFileImporterHelper.Read<ILineString>(shapeFilePath, null).ToList();

                // Then
                Assert.That(result, Is.Not.Null, "Expected the Read result not to be null.");
                Assert.That(result.Count, Is.EqualTo(6), "Expected a different number of IFeatures to be read.");

                foreach (IFeature feature in result)
                {
                    Assert.That(feature.Geometry, Is.Not.Null, $"Expected Geometry on {feature} retrieved IFeature.");
                    Assert.That(feature.Attributes, Is.Not.Null, $"Expected attributes on {feature} retrieved IFeature.");
                }
            }
        }

        /// <summary>
        /// GIVEN a valid path describing a file with a multiple IFeatures
        /// WHEN Read is called with an incorrect type parameter
        /// THEN an empty list is returned
        /// </summary>
        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAValidPathDescribingAFileWithAMultipleIFeatures_WhenReadIsCalledWithAnIncorrectTypeParameter_ThenAnEmptyListIsReturned()
        {
            using (var tempDir = new TemporaryDirectory())
            {
                // Given
                string featurePathSrc = Path.Combine(TestHelper.GetTestDataDirectory(), "ShapeFileImporter", "MultipleFeatures");

                FileUtils.CopyDirectory(featurePathSrc, tempDir.Path);

                string shapeFilePath = Path.Combine(tempDir.Path, "SixFeatures.shp");

                // When
                IList<IFeature> result = ShapeFileImporterHelper.Read<IPoint>(shapeFilePath, null).ToList();

                // Then
                Assert.That(result, Is.Not.Null, "Expected the Read result not to be null.");
                Assert.That(result.Count, Is.EqualTo(0), "Expected a different number of IFeatures to be read.");
            }
        }

        /// <summary>
        /// GIVEN an IFeature
        /// WHEN ConvertGeometry is called with IPoint
        /// THEN the corresponding IGeometry is returned
        /// </summary>
        [Test]
        public void GivenAnIFeature_WhenConvertGeometryIsCalledWithIPoint_ThenTheCorrespondingIGeometryIsReturned()
        {
            // Given
            var coordinates = new Coordinate[1];
            coordinates[0] = new Coordinate(10.0, 12.0);

            var featureMock = MockRepository.GenerateStrictMock<IFeature>();
            featureMock.Expect(f => f.Geometry.Coordinates)
                       .Return(coordinates)
                       .Repeat.AtLeastOnce();

            var geometryMock = MockRepository.GenerateStrictMock<IPoint>();

            var factoryMock = MockRepository.GenerateStrictMock<IGeometryFactory>();
            factoryMock.Expect(f => f.CreatePoint(coordinates[0]))
                       .Return(geometryMock)
                       .Repeat.Once();

            featureMock.Replay();
            geometryMock.Replay();
            factoryMock.Replay();

            // When
            IGeometry result = ShapeFileImporterHelper.ConvertGeometry<IPoint>(featureMock, factoryMock);

            // Then
            featureMock.VerifyAllExpectations();
            geometryMock.VerifyAllExpectations();
            factoryMock.VerifyAllExpectations();

            Assert.That(result, Is.EqualTo(geometryMock));
        }

        /// <summary>
        /// GIVEN an IFeature
        /// WHEN ConvertGeometry is called with ILineString
        /// THEN the corresponding IGeometry is returned
        /// </summary>
        [Test]
        public void GivenAnIFeature_WhenConvertGeometryIsCalledWithILineString_ThenTheCorrespondingIGeometryIsReturned()
        {
            // Given
            var coordinates = new Coordinate[1];
            coordinates[0] = new Coordinate(10.0, 12.0);

            var featureMock = MockRepository.GenerateStrictMock<IFeature>();
            featureMock.Expect(f => f.Geometry.Coordinates)
                       .Return(coordinates)
                       .Repeat.AtLeastOnce();

            var geometryMock = MockRepository.GenerateStrictMock<ILineString>();

            var factoryMock = MockRepository.GenerateStrictMock<IGeometryFactory>();
            factoryMock.Expect(f => f.CreateLineString(coordinates))
                       .Return(geometryMock)
                       .Repeat.Once();

            featureMock.Replay();
            geometryMock.Replay();
            factoryMock.Replay();

            // When
            IGeometry result = ShapeFileImporterHelper.ConvertGeometry<ILineString>(featureMock, factoryMock);

            // Then
            featureMock.VerifyAllExpectations();
            geometryMock.VerifyAllExpectations();
            factoryMock.VerifyAllExpectations();

            Assert.That(result, Is.EqualTo(geometryMock));
        }

        /// <summary>
        /// GIVEN an IFeature
        /// WHEN ConvertGeometry is called with IPolygon
        /// THEN the corresponding IGeometry is returned
        /// </summary>
        [Test]
        public void GivenAnIFeature_WhenConvertGeometryIsCalledWithIPolygon_ThenTheCorrespondingIGeometryIsReturned()
        {
            // Given
            var coordinates = new Coordinate[1];
            coordinates[0] = new Coordinate(10.0, 12.0);

            var featureMock = MockRepository.GenerateStrictMock<IFeature>();
            featureMock.Expect(f => f.Geometry.Coordinates)
                       .Return(coordinates)
                       .Repeat.AtLeastOnce();

            var geometryMock = MockRepository.GenerateStrictMock<IPolygon>();

            var factoryMock = MockRepository.GenerateStrictMock<IGeometryFactory>();
            factoryMock.Expect(f => f.CreatePolygon(coordinates))
                       .Return(geometryMock)
                       .Repeat.Once();

            featureMock.Replay();
            geometryMock.Replay();
            factoryMock.Replay();

            // When
            IGeometry result = ShapeFileImporterHelper.ConvertGeometry<IPolygon>(featureMock, factoryMock);

            // Then
            featureMock.VerifyAllExpectations();
            geometryMock.VerifyAllExpectations();
            factoryMock.VerifyAllExpectations();

            Assert.That(result, Is.EqualTo(geometryMock));
        }

        /// <summary>
        /// GIVEN an IFeature
        /// WHEN ConvertGeometry is called with an incorrect type
        /// THEN null is returned
        /// </summary>
        [Test]
        public void GivenAnIFeature_WhenConvertGeometryIsCalledWithAnIncorrectType_ThenNullIsReturned()
        {
            // Given
            var coordinates = new Coordinate[1];
            coordinates[0] = new Coordinate(10.0, 12.0);

            var featureMock = MockRepository.GenerateStrictMock<IFeature>();
            featureMock.Expect(f => f.Geometry.Coordinates)
                       .Return(coordinates)
                       .Repeat.AtLeastOnce();

            var geometryMock = MockRepository.GenerateStrictMock<IPolygon>();

            featureMock.Replay();
            geometryMock.Replay();

            // When
            IGeometry result = ShapeFileImporterHelper.ConvertGeometry<IGeometry>(featureMock);

            // Then
            featureMock.VerifyAllExpectations();
            geometryMock.VerifyAllExpectations();

            Assert.That(result, Is.Null);
        }

        /// <summary>
        /// GIVEN a ShapeFile reader with a shape type
        /// WHEN IsShapeTypeValid with the corresponding Geometry type is called
        /// THEN true is returned
        /// </summary>
        [TestCase(ShapeType.Point, typeof(IPoint), true)]
        [TestCase(ShapeType.Point, typeof(ILineString), false)]
        [TestCase(ShapeType.Point, typeof(IPolygon), false)]
        [TestCase(ShapeType.PolyLine, typeof(IPoint), false)]
        [TestCase(ShapeType.PolyLine, typeof(ILineString), true)]
        [TestCase(ShapeType.PolyLine, typeof(IPolygon), false)]
        [TestCase(ShapeType.PolyLineZ, typeof(IPoint), false)]
        [TestCase(ShapeType.PolyLineZ, typeof(ILineString), true)]
        [TestCase(ShapeType.PolyLineZ, typeof(IPolygon), false)]
        [TestCase(ShapeType.Polygon, typeof(IPoint), false)]
        [TestCase(ShapeType.Polygon, typeof(ILineString), false)]
        [TestCase(ShapeType.Polygon, typeof(IPolygon), true)]
        [TestCase(ShapeType.MultiPatch, typeof(IPoint), false)]
        [TestCase(ShapeType.MultiPatch, typeof(ILineString), false)]
        [TestCase(ShapeType.MultiPatch, typeof(IPolygon), false)]
        public void GivenAShapeFileReaderWithAShapeType_WhenIsShapeTypeValidWithTheCorrespondingGeometryTypeIsCalled_ThenTrueIsReturned(ShapeType shapeType,
                                                                                                                                        Type genericType,
                                                                                                                                        bool expectedVal)
        {
            // Given
            var shapeFile = MockRepository.GenerateMock<ShapeFile>();
            shapeFile.Expect(e => e.ShapeType)
                     .Return(shapeType)
                     .Repeat.Once();

            shapeFile.Replay();

            // When
            var result = (bool) TypeUtils.CallStaticGenericMethod(typeof(ShapeFileImporterHelperTest),
                                                                  nameof(IsShapeTypeValidWrapper),
                                                                  genericType,
                                                                  shapeFile);

            // Then
            shapeFile.VerifyAllExpectations();
            Assert.That(result, Is.EqualTo(expectedVal),
                        $"Expected a different result for IsShapeTypeValid<{genericType}> with {shapeType}.");
        }

        // Ensure our type reflection only relies on functions internal to our class.
        public static bool IsShapeTypeValidWrapper<T>(ShapeFile shapeFile) where T : IGeometry
        {
            return ShapeFileImporterHelper.IsShapeTypeValid<T>(shapeFile);
        }
    }
}