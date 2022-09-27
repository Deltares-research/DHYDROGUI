using System;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileWriters.Roughness;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders
{
    [TestFixture]
    public class DelftIniMultiLineReaderTest
    {
        [Test]
        public void GivenIniFileWithMultiLineValuesProperty_WhenReading_ThenReturnsAllValues()
        {
            var fileName = "DelftIniMultiLineReaderTestData.ini";
            var originalFile = TestHelper.GetTestFilePath(fileName);
            var localCopy = TestHelper.CreateLocalCopy(originalFile);
            try
            {
                var categories = new DelftIniMultiLineReader().ReadDelftIniFile(localCopy);
                Assert.That(categories.Count, Is.EqualTo(1));

                var properties = categories[0].Properties;
                Assert.That(properties.Count, Is.EqualTo(8));

                var multiLineValueProperty = properties.FirstOrDefault(p => p.Name.Equals(RoughnessDataRegion.Values.Key));
                Assert.That(multiLineValueProperty, Is.Not.Null);

                var values = multiLineValueProperty.Value.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

                Assert.That(values.Length, Is.EqualTo(5));
                Assert.That(values[0].Trim(), Is.EqualTo("7.00000 7.50000"));
                Assert.That(values[1].Trim(), Is.EqualTo("8.00000 8.50000"));
                Assert.That(values[2].Trim(), Is.EqualTo("9.00000 9.50000"));
                Assert.That(values[3].Trim(), Is.EqualTo("10.00000 10.50000"));
                Assert.That(values[4].Trim(), Is.EqualTo("11.00000 11.50000"));
            }
            finally
            {
                FileUtils.DeleteIfExists(localCopy);
            }
            
        }

        [Test]
        public void GivenIniFileWithMultiLineValuesAndCommentsProperty_WhenReading_ThenReturnsAllComments()
        {
            var fileName = "DelftIniMultiLineReaderTestData.ini";
            var originalFile = TestHelper.GetTestFilePath(fileName);
            var localCopy = TestHelper.CreateLocalCopy(originalFile);
            try
            {
                var categories = new DelftIniMultiLineReader().ReadDelftIniFile(localCopy);
                Assert.That(categories.Count, Is.EqualTo(1));

                var properties = categories[0].Properties;
                Assert.That(properties.Count, Is.EqualTo(8));

                var multiLineValueProperty = properties.FirstOrDefault(p => p.Name.Equals(RoughnessDataRegion.Values.Key));
                Assert.That(multiLineValueProperty, Is.Not.Null);

                var comments = multiLineValueProperty.Comment.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

                Assert.That(comments.Length, Is.EqualTo(5));
                Assert.That(comments[0].Trim(), Is.EqualTo("comment 1"));
                Assert.That(comments[1].Trim(), Is.EqualTo("comment 2"));
                Assert.That(comments[2].Trim(), Is.EqualTo(""));
                Assert.That(comments[3].Trim(), Is.EqualTo("comment 4"));
                Assert.That(comments[4].Trim(), Is.EqualTo(""));
            }
            finally
            {
                FileUtils.DeleteIfExists(localCopy);
            }

        }
    }
}