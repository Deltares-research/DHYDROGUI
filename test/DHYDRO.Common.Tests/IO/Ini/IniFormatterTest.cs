using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DHYDRO.Common.IO.Ini;
using DHYDRO.Common.IO.Ini.Configuration;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.Ini
{
    [TestFixture]
    public class IniFormatterTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            IniFormatter iniFormatter = CreateFormatter();

            Assert.That(iniFormatter.Configuration, Is.Not.Null);
            Assert.That(iniFormatter.Scheme, Is.Not.Null);
        }

        [Test]
        public void Configuration_SetToNull_ThrowsArgumentNullException()
        {
            IniFormatter iniFormatter = CreateFormatter();

            Assert.Throws<ArgumentNullException>(() => iniFormatter.Configuration = null);
        }

        [Test]
        public void Configuration_SetToValidConfiguration_ReturnsSameInstance()
        {
            IniFormatConfiguration configuration = CreateConfiguration();
            IniFormatter iniFormatter = CreateFormatter();

            iniFormatter.Configuration = configuration;

            Assert.That(configuration, Is.SameAs(iniFormatter.Configuration));
        }

        [Test]
        public void Scheme_SetToNull_ThrowsArgumentNullException()
        {
            IniFormatter iniFormatter = CreateFormatter();

            Assert.Throws<ArgumentNullException>(() => iniFormatter.Scheme = null);
        }

        [Test]
        public void Scheme_SetToValidScheme_ReturnsSameInstance()
        {
            IniScheme scheme = CreateScheme();
            IniFormatter iniFormatter = CreateFormatter();

            iniFormatter.Scheme = scheme;

            Assert.That(scheme, Is.SameAs(iniFormatter.Scheme));
        }

        [Test]
        public void Format_IniDataIsNull_ThrowsArgumentNullException()
        {
            IniFormatter iniFormatter = CreateFormatter();

            Assert.Throws<ArgumentNullException>(() => iniFormatter.Format(null));
        }

        [Test]
        public void Format_IniDataIsNullAndStreamIsNotNull_ThrowsArgumentNullException()
        {
            Stream stream = Stream.Null;
            IniFormatter iniFormatter = CreateFormatter();

            Assert.Throws<ArgumentNullException>(() => iniFormatter.Format(null, stream));
        }

        [Test]
        public void Format_IniDataIsNotNullAndStreamIsNull_ThrowsArgumentNullException()
        {
            IniData iniData = IniDataFixture.CreateEmptyIniData();
            IniFormatter iniFormatter = CreateFormatter();

            Assert.Throws<ArgumentNullException>(() => iniFormatter.Format(iniData, (Stream)null));
        }

        [Test]
        public void Format_IniDataIsNullAndTextWriterIsNotNull_ThrowsArgumentNullException()
        {
            TextWriter writer = TextWriter.Null;
            IniFormatter iniFormatter = CreateFormatter();

            Assert.Throws<ArgumentNullException>(() => iniFormatter.Format(null, writer));
        }

        [Test]
        public void Format_IniDataIsNotNullAndTextWriterIsNull_ThrowsArgumentNullException()
        {
            IniData iniData = IniDataFixture.CreateEmptyIniData();
            IniFormatter iniFormatter = CreateFormatter();

            Assert.Throws<ArgumentNullException>(() => iniFormatter.Format(iniData, (TextWriter)null));
        }

        [Test]
        public void Format_EmptyIniData_ReturnsEmptyString()
        {
            IniData iniData = IniDataFixture.CreateEmptyIniData();
            IniFormatter iniFormatter = CreateFormatter();

            string ini = iniFormatter.Format(iniData);

            Assert.That(ini, Is.Empty);
        }

        [Test]
        public void Format_EmptySection_ReturnsFormattedString()
        {
            IniSection section = IniDataFixture.CreateEmptySection();
            IniData iniData = IniDataFixture.CreateIniData(section);

            IniFormatter iniFormatter = CreateFormatter();

            string ini = iniFormatter.Format(iniData);

            const string expected = @"[section]
";

            Assert.That(ini, Is.EqualTo(expected));
        }

        [Test]
        public void Format_SectionWithComments_ReturnsFormattedString()
        {
            IniSection section = IniDataFixture.CreateEmptySection();
            section.AddComment("comment1");
            section.AddComment("comment2");

            IniData iniData = IniDataFixture.CreateIniData(section);

            IniFormatter iniFormatter = CreateFormatter();

            string ini = iniFormatter.Format(iniData);

            const string expected = @"# comment1
# comment2

[section]
";

            Assert.That(ini, Is.EqualTo(expected));
        }

        [Test]
        public void Format_SectionWithCommentsAndWriteCommentsIsFalse_ReturnsFormattedString()
        {
            IniSection section = IniDataFixture.CreateEmptySection();
            section.AddComment("comment1");
            section.AddComment("comment2");

            IniData iniData = IniDataFixture.CreateIniData(section);

            IniFormatter iniFormatter = CreateFormatter();
            iniFormatter.Configuration.WriteComments = false;

            string ini = iniFormatter.Format(iniData);

            const string expected = @"[section]
";

            Assert.That(ini, Is.EqualTo(expected));
        }

        [Test]
        public void Format_SectionWithPropertiesAndComments_ReturnsFormattedString()
        {
            IniData iniData = IniDataFixture.CreateIniDataWithSingleSection();

            IniFormatter iniFormatter = CreateFormatter();

            string ini = iniFormatter.Format(iniData);

            const string expected = @"[section]
property1             = value1              # comment
property2             = value2              # comment
property3             = value3              # comment

";

            Assert.That(ini, Is.EqualTo(expected));
        }

        [Test]
        [TestCaseSource(nameof(CreatePropertiesWithSpecialCharacters))]
        public string Format_PropertyWithSpecialCharacters_ReturnsFormattedString(string key, string value)
        {
            IniData iniData = IniDataFixture.CreateIniDataFromProperty(key, value, string.Empty);

            IniFormatter iniFormatter = CreateFormatter();

            return iniFormatter.Format(iniData);
        }

        private static IEnumerable<TestCaseData> CreatePropertiesWithSpecialCharacters()
        {
            yield return GenerateTestCaseDate("property-1", "value-1");
            yield return GenerateTestCaseDate("property\\1", "value\\1");
            yield return GenerateTestCaseDate("property¹²³", "value¹²³");
            yield return GenerateTestCaseDate("p][r[o]p][e[]rt[y", "v][a[l]u][e[]");
            yield break;

            TestCaseData GenerateTestCaseDate(string propertyKey, string propertyValue)
            {
                var expected = $@"[section]
{propertyKey,-21} = {propertyValue,-20}

";
                return new TestCaseData(propertyKey, propertyValue).Returns(expected);
            }
        }

        [Test]
        public void Format_SectionWithPropertiesAndCommentsAndWriteCommentsIsFalse_ReturnsFormattedString()
        {
            IniData iniData = IniDataFixture.CreateIniDataWithSingleSection();

            IniFormatter iniFormatter = CreateFormatter();
            iniFormatter.Configuration.WriteComments = false;

            string ini = iniFormatter.Format(iniData);

            const string expected = @"[section]
property1             = value1              
property2             = value2              
property3             = value3              

";

            Assert.That(ini, Is.EqualTo(expected));
        }

        [Test]
        public void Format_WritePropertyWithoutValueIsFalse_ReturnsFormattedString()
        {
            IniData iniData = IniDataFixture.CreateIniDataFromProperty(value: string.Empty);

            IniFormatter iniFormatter = CreateFormatter();
            iniFormatter.Configuration.WritePropertyWithoutValue = false;

            string ini = iniFormatter.Format(iniData);

            const string expected = @"[section]

";

            Assert.That(ini, Is.EqualTo(expected));
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void Format_WritePropertyWithoutValueIsTrue_ReturnsFormattedString(string value)
        {
            IniData iniData = IniDataFixture.CreateIniDataFromProperty(value: value, comment: string.Empty);

            IniFormatter iniFormatter = CreateFormatter();
            iniFormatter.Configuration.WritePropertyWithoutValue = true;

            string ini = iniFormatter.Format(iniData);

            const string expected = @"[section]
property              =                     

";

            Assert.That(ini, Is.EqualTo(expected));
        }

        [Test]
        public void Format_WithPropertyFormattingConfigured_ReturnsFormattedString()
        {
            IniData iniData = IniDataFixture.CreateIniDataWithSingleSection();

            IniFormatter iniFormatter = CreateFormatter();
            iniFormatter.Configuration.PropertyIndentationLevel = 4;
            iniFormatter.Configuration.PropertyKeyWidth = 10;
            iniFormatter.Configuration.PropertyValueWidth = 10;

            string ini = iniFormatter.Format(iniData);

            const string expected = @"[section]
    property1  = value1    # comment
    property2  = value2    # comment
    property3  = value3    # comment

";

            Assert.That(ini, Is.EqualTo(expected));
        }

        [Test]
        public void Format_WithIniSchemeConfigured_ReturnsFormattedString()
        {
            IniData iniData = IniDataFixture.CreateIniDataWithSingleSection();

            IniFormatter iniFormatter = CreateFormatter();
            iniFormatter.Scheme.CommentDelimiter = ';';
            iniFormatter.Scheme.SectionStartDelimiter = '<';
            iniFormatter.Scheme.SectionEndDelimiter = '>';
            iniFormatter.Scheme.PropertyAssignmentDelimiter = ':';

            string ini = iniFormatter.Format(iniData);

            const string expected = @"<section>
property1             : value1              ; comment
property2             : value2              ; comment
property3             : value3              ; comment

";

            Assert.That(ini, Is.EqualTo(expected));
        }

        [Test]
        public void Format_ValidStream_KeepsStreamOpen()
        {
            IniData iniData = IniDataFixture.CreateEmptyIniData();

            IniFormatter iniFormatter = CreateFormatter();

            using (var stream = new MemoryStream())
            {
                iniFormatter.Format(iniData, stream);

                Assert.That(stream.CanRead);
                Assert.That(stream.CanWrite);
            }
        }

        [Test]
        public void Format_SectionWithPropertiesAndComments_WritesToStream()
        {
            IniData iniData = IniDataFixture.CreateIniDataWithSingleSection();

            IniFormatter iniFormatter = CreateFormatter();
            Encoding encoding = iniFormatter.Configuration.Encoding;

            string ini;
            using (var stream = new MemoryStream())
            using (var streamReader = new StreamReader(stream, encoding))
            {
                iniFormatter.Format(iniData, stream);
                stream.Seek(0, SeekOrigin.Begin);
                ini = streamReader.ReadToEnd();
            }

            const string expected = @"[section]
property1             = value1              # comment
property2             = value2              # comment
property3             = value3              # comment

";

            Assert.That(ini, Is.EqualTo(expected));
        }

        [Test]
        [TestCaseSource(nameof(CreatePropertiesWithSpecialCharacters))]
        public string Format_PropertyWithSpecialCharacters_WritesToStream(string key, string value)
        {
            IniData iniData = IniDataFixture.CreateIniDataFromProperty(key, value, string.Empty);

            IniFormatter iniFormatter = CreateFormatter();
            Encoding encoding = iniFormatter.Configuration.Encoding;
            
            using (var stream = new MemoryStream())
            using (var streamReader = new StreamReader(stream, encoding))
            {
                iniFormatter.Format(iniData, stream);
                stream.Seek(0, SeekOrigin.Begin);
                return streamReader.ReadToEnd();
            }
        }

        [Test]
        public void Format_SectionWithPropertiesAndComments_WritesToTextWriter()
        {
            IniData iniData = IniDataFixture.CreateIniDataWithSingleSection();

            IniFormatter iniFormatter = CreateFormatter();

            string ini;
            using (var writer = new StringWriter())
            {
                iniFormatter.Format(iniData, writer);
                ini = writer.ToString();
            }

            const string expected = @"[section]
property1             = value1              # comment
property2             = value2              # comment
property3             = value3              # comment

";

            Assert.That(ini, Is.EqualTo(expected));
        }

        [Test]
        public void Format_ValidTextWriterFromStream_KeepsStreamOpen()
        {
            IniData iniData = IniDataFixture.CreateEmptyIniData();

            IniFormatter iniFormatter = CreateFormatter();

            using (var stream = new MemoryStream())
            using (var streamWriter = new StreamWriter(stream))
            {
                iniFormatter.Format(iniData, streamWriter);

                Assert.That(stream.CanRead);
                Assert.That(stream.CanWrite);
            }
        }

        private static IniFormatter CreateFormatter()
        {
            return new IniFormatter();
        }

        private static IniFormatConfiguration CreateConfiguration()
        {
            return new IniFormatConfiguration();
        }

        private static IniScheme CreateScheme()
        {
            return new IniScheme();
        }
    }
}