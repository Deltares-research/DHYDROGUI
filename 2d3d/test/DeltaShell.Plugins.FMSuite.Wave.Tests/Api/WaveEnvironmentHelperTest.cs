using System.IO;
using DelftTools.TestUtils;
using DeltaShell.Dimr;
using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.FMSuite.Wave.Api;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Api
{
    [TestFixture]
    public class WaveEnvironmentHelperTest
    {
        private string previousWorkingDirectory;

        [SetUp]
        public void SetUp()
        {
            previousWorkingDirectory = Directory.GetCurrentDirectory();
        }

        [TearDown]
        public void TearDown()
        {
            Directory.SetCurrentDirectory(previousWorkingDirectory);
            WaveEnvironmentHelper.DimrRun = false;
        }

        [Test]
        public void WaveEnvironmentHelper_AdjustsAndRestoresEnvironmentCorrectly()
        {
            // Setup
            const string expectedPath = "Path1;Path2;Path3";
            var environment = Substitute.For<IEnvironment>();
            environment.GetVariable(EnvironmentConstants.PathKey)
                       .Returns(expectedPath);

            const string expectedArch = "x86";
            environment.GetVariable(WaveEnvironmentConstants.ArchKey)
                       .Returns(expectedArch);

            string expectedDirectoryPath = Directory.GetCurrentDirectory();

            // Call | Update
            using (var tempDir = new TemporaryDirectory())
            {
                using (new WaveEnvironmentHelper(tempDir.Path, environment))
                {
                    // Assert | Update
                    Assert.That(Directory.GetCurrentDirectory(),
                                Is.EqualTo(tempDir.Path));
                    environment.Received(1).SetVariable(WaveEnvironmentConstants.ArchKey,
                                                        WaveEnvironmentConstants.ArchValue);

                    string expectedModifiedPath = string.Join(";",
                                                              DimrApiDataSet.WaveExeDirectory,
                                                              DimrApiDataSet.SwanExeDirectory,
                                                              DimrApiDataSet.EsmfExeDirectory,
                                                              expectedPath);
                    environment.Received(1).SetVariable(EnvironmentConstants.PathKey, expectedModifiedPath);
                } // Call | Restore

                // Assert | Restore
                Assert.That(Directory.GetCurrentDirectory(),
                            Is.EqualTo(expectedDirectoryPath));
                environment.Received(1).SetVariable(WaveEnvironmentConstants.ArchKey, expectedArch);
                environment.Received(1).SetVariable(EnvironmentConstants.PathKey, expectedPath);
            }
        }

        [Test]
        public void WaveEnvironmentHelper_DimrRunTrue_StoresArchInOldArch()
        {
            const string expectedPath = "Path1;Path2;Path3";
            var environment = Substitute.For<IEnvironment>();
            environment.GetVariable(EnvironmentConstants.PathKey)
                       .Returns(expectedPath);

            const string expectedArch = "x86";
            environment.GetVariable(WaveEnvironmentConstants.ArchKey)
                       .Returns(expectedArch);

            using (new WaveEnvironmentHelper(null, environment))
            {
                WaveEnvironmentHelper.DimrRun = true;

                // Assert | Update
                environment.Received(1).SetVariable(WaveEnvironmentConstants.ArchKey, WaveEnvironmentConstants.ArchValue);
            } // Call | Restore

            // Assert | Restore
            environment.Received(1).SetVariable(WaveEnvironmentConstants.OldArchKey, expectedArch);
        }
    }
}