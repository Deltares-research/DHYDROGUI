using System;
using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.Helpers;
using DHYDRO.Common.Logging;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders
{
    [TestFixture]
    public class BcCategoryParserTest
    {
        private const string secondsSince = "seconds since 2021-01-01 00:00:00";
        private const string minutesSince = "minutes since 2021-01-01 00:00:00";
        private const string hoursSince = "hours since 2021-01-01 00:00:00";
        private const int timeToAdd = 10; //keep 24 hours in mind with changing this value
        private const int randomLineNumber = 123;
        
        private IBcCategoryParser parser;
        private ILogHandler logHandler;

        [SetUp]
        public void SetUp()
        {
            logHandler = Substitute.For<ILogHandler>();
            parser = new BcCategoryParser(logHandler);
        }

        [Test]
        public void Constructor_ArgNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new BcCategoryParser(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void TryParseDoubles_ArgNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => parser.TryParseDoubles(null, randomLineNumber, out IEnumerable<double> _);

            // Assert
            Assert.Throws<ArgumentNullException>(Call);
        }
        
        [Test]
        [TestCaseSource(nameof(ArgNull))]
        public void TryParseDateTimes_ArgNull_ThrowsArgumentNullException(IEnumerable<string> values, string unitValue)
        {
            // Call
            void Call() => parser.TryParseDateTimes(values, unitValue, randomLineNumber, out IEnumerable<DateTime> _);

            // Assert
            Assert.Throws<ArgumentNullException>(Call);
        }
        
        [Test]
        public void CreateConstant_ArgNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => parser.CreateConstant(null, randomLineNumber);

            // Assert
            Assert.Throws<ArgumentNullException>(Call);
        }

        [Test]
        [TestCaseSource(nameof(CreateConstant_TableHasNoValues_TestCases))]
        public void CreateConstant_TableHasNoValues_LogsMessageAndReturnsZero(IList<IDelftBcQuantityData> table)
        {
            // Call
            double value = parser.CreateConstant(table, randomLineNumber);
            
            // Assert
            const double expectedValue = 0;
            Assert.That(value, Is.EqualTo(expectedValue));

            var expectedErrorMessage = $"The provided table on line {randomLineNumber} does not contain any values.";
            logHandler.Received(1).ReportError(expectedErrorMessage);
        }

        private static IEnumerable<TestCaseData> CreateConstant_TableHasNoValues_TestCases()
        {
            IList<IDelftBcQuantityData> table = CreateValidTableSubstitute("randomValue");

            table[0].Values[0].ReturnsNull();
            yield return new TestCaseData(table).SetName("Table contains no values.");

            table[0].Values.ReturnsNull();
            yield return new TestCaseData(table).SetName("Table has no values property.");

            table[0].ReturnsNull();
            yield return new TestCaseData(table).SetName("Table has no data.");
        }

        [Test]
        [TestCase("1", 1)]
        [TestCase("NoValidData", 0)]
        public void WhenCreatingConstant_ThenReturnExpectedDouble(string insertedString, double expectedDouble)
        {
            // Arrange
            IList<IDelftBcQuantityData> table = CreateValidTableSubstitute(insertedString);

            // Act & Assert
            Assert.That(parser.CreateConstant(table, randomLineNumber), Is.EqualTo(expectedDouble));
        }

        private static IList<IDelftBcQuantityData> CreateValidTableSubstitute(string value)
        {
            var table = Substitute.For<IList<IDelftBcQuantityData>>();
            
            var dataEnumerator = Substitute.For<IEnumerator<IDelftBcQuantityData>>();
            dataEnumerator.MoveNext().Returns(true);
            table.GetEnumerator().Returns(dataEnumerator);
            
            var values = Substitute.For<IList<string>>();
            var enumerator = Substitute.For<IEnumerator<string>>();
            enumerator.MoveNext().Returns(true);
            values.GetEnumerator().Returns(enumerator);
            values[0].Returns(value);

            var data = Substitute.For<IDelftBcQuantityData>();
            data.Values.Returns(values);

            table[0].Returns(data);

            return table;
        }

        [Test]
        public void WhenParsingDoubles_ThenReturnExpectedDouble()
        {
            // Arrange
            List<string> value = new List<string>();
            value.Add("1.1");

            // Act
            parser.TryParseDoubles(value, randomLineNumber, out IEnumerable<double> outValue);

            // Assert
            Assert.That(outValue.First(), Is.EqualTo(1.1));
        }
        
        [Test]
        public void WhenParsingDoubles_WhenInvalidData_ThenReturnExpectedDouble()
        {
            // Arrange
            List<string> value = new List<string>();
            value.Add("InvalidData");

            // Act
            parser.TryParseDoubles(value, randomLineNumber, out IEnumerable<double> _);

            // Assert
            logHandler.Received(1).ReportError($"Cannot parse 'InvalidData' to a double, see category on line {randomLineNumber}.");
        }

        [Test]
        [TestCaseSource(nameof(TimeCases))]
        public void WhenParsingDateTimes_AddTime_ThenOutputExpectedDateTimesPlusAddedTime(string addedTime, string timeSince, DateTime expectedDateTime)
        {
            // Arrange
            List<string> values = new List<string>();
            values.Add(addedTime);

            // Act
            parser.TryParseDateTimes(values, timeSince, randomLineNumber, out IEnumerable<DateTime> dateTimes);

            // Assert
            Assert.That(dateTimes.First(), Is.EqualTo(expectedDateTime));
        }

        [Test]
        [TestCaseSource(nameof(TimeStringCases))]
        public void WhenParsingDateTimes_GiveIncorrectTime_ThenThrowExceptionWithErrorMessage(string addedTime, string timeUnit, string expError)
        {
            // Arrange
            List<string> values = new List<string>();
            values.Add(addedTime);

            // Act
            parser.TryParseDateTimes(values, timeUnit, randomLineNumber, out IEnumerable<DateTime> _);

            // Assert
            logHandler.Received(1).ReportError(expError);

        }
        
        private static IEnumerable<TestCaseData> TimeCases()
        {
            yield return new TestCaseData(timeToAdd.ToString(), secondsSince, new DateTime(2021, 1, 1).AddSeconds(timeToAdd));
            yield return new TestCaseData(timeToAdd.ToString(), minutesSince, new DateTime(2021, 1, 1).AddMinutes(timeToAdd));
            yield return new TestCaseData(timeToAdd.ToString(), hoursSince, new DateTime(2021, 1, 1).AddHours(timeToAdd));
        }
        
        private static IEnumerable<TestCaseData> TimeStringCases()
        {
            yield return new TestCaseData(timeToAdd.ToString(), "centuries since 2021-01-01 00:00:00", $"Cannot interpret 'centuries since 2021-01-01 00:00:00', see category on line {randomLineNumber}.");
            yield return new TestCaseData(timeToAdd.ToString(), "minutes since yesterday", $"Cannot parse 'yesterday' to a date time, see category on line {randomLineNumber}.");
        }
        
        private static IEnumerable<TestCaseData> ArgNull()
        {
            yield return new TestCaseData(null, string.Empty);
            yield return new TestCaseData(new List<string>(), null);
        }
    }
}