using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Common.Gui.RgfGrid;
using GeoAPI.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.RgfGrid
{
    [TestFixture]
    public class RgfConfigTest
    {
        /// <summary>
        /// WHEN an RgfConfig is constructed
        /// THEN the AdditionalGeometryPaths are empty
        /// AND the GridFileNames are empty
        /// AND the PolFileName is null
        /// AND the Polygons are null
        /// </summary>
        [Test]
        public void WhenAnRgfConfigIsConstructed_ThenTheAdditionalGeometryPathsAreEmptyAndTheGridFileNamesAreEmptyAndThePolFileNameIsNullAndThePolygonsAreNull()
        {
            // When
            var config = new RgfConfig();

            // Then
            Assert.That(config.AdditionalGeometryPaths, Is.Not.Null,
                        "Expected the additional geometry paths not to be null.");
            Assert.That(config.AdditionalGeometryPaths, Is.Empty,
                        "Expected the additional geometry paths to be empty.");

            Assert.That(config.GridFileNames, Is.Not.Null,
                        "Expected the grid file names not to be null.");
            Assert.That(config.GridFileNames, Is.Empty,
                        "Expected the grid file names to be empty.");

            Assert.That(config.Polygons, Is.Null,
                        "Expected the Polygons to be null.");
            Assert.That(config.PolFileName, Is.Null,
                        "Expected the PolFileName to be null.");
        }

        /// <summary>
        /// GIVEN a new RgfConfig
        /// WHEN it is converted to DelftIniCategories
        /// THEN it has the correct FileInformation header
        /// </summary>
        [Test]
        public void GivenANewRgfConfig_WhenItIsConvertedToDelftIniCategories_ThenItHasTheCorrectFileInformationHeader()
        {
            // Given
            var config = new RgfConfig();

            // When
            IList<DelftIniCategory> categories = config.ToDelftIniCategories().ToList();

            // Then
            Assert.That(categories, Is.Not.Null,
                        "Expected the returned categories not to be null.");
            Assert.That(categories, Has.Count.EqualTo(1),
                        "Expected a single category to be returned.");

            DelftIniCategory category = categories.First();
            AssertValidCategory(category, RgfConfig.FileInformationHeader,
                                new Tuple<string, string>(RgfConfig.FileGeneratedBy, null),
                                new Tuple<string, string>(RgfConfig.FileCreationData, null),
                                new Tuple<string, string>(RgfConfig.FileVersion, null));
        }

        /// <summary>
        /// GIVEN a RgfConfig without a polygon file name and polygons
        /// WHEN it is converted to DelftIniCategories
        /// THEN it has no Polygon category
        /// AND it has no Batch category
        /// </summary>
        [Test]
        public void GivenARgfConfigWithoutAPolygonFileNameAndPolygons_WhenItIsConvertedToDelftIniCategories_ThenItHasNoPolygonCategoryAndItHasNoBatchCategory()
        {
            // Given
            var config = new RgfConfig
            {
                Polygons = null,
                PolFileName = null
            };

            // When 
            IList<DelftIniCategory> categories = config.ToDelftIniCategories().ToList();

            // Then
            Assert.That(categories, Is.Not.Null,
                        "Expected the returned categories not to be null.");

            Assert.That(categories.Any(c => c.Name == RgfConfig.PolygonsHeader), Is.False,
                        $"Expected no category with the {RgfConfig.PolygonsHeader} header.");
            Assert.That(categories.Any(c => c.Name == RgfConfig.BatchHeader), Is.False,
                        $"Expected no category with the {RgfConfig.BatchHeader} header.");
        }

        /// <summary>
        /// GIVEN a RgfConfig with a polygon file name and polygons
        /// WHEN it is converted to DelftIniCategories
        /// THEN it has the correct Polygon category
        /// AND it has the correct Batch category
        /// </summary>
        [Test]
        public void GivenARgfConfigWithAPolygonFileNameAndPolygons_WhenItIsConvertedToDelftIniCategories_ThenItHasTheCorrectPolygonCategoryAndItHasTheCorrectBatchCategory()
        {
            // Given
            const string expectedPolFileName = "bacon.pol";
            const string expectedGridFileName = "cheese.nc";

            var config = new RgfConfig
            {
                Polygons = new List<IPolygon>(),
                PolFileName = expectedPolFileName
            };

            config.AddGridFileNames(expectedGridFileName);

            // When 
            IList<DelftIniCategory> categories = config.ToDelftIniCategories().ToList();

            // Then
            Assert.That(categories, Is.Not.Null,
                        "Expected the returned categories not to be null.");

            DelftIniCategory polygonCategory = categories.FirstOrDefault(c => c.Name == RgfConfig.PolygonsHeader);
            AssertValidCategory(polygonCategory,
                                RgfConfig.PolygonsHeader,
                                new Tuple<string, string>(RgfConfig.PolygonFileName, expectedPolFileName));

            DelftIniCategory batchCategory = categories.FirstOrDefault(c => c.Name == RgfConfig.BatchHeader);
            AssertValidCategory(batchCategory,
                                RgfConfig.BatchHeader,
                                new Tuple<string, string>(RgfConfig.BatchFileName, expectedGridFileName),
                                new Tuple<string, string>(RgfConfig.BatchGridType, RgfConfig.SepranGrid));
        }

        /// <summary>
        /// GIVEN a RgfConfig with null AdditionalGridPaths
        /// WHEN it is converted to DelftIniCategories
        /// THEN it throws an InvalidOperationException
        /// </summary>
        [Test]
        public void GivenARgfConfigWithNullAdditionalGridPaths_WhenItIsConvertedToDelftIniCategories_ThenItThrowsAnInvalidOperationException()
        {
            // Given
            var config = new RgfConfig()
            {
                AdditionalGeometryPaths = null
            };

            // When
            void testAction()
            {
                config.ToDelftIniCategories();
            }

            // Then
            Assert.Throws<InvalidOperationException>(testAction, "Expected a different exception:");
        }

        /// <summary>
        /// GIVEN a RgfConfig with a null GridFilePaths
        /// WHEN it is converted to DelftIniCategories
        /// THEN it throws an InvalidOperationException
        /// </summary>
        [Test]
        public void GivenARgfConfigWithANullGridFilePaths_WhenItIsConvertedToDelftIniCategories_ThenItThrowsAnInvalidOperationException()
        {
            // Given
            var config = new RgfConfig()
            {
                GridFileNames = null
            };

            // When
            void testAction()
            {
                config.ToDelftIniCategories();
            }

            // Then
            Assert.Throws<InvalidOperationException>(testAction, "Expected a different exception:");
        }

        /// <summary>
        /// GIVEN a RgfConfig with an AdditionalGeometryPath with an unknown extension
        /// WHEN it is converted to DelftIniCategories
        /// THEN it throws an InvalidOperationException
        /// </summary>
        [Test]
        public void GivenARgfConfigWithAnAdditionalGeometryPathWithAnUnknownExtension_WhenItIsConvertedToDelftIniCategories_ThenItThrowsAnInvalidOperationException()
        {
            // Given
            var config = new RgfConfig();

            config.AdditionalGeometryPaths.Add("InvalidExtension.cheese");

            // When
            void testAction()
            {
                config.ToDelftIniCategories();
            }

            // Then
            Assert.Throws<InvalidOperationException>(testAction, "Expected a different exception:");
        }

        /// <summary>
        /// GIVEN a RgfConfig
        /// AND a grid file name
        /// WHEN this grid file name is added to this RgfConfig
        /// THEN it is added to this RgfConfig with the correct format
        /// </summary>
        [TestCase("bacon.nc", RgfConfig.FMGridKeyword)]
        [TestCase("bacon.grd", RgfConfig.GrdKeyword)]
        public void GivenARgfConfigAndAGridFileName_WhenThisGridFileNameIsAddedToThisRgfConfig_ThenItIsAddedToThisRgfConfigWithTheCorrectFormat(string inputGridFileName, string expectedFormat)
        {
            // Given
            var config = new RgfConfig();

            // When
            config.AddGridFileNames(inputGridFileName);

            // Then
            Assert.That(config.GridFileNames, Is.Not.Null, "Expected GridFileNames is not null");
            Assert.That(config.GridFileNames, Has.Count.EqualTo(1), "Expected a single element in GridFileNames.");

            Tuple<string, string> elem = config.GridFileNames.First();
            string fileName = elem.Item1;
            string fileType = elem.Item2;

            Assert.That(fileName, Is.EqualTo(inputGridFileName),
                        "Expected the file name to be different:");
            Assert.That(fileType, Is.EqualTo(expectedFormat),
                        "Expected the file type to be different:");
        }

        /// <summary>
        /// GIVEN a RgfConfig
        /// AND a grid file name
        /// AND a format
        /// WHEN this grid file name is added to this RgfConfig
        /// THEN it is added to this RgfConfig correctly
        /// </summary>
        [TestCase("bacon.nc", RgfConfig.FMGridKeyword)]
        [TestCase("hamburger.nc", RgfConfig.GrdKeyword)]
        public void GivenARgfConfigAndAGridFileNameAndAFormat_WhenThisGridFileNameIsAddedToThisRgfConfig_ThenItIsAddedToThisRgfConfigCorrectly(string inputGridFileName, string inputFormat)
        {
            // Given
            var config = new RgfConfig();

            // When
            config.AddGridFileNames(new Tuple<string, string>(inputGridFileName, inputFormat));

            // Then
            Assert.That(config.GridFileNames, Is.Not.Null, "Expected GridFileNames is not null");
            Assert.That(config.GridFileNames, Has.Count.EqualTo(1), "Expected a single element in GridFileNames.");

            Tuple<string, string> elem = config.GridFileNames.First();
            string fileName = elem.Item1;
            string fileType = elem.Item2;

            Assert.That(fileName, Is.EqualTo(inputGridFileName),
                        "Expected the file name to be different:");
            Assert.That(fileType, Is.EqualTo(inputFormat),
                        "Expected the file type to be different:");
        }

        /// <summary>
        /// GIVEN a RgfConfig with a set of additional geometry paths
        /// WHEN it is converted to DelftIniCategories
        /// THEN it has the correct Geometry headers
        /// </summary>
        [TestCase("steak.ldb", "TEKAL")]
        [TestCase("steak.shp", "SHAPEFILE")]
        [TestCase("steak", "TEKAL")]
        public void GivenARgfConfigWithASetOfAdditionalGeometryPaths_WhenItIsConvertedToDelftIniCategories_ThenItHasTheCorrectGeometryHeaders(string additionalPath, string expectedFormat)
        {
            // Given
            var config = new RgfConfig();
            config.AdditionalGeometryPaths.Add(additionalPath);

            // When
            IList<DelftIniCategory> categories = config.ToDelftIniCategories().ToList();

            // Then
            Assert.That(categories, Is.Not.Null,
                        "Expected the returned categories not to be null.");

            DelftIniCategory geometryCategory = categories.FirstOrDefault(c => c.Name == RgfConfig.GeometryHeader);
            AssertValidCategory(geometryCategory, RgfConfig.GeometryHeader,
                                new Tuple<string, string>(RgfConfig.LandBoundaryFile, additionalPath),
                                new Tuple<string, string>(RgfConfig.LandBoundaryFormat, expectedFormat));
        }

        /// <summary>
        /// GIVEN a RgfConfig with a set of Grid file paths
        /// WHEN it is converted to DelftIniCategories
        /// THEN it has the correct Grid categories
        /// </summary>
        [TestCase("bacon.nc", RgfConfig.FMGridKeyword)]
        [TestCase("bacon.grd", RgfConfig.GrdKeyword)]
        public void GivenARgfConfigWithASetOfGridFilePaths_WhenItIsConvertedToDelftIniCategories_ThenItHasTheCorrectGridCategories(string gridFilePath, string gridFileFormat)
        {
            // Given
            var config = new RgfConfig();
            config.AddGridFileNames(new Tuple<string, string>(gridFilePath, gridFileFormat));

            // When
            IList<DelftIniCategory> categories = config.ToDelftIniCategories().ToList();

            // Then
            Assert.That(categories, Is.Not.Null,
                        "Expected the returned categories not to be null.");

            DelftIniCategory gridCategory = categories.FirstOrDefault(c => c.Name == RgfConfig.GridHeader);
            AssertValidCategory(gridCategory, RgfConfig.GridHeader,
                                new Tuple<string, string>(RgfConfig.GridFileName, gridFilePath),
                                new Tuple<string, string>(RgfConfig.GridType, gridFileFormat));
        }

        private static void AssertValidCategory(DelftIniCategory category,
                                                string headerName,
                                                params Tuple<string, string>[] expectedProperties)
        {
            Assert.That(category, Is.Not.Null,
                        $"Expected the returned categories to contain a {headerName} header.");

            Assert.That(category.Name, Is.EqualTo(headerName),
                        "Expected the category name to be different:");
            Assert.That(category.Properties, Is.Not.Null,
                        $"Expected the properties of the {headerName} header to not be null.");
            Assert.That(category.Properties, Has.Count.EqualTo(expectedProperties.Length),
                        $"Expected a different number of properties within {headerName} header:");

            foreach (Tuple<string, string> prop in expectedProperties)
            {
                DelftIniProperty propInCategory = category.Properties.FirstOrDefault(p => p.Name == prop.Item1);
                Assert.That(propInCategory, Is.Not.Null,
                            $"Expected a {prop.Item1} property to be in {headerName} header.");

                if (prop.Item2 != null)
                {
                    Assert.That(propInCategory.Value, Is.EqualTo(prop.Item2),
                                $"Expected the {prop.Item1} property in {headerName} header to have a different value:");
                }
            }
        }
    }
}