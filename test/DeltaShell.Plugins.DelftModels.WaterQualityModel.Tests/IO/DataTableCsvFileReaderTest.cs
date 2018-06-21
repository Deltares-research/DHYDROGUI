using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Properties;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO
{
    [TestFixture]
    public class DataTableCsvFileReaderTest
    {
        private readonly string timeBlockCsvPath =
            TestHelper.GetTestFilePath(Path.Combine("IO", "DataTables", "timeBlock.csv"));
        private readonly string timeBlockCapitalsCsvPath =
            TestHelper.GetTestFilePath(Path.Combine("IO", "DataTables", "timeBlock_capitals.csv"));
        private readonly string[] timeBlockCsvSubstances = { "SubA", "SubB" };
        private readonly string[] timeBlockCsvLocations = { "locA", "locB", "locC", "locD", "locE" };

        private readonly string timeLinearCsvPath =
            TestHelper.GetTestFilePath(Path.Combine("IO", "DataTables", "timeLinear.csv"));

        private readonly string unorderedTimeLinearCsvPath =
            TestHelper.GetTestFilePath(Path.Combine("IO", "DataTables", "unorderedTimeLinear.csv"));

        private readonly string[] timeLinearCsvSubstances = { "SubA", "SubB", "SubC", "SubD" };
        private readonly string[] timeLinearCsvLocations = { "locA", "locB", "locC" };

        [Test]
        [Explicit("This test can be used to generate test data files for tests in this suite.")]
        public void GenerateDataTableTestData()
        {
            CreateTimeBlockCsvFile();
            CreateTimeLinearCsvFile();
            CreateUnorderedTimeLinearCsvFile();
        }

        private void CreateTimeBlockCsvFile()
        {
            var random = new Random(25032015);

            using (var writer = new StringWriter(new StringBuilder()))
            {
                writer.WriteLine("timeBlock,location,substance,value");
                foreach (var location in timeBlockCsvLocations)
                {
                    foreach (var substance in timeBlockCsvSubstances)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            writer.WriteLine("{0},{1},{2},{3}",
                                new DateTime(2015, 3, 1 + i, 0, 0, 0).ToString("yyyy-MM-dd hh:mm:ss"),
                                location,
                                substance,
                                (3.4 * random.NextDouble() + 1.2).ToString(CultureInfo.InvariantCulture));
                        }
                    }
                }

                File.WriteAllText(timeBlockCsvPath, writer.ToString());
            }
        }

        private void CreateTimeLinearCsvFile()
        {
            var random = new Random(2432015);

            using (var writer = new StringWriter(new StringBuilder()))
            {
                writer.WriteLine("timeLinear,location,substance,value");
                foreach (var location in timeLinearCsvLocations)
                {
                    foreach (var substance in timeLinearCsvSubstances)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            writer.WriteLine("{0},{1},{2},{3}",
                                new DateTime(2015, 3, 1 + i, 0, 0, 0).ToString("yyyy-MM-dd hh:mm:ss"),
                                location,
                                substance,
                                (101 * random.NextDouble()).ToString(CultureInfo.InvariantCulture));
                        }
                    }
                }

                File.WriteAllText(timeLinearCsvPath, writer.ToString());
            }
        }
        
        private void CreateUnorderedTimeLinearCsvFile()
        {
            var random = new Random(2432015);

            using (var writer = new StringWriter(new StringBuilder()))
            {
                writer.WriteLine("timeLinear,location,substance,value");
                foreach (var location in timeLinearCsvLocations)
                {
                    foreach (var substance in timeLinearCsvSubstances)
                    {
                        var signSwitch = -1;
                        for (int i = 0; i < 10; i++)
                        {
                            writer.WriteLine("{0},{1},{2},{3}",
                                new DateTime(2015, 3, 15 + i*signSwitch, 0, 0, 0).ToString("yyyy-MM-dd hh:mm:ss"),
                                location,
                                substance,
                                (101 * random.NextDouble()).ToString(CultureInfo.InvariantCulture));

                            signSwitch = signSwitch * -1;
                        }
                    }
                }

                File.WriteAllText(unorderedTimeLinearCsvPath, writer.ToString());
            }
        }

        [Test]
        public void ImportDataTable_TimesAreSorted()
        {
            var path = Path.Combine("test", "haha");
            var dataTableData = DataTableCsvFileReader.Read(unorderedTimeLinearCsvPath, path);

            Assert.AreEqual(Path.GetFileNameWithoutExtension(unorderedTimeLinearCsvPath), dataTableData.Name);
            Assert.AreEqual(DataTableInterpolationType.Linear, dataTableData.Interpolation);
            Assert.AreEqual(path, dataTableData.UseforIncludeFolderPath);
            CollectionAssert.AreEqual(timeLinearCsvLocations, dataTableData.DataRows.Select(dr => dr.Name).ToArray());

            foreach (var locationData in dataTableData.DataRows)
            {
                var orderedData = locationData.TimeDependentSubstanceData.OrderBy(key => key.Key);
                CollectionAssert.AreEqual(orderedData, locationData.TimeDependentSubstanceData);
            }
        }

        [Test]
        public void ImportDataTableFromTimeBlockCsv()
        {
            var path = Path.Combine("test", "haha");
            var dataTableData = DataTableCsvFileReader.Read(timeBlockCsvPath, path);

            Assert.AreEqual(Path.GetFileNameWithoutExtension(timeBlockCsvPath), dataTableData.Name);
            Assert.AreEqual(DataTableInterpolationType.Block, dataTableData.Interpolation);
            Assert.AreEqual(path, dataTableData.UseforIncludeFolderPath);
            CollectionAssert.AreEqual(timeBlockCsvLocations, dataTableData.DataRows.Select(dr => dr.Name).ToArray());
            foreach (var locationData in dataTableData.DataRows)
            {
                var expectedDateTimeEntries = Enumerable.Range(0, 5)
                    .Select(i => new DateTime(2015, 3, 1 + i, 12, 0, 0))
                    .ToArray();
                CollectionAssert.AreEquivalent(expectedDateTimeEntries, locationData.TimeDependentSubstanceData.Keys);
                foreach (var substanceAndValue in locationData.TimeDependentSubstanceData.Values)
                {
                    CollectionAssert.AreEquivalent(timeBlockCsvSubstances, substanceAndValue.Keys);
                    foreach (var value in substanceAndValue.Values)
                    {
                        var valueToDouble = double.Parse(value, CultureInfo.InvariantCulture);
                        Assert.GreaterOrEqual(valueToDouble, 1.2);
                        Assert.LessOrEqual(valueToDouble, 4.6);
                    }
                }
            }
        }

        [Test]
        public void ImportDataTableFromTimeBlockCapitalsCsv()
        {
            var path = Path.Combine("test", "haha");
            var dataTableData = DataTableCsvFileReader.Read(timeBlockCapitalsCsvPath, path);

            Assert.AreEqual(Path.GetFileNameWithoutExtension(timeBlockCapitalsCsvPath), dataTableData.Name);
            Assert.AreEqual(DataTableInterpolationType.Block, dataTableData.Interpolation);
            Assert.AreEqual(path, dataTableData.UseforIncludeFolderPath);
            CollectionAssert.AreEqual(timeBlockCsvLocations, dataTableData.DataRows.Select(dr => dr.Name).ToArray());
            foreach (var locationData in dataTableData.DataRows)
            {
                var expectedDateTimeEntries = Enumerable.Range(0, 5)
                    .Select(i => new DateTime(2015, 3, 1 + i, 12, 0, 0))
                    .ToArray();
                CollectionAssert.AreEquivalent(expectedDateTimeEntries, locationData.TimeDependentSubstanceData.Keys);
                foreach (var substanceAndValue in locationData.TimeDependentSubstanceData.Values)
                {
                    CollectionAssert.AreEquivalent(timeBlockCsvSubstances, substanceAndValue.Keys);
                    foreach (var value in substanceAndValue.Values)
                    {
                        var valueToDouble = double.Parse(value, CultureInfo.InvariantCulture);
                        Assert.GreaterOrEqual(valueToDouble, 1.2);
                        Assert.LessOrEqual(valueToDouble, 4.6);
                    }
                }
            }
        }

        [Test]
        public void ImportDataTableFromTimeLinearCsv()
        {
            var path = Path.Combine("lol", "hihi");
            var dataTableData = DataTableCsvFileReader.Read(timeLinearCsvPath, path);

            Assert.AreEqual(Path.GetFileNameWithoutExtension(timeLinearCsvPath), dataTableData.Name);
            Assert.AreEqual(DataTableInterpolationType.Linear, dataTableData.Interpolation);
            Assert.AreEqual(path, dataTableData.UseforIncludeFolderPath);
            CollectionAssert.AreEqual(timeLinearCsvLocations, dataTableData.DataRows.Select(dr => dr.Name).ToArray());
            foreach (var locationData in dataTableData.DataRows)
            {
                var expectedDateTimeEntries = Enumerable.Range(0, 10)
                    .Select(i => new DateTime(2015, 3, 1 + i, 12, 0, 0))
                    .ToArray();
                CollectionAssert.AreEquivalent(expectedDateTimeEntries, locationData.TimeDependentSubstanceData.Keys);
                foreach (var substanceAndValue in locationData.TimeDependentSubstanceData.Values)
                {
                    CollectionAssert.AreEquivalent(timeLinearCsvSubstances, substanceAndValue.Keys);
                    foreach (var value in substanceAndValue.Values)
                    {
                        var valueToDouble = double.Parse(value, CultureInfo.InvariantCulture);
                        Assert.GreaterOrEqual(valueToDouble, 0.0);
                        Assert.LessOrEqual(valueToDouble, 101.0);
                    }
                }
            }
        }

        [Test]
        public void Read_EmptyFile_ThrowsFormatException()
        {
            // setup
            var testDataPath = TestHelper.GetTestFilePath(Path.Combine("IO", "DataTables", "empty.csv"));

            // call
            TestDelegate call = () => DataTableCsvFileReader.Read(testDataPath, "test");

            // assert
            var exception = Assert.Throws<FormatException>(call);
            var expectedMessage = 
                "No valid header was found; First line: <missing>" + Environment.NewLine +
                "Expected: time[Block/Linear],location,substance,value";
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        [Test]
        public void Read_MalformattedFile_ThrowsFormatException()
        {
            // setup
            var testDataPath = TestHelper.GetTestFilePath(Path.Combine("IO", "DataTables", "not_valid_format.csv"));

            // call
            TestDelegate call = () => DataTableCsvFileReader.Read(testDataPath, "test");

            // assert
            var exception = Assert.Throws<FormatException>(call);
            var expectedMessage =
                "No valid header was found; First line: I,am,not,a,valid,boundary,data,file" + Environment.NewLine +
                "Expected: time[Block/Linear],location,substance,value";
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        [Test]
        public void Read_MalformattedFile2_ThrowsFormatException()
        {
            // setup
            var testDataPath = TestHelper.GetTestFilePath(Path.Combine("IO", "DataTables", "not_valid_format2.csv"));

            // call
            TestDelegate call = () => DataTableCsvFileReader.Read(testDataPath, "test");

            // assert
            var exception = Assert.Throws<FormatException>(call);
            var expectedMessage =
                "No valid header was found; First line: timeBlock" + Environment.NewLine +
                "Expected: time[Block/Linear],location,substance,value";
            Assert.AreEqual(expectedMessage, exception.Message);
        }

        [Test]
        public void Read_FileDoesNotExist_ThrowsArgumentException()
        {
            // setup

            // call
            TestDelegate call = () => DataTableCsvFileReader.Read("not-a-valid-path", "test");

            // assert
            var exception = Assert.Throws<ArgumentException>(call);
            var expected = "Not a valid file-path (not-a-valid-path) specified." + Environment.NewLine +
                           "Parameter name: path";
            Assert.AreEqual(expected, exception.Message);
        }

        [Test]
        public void Read_File_With_ExponentialNotation_KeepsTheFormat()
        {
            var filePath = TestHelper.GetTestFilePath(@"CsvExponentialNumbers\spill.csv");
            Assert.IsTrue(File.Exists(filePath));

            filePath = TestHelper.CreateLocalCopy(filePath);
            var path = Path.Combine("test", "test");
            var dataRead = DataTableCsvFileReader.Read(filePath, path);

            var values = dataRead.DataRows
                        .SelectMany( dr => dr.TimeDependentSubstanceData.Values)
                        .SelectMany( k => k.Values).ToList();

            Assert.IsTrue(values.Contains("4.0E12"));
            Assert.IsTrue(values.Contains("13.0E12"));

            FileUtils.DeleteIfExists(filePath);
        }

        [Test]
        public void Read_File_With_WrongNumber_LogsMessage()
        {
            var filePath = TestHelper.GetTestFilePath(@"CsvDataTableValueColumn\wrongFormat.csv");
            Assert.IsTrue(File.Exists(filePath));

            filePath = TestHelper.CreateLocalCopy(filePath);
            var path = Path.Combine("test", "test");
            var expectedErrorMessage = 
                string.Format(
                    Resources.DataTableCsvFileReader_CreateDataTableCsvContents_Line__0__contains_wrong_substance_value___1_, 
                    2, "wr0ngV4lu3");

            TestHelper.AssertAtLeastOneLogMessagesContains(() => DataTableCsvFileReader.Read(filePath, path), expectedErrorMessage);

            FileUtils.DeleteIfExists(filePath);
        }
    }
}