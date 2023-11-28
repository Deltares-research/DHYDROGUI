using System;
using DHYDRO.Common.IO.Ini.Converters;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.Ini.Converters
{
    [TestFixture]
    public class IniValueConverterTest
    {
        [Test]
        public void ConvertToString_BooleanValue_ReturnsFormattedString()
        {
            const bool value = true;

            string result = IniValueConverter.ConvertToString(value);

            Assert.AreEqual("True", result);
        }

        [Test]
        public void ConvertToString_IntegerValue_ReturnsFormattedString()
        {
            const int value = 42;

            string result = IniValueConverter.ConvertToString(value);

            Assert.AreEqual("42", result);
        }

        [Test]
        public void ConvertToString_FloatValue_ReturnsFormattedString()
        {
            const float value = 3.14f;

            string result = IniValueConverter.ConvertToString(value);

            Assert.AreEqual("3.1400001e+000", result);
        }

        [Test]
        public void ConvertToString_DoubleValue_ReturnsFormattedString()
        {
            const double value = 2.718281828;

            string result = IniValueConverter.ConvertToString(value);

            Assert.AreEqual("2.7182818e+000", result);
        }

        [Test]
        public void ConvertToString_DateTimeValue_ReturnsFormattedString()
        {
            var value = new DateTime(2023, 8, 14, 15, 30, 0);

            string result = IniValueConverter.ConvertToString(value);

            Assert.AreEqual("2023-08-14 15:30:00", result);
        }

        [Test]
        public void ConvertToString_EnumValue_ReturnsToString()
        {
            const DayOfWeek value = DayOfWeek.Friday;

            string result = IniValueConverter.ConvertToString(value);

            Assert.AreEqual("Friday", result);
        }

        [Test]
        public void ConvertToString_StringValue_ReturnsToString()
        {
            const string value = "Hello, World!";

            string result = IniValueConverter.ConvertToString(value);

            Assert.AreEqual("Hello, World!", result);
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void ConvertToString_StringValueIsNullOrEmpty_ReturnsEmptyString(string value)
        {
            string result = IniValueConverter.ConvertToString(value);

            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public void ConvertFromString_NullBooleanString_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => IniValueConverter.ConvertFromString<bool>(null));
        }

        [Test]
        [TestCase("")]
        [TestCase("invalid")]
        public void ConvertFromString_InvalidBooleanFormattedString_ThrowsFormatException(string value)
        {
            Assert.Throws<FormatException>(() => IniValueConverter.ConvertFromString<bool>(value));
        }

        [Test]
        [TestCase("True", ExpectedResult = true)]
        [TestCase("TRUE", ExpectedResult = true)]
        [TestCase("false", ExpectedResult = false)]
        [TestCase("1", ExpectedResult = true)]
        [TestCase("0", ExpectedResult = false)]
        [TestCase("-1", ExpectedResult = true)]
        public bool ConvertFromString_BooleanFormattedString_ReturnsBooleanValue(string value)
        {
            return IniValueConverter.ConvertFromString<bool>(value);
        }

        [Test]
        public void ConvertFromString_NullIntegerString_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => IniValueConverter.ConvertFromString<int>(null));
        }

        [Test]
        [TestCase("")]
        [TestCase("invalid")]
        public void ConvertFromString_InvalidIntegerFormattedString_ThrowsFormatException(string value)
        {
            Assert.Throws<FormatException>(() => IniValueConverter.ConvertFromString<int>(value));
        }

        [Test]
        public void ConvertFromString_IntegerFormattedString_ReturnsIntegerValue()
        {
            const string value = "42";

            var result = IniValueConverter.ConvertFromString<int>(value);

            Assert.AreEqual(42, result);
        }

        [Test]
        public void ConvertFromString_NullDoubleString_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => IniValueConverter.ConvertFromString<double>(null));
        }

        [Test]
        [TestCase("")]
        [TestCase("invalid")]
        public void ConvertFromString_InvalidDoubleFormattedString_ThrowsFormatException(string value)
        {
            Assert.Throws<FormatException>(() => IniValueConverter.ConvertFromString<double>(value));
        }

        [Test]
        public void ConvertFromString_DoubleFormattedString_ReturnsDoubleValue()
        {
            const string value = "3.34343e+000";

            var result = IniValueConverter.ConvertFromString<double>(value);

            Assert.AreEqual(3.34343d, result);
        }

        [Test]
        public void ConvertFromString_NullFloatString_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => IniValueConverter.ConvertFromString<float>(null));
        }

        [Test]
        [TestCase("")]
        [TestCase("invalid")]
        public void ConvertFromString_InvalidFloatFormattedString_ThrowsFormatException(string value)
        {
            Assert.Throws<FormatException>(() => IniValueConverter.ConvertFromString<float>(value));
        }

        [Test]
        public void ConvertFromString_FloatFormattedString_ReturnsFloatValue()
        {
            const string value = "3.14";

            var result = IniValueConverter.ConvertFromString<float>(value);

            Assert.AreEqual(3.14f, result);
        }

        [Test]
        public void ConvertFromString_NullDateTimeString_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => IniValueConverter.ConvertFromString<DateTime>(null));
        }

        [Test]
        [TestCase("")]
        [TestCase("invalid")]
        public void ConvertFromString_InvalidDateTimeFormattedString_ThrowsFormatException(string value)
        {
            Assert.Throws<FormatException>(() => IniValueConverter.ConvertFromString<DateTime>(value));
        }

        [Test]
        public void ConvertFromString_DateTimeFormattedString_ReturnsDateTimeValue()
        {
            const string value = "2023-08-14 12:00:00";

            var result = IniValueConverter.ConvertFromString<DateTime>(value);

            Assert.AreEqual(new DateTime(2023, 8, 14, 12, 0, 0), result);
        }

        [Test]
        public void ConvertFromString_NullEnumString_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => IniValueConverter.ConvertFromString<DayOfWeek>(null));
        }

        [Test]
        [TestCase("")]
        [TestCase("-1")]
        [TestCase("invalid")]
        [TestCase("DayOfWeek")]
        public void ConvertFromString_InvalidEnumFormattedString_ThrowsFormatException(string value)
        {
            Assert.Throws<FormatException>(() => IniValueConverter.ConvertFromString<DayOfWeek>(value));
        }

        [Test]
        [TestCase("0", ExpectedResult = DayOfWeek.Sunday)]
        [TestCase("monday", ExpectedResult = DayOfWeek.Monday)]
        [TestCase("TUESDAY", ExpectedResult = DayOfWeek.Tuesday)]
        public DayOfWeek ConvertFromString_EnumFormattedString_ReturnsEnumValue(string value)
        {
            return IniValueConverter.ConvertFromString<DayOfWeek>(value);
        }

        [Test]
        public void ConvertFromString_NullStringValue_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => IniValueConverter.ConvertFromString<string>(null));
        }

        [Test]
        [TestCase("")]
        [TestCase("Hello, World!")]
        public void ConvertFromString_StringValue_ReturnsStringValue(string value)
        {
            var result = IniValueConverter.ConvertFromString<string>(value);

            Assert.AreEqual(value, result);
        }
    }
}