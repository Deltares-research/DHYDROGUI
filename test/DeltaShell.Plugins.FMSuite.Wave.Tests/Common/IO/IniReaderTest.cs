using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.Ini;
using DeltaShell.NGHS.IO.Properties;
using DHYDRO.Common.IO.Ini;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Common.IO
{
    [TestFixture]
    public class IniReaderTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ParseMdwFile()
        {
            var iniReader = new IniReader();
            string mdwFilePath = TestHelper.GetTestFilePath(@"wave_timespacevarbnd\tst.mdw");

            IniData iniData;
            using (var fileStream = new FileStream(mdwFilePath, FileMode.Open, FileAccess.Read))
            {
                iniData = iniReader.ReadIniFile(fileStream, mdwFilePath);
            }

            Assert.AreEqual(2, iniData.GetAllSections("Domain").Count());
            Assert.AreEqual(2, iniData.GetAllSections("Boundary").Count());
            Assert.AreEqual(3, iniData.GetAllSections("TimePoint").Count());
            Assert.AreEqual(13, iniData.SectionCount);

            IniSection innerDomain = iniData.GetAllSections("Domain").ToList()[1];
            Assert.AreEqual(85, innerDomain.LineNumber);

            IniProperty bedLevelProperty = innerDomain.GetProperty("BedLevel");
            Assert.AreEqual(88, bedLevelProperty.LineNumber);
            Assert.AreEqual("inner.dep", bedLevelProperty.Value);

            Assert.AreEqual("36", innerDomain.GetPropertyValueOrDefault("NDir"));
            Assert.AreEqual(null, innerDomain.GetPropertyValueOrDefault("harazafraz"));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAnIniFileWithEqualsCharsInTheName_WhenReading_ThenItIsSCorrectlyInterpretedAsAString()
        {
            string testFilePath = TestHelper.GetTestFilePath(@"IniReaderTest\NetworkDefinition.ini");

            IniData iniData;
            using (var fileStream = new FileStream(testFilePath, FileMode.Open, FileAccess.Read))
            {
                iniData = new IniReader().ReadIniFile(fileStream, testFilePath);
            }

            List<IniSection> sections = iniData.Sections.ToList();
            Assert.That(sections.Count, Is.EqualTo(4));

            Assert.That(sections[1].Name, Is.EqualTo("Node=1"));
            Assert.That(sections[1].Properties.FirstOrDefault()?.Value, Is.EqualTo("Node001=6"));

            Assert.That(sections[2].Name, Is.EqualTo("Node"));
            Assert.That(sections[2].Properties.FirstOrDefault()?.Value, Is.EqualTo("T1_B1_Manhole_x=1000m"));
        }

        [Test]
        public void GivenAnIniFileWhichDoesNotMatchTheRegexPattern_WhenGettingKeyValueComment_ThenAFormatExceptionIsThrown()
        {
            // Setup
            string testFilePath = TestHelper.GetTestFilePath(@"IniReaderTest\NetworkDefinitionNotMatchingRegex.ini");

            // Call
            void Call()
            {
                using (var fileStream = new FileStream(testFilePath, FileMode.Open, FileAccess.Read))
                {
                    new IniReader().ReadIniFile(fileStream, testFilePath);
                }
            }

            // Assert
            Assert.Throws<FormatException>(Call, string.Format(
                                               Resources.IniReader_GetKeyValueComment_Invalid_key_value_comment_line_on_line__0__in_file__1_,
                                               1, testFilePath));
        }
    }
}