using System;
using System.Collections.Generic;
using System.Linq;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.Plugins.FMSuite.Common.Gui.RgfGrid;
using GeoAPI.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.RgfGrid
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
        /// WHEN it is converted to IniData
        /// THEN it has the correct FileInformation header
        /// </summary>
        [Test]
        public void GivenANewRgfConfig_WhenItIsConvertedToIniData_ThenItHasTheCorrectFileInformationHeader()
        {
            // Given
            var config = new RgfConfig();

            // When
            var iniData = config.ToIniData();

            // Then
            Assert.That(iniData, Is.Not.Null, "Expected the returned INI data not to be null.");
            Assert.That(iniData.Sections, Has.Count.EqualTo(1), "Expected INI data with a single section to be returned.");

            IniSection section = iniData.Sections.First();
            AssertValidSection(section, RgfConfig.FileInformationHeader,
                               new Tuple<string, string>(RgfConfig.FileGeneratedBy, null),
                               new Tuple<string, string>(RgfConfig.FileCreationData, null),
                               new Tuple<string, string>(RgfConfig.FileVersion, null));
        }

        /// <summary>
        /// GIVEN a RgfConfig without a polygon file name and polygons
        /// WHEN it is converted to IniData
        /// THEN it has no Polygon section
        /// AND it has no Batch section
        /// </summary>
        [Test]
        public void GivenARgfConfigWithoutAPolygonFileNameAndPolygons_WhenItIsConvertedToIniData_ThenItHasNoPolygonSectionAndItHasNoBatchSection()
        {
            // Given
            var config = new RgfConfig
            {
                Polygons = null,
                PolFileName = null
            };

            // When 
            var iniData = config.ToIniData();

            // Then
            Assert.That(iniData, Is.Not.Null,
                        "Expected the returned INI data not to be null.");

            Assert.That(iniData.ContainsSection(RgfConfig.PolygonsHeader), Is.False,
                        $"Expected no section with the {RgfConfig.PolygonsHeader} header.");
            Assert.That(iniData.ContainsSection(RgfConfig.BatchHeader), Is.False,
                        $"Expected no section with the {RgfConfig.BatchHeader} header.");
        }

        /// <summary>
        /// GIVEN a RgfConfig with a polygon file name and polygons
        /// WHEN it is converted to IniData
        /// THEN it has the correct Polygon section
        /// AND it has the correct Batch section
        /// </summary>
        [Test]
        public void GivenARgfConfigWithAPolygonFileNameAndPolygons_WhenItIsConvertedToIniData_ThenItHasTheCorrectPolygonSectionAndItHasTheCorrectBatchSection()
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
            var iniData = config.ToIniData();

            // Then
            Assert.That(iniData, Is.Not.Null,
                        "Expected the returned INI data not to be null.");

            IniSection polygonSection = iniData.FindSection(RgfConfig.PolygonsHeader);
            AssertValidSection(polygonSection,
                                RgfConfig.PolygonsHeader,
                                new Tuple<string, string>(RgfConfig.PolygonFileName, expectedPolFileName));

            IniSection batchSection = iniData.FindSection(RgfConfig.BatchHeader);
            AssertValidSection(batchSection,
                               RgfConfig.BatchHeader,
                               new Tuple<string, string>(RgfConfig.BatchFileName, expectedGridFileName),
                               new Tuple<string, string>(RgfConfig.BatchGridType, RgfConfig.SepranGrid));
        }

        /// <summary>
        /// GIVEN a RgfConfig with null AdditionalGridPaths
        /// WHEN it is converted to IniData
        /// THEN it throws an InvalidOperationException
        /// </summary>
        [Test]
        public void GivenARgfConfigWithNullAdditionalGridPaths_WhenItIsConvertedToIniData_ThenItThrowsAnInvalidOperationException()
        {
            // Given
            var config = new RgfConfig() {AdditionalGeometryPaths = null};

            // When
            void testAction()
            {
                config.ToIniData();
            }

            // Then
            Assert.Throws<InvalidOperationException>(testAction, "Expected a different exception:");
        }

        /// <summary>
        /// GIVEN a RgfConfig with a null GridFilePaths
        /// WHEN it is converted to IniData
        /// THEN it throws an InvalidOperationException
        /// </summary>
        [Test]
        public void GivenARgfConfigWithANullGridFilePaths_WhenItIsConvertedToIniData_ThenItThrowsAnInvalidOperationException()
        {
            // Given
            var config = new RgfConfig() {GridFileNames = null};

            // When
            void testAction()
            {
                config.ToIniData();
            }

            // Then
            Assert.Throws<InvalidOperationException>(testAction, "Expected a different exception:");
        }

        /// <summary>
        /// GIVEN a RgfConfig with an AdditionalGeometryPath with an unknown extension
        /// WHEN it is converted to IniData
        /// THEN it throws an InvalidOperationException
        /// </summary>
        [Test]
        public void GivenARgfConfigWithAnAdditionalGeometryPathWithAnUnknownExtension_WhenItIsConvertedToIniData_ThenItThrowsAnInvalidOperationException()
        {
            // Given
            var config = new RgfConfig();

            config.AdditionalGeometryPaths.Add("InvalidExtension.cheese");

            // When
            void testAction()
            {
                config.ToIniData();
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
        /// WHEN it is converted to IniData
        /// THEN it has the correct Geometry headers
        /// </summary>
        [TestCase("steak.ldb", "TEKAL")]
        [TestCase("steak.shp", "SHAPEFILE")]
        [TestCase("steak", "TEKAL")]
        public void GivenARgfConfigWithASetOfAdditionalGeometryPaths_WhenItIsConvertedToIniData_ThenItHasTheCorrectGeometryHeaders(string additionalPath, string expectedFormat)
        {
            // Given
            var config = new RgfConfig();
            config.AdditionalGeometryPaths.Add(additionalPath);

            // When
            var iniData = config.ToIniData();

            // Then
            Assert.That(iniData, Is.Not.Null,
                        "Expected the returned INI data not to be null.");

            IniSection geometrySection = iniData.FindSection(RgfConfig.GeometryHeader);
            AssertValidSection(geometrySection, RgfConfig.GeometryHeader,
                                new Tuple<string, string>(RgfConfig.LandBoundaryFile, additionalPath),
                                new Tuple<string, string>(RgfConfig.LandBoundaryFormat, expectedFormat));
        }

        /// <summary>
        /// GIVEN a RgfConfig with a set of Grid file paths
        /// WHEN it is converted to IniData
        /// THEN it has the correct Grid sections
        /// </summary>
        [TestCase("bacon.nc", RgfConfig.FMGridKeyword)]
        [TestCase("bacon.grd", RgfConfig.GrdKeyword)]
        public void GivenARgfConfigWithASetOfGridFilePaths_WhenItIsConvertedToIniData_ThenItHasTheCorrectGridSections(string gridFilePath, string gridFileFormat)
        {
            // Given
            var config = new RgfConfig();
            config.AddGridFileNames(new Tuple<string, string>(gridFilePath, gridFileFormat));

            // When
            var iniData = config.ToIniData();

            // Then
            Assert.That(iniData, Is.Not.Null,
                        "Expected the returned INI data not to be null.");

            IniSection gridSection = iniData.FindSection(RgfConfig.GridHeader);
            AssertValidSection(gridSection, RgfConfig.GridHeader,
                                new Tuple<string, string>(RgfConfig.GridFileName, gridFilePath),
                                new Tuple<string, string>(RgfConfig.GridType, gridFileFormat));
        }

        private static void AssertValidSection(IniSection section,
                                               string headerName,
                                               params Tuple<string, string>[] expectedProperties)
        {
            Assert.That(section, Is.Not.Null,
                        $"Expected the returned sections to contain a {headerName} header.");

            Assert.That(section.Name, Is.EqualTo(headerName),
                        "Expected the section name to be different:");
            Assert.That(section.Properties, Is.Not.Null,
                        $"Expected the properties of the {headerName} header to not be null.");
            Assert.That(section.Properties, Has.Count.EqualTo(expectedProperties.Length),
                        $"Expected a different number of properties within {headerName} header:");

            foreach (Tuple<string, string> prop in expectedProperties)
            {
                IniProperty propInSection = section.FindProperty(prop.Item1);
                Assert.That(propInSection, Is.Not.Null,
                            $"Expected a {prop.Item1} property to be in {headerName} header.");

                if (prop.Item2 != null)
                {
                    Assert.That(propInSection.Value, Is.EqualTo(prop.Item2),
                                $"Expected the {prop.Item1} property in {headerName} header to have a different value:");
                }
            }
        }
    }
}