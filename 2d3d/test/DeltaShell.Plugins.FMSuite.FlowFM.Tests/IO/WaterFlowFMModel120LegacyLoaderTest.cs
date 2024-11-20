using System;
using System.IO;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using NUnit.Framework;
using Does = NUnit.Framework.Does;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class WaterFlowFMModel120LegacyLoaderTest
    {
        [Test]
        public void OnAfterProjectMigrated_ProjectNull_ThrowsArgumentNullException()
        {
            // Setup
            var legacyLoader = new WaterFlowFMModel120LegacyLoader();

            // Call
            void Call() => legacyLoader.OnAfterProjectMigrated(null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("project"));
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void OnAfterProjectMigrated_ReorganizesStateFiles()
        {
            // Setup
            string testData = TestHelper.GetTestFilePath(@"WaterFlowFMModel120LegacyLoaderTest\standalone");

            var legacyLoader = new WaterFlowFMModel120LegacyLoader();
            var project = new Project();

            using (var temp = new TemporaryDirectory())
            using (var model = new WaterFlowFMModel())
            {
                string dir = temp.CopyDirectoryToTempDirectory(testData);
                string mduFilePath = Path.Combine(dir, @"Project1.dsproj_data\FlowFM\input\FlowFM.mdu");

                model.ImportFromMdu(mduFilePath);
                project.RootFolder.Add(model);

                // Call
                legacyLoader.OnAfterProjectMigrated(project);

                // Assert
                string projectDir = Path.Combine(dir, "Project1.dsproj_data");
                string explicitWorkingDir = Path.Combine(projectDir, "FlowFM_output");
                string modelDir = Path.Combine(projectDir, "FlowFM");
                string inputDir = Path.Combine(modelDir, "input");
                string outputDir = Path.Combine(modelDir, "output");

                Assert.That(projectDir, Does.Exist);
                Assert.That(explicitWorkingDir, Does.Not.Exist);
                Assert.That(modelDir, Does.Exist);
                Assert.That(inputDir, Does.Exist);
                Assert.That(outputDir, Does.Exist);
                Assert.That(Directory.EnumerateFiles(projectDir), Is.Empty);
                Assert.That(Path.Combine(inputDir, "metadata.xml"), Does.Not.Exist);
                Assert.That(Path.Combine(inputDir, "restart.meta"), Does.Not.Exist);
                Assert.That(Path.Combine(outputDir, "metadata.xml"), Does.Not.Exist);
            }
        }
    }
}