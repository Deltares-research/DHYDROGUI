using System.Collections.Generic;
using System.IO;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.WaveOutputData;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess.Helpers.WaveOutputData
{
    [TestFixture]
    public class WaveOutputFileHelperTest
    {
        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void CollectInputFileNamesFromWorkingDirectoryMdw_MdwPathNullOrEmpty_ThrowsArgumentException(string mdwPath)
        {
            void Call() => WaveOutputFileHelper.CollectInputFileNamesFromWorkingDirectoryMdw(mdwPath);
            Assert.That(Call, Throws.ArgumentException);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void CollectInputFileNamesFromWorkingDirectoryMdw_ReturnsInputFiles()
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                const string mdwFileName = "Waves.mdw";
                string alternativeOutputSourcePath = TestHelper.GetTestFilePath(@"WaveModelTest\alternative_output");
                string alternativeOutputPath = tempDir.CopyDirectoryToTempDirectory(alternativeOutputSourcePath);

                string mdwPath = Path.Combine(alternativeOutputPath, mdwFileName);


                // Cal
                HashSet<string> result = WaveOutputFileHelper.CollectInputFileNamesFromWorkingDirectoryMdw(mdwPath);

                // Assert
                string[] expectedFileNames =
                {
                    "Waves.pol",
                    "Waves.obt",
                    "Waves.loc",
                    "coastw.grd",
                    "coastw20.dep",
                    "Waves.mdw",
                };

                foreach (string expectedFileName in expectedFileNames)
                {
                    string fullPath = Path.Combine(alternativeOutputPath, expectedFileName);
                    Assert.That(result, Has.Member(fullPath));
                }

                Assert.That(result.Count, Is.EqualTo(expectedFileNames.Length));
            }
        }
    }
}