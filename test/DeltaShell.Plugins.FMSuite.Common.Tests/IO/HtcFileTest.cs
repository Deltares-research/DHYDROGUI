using System;
using System.IO;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO
{
    [Category(TestCategory.DataAccess)]
    [TestFixture]
    public class HtcFileTest
    {
        [Test]
        public void GivenNullForHtcFile_WhenReading_ExceptionShouldBeThrown()
        {
            Assert.Throws<InvalidOperationException>(() => { HtcFile.GetCorrespondingGridFilePath(null); },
                                                     "Heat flux file path is not valid");
        }

        [Test]
        public void GivenHtcFile_WhenReading_GridFileNameIsReturned()
        {
            
            var testFilePath = TestHelper.GetTestFilePath(@"heatFluxFiles\meteo.htc");
            testFilePath = TestHelper.CreateLocalCopy(testFilePath);
            var htcDir = Path.GetDirectoryName(testFilePath);
            Assert.IsNotNull(htcDir);

            try
            {
                string gridFilePath = HtcFile.GetCorrespondingGridFilePath(testFilePath);
                Assert.That(gridFilePath,
                    Is.EqualTo(Path.Combine(htcDir, "meteo.grd")));
            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(testFilePath));
            }
        }

        [Test]
        public void GivenHtcFilePath_WhenReadingAndNoGridFileNameIsFound_ExceptionShouldBeReturned()
        {
            var testFilePath = TestHelper.GetTestFilePath(@"heatFluxFiles\meteo2.htc");
            testFilePath = TestHelper.CreateLocalCopy(testFilePath);
            var htcDir = Path.GetDirectoryName(testFilePath);
            Assert.IsNotNull(htcDir);

            try
            {
                Assert.Throws<InvalidOperationException>(() => { HtcFile.GetCorrespondingGridFilePath(testFilePath); },
                    "Relative Grid file path is missing in the *.htc file");

            }
            finally
            {
                FileUtils.DeleteIfExists(Path.GetDirectoryName(testFilePath));
            }
        }
    }
}