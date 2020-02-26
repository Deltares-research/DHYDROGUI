using System;
using System.IO;
using DeltaShell.Dimr;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Api;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Api
{
    [TestFixture]
    public class WaveEnvironmentHelperTest
    {
        private string previousPath;
        private string previousArch;
        private string previousWorkingDirectory;

        [SetUp]
        public void SetUp()
        {
            previousPath = Environment.GetEnvironmentVariable("PATH");
            previousArch = Environment.GetEnvironmentVariable("ARCH");
            previousWorkingDirectory = Directory.GetCurrentDirectory();
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("PATH", previousPath);
            Environment.SetEnvironmentVariable("ARCH", previousArch);
            Directory.SetCurrentDirectory(previousWorkingDirectory);
            WaveEnvironmentHelper.DimrRun = false;
        }

        [Test]
        public void WaveEnvironmentHelper_AdjustsAndRestoresEnvironmentCorrectly()
        {
            // Setup
            const string expectedPath = "Path1;Path2;Path3";
            Environment.SetEnvironmentVariable("PATH", expectedPath);

            const string expectedArch = "x86";
            Environment.SetEnvironmentVariable("ARCH", expectedArch);

            string expectedDirectoryPath = Directory.GetCurrentDirectory();

            // Call | Update
            using (var tempDir = new TemporaryDirectory())
            {
                using (var _ = new WaveEnvironmentHelper(tempDir.Path))
                {
                    // Assert | Update
                    Assert.That(Directory.GetCurrentDirectory(),
                                Is.EqualTo(tempDir.Path));
                    Assert.That(Environment.GetEnvironmentVariable("ARCH", EnvironmentVariableTarget.Process),
                                Is.EqualTo("x64"));
                    string expectedModifiedPath = string.Join(";",
                                                              DimrApiDataSet.WaveExePath,
                                                              DimrApiDataSet.SwanExePath,
                                                              DimrApiDataSet.SwanScriptPath,
                                                              DimrApiDataSet.EsmfExePath,
                                                              DimrApiDataSet.EsmfScriptPath,
                                                              expectedPath);
                    Assert.That(Environment.GetEnvironmentVariable("PATH"),
                                Is.EqualTo(expectedModifiedPath));
                } // Call | Restore


                // Assert | Restore
                Assert.That(Directory.GetCurrentDirectory(),
                            Is.EqualTo(expectedDirectoryPath));
                Assert.That(Environment.GetEnvironmentVariable("ARCH", EnvironmentVariableTarget.Process),
                            Is.EqualTo(expectedArch));
                Assert.That(Environment.GetEnvironmentVariable("PATH"),
                            Is.EqualTo(expectedPath));
            }
        }

        [Test]
        public void WaveEnvironmentHelper_DimrRunTrue_StoresArchInOldArch()
        {
            const string expectedArch = "x86";
            Environment.SetEnvironmentVariable("ARCH", expectedArch);
            
            using (var _ = new WaveEnvironmentHelper(null))
            {
                WaveEnvironmentHelper.DimrRun = true;

                // Assert | Update
                Assert.That(Environment.GetEnvironmentVariable("ARCH", EnvironmentVariableTarget.Process),
                            Is.EqualTo("x64"));
                Assert.That(Environment.GetEnvironmentVariable("OLD_ARCH", EnvironmentVariableTarget.Process),
                            Is.Null);

            } // Call | Restore
            
            // Assert | Restore
            Assert.That(Environment.GetEnvironmentVariable("OLD_ARCH", EnvironmentVariableTarget.Process),
                        Is.EqualTo(expectedArch));
        }
    }
}