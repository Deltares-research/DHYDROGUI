using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Properties;
using log4net.Core;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class DelwaqNetCdfMapFileReaderTest
    {
        private const string salinityName = "Salinity";
        private const string eColiName = "EColi";
        private const string volumeName = "volume";
        private string path;

        [SetUp]
        public void SetUp()
        {
            path = TestHelper.GetTestFilePath(@"IO\deltashell_map.nc");
        }

        [Test]
        public void GivenADelwaqNcMapFileReader_WhenReadMetaDataIsCalled_ThenCorrectMetaDataIsCreated()
        {
            // When
            MapFileMetaData metaData = DelwaqNetCdfMapFileReader.ReadMetaData(path);

            // Then
            ValidateCounts(metaData);
            ValidateSubstances(metaData);
            ValidateTimes(metaData);
        }

        [Test]
        public void GivenADelwaqNcMapFileReader_WhenGetTimeSeriesDataIsCalledWithASpecifiedSegmentIndex_ThenCorrectValuesAreReturned()
        {
            // Given
            MapFileMetaData metaData = CreateMapFileMetaData();

            // When
            List<double> timeSeriesData = DelwaqNetCdfMapFileReader.GetTimeSeriesData(path,
                                                                                      metaData,
                                                                                      salinityName,
                                                                                      6);

            var expectedValues = new[]
            {
                5.0,
                7.880000114440918,
                10.760000228881836,
                13.640000343322754,
                16.520000457763672,
                19.399999618530273,
                22.280000686645508
            };

            // Then
            Assert.AreEqual(expectedValues, timeSeriesData,
                            "Values read from waq netCDF file were different than expected.");
        }

        [Test]
        public void GivenMetaDataWithoutExpectedSubstanceMapping_WhenGetTimeeriesDataIsCalled_ThenAnEmptyListIsReturned()
        {
            // Given
            MapFileMetaData metaData = CreateMapFileMetaData();
            metaData.SubstancesMapping.Clear();

            // When
            List<double> data = DelwaqNetCdfMapFileReader.GetTimeSeriesData(path,
                                                                            metaData,
                                                                            salinityName,
                                                                            metaData.NumberOfTimeSteps - 1);

            // Then
            Assert.That(data, Is.Empty,
                        "An empty collection was expected to be returned when substance is not found in the metadata.");
        }

        [Test]
        public void GivenMetaDataWithoutExpectedSubstanceMapping_WhenGetTimeStepDataIsCalled_ThenAnEmptyListIsReturned()
        {
            // Given
            MapFileMetaData metaData = CreateMapFileMetaData();
            metaData.SubstancesMapping.Clear();

            // When
            List<double> data = DelwaqNetCdfMapFileReader.GetTimeStepData(path,
                                                                          metaData,
                                                                          metaData.NumberOfTimeSteps - 1,
                                                                          salinityName);

            // Then
            Assert.That(data, Is.Empty,
                        "An empty collection was expected to be returned when substance is not found in the metadata.");
        }

        [Test]
        public void ReadMetaData_FromFileContainingOtherFaceDimensionName_ThenMetaDataCanBeRead()
        {
            path = TestHelper.GetTestFilePath(@"IO\deltashell_map_other_name.nc");

            // When
            MapFileMetaData metaData = DelwaqNetCdfMapFileReader.ReadMetaData(path);

            // Then
            Assert.That(metaData.NumberOfSegments, Is.EqualTo(16),
                        "Number of segments was different than expected.");
        }

        [Test]
        public void ReadMetaData_WhenFileDoesNotExist_ThenCorrectErrorIsGiven()
        {
            // Set-up
            path = "no_exist";
            MapFileMetaData metaData = null;

            // Action
            void TestAction()
            {
                metaData = DelwaqNetCdfMapFileReader.ReadMetaData(path);
            }

            // Assert
            IEnumerable<string> renderedErrorMessages = TestHelper.GetAllRenderedMessages(TestAction, Level.Error);
            string expectedMessage = string.Format(Resources.DelwaqNetCdfMapFileReader_Map_file_not_found, path);
            Assert.That(renderedErrorMessages, Contains.Item(expectedMessage));
            Assert.That(metaData, Is.Not.Null, "Meta data should not have been null.");
        }

        [Test]
        public void GetTimeStepData_WhenFileDoesNotExist_ThenCorrectErrorIsGiven()
        {
            // Set-up
            path = "no_exist";
            List<double> data = null;

            // Action
            void TestAction()
            {
                data = DelwaqNetCdfMapFileReader.GetTimeStepData(path, null, 0, "");
            }

            // Assert
            IEnumerable<string> renderedErrorMessages = TestHelper.GetAllRenderedMessages(TestAction, Level.Error);
            string expectedMessage = string.Format(Resources.DelwaqNetCdfMapFileReader_Map_file_not_found, path);
            Assert.That(renderedErrorMessages, Contains.Item(expectedMessage));
            Assert.That(data, Is.Not.Null, "Empty list should have been returned.");
            Assert.That(data, Is.Empty, "Empty list should have been returned.");
        }

        [Test]
        public void GetTimeSeriesData_WhenFileDoesNotExist_ThenCorrectErrorIsGiven()
        {
            // Set-up
            path = "no_exist";
            List<double> data = null;

            // Action
            void TestAction()
            {
                data = DelwaqNetCdfMapFileReader.GetTimeSeriesData(path, null, "", 0);
            }

            // Assert
            IEnumerable<string> renderedErrorMessages = TestHelper.GetAllRenderedMessages(TestAction, Level.Error);
            string expectedMessage = string.Format(Resources.DelwaqNetCdfMapFileReader_Map_file_not_found, path);
            Assert.That(renderedErrorMessages, Contains.Item(expectedMessage));
            Assert.That(data, Is.Not.Null, "Empty list should have been returned.");
            Assert.That(data, Is.Empty, "Empty list should have been returned.");
        }

        [TestCase(0, 5.0)]
        [TestCase(1, 7.880000114440918)]
        [TestCase(2, 10.760000228881836)]
        [TestCase(3, 13.640000343322754)]
        [TestCase(4, 16.520000457763672)]
        [TestCase(5, 19.399999618530273)]
        [TestCase(6, 22.280000686645508)]
        public void GivenADelwaqNcMapFileReader_WhenGetTimeStepDataIsCalledWithASpecifiedTimeStepIndex_ThenCorrectValuesAreReturned(int timeStepIndex, double expectedValue)
        {
            MapFileMetaData metaData = CreateMapFileMetaData();

            // When
            List<double> timeStepData = DelwaqNetCdfMapFileReader.GetTimeStepData(path,
                                                                                  metaData,
                                                                                  timeStepIndex,
                                                                                  salinityName);

            double[] expectedValues =
            {
                5.0,
                5.0,
                5.0,
                5.0,
                5.0,
                5.0,
                expectedValue,
                5.0,
                5.0,
                5.0,
                5.0,
                5.0,
                5.0,
                5.0,
                5.0,
                5.0
            };

            // Then
            Assert.That(timeStepData, Is.EqualTo(expectedValues),
                        "Values read from waq netCDF file were different than expected.");
        }

        [TestCase(0, 5.0)]
        [TestCase(1, 7.880000114440918)]
        [TestCase(2, 10.760000228881836)]
        [TestCase(3, 13.640000343322754)]
        [TestCase(4, 16.520000457763672)]
        [TestCase(5, 19.399999618530273)]
        [TestCase(6, 22.280000686645508)]
        public void GivenADelwaqNcMapFileReader_WhenGetTimeStepDataIsCalledWithSpecifiedTimeAndSegmentIndex_ThenCorrectValueIsReturned(int timeStepIndex, double expectedValue)
        {
            MapFileMetaData metaData = CreateMapFileMetaData();

            // When
            List<double> timeStepData = DelwaqNetCdfMapFileReader.GetTimeStepData(path,
                                                                                  metaData,
                                                                                  timeStepIndex,
                                                                                  salinityName,
                                                                                  6);

            // Then
            Assert.That(timeStepData.Single(), Is.EqualTo(expectedValue),
                        "Value read from waq netCDF file is different than expected.");
        }

        private static void ValidateCounts(MapFileMetaData metaData)
        {
            Assert.AreEqual(7, metaData.NumberOfTimeSteps,
                            "Number of time steps in the meta data were different than expected.");
            Assert.AreEqual(7, metaData.Times.Count,
                            "Number of times in the meta data were different than expected.");
            Assert.AreEqual(16, metaData.NumberOfSegments,
                            "Number of segments in the meta data were different than expected.");
            Assert.AreEqual(3, metaData.NumberOfSubstances,
                            "Number of substances in the meta data were different than expected.");
            Assert.AreEqual(3, metaData.Substances.Count,
                            "Number of substances in the meta data were different than expected.");
            Assert.AreEqual(3, metaData.SubstancesMapping.Count,
                            "Number of mapped substances in the meta data were different than expected.");
        }

        private static void ValidateSubstances(MapFileMetaData metaData)
        {
            IList<string> substances = metaData.Substances;
            Assert.That(substances, Contains.Item(salinityName),
                        "Substances did not contain expected substance.");
            Assert.That(substances, Contains.Item(eColiName),
                        "Substances did not contain expected substance.");
            Assert.That(substances, Contains.Item(volumeName),
                        "Substances did not contain expected substance.");

            IDictionary<string, string> mapping = metaData.SubstancesMapping;
            Assert.That(mapping[salinityName], Is.EqualTo("mesh2d_Salinity"),
                        $"Mapping of {salinityName} to the variable name was different than expected.");
            Assert.That(mapping[eColiName], Is.EqualTo("mesh2d_EColi"),
                        $"Mapping of {eColiName} to the variable name was different than expected.");
            Assert.That(mapping[volumeName], Is.EqualTo("mesh2d_volume"),
                        $"Mapping of {volumeName} to the variable name was different than expected.");
        }

        private static void ValidateTimes(MapFileMetaData metaData)
        {
            IEnumerable<DateTime> expectedTimes = CreateTimes();
            Assert.AreEqual(expectedTimes, metaData.Times,
                            "Incorrect Date Times were read from file");
        }

        private static IEnumerable<DateTime> CreateTimes()
        {
            var referenceDate = new DateTime(2001, 1, 1);
            for (var i = 0; i < 7; i++)
            {
                yield return referenceDate.AddHours(4 * i);
            }
        }

        private static MapFileMetaData CreateMapFileMetaData()
        {
            var metaData = new MapFileMetaData
            {
                SubstancesMapping = new Dictionary<string, string> {{salinityName, $"mesh2d_{salinityName}"}},
                NumberOfTimeSteps = 7,
                NumberOfSegments = 16
            };
            return metaData;
        }
    }
}