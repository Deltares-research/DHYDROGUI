using System;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO.HydFileElement;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.IO.HydFileElement
{
    [TestFixture]
    public class HydFileStringValueParserTest
    {
        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void ParseValueFromEmptyStringSetsValueToMinDateTime(string textToParse)
        {
            Assert.AreEqual(DateTime.MinValue, HydFileStringValueParser.Parse<DateTime>(textToParse));
        }

        [Test]
        [TestCase("10:12:13,08-01-2015", 2015, 01, 08, 10, 12, 13)]
        [TestCase("10:12:13, 08-01-2015", 2015, 01, 08, 10, 12, 13)]
        public void ParseValueFromDutchTimeDateStringSetsValueToThatDate(string textToParse,
                                                                         int expectedYear, int expectedMonth, int expectedDay,
                                                                         int expectedHour, int expectedMinute, int expectedSecond)
        {
            var expected = new DateTime(expectedYear, expectedMonth, expectedDay, expectedHour, expectedMinute, expectedSecond);
            Assert.AreEqual(expected, HydFileStringValueParser.Parse<DateTime>(textToParse));
        }

        [Test]
        [TestCase("'19991216000000'", 1999, 12, 16, 00, 00, 00)]
        [TestCase("'19991218123456'", 1999, 12, 18, 12, 34, 56)]
        public void ParseValueFromCustomTimeDateStringSetsValueToThatDate(string textToParse,
                                                                          int expectedYear, int expectedMonth, int expectedDay,
                                                                          int expectedHour, int expectedMinute, int expectedSecond)
        {
            var expected = new DateTime(expectedYear, expectedMonth, expectedDay, expectedHour, expectedMinute, expectedSecond);
            Assert.AreEqual(expected, HydFileStringValueParser.Parse<DateTime>(textToParse));
        }

        [Test]
        public void ParseValueFromNonDateTimeThrowsFormatException()
        {
            // setup
            const string textToParse = "Definitely not a date nor time!";

            // call
            TestDelegate call = () => HydFileStringValueParser.Parse<DateTime>(textToParse);

            // assert
            Assert.Throws<FormatException>(call);
        }

        [Test]
        [TestCase("10:12:13,08-01-2015", 2015, 1, 8, 10, 12, 13)]
        [TestCase("'19991218123456'", 1999, 12, 18, 12, 34, 56)]
        public void SetDataToHydFileWithParsedValueUpdatesHydFileDataToParsedDateTime(string textToParse,
                                                                                      int expectedYear, int expectedMonth, int expectedDay,
                                                                                      int expectedHour, int expectedMinute, int expectedSecond)
        {
            var dateTime = new DateTime(expectedYear, expectedMonth, expectedDay, expectedHour, expectedMinute, expectedSecond);
            Assert.AreEqual(dateTime, HydFileStringValueParser.Parse<DateTime>(textToParse));
        }

        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void ParseValueFromEmptyStringSetsEmptyCollection(string textToParse)
        {
            Assert.IsEmpty(HydFileStringValueParser.Parse<double[]>(textToParse));
        }

        [Test]
        [TestCase("0", new[]
        {
            0.0
        })]
        [TestCase("1 3", new[]
        {
            1.0,
            3.0
        })]
        [TestCase("1.0 2.1 3.2 4.34", new[]
        {
            1.0,
            2.1,
            3.2,
            4.34
        })]
        [TestCase("6\t7.1\t8.2", new[]
        {
            6.0,
            7.1,
            8.2
        })]
        [TestCase("9\n10\n11.1\n12\n13", new[]
        {
            9.0,
            10.0,
            11.1,
            12.0,
            13.0
        })]
        public void ParseValueFromDoubleArrayStringSetsValueToThatValue(string textToParse, double[] expectedValue)
        {
            Assert.AreEqual(expectedValue, HydFileStringValueParser.Parse<double[]>(textToParse));
        }

        [Test]
        public void ParseInvalidValueArrayStringThrowsFormatException()
        {
            // call
            TestDelegate call = () => HydFileStringValueParser.Parse<double[]>("Definitely not a double array value");

            // assert
            Assert.Throws<FormatException>(call);
        }

        [Test]
        public void ParseTooLargeDoubleThrowsFormatException()
        {
            string textToParse = string.Format("9{0}", double.MaxValue);
            TestDelegate call = () => HydFileStringValueParser.Parse<double[]>(textToParse);

            // assert
            var exception = Assert.Throws<FormatException>(call);

            Assert.AreEqual(string.Format("Value ({0}) must fall within the range [{1}, {2}].", textToParse, double.MinValue, double.MaxValue), exception.Message);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("     ")]
        [TestCase("none")]
        public void ParseValueFromEmptyStringOrNoneSetsValueToEmptyString(string textToParse)
        {
            Assert.AreEqual("", HydFileStringValueParser.Parse<string>(textToParse));
        }

        [Test]
        [TestCase("'test.c'", "test.c")]
        [TestCase("'haha.h'", "haha.h")]
        [TestCase("'     '", "")]
        public void ParseValueFromStringSetsValueToThatValue(string textToParse, string expectedValue)
        {
            Assert.AreEqual(expectedValue, HydFileStringValueParser.Parse<string>(textToParse));
        }

        [Test]
        public void ParseInvalidStringThrowsFormatException()
        {
            // setup
            const string textToParse = "Definitely not a filename value";

            // call
            TestDelegate call = () => HydFileStringValueParser.Parse<string>(textToParse);

            // assert
            Assert.Throws<FormatException>(call);
        }

        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void ParseModelTypeValueFromEmptyStringSetsValueToUndefined(string textToParse)
        {
            Assert.AreEqual(HydroDynamicModelType.Undefined, HydFileStringValueParser.Parse<HydroDynamicModelType>(textToParse));
        }

        [Test]
        [TestCase("unstructured", HydroDynamicModelType.Unstructured)]
        [TestCase("curvilinear-grid", HydroDynamicModelType.Curvilinear)]
        [TestCase("finite-elements", HydroDynamicModelType.FiniteElements)]
        [TestCase("network", HydroDynamicModelType.HydroNetwork)]
        public void ParseValueFromGeometryStringSetsValueToThatValue(string textToParse, HydroDynamicModelType expectedValue)
        {
            Assert.AreEqual(expectedValue, HydFileStringValueParser.Parse<HydroDynamicModelType>(textToParse));
            Assert.AreEqual(expectedValue, HydFileStringValueParser.Parse<HydroDynamicModelType>(textToParse + " z-layers"));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("     ")]
        public void ParseLayerTypeValueFromEmptyStringSetsValueToUndefined(string textToParse)
        {
            Assert.AreEqual(LayerType.Undefined, HydFileStringValueParser.Parse<LayerType>(textToParse));
        }

        [Test]
        [TestCase("", LayerType.Sigma)]
        [TestCase(" sigma", LayerType.Sigma)]
        [TestCase(" z-layers", LayerType.ZLayer)]
        public void ParseLayerTypeValueFromGeometryStringSetsValueToThatValue(string textToParse, LayerType expectedValue)
        {
            var geometryTypes = new[]
            {
                "unstructured",
                "curvilinear-grid",
                "finite-elements",
                "network"
            };
            foreach (string geometryType in geometryTypes)
            {
                Assert.AreEqual(expectedValue, HydFileStringValueParser.Parse<LayerType>(geometryType + textToParse));
            }
        }

        [Test]
        public void ParseInvalidGeometryStringThrowsFormatException()
        {
            // setup
            const string textToParse = "Definitely not a geometry enum value";
            // call
            TestDelegate call = () => HydFileStringValueParser.Parse<HydroDynamicModelType>(textToParse);

            // assert
            Assert.Throws<FormatException>(call);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("     ")]
        public void ParseValueFromEmptyStringSetsEmptyIntCollection(string textToParse)
        {
            Assert.IsEmpty(HydFileStringValueParser.Parse<int[]>(textToParse));
        }

        [Test]
        [TestCase("0", new[]
        {
            0
        })]
        [TestCase("1 2 3 4", new[]
        {
            1,
            2,
            3,
            4
        })]
        [TestCase("1.0 2.0 3.0 4.00", new[]
        {
            1,
            2,
            3,
            4
        })]
        [TestCase("6\t7\t8", new[]
        {
            6,
            7,
            8
        })]
        [TestCase("9\n10\n11\n12\n13", new[]
        {
            9,
            10,
            11,
            12,
            13
        })]
        public void ParseValueFromIntegerArrayStringSetsValueToThatValue(string textToParse, int[] expectedValue)
        {
            Assert.AreEqual(expectedValue, HydFileStringValueParser.Parse<int[]>(textToParse));
        }

        [Test]
        public void ParseInvalidIntValueArrayStringThrowsFormatException()
        {
            // call
            TestDelegate call = () => HydFileStringValueParser.Parse<int[]>("Definitely not an integer array value");

            // assert
            Assert.Throws<FormatException>(call);
        }

        [Test]
        public void ParseTooLargeIntegerThrowsFormatException()
        {
            // setup
            string textToParse = string.Format("{0}9", int.MaxValue);

            // call
            TestDelegate call = () => HydFileStringValueParser.Parse<int[]>(textToParse);

            // assert
            var exception = Assert.Throws<FormatException>(call);
            Assert.AreEqual(string.Format("Value ({0}) must fall within the range [{1}, {2}].", textToParse, int.MinValue, int.MaxValue), exception.Message);
        }

        [Test]
        public void ParseDoubleArrayThrowsFormatException()
        {
            // setup
            const string textToParse = "1.2 3.4 5.6";

            // call
            TestDelegate call = () => HydFileStringValueParser.Parse<int[]>(textToParse);

            // assert
            var exception = Assert.Throws<FormatException>(call);
            Assert.AreEqual("Value (1.2) must fall within the range [-2147483648, 2147483647].", exception.Message);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("     ")]
        public void ParseValueFromEmptyStringSetsValueToZero(string textToParse)
        {
            Assert.AreEqual(0, HydFileStringValueParser.Parse<int>(textToParse));
        }

        [Test]
        [TestCase("-123456789", -123456789)]
        [TestCase("987321654", 987321654)]
        public void ParseValueFromIntegerStringSetsValueToThatValue(string textToParse, int expectedValue)
        {
            Assert.AreEqual(expectedValue, HydFileStringValueParser.Parse<int>(textToParse));
        }

        [Test]
        [TestCaseSource(typeof(CommonTestCaseSource), nameof(CommonTestCaseSource.NullOrWhiteSpace))]
        public void ParseValueFromEmptyStringSetsValueToNewTimeSpan(string textToParse)
        {
            Assert.AreEqual(new TimeSpan(), HydFileStringValueParser.Parse<TimeSpan>(textToParse));
        }

        [Test]
        [TestCase("'00000000010000'", 00, 01, 00, 00)]
        [TestCase("'10675199024805'", 10675199, 2, 48, 05)]
        [TestCase("'10675198123456'", 10675198, 12, 34, 56)]
        public void ParseValueFromCustomTimeDateStringSetsValueToThatTimeSpan(string textToParse,
                                                                              int expectedDay, int expectedHour, int expectedMinute, int expectedSecond)
        {
            var expected = new TimeSpan(expectedDay, expectedHour, expectedMinute, expectedSecond);
            Assert.AreEqual(expected, HydFileStringValueParser.Parse<TimeSpan>(textToParse));
        }

        [Test]
        public void ParseValueFromNonTimeSpanThrowsFormatException()
        {
            // setup
            const string textToParse = "Definitely not a date nor time!";

            // call
            TestDelegate call = () => HydFileStringValueParser.Parse<TimeSpan>(textToParse);

            // assert
            var exception = Assert.Throws<FormatException>(call);
            Assert.AreEqual(string.Format("Timespan ({0}) is not given in expected format of 'ddddddddhhmmss'.", textToParse), exception.Message);
        }

        [Test]
        public void ParseTooBigTimeSpanThrowsFormatException()
        {
            // setup
            const string textToParse = "'10675199024806'";

            // call
            TestDelegate call = () => HydFileStringValueParser.Parse<TimeSpan>(textToParse);

            // assert
            var exception = Assert.Throws<FormatException>(call);

            Assert.AreEqual("Timespan must be smaller or equal to value '10675199024805'.", exception.Message);
            Assert.IsInstanceOf<ArgumentOutOfRangeException>(exception.InnerException,
                                                             "Should capture ArgumentOfOfRangeException and wrap it into FormatException.");
        }
    }
}