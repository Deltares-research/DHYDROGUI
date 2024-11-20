using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Converters;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Exporters;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Files.Evaporation;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.FileWriters;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.IO.Exporters
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class EvaporationExporterTest
    {
        [Test]
        [TestCaseSource(nameof(ConstructorArgNullCases))]
        public void Constructor_ArgNull_ThrowsArgumentNullException(EvaporationFileWriter evaporationFileWriter,
                                                                    EvaporationFileCreator evaporationFileCreator,
                                                                    EvaporationFileNameConverter evaporationFileNameConverter,
                                                                    IOEvaporationMeteoDataSourceConverter meteoDataSourceConverter,
                                                                    string expParamName)
        {
            // Call
            void Call() => new EvaporationExporter(evaporationFileWriter,
                                                   evaporationFileCreator,
                                                   evaporationFileNameConverter,
                                                   meteoDataSourceConverter);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException
                                    .With.Property(nameof(ArgumentException.ParamName)).EqualTo(expParamName));
        }

        [Test]
        [TestCaseSource(nameof(ExportToFileArgNullCases))]
        public void Export_ToFile_ArgNull_ThrowsArgumentNullException(EvaporationMeteoData evaporationMeteoData,
                                                                      FileInfo file,
                                                                      string expParamName)
        {
            // Setup
            EvaporationExporter exporter = CreateEvaporationExporter();

            // Call
            void Call() => exporter.Export(evaporationMeteoData, file);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException
                                    .With.Property(nameof(ArgumentException.ParamName)).EqualTo(expParamName));
        }

        [Test]
        [TestCaseSource(nameof(ExportToDirectoryArgNullCases))]
        public void Export_ToDirectory_ArgNull_ThrowsArgumentNullException(EvaporationMeteoData evaporationMeteoData,
                                                                           DirectoryInfo directory,
                                                                           string expParamName)
        {
            // Setup
            EvaporationExporter exporter = CreateEvaporationExporter();

            // Call
            void Call() => exporter.Export(evaporationMeteoData, directory);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException
                                    .With.Property(nameof(ArgumentException.ParamName)).EqualTo(expParamName));
        }

        [Test]
        [TestCase(MeteoDataSource.UserDefined)]
        [TestCase(MeteoDataSource.LongTermAverage)]
        [TestCase(MeteoDataSource.GuidelineSewerSystems)]
        public void Export_ToFile_ExportedFileExists(MeteoDataSource meteoDataSource)
        {
            // Setup
            EvaporationExporter exporter = CreateEvaporationExporter();

            var evaporationMeteoData = new EvaporationMeteoData { SelectedMeteoDataSource = meteoDataSource };

            using (var temp = new TemporaryDirectory())
            {
                var exportFile = new FileInfo(Path.Combine(temp.Path, "evaporation.evp"));

                // Call
                exporter.Export(evaporationMeteoData, exportFile);

                // Assert
                Assert.That(exportFile, Does.Exist);
            }
        }

        [Test]
        [TestCase(MeteoDataSource.UserDefined, "default.evp")]
        [TestCase(MeteoDataSource.LongTermAverage, "EVAPOR.GEM")]
        [TestCase(MeteoDataSource.GuidelineSewerSystems, "EVAPOR.PLV")]
        public void Export_ToDirectory_ExportedFileExists(MeteoDataSource meteoDataSource, string expFileName)
        {
            // Setup
            EvaporationExporter exporter = CreateEvaporationExporter();

            var evaporationMeteoData = new EvaporationMeteoData { SelectedMeteoDataSource = meteoDataSource };

            using (var temp = new TemporaryDirectory())
            {
                var exportDirectory = new DirectoryInfo(temp.Path);

                // Call
                exporter.Export(evaporationMeteoData, exportDirectory);

                // Assert
                Assert.That(Path.Combine(temp.Path, expFileName), Does.Exist);
            }
        }

        [Test]
        public void Export_ToFile_EvaporationMeteoData_GlobalUserDefined_ExportedFileIsCorrect()
        {
            // Setup
            EvaporationExporter exporter = CreateEvaporationExporter();

            var evaporationMeteoData = new EvaporationMeteoData
            {
                DataDistributionType = MeteoDataDistributionType.Global,
                SelectedMeteoDataSource = MeteoDataSource.UserDefined
            };

            SetTimes(evaporationMeteoData,
                     new DateTime(2014, 1, 1),
                     new DateTime(2014, 1, 2),
                     new DateTime(2014, 1, 3));
            SetValues(evaporationMeteoData,
                      1.234,
                      2.345,
                      3.456);

            using (var temp = new TemporaryDirectory())
            {
                var exportFile = new FileInfo(Path.Combine(temp.Path, "evaporation.evp"));

                // Call
                exporter.Export(evaporationMeteoData, exportFile);

                // Assert
                Assert.That(exportFile, Does.Exist);
                string[] lines = GetLines(exportFile);
                string[] expLines =
                {
                    "*Verdampingsfile",
                    "*Meteo data: evaporation intensity in mm/day",
                    "*First record: start date, data in mm/day",
                    "*Datum (year month day), verdamping (mm/dag) voor elk weerstation",
                    "*jaar maand dag verdamping[mm]",
                    "2014 01 01 1.234",
                    "2014 01 02 2.345",
                    "2014 01 03 3.456"
                };
                Assert.That(lines, Is.EqualTo(expLines));
            }
        }

        [Test]
        public void Export_ToFile_Evaporation_PerMeteoStationUserDefined_ExportedFileIsCorrect()
        {
            // Setup
            EvaporationExporter exporter = CreateEvaporationExporter();

            var evaporationMeteoData = new EvaporationMeteoData
            {
                DataDistributionType = MeteoDataDistributionType.PerStation,
                SelectedMeteoDataSource = MeteoDataSource.UserDefined
            };

            SetTimes(evaporationMeteoData,
                     new DateTime(2014, 1, 1),
                     new DateTime(2014, 1, 2),
                     new DateTime(2014, 1, 3));
            SetMeteoStations(evaporationMeteoData,
                             "station1",
                             "station2");
            SetValues(evaporationMeteoData,
                      1.234, 4.567,
                      2.345, 5.678,
                      3.456, 6.789);

            using (var temp = new TemporaryDirectory())
            {
                var exportFile = new FileInfo(Path.Combine(temp.Path, "evaporation.evp"));

                // Call
                exporter.Export(evaporationMeteoData, exportFile);

                // Assert
                Assert.That(exportFile, Does.Exist);
                string[] lines = GetLines(exportFile);
                string[] expLines =
                {
                    "*Verdampingsfile",
                    "*Meteo data: evaporation intensity in mm/day",
                    "*First record: start date, data in mm/day",
                    "*Datum (year month day), verdamping (mm/dag) voor elk weerstation",
                    "*jaar maand dag verdamping[mm]",
                    "2014 01 01 1.234 4.567",
                    "2014 01 02 2.345 5.678",
                    "2014 01 03 3.456 6.789"
                };
                Assert.That(lines, Is.EqualTo(expLines));
            }
        }

        [Test]
        public void Export_ToFile_EvaporationMeteoData_GlobalLongTermAverage_ExportedFileIsCorrect()
        {
            // Setup
            EvaporationExporter exporter = CreateEvaporationExporter();

            var evaporationMeteoData = new EvaporationMeteoData
            {
                DataDistributionType = MeteoDataDistributionType.Global,
                SelectedMeteoDataSource = MeteoDataSource.LongTermAverage
            };

            evaporationMeteoData.Data.Clear();

            SetTimes(evaporationMeteoData,
                     new DateTime(2014, 1, 1),
                     new DateTime(2014, 1, 2),
                     new DateTime(2014, 1, 3));
            SetValues(evaporationMeteoData,
                      1.234,
                      2.345,
                      3.456);

            using (var temp = new TemporaryDirectory())
            {
                var exportFile = new FileInfo(Path.Combine(temp.Path, "evaporation.evp"));

                // Call
                exporter.Export(evaporationMeteoData, exportFile);

                // Assert
                Assert.That(exportFile, Does.Exist);
                string[] lines = GetLines(exportFile);
                string[] expLines =
                {
                    "*Longtime average",
                    "*year column is dummy, year 'value' should be fixed 0000",
                    "0000 01 01 1.234",
                    "0000 01 02 2.345",
                    "0000 01 03 3.456"
                };
                Assert.That(lines, Is.EqualTo(expLines));
            }
        }

        [Test]
        public void Export_ToFile_EvaporationMeteoData_GlobalGuidelineSewerSystems_ExportedFileIsCorrect()
        {
            // Setup
            EvaporationExporter exporter = CreateEvaporationExporter();

            var evaporationMeteoData = new EvaporationMeteoData
            {
                DataDistributionType = MeteoDataDistributionType.Global,
                SelectedMeteoDataSource = MeteoDataSource.GuidelineSewerSystems
            };

            evaporationMeteoData.Data.Clear();

            SetTimes(evaporationMeteoData,
                     new DateTime(2014, 1, 1),
                     new DateTime(2014, 1, 2),
                     new DateTime(2014, 1, 3));
            SetValues(evaporationMeteoData,
                      1.234,
                      2.345,
                      3.456);

            using (var temp = new TemporaryDirectory())
            {
                var exportFile = new FileInfo(Path.Combine(temp.Path, "evaporation.evp"));

                // Call
                exporter.Export(evaporationMeteoData, exportFile);

                // Assert
                Assert.That(exportFile, Does.Exist);
                string[] lines = GetLines(exportFile);
                string[] expLines =
                {
                    "*Verdampingsfile",
                    "*Meteo data: Evaporation stations; for each station: evaporation intensity in mm",
                    "*First record: start date, data in mm/day",
                    "*Datum (year month day), verdamping (mm/dag) voor elk weerstation",
                    "*jaar maand dag verdamping[mm]",
                    "0000 01 01 1.234",
                    "0000 01 02 2.345",
                    "0000 01 03 3.456"
                };
                Assert.That(lines, Is.EqualTo(expLines));
            }
        }

        private static IEnumerable<TestCaseData> ConstructorArgNullCases()
        {
            yield return new TestCaseData(null,
                                          new EvaporationFileCreator(),
                                          new EvaporationFileNameConverter(),
                                          new IOEvaporationMeteoDataSourceConverter(),
                                          "evaporationFileWriter");
            yield return new TestCaseData(new EvaporationFileWriter(),
                                          null,
                                          new EvaporationFileNameConverter(),
                                          new IOEvaporationMeteoDataSourceConverter(),
                                          "evaporationFileCreator");
            yield return new TestCaseData(new EvaporationFileWriter(),
                                          new EvaporationFileCreator(),
                                          null,
                                          new IOEvaporationMeteoDataSourceConverter(),
                                          "evaporationFileNameConverter");
            yield return new TestCaseData(new EvaporationFileWriter(),
                                          new EvaporationFileCreator(),
                                          new EvaporationFileNameConverter(),
                                          null,
                                          "meteoDataSourceConverter");
        }

        private static IEnumerable<TestCaseData> ExportToFileArgNullCases()
        {
            yield return new TestCaseData(null,
                                          new FileInfo("some_path"),
                                          "evaporationMeteoData");
            yield return new TestCaseData(new EvaporationMeteoData(),
                                          null,
                                          "file");
        }

        private static IEnumerable<TestCaseData> ExportToDirectoryArgNullCases()
        {
            yield return new TestCaseData(null,
                                          new DirectoryInfo("some_path"),
                                          "evaporationMeteoData");
            yield return new TestCaseData(new EvaporationMeteoData(),
                                          null,
                                          "directory");
        }

        private static EvaporationExporter CreateEvaporationExporter()
        {
            return new EvaporationExporter(new EvaporationFileWriter(),
                                           new EvaporationFileCreator(),
                                           new EvaporationFileNameConverter(),
                                           new IOEvaporationMeteoDataSourceConverter());
        }

        private static string[] GetLines(FileInfo file)
        {
            return File.ReadAllLines(file.FullName).Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
        }

        private static void SetTimes(MeteoData meteoData, params DateTime[] values)
        {
            meteoData.MeteoDataDistributed.Data.Arguments[0].SetValues(values);
        }

        private static void SetMeteoStations(MeteoData meteoData, params string[] values)
        {
            meteoData.MeteoDataDistributed.Data.Arguments[1].SetValues(values);
        }

        private static void SetValues(MeteoData meteoData, params double[] values)
        {
            meteoData.MeteoDataDistributed.Data.Components[0].SetValues(values);
        }
    }
}