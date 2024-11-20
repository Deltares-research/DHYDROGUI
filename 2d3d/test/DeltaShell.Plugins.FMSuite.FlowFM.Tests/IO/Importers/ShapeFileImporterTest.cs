using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.IO.ImportExport;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
    public class ShapeFileImporterTest
    {
        /// WHEN a ShapeFileImporter is constructed with null
        /// THEN an ArgumentNullException is thrown
        [Test]
        public void WhenAShapeFileImporterIsConstructedWithNull_ThenAnArgumentNullExceptionIsThrown()
        {
            Assert.Throws<ArgumentNullException>(() => new ShapeFileImporter<IGeometry, Feature2D>(null),
                                                 $"Expected a {typeof(ArgumentNullException)}, but got:");
        }

        /// GIVEN a ShapeFileImporter
        /// WHEN Name is retrieved
        /// THEN ESRI Shapefile importer is retrieved
        [Test]
        public void WhenNameIsRetrieved_ThenESRIShapefileImporterIsRetrieved()
        {
            var importer = new ShapeFileImporter<IGeometry, Feature2D>((p, log) => new List<IFeature>());
            const string expectedVal = "ESRI Shapefile importer";
            Assert.That(importer.Name, Is.EqualTo(expectedVal),
                        "Expected a different name:");
        }

        /// <summary>
        /// GIVEN a ShapeFileImporter
        /// WHEN the category is retrieved
        /// THEN 2D3D is given
        /// </summary>
        [Test]
        public void GivenAShapeFileImporter_WhenTheCategoryIsRetrieved_Then2D3DIsGiven()
        {
            var importer = new ShapeFileImporter<IGeometry, Feature2D>((p, log) => new List<IFeature>());
            const string expectedVal = "2D / 3D";
            Assert.That(importer.Category, Is.EqualTo(expectedVal),
                        "Expected a different category:");
        }

        /// <summary>
        /// GIVEN a ShapeFileImporter
        /// WHEN the description is retrieved
        /// THEN the correct description is given
        /// </summary>
        [Test]
        public void GivenAShapeFileImporter_WhenTheDescriptionIsRetrieved_ThenTheCorrectDescriptionIsGiven()
        {
            var importer = new ShapeFileImporter<IGeometry, Feature2D>((p, log) => new List<IFeature>());
            const string expectedVal = "ESRI Shapefile importer";
            Assert.That(importer.Description, Is.EqualTo(expectedVal),
                        "Expected a different description:");
        }

        /// <summary>
        /// GIVEN a ShapeFileImporter
        /// WHEN the SupportedItemTypes are retrieved
        /// THEN a set containing IList of TFeature2D is given
        /// </summary>
        [Test]
        public void GivenAShapeFileImporter_WhenTheSupportedItemTypesAreRetrieved_ThenASetContainingIListOfTFeature2DIsGiven()
        {
            var importer = new ShapeFileImporter<IGeometry, Feature2D>((p, log) => new List<IFeature>());

            IList<Type> supportedTypes = importer.SupportedItemTypes?.ToList();

            Assert.That(supportedTypes, Is.Not.Null, "Expected the list of not to be null.");
            Assert.That(supportedTypes.Count, Is.EqualTo(1), "Expected a single element in SupportedItemTypes.");
            Assert.That(supportedTypes.First(), Is.EqualTo(typeof(IList<Feature2D>)),
                        "Expected a different type contained in SupportedItemType.");
        }

        /// <summary>
        /// GIVEN a ShapeFileImporter
        /// WHEN CanImportOnRootLevel is retrieved
        /// THEN false is returned
        /// </summary>
        [Test]
        public void GivenAShapeFileImporter_WhenCanImportOnRootLevelIsRetrieved_ThenFalseIsReturned()
        {
            var importer = new ShapeFileImporter<IGeometry, Feature2D>((p, log) => new List<IFeature>());
            Assert.That(importer.CanImportOnRootLevel, Is.False, "Expected CanImportOnRootLevel to be false.");
        }

        /// <summary>
        /// GIVEN a ShapeFileImporter
        /// WHEN FileFilter is retrieved
        /// THEN the correct file filter is given
        /// </summary>
        [Test]
        public void GivenAShapeFileImporter_WhenFileFilterIsRetrieved_ThenTheCorrectFileFilterIsGiven()
        {
            var importer = new ShapeFileImporter<IGeometry, Feature2D>((p, log) => new List<IFeature>());
            const string expectedVal = "Shape file (*.shp)|*.shp";
            Assert.That(importer.FileFilter, Is.EqualTo(expectedVal),
                        "Expected a different file filter:");
        }

        /// GIVEN a ShapeFileImporter
        /// WHEN Mode is retrieved
        /// THEN Import is returned
        [Test]
        public void GivenAShapeFileImporter_WhenModeIsRetrieved_ThenImportIsReturned()
        {
            var importer = new ShapeFileImporter<IGeometry, Feature2D>((p, log) => new List<IFeature>());
            Assert.That(importer.Mode, Is.EqualTo(Feature2DImportExportMode.Import), "Expected a different Mode:");
        }

        /// <summary>
        /// GIVEN a ShapeFileImporter
        /// WHEN OnImportItem is called with a null path
        /// THEN null is returned
        /// AND an error is logged
        /// </summary>
        [Test]
        public void GivenAShapeFileImporter_WhenOnImportItemIsCalledWithANullPath_ThenNullIsReturnedAndAnErrorIsLogged()
        {
            var readMock = MockRepository.GenerateStrictMock<Func<string, ILog, IEnumerable<IFeature>>>();
            readMock.Replay();

            var importer = new ShapeFileImporter<ILineString, Feature2D>(readMock);
            object result = null;

            TestHelper.AssertLogMessageIsGenerated(() => result = importer.ImportItem(null),
                                                   "No file was presented to import from.",
                                                   1);

            readMock.VerifyAllExpectations();
            Assert.That(result, Is.Null, "Expected result to be null.");
        }

        /// <summary>
        /// GIVEN a ShapeFileImporter
        /// WHEN OnImportItem is called with a valid path and a null target
        /// THEN null is returned
        /// AND an error is logged
        /// </summary>
        [Test]
        public void GivenAShapeFileImporter_WhenOnImportItemIsCalledWithAValidPathAndANullTarget_ThenNullIsReturnedAndAnErrorIsLogged()
        {
            const string validPath = "MostSurelyIAmAValidPath.shp";

            var readMock = MockRepository.GenerateStrictMock<Func<string, ILog, IEnumerable<IFeature>>>();
            readMock.Replay();

            var importer = new ShapeFileImporter<ILineString, Feature2D>(readMock);
            object result = null;

            TestHelper.AssertLogMessageIsGenerated(() => result = importer.ImportItem(validPath, null),
                                                   "No target was presented to import to.",
                                                   1);

            readMock.VerifyAllExpectations();
            Assert.That(result, Is.Null, "Expected result to be null.");
        }

        /// <summary>
        /// GIVEN a ShapeFileImporter
        /// AND a valid path
        /// AND a valid target
        /// WHEN OnImportItem is called
        /// THEN the features corresponding with the file at path are added to the target
        /// </summary>
        [Test]
        public void GivenAShapeFileImporterAndAValidPathAndAValidTarget_WhenOnImportItemIsCalled_ThenTheFeaturesCorrespondingWithTheFileAtPathAreAddedToTheTarget()
        {
            // Given
            const string validPath = "YetAnotherTotallyLegitPath.shp";

            IFeature featureMock1 = GetFeatureMock();
            IFeature featureMock2 = GetFeatureMock();

            featureMock1.Replay();
            featureMock2.Replay();

            var featureList = new List<IFeature>()
            {
                featureMock1,
                featureMock2
            };

            var readMock = MockRepository.GenerateStrictMock<Func<string, ILog, IEnumerable<IFeature>>>();
            readMock.Expect(f => f.Invoke(Arg<string>.Matches(s => s.Equals(validPath)),
                                          Arg<ILog>.Is.Anything))
                    .Return(featureList)
                    .Repeat.Once();

            readMock.Replay();
            ICoordinateTransformation transformation = GetTransformationMock();

            var importer = new ShapeFileImporter<ILineString, Feature2D>(readMock, ShapeFileImporterFactory.AfterFeatureCreateActions.TryAddName) {CoordinateTransformation = transformation};

            Feature2D feature2DMock = GetFeature2DMock();
            feature2DMock.Replay();

            var target = new List<Feature2D>() {feature2DMock};

            // When
            importer.ImportItem(validPath, target);

            // Then
            readMock.VerifyAllExpectations();
            featureMock1.VerifyAllExpectations();
            featureMock2.VerifyAllExpectations();

            Assert.That(target, Is.Not.Null, "Expected target not to be null after importing.");
            Assert.That(target.Count, Is.EqualTo(3), "Expected three elements after importing, but got:");
            Assert.That(target, Has.Member(feature2DMock), "Expected the original element to still exist in the target.");

            const string nameElement1 = "imported_feature";
            Feature2D firstElement = target.FirstOrDefault(e => e?.Name?.Equals(nameElement1) ?? false);
            Assert.That(firstElement, Is.Not.Null,
                        $"Expected an imported item with name {nameElement1} but found none.");

            const string nameElement2 = "imported_feature_1";
            Feature2D secondElement = target.FirstOrDefault(e => e?.Name?.Equals(nameElement2) ?? false);
            Assert.That(secondElement, Is.Not.Null,
                        $"Expected an imported item with name {nameElement2} but found none.");
        }

        private static IFeature GetFeatureMock()
        {
            var coordinates = new Coordinate[]
            {
                new Coordinate(0.0, 1.0),
                new Coordinate(5.0, 10.0)
            };

            var geometry = new LineString(coordinates);

            var featureMock = MockRepository.GenerateStrictMock<IFeature>();

            featureMock.Expect(f => f.Geometry)
                       .Return(geometry)
                       .Repeat.Any();

            featureMock.Expect(f => f.Attributes)
                       .Return(new DictionaryFeatureAttributeCollection())
                       .Repeat.Any();

            return featureMock;
        }

        private static ICoordinateTransformation GetTransformationMock()
        {
            var coordinates = new Coordinate[]
            {
                new Coordinate(0.0, 1.0),
                new Coordinate(5.0, 10.0)
            };

            var mathTransform = MockRepository.GenerateMock<IMathTransform>();
            mathTransform.Expect(f => f.TransformList(Arg<IList<Coordinate>>.Is.Anything))
                         .Return(coordinates)
                         .Repeat.Any();

            var transformation = MockRepository.GenerateMock<ICoordinateTransformation>();
            transformation.Expect(t => t.MathTransform)
                          .Return(mathTransform)
                          .Repeat.Any();

            return transformation;
        }

        private static Feature2D GetFeature2DMock()
        {
            var feature2D = MockRepository.GenerateMock<Feature2D>();
            feature2D.Stub(f => f.Name).Return("Ce ne est pas une Feature");

            return feature2D;
        }
    }
}