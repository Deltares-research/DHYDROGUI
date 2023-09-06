using System;
using DeltaShell.NGHS.IO.Ini;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.Ini
{
    [TestFixture]
    public class IniValueConverterTest
    {
        [Test]
        public void ConvertToString_IntValue_ReturnsFormattedString()
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
        public void ConvertFromString_IntFormattedString_ReturnsIntegerValue()
        {
            const string value = "42";

            var result = IniValueConverter.ConvertFromString<int>(value);

            Assert.AreEqual(42, result);
        }

        [Test]
        public void ConvertFromString_DoubleFormattedString_ReturnsDoubleValue()
        {
            const string value = "3.34343e+000";

            var result = IniValueConverter.ConvertFromString<double>(value);

            Assert.AreEqual(3.34343d, result);
        }

        [Test]
        public void ConvertFromString_FloatFormattedString_ReturnsFloatValue()
        {
            const string value = "3.14";

            var result = IniValueConverter.ConvertFromString<float>(value);

            Assert.AreEqual(3.14f, result);
        }

        [Test]
        public void ConvertFromString_DateTimeFormattedString_ReturnsDateTimeValue()
        {
            const string value = "2023-08-14 12:00:00";

            var result = IniValueConverter.ConvertFromString<DateTime>(value);

            Assert.AreEqual(new DateTime(2023, 8, 14, 12, 0, 0), result);
        }

        [Test]
        public void ConvertFromString_EnumFormattedString_ReturnsEnumValue()
        {
            const string value = "monday";

            var result = IniValueConverter.ConvertFromString<DayOfWeek>(value);

            Assert.AreEqual(DayOfWeek.Monday, result);
        }

        [Test]
        public void ConvertFromString_StringValue_ReturnsStringValue()
        {
            const string value = "Hello, World!";

            var result = IniValueConverter.ConvertFromString<string>(value);

            Assert.AreEqual(value, result);
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void ConvertFromString_StringValueIsNullOrEmpty_ReturnsStringValue(string value)
        {
            var result = IniValueConverter.ConvertFromString<string>(value);

            Assert.AreEqual(value, result);
        }

        [Test]
        public void ConvertFromString_InvalidType_ThrowsException()
        {
            const string value = "TestValue";

            Assert.Throws<FormatException>(() => IniValueConverter.ConvertFromString<int>(value));
            Assert.Throws<FormatException>(() => IniValueConverter.ConvertFromString<float>(value));
            Assert.Throws<FormatException>(() => IniValueConverter.ConvertFromString<double>(value));
            Assert.Throws<FormatException>(() => IniValueConverter.ConvertFromString<DateTime>(value));
            Assert.Throws<ArgumentException>(() => IniValueConverter.ConvertFromString<DayOfWeek>(value));
        }
    }
}