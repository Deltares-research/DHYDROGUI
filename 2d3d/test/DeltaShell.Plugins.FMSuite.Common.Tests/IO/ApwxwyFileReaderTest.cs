using System.IO;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.Common.IO.Readers;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO
{
    [Category(TestCategory.DataAccess)]
    [TestFixture]
    public class ApwxwyFileReaderTest
    {
        [Test]
        public void GivenNonExistingApwxwyFile_WhenReading_LogMessageShouldBeGiven()
        {
            const string filePath = "NonExistingFilePath.apwxwy";
            TestHelper.AssertAtLeastOneLogMessagesContains(() => ReadGridFileFromApwxwyFile(filePath),
                                                           $"File at '{filePath}' was not found.");
        }

        [TestCase(@"windFiles\myWindFile.apwxwy", "wind1.grd")]
        [TestCase(@"windFiles\myWindFile2.apwxwy", "wind2.grd")]
        [TestCase(@"windFiles\myWindFile3.apwxwy", null)]
        [TestCase("NonExistingFilePath.apwxwy", null)]
        [TestCase(@"windFiles\myWindFileWithComments.apwxwy", "wind3.grd")]
        [TestCase(@"windFiles\myWindFileExceptionBeforeGridFileNameGiven.apwxwy", "wind4.grd")]
        [TestCase(@"windFiles\myWindFileExceptionAfterGridFileNameGiven.apwxwy", "wind4.grd")]
        public void GivenApwxwyFile_WhenReading_GridFileNameIsReturned(string relativeTestFilePath, string expectedGridFileName)
        {
            string filePath = TestHelper.GetTestFilePath(relativeTestFilePath);
            string testFilePath = TestHelper.CreateLocalCopy(filePath);
            Assert.That(ReadGridFileFromApwxwyFile(testFilePath), Is.EqualTo(expectedGridFileName));
        }

        private static string ReadGridFileFromApwxwyFile(string filePath)
        {
            try
            {
                var apwxwyReader = new ApwxwyFileReader(filePath);
                return apwxwyReader.ReadGridFileNameWithExtension();
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(filePath));
            }
        }
    }
}