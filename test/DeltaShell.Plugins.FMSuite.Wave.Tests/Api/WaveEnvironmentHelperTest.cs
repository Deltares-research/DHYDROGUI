using System;
using System.IO;
using DeltaShell.Dimr;
using DeltaShell.NGHS.Common;
using DeltaShell.NGHS.IO.TestUtils;
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
            environment.GetVariable("PATH").Returns(expectedPath);

            const string expectedArch = "x86";
            environment.GetVariable("ARCH").Returns(expectedArch);

            string expectedDirectoryPath = Directory.GetCurrentDirectory();

            // Call | Update
            using (var tempDir = new TemporaryDirectory())
            {
                using (var _ = new WaveEnvironmentHelper(tempDir.Path, environment))
                {
                    // Assert | Update
                    Assert.That(Directory.GetCurrentDirectory(),
                                Is.EqualTo(tempDir.Path));
                    environment.Received(1).SetVariable("ARCH", "x64", EnvironmentVariableTarget.Process);

                    string expectedModifiedPath = string.Join(";",
                                                              DimrApiDataSet.WaveExePath,
                                                              DimrApiDataSet.SwanExePath,
                                                              DimrApiDataSet.SwanScriptPath,
                                                              DimrApiDataSet.EsmfExePath,
                                                              DimrApiDataSet.EsmfScriptPath,
                                                              expectedPath);
                    environment.Received(1).SetVariable("PATH", expectedModifiedPath, EnvironmentVariableTarget.Process);
                } // Call | Restore


                // Assert | Restore
                Assert.That(Directory.GetCurrentDirectory(),
                            Is.EqualTo(expectedDirectoryPath));
                environment.Received(1).SetVariable("ARCH", expectedArch, EnvironmentVariableTarget.Process);
                environment.Received(1).SetVariable("PATH", expectedPath, EnvironmentVariableTarget.Process);
            }
        }

        [Test]
        public void WaveEnvironmentHelper_DimrRunTrue_StoresArchInOldArch()
        {
            const string expectedPath = "Path1;Path2;Path3";
            var environment = Substitute.For<IEnvironment>();
            environment.GetVariable("PATH").Returns(expectedPath);

            const string expectedArch = "x86";
            environment.GetVariable("ARCH").Returns(expectedArch);

            using (var _ = new WaveEnvironmentHelper(null, environment))
            {
                WaveEnvironmentHelper.DimrRun = true;

                // Assert | Update
                environment.Received(1).SetVariable("ARCH", "x64", EnvironmentVariableTarget.Process);
            } // Call | Restore
            
            // Assert | Restore
            environment.Received(1).SetVariable("OLD_ARCH", expectedArch, EnvironmentVariableTarget.Process);
        }
    }
}