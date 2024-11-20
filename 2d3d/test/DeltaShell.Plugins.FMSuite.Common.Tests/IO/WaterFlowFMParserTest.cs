using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO
{
    [TestFixture]
    public class WaterFlowFMParserTest
    {
        [Test]
        public void GetClrTypeTest()
        {
            var captionField = "";
            Assert.AreEqual(typeof(int), FMParser.GetClrType(null, "Integer", ref captionField, null, 0));
            Assert.AreEqual(typeof(double), FMParser.GetClrType(null, "Double", ref captionField, null, 0));
            Assert.AreEqual(typeof(IList<double>), FMParser.GetClrType(null, "DoubleArray", ref captionField, null, 0));
            Assert.AreEqual(typeof(DateTime), FMParser.GetClrType(null, "DateTime", ref captionField, null, 0));
            Assert.AreEqual(typeof(DateOnly), FMParser.GetClrType(null, "DateOnly", ref captionField, null, 0));
            Assert.AreEqual(typeof(string), FMParser.GetClrType(null, "String", ref captionField, null, 0));
            Assert.AreEqual(typeof(string), FMParser.GetClrType(null, "FileName", ref captionField, null, 0));
            Assert.AreEqual(typeof(IList<string>), FMParser.GetClrType(null, "MultipleEntriesFileName", ref captionField, null, 0));
            Assert.AreEqual(typeof(bool), FMParser.GetClrType(null, "0|1", ref captionField, null, 0));
            Assert.AreEqual(typeof(bool), FMParser.GetClrType(null, "1|0", ref captionField, null, 0));
            Assert.AreEqual(typeof(Steerable), FMParser.GetClrType(null, "Steerable", ref captionField, null, 0));
            captionField = "Test:1|2|3|4";
            Assert.IsNotNull(FMParser.GetClrType("TestVar", "T|e|s|t", ref captionField, null, 0));
            captionField = "Test:a|b|c|d";
            Assert.IsNotNull(FMParser.GetClrType("TestVar", "T|e|s|t", ref captionField, null, 0));

            captionField = "It's a syntax error if no colon is specified when using '|' characters";
            Assert.Throws<FormatException>(() => FMParser.GetClrType(null, "T|e|s|t", ref captionField, null, 0), "");
            captionField = "Syntax error: Number of '|' characters should be the same as in typefield";
            Assert.Throws<FormatException>(() => FMParser.GetClrType(null, "T|e|s|t", ref captionField, null, 0), "");
            Assert.Throws<ArgumentException>(() => FMParser.GetClrType(null, "I am not defined", ref captionField, null, 0), "");
        }

        [Test]
        public void GetStringArrayTest()
        {
            var list = FMParser.FromString<IList<string>>("");
            Assert.AreEqual(0, list.Count);

            #region FromString

            // Can deal with space separated:
            list = FMParser.FromString<IList<string>>("alpha beta gamma delta epsilon");
            Assert.AreEqual(5, list.Count);
            Assert.AreEqual(new[]
            {
                "alpha",
                "beta",
                "gamma",
                "delta",
                "epsilon"
            }, list);

            // Can deal with tab separated:
            list = FMParser.FromString<IList<string>>("alpha\tbeta\tgamma\tdelta\tepsilon");
            Assert.AreEqual(5, list.Count);
            Assert.AreEqual(new[]
            {
                "alpha",
                "beta",
                "gamma",
                "delta",
                "epsilon"
            }, list);

            #endregion

            #region ToString

            // Can deal with space separated:
            var stringList = new List<string>(new[]
            {
                "alpha",
                "beta",
                "gamma",
                "delta",
                "epsilon"
            });
            var newList = FMParser.ToString(stringList, typeof(IList<string>));
            Assert.AreEqual("alpha beta gamma delta epsilon", newList);

            #endregion
        }

        [Test]
        public void GetDoubleArrayTest()
        {
            var list = FMParser.FromString<IList<double>>("");
            Assert.AreEqual(0, list.Count);

            // Can deal with ints:
            list = FMParser.FromString<IList<double>>("1");
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(new[]
            {
                1.0
            }, list);

            // Always read as Culture Invariant:
            list = FMParser.FromString<IList<double>>("1.000");
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(new[]
            {
                1.0
            }, list);
            list = FMParser.FromString<IList<double>>("1,000");
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(new[]
            {
                1000.0
            }, list);

            // Can deal with space separated:
            list = FMParser.FromString<IList<double>>("1.0 2 3.2 4 5.4");
            Assert.AreEqual(5, list.Count);
            Assert.AreEqual(new[]
            {
                1.0,
                2.0,
                3.2,
                4.0,
                5.4
            }, list);

            // Can deal with tab separated:
            list = FMParser.FromString<IList<double>>("1.0\t2\t3.2\t4\t5.4");
            Assert.AreEqual(5, list.Count);
            Assert.AreEqual(new[]
            {
                1.0,
                2.0,
                3.2,
                4.0,
                5.4
            }, list);

            // Can deal with tab and space separated combined:
            list = FMParser.FromString<IList<double>>("1.0\t2 3.2\t4 5.4");
            Assert.AreEqual(5, list.Count);
            Assert.AreEqual(new[]
            {
                1.0,
                2.0,
                3.2,
                4.0,
                5.4
            }, list);

            Assert.Throws<ArgumentNullException>(() => FMParser.FromString<IList<double>>(null));
        }

        [Test]
        [SetCulture("NL-nl")]
        public void ToStringTestInNlCulture()
        {
            Assert.AreEqual("1.2", FMParser.ToString(1.2, typeof(double)));
            Assert.AreEqual("1.2 3.4", FMParser.ToString(new List<double>(new[]
            {
                1.2,
                3.4
            }), typeof(IList<double>)));
        }

        [Test]
        public void ToStringForSteerable()
        {
            var steerable = new Steerable
            {
                ConstantValue = 1.2,
                TimeSeriesFilename = "weir01_crest_level.tim",
                Mode = SteerableMode.ConstantValue
            };
            Assert.AreEqual("1.2", FMParser.ToString(steerable, typeof(Steerable)));

            steerable.Mode = SteerableMode.TimeSeries;

            Assert.AreEqual("weir01_crest_level.tim", FMParser.ToString(steerable, typeof(Steerable)));

            steerable.Mode = SteerableMode.External;

            Assert.AreEqual("REALTIME", FMParser.ToString(steerable, typeof(Steerable)));
        }

        [TestCaseSource(nameof(DateTimeConversionSuccess))]
        [TestCaseSource(nameof(DateTimesAndStrings))]
        public void DateTimeFromString_WhenInputIsValid_ReturnsDateTime(string input, object expectedOutput)
        {
            Assert.AreEqual(expectedOutput, FMParser.FromString(input, typeof(DateTime)));
        }

        [Test]
        public static void DateTimeFromString_WhenInputIsNullOrEmpty_ReturnsNow()
        {
            DateTime now = DateTime.Now;
            DateTime result = (DateTime)FMParser.FromString("", typeof(DateTime));
            TimeSpan timeLapsed = result - now;
            Assert.Less(timeLapsed.Seconds, 0.5d);
        }

        [TestCaseSource(nameof(DateTimeConversionFailure))]
        public void DateTimeFromString_WhenInputNotValid_ThrowsFormatException(string input)
        {
            Assert.Throws<FormatException>(() => FMParser.FromString(input, typeof(DateTime)));
        }

        [Test]
        [TestCaseSource(nameof(DateTimesAndStrings))]
        public void DateTimeToString_WhenInputIsValid_ReturnsDateTimeAsString(string expectedDateTime, DateTime givenDateTime)
        {
            Assert.That(FMParser.ToString(givenDateTime, typeof(DateTime)), Is.EqualTo(expectedDateTime));
        }

        public static IEnumerable<TestCaseData> DateTimeConversionSuccess()
        {
            yield return new TestCaseData("20230509000000", new DateTime(2023, 05, 09));
            yield return new TestCaseData("20221231", new DateTime(2022, 12, 31));
            yield return new TestCaseData("20230101", new DateTime(2023, 01, 01));
            yield return new TestCaseData("2023-05-09", new DateTime(2023, 05, 09));
            yield return new TestCaseData("20230509012345", new DateTime(2023, 05, 09, 01, 23, 45));
        }

        public static IEnumerable<TestCaseData> DateTimeConversionFailure()
        {
            yield return new TestCaseData("20230509240000");
            yield return new TestCaseData("20230231000000");
            yield return new TestCaseData("2023-05-09 00:00:00");
            yield return new TestCaseData("NonDateTimeString");
        }

        private static IEnumerable<TestCaseData> DateTimesAndStrings()
        {
            yield return new TestCaseData("20230612121314", new DateTime(2023, 06, 12, 12, 13, 14));
            yield return new TestCaseData("20230612000000", new DateTime(2023, 06, 12));
            yield return new TestCaseData("20230612000000", new DateTime(2023, 06, 12, 0, 0, 0));
        }

        [TestCaseSource(nameof(DateOnlyConversionSuccess))]
        public void DateOnlyFromString_WhenInputIsValid_ReturnsDateOnly(string input, object expectedOutput)
        {
            Assert.AreEqual(expectedOutput, FMParser.FromString(input, typeof(DateOnly)));
        }

        [Test]
        public static void DateOnlyFromString_WhenInputIsNullOrEmpty_ReturnsNow()
        {
            var now = DateOnly.FromDateTime(DateTime.Now);
            var result = (DateOnly)FMParser.FromString("", typeof(DateOnly));
            var timeLapsed = result.ToDateTime(TimeOnly.MinValue) - now.ToDateTime(TimeOnly.MinValue);
            Assert.Less( timeLapsed.Days, 0.5d );
        }

        [TestCaseSource(nameof(DateOnlyConversionFailure))]
        public void DateOnlyFromStringTest_WhenInputNotValid_ThrowsFormatException(string input)
        {
            Assert.Throws<FormatException>(() => FMParser.FromString(input, typeof(DateOnly)));
        }
        
        [TestCaseSource(nameof(DateOnlyToStringConversionSuccess))]
        public void DateOnlyToString_WhenInputIsValid_ReturnsDateOnly(string expectedDateTime, DateOnly givenDateTime)
        {
            Assert.That(FMParser.ToString(givenDateTime, typeof(DateOnly)), Is.EqualTo(expectedDateTime));
        }

        public static IEnumerable<TestCaseData> DateOnlyConversionSuccess()
        {
            yield return new TestCaseData("20221231", new DateOnly(2022, 12, 31));
            yield return new TestCaseData("20230101", new DateOnly(2023, 01, 01));
            yield return new TestCaseData("2023-05-09",new DateOnly(2023,05,09));
            yield return new TestCaseData("20230509000000",new DateOnly(2023,05,09));
        }
        
        public static IEnumerable<TestCaseData> DateOnlyToStringConversionSuccess()
        {
            yield return new TestCaseData("20221231", new DateOnly(2022, 12, 31));
            yield return new TestCaseData("20230101", new DateOnly(2023, 01, 01));
            yield return new TestCaseData("20230509",new DateOnly(2023,05,09));
        }

        public static IEnumerable<TestCaseData> DateOnlyConversionFailure()
        {
            yield return new TestCaseData("InvalidDateTime");
            yield return new TestCaseData("20230231");
        }

        [Test]
        public void FromStringTest()
        {
            var captionField = "Test:1|2|3|4";
            Type dataType = FMParser.GetClrType("TestVar", "T|e|s|t", ref captionField, null, 0);

            Assert.Throws<FormatException>(() => FMParser.FromString("1", dataType));
            Assert.AreEqual(dataType.GetEnumValues().GetValue(1), FMParser.FromString("e", dataType));
        }

        /// <summary>
        /// GIVEN an input boolean string
        /// WHEN FromString is called
        /// THEN the correct value is returned
        /// </summary>
        [TestCase("0", false)]
        [TestCase("false", false)]
        [TestCase("False", false)]
        [TestCase("1", true)]
        [TestCase("True", true)]
        [TestCase("true", true)]
        public void GivenAnInputBooleanString_WhenFromStringIsCalled_ThenTheCorrectValueIsReturned(string inputString, bool expectedOutput)
        {
            // When
            var result = (bool) FMParser.FromString(inputString, typeof(bool));

            // Then
            Assert.That(result, Is.EqualTo(expectedOutput));
        }
    }
}