using System.IO;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.Common.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO
{
    [Category(TestCategory.DataAccess)]
    [TestFixture]
    public class WindFileTest
    {
        [TestCase(@"windFiles\myWindFile.apwxwy", "wind1.grd")]
        [TestCase(@"windFiles\myWindFile2.apwxwy", "wind2.grd")]
        public void GivenApwxwyFile_WhenReading_GridFileNameIsReturned(string relativeTestFilePath, string expectedGridFileName)
        {
            var testFilePath = GetFullTestFilePath(relativeTestFilePath);
            var apwxwyDir = Path.GetDirectoryName(testFilePath);
            Assert.IsNotNull(apwxwyDir);

            try
            {
                var gridFilePath = WindFile.GetCorrespondingGridFilePath(testFilePath);
                Assert.That(gridFilePath, 
                    Is.EqualTo(Path.Combine(apwxwyDir, expectedGridFileName)));
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(testFilePath));
            }
        }

        [TestCase(@"windFiles\myWindFile3.apwxwy")]
        [TestCase("NonExistingFilePath.apwxwy")]
        public void GivenApwxwyFilePath_WhenReadingAndNoGridFileIsFound_NullShouldBeReturned(string relativeTestFilePath)
        {
            var testFilePath = GetFullTestFilePath(relativeTestFilePath);

            try
            {
                var gridFilePath = WindFile.GetCorrespondingGridFilePath(testFilePath);
                Assert.IsNull(gridFilePath);
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(testFilePath));
            }
        }

        [TestCase(null)]
        [TestCase("")]
        public void GivenEmptyFilePath_WhenGettingCorrespondingGridFilePath_NullShouldBeReturned(string nonsenseValueForFilePath)
        {
            var gridFilePath = WindFile.GetCorrespondingGridFilePath(nonsenseValueForFilePath);
            Assert.IsNull(gridFilePath);
        }

        private static string GetFullTestFilePath(string relativeTestFilePath)
        {
            if (relativeTestFilePath == null) return null;
            var fullFilePath = TestHelper.GetTestFilePath(relativeTestFilePath);
            return TestHelper.CreateLocalCopy(fullFilePath);
        }
    }
}