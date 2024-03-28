using System;
using DHYDRO.Common.IO.Ini.Converters;
using NUnit.Framework;
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;

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

            Assert.That(result, Is.EqualTo("True"));
        }

        [Test]
        public void ConvertToString_IntegerValue_ReturnsFormattedString()
        {
            const int value = 42;

            string result = IniValueConverter.ConvertToString(value);

            Assert.That(result, Is.EqualTo("42"));
        }

        [Test]
        public void ConvertToString_FloatValue_ReturnsFormattedString()
        {
            const float value = 3.14f;

            string result = IniValueConverter.ConvertToString(value);

            Assert.That(result, Is.EqualTo("3.1400001e+000"));
        }

        [Test]
        public void ConvertToString_DoubleValue_ReturnsFormattedString()
        {
            const double value = 2.718281828;

            string result = IniValueConverter.ConvertToString(value);

            Assert.That(result, Is.EqualTo("2.7182818e+000"));
        }

        [Test]
        public void ConvertToString_DateTimeValue_ReturnsFormattedString()
        {
            var value = new DateTime(2023, 8, 14, 15, 30, 0);

            string result = IniValueConverter.ConvertToString(value);

            Assert.That(result, Is.EqualTo("2023-08-14 15:30:00"));
        }

        [Test]
        public void ConvertToString_EnumValue_ReturnsToString()
        {
            const DescriptionEnum value = DescriptionEnum.Option1;

            string result = IniValueConverter.ConvertToString(value);

            Assert.That(result, Is.EqualTo("Option1"));
        }

        [Test]
        public void ConvertToString_StringValue_ReturnsToString()
        {
            const string value = "Hello, World!";

            string result = IniValueConverter.ConvertToString(value);

            Assert.That(result, Is.EqualTo("Hello, World!"));
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void ConvertToString_StringValueIsNullOrEmpty_ReturnsEmptyString(string value)
        {
            string result = IniValueConverter.ConvertToString(value);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void ConvertFromString_NullBooleanString_ThrowsArgumentNullException()
        {
            Assert.That(() => IniValueConverter.ConvertFromString<bool>(null), Throws.ArgumentNullException);
        }

        [Test]
        [TestCase("")]
        [TestCase("invalid")]
        public void ConvertFromString_InvalidBooleanFormattedString_ThrowsFormatException(string value)
        {
            Assert.That(() => IniValueConverter.ConvertFromString<bool>(value), Throws.TypeOf<FormatException>());
        }

        [Test]
        [TestCase("True", ExpectedResult = true)]
        [TestCase("TRUE", ExpectedResult = true)]
        [TestCase("false", ExpectedResult = false)]
        [TestCase("YES", ExpectedResult = true)]
        [TestCase("yes", ExpectedResult = true)]
        [TestCase("no", ExpectedResult = false)]
        [TestCase("1", ExpectedResult = true)]
        [TestCase("0", ExpectedResult = false)]
        [TestCase("-1", ExpectedResult = true)]
        public bool ConvertFromString_BooleanFormattedString_ReturnsBooleanValue(string value)
        {
            return IniValueConverter.ConvertFromString<bool>(value);
        }

        [Test]
        [TestCase(" True", ExpectedResult = true)]
        [TestCase("false  ", ExpectedResult = false)]
        [TestCase(" yes   ", ExpectedResult = true)]
        [TestCase("no ", ExpectedResult = false)]
        [TestCase(" 1", ExpectedResult = true)]
        [TestCase("0 ", ExpectedResult = false)]
        [TestCase(" -1 ", ExpectedResult = true)]
        public bool ConvertFromString_BooleanFormattedStringWithSpaces_ReturnsBooleanValue(string value)
        {
            return IniValueConverter.ConvertFromString<bool>(value);
        }

        [Test]
        public void ConvertFromString_NullIntegerString_ThrowsArgumentNullException()
        {
            Assert.That(() => IniValueConverter.ConvertFromString<int>(null), Throws.ArgumentNullException);
        }

        [Test]
        [TestCase("")]
        [TestCase("invalid")]
        public void ConvertFromString_InvalidIntegerFormattedString_ThrowsFormatException(string value)
        {
            Assert.That(() => IniValueConverter.ConvertFromString<int>(value), Throws.TypeOf<FormatException>());
        }

        [Test]
        public void ConvertFromString_IntegerFormattedString_ReturnsIntegerValue()
        {
            const string value = "42";

            var result = IniValueConverter.ConvertFromString<int>(value);

            Assert.That(result, Is.EqualTo(42));
        }

        [Test]
        public void ConvertFromString_NullDoubleString_ThrowsArgumentNullException()
        {
            Assert.That(() => IniValueConverter.ConvertFromString<double>(null), Throws.ArgumentNullException);
        }

        [Test]
        [TestCase("")]
        [TestCase("invalid")]
        public void ConvertFromString_InvalidDoubleFormattedString_ThrowsFormatException(string value)
        {
            Assert.That(() => IniValueConverter.ConvertFromString<double>(value), Throws.TypeOf<FormatException>());
        }

        [Test]
        public void ConvertFromString_DoubleFormattedString_ReturnsDoubleValue()
        {
            const string value = "3.34343e+000";

            var result = IniValueConverter.ConvertFromString<double>(value);

            Assert.That(result, Is.EqualTo(3.34343d));
        }

        [Test]
        public void ConvertFromString_NullFloatString_ThrowsArgumentNullException()
        {
            Assert.That(() => IniValueConverter.ConvertFromString<float>(null), Throws.ArgumentNullException);
        }

        [Test]
        [TestCase("")]
        [TestCase("invalid")]
        public void ConvertFromString_InvalidFloatFormattedString_ThrowsFormatException(string value)
        {
            Assert.That(() => IniValueConverter.ConvertFromString<float>(value), Throws.TypeOf<FormatException>());
        }

        [Test]
        public void ConvertFromString_FloatFormattedString_ReturnsFloatValue()
        {
            const string value = "3.14";

            var result = IniValueConverter.ConvertFromString<float>(value);

            Assert.That(result, Is.EqualTo(3.14f));
        }

        [Test]
        public void ConvertFromString_NullDateTimeString_ThrowsArgumentNullException()
        {
            Assert.That(() => IniValueConverter.ConvertFromString<DateTime>(null), Throws.ArgumentNullException);
        }

        [Test]
        [TestCase("")]
        [TestCase("invalid")]
        public void ConvertFromString_InvalidDateTimeFormattedString_ThrowsFormatException(string value)
        {
            Assert.That(() => IniValueConverter.ConvertFromString<DateTime>(value), Throws.TypeOf<FormatException>());
        }

        [Test]
        public void ConvertFromString_DateTimeFormattedString_ReturnsDateTimeValue()
        {
            const string value = "2023-08-14 12:00:00";

            var result = IniValueConverter.ConvertFromString<DateTime>(value);

            Assert.That(result, Is.EqualTo(new DateTime(2023, 8, 14, 12, 0, 0)));
        }

        [Test]
        public void ConvertFromString_NullEnumString_ThrowsArgumentNullException()
        {
            Assert.That(() => IniValueConverter.ConvertFromString<DescriptionEnum>(null), Throws.ArgumentNullException);
        }

        [Test]
        [TestCase("")]
        [TestCase("-1")]
        [TestCase("invalid")]
        [TestCase("Option")]
        [TestCase("DescriptionEnum")]
        [TestCase("First Option")]
        [TestCase("Option Description")]
        [TestCase("FirstOptionDescription")]
        public void ConvertFromString_InvalidEnumFormattedString_ThrowsFormatException(string value)
        {
            Assert.That(() => IniValueConverter.ConvertFromString<DescriptionEnum>(value), Throws.TypeOf<FormatException>());
        }

        [Test]
        [TestCase("option1 ", ExpectedResult = DescriptionEnum.Option1)]
        [TestCase(" option2", ExpectedResult = DescriptionEnum.Option2)]
        [TestCase(" option3 ", ExpectedResult = DescriptionEnum.Option3)]
        public DescriptionEnum ConvertFromString_EnumValueFormattedStringWithSpaces_ReturnsEnumValue(string value)
        {
            return IniValueConverter.ConvertFromString<DescriptionEnum>(value);
        }

        [Test]
        [TestCase("0", ExpectedResult = DescriptionEnum.Option1)]
        [TestCase("option2", ExpectedResult = DescriptionEnum.Option2)]
        [TestCase("OPTION3", ExpectedResult = DescriptionEnum.Option3)]
        public DescriptionEnum ConvertFromString_EnumValueFormattedString_ReturnsEnumValue(string value)
        {
            return IniValueConverter.ConvertFromString<DescriptionEnum>(value);
        }

        [Test]
        [TestCase("First Option Description", ExpectedResult = DescriptionEnum.Option1)]
        [TestCase("second option description", ExpectedResult = DescriptionEnum.Option2)]
        [TestCase("THIRD OPTION DESCRIPTION", ExpectedResult = DescriptionEnum.Option3)]
        public DescriptionEnum ConvertFromString_EnumDescriptionFormattedString_ReturnsEnumValue(string value)
        {
            return IniValueConverter.ConvertFromString<DescriptionEnum>(value);
        }

        [Test]
        public void ConvertFromString_NullStringValue_ThrowsArgumentNullException()
        {
            Assert.That(() => IniValueConverter.ConvertFromString<string>(null), Throws.ArgumentNullException);
        }

        [Test]
        [TestCase("")]
        [TestCase("Hello, World!")]
        public void ConvertFromString_StringValue_ReturnsStringValue(string value)
        {
            var result = IniValueConverter.ConvertFromString<string>(value);

            Assert.That(result, Is.EqualTo(value));
        }

        public enum DescriptionEnum
        {
            [Description("First Option Description")]
            Option1,

            [Description("Second Option Description")]
            Option2,

            [Description("Third Option Description")]
            Option3
        }
    }
}