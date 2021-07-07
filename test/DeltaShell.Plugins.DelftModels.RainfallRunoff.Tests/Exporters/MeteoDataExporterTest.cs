using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Exporters;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Exporters
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class MeteoDataExporterTest
    {
        [Test]
        public void ExportEvaporation()
        {
            using (var temp = new TemporaryDirectory())
            {
                var meteoData = new MeteoData
                {
                    Name = RainfallRunoffModelDataSet.EvaporationName,
                    DataDistributionType = MeteoDataDistributionType.Global
                };
            
                SetTimes(meteoData, new DateTime(2014,1,1), new DateTime(2014,1,2), new DateTime(2014,1,3));
                SetValues(meteoData, 1.0, 2.0, 3.0);

                string file = Path.Combine(temp.Path, "some_file_name.bui");
                var exporter = new MeteoDataExporter();
                
                // Call
                bool result = exporter.Export(meteoData, file);
                
                // Assert
                Assert.That(result, Is.True);
                Assert.That(file, Does.Exist);

                string[][] lines = GetLastLines(file, 3);
                AssertLineEqualTo(lines[0], "2014", "01", "01", "1.00");
                AssertLineEqualTo(lines[1], "2014", "01", "02", "2.00");
                AssertLineEqualTo(lines[2], "2014", "01", "03", "3.00");
            }
        }

        [Test]
        public void ExportEvaporationMultipleStations()
        {
            using (var temp = new TemporaryDirectory())
            {
                var meteoData = new MeteoData
                {
                    Name = RainfallRunoffModelDataSet.EvaporationName,
                    DataDistributionType = MeteoDataDistributionType.PerStation
                };
            
                SetTimes(meteoData, new DateTime(2014, 1, 1), new DateTime(2014, 1, 2), new DateTime(2014, 1, 3));
                SetNames(meteoData, "station1", "station2");
                SetValues(meteoData, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0);

                string file = Path.Combine(temp.Path, "some_file_name.bui");
                var exporter = new MeteoDataExporter();
                
                // Call
                bool result = exporter.Export(meteoData, file);
                
                // Assert
                Assert.That(result, Is.True);
                Assert.That(file, Does.Exist);

                string[][] lines = GetLastLines(file, 3);
                AssertLineEqualTo(lines[0], "2014", "01", "01", "1.00", "2.00");
                AssertLineEqualTo(lines[1], "2014", "01", "02", "3.00", "4.00");
                AssertLineEqualTo(lines[2], "2014", "01", "03", "5.00", "6.00");
            }
        }

        [Test]
        public void ExportTemperatureData()
        {
            using (var temp = new TemporaryDirectory())
            {
                var meteoData = new MeteoData
                {
                    Name = RainfallRunoffModelDataSet.TemperatureName,
                    DataDistributionType = MeteoDataDistributionType.Global
                };

                SetTimes(meteoData, new DateTime(2014, 1, 1, 0, 0, 0), new DateTime(2014, 1, 1, 2, 10, 0), new DateTime(2014, 1, 1, 4, 20, 0));
                SetValues(meteoData, 1.0, 2.0, 3.0);
            
                string file = Path.Combine(temp.Path, "some_file_name.bui");
                var exporter = new MeteoDataExporter();
                
                // Call
                bool result = exporter.Export(meteoData, file);
                
                // Assert
                Assert.That(result, Is.True);
                Assert.That(file, Does.Exist);

                string[][] lines = GetLastLines(file, 4);
                AssertLineEqualTo(lines[0], "2014", "01", "01", "00", "00", "00", "00", "04", "20", "00");
                AssertLineEqualTo(lines[1], "1.00");
                AssertLineEqualTo(lines[2], "2.00");
                AssertLineEqualTo(lines[3], "3.00");
            }
        }

        [Test]
        public void ExportPrecipitationData()
        {
            using (var temp = new TemporaryDirectory())
            {
                var meteoData = new MeteoData
                {
                    Name = RainfallRunoffModelDataSet.PrecipitationName,
                    DataDistributionType = MeteoDataDistributionType.Global
                };

                SetTimes(meteoData, new DateTime(2014, 1, 1, 0, 0, 0), new DateTime(2014, 1, 1, 2, 10, 0), new DateTime(2014, 1, 1, 4, 20, 0));
                SetValues(meteoData, 1.0, 2.0, 3.0);

                string file = Path.Combine(temp.Path, "some_file_name.bui");
                var exporter = new MeteoDataExporter();
                
                // Call
                bool result = exporter.Export(meteoData, file);
                
                // Assert
                Assert.That(result, Is.True);
                Assert.That(file, Does.Exist);
                
                string[][] lines = GetLastLines(file, 4);
                AssertLineEqualTo(lines[0], "2014", "01", "01", "00", "00", "00", "00", "04", "20", "00");
                AssertLineEqualTo(lines[1], "1.00");
                AssertLineEqualTo(lines[2], "2.00");
                AssertLineEqualTo(lines[3], "3.00");
            }
        }

        [Test]
        public void Export_Precipitation_PerMeteoStation_ExportsCorrectFile()
        {
            using (var temp = new TemporaryDirectory())
            {
                // Setup
                var meteoData = new MeteoData
                {
                    Name = RainfallRunoffModelDataSet.PrecipitationName,
                    DataDistributionType = MeteoDataDistributionType.PerStation
                };
                
                SetTimes(meteoData, new DateTime(2021, 1, 1), new DateTime(2021, 1, 2), new DateTime(2021, 1, 3));
                SetNames(meteoData, "some_meteo_station_1", "some_meteo_station_2");
                SetValues(meteoData, 0.0123, 1.1234, 2.2345, 3.3456, 4.4567, 5.5678);

                string file = Path.Combine(temp.Path, "some_file_name.bui");
                var exporter = new MeteoDataExporter();
                
                // Call
                bool result = exporter.Export(meteoData, file);
                
                // Assert
                Assert.That(result, Is.True);
                Assert.That(file, Does.Exist);
                
                string[][] lines = GetLastLines(file, 4);
                AssertLineEqualTo(lines[0], "2021", "01", "01", "00", "00", "00", "02", "00", "00", "00");
                AssertLineEqualTo(lines[1], "0.012", "1.123");
                AssertLineEqualTo(lines[2], "2.235", "3.346");
                AssertLineEqualTo(lines[3], "4.457", "5.568");
            }
        }
        
        private static string[][] GetLastLines(string path, int n)
        {
            string[] allLines = File.ReadAllLines(path).Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
            IEnumerable<string> lastNLines = allLines.Skip(allLines.Length - n);
            string[][] splitLines = lastNLines.Select(l => l.Split(new char[0], StringSplitOptions.RemoveEmptyEntries)).ToArray();
            
            return splitLines ;
        }

        private static void AssertLineEqualTo(string[] line, params string[] values)
        {
            Assert.That(line, Is.EqualTo(values));
        }

        private static void SetTimes(MeteoData meteoData, params DateTime[] values)
        {
            meteoData.MeteoDataDistributed.Data.Arguments[0].SetValues(values);
        }
        
        private static void SetNames(MeteoData meteoData, params string[] values)
        {
            meteoData.MeteoDataDistributed.Data.Arguments[1].SetValues(values);
        }
        
        private static void SetValues(MeteoData meteoData, params double[] values)
        {
            meteoData.MeteoDataDistributed.Data.Components[0].SetValues(values);
        }
    }
}