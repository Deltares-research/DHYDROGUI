using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.DelftIniObjects;
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
            string mdwFilePath = TestHelper.GetTestFilePath(@"wave_timespacevarbnd\tst.mdw");

            IList<DelftIniCategory> categories;
            using (var fileStream = new FileStream(mdwFilePath, FileMode.Open, FileAccess.Read))
            {
                categories = delftIniReader.ReadDelftIniFile(fileStream, mdwFilePath);
            }

            Assert.AreEqual(2, categories.Count(k => k.Name == "Domain"));
            Assert.AreEqual(2, categories.Count(k => k.Name == "Boundary"));
            Assert.AreEqual(3, categories.Count(k => k.Name == "TimePoint"));
            Assert.AreEqual(13, categories.Count);

            DelftIniCategory innerDomain = categories.Where(c => c.Name == "Domain").ToList()[1];
            Assert.AreEqual(85, innerDomain.LineNumber);

            DelftIniProperty bedLevelProperty = innerDomain.Properties.First(p => p.Name == "BedLevel");
            Assert.AreEqual(88, bedLevelProperty.LineNumber);
            Assert.AreEqual("inner.dep", bedLevelProperty.Value);

            Assert.AreEqual("36", innerDomain.GetPropertyValue("NDir"));
            Assert.AreEqual(null, innerDomain.GetPropertyValue("harazafraz"));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAnIniFileWithEqualsCharsInTheName_WhenReading_ThenItIsSCorrectlyInterpretedAsAString()
        {
            string testFilePath = TestHelper.GetTestFilePath(@"IniReaderTest\NetworkDefinition.ini");

            IList<DelftIniCategory> categories;
            using (var fileStream = new FileStream(testFilePath, FileMode.Open, FileAccess.Read))
            {
                categories = new DelftIniReader().ReadDelftIniFile(fileStream, testFilePath);
            }

            Assert.That(categories.Count, Is.EqualTo(4));

            Assert.That(categories[1].Name, Is.EqualTo("Node=1"));
            Assert.That(categories[1].Properties.FirstOrDefault()?.Value, Is.EqualTo("Node001=6"));

            Assert.That(categories[2].Name, Is.EqualTo("Node"));
            Assert.That(categories[2].Properties.FirstOrDefault()?.Value, Is.EqualTo("T1_B1_Manhole_x=1000m"));
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
                    new DelftIniReader().ReadDelftIniFile(fileStream, testFilePath);
                }
            }

            // Assert
            Assert.Throws<FormatException>(Call, string.Format(
                                               Resources.DelftIniReader_GetKeyValueComment_Invalid_key_value_comment_line_on_line__0__in_file__1_,
                                               1, testFilePath));
        }
    }
}