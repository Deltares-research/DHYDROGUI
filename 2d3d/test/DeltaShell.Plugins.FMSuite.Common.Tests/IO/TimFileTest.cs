using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO
{
    [TestFixture]
    public class TimFileTest
    {
        [Test]
        public void CheckRelativeDateTimeParsing()
        {
            var dateTime = new DateTime(2013, 8, 29, 17, 8, 20);
            var timeSpan = new TimeSpan(400, 23, 59, 30);
            var minutes = timeSpan.TotalMinutes.ToString(CultureInfo.InvariantCulture);

            var reader = new TimFile();
            DateTime? readDateTime = reader.GetDateTime(minutes, dateTime, "time");

            Assert.AreEqual(readDateTime, dateTime + timeSpan, "the imported time");
        }

        [Test]
        public void CheckExtrapolationPlaceholderDateTimeParsing()
        {
            var dateTime = new DateTime(2013, 8, 29, 17, 8, 20);

            var reader = new TimFile();
            DateTime? readDateTime = reader.GetDateTime("9999999999.0", dateTime, "time");

            Assert.AreEqual(readDateTime, dateTime + new TimeSpan(0, 999999999, 0), "the imported time");
        }

        [Test]
        public void CheckAbsoluteeDateTimeParsing()
        {
            var dateTime = new DateTime(2013, 8, 29, 17, 8, 00);
            const long formattedDateTime = 201308291708;

            var reader = new TimFile();
            DateTime? readDateTime = reader.GetDateTime(formattedDateTime.ToString(CultureInfo.InvariantCulture), dateTime, "time");

            Assert.AreEqual(readDateTime, dateTime, "the imported time");
        }

        [Test]
        public void WriteReadMultiColumnTimFile()
        {
            var refDate = new DateTime(2014, 5, 1, 0, 0, 0);

            var startDate = new DateTime(2014, 5, 5, 13, 25, 30);

            var timeStep = new TimeSpan(0, 0, 1, 00);

            const int count = 100;

            var timesList = new List<DateTime>();
            var valuesList1 = new List<double>();
            var valuesList2 = new List<double>();
            var valuesList3 = new List<double>();

            DateTime date = startDate;
            for (var i = 0; i < count; ++i)
            {
                timesList.Add(date);
                date += timeStep;
                valuesList1.Add(1.0 / ((i * i) + 1));
                valuesList2.Add(1.0 / ((i * i) + 2));
                valuesList3.Add(1.0 / ((i * i) + 3));
            }

            var function = new TimeSeries();
            function.Components.Add(new Variable<double>("a"));
            function.Components.Add(new Variable<double>("b"));
            function.Components.Add(new Variable<double>("c"));

            function.Time.SetValues(timesList);
            function.Components[0].SetValues(valuesList1);
            function.Components[1].SetValues(valuesList2);
            function.Components[2].SetValues(valuesList3);

            var fileWriter = new TimFile();
            fileWriter.Write("testFile.tim", function, refDate);

            TimeSeries readFunction = fileWriter.Read("testFile.tim", refDate);

            Assert.AreEqual(1, readFunction.Arguments.Count);
            Assert.AreEqual(3, readFunction.Components.Count);
            Assert.AreEqual(function.Time.Values, readFunction.Arguments[0].Values);
            ListTestUtils.AssertAreEqual(function.Components[0].GetValues<double>(),
                                         readFunction.Components[0].GetValues<double>(), 1e-06);
            ListTestUtils.AssertAreEqual(function.Components[1].GetValues<double>(),
                                         readFunction.Components[1].GetValues<double>(), 1e-06);
            ListTestUtils.AssertAreEqual(function.Components[2].GetValues<double>(),
                                         readFunction.Components[2].GetValues<double>(), 1e-06);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCase("Aimen.tim")]
        [TestCase("test_mk.tim")]
        public void GivenFileWithMultipleSpacesInColumn_WhenRead_ThenDoesNotCrashAndReturnsExpectedColumns(string fileName)
        {
            // 1. Prepare test data
            string filepath = TestHelper.GetTestFilePath(Path.Combine("timFiles", fileName));
            double[] firstValues =
            {
                392.30,
                0.10,
                25.0
            };
            double[] lastValues =
            {
                190.86,
                0.10,
                25.0
            };
            const int expectedComponents = 3;
            const int expectedValues = 50;

            var refDate = new DateTime(2014, 5, 1, 0, 0, 0);
            TimeSeries readTimeSeries = null;

            // 2. Verify initial expectations.
            Assert.That(File.Exists(filepath));

            // 3. Run test.
            TestDelegate testAction = () => readTimeSeries = new TimFile().Read(filepath, refDate);

            // 4. Verify final expectations.
            Assert.That(testAction, Throws.Nothing);
            Assert.That(readTimeSeries, Is.Not.Null);
            Assert.That(readTimeSeries.Time.AllValues.Count, Is.EqualTo(expectedValues));
            Assert.That(readTimeSeries.Components.Count, Is.EqualTo(expectedComponents));
            Assert.That(readTimeSeries.Components[0].GetValues<double>().First(), Is.EqualTo(firstValues[0]));
            Assert.That(readTimeSeries.Components[1].GetValues<double>().First(), Is.EqualTo(firstValues[1]));
            Assert.That(readTimeSeries.Components[2].GetValues<double>().First(), Is.EqualTo(firstValues[2]));
            Assert.That(readTimeSeries.Components[0].GetValues<double>().Last(), Is.EqualTo(lastValues[0]));
            Assert.That(readTimeSeries.Components[1].GetValues<double>().Last(), Is.EqualTo(lastValues[1]));
            Assert.That(readTimeSeries.Components[2].GetValues<double>().Last(), Is.EqualTo(lastValues[2]));
        }

        [Test]
        public void ReadMultiColumnTimFile()
        {
            var refDate = new DateTime(2014, 5, 1, 0, 0, 0);

            var startDate = new DateTime(2014, 5, 5, 13, 25, 30);

            var timeStep = new TimeSpan(0, 0, 1, 00);

            const int count = 50;

            var timesList = new List<DateTime>();
            var valuesList1 = new List<double>();
            var valuesList2 = new List<double>();
            var valuesList3 = new List<double>();

            DateTime date = startDate;
            for (var i = 0; i < count; ++i)
            {
                timesList.Add(date);
                date += timeStep;
                valuesList1.Add(1.0 / ((i * i) + 1));
                valuesList2.Add(1.0 / ((i * i) + 2));
                valuesList3.Add(1.0 / ((i * i) + 3));
            }

            var function = new TimeSeries();
            function.Components.Add(new Variable<double>("a"));
            function.Components.Add(new Variable<double>("b"));
            function.Components.Add(new Variable<double>("c"));

            function.Time.SetValues(timesList);
            function.Components[0].SetValues(valuesList1);
            function.Components[1].SetValues(valuesList2);
            function.Components[2].SetValues(valuesList3);

            var fileWriter = new TimFile();
            TimeSeries readFunction = fileWriter.Read(TestHelper.GetTestFilePath("timFiles/testFile.tim"), refDate);

            Assert.AreEqual(1, readFunction.Arguments.Count);
            Assert.AreEqual(3, readFunction.Components.Count);
            Assert.AreEqual(function.Time.Values, readFunction.Arguments[0].Values);
            ListTestUtils.AssertAreEqual(function.Components[0].GetValues<double>(),
                                         readFunction.Components[0].GetValues<double>(), 1e-06);
            ListTestUtils.AssertAreEqual(function.Components[1].GetValues<double>(),
                                         readFunction.Components[1].GetValues<double>(), 1e-06);
            ListTestUtils.AssertAreEqual(function.Components[2].GetValues<double>(),
                                         readFunction.Components[2].GetValues<double>(), 1e-06);
        }

        [Test]
        public void ReadMultiColumnTimFileWithMissingValues_AddsDefaultZeroValues() // Issue: DELFT3DFM-817
        {
            var refDate = new DateTime(2014, 5, 1, 0, 0, 0);

            var readFunction = new TimeSeries();
            readFunction.Components.Add(new Variable<double>("a"));
            readFunction.Components.Add(new Variable<double>("b"));
            readFunction.Components.Add(new Variable<double>("c"));
            readFunction.Components.Add(new Variable<double>("d"));
            readFunction.Components.Add(new Variable<double>("e"));

            var fileReader = new TimFile();
            fileReader.Read(TestHelper.GetTestFilePath("timFiles/testFile.tim"), readFunction, refDate);

            IMultiDimensionalArray<double> componentAValues = readFunction.Components[0].GetValues<double>();
            IMultiDimensionalArray<double> componentBValues = readFunction.Components[1].GetValues<double>();
            IMultiDimensionalArray<double> componentCValues = readFunction.Components[2].GetValues<double>();
            IMultiDimensionalArray<double> componentDValues = readFunction.Components[3].GetValues<double>();
            IMultiDimensionalArray<double> componentEValues = readFunction.Components[4].GetValues<double>();

            Assert.AreEqual(50, componentAValues.Count);
            Assert.AreEqual(50, componentBValues.Count);
            Assert.AreEqual(50, componentCValues.Count);

            Assert.AreEqual(50, componentDValues.Count);
            Assert.AreEqual(48, componentDValues.Count(v => v <= double.Epsilon));
            Assert.AreEqual(2, componentDValues.Count(v => v > double.Epsilon));

            Assert.AreEqual(50, componentEValues.Count);
            Assert.AreEqual(49, componentEValues.Count(v => v <= double.Epsilon));
            Assert.AreEqual(1, componentEValues.Count(v => v > double.Epsilon));
        }

        [Test]
        public void ReadMultiColumnTimFileWithAdditionalValues()
        {
            var refDate = new DateTime(2014, 5, 1, 0, 0, 0);

            var readFunction = new TimeSeries();
            readFunction.Components.Add(new Variable<double>("a"));

            // Test to check that fileReader can handle files with additional data
            var fileReader = new TimFile();
            fileReader.Read(TestHelper.GetTestFilePath("timFiles/testFile.tim"), readFunction, refDate);

            Assert.AreEqual(50, readFunction.Components[0].GetValues<double>().Count);
        }

        [Test]
        public void ReadTimFile_HandlesNullFunction()
        {
            var fileReader = new TimFile();
            Assert.That(() => fileReader.Read(string.Empty, null, DateTime.MinValue),
                Throws.ArgumentException);
        }
    }
}