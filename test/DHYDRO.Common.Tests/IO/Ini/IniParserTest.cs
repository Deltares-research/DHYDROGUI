using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DHYDRO.Common.IO.Ini;
using DHYDRO.Common.IO.Ini.Configuration;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.Ini
{
    [TestFixture]
    public class IniParserTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            IniParser iniParser = CreateParser();

            Assert.That(iniParser.Configuration, Is.Not.Null);
            Assert.That(iniParser.Scheme, Is.Not.Null);
        }

        [Test]
        public void Configuration_SetToNull_ThrowsArgumentNullException()
        {
            IniParser iniParser = CreateParser();

            Assert.Throws<ArgumentNullException>(() => iniParser.Configuration = null);
        }

        [Test]
        public void Configuration_SetToValidConfiguration_ReturnsSameInstance()
        {
            IniParseConfiguration configuration = CreateConfiguration();
            IniParser iniParser = CreateParser();

            iniParser.Configuration = configuration;

            Assert.That(configuration, Is.SameAs(iniParser.Configuration));
        }

        [Test]
        public void Scheme_SetToNull_ThrowsArgumentNullException()
        {
            IniParser iniParser = CreateParser();

            Assert.Throws<ArgumentNullException>(() => iniParser.Scheme = null);
        }

        [Test]
        public void Scheme_SetToValidScheme_ReturnsSameInstance()
        {
            IniScheme scheme = CreateScheme();
            IniParser iniParser = CreateParser();

            iniParser.Scheme = scheme;

            Assert.That(scheme, Is.SameAs(iniParser.Scheme));
        }

        [Test]
        public void Parse_IniStringIsNull_ThrowsArgumentNullException()
        {
            IniParser iniParser = CreateParser();

            Assert.Throws<ArgumentNullException>(() => iniParser.Parse((string)null));
        }

        [Test]
        public void Parse_StreamIsNull_ThrowsArgumentNullException()
        {
            IniParser iniParser = CreateParser();

            Assert.Throws<ArgumentNullException>(() => iniParser.Parse((Stream)null));
        }

        [Test]
        public void Parse_TextReaderIsNull_ThrowsArgumentNullException()
        {
            IniParser iniParser = CreateParser();

            Assert.Throws<ArgumentNullException>(() => iniParser.Parse((TextReader)null));
        }

        [Test]
        public void Parse_EmptyString_ReturnsIniDataWithoutSections()
        {
            IniParser iniParser = CreateParser();

            IniData iniData = iniParser.Parse(string.Empty);

            Assert.That(iniData, Is.Not.Null);
            Assert.That(iniData.Sections, Is.Empty);
        }

        [Test]
        public void Parse_EmptyLinesString_ReturnsIniDataWithoutSections()
        {
            IniParser iniParser = CreateParser();

            IniData iniData = iniParser.Parse(@"


");

            Assert.That(iniData, Is.Not.Null);
            Assert.That(iniData.Sections, Is.Empty);
        }

        [Test]
        [TestCase("[section]")]
        [TestCase(" [section] ")]
        [TestCase("\t[section]\t")]
        public void Parse_ValidSectionFormat_IniDataHasSection(string ini)
        {
            IniParser iniParser = CreateParser();

            IniData iniData = iniParser.Parse(ini);

            Assert.That(iniData.ContainsSection("section"));
        }

        [Test]
        [TestCase("section#1")]
        [TestCase("section-1")]
        [TestCase("section²")]
        [TestCase("section\\subsection")]
        [TestCase("section~subsection")]
        [TestCase("section*subsection")]
        [TestCase("section \" xyz")]
        [TestCase("#section#")]
        [TestCase("s][e[c]t][i[]on[")]
        [TestCase("https://example.com/page")]
        [TestCase("{C3BA7795-F319-4CC0-B091-783DDEBCCDF1}")]
        public void Parse_SpecialCharactersInSectionName_IniDataHasSection(string sectionName)
        {
            IniParser iniParser = CreateParser();

            IniData iniData = iniParser.Parse($"[{sectionName}]");

            Assert.That(iniData.ContainsSection(sectionName));
        }

        [Test]
        [TestCase("[section")]
        [TestCase("[section[")]
        [TestCase("]section[")]
        [TestCase("a[section]")]
        public void Parse_InvalidSectionFormat_ThrowsFormatException(string ini)
        {
            IniParser iniParser = CreateParser();

            Assert.Throws<FormatException>(() => iniParser.Parse(ini));
        }

        [Test]
        [TestCase("[]")]
        [TestCase("[ ]")]
        [TestCase("[\t\t]")]
        public void Parse_EmptySectionName_ThrowsFormatException(string ini)
        {
            IniParser iniParser = CreateParser();

            Assert.Throws<FormatException>(() => iniParser.Parse(ini));
        }

        [Test]
        public void Parse_DuplicateSectionNamesAndAllowDuplicateSectionsIsFalse_ThrowsFormatException()
        {
            IniParser iniParser = CreateParser();

            iniParser.Configuration.AllowDuplicateSections = false;

            const string ini = @"
[section]
[section]";

            Assert.Throws<FormatException>(() => iniParser.Parse(ini));
        }

        [Test]
        public void Parse_DuplicateSectionNamesAndAllowDuplicateSectionsIsTrue_IniDataHasMultipleSections()
        {
            IniParser iniParser = CreateParser();

            iniParser.Configuration.AllowDuplicateSections = true;

            const string ini = @"
[section]
[section]";

            IniData iniData = iniParser.Parse(ini);
            IEnumerable<IniSection> sections = iniData.GetAllSections("section");

            Assert.That(sections.Count(), Is.EqualTo(2));
        }

        [Test]
        public void Parse_MultipleSections_SectionsHaveLineNumbers()
        {
            IniParser iniParser = CreateParser();

            const string ini = @"
[section1]
[section2]";

            IniData iniData = iniParser.Parse(ini);
            IniSection section1 = iniData.FindSection("section1");
            IniSection section2 = iniData.FindSection("section2");

            Assert.That(section1, Is.Not.Null);
            Assert.That(section2, Is.Not.Null);
            Assert.That(section1.LineNumber, Is.EqualTo(2));
            Assert.That(section2.LineNumber, Is.EqualTo(3));
        }

        [Test]
        [TestCase("# section comment")]
        [TestCase(" #section comment ")]
        [TestCase(" # section comment  ")]
        [TestCase("\t\t#section comment\t")]
        public void Parse_SectionWithCommentLine_SectionHasComment(string commentLine)
        {
            IniParser iniParser = CreateParser();

            var ini = $@"
{commentLine}
[section]";

            IniData iniData = iniParser.Parse(ini);
            IniSection section = iniData.FindSection("section");

            Assert.That(section, Is.Not.Null);
            Assert.That(section.Comments, Has.Exactly(1).EqualTo("section comment"));
        }

        [Test]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t")]
        public void Parse_SectionWithEmptyCommentLine_SectionHasEmptyComment(string comment)
        {
            IniParser iniParser = CreateParser();

            var ini = $@"
#{comment}
[section]";

            IniData iniData = iniParser.Parse(ini);
            IniSection section = iniData.FindSection("section");

            Assert.That(section, Is.Not.Null);
            Assert.That(section.Comments, Has.Exactly(1).EqualTo(string.Empty));
        }

        [Test]
        public void Parse_SectionWithCommentLineAndParseCommentsIsFalse_SectionCommentsIsEmpty()
        {
            IniParser iniParser = CreateParser();

            iniParser.Configuration.ParseComments = false;

            const string ini = @"
# section comment
[section]";

            IniData iniData = iniParser.Parse(ini);
            IniSection section = iniData.FindSection("section");

            Assert.That(section, Is.Not.Null);
            Assert.That(section.Comments, Is.Empty);
        }

        [Test]
        public void Parse_SectionWithInlineComment_SectionCommentsIsEmpty()
        {
            IniParser iniParser = CreateParser();

            const string ini = "[section] # inline comment";

            IniData iniData = iniParser.Parse(ini);
            IniSection section = iniData.FindSection("section");

            Assert.That(section, Is.Not.Null);
            Assert.That(section.Comments, Is.Empty);
        }

        [Test]
        public void Parse_PropertyWithoutSection_ThrowsFormatException()
        {
            IniParser iniParser = CreateParser();

            const string ini = "property = value";

            Assert.Throws<FormatException>(() => iniParser.Parse(ini));
        }

        [Test]
        [TestCase("property=value")]
        [TestCase(" property = value")]
        [TestCase("\tproperty\t=\tvalue")]
        public void Parse_ValidPropertyFormat_SectionHasProperty(string propertyLine)
        {
            IniParser iniParser = CreateParser();

            var ini = $@"
[section]
{propertyLine}";

            IniData iniData = iniParser.Parse(ini);
            IniSection section = iniData.Sections.First();
            IniProperty property = section.FindProperty("property");

            Assert.That(property, Is.Not.Null);
            Assert.That(property.Value, Is.EqualTo("value"));
        }

        [Test]
        [TestCase("property_1")]
        [TestCase("property-1")]
        [TestCase("property~1")]
        [TestCase("property*1")]
        [TestCase("property.1")]
        [TestCase("property#1")]
        [TestCase("property\\1")]
        [TestCase("property²")]
        [TestCase("p][r[o]p][e[]rt[y")]
        public void Parse_SpecialCharactersInPropertyKey_SectionHasProperty(string propertyKey)
        {
            IniParser iniParser = CreateParser();

            var ini = $@"
[section]
{propertyKey}=value";

            IniData iniData = iniParser.Parse(ini);
            IniSection section = iniData.Sections.First();
            IniProperty property = section.FindProperty(propertyKey);

            Assert.That(property, Is.Not.Null);
            Assert.That(property.Value, Is.EqualTo("value"));
        }

        [Test]
        [TestCase("=value")]
        [TestCase(" = value")]
        [TestCase("\t=value")]
        [TestCase("property with spaces = value")]
        [TestCase("property\twith\ttabs=value")]
        public void Parse_InvalidPropertyFormat_ThrowsFormatException(string propertyLine)
        {
            IniParser iniParser = CreateParser();

            var ini = $@"
[section]
{propertyLine}";

            Assert.Throws<FormatException>(() => iniParser.Parse(ini));
        }

        [Test]
        [TestCase("property with spaces")]
        [TestCase("property\twith\ttabs")]
        public void Parse_PropertyKeyWithSpacesAndAllowPropertyKeysWithSpacesIsTrue_SectionHasProperty(string propertyKey)
        {
            IniParser iniParser = CreateParser();

            iniParser.Configuration.AllowPropertyKeysWithSpaces = true;

            var ini = $@"
[section]
{propertyKey}=value";

            IniData iniData = iniParser.Parse(ini);
            IniSection section = iniData.Sections.First();
            IniProperty property = section.FindProperty(propertyKey.Replace('\t', ' '));

            Assert.That(property, Is.Not.Null);
            Assert.That(property.Value, Is.EqualTo("value"));
        }

        [Test]
        public void Parse_DuplicatePropertyKeysAndAllowDuplicatePropertiesIsFalse_ThrowsFormatException()
        {
            IniParser iniParser = CreateParser();

            iniParser.Configuration.AllowDuplicateProperties = false;

            const string ini = @"
[section]
property=value1
property=value2";

            Assert.Throws<FormatException>(() => iniParser.Parse(ini));
        }

        [Test]
        public void Parse_DuplicatePropertyKeysAndAllowDuplicatePropertiesIsTrue_SectionHasProperties()
        {
            IniParser iniParser = CreateParser();

            iniParser.Configuration.AllowDuplicateProperties = true;

            const string ini = @"
[section]
property=value1
property=value2";

            IniData iniData = iniParser.Parse(ini);
            IniSection section = iniData.Sections.First();
            IEnumerable<IniProperty> properties = section.GetAllProperties("property");

            Assert.That(properties.Count(), Is.EqualTo(2));
        }

        [Test]
        public void Parse_MultipleProperties_PropertiesHaveLineNumbers()
        {
            IniParser iniParser = CreateParser();

            const string ini = @"
[section]
property1=value1
property2=value2";

            IniData iniData = iniParser.Parse(ini);
            IniSection section = iniData.Sections.First();
            IniProperty property1 = section.FindProperty("property1");
            IniProperty property2 = section.FindProperty("property2");

            Assert.That(property1, Is.Not.Null);
            Assert.That(property2, Is.Not.Null);
            Assert.That(property1.LineNumber, Is.EqualTo(3));
            Assert.That(property2.LineNumber, Is.EqualTo(4));
        }

        [Test]
        public void Parse_PropertyWithoutValue_PropertyHasEmptyValue()
        {
            IniParser iniParser = CreateParser();

            const string ini = @"
[section]
property=";

            IniData iniData = iniParser.Parse(ini);
            IniSection section = iniData.Sections.First();
            IniProperty property = section.FindProperty("property");

            Assert.That(property, Is.Not.Null);
            Assert.That(property.Value, Is.Empty);
        }

        [Test]
        [TestCase("value-1")]
        [TestCase("value ¹²³")]
        [TestCase("value\\1")]
        [TestCase("value \" xyz")]
        [TestCase("v][a[l]u][e[")]
        [TestCase("https://example.com/page")]
        [TestCase("{C3BA7795-F319-4CC0-B091-783DDEBCCDF1}")]
        public void Parse_SpecialCharactersInPropertyValue_SectionHasProperty(string propertyValue)
        {
            IniParser iniParser = CreateParser();

            var ini = $@"
[section]
property={propertyValue}";

            IniData iniData = iniParser.Parse(ini);
            IniSection section = iniData.Sections.First();
            IniProperty property = section.FindProperty("property");

            Assert.That(property, Is.Not.Null);
            Assert.That(property.Value, Is.EqualTo(propertyValue));
        }

        [Test]
        public void Parse_PropertyWithCommentLines_CommentLinesAreIgnored()
        {
            IniParser iniParser = CreateParser();

            const string ini = @"
[section]
# property comment 1
# property comment 2
key=value";

            IniData iniData = iniParser.Parse(ini);
            IniSection section = iniData.Sections.First();
            IniProperty property = section.FindProperty("key");

            Assert.That(property, Is.Not.Null);
            Assert.That(section.Comments, Is.Empty);
            Assert.That(property.Comment, Is.Empty);
        }

        [Test]
        public void Parse_PropertyWithInlineComment_PropertyHasComment()
        {
            IniParser iniParser = CreateParser();

            const string ini = @"
[section]
property=value # inline comment";

            IniData iniData = iniParser.Parse(ini);
            IniSection section = iniData.Sections.First();
            IniProperty property = section.FindProperty("property");

            Assert.That(property, Is.Not.Null);
            Assert.That(property.Comment, Is.EqualTo("inline comment"));
        }

        [Test]
        public void Parse_PropertyWithInlineCommentAndParseCommentsIsFalse_PropertyCommentIsEmpty()
        {
            IniParser iniParser = CreateParser();

            iniParser.Configuration.ParseComments = false;

            const string ini = @"
[section]
property=value # inline comment";

            IniData iniData = iniParser.Parse(ini);
            IniSection section = iniData.Sections.First();
            IniProperty property = section.FindProperty("property");

            Assert.That(property, Is.Not.Null);
            Assert.That(property.Comment, Is.Empty);
        }

        [Test]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t")]
        public void Parse_PropertyWithEmptyValueAndComment_PropertyValueAndCommentIsEmpty(string comment)
        {
            IniParser iniParser = CreateParser();

            var ini = $@"
[section]
property=#{comment}";

            IniData iniData = iniParser.Parse(ini);
            IniSection section = iniData.Sections.First();
            IniProperty property = section.FindProperty("property");

            Assert.That(property, Is.Not.Null);
            Assert.That(property.Value, Is.Empty);
            Assert.That(property.Comment, Is.Empty);
        }

        [Test]
        public void Parse_PropertyWithDelimitedValueAndCleanDelimitedValuesIsTrue_PropertyValueIsCleaned()
        {
            IniParser iniParser = CreateParser();

            iniParser.Configuration.CleanDelimitedValues = true;

            const string ini = @"
[section]
property=#value# # comment";

            IniData iniData = iniParser.Parse(ini);
            IniSection section = iniData.Sections.First();
            IniProperty property = section.FindProperty("property");

            Assert.That(property, Is.Not.Null);
            Assert.That(property.Value, Is.EqualTo("value"));
            Assert.That(property.Comment, Is.EqualTo("comment"));
        }

        [Test]
        [TestCase('\n')]
        [TestCase('\\')]
        public void Parse_PropertyWithMultiLineValue_PropertyHasMultiLineValue(char multiLineValueDelimiter)
        {
            IniParser iniParser = CreateParser();

            iniParser.Configuration.AllowMultiLineValues = true;
            iniParser.Scheme.MultiLineValueDelimiter = multiLineValueDelimiter;

            var ini = $@"
[section]
property=value1 {multiLineValueDelimiter}
value2 {multiLineValueDelimiter}
value3 {multiLineValueDelimiter}";

            IniData iniData = iniParser.Parse(ini);
            IniSection section = iniData.Sections.First();
            IniProperty property = section.FindProperty("property");

            Assert.That(property, Is.Not.Null);
            Assert.That(property.Value, Is.EqualTo(@"value1
value2
value3"));
        }

        [Test]
        public void Parse_MultiLineValueWithoutProperty_ThrowsFormatException()
        {
            IniParser iniParser = CreateParser();

            iniParser.Configuration.AllowMultiLineValues = true;

            const string ini = @"
[section]
value1 \
value2 \
value3";

            Assert.Throws<FormatException>(() => iniParser.Parse(ini));
        }

        [Test]
        public void Parse_PropertyWithMultiLineValueAndAllowMultiLineValuesIsFalse_ThrowsFormatException()
        {
            IniParser iniParser = CreateParser();

            iniParser.Configuration.AllowMultiLineValues = false;

            const string ini = @"
[section]
property=value1 \
value2 \
value3";

            Assert.Throws<FormatException>(() => iniParser.Parse(ini));
        }

        [Test]
        public void Parse_PropertyWithMultiLineValueAndCommentLines_CommentLinesAreIgnored()
        {
            IniParser iniParser = CreateParser();

            iniParser.Configuration.AllowMultiLineValues = true;

            const string ini = @"
[section]
property=value1 \
# value comment 1
value2 \
# value comment 2
value3";

            IniData iniData = iniParser.Parse(ini);
            IniSection section = iniData.Sections.First();
            IniProperty property = section.FindProperty("property");

            Assert.That(property, Is.Not.Null);
            Assert.That(section.Comments, Is.Empty);
            Assert.That(property.Comment.Trim(), Is.Empty);
            Assert.That(property.Value, Is.EqualTo(@"value1
value2
value3"));
        }

        [Test]
        public void Parse_PropertyWithMultiLineValueAndInlineComment_PropertyHasComment()
        {
            IniParser iniParser = CreateParser();

            iniParser.Configuration.AllowMultiLineValues = true;

            const string ini = @"
[section]
property=value1 \ # comment1
value2 \ # comment2
value3 # comment3";

            IniData iniData = iniParser.Parse(ini);
            IniSection section = iniData.Sections.First();
            IniProperty property = section.FindProperty("property");

            Assert.That(property, Is.Not.Null);
            Assert.That(property.Comment, Is.EqualTo(@"comment1
comment2
comment3"));
            Assert.That(property.Value, Is.EqualTo(@"value1
value2
value3"));
        }

        [Test]
        public void Parse_PropertyWithMultiLineValueAndInlineCommentAndParseCommentsIsFalse_PropertyCommentIsEmpty()
        {
            IniParser iniParser = CreateParser();

            iniParser.Configuration.AllowMultiLineValues = true;
            iniParser.Configuration.ParseComments = false;

            const string ini = @"
[section]
property=value1 \ # comment1
value2 \ # comment2
value3 # comment3";

            IniData iniData = iniParser.Parse(ini);
            IniSection section = iniData.Sections.First();
            IniProperty property = section.FindProperty("property");

            Assert.That(property, Is.Not.Null);
            Assert.That(property.Comment.Trim(), Is.Empty);
        }

        [Test]
        public void Parse_WithIniSchemeConfigured_ReturnsFormattedString()
        {
            IniParser iniParser = CreateParser();

            iniParser.Configuration.AllowMultiLineValues = true;
            iniParser.Scheme.SectionStartDelimiter = '<';
            iniParser.Scheme.SectionEndDelimiter = '>';
            iniParser.Scheme.PropertyAssignmentDelimiter = ':';
            iniParser.Scheme.CommentDelimiter = ';';
            iniParser.Scheme.MultiLineValueDelimiter = '\n';

            const string ini = @"
<section>
property1:value1 ; comment1
value2 ; comment2";

            IniData iniData = iniParser.Parse(ini);

            var expected = new IniData();
            var section = new IniSection("section") { LineNumber = 2 };
            var property = new IniProperty("property1", "value1\r\nvalue2", "comment1\r\ncomment2") { LineNumber = 3 };
            section.AddProperty(property);
            expected.AddSection(section);

            Assert.That(iniData, Is.EqualTo(expected));
        }

        [Test]
        public void Parse_ValidStream_KeepsStreamOpen()
        {
            IniParser iniParser = CreateParser();

            using (var stream = new MemoryStream())
            {
                iniParser.Parse(stream);

                Assert.That(stream.CanRead);
                Assert.That(stream.CanWrite);
            }
        }

        [Test]
        public void Parse_AnsiEncodedTextWithUnicodeCharacters_ReadsFromStream()
        {
            IniParser iniParser = CreateParser();

            const string ini = @"
[section]
property1=value¹²³";

            IniData iniData;
            using (var stream = new MemoryStream(Encoding.Default.GetBytes(ini)))
            {
                iniData = iniParser.Parse(stream);
            }

            IniSection section = iniData.Sections.First();
            IniProperty property = section.FindProperty("property1");

            Assert.That(property, Is.Not.Null);
            Assert.That(property.Value, Is.EqualTo("value���"));
        }

        [Test]
        [TestCaseSource(nameof(CreateSectionWithProperties))]
        public IniData Parse_SectionWithProperties_ReadsFromStream(string ini)
        {
            IniParser iniParser = CreateParser();

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(ini)))
            {
                return iniParser.Parse(stream);
            }
        }

        [Test]
        public void Parse_ValidStreamReaderFromStream_KeepsStreamOpen()
        {
            IniParser iniParser = CreateParser();

            using (var stream = new MemoryStream())
            using (var streamReader = new StreamReader(stream))
            {
                iniParser.Parse(streamReader);

                Assert.That(stream.CanRead);
                Assert.That(stream.CanWrite);
            }
        }

        [Test]
        [TestCaseSource(nameof(CreateSectionWithProperties))]
        public IniData Parse_SectionWithProperties_ReadsFromStreamReader(string ini)
        {
            IniParser iniParser = CreateParser();

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(ini)))
            using (var streamReader = new StreamReader(stream))
            {
                return iniParser.Parse(streamReader);
            }
        }

        private static IEnumerable<TestCaseData> CreateSectionWithProperties()
        {
            yield return GenerateTestCaseData("TestProperty", "TestValue").SetName("PropertyWithValue");
            yield return GenerateTestCaseData("property¹²³", "value¹²³").SetName("PropertyWithUnicodeCharacters");
            yield return GenerateTestCaseData("p][r[o]p][e[]rt[y", "v][a[l]u][e[]").SetName("PropertyWithSpecialCharacters");
            yield break;

            TestCaseData GenerateTestCaseData(string propertyKey, string propertyValue)
            {
                var ini = $@"[section]
{propertyKey}1={propertyValue}1
{propertyKey}2={propertyValue}2";

                var expected = new IniData();
                var section = new IniSection("section") { LineNumber = 1 };
                var property1 = new IniProperty($"{propertyKey}1", $"{propertyValue}1") { LineNumber = 2 };
                var property2 = new IniProperty($"{propertyKey}2", $"{propertyValue}2") { LineNumber = 3 };
                section.AddProperty(property1);
                section.AddProperty(property2);
                expected.AddSection(section);

                return new TestCaseData(ini).Returns(expected);
            }
        }

        private static IniParser CreateParser()
        {
            return new IniParser();
        }

        private static IniParseConfiguration CreateConfiguration()
        {
            return new IniParseConfiguration();
        }

        private static IniScheme CreateScheme()
        {
            return new IniScheme();
        }
    }
}