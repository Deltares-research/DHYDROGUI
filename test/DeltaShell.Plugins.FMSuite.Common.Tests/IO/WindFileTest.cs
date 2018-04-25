using System;
using System.IO;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.Common.IO;
using NUnit.Framework;
using RTools_NTS.Util;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO
{
    [Category(TestCategory.DataAccess)]
    [TestFixture]
    public class WindFileTest
    {
        private static string filePath;
        private static string testFilePath;
        private static string apwxwyDir;
        
        [TestCase(@"windFiles\myWindFile.apwxwy", "wind1.grd")]
        [TestCase(@"windFiles\myWindFile2.apwxwy", "wind2.grd")]
       
        
        public void GivenApwxwyFile_WhenReading_GridFileNameIsReturned(string relativeTestFilePath, string expectedGridFileName)
        {
            SetUpTest(relativeTestFilePath);

            try
            {
                var gridFilePath = WindFile.GetCorrespondingGridFilePath(testFilePath);
                Assert.That(gridFilePath, 
                    Is.EqualTo(Path.Combine(apwxwyDir,expectedGridFileName)));
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(testFilePath));
            }
        }

        [TestCase(@"windFiles\myWindFile3.apwxwy")]
        [TestCase("NonExistingFilePath.apwxwy")]
        [TestCase(null)]
       
        public void GivenApwxwyFile_WhenReadingAndGridFileNameIsMissingInFile_NullShouldBeReturned(string relativeTestFilePath)
        {
            SetUpTest(relativeTestFilePath);

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
        [Test]
        public void GivenApwxwyFile_WhenGettingExceptionDuringGetCorrespondingGridFilePath_NullShouldBeReturned()
        {
            filePath = testFilePath = string.Empty;
            
            var gridFilePath = WindFile.GetCorrespondingGridFilePath(testFilePath);
            Assert.IsNull(gridFilePath);
        }

        private static void SetUpTest(string relativeTestFilePath)
        {
            if (relativeTestFilePath != null)
            {
                filePath = TestHelper.GetTestFilePath(relativeTestFilePath);
                testFilePath = TestHelper.CreateLocalCopy(filePath);
                apwxwyDir = System.IO.Path.GetDirectoryName(testFilePath);
                Assert.IsNotNull(apwxwyDir);
            }
            else 
            {
                testFilePath = null;
            }
        }
    }
}