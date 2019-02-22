using System;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.Properties;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Common.IO
{
    [TestFixture]
    public class DelftIniReaderTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ParseMdwFile()
        {
            var delftIniReader = new DelftIniReader();
            var mdwFilePath = TestHelper.GetTestFilePath(@"wave_timespacevarbnd\tst.mdw");
            var categories = delftIniReader.ReadDelftIniFile(mdwFilePath);

            Assert.AreEqual(2, categories.Count(k => k.Name == "Domain"));
            Assert.AreEqual(2, categories.Count(k => k.Name == "Boundary"));
            Assert.AreEqual(3, categories.Count(k => k.Name == "TimePoint"));
            Assert.AreEqual(13, categories.Count);

            var innerDomain = categories.Where(c => c.Name == "Domain").ToList()[1];
            Assert.AreEqual(85, innerDomain.LineNumber);

            var bedLevelProperty = innerDomain.Properties.First(p => p.Name == "BedLevel");
            Assert.AreEqual(88, bedLevelProperty.LineNumber);
            Assert.AreEqual("inner.dep", bedLevelProperty.Value);

            Assert.AreEqual("36", innerDomain.GetPropertyValue("NDir"));
            Assert.AreEqual(null, innerDomain.GetPropertyValue("harazafraz"));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAnIniFileWithEqualsCharsInTheName_WhenReading_ThenItIsSCorrectlyInterpretedAsAString()
        {
            var delftIniReader = new DelftIniReader();
            var testFilePath = TestHelper.GetTestFilePath(@"IniReaderTest\NetworkDefinition.ini");
            var categories = delftIniReader.ReadDelftIniFile(testFilePath);
            Assert.That(categories.Count, Is.EqualTo(4));

            Assert.That(categories[1].Name, Is.EqualTo("Node=1"));
            Assert.That(categories[1].Properties[0].Value, Is.EqualTo("Node001=6"));

            Assert.That(categories[2].Name, Is.EqualTo("Node"));
            Assert.That(categories[2].Properties[0].Value, Is.EqualTo("T1_B1_Manhole_x=1000m"));
        }

        [Test]
        public void GivenAnIniFileWhichDoesNotMatchTheRegexPattern_WhenGettingKeyValueComment_ThenAFormatExceptionIsThrown()
        {
            var delftIniReader = new DelftIniReader();
            var testFilePath = TestHelper.GetTestFilePath(@"IniReaderTest\NetworkDefinitionNotMatchingRegex.ini");
            Assert.Throws<FormatException>(() => delftIniReader.ReadDelftIniFile(testFilePath), string.Format(
                Resources.DelftIniReader_GetKeyValueComment_Invalid_key_value_comment_line_on_line__0__in_file__1_,
                1, testFilePath));
        }
    }
}