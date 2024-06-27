using System;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using Deltares.Infrastructure.API.Logging;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.GWSW.Tests.IO.Importers
{
    [TestFixture]
    public class DefinitionsVersionProviderTest
    {
        /*
         *  Currently, we determine the Gwsw version by reading the header of knooppunt.csv.
         *  If the header contains a column 'AAN_PRO', we deal with the new Gwsw format.
         */

        [Test]
        public void GivenInvalidFolderToImport_WhenDeterminingGwswVersion_ThenOldDefinitionIsReturnedWithWarning()
        {
            // Setup
            var testDir = FileUtils.CreateTempDirectory();

            try
            {
                string gwswVersion = null;
                ILogHandler logHandler = Substitute.For<ILogHandler>();
                var definitionsVersionProvider = new DefinitionsVersionProvider(logHandler);
                // Call
                gwswVersion = definitionsVersionProvider.GetDefinitionVersionName(testDir);

                // Assert
                const string msg =
                    "Can't determine the Gwsw file format. Please select a folder with a valid Verbinding.csv file.";
                logHandler.Received().ReportWarningFormat(msg);

                Assert.That(gwswVersion, Is.EqualTo("GWSWDefinition"));
            }
            finally
            {
                FileUtils.DeleteIfExists(testDir);
            }
        }

        [Test]
        public void GivenValidGwswFolderToImportWithNewVersion_WhenDeterminingGwswVersion_ThenOldDefinitionIsReturned()
        {
            var originalDir = TestHelper.GetTestFilePath(@"gwswFiles\GWSW_Juinen_New");
            var testDir = FileUtils.CreateTempDirectory();
            FileUtils.CopyDirectory(originalDir, testDir);
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            var definitionsVersionProvider = new DefinitionsVersionProvider(logHandler);

            try
            {
                var gwswVersion = definitionsVersionProvider.GetDefinitionVersionName(testDir);
                Assert.That(gwswVersion, Is.EqualTo("GWSWDefinition1_5"));
            }
            finally
            {
                FileUtils.DeleteIfExists(testDir);
            }
        }

        [Test]
        public void GivenValidGwswFolderToImportWithOldVersion_WhenDeterminingGwswVersion_ThenOldDefinitionIsReturned()
        {
            var originalDir = TestHelper.GetTestFilePath(@"gwswFiles\2Connection3Manholes");
            var testDir = FileUtils.CreateTempDirectory();
            FileUtils.CopyDirectory(originalDir, testDir);
            ILogHandler logHandler = Substitute.For<ILogHandler>();
            var definitionsVersionProvider = new DefinitionsVersionProvider(logHandler);

            try
            {
                var gwswVersion = definitionsVersionProvider.GetDefinitionVersionName(testDir);
                Assert.That(gwswVersion, Is.EqualTo("GWSWDefinition"));
            }
            finally
            {
                FileUtils.DeleteIfExists(testDir);
            }
        }
    }
}